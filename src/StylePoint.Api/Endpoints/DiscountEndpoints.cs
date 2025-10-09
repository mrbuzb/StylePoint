using Microsoft.AspNetCore.Http;
using StylePoint.Application.Services.Interfaces;
using System.Net.Http;
using System.Security.Claims;

namespace StylePoint.Api.Endpoints;

public static class DiscountEndpoints
{
    public static void MapDiscountEndpoints(this WebApplication app)
    {
        var discountGroup = app.MapGroup("/api/discounts")
                               .WithTags("DiscountManagement").RequireAuthorization();

        discountGroup.MapPost("/apply", async (string code,IDiscountService service,HttpContext httpContext) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var discount = await service.ApplyDiscountAsync(userId,code);
            return Results.Ok(discount);
        })
        .WithName("ApplyDiscount");

        discountGroup.MapGet("/active", async (IDiscountService service) =>
        {
            var active = await service.GetActiveDiscountsAsync();
            return Results.Ok(active);
        })
        .WithName("GetActiveDiscounts");

        discountGroup.MapGet("/validate/{code}", async (string code, IDiscountService service,HttpContext httpContext) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var isValid = await service.ValidateDiscountAsync(code,userId);
            return Results.Ok(new { code, isValid });
        })
        .WithName("ValidateDiscount");

        discountGroup.MapGet("/code/{code}", async (string code, IDiscountService service) =>
        {
            var discount = await service.GetByCodeAsync(code);
            return discount is not null ? Results.Ok(discount) : Results.NotFound();
        })
        .WithName("GetDiscountByCode");
    }
}
