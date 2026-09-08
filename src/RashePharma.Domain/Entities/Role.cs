namespace RashePharma.Domain.Entities;

public class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Navigation Property
    public ICollection<User> Users { get; set; } = new List<User>();
}