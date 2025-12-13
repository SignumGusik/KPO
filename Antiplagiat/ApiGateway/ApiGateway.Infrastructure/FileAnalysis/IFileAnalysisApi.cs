using System.Net.Http;
using Refit;

namespace ApiGateway.Infrastructure.FileAnalysis;

public interface IFileAnalysisApi
{
    /// <summary>
    /// POST /analysis — запускает анализ по workId.
    /// Возвращает ApiResponse с RunAnalysisResponseDto при успехе.
    /// </summary>
    [Post("/analysis")]
    Task<ApiResponse<RunAnalysisResponseDto>> RunAnalysisAsync(
        [Body] RunAnalysisRequestDto request
    );

    /// <summary>
    /// GET /works/{workId}/reports — проксирует получение отчётов как сырой HttpResponseMessage.
    /// ApiGateway читает содержимое и возвращает его клиенту.
    /// </summary>
    [Get("/works/{workId}/reports")]
    Task<HttpResponseMessage> GetReportsForWorkAsync(Guid workId);
}