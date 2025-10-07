using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using StylePoint.Application.Dtos;
using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Api.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags").RequireAuthorization();

        // 🔹 Get all tags
        group.MapGet("/", async (ITagService service) =>
        {
            var tags = await service.GetAllAsync();
            return Results.Ok(tags);
        })
        .WithName("GetAllTags");

        // 🔹 Get tag by Id
        group.MapGet("/{id:long}", async (long id, ITagService service) =>
        {
            var tag = await service.GetByIdAsync(id);
            return tag is null ? Results.NotFound() : Results.Ok(tag);
        })
        .WithName("GetTagById");

        

        return app;
    }
}
