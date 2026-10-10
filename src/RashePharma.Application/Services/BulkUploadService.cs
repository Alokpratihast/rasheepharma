using Microsoft.Extensions.Logging;

using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;

namespace RashePharma.Application.Services;

public class BulkUploadService : IBulkUploadService
{
    private const string ExcelFileType = "Excel";
    private const string ImageFileType = "Image";
    private const string UsdCurrency = "USD";
    private const long MaxExcelFileSize = 50L * 1024 * 1024;
    private const long MaxImageFileSize = 1024L * 1024 * 1024;
    private const long MaxBatchSize = 10L * 1024 * 1024 * 1024;
    private const int MaxImageCount = 500;

    private readonly IBulkUploadRepository _bulkUploadRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IProductImageRepository _imageRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBulkUploadStorageService _bulkUploadStorageService;
    private readonly IImageStorageService _imageStorageService;
    private readonly IBulkUploadExcelParser _excelParser;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly ILogger<BulkUploadService> _logger;

    public BulkUploadService(
        IBulkUploadRepository bulkUploadRepository,
        IProductRepository productRepository,
        IProductVariantRepository variantRepository,
        IProductImageRepository imageRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IBulkUploadStorageService bulkUploadStorageService,
        IImageStorageService imageStorageService,
        IBulkUploadExcelParser excelParser,
        IExchangeRateService exchangeRateService,
        ILogger<BulkUploadService> logger)
    {
        _bulkUploadRepository = bulkUploadRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _imageRepository = imageRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _bulkUploadStorageService = bulkUploadStorageService;
        _imageStorageService = imageStorageService;
        _excelParser = excelParser;
        _exchangeRateService = exchangeRateService;
        _logger = logger;
    }

