namespace RashePharma.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Parent Category
    public int? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    // Child Categories
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}