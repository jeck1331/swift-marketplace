using System.Security.Claims;
using IdentityService.Application;
using IdentityService.Application.DTO;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartRepository _cartRepo;

    public CartController(ICartRepository cartRepo)
    {
        _cartRepo = cartRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetCart(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var items = await _cartRepo.GetByUserIdAsync(userId, ct);
        return Ok(items.Select(x => new CartItemDto(x.ProductId, x.Quantity)));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(
        [FromBody] CartItemDto request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _cartRepo.UpsertAsync(new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            AddedAt = DateTime.UtcNow
        }, ct);
        return Ok();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromCart(
        Guid productId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _cartRepo.RemoveAsync(userId, productId, ct);
        return NoContent();
    }
}