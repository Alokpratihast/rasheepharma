using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Orders;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }


    // =========================================================
    // Get Current User's Orders
    // =========================================================

    [HttpGet("my-orders")]
    public async Task<ActionResult<List<OrderListDto>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var orders = await _orderService.GetMyOrdersAsync(userId.Value);

        return Ok(orders);
    }


    // =========================================================
    // Get Current User's Order Details
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailsDto>> GetById(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var order = await _orderService.GetByIdAsync(
            id,
            userId.Value);

        if (order == null)
            return NotFound();

        return Ok(order);
    }


    // =========================================================
    // Get Current User's Order By Order Number
    // =========================================================

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDetailsDto>> GetByOrderNumber(
        string orderNumber)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var order = await _orderService.GetByOrderNumberAsync(
            orderNumber,
            userId.Value);

        if (order == null)
            return NotFound();

        return Ok(order);
    }


    // =========================================================
    // Create Order For Current User
    // =========================================================

    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> Create(
        CreateOrderDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var order = await _orderService.CreateAsync(
                userId.Value,
                dto);

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    // =========================================================
    // Update Order Status
    // Admin Only
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateOrderStatusDto dto)
    {
        var updated = await _orderService.UpdateStatusAsync(
            id,
            dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }


    // =========================================================
    // Helper
    // =========================================================

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return null;

        if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
        {
            return null;
        }

        return userId;
    }
}