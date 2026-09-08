using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Cart;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }


    // =========================================================
    // GET CURRENT USER CART
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var cart = await _cartService.GetCartAsync(
            userId.Value);

        return Ok(cart);
    }


    // =========================================================
    // ADD ITEM TO CURRENT USER CART
    // =========================================================

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart(
        AddToCartDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var cart = await _cartService.AddToCartAsync(
            userId.Value,
            dto);

        return Ok(cart);
    }


    // =========================================================
    // UPDATE CART ITEM
    // =========================================================

    [HttpPut("items/{productVariantId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(
        int productVariantId,
        [FromQuery] int quantity)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var cart = await _cartService.UpdateItemAsync(
            userId.Value,
            productVariantId,
            quantity);

        if (cart == null)
            return NotFound();

        return Ok(cart);
    }


    // =========================================================
    // REMOVE CART ITEM
    // =========================================================

    [HttpDelete("items/{productVariantId:int}")]
    public async Task<IActionResult> RemoveItem(
        int productVariantId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var removed = await _cartService.RemoveItemAsync(
            userId.Value,
            productVariantId);

        if (!removed)
            return NotFound();

        return NoContent();
    }


    // =========================================================
    // JWT USER ID
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