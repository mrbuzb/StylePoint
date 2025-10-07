using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Application.Dtos;

namespace StylePoint.Api.Endpoints;

public static class BrandEndpoints
{
    public static void MapBrandEndpoints(this WebApplication app)
    {
        var brandGroup = app.MapGroup("/api/brands")
                            .WithTags("BrandManagement").RequireAuthorization();

        brandGroup.MapGet("/",
        async (IBrandService brandService) =>
        {
            var brands = await brandService.GetAllAsync();
            return Results.Ok(brands);
        })
        .WithName("GetAllBrands");

        brandGroup.MapGet("/{id:long}",
        async (long id, IBrandService brandService) =>
        {
            var brand = await brandService.GetByIdAsync(id);
            return brand is not null ? Results.Ok(brand) : Results.NotFound();
        })
        .WithName("GetBrandById");
    }
}
