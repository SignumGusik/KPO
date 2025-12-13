using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FileAnalysisService.Entities.Models;

namespace FileAnalysisService.UseCases.RunAnalysis;
/// <summary>
/// Обработчик запроса RunAnalysis - реализует сценарий анализа
/// </summary>
public sealed class RunAnalysisRequestHandler : IRunAnalysisRequestHandler
{
    private readonly IFileStorageClient _fileStorageClient;
    private readonly IReportsRepository _reportsRepository;
    private readonly TimeProvider _timeProvider;

    private const int MaxWordCloudUrlLength = 1024;
    private const string WordCloudBaseUrl = "https://quickchart.io/wordcloud?text=";

    /// <summary>
    /// Конструктор инжектирует зависимости: клиент FileStorage, репозиторий отчётов и поставщик времени.
    /// </summary>
    public RunAnalysisRequestHandler(
        IFileStorageClient fileStorageClient,
        IReportsRepository reportsRepository,
        TimeProvider timeProvider)
    {
        _fileStorageClient = fileStorageClient;
        _reportsRepository = reportsRepository;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Обрабатывает команду RunAnalysis: пытается получить метаданные и контент, строит отчёт и сохраняет в БД.
    /// В случае отсутствия метаданных/контента создаёт Failed-отчёт.
    /// </summary>
    public async Task<RunAnalysisResponse> HandleAsync(
        RunAnalysisCommand command,
        CancellationToken cancellationToken)
    {
        var meta = await _fileStorageClient
            .GetWorkMetadataAsync(command.WorkId, cancellationToken);

        if (meta is null)
        {
            var failed = await SaveFailedAsync(command.WorkId, "Failed:Meta", cancellationToken);
            return new RunAnalysisResponse(failed.Id, failed.Status);
        }

        var bytes = await _fileStorageClient
            .GetWorkContentAsync(command.WorkId, cancellationToken);

        if (bytes is null || bytes.Length == 0)
        {
            var failed = await SaveFailedAsync(command.WorkId, "Failed:Content", cancellationToken);
            return new RunAnalysisResponse(failed.Id, failed.Status);
        }
        
        var documentHash = ComputeSha256(bytes);
        
        var rawText = Encoding.UTF8.GetString(bytes);
        
        var safeText = SanitizeForWordCloud(rawText);
        
        var words = safeText.Split(
            new[] { ' ', '\r', '\n', '\t', ',', '.', '!', '?' },
            StringSplitOptions.RemoveEmptyEntries);
        
        var wordCloudUrl = BuildWordCloudUrl(words);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        
        var previousReports = await _reportsRepository
            .GetPreviousReportsForAssignmentAsync(meta.AssignmentId, meta.StudentId, cancellationToken);

        var similar = previousReports
            .Where(r =>
                r.DocumentHash is not null &&
                string.Equals(r.DocumentHash, documentHash, StringComparison.OrdinalIgnoreCase) &&
                r.StudentId != meta.StudentId &&
                r.WorkId != meta.Id &&
                r.CreatedAt <= now)
            .OrderBy(r => r.CreatedAt)
            .FirstOrDefault();

        var plagiarismFlag = similar is not null;
        Guid? similarWorkId = similar?.WorkId;
        var score = plagiarismFlag ? 0.0 : 100.0;
        var status = plagiarismFlag ? "PlagiarismDetected" : "Completed";

        var report = new Report(
            id: Guid.NewGuid(),
            workId: meta.Id,
            studentId: meta.StudentId,
            assignmentId: meta.AssignmentId,
            plagiarismFlag: plagiarismFlag,
            similarWorkId: similarWorkId,
            score: score,
            status: status,
            wordCloudUrl: wordCloudUrl,
            createdAt: now,
            documentHash: documentHash);

        var saved = await _reportsRepository.AddAsync(report, cancellationToken);

        return new RunAnalysisResponse(saved.Id, saved.Status);
    }

    /// <summary>
    /// Сохраняет неуспешный Report - используется, когда не удалось получить метаданные или контент.
    /// </summary>
    private async Task<Report> SaveFailedAsync(Guid workId, string status, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var report = new Report(
            id: Guid.NewGuid(),
            workId: workId,
            studentId: Guid.Empty,
            assignmentId: Guid.Empty,
            plagiarismFlag: false,
            similarWorkId: null,
            score: 0,
            status: status,
            wordCloudUrl: null,
            createdAt: now,
            documentHash: null);

        return await _reportsRepository.AddAsync(report, ct);
    }

    /// <summary>
    /// Вычисляет SHA-256 хэш массива байт и возвращает hex-строку
    /// </summary>
    private static string ComputeSha256(byte[] data)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(data);

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
    
    /// <summary>
    /// Убирает опасные управляющие символы, которые ломают XML в quickchart.
    /// </summary>
    private static string SanitizeForWordCloud(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            if (char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t')
                continue;

            sb.Append(ch);
        }

        return sb.ToString();
    }
    /// <summary>
    /// Безопасно строит URL для wordcloud, ограничивая длину входного текста
    /// Возвращает null, если нет слов.
    /// </summary>
    private static string? BuildWordCloudUrl(string[] words)
    {
        if (words.Length == 0)
            return null;

        var selectedWords = new List<string>();

        foreach (var word in words)
        {
            string candidateText;
            if (selectedWords.Count == 0)
            {
                candidateText = word;
            }
            else
            {
                candidateText = string.Join(" ", selectedWords) + " " + word;
            }

            var encoded = Uri.EscapeDataString(candidateText);
            var fullLength = WordCloudBaseUrl.Length + encoded.Length;

            if (fullLength > MaxWordCloudUrlLength)
            {
                break;
            }

            selectedWords.Add(word);
        }

        if (selectedWords.Count == 0)
            return null;

        var finalEncoded = Uri.EscapeDataString(string.Join(" ", selectedWords));
        return WordCloudBaseUrl + finalEncoded;
    }
}