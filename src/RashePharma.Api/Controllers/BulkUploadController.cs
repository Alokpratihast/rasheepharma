using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/admin/bulk-upload")]
[Authorize(Roles = "Admin")]
public class BulkUploadController : ControllerBase
{
    private readonly IBulkUploadService _bulkUploadService;

    public BulkUploadController(IBulkUploadService bulkUploadService)
    {
        _bulkUploadService = bulkUploadService;
    }

    /// <summary>
    /// Requests one short-lived, single-blob SAS URL per file. Only small
    /// metadata crosses the API; file bytes are uploaded directly to Blob.
    /// </summary>
    [HttpPost("targets")]
    public async Task<IActionResult> CreateUploadTargets(
        [FromBody] CreateBulkUploadTargetsRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.ExcelFile == null)
        {
            return BadRequest(new { Message = "An Excel file is required." });
        }

        try
        {
            var manifest = new List<BulkUploadFileDescriptor>
            {
                new(
                    request.ExcelFile.FileName,
                    request.ExcelFile.ContentType,
                    request.ExcelFile.FileSize,
                    "Excel")
            };

            manifest.AddRange((request.Images ?? []).Select(image =>
                new BulkUploadFileDescriptor(
                    image.FileName,
                    image.ContentType,
                    image.FileSize,
                    "Image")));

            var targets = await _bulkUploadService.CreateUploadTargetsAsync(
                manifest,
                cancellationToken);

            return Ok(new
            {
                ExcelFile = targets[0],
                Images = targets.Skip(1)
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }

    /// <summary>
    /// Completes a job after the browser has committed all blob blocks. The
    /// service verifies each stored blob before it becomes visible to the worker.
    /// </summary>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteUpload(
        [FromBody] CompleteBulkUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.ExcelFile == null)
        {
            return BadRequest(new { Message = "An Excel file is required." });
        }

        try
        {
            var files = new List<BulkUploadStagedFileInput>
            {
                new(
                    request.ExcelFile.FileName,
                    request.ExcelFile.ContentType,
                    request.ExcelFile.FileSize,
                    "Excel",
                    request.ExcelFile.BlobName)
            };

            files.AddRange((request.Images ?? []).Select(image =>
                new BulkUploadStagedFileInput(
                    image.FileName,
                    image.ContentType,
                    image.FileSize,
                    "Image",
                    image.BlobName)));

            var jobId = await _bulkUploadService.CreateJobFromStagedFilesAsync(
                files,
                cancellationToken);

            return AcceptedAtAction(
                nameof(GetStatus),
                new { jobId },
                new
                {
                    JobId = jobId,
                    Status = "Pending",
                    Message = "Bulk upload completed and queued for processing.",
                    ImageCount = files.Count - 1
                });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }

    // Legacy multipart endpoint. Kept for existing clients; large batches should
    // use /targets and /complete so the API does not carry file bytes.
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateJob(
        IFormFile excelFile,
        List<IFormFile>? images,
        CancellationToken cancellationToken)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            return BadRequest(new { Message = "Please upload a valid Excel file." });
        }

        var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return BadRequest(new { Message = "Only .xlsx files are allowed." });
        }

        var files = new List<BulkUploadFileInput>();
        if (images != null)
        {
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    files.Add(new BulkUploadFileInput(
                        image.OpenReadStream(),
                        image.FileName,
                        image.ContentType,
                        image.Length));
                }
            }
        }

        await using var excelStream = excelFile.OpenReadStream();
        var jobId = await _bulkUploadService.CreateJobAsync(
            excelStream,
            excelFile.FileName,
            files,
            cancellationToken);

        return AcceptedAtAction(
            nameof(GetStatus),
            new { jobId },
            new
            {
                JobId = jobId,
                Status = "Pending",
                Message = "Bulk upload job created successfully.",
                ImageCount = files.Count
            });
    }

    [HttpGet("{jobId:int}")]
    public async Task<IActionResult> GetStatus(
        int jobId,
        CancellationToken cancellationToken)
    {
        var result = await _bulkUploadService.GetStatusAsync(jobId, cancellationToken);
        if (result == null)
        {
            return NotFound(new { Message = "Bulk upload job not found." });
        }

        return Ok(result);
    }
}

public sealed record BulkUploadFileRequest(
    string FileName,
    string? ContentType,
    long FileSize);

public sealed record CreateBulkUploadTargetsRequest(
    BulkUploadFileRequest ExcelFile,
    IReadOnlyList<BulkUploadFileRequest>? Images);

public sealed record StagedBulkUploadFileRequest(
    string FileName,
    string? ContentType,
    long FileSize,
    string BlobName);

public sealed record CompleteBulkUploadRequest(
    StagedBulkUploadFileRequest ExcelFile,
    IReadOnlyList<StagedBulkUploadFileRequest>? Images);