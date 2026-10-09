using InventoryOrder.Application;
using InventoryOrder.Application.Common.Exceptions;
using InventoryOrder.Application.Features.Inventory.AdjustStock;
using InventoryOrder.Application.Features.Orders.PlaceOrder;
using InventoryOrder.Application.Features.Orders.ShipOrder;
using InventoryOrder.Application.Features.Ping;
using InventoryOrder.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

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

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            ArgumentException or InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        await Results.Problem(
            statusCode: statusCode,
            title: statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception is DbUpdateConcurrencyException
                    ? "The resource was changed by another request."
                : exception?.Message,
            detail: statusCode == StatusCodes.Status500InternalServerError
                ? null
                : exception?.Message)
            .ExecuteAsync(context);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapHealthChecks("/health");
}

app.UseHttpsRedirection();

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
});

app.MapPost("/inventory/adjustments", async (
    AdjustStockCommand command,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var response = await sender.Send(command, cancellationToken);
    return Results.Ok(response);
});

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
});

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
