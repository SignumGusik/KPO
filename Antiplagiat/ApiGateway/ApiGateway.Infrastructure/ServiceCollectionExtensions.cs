using ApiGateway.Infrastructure.FileStorage;
using ApiGateway.Infrastructure.FileAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace ApiGateway.Infrastructure;
/// <summary>
/// Регистрация зависимостей инфраструктуры FileStorageService:
/// - DbContext с чтением connection string из IConfiguration,
/// - реализация репозиториев.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует инфраструктурные сервисы: DbContext и репозитории.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        var cfg = sp.GetRequiredService<IConfiguration>();
        
        services
            .AddRefitClient<IFileStorageApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseAddress = configuration["FileStorageApi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseAddress))
                    throw new InvalidOperationException("FileStorageApi:BaseUrl is not configured");

                client.BaseAddress = new Uri(baseAddress);
            });
        
        services
            .AddRefitClient<IFileAnalysisApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseAddress = configuration["FileAnalysisApi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseAddress))
                    throw new InvalidOperationException("FileAnalysisApi:BaseUrl is not configured");

                client.BaseAddress = new Uri(baseAddress);
            });

        return services;
    }
}