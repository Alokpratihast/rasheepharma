namespace RashePharma.Application.DTOs.Categories;

public class CategoryNavigationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public List<CategoryNavigationDto> Children { get; set; } = new();

    public List<CategoryNavigationProductDto> Products { get; set; } = new();
}