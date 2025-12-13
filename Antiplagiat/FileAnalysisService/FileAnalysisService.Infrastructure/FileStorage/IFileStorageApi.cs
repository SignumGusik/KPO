using System.Net.Http;
using Refit;

namespace FileAnalysisService.Infrastructure.FileStorage;

/// <summary>
/// Refit-интерфейс для обращения к FileStorageService из FileAnalysisService.
/// Определяет два вызова:
/// - GET /works/{id} — метаданные работы
/// - GET /works/{id}/content — бинарный контент файла
/// </summary>
public interface IFileStorageApi
{
    /// <summary>GET /works/{id} — получить метаданные работы.</summary>
    [Get("/works/{id}")]
    Task<ApiResponse<FileStorageWorkMetadataDto>> GetMetadataAsync(Guid id);
    
    /// <summary>GET /works/{id}/content — получить бинарное содержимое файла.</summary>
    [Get("/works/{id}/content")]
    Task<HttpResponseMessage> GetContentAsync(Guid id);
}

/// <summary>
/// DTO, возвращаемый FileStorageService при GET /works/{id}.
/// </summary>
public sealed class FileStorageWorkMetadataDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid AssignmentId { get; set; }
}