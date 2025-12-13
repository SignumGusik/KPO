using System.Net;
using System.Net.Http.Headers;
using ApiGateway.Infrastructure.FileAnalysis;
using ApiGateway.Infrastructure.FileStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Refit;

namespace ApiGateway.Presentation.Endpoints;

public static class WorksEndpoints
{
    public static void MapWorksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/works");

        group.MapPost("/", UploadAndAnalyzeWork)
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .DisableAntiforgery(); // ← ВОТ ЭТО ДОБАВИТЬ

        group.MapGet("/{workId:guid}/reports", GetReportsForWork)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    private static async Task<IResult> UploadAndAnalyzeWork(
        [FromServices] IFileStorageApi fileStorageApi,
        [FromServices] IFileAnalysisApi fileAnalysisApi,
        [FromForm] IFormFile file,
        [FromForm] Guid studentId,
        [FromForm] Guid assignmentId)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "file is required" });
        }
        
        UploadWorkResponse? uploadResult;

        try
        {
            await using var stream = file.OpenReadStream();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);

            var storageResponse = await fileStorageApi.UploadWorkAsync(
                streamPart,
                studentId,
                assignmentId
            );

            if (!storageResponse.IsSuccessStatusCode || storageResponse.Content is null)
            {
                var storageError = storageResponse.Error?.Content;

                return Results.Json(
                    new
                    {
                        error = "FileStorageService returned error",
                        storageStatusCode = (int)storageResponse.StatusCode,
                        storageError
                    },
                    statusCode: (int)storageResponse.StatusCode
                );
            }

            uploadResult = storageResponse.Content;
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(
                new
                {
                    error = "FileStorageService unavailable",
                    details = ex.Message
                },
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }

        var workId = uploadResult.WorkId;
        
        bool analysisStarted = false;
        string? analysisStatus = null;

        try
        {
            var analysisResponse = await fileAnalysisApi.RunAnalysisAsync(
                new RunAnalysisRequestDto { WorkId = workId });

            if (analysisResponse.IsSuccessStatusCode && analysisResponse.Content is not null)
            {
                analysisStarted = true;
                analysisStatus = analysisResponse.Content.Status;
            }
            else
            {
                analysisStarted = false;
                analysisStatus = null;
            }
        }
        catch (HttpRequestException)
        {
            analysisStarted = false;
            analysisStatus = null;
        }
        
        return Results.Ok(new
        {
            workId,
            analysisStarted,
            analysisStatus,
            message = analysisStarted
                ? "Work uploaded and analysis started"
                : "Work uploaded, but analysis is not started yet. Try again later."
        });
    }

    private static async Task<IResult> GetReportsForWork(
        [FromServices] IFileAnalysisApi fileAnalysisApi,
        Guid workId)
    {
        try
        {
            var response = await fileAnalysisApi.GetReportsForWorkAsync(workId);

            if (!response.IsSuccessStatusCode)
            {
                return Results.Json(
                    new
                    {
                        error = "FileAnalysisService returned error",
                        statusCode = (int)response.StatusCode
                    },
                    statusCode: (int)response.StatusCode
                );
            }

            var json = await response.Content.ReadAsStringAsync();
            return Results.Content(json, "application/json");
        }
        catch (HttpRequestException ex)
        {
            return Results.Json(
                new
                {
                    error = "FileAnalysisService unavailable",
                    details = ex.Message
                },
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }
    }
}