    public async Task<int> CreateJobAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var job = new BulkUploadJob
        {
            FileName = fileName,
            Status = BulkUploadStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _bulkUploadRepository.CreateJobAsync(
            job,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync();

        return job.Id;
    }

    public async Task<int> CreateJobAsync(
        Stream excelStream,
        string excelFileName,
        IReadOnlyCollection<BulkUploadFileInput> files,
        CancellationToken cancellationToken = default)
    {
        var job = new BulkUploadJob
        {
            FileName = excelFileName,
            Status = BulkUploadStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _bulkUploadRepository.CreateJobAsync(
            job,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync();

        var excelFileSize = excelStream.Length;

        var excelUrl = await _bulkUploadStorageService.UploadAsync(
            excelStream,
            excelFileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var excelFile = new BulkUploadFile
        {
            BulkUploadJobId = job.Id,
            OriginalFileName = excelFileName,
            BlobName = Path.GetFileName(
                new Uri(excelUrl).AbsolutePath),
            FileUrl = excelUrl,
            FileType = ExcelFileType,
            ContentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileSize = excelFileSize
        };

        await _bulkUploadRepository.AddFileAsync(
            excelFile,
            cancellationToken);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileUrl = await _bulkUploadStorageService.UploadAsync(
                file.Stream,
                file.FileName,
                file.ContentType);

            var bulkFile = new BulkUploadFile
            {
                BulkUploadJobId = job.Id,
                OriginalFileName = file.FileName,
                BlobName = Path.GetFileName(
                    new Uri(fileUrl).AbsolutePath),
                FileUrl = fileUrl,
                FileType = ImageFileType,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            };

            await _bulkUploadRepository.AddFileAsync(
                bulkFile,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync();

        return job.Id;
    }


    /// <summary>
    /// Validates an admin's upload manifest and creates one narrowly scoped
    /// SAS URL per file. Large file bytes never pass through this API process.
    /// </summary>
    public async Task<IReadOnlyList<BulkUploadUploadTarget>> CreateUploadTargetsAsync(
        IReadOnlyCollection<BulkUploadFileDescriptor> files,
        CancellationToken cancellationToken = default)
    {
        var normalized = ValidateManifest(files);
        await _bulkUploadStorageService.EnsureContainerExistsAsync(cancellationToken);
        var targets = new List<BulkUploadUploadTarget>(normalized.Count);

        foreach (var file in normalized)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = await _bulkUploadStorageService.CreateUploadTargetAsync(
                file.FileName,
                cancellationToken);

            targets.Add(new BulkUploadUploadTarget(
                file.FileName,
                file.FileType,
                file.FileSize,
                target.BlobName,
                target.UploadUrl));
        }

        return targets;
    }

    /// <summary>
    /// Verifies every staged blob before persisting a Pending job. The worker
    /// only claims Pending jobs, so incomplete uploads cannot be processed.
    /// </summary>
    public async Task<int> CreateJobFromStagedFilesAsync(
        IReadOnlyCollection<BulkUploadStagedFileInput> files,
        CancellationToken cancellationToken = default)
    {
        var normalized = ValidateManifest(files);

        foreach (var file in normalized)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var exists = await _bulkUploadStorageService.VerifyStagedFileAsync(
                file.BlobName,
                file.FileSize,
                cancellationToken);

            if (!exists)
            {
                throw new InvalidOperationException(
                    $"Uploaded file '{file.FileName}' is missing or its size does not match the upload manifest.");
            }
        }

        var excelFile = normalized.Single(file => file.FileType == ExcelFileType);
        var job = new BulkUploadJob
        {
            FileName = excelFile.FileName,
            Status = BulkUploadStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var file in normalized)
        {
            job.Files.Add(new BulkUploadFile
            {
                OriginalFileName = file.FileName,
                BlobName = file.BlobName,
                FileUrl = _bulkUploadStorageService.GetStagedFileUrl(file.BlobName),
                FileType = file.FileType,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            });
        }

        // Persist job and its complete file list in one SaveChanges call.
        // Otherwise the worker could claim Pending between two database writes.
        await _bulkUploadRepository.CreateJobAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync();
        return job.Id;
    }

    private static List<BulkUploadStagedFileInput> ValidateManifest<T>(
        IReadOnlyCollection<T> files)
    {
        if (files == null || files.Count == 0 || files.Count > MaxImageCount + 1)
        {
            throw new InvalidOperationException(
                $"Upload one .xlsx workbook and no more than {MaxImageCount} images.");
        }

        var normalized = files.Select(file => file switch
        {
            BulkUploadFileDescriptor descriptor => new BulkUploadStagedFileInput(
                descriptor.FileName,
                descriptor.ContentType,
                descriptor.FileSize,
                descriptor.FileType,
                string.Empty),
            BulkUploadStagedFileInput staged => staged,

            _ => throw new InvalidOperationException("Unsupported upload manifest entry.")
        }).ToList();

        if (normalized.Count(file => file.FileType == ExcelFileType) != 1 ||
            normalized.Any(file => file.FileType is not (ExcelFileType or ImageFileType)))
        {
            throw new InvalidOperationException(
                "The upload must contain exactly one Excel file; remaining files must be images.");
        }

        var excel = normalized.Single(file => file.FileType == ExcelFileType);
        if (!string.Equals(Path.GetExtension(excel.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase) ||
            excel.FileSize <= 0 || excel.FileSize > MaxExcelFileSize)
        {
            throw new InvalidOperationException(
                "The workbook must be a non-empty .xlsx file no larger than 50 MiB because the current workbook parser loads it into memory.");
        }

        var images = normalized.Where(file => file.FileType == ImageFileType).ToList();
        if (images.Count > MaxImageCount)
        {
            throw new InvalidOperationException($"A maximum of {MaxImageCount} images is allowed per job.");
        }

        var duplicateNames = images
            .GroupBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);
        if (duplicateNames)
        {
            throw new InvalidOperationException(
                "Image file names must be unique within a bulk upload because the workbook references images by file name.");
        }

        foreach (var image in images)
        {
            var expectedContentType = image.FileName is null
                ? null
                : Path.GetExtension(image.FileName).ToLowerInvariant() switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => null
                };

            if (expectedContentType == null || image.FileSize <= 0 || image.FileSize > MaxImageFileSize)
            {
                throw new InvalidOperationException(
                    $"Image '{image.FileName}' must be JPEG, PNG, WebP or GIF and no larger than 1 GiB.");
            }

            if (!string.IsNullOrWhiteSpace(image.ContentType) &&
                !string.Equals(image.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Image '{image.FileName}' has a content type that does not match its extension.");
            }
        }

        if (normalized.Sum(file => file.FileSize) > MaxBatchSize)
        {
            throw new InvalidOperationException("The maximum combined bulk-upload size is 10 GiB.");
        }

        if (normalized.Any(file => string.IsNullOrWhiteSpace(file.FileName) ||
                                   file.FileName != Path.GetFileName(file.FileName)))
        {
            throw new InvalidOperationException("File names must not contain directory paths.");
        }

        if (normalized.Where(file => !string.IsNullOrWhiteSpace(file.BlobName))
            .Select(file => file.BlobName)
            .Distinct(StringComparer.Ordinal)
            .Count() != normalized.Count(file => !string.IsNullOrWhiteSpace(file.BlobName)))
        {
            throw new InvalidOperationException("Each uploaded file must use a unique staging blob.");
        }

        return normalized;
    }
    public async Task ProcessAsync(
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _bulkUploadRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        if (job == null)
        {
            throw new KeyNotFoundException(
                $"Bulk upload job with ID {jobId} was not found.");
        }

        // Diagnostic logging
        _logger.LogInformation(
            "DEBUG BulkUpload Job {JobId}: Files loaded = {FileCount}",
            job.Id,
            job.Files.Count);

        foreach (var file in job.Files)
        {
            _logger.LogInformation(
                "DEBUG BulkUpload Job {JobId}: FileType={FileType}, OriginalFileName={FileName}, BlobName={BlobName}, FileUrl={FileUrl}",
                job.Id,
                file.FileType,
                file.OriginalFileName,
                file.BlobName,
                file.FileUrl);
        }

        if (job.Status == BulkUploadStatus.Completed ||
            job.Status == BulkUploadStatus.CompletedWithErrors)
        {
            return;
        }

        try
        {
            job.Status = BulkUploadStatus.Processing;
            job.StartedAt ??= DateTime.UtcNow;
            job.ErrorMessage = null;

            await _bulkUploadRepository.UpdateJobAsync(
                job,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync();

            var excelFile = job.Files
                .FirstOrDefault(file =>
                    file.FileType == ExcelFileType);

            if (excelFile == null)
            {
                throw new InvalidOperationException(
                    "Excel file was not found for this bulk upload job.");
            }

            await using var excelStream =
                await DownloadStagedFileAsync(
                    excelFile.FileUrl,
                    cancellationToken);

            var data = await _excelParser.ParseAsync(
                excelStream,
                cancellationToken);

            var validImageRows = data.Images
                .Where(image =>
                    !string.IsNullOrWhiteSpace(image.ImageFileName))
                .ToList();

            job.TotalRecords =
                data.Products.Count +
                data.Variants.Count +
                validImageRows.Count;

            await _bulkUploadRepository.UpdateJobAsync(
                job,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync();

            var inrToUsdRate =
                await _exchangeRateService.GetInrToUsdRateAsync();

            await ProcessProductsAsync(
                job,
                data.Products,
                cancellationToken);

            await ProcessVariantsAsync(
                job,
                data.Variants,
                inrToUsdRate,
                cancellationToken);

            await ProcessImagesAsync(
                job,
                data.Images,
                cancellationToken);

            job.Status = job.ErrorCount > 0
                ? BulkUploadStatus.CompletedWithErrors
                : BulkUploadStatus.Completed;

            job.CompletedAt = DateTime.UtcNow;

            await _bulkUploadRepository.UpdateJobAsync(
                job,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            job.Status = BulkUploadStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(
                ex,
                "Bulk upload job {JobId} failed during processing.",
                job.Id);

            await _bulkUploadRepository.UpdateJobAsync(
                job,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync();

            throw;
        }
    }

    public async Task<object?> GetStatusAsync(
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _bulkUploadRepository.GetByIdAsync(
            jobId,
            cancellationToken);

        if (job == null)
        {
            return null;
        }

        return new
        {
            job.Id,
            job.FileName,
            Status = job.Status.ToString(),
            job.TotalRecords,
            job.ProcessedRecords,
            job.SuccessCount,
            job.ErrorCount,
            job.ErrorMessage,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt
        };
    }

    private async Task ProcessProductsAsync(
        BulkUploadJob job,
        IReadOnlyList<BulkUploadProductRow> rows,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = rows[index];

            try
            {
                if (string.IsNullOrWhiteSpace(row.Slug))
                {
                    throw new InvalidOperationException(
                        "Product slug is required.");
                }

                if (string.IsNullOrWhiteSpace(row.ProductName))
                {
                    throw new InvalidOperationException(
                        "Product name is required.");
                }

                if (string.IsNullOrWhiteSpace(row.Category))
                {
                    throw new InvalidOperationException(
                        "Category is required.");
                }

                var categorySlug = row.Category.Trim();

                var category = await _categoryRepository.GetBySlugAsync(
                    categorySlug);

                if (category == null)
                {
                    category = new Category
                    {
                        Name = categorySlug,
                        Slug = categorySlug,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _categoryRepository.AddAsync(category);

                    await _unitOfWork.SaveChangesAsync();
                }

                var slug = row.Slug.Trim();

                var product =
                    await _productRepository.GetBySlugAsync(slug);

                if (product == null)
                {
                    product = new Product
                    {
                        Name = row.ProductName.Trim(),
                        Slug = slug,
                        GenericName = row.GenericName,
                        Composition = row.Composition,
                        DosageForm = row.DosageForm,
                        Description = row.Description,
                        BrandName = row.BrandName,
                        Manufacturer = row.Manufacturer,
                        CategoryId = category.Id,
                        IsActive = row.IsActive,
                        IsFeatured = row.IsFeatured,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _productRepository.AddAsync(product);
                }
                else
                {
                    product.Name = row.ProductName.Trim();
                    product.GenericName = row.GenericName;
                    product.Composition = row.Composition;
                    product.DosageForm = row.DosageForm;
                    product.Description = row.Description;
                    product.BrandName = row.BrandName;
                    product.Manufacturer = row.Manufacturer;
                    product.CategoryId = category.Id;
                    product.IsActive = row.IsActive;
                    product.IsFeatured = row.IsFeatured;
                    product.UpdatedAt = DateTime.UtcNow;

                    await _productRepository.UpdateAsync(product);
                }

                job.ProcessedRecords++;
                job.SuccessCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await AddErrorAsync(
                    job,
                    "Products",
                    index + 2,
                    row.Slug,
                    ex.Message,
                    cancellationToken);

                job.ProcessedRecords++;
                job.ErrorCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
        }
    }

    private async Task ProcessVariantsAsync(
        BulkUploadJob job,
        IReadOnlyList<BulkUploadVariantRow> rows,
        decimal inrToUsdRate,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = rows[index];

            try
            {
                if (string.IsNullOrWhiteSpace(row.ProductSlug))
                {
                    throw new InvalidOperationException(
                        "Product slug is required.");
                }

                if (row.PriceInr <= 0)
                {
                    throw new InvalidOperationException(
                        "Price INR must be greater than zero.");
                }

                if (row.MOQ < 0)
                {
                    throw new InvalidOperationException(
                        "MOQ cannot be negative.");
                }

                if (row.StockQuantity < 0)
                {
                    throw new InvalidOperationException(
                        "Stock quantity cannot be negative.");
                }

                var product =
                    await _productRepository.GetBySlugAsync(
                        row.ProductSlug.Trim());

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Product '{row.ProductSlug}' was not found.");
                }

                var usdPrice = Math.Round(
                    row.PriceInr * inrToUsdRate,
                    2,
                    MidpointRounding.AwayFromZero);

                var variants =
                    await _variantRepository.GetByProductIdAsync(
                        product.Id);

                ProductVariant? variant = null;

                if (!string.IsNullOrWhiteSpace(row.SKU))
                {
                    variant = variants.FirstOrDefault(v =>
                        !string.IsNullOrWhiteSpace(v.SKU) &&
                        string.Equals(
                            v.SKU.Trim(),
                            row.SKU.Trim(),
                            StringComparison.OrdinalIgnoreCase));
                }

                if (variant == null)
                {
                    variant = variants.FirstOrDefault(v =>
                        string.Equals(
                            v.Strength?.Trim(),
                            row.Strength?.Trim(),
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            v.PackSize?.Trim(),
                            row.PackSize?.Trim(),
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            v.UnitType?.Trim(),
                            row.UnitType?.Trim(),
                            StringComparison.OrdinalIgnoreCase));
                }

                if (variant == null)
                {
                    variant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Strength = row.Strength,
                        PackSize = row.PackSize,
                        Price = usdPrice,
                        Currency = UsdCurrency,
                        MOQ = row.MOQ,
                        UnitType = row.UnitType,
                        SKU = row.SKU,
                        StockQuantity = row.StockQuantity,
                        IsActive = row.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _variantRepository.AddAsync(variant);
                }
                else
                {
                    variant.Strength = row.Strength;
                    variant.PackSize = row.PackSize;
                    variant.Price = usdPrice;
                    variant.Currency = UsdCurrency;
                    variant.MOQ = row.MOQ;
                    variant.UnitType = row.UnitType;
                    variant.SKU = row.SKU;
                    variant.StockQuantity = row.StockQuantity;
                    variant.IsActive = row.IsActive;
                    variant.UpdatedAt = DateTime.UtcNow;

                    await _variantRepository.UpdateAsync(variant);
                }

                job.ProcessedRecords++;
                job.SuccessCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await AddErrorAsync(
                    job,
                    "Variants",
                    index + 2,
                    row.ProductSlug,
                    ex.Message,
                    cancellationToken);

                job.ProcessedRecords++;
                job.ErrorCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
        }
    }

    private async Task ProcessImagesAsync(
        BulkUploadJob job,
        IReadOnlyList<BulkUploadImageRow> rows,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = rows[index];

            if (string.IsNullOrWhiteSpace(row.ImageFileName))
            {
                continue;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(row.ProductSlug))
                {
                    throw new InvalidOperationException(
                        "Product slug is required.");
                }

                var product =
                    await _productRepository.GetBySlugAsync(
                        row.ProductSlug.Trim());

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Product '{row.ProductSlug}' was not found.");
                }

                var stagedFile = job.Files.FirstOrDefault(file =>
                    file.FileType == ImageFileType &&
                    string.Equals(
                        file.OriginalFileName.Trim(),
                        row.ImageFileName.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (stagedFile == null)
                {
                    throw new InvalidOperationException(
                        $"Image file '{row.ImageFileName}' was not uploaded.");
                }

                await using var stagedStream =
                    await DownloadStagedFileAsync(
                        stagedFile.FileUrl,
                        cancellationToken);

                var imageUrl = await _imageStorageService.UploadAsync(
                    stagedStream,
                    stagedFile.OriginalFileName,
                    stagedFile.ContentType);

                var existingImages =
                    await _imageRepository.GetByProductIdAsync(
                        product.Id);

                var hasPrimaryImage =
                    existingImages.Any(image => image.IsPrimary);

                var shouldBePrimary =
                    row.IsPrimary || !hasPrimaryImage;

                if (shouldBePrimary)
                {
                    foreach (var existingImage in existingImages)
                    {
                        existingImage.IsPrimary = false;
                    }
                }

                var image = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = imageUrl,
                    AltText = row.AltText,
                    IsPrimary = shouldBePrimary,
                    DisplayOrder = row.DisplayOrder
                };

                await _imageRepository.AddAsync(image);

                job.ProcessedRecords++;
                job.SuccessCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await AddErrorAsync(
                    job,
                    "Images",
                    index + 2,
                    row.ProductSlug,
                    ex.Message,
                    cancellationToken);

                job.ProcessedRecords++;
                job.ErrorCount++;

                await SaveProgressAsync(
                    job,
                    cancellationToken);
            }
        }
    }

    private async Task<Stream> DownloadStagedFileAsync(
        string fileUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result =
            await _bulkUploadStorageService.DownloadAsync(fileUrl);

        return result.Stream;
    }

    private async Task AddErrorAsync(
        BulkUploadJob job,
        string sheetName,
        int rowNumber,
        string? productSlug,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var error = new BulkUploadError
        {
            BulkUploadJobId = job.Id,
            SheetName = sheetName,
            RowNumber = rowNumber,
            ProductSlug = productSlug,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        };

        // SaveProgressAsync commits this error with the row counters.
        await _bulkUploadRepository.AddErrorAsync(
            error,
            cancellationToken);
    }

    private async Task SaveProgressAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken)
    {
        await _bulkUploadRepository.UpdateJobAsync(
            job,
            cancellationToken);

        // Product/image/error changes and progress counters are persisted together.
        await _unitOfWork.SaveChangesAsync();
    }
}