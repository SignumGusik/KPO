using FileStorageService.Infrastructure.Data;
using FileStorageService.Infrastructure.Data.Works;
using FileStorageService.UseCases.Works.GetWork;
using FileStorageService.UseCases.Works.UploadWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorageService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<FileStorageDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUploadWorkRepository, EfWorksRepository>();
        services.AddScoped<IGetWorkRepository, EfWorksRepository>();

        return services;
    }
}