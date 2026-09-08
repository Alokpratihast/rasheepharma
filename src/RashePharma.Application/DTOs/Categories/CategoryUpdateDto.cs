namespace RashePharma.Application.DTOs.Categories;

public class CategoryUpdateDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}