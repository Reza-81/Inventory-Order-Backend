using InventoryOrder.Application;
using InventoryOrder.Application.Features.Ping;
using InventoryOrder.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
