// Data/MigrationRunner.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileStorageService.Infrastructure.Data;
/// <summary>
/// Hosted service для FileStorageService, автоматически применяющий миграции при старте.
/// </summary>
internal sealed class MigrationRunner(IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        using var scope = serviceScopeFactory.CreateScope();
        await using var dbContext =
            scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}