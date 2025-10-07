using StylePoint.Application.Dtos;
using StylePoint.Application.Services.Interfaces;
using System.Security.Claims;

namespace StylePoint.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments").RequireAuthorization();

        group.MapPost("/", async (HttpContext httpContext, PaymentCreateDto dto, IPaymentService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var payment = await service.ProcessPaymentAsync(userId, dto);
            return Results.Ok(payment);
        })
        .WithName("ProcessPayment");

        group.MapGet("/{paymentId:long}", async (HttpContext httpContext, long paymentId, IPaymentService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var payment = await service.GetByIdAsync(userId, paymentId);
            return payment is not null ? Results.Ok(payment) : Results.NotFound();
        })
        .WithName("GetPaymentById");

        group.MapPost("/{paymentId:long}/refund", async (HttpContext httpContext, long paymentId, IPaymentService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var result = await service.RefundPaymentAsync(userId, paymentId);
            return result ? Results.Ok(new { Message = "Refund successful" }) : Results.BadRequest(new { Message = "Refund failed" });
        })
        .WithName("RefundPayment");

        group.MapGet("/{paymentId:long}/status", async (HttpContext httpContext, long paymentId, IPaymentService service) =>
        {
            var userId = long.Parse(httpContext.User.FindFirstValue("UserId")!);
            var status = await service.GetPaymentStatusAsync(userId, paymentId);
            return Results.Ok(new { PaymentId = paymentId, Status = status.ToString() });
        })
        .WithName("GetPaymentStatus");

        return app;
    }
}
