using RashePharma.Application.DTOs.Admin;

namespace RashePharma.Application.Interfaces.Services;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
}