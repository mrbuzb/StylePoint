using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using StylePoint.Application.Dtos;
using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        // 🟢 Place order
        group.MapPost("/", async (HttpContext httpContext, OrderCreateDto dto, IOrderService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var order = await service.PlaceOrderAsync(userId, dto);
            return Results.Ok(order);
        })
        .WithName("PlaceOrder");

        // 🟢 Get order by ID
        group.MapGet("/{orderId:long}", async (HttpContext httpContext, long orderId, IOrderService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var order = await service.GetByIdAsync(userId, orderId);
            return order is not null ? Results.Ok(order) : Results.NotFound();
        })
        .WithName("GetOrderById");

        // 🟢 Get all orders of user
        group.MapGet("/", async (HttpContext httpContext, IOrderService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var orders = await service.GetUserOrdersAsync(userId);
            return Results.Ok(orders);
        })
        .WithName("GetUserOrders");

        // 🟢 Cancel order
        group.MapDelete("/{orderId:long}", async (HttpContext httpContext, long orderId, IOrderService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await service.CancelOrderAsync(userId, orderId);
            return Results.NoContent();
        })
        .WithName("CancelOrder");

        return app;
    }
}
