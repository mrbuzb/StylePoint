using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Application.Dtos;
using System.Security.Claims;

namespace StylePoint.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var cartGroup = app.MapGroup("/api/cart")
                           .WithTags("CartManagement")
                           .RequireAuthorization(); // Faqat login bo‘lganlarga

        // Get user cart
        cartGroup.MapGet("/", async (HttpContext httpContext, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var cart = await cartService.GetUserCartAsync(userId);
            return Results.Ok(cart);
        })
        .WithName("GetUserCart");

        // Add item to cart
        cartGroup.MapPost("/", async (HttpContext httpContext, [FromBody] CartItemCreateDto dto, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await cartService.AddToCartAsync(userId, dto);
            return Results.Ok(new { success = true, message = "Item added to cart" });
        })
        .WithName("AddToCart");

        // Update quantity
        cartGroup.MapPatch("/{cartItemId:long}/quantity", async (HttpContext httpContext, long cartItemId, [FromBody] int quantity, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await cartService.UpdateQuantityAsync(userId, cartItemId, quantity);
            return Results.Ok(new { success = true });
        })
        .WithName("UpdateCartQuantity");

        // Remove item
        cartGroup.MapDelete("/{cartItemId:long}", async (HttpContext httpContext, long cartItemId, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await cartService.RemoveFromCartAsync(userId, cartItemId);
            return Results.Ok(new { success = true, message = "Item removed from cart" });
        })
        .WithName("RemoveFromCart");

        // Clear cart
        cartGroup.MapDelete("/clear", async (HttpContext httpContext, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await cartService.ClearCartAsync(userId);
            return Results.Ok(new { success = true, message = "Cart cleared" });
        })
        .WithName("ClearCart");

        // Get subtotal
        cartGroup.MapGet("/subtotal", async (HttpContext httpContext, ICartService cartService) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var subtotal = await cartService.CalculateSubtotalAsync(userId);
            return Results.Ok(new { subtotal });
        })
        .WithName("GetCartSubtotal");
    }
}
