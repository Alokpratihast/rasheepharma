namespace RashePharma.Application.DTOs.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int TotalProducts { get; set; }

    public int FeaturedProducts { get; set; }

    public int TotalCategories { get; set; }

    public int TotalEnquiries { get; set; }

    public int PendingEnquiries { get; set; }

    public int TotalOrders { get; set; }
}