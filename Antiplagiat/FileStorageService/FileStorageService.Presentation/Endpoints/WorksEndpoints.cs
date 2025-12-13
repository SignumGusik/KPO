using FileStorageService.UseCases.Works.GetWork;
using FileStorageService.UseCases.Works.UploadWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileStorageService.Presentation.Endpoints;

/// <summary>
/// Набор HTTP-эндпоинтов для работы с работами студентов (/works).
/// Экстеншн для WebApplication, регистрирует маршруты загрузки, получения метаданных и скачивания содержимого.
/// </summary>
public static class WorksEndpoints
{
    /// <summary>
    /// Регистрирует группу эндпоинтов /works в приложении.
    /// </summary>
    public static WebApplication MapWorksEndpoints(this WebApplication app)
    {
        app.MapGroup("/works")
            .WithTags("Works")
            .MapUploadWork()
            .MapGetWork()
            .MapGetWorkContent();

        return app;
    }

    /// <summary>
    /// Модель формы при загрузке работы.
    /// </summary>
    public sealed class UploadWorkForm
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = null!;

        [FromForm(Name = "studentId")]
        public string StudentId { get; set; } = string.Empty;

        [FromForm(Name = "assignmentId")]
        public string AssignmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Регистрирует POST /works - загрузка файла.
    /// Обрабатывает multipart/form-data, валидирует параметры и вызывает use case загрузки.
    /// </summary>
    private static RouteGroupBuilder MapUploadWork(this RouteGroupBuilder group)
    {
        group.MapPost("",
                async ([FromForm] UploadWorkForm form,
                    IUploadWorkRequestHandler handler) =>
                {
                    if (form.File is null || form.File.Length == 0)
                    {
                        return Results.BadRequest("file is required");
                    }

                    if (string.IsNullOrWhiteSpace(form.StudentId))
                    {
                        return Results.BadRequest("studentId is required");
                    }

                    if (string.IsNullOrWhiteSpace(form.AssignmentId))
                    {
                        return Results.BadRequest("assignmentId is required");
                    }
                    if (!Guid.TryParse(form.StudentId.Trim('"'), out var studentId))
                    {
                        return Results.BadRequest($"Invalid studentId: {form.StudentId}");
                    }

                    if (!Guid.TryParse(form.AssignmentId.Trim('"'), out var assignmentId))
                    {
                        return Results.BadRequest($"Invalid assignmentId: {form.AssignmentId}");
                    }

                    await using var stream = form.File.OpenReadStream();

                    var command = new UploadWorkCommand(
                        StudentId: studentId,
                        AssignmentId: assignmentId,
                        FileName: form.File.FileName,
                        Content: stream
                    );

                    var response = handler.Handle(command);

                    return Results.Ok(response);
                })
            .DisableAntiforgery()
            .Accepts<UploadWorkForm>("multipart/form-data")
            .WithName("UploadWork")
            .WithSummary("Upload a work file")
            .WithDescription("Uploads a student work and stores it in the file storage service");

        return group;
    }

    /// <summary>
    // /// Регистрирует GET /works/{id} - получение метаданных работы.
    // /// Возвращает 404 если запись не найдена.
    // /// </summary>
    private static RouteGroupBuilder MapGetWork(this RouteGroupBuilder group)
    {
        group.MapGet("{id:guid}",
                (Guid id, IGetWorkRequestHandler handler) =>
                {
                    var response = handler.Handle(new GetWorkRequest(id));
                    return response is null
                        ? Results.NotFound()
                        : Results.Ok(response);
                })
            .WithName("GetWork")
            .WithSummary("Get work metadata")
            .WithDescription("Returns metadata about a stored work");

        return group;
    }

    /// <summary>
    // /// Регистрирует GET /works/{id}/content — отдаёт бинарное содержимое файла.
    // /// </summary>
    private static RouteGroupBuilder MapGetWorkContent(this RouteGroupBuilder group)
    {
        group.MapGet("{id:guid}/content",
                (Guid id, IGetWorkRequestHandler handler) =>
                {
                    var response = handler.Handle(new GetWorkRequest(id));
                    if (response is null)
                    {
                        return Results.NotFound();
                    }

                    if (!File.Exists(response.FilePath))
                    {
                        return Results.NotFound("File not found on disk");
                    }

                    var contentType = "application/octet-stream";
                    var fileName = Path.GetFileName(response.FilePath);
                    var fileStream = File.OpenRead(response.FilePath);

                    return Results.File(
                        fileStream,
                        contentType: contentType,
                        fileDownloadName: fileName
                    );
                })
            .WithName("GetWorkContent")
            .WithSummary("Download work file")
            .WithDescription("Returns the binary content of a stored work");

        return group;
    }
}