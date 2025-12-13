using FileAnalysisService.UseCases.GetReportsForWork;
using FileAnalysisService.UseCases.RunAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace FileAnalysisService.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IRunAnalysisRequestHandler, RunAnalysisRequestHandler>();
        services.AddScoped<IGetReportsForWorkRequestHandler, GetReportsForWorkRequestHandler>();

        return services;
    }
}