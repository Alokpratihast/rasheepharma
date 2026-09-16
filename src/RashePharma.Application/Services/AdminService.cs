using RashePharma.Application.DTOs.Admin;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEnquiryRepository _enquiryRepository;
    private readonly IOrderRepository _orderRepository;

    public AdminService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IEnquiryRepository enquiryRepository,
        IOrderRepository orderRepository)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _enquiryRepository = enquiryRepository;
        _orderRepository = orderRepository;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var totalUsers =
            await _userRepository.GetTotalCountAsync();

        var activeUsers =
            await _userRepository.GetActiveCountAsync();

        var totalProducts =
            await _productRepository.GetTotalCountAsync();

        var featuredProducts =
            await _productRepository.GetFeaturedCountAsync();

        var totalCategories =
            await _categoryRepository.GetTotalCountAsync();

        var totalEnquiries =
            await _enquiryRepository.GetTotalCountAsync();

        var pendingEnquiries =
            await _enquiryRepository.GetPendingCountAsync();

        var totalOrders =
            await _orderRepository.GetTotalCountAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalProducts = totalProducts,
            FeaturedProducts = featuredProducts,
            TotalCategories = totalCategories,
            TotalEnquiries = totalEnquiries,
            PendingEnquiries = pendingEnquiries,
            TotalOrders = totalOrders
        };
    }
}