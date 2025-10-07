using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Api.Endpoints;

public static class DiscountEndpoints
{
    public static void MapDiscountEndpoints(this WebApplication app)
    {
        var discountGroup = app.MapGroup("/api/discounts")
                               .WithTags("DiscountManagement").RequireAuthorization();

        discountGroup.MapPost("/apply", async (string code, decimal orderAmount, long userId, IDiscountService service) =>
        {
            var discount = await service.ApplyDiscountAsync(code, userId, orderAmount);
            return discount is not null ? Results.Ok(discount) : Results.NotFound(new { message = "Invalid or expired discount code" });
        })
        .WithName("ApplyDiscount");

        discountGroup.MapGet("/active", async (IDiscountService service) =>
        {
            var active = await service.GetActiveDiscountsAsync();
            return Results.Ok(active);
        })
        .WithName("GetActiveDiscounts");

        discountGroup.MapGet("/validate/{code}", async (string code, IDiscountService service) =>
        {
            var isValid = await service.ValidateDiscountAsync(code);
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
