using AutoLedger.Api.Configurations;
using StylePoint.Api.Endpoints;
using StylePoint.Api.Extensions;
using StylePoint.Api.Middlewares;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Infrastructure.Persistence.TgService;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.ConfigureDataBase();
builder.ConfigurationJwtAuth();
builder.ConfigureJwtSettings();
//builder.ConfigureSerilog();
builder.Services.ConfigureDependecies();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var botToken = builder.Configuration["TelegramBot:Token"];
if (string.IsNullOrEmpty(botToken))
    throw new InvalidOperationException("? Telegram bot token topilmadi!");
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<ProductBotService>();

builder.Services.AddSingleton<ITelegramBotService, TgBotService>();
builder.Services.AddSingleton<IHostedService>(sp => (TgBotService)sp.GetRequiredService<ITelegramBotService>());


ServiceCollectionExtensions.AddSwaggerWithJwt(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost5173");
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapProductEndpoints();
app.MapCategoryEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();
app.MapDeliveryAddressEndpoints();
app.MapTagEndpoints();
app.MapPaymentEndpoints();
app.MapBrandEndpoints();

app.MapControllers();

app.Run();
