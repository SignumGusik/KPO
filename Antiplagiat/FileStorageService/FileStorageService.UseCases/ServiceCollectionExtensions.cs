using FileStorageService.UseCases.Works.GetWork;
using FileStorageService.UseCases.Works.UploadWork;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorageService.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUploadWorkRequestHandler, UploadWorkRequestHandler>();
        services.AddScoped<IGetWorkRequestHandler, GetWorkRequestHandler>();

        return services;
    }
}