namespace RashePharma.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // Navigation Properties
    public Cart Cart { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;
}