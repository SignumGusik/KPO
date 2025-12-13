// Data/MigrationRunner.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileAnalysisService.Infrastructure.Data;
/// <summary>
/// Hosted service, который при старте приложения применяет миграции EF Core к базе данных.
/// </summary>
internal sealed class MigrationRunner(IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    /// <summary>
    /// При старте создаёт scope, получает FileAnalysisDbContext и вызывает MigrateAsync.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        using var scope = serviceScopeFactory.CreateScope();
        await using var dbContext =
            scope.ServiceProvider.GetRequiredService<FileAnalysisDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Ничего не делает при остановке.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}