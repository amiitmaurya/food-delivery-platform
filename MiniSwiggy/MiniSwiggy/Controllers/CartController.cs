using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Cart;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private int GetUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }

    // GET: api/cart
    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var result = await _cartService.GetMyCartAsync(GetUserId());

        if (result == null)
            return Ok(new { cartId = 0, items = new List<object>(), totalAmount = 0 });

        return Ok(result);
    }

    // POST: api/cart
    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartRequest request)
    {
        var result = await _cartService.AddToCartAsync(GetUserId(), request);

        if (!result)
            return BadRequest(new { message = "Unable to add item to cart." });

        return Ok(new
        {
            message = "Item added to cart successfully."
        });
    }

    // PUT: api/cart/{cartItemId}
    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, UpdateCartItemRequest request)
    {
        var result = await _cartService.UpdateCartItemAsync(cartItemId, request);

        if (!result)
            return BadRequest(new { message = "Unable to update cart item." });

        return Ok(new
        {
            message = "Cart updated successfully."
        });
    }

    // DELETE: api/cart/{cartItemId}
    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
    {
        var result = await _cartService.RemoveCartItemAsync(cartItemId);

        if (!result)
            return BadRequest(new { message = "Unable to remove cart item." });

        return Ok(new
        {
            message = "Item removed from cart successfully."
        });
    }

    // DELETE: api/cart/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCartAsync(GetUserId());

        if (!result)
            return BadRequest(new { message = "Unable to clear cart." });

        return Ok(new
        {
            message = "Cart cleared successfully."
        });
    }
}