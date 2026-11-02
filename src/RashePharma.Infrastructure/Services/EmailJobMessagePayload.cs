namespace RashePharma.Infrastructure.Services;

public sealed class EmailJobMessagePayload
{
    public string ToName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string HtmlBody { get; set; } = string.Empty;
}
