using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Application.Dtos;
using System.Security.Claims;

namespace StylePoint.Api.Endpoints;

public static class DeliveryAddressEndpoints
{
    public static void MapDeliveryAddressEndpoints(this WebApplication app)
    {
        var addressGroup = app.MapGroup("/api/addresses")
                              .WithTags("DeliveryAddressManagement")
                              .RequireAuthorization(); // faqat login bo‘lganlarga

        // Get all user addresses
        addressGroup.MapGet("/", async (HttpContext httpContext, IDeliveryAddressService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var addresses = await service.GetUserAddressesAsync(userId);
            return Results.Ok(addresses);
        })
        .WithName("GetUserAddresses");

        // Add new address
        addressGroup.MapPost("/", async (HttpContext httpContext, [FromBody] DeliveryAddressCreateDto dto, IDeliveryAddressService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await service.AddAddressAsync(userId, dto);
            return Results.Ok(new { success = true, message = "Address added successfully" });
        })
        .WithName("AddAddress");

        // Update address
        addressGroup.MapPut("/", async (HttpContext httpContext, [FromBody] DeliveryAddressUpdateDto dto, IDeliveryAddressService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await service.UpdateAddressAsync(userId, dto);
            return Results.Ok(new { success = true, message = "Address updated successfully" });
        })
        .WithName("UpdateAddress");

        // Delete address
        addressGroup.MapDelete("/{addressId:long}", async (HttpContext httpContext, long addressId, IDeliveryAddressService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            await service.DeleteAddressAsync(userId, addressId);
            return Results.Ok(new { success = true, message = "Address deleted successfully" });
        })
        .WithName("DeleteAddress");
    }
}
