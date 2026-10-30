using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Entities.Email;

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

    public DbSet<PasswordResetToken> PasswordResetTokens
    => Set<PasswordResetToken>();

    public DbSet<RefreshToken> RefreshTokens
    => Set<RefreshToken>();

    public DbSet<RevokedAccessToken> RevokedAccessTokens
    => Set<RevokedAccessToken>();

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

    public DbSet<BulkUploadFile> BulkUploadFiles
    => Set<BulkUploadFile>();

    public DbSet<StripeWebhookEvent> StripeWebhookEvents
    => Set<StripeWebhookEvent>();

    public DbSet<EmailNotification> EmailNotifications
    => Set<EmailNotification>();
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

    public DbSet<LocationSeoPage> LocationSeoPages => Set<LocationSeoPage>();

    public DbSet<WebsiteContent> WebsiteContents
        => Set<WebsiteContent>();

    // Kept separate from WebsiteContent because articles have publication and SEO lifecycles.
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<BulkUploadJob> BulkUploadJobs
    => Set<BulkUploadJob>();

    public DbSet<BulkUploadError> BulkUploadErrors
    => Set<BulkUploadError>();

    public DbSet<ExternalLead> ExternalLeads => Set<ExternalLead>();
    public DbSet<LeadImportBatch> LeadImportBatches => Set<LeadImportBatch>();
    public DbSet<LeadImportRow> LeadImportRows => Set<LeadImportRow>();
    public DbSet<LeadOutreachCampaign> LeadOutreachCampaigns => Set<LeadOutreachCampaign>();
    public DbSet<LeadOutreachRecipient> LeadOutreachRecipients => Set<LeadOutreachRecipient>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();


    public DbSet<EmailJob> EmailJobs
        => Set<EmailJob>();

    public DbSet<EmailOutboxMessage> EmailOutboxMessages
        => Set<EmailOutboxMessage>();

    public DbSet<EmailDeliveryEvent> EmailDeliveryEvents
        => Set<EmailDeliveryEvent>();

    public DbSet<EmailQuotaBucket> EmailQuotaBuckets
        => Set<EmailQuotaBucket>();

    public DbSet<EmailQuotaReservation> EmailQuotaReservations
        => Set<EmailQuotaReservation>();

    public DbSet<EmailSuppression> EmailSuppressions
        => Set<EmailSuppression>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)




    {
        base.OnModelCreating(modelBuilder);

         // Discover and apply all IEntityTypeConfiguration classes.
        modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<ExternalLead>(entity =>
        {
            entity.HasIndex(lead => lead.EmailNormalized).IsUnique()
                .HasFilter("[EmailNormalized] IS NOT NULL");
            entity.Property(lead => lead.Email).HasMaxLength(320);
            entity.Property(lead => lead.EmailNormalized).HasMaxLength(320);
            entity.Property(lead => lead.Phone).HasMaxLength(40);
            entity.Property(lead => lead.Name).HasMaxLength(200).IsRequired();
            entity.Property(lead => lead.Company).HasMaxLength(200);
            entity.Property(lead => lead.Country).HasMaxLength(100);
            entity.Property(lead => lead.BusinessType).HasMaxLength(100);
            entity.Property(lead => lead.ProductInterest).HasMaxLength(1000);
            entity.Property(lead => lead.Source).HasMaxLength(80).IsRequired();
            entity.Property(lead => lead.PermissionStatus).HasMaxLength(20).IsRequired();
            entity.Property(lead => lead.PermissionEvidence).HasMaxLength(500);
            entity.Property(lead => lead.HistoricalMailSentOn).HasMaxLength(200);
            entity.Property(lead => lead.RowVersion).IsRowVersion();

        });


        modelBuilder.Entity<LeadImportBatch>(entity =>
        {
            entity.Property(batch => batch.FileName).HasMaxLength(260).IsRequired();
            entity.Property(batch => batch.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(batch => batch.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(batch => new { batch.Sha256, batch.Status });
        });

        modelBuilder.Entity<LeadImportRow>(entity =>
        {
            entity.Property(row => row.SheetName).HasMaxLength(40).IsRequired();
            entity.Property(row => row.Email).HasMaxLength(320);
            entity.Property(row => row.EmailNormalized).HasMaxLength(320);
            entity.Property(row => row.Phone).HasMaxLength(40);
            entity.Property(row => row.Name).HasMaxLength(200).IsRequired();
            entity.Property(row => row.Company).HasMaxLength(200);
            entity.Property(row => row.ProductInterest).HasMaxLength(1000);
            entity.Property(row => row.HistoricalMailSentOn).HasMaxLength(200);
            entity.Property(row => row.ValidationError).HasMaxLength(500);
            entity.HasOne(row => row.Batch).WithMany(batch => batch.Rows)
                .HasForeignKey(row => row.BatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(row => new { row.BatchId, row.SheetName, row.SheetRow }).IsUnique();
        });

        modelBuilder.Entity<LeadOutreachCampaign>(entity =>
        {
            entity.Property(campaign => campaign.Name).HasMaxLength(160).IsRequired();
            entity.Property(campaign => campaign.Subject).HasMaxLength(200).IsRequired();
            entity.Property(campaign => campaign.BodyText).HasMaxLength(12000).IsRequired();
            entity.Property(campaign => campaign.Status).HasMaxLength(20).IsRequired();
            entity.Property(campaign => campaign.CreatedBy).HasMaxLength(320).IsRequired();
            entity.HasIndex(campaign => new { campaign.Status, campaign.CreatedAtUtc });
        });

        modelBuilder.Entity<LeadOutreachRecipient>(entity =>
        {
            entity.Property(recipient => recipient.Status).HasMaxLength(20).IsRequired();
            entity.Property(recipient => recipient.ErrorMessage).HasMaxLength(1000);
            entity.HasIndex(recipient => new { recipient.CampaignId, recipient.LeadId }).IsUnique();
            entity.HasIndex(recipient => new { recipient.Status, recipient.CreatedAtUtc });
            entity.HasOne(recipient => recipient.Campaign).WithMany(campaign => campaign.Recipients)
                .HasForeignKey(recipient => recipient.CampaignId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(recipient => recipient.Lead).WithMany()
                .HasForeignKey(recipient => recipient.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeadActivity>(entity =>
        {
            entity.Property(activity => activity.Type).HasMaxLength(40).IsRequired();
            entity.Property(activity => activity.Details).HasMaxLength(1000).IsRequired();
            entity.Property(activity => activity.Actor).HasMaxLength(320).IsRequired();
            entity.HasIndex(activity => new { activity.LeadId, activity.CreatedAtUtc });
            entity.HasOne<ExternalLead>().WithMany().HasForeignKey(activity => activity.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Unique slugs and a published/date index keep public blog lookups deterministic and fast.
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasIndex(post => post.Slug).IsUnique();
            entity.HasIndex(post => new { post.Status, post.PublishedAtUtc });
            entity.Property(post => post.Title).HasMaxLength(180).IsRequired();
            entity.Property(post => post.Slug).HasMaxLength(200).IsRequired();
            entity.Property(post => post.Excerpt).HasMaxLength(500).IsRequired();
            entity.Property(post => post.AuthorName).HasMaxLength(120).IsRequired();
            entity.Property(post => post.Status).HasMaxLength(20).IsRequired();
            entity.Property(post => post.MetaTitle).HasMaxLength(180);
            entity.Property(post => post.MetaDescription).HasMaxLength(320);
            entity.Property(post => post.CanonicalUrl).HasMaxLength(2048);
            entity.Property(post => post.OgTitle).HasMaxLength(180);
            entity.Property(post => post.OgDescription).HasMaxLength(320);
            entity.Property(post => post.CoverImageUrl).HasMaxLength(2048);
            entity.Property(post => post.OriginalCoverImageUrl).HasMaxLength(2048);
            entity.Property(post => post.OgImageUrl).HasMaxLength(2048);
            entity.Property(post => post.ReviewerName).HasMaxLength(120);
            entity.Property(post => post.Content).HasMaxLength(100000).IsRequired();
        });

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
        // BulkUploadJob → BulkUploadError
        // =====================================================

        modelBuilder.Entity<BulkUploadError>()
            .HasOne(e => e.BulkUploadJob)
            .WithMany(j=> j.Errors)
            .HasForeignKey(e => e.BulkUploadJobId)
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
        // Order → EmailNotification
        // =====================================================

        modelBuilder.Entity<EmailNotification>()
            .HasOne(n => n.Order)
            .WithMany()
            .HasForeignKey(n => n.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Quotation → EmailNotification
        // =====================================================

        modelBuilder.Entity<EmailNotification>()
            .HasOne(n => n.Quotation)
            .WithMany()
            .HasForeignKey(n => n.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Enquiry → EmailNotification
        // =====================================================

        modelBuilder.Entity<EmailNotification>()
            .HasOne(n => n.Enquiry)
            .WithMany(e => e.EmailNotifications)
            .HasForeignKey(n => n.EnquiryId)
            .OnDelete(DeleteBehavior.Restrict);

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
        // BulkUploadJob → BulkUploadFile
        // =====================================================

        modelBuilder.Entity<BulkUploadFile>()
            .HasOne(f => f.BulkUploadJob)
            .WithMany(j => j.Files)
            .HasForeignKey(f => f.BulkUploadJobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .Property(t => t.RowVersion)
            .IsRowVersion();

        // =====================================================
        // RevokedAccessToken
        // =====================================================

        modelBuilder.Entity<RevokedAccessToken>(entity =>
        {
            entity.HasKey(t => t.Jti);

            entity.Property(t => t.Jti)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(t => t.ExpiresAtUtc)
                .IsRequired();

            entity.Property(t => t.RevokedAtUtc)
                .IsRequired();

            entity.HasIndex(t => t.ExpiresAtUtc);
        });


        // =====================================================
        // Unique Indexes
        // =====================================================

        modelBuilder.Entity<LocationSeoPage>(entity =>
        {
            entity.HasIndex(p => new { p.CountrySlug, p.CitySlug }).IsUnique();
            entity.Property(p => p.Country).IsRequired();
            entity.Property(p => p.CountryCode).IsRequired();
            entity.Property(p => p.CountrySlug).IsRequired();
            entity.Property(p => p.City).IsRequired();
            entity.Property(p => p.CitySlug).IsRequired();
            entity.Property(p => p.MetaTitle).IsRequired();
            entity.Property(p => p.MetaDescription).IsRequired();
            entity.Property(p => p.Heading).IsRequired();
            entity.Property(p => p.Intro).IsRequired();
            entity.Property(p => p.BuyerFocus).IsRequired();
        });

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(t => t.TokenFamilyId);

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(t => t.TokenHash)
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

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.StripeSessionId)
            .IsUnique()
            .HasFilter("[StripeSessionId] IS NOT NULL");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.CheckoutIdempotencyKey)
            .IsUnique()
            .HasFilter("[CheckoutIdempotencyKey] IS NOT NULL");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.OrderId)
            .IsUnique()
            .HasFilter("[Status] IN ('Pending', 'Failed')");

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("[StripePaymentIntentId] IS NOT NULL");



        // =====================================================
        // Email Notification Unique Indexes
        // =====================================================

        // Prevent duplicate Order notifications
        // e.g. only one OrderInvoice notification per order.
        modelBuilder.Entity<EmailNotification>()
            .HasIndex(n => new
            {
                n.OrderId,
                n.Type
            })
            .IsUnique()
            .HasFilter("[OrderId] IS NOT NULL");


        modelBuilder.Entity<EmailNotification>()
            .HasIndex(n => new
            {
                n.EnquiryId,
                n.Type
            })
            .IsUnique()
            .HasFilter("[EnquiryId] IS NOT NULL");

        // Prevent duplicate Quotation notifications
        // e.g. only one Quotation notification per quotation.
        modelBuilder.Entity<EmailNotification>()
            .HasIndex(n => new
            {
                n.QuotationId,
                n.Type
            })
            .IsUnique()
            .HasFilter("[QuotationId] IS NOT NULL");

        modelBuilder.Entity<Enquiry>()
            .HasIndex(e => e.EnquiryNumber)
            .IsUnique();

        modelBuilder.Entity<Quotation>()
            .HasIndex(q => q.QuoteNumber)
            .IsUnique();

        modelBuilder.Entity<StripeWebhookEvent>()
        .HasIndex(e => e.StripeEventId)
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


        // =========================================================
// User → PasswordResetToken
// =========================================================

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
