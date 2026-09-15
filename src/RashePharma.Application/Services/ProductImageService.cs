using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class ProductImageService : IProductImageService
{
    private readonly IProductImageRepository _imageRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductImageService(
        IProductImageRepository imageRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductImageDto>> GetByProductIdAsync(int productId)
    {
        var images = await _imageRepository
            .GetByProductIdAsync(productId);

        return images.Select(MapToDto).ToList();
    }

    public async Task<ProductImageDto?> GetByIdAsync(int id)
    {
        var image = await _imageRepository.GetByIdAsync(id);

        if (image == null)
            return null;

        return MapToDto(image);
    }

    public async Task<ProductImageDto> CreateAsync(
        int productId,
        ProductImageCreateDto dto)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
            throw new KeyNotFoundException(
                $"Product with ID {productId} was not found.");

        // If this image is primary,
        // make all existing images non-primary.
        if (dto.IsPrimary)
        {
            var existingImages =
                await _imageRepository.GetByProductIdAsync(productId);

            foreach (var image in existingImages)
            {
                image.IsPrimary = false;
            }
        }

        var imageEntity = new ProductImage
        {
            ProductId = productId,
            ImageUrl = dto.ImageUrl,
            AltText = dto.AltText,
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder
        };

        await _imageRepository.AddAsync(imageEntity);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(imageEntity);
    }

    public async Task<ProductImageDto?> UpdateAsync(
        int id,
        ProductImageUpdateDto dto)
    {
        var image = await _imageRepository.GetByIdAsync(id);

        if (image == null)
            return null;

        // If this image is being made primary,
        // remove primary status from other images.
        if (dto.IsPrimary)
        {
            var existingImages =
                await _imageRepository
                    .GetByProductIdAsync(image.ProductId);

            foreach (var existingImage in existingImages)
            {
                if (existingImage.Id != id)
                {
                    existingImage.IsPrimary = false;
                }
            }
        }

        image.ImageUrl = dto.ImageUrl;
        image.AltText = dto.AltText;
        image.IsPrimary = dto.IsPrimary;
        image.DisplayOrder = dto.DisplayOrder;

        await _imageRepository.UpdateAsync(image);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(image);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _imageRepository.GetByIdAsync(id);

        if (image == null)
            return false;

        await _imageRepository.DeleteAsync(image);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static ProductImageDto MapToDto(ProductImage image)
    {
        return new ProductImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder
        };
    }
}