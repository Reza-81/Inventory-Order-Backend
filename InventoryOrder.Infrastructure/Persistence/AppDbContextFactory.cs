using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace InventoryOrder.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(FindApiProjectDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:MySql was not found in the API configuration.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0)));

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string FindApiProjectDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
            return currentDirectory;

        var apiDirectory = Path.Combine(currentDirectory, "InventoryOrder.Api");
        if (File.Exists(Path.Combine(apiDirectory, "appsettings.json")))
            return apiDirectory;

        throw new FileNotFoundException(
            "Could not find InventoryOrder.Api/appsettings.json.");
    }
}
