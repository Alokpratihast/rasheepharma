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

    public BulkUploadController(
        IBulkUploadService bulkUploadService)
    {
        _bulkUploadService = bulkUploadService;
    }

    // POST: api/admin/bulk-upload
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateJob(
        IFormFile excelFile,
        List<IFormFile>? images,
        CancellationToken cancellationToken)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            return BadRequest(new
            {
                Message = "Please upload a valid Excel file."
            });
        }

        var extension = Path.GetExtension(
            excelFile.FileName).ToLowerInvariant();

        if (extension != ".xlsx")
        {
            return BadRequest(new
            {
                Message = "Only .xlsx files are allowed."
            });
        }

        var files = new List<BulkUploadFileInput>();

        if (images != null)
        {
            foreach (var image in images)
            {
                if (image.Length == 0)
                {
                    continue;
                }

                files.Add(
                    new BulkUploadFileInput(
                        image.OpenReadStream(),
                        image.FileName,
                        image.ContentType,
                        image.Length));
            }
        }

        await using var excelStream =
            excelFile.OpenReadStream();

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

    // GET: api/admin/bulk-upload/{jobId}
    [HttpGet("{jobId:int}")]
    public async Task<IActionResult> GetStatus(
        int jobId,
        CancellationToken cancellationToken)
    {
        var result = await _bulkUploadService.GetStatusAsync(
            jobId,
            cancellationToken);

        if (result == null)
        {
            return NotFound(new
            {
                Message = "Bulk upload job not found."
            });
        }

        return Ok(result);
    }
}