using InventoryOrder.Application;
using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Application.Features.Auth.Login;
using InventoryOrder.Application.Features.Inventory.AdjustStock;
using InventoryOrder.Application.Features.Inventory.GetStockLevels;
using InventoryOrder.Application.Features.Orders.PlaceOrder;
using InventoryOrder.Application.Features.Orders.ShipOrder;
using InventoryOrder.Application.Features.Ping;
using InventoryOrder.Infrastructure;
using InventoryOrder.Infrastructure.Persistence;
using InventoryOrder.Infrastructure.Persistence.Seed;
using InventoryOrder.Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("InventoryOrder.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("writes", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is missing from configuration.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "InventoryOrder.Api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "InventoryOrder.Client",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanAdjustStock", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Admin", "WarehouseManager"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var mysqlConnectionString =
    builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:MySql is missing from configuration.");

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Redis is missing from configuration.");

builder.Services
    .AddHealthChecks()
    .AddMySql(mysqlConnectionString, name: "mysql")
    .AddRedis(redisConnectionString, name: "redis");

var app = builder.Build();

if (app.Configuration.GetValue("Database:Seed", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(
        dbContext,
        scope.ServiceProvider.GetRequiredService<IPasswordService>());
}

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapHealthChecks("/health");
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async (
    LoginCommand command,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(command, cancellationToken);
    return Results.Ok(response);
}).AllowAnonymous().RequireRateLimiting("auth");

app.MapGet("/ping", async (
    string message,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(
        new PingQuery(message),
        cancellationToken);

    return Results.Ok(response);
});

app.MapPost("/orders", async (
    PlaceOrderCommand command,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(command, cancellationToken);
    return Results.Created($"/orders/{response.OrderId}", response);
}).RequireAuthorization().RequireRateLimiting("writes");

app.MapPost("/inventory/adjustments", async (
    AdjustStockCommand command,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(command, cancellationToken);
    return Results.Ok(response);
}).RequireAuthorization("CanAdjustStock").RequireRateLimiting("writes");

app.MapGet("/warehouses/{warehouseId:guid}/stock", async (
    Guid warehouseId,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(new GetStockLevelsQuery(warehouseId), cancellationToken);
    return Results.Ok(response);
}).RequireAuthorization().RequireRateLimiting("writes");

app.MapPost("/orders/{orderId:guid}/shipments", async (
    Guid orderId,
    ShipOrderRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var command = new ShipOrderCommand(
        orderId,
        request.ShipmentNumber,
        request.Lines,
        request.Carrier,
        request.TrackingNumber,
        request.IdempotencyKey);

    var response = await sender.Send(command, cancellationToken);
    return Results.Created($"/shipments/{response.ShipmentId}", response);
}).RequireAuthorization().RequireRateLimiting("writes");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

public sealed record ShipOrderRequest(
    string ShipmentNumber,
    IReadOnlyCollection<ShipOrderLineDto> Lines,
    string? Carrier,
    string? TrackingNumber,
    string IdempotencyKey);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
