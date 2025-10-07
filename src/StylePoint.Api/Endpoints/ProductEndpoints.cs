using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", async (IProductService service) =>
        {
            var products = await service.GetAllAsync();
            return Results.Ok(products);
        })
        .WithName("GetAllProducts");

        group.MapGet("/{id:long}", async (long id, IProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        })
        .WithName("GetProductById");

        group.MapGet("/category/{categoryId:long}", async (long categoryId, IProductService service) =>
        {
            var products = await service.GetByCategoryAsync(categoryId);
            return Results.Ok(products);
        })
        .WithName("GetProductsByCategory");

        group.MapGet("/brand/{brandId:long}", async (long brandId, IProductService service) =>
        {
            var products = await service.GetByBrandAsync(brandId);
            return Results.Ok(products);
        })
        .WithName("GetProductsByBrand");

        group.MapGet("/tag/{tagId:long}", async (long tagId, IProductService service) =>
        {
            var products = await service.GetByTagAsync(tagId);
            return Results.Ok(products);
        })
        .WithName("GetProductsByTag");

        group.MapGet("/best-sellers", async (IProductService service) =>
        {
            var products = await service.GetBestSellersAsync();
            return Results.Ok(products);
        })
        .WithName("GetBestSellers");

        group.MapGet("/new-arrivals", async (IProductService service) =>
        {
            var products = await service.GetNewArrivalsAsync();
            return Results.Ok(products);
        })
        .WithName("GetNewArrivals");

        group.MapGet("/search", async (string keyword, IProductService service) =>
        {
            var products = await service.SearchAsync(keyword);
            return Results.Ok(products);
        })
        .WithName("SearchProducts");

        return app;
    }
}
