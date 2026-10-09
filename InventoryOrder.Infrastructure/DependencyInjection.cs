using InventoryOrder.Application.Common.Interfaces;
using InventoryOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryOrder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:MySql is missing from configuration.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ITransactionManager, EfTransactionManager>();

        return services;
    }

    private sealed class EfTransactionManager(AppDbContext dbContext)
        : ITransactionManager
    {
        public async Task<TResponse> ExecuteAsync<TResponse>(
            Func<CancellationToken, Task<TResponse>> operation,
            int maxConcurrencyRetries = 0,
            CancellationToken cancellationToken = default)
        {
            for (var attempt = 0; ; attempt++)
            {
                await using var transaction = await dbContext.Database
                    .BeginTransactionAsync(cancellationToken);

                try
                {
                    var response = await operation(cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return response;
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxConcurrencyRetries)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
