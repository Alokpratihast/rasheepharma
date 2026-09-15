namespace RashePharma.Application.DTOs.Categories;

public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}