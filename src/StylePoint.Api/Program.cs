using AutoLedger.Api.Configurations;
using StylePoint.Api.Endpoints;
using StylePoint.Api.Extensions;
using StylePoint.Api.Middlewares;

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
