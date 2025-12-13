using FileAnalysisService.Infrastructure.Data;
using FileAnalysisService.Infrastructure.Data.Reports;
using FileAnalysisService.Infrastructure.FileStorage;
using FileAnalysisService.UseCases.RunAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace FileAnalysisService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // DbContext
        services.AddDbContext<FileAnalysisDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured");
            }

            options.UseNpgsql(connectionString);
        });

        // Refit-клиент к FileStorage (HTTP)
        services
            .AddRefitClient<IFileStorageApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseAddress = configuration["FileStorageApi:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseAddress))
                {
                    throw new InvalidOperationException("FileStorageApi:BaseUrl is not configured");
                }

                client.BaseAddress = new Uri(baseAddress);
            });

        // Реализация интерфейса из UseCases
        services.AddScoped<IFileStorageClient, FileStorageClient>();
        services.AddScoped<IReportsRepository, EfReportsRepository>();

        return services;
    }
}