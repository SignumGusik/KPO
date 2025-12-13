using System.Net.Http;
using FileAnalysisService.UseCases.RunAnalysis;

namespace FileAnalysisService.Infrastructure.FileStorage;
/// <summary>
/// Адаптер HTTP/Refit клиента к FileStorageService. Прячет детали HTTP и возвращает чистые модели для use cases.
/// </summary>
public sealed class FileStorageClient : IFileStorageClient
{
    private readonly IFileStorageApi _api;

    /// <summary>
    /// Конструктор принимает Refit-интерфейс IFileStorageApi.
    /// </summary>
    public FileStorageClient(IFileStorageApi api)
    {
        _api = api;
    }

    /// <summary>
    /// Получает метаданные работы по id. Возвращает null при ошибке или отсутствии.
    /// </summary>
    public async Task<WorkMetadata?> GetWorkMetadataAsync(Guid workId, CancellationToken cancellationToken)
    {
        var response = await _api.GetMetadataAsync(workId);

        if (!response.IsSuccessStatusCode || response.Content is null)
            return null;

        var m = response.Content;
        return new WorkMetadata(m.Id, m.StudentId, m.AssignmentId);
    }

    /// <summary>
    /// Получает бинарный контент файла (HTTP GET /works/{id}/content).
    /// Возвращает null при ошибке.
    /// </summary>
    public async Task<byte[]?> GetWorkContentAsync(Guid workId, CancellationToken cancellationToken)
    {
        var response = await _api.GetContentAsync(workId);

        if (!response.IsSuccessStatusCode || response.Content is null)
            return null;
        
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}