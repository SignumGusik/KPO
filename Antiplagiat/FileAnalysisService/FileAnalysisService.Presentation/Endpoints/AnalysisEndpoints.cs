using FileAnalysisService.UseCases.GetReportsForWork;
using FileAnalysisService.UseCases.RunAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileAnalysisService.Presentation.Endpoints;

/// <summary>
/// Эндпоинты FileAnalysisService:
/// - POST /analysis  — запуск анализа для workId
/// - GET  /works/{workId}/reports — получение списка отчётов по работе
/// </summary>
public static class AnalysisEndpoints
{
    /// <summary>
    /// Регистрирует эндпоинты анализа в приложении.
    /// </summary>
    public static WebApplication MapAnalysisEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/");

        MapRunAnalysis(group);
        MapGetReportsForWork(group);

        return app;
    }
    /// <summary>
    /// Регистрирует POST /analysis — принимает RunAnalysisRequest, вызывает handler и возвращает результат.
    /// </summary>
    private static void MapRunAnalysis(RouteGroupBuilder group)
    {
        group.MapPost("analysis",
                async (
                    RunAnalysisRequest request,
                    IRunAnalysisRequestHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RunAnalysisCommand(request.WorkId);
                    var response = await handler.HandleAsync(command, cancellationToken);
                    return Results.Ok(response);
                })
            .WithName("RunAnalysis")
            .WithSummary("Run analysis for a work")
            .WithDescription("Starts plagiarism and text analysis for the given work");
    }
    /// <summary>
    /// Регистрирует GET /works/{workId}/reports — возвращает коллекцию отчётов для указанного workId.
    /// </summary>
    private static void MapGetReportsForWork(RouteGroupBuilder group)
    {
        group.MapGet("works/{workId:guid}/reports",
                async (
                    Guid workId,
                    IGetReportsForWorkRequestHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var request = new GetReportsForWorkRequest(workId);
                    var response = await handler.HandleAsync(request, cancellationToken);
                    return Results.Ok(response);
                })
            .WithName("GetReportsForWork")
            .WithSummary("Get analysis reports for a work")
            .WithDescription("Returns all plagiarism analysis reports for a given work");
        
    }
}
/// <summary>
/// DTO для POST /analysis
/// </summary>
public sealed record RunAnalysisRequest(Guid WorkId);