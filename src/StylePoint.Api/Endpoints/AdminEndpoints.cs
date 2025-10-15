using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylePoint.Application.Dtos;
using StylePoint.Application.Services.Interfaces;
using System.Security.Claims;

namespace StylePoint.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/admin")
            .WithTags("AdminManagement")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin,User" });

        adminGroup.MapPut("/top-up-card", async (Guid cardNumber, long amount, IPaymentService service) =>
        {
            return Results.Ok(await service.TopUpCardAsync(cardNumber, amount));
        })
        .WithName("TopUpCard");

        adminGroup.MapPost("/upload-product-img", async (IFormFile file, ICloudService service) =>
        {
            return Results.Ok(await service.UploadImageAsync(file));
        })
        .WithName("UploadProductImage")
        .DisableAntiforgery();


        adminGroup.MapPost("/product", async ([FromBody] ProductCreateDto dto, IProductService service) =>
        {
            var product = await service.AddProductAsync(dto);
            return Results.Ok(product);
        })
        .WithName("AddProduct")
        .DisableAntiforgery();

        adminGroup.MapPut("/product{id:long}", async (long id, ProductUpdateDto dto, IProductService service) =>
        {
            var product = await service.UpdateProductAsync(id, dto);
            return Results.Ok(product);
        })
        .WithName("UpdateProduct");

        

        adminGroup.MapDelete("product/{id:long}", async (long id, IProductService service) =>
        {
            var deleted = await service.DeleteProductAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct");


        adminGroup.MapGet("/roles",
            async (IRoleService _roleService) =>
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Results.Ok(roles);
            })
            .WithName("GetAllRoles");

        adminGroup.MapGet("/users-by-role",
            [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, NoStore = false)]
        async (string role, IRoleService _roleService) =>
            {
                var users = await _roleService.GetAllUsersByRoleAsync(role);
                return Results.Ok(new { success = true, data = users });
            })
            .WithName("GetAllUsersByRole");

        adminGroup.MapDelete("/users/{userId:long}",
            async (long userId, HttpContext httpContext, IUserService userService) =>
            {
                var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                await userService.DeleteUserByIdAsync(userId, role);
                return Results.Ok();
            })
            .WithName("DeleteUser");

        adminGroup.MapPatch("/users/{userId:long}/role",
            async (long userId, string userRole, IUserService userService) =>
            {
                await userService.UpdateUserRoleAsync(userId, userRole);
                return Results.Ok();
            })
            .WithName("UpdateUserRole");

        var brandGroup = adminGroup.MapGroup("/brands");
        brandGroup.MapPost("/", async ([FromBody] string name, IBrandService service) =>
        {
            var created = await service.CreateAsync(name);
            return Results.Created($"/api/admin/brands/{created.Id}", created);
        }).WithName("CreateBrand");

        brandGroup.MapPut("/{id:long}", async (long id, [FromBody] string name, IBrandService service) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, name);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).WithName("UpdateBrand");

        brandGroup.MapDelete("/{id:long}", async (long id, IBrandService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.Ok(new { success = true }) : Results.NotFound();
        }).WithName("DeleteBrand");

        var categoryGroup = adminGroup.MapGroup("/categories");
        categoryGroup.MapPost("/", async ([FromBody] string name, ICategoryService service) =>
        {
            var created = await service.CreateAsync(name);
            return Results.Created($"/api/admin/categories/{created.Id}", created);
        }).WithName("CreateCategory");

        categoryGroup.MapPut("/{id:long}", async (long id, [FromBody] string name, ICategoryService service) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, name);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).WithName("UpdateCategory");

        categoryGroup.MapDelete("/{id:long}", async (long id, ICategoryService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.Ok(new { success = true }) : Results.NotFound();
        }).WithName("DeleteCategory");

        var discountGroup = adminGroup.MapGroup("/discounts");
        discountGroup.MapGet("/", async (IDiscountService service) =>
        {
            var all = await service.GetAllAsync();
            return Results.Ok(all);
        }).WithName("GetAllDiscounts");

        discountGroup.MapGet("/{id:long}", async (long id, IDiscountService service) =>
        {
            var discount = await service.GetByIdAsync(id);
            return discount is not null ? Results.Ok(discount) : Results.NotFound();
        }).WithName("GetDiscountById");

        discountGroup.MapPost("/", async ([FromBody] DiscountCreateDto dto, IDiscountService service) =>
        {
            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/admin/discounts/{created.Id}", created);
        }).WithName("CreateDiscount");

        discountGroup.MapPut("/{id:long}", async (long id, [FromBody] DiscountUpdateDto dto, IDiscountService service) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, dto);
                return Results.Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).WithName("UpdateDiscount");

        discountGroup.MapDelete("/{id:long}", async (long id, IDiscountService service) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.Ok(new { success = true }) : Results.NotFound();
        }).WithName("DeleteDiscount");

        var tagGroup = adminGroup.MapGroup("/tags");
        tagGroup.MapPost("/", async (string name, ITagService service) =>
        {
            var tag = await service.CreateAsync(name);
            return Results.Created($"/api/admin/tags/{tag.Id}", tag);
        }).WithName("CreateTag");

        tagGroup.MapPut("/{id:long}", async (long id, string name, ITagService service) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, name);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).WithName("UpdateTag");

        tagGroup.MapDelete("/{id:long}", async (long id, ITagService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        }).WithName("DeleteTag");
    }
}
