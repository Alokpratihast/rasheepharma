namespace RashePharma.Application.DTOs.Categories;

public class CategoryDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}