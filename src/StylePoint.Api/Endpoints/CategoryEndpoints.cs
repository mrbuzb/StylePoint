using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var categoryGroup = app.MapGroup("/api/categories")
                               .WithTags("CategoryManagement").RequireAuthorization();

        categoryGroup.MapGet("/",
        async (ICategoryService categoryService) =>
        {
            var categories = await categoryService.GetAllAsync();
            return Results.Ok(categories);
        })
        .WithName("GetAllCategories");

        categoryGroup.MapGet("/{id:long}",
        async (long id, ICategoryService categoryService) =>
        {
            var category = await categoryService.GetByIdAsync(id);
            return category is not null ? Results.Ok(category) : Results.NotFound();
        })
        .WithName("GetCategoryById");

        
    }
}
