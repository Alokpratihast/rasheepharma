using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Admin;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> Dashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();

        return Ok(dashboard);
    }
}