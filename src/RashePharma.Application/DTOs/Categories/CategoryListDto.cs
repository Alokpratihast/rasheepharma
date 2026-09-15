namespace RashePharma.Application.DTOs.Categories;

public class CategoryListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }
}