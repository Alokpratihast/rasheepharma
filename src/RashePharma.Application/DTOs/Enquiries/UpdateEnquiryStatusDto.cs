namespace RashePharma.Application.DTOs.Enquiries;

public class UpdateEnquiryStatusDto
{
    public string Status { get; set; } = string.Empty;

    public string? Comment { get; set; }
}