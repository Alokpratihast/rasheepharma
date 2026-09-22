using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductVariant> ProductVariants
        => Set<ProductVariant>();

    public DbSet<ProductImage> ProductImages
        => Set<ProductImage>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistories
        => Set<OrderStatusHistory>();

    public DbSet<Payment> Payments
        => Set<Payment>();

    public DbSet<Enquiry> Enquiries => Set<Enquiry>();

    public DbSet<EnquiryItem> EnquiryItems
        => Set<EnquiryItem>();

    public DbSet<Quotation> Quotations
        => Set<Quotation>();

    public DbSet<QuotationItem> QuotationItems
        => Set<QuotationItem>();

    public DbSet<PartnerRequest> PartnerRequests
        => Set<PartnerRequest>();

    public DbSet<Partner> Partners => Set<Partner>();

    public DbSet<WebsiteContent> WebsiteContents
        => Set<WebsiteContent>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // Category → Category
        // Self Referencing
        // =====================================================

        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Category → Product
        // =====================================================

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Product → ProductVariant
        // =====================================================

        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Product → ProductImage
        // =====================================================

        modelBuilder.Entity<ProductImage>()
            .HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Role → User
        // =====================================================

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // User → Address
        // =====================================================

        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // User → Cart
        // =====================================================

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Cart → CartItem
        // =====================================================

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // CartItem → ProductVariant
        // =====================================================

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // User → Order
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Order → OrderItem
        // =====================================================

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // OrderItem → ProductVariant
        // =====================================================

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Order → OrderStatusHistory
        // =====================================================

        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Order → Payment
        // =====================================================

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // User → Enquiry
        // =====================================================

        modelBuilder.Entity<Enquiry>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enquiries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Enquiry → EnquiryItem
        // =====================================================

        modelBuilder.Entity<EnquiryItem>()
            .HasOne(ei => ei.Enquiry)
            .WithMany(e => e.Items)
            .HasForeignKey(ei => ei.EnquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // EnquiryItem → ProductVariant
        // =====================================================

        modelBuilder.Entity<EnquiryItem>()
            .HasOne(ei => ei.ProductVariant)
            .WithMany()
            .HasForeignKey(ei => ei.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Enquiry → Quotation
        // =====================================================

        modelBuilder.Entity<Quotation>()
            .HasOne(q => q.Enquiry)
            .WithMany()
            .HasForeignKey(q => q.EnquiryId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // User → Quotation
        // =====================================================

        modelBuilder.Entity<Quotation>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Quotation → QuotationItem
        // =====================================================

        modelBuilder.Entity<QuotationItem>()
            .HasOne(qi => qi.Quotation)
            .WithMany(q => q.Items)
            .HasForeignKey(qi => qi.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // QuotationItem → ProductVariant
        // =====================================================

        modelBuilder.Entity<QuotationItem>()
            .HasOne(qi => qi.ProductVariant)
            .WithMany()
            .HasForeignKey(qi => qi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // User → PartnerRequest
        // =====================================================

        modelBuilder.Entity<PartnerRequest>()
            .HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // User → Partner
        // =====================================================

        modelBuilder.Entity<Partner>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // PartnerRequest → Partner
        // One-to-One
        // =====================================================

        modelBuilder.Entity<Partner>()
            .HasOne(p => p.PartnerRequest)
            .WithOne()
            .HasForeignKey<Partner>(
                p => p.PartnerRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Unique Indexes
        // =====================================================

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Enquiry>()
            .HasIndex(e => e.EnquiryNumber)
            .IsUnique();

        modelBuilder.Entity<Quotation>()
            .HasIndex(q => q.QuoteNumber)
            .IsUnique();

        // =====================================================
        // Decimal Precision
        // =====================================================

        modelBuilder.Entity<ProductVariant>()
            .Property(v => v.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Quotation>()
            .Property(q => q.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<QuotationItem>()
            .Property(qi => qi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<QuotationItem>()
            .Property(qi => qi.TotalPrice)
            .HasPrecision(18, 2);
    }
}