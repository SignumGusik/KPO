namespace FileAnalysisService.Entities.Models;

public sealed class Report
{
    public Guid Id { get; private set; }
    public Guid WorkId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public bool PlagiarismFlag { get; private set; }
    public Guid? SimilarWorkId { get; private set; }
    public double Score { get; private set; }
    public string Status { get; private set; }
    public string? WordCloudUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Хэш содержимого файла (например, SHA-256 в hex).
    /// Используем для определения совпадения текста.
    /// </summary>
    public string? DocumentHash { get; private set; }

    public Report(
        Guid id,
        Guid workId,
        Guid studentId,
        Guid assignmentId,
        bool plagiarismFlag,
        Guid? similarWorkId,
        double score,
        string status,
        string? wordCloudUrl,
        DateTime createdAt,
        string? documentHash = null)
    {
        Id = id;
        WorkId = workId;
        StudentId = studentId;
        AssignmentId = assignmentId;
        PlagiarismFlag = plagiarismFlag;
        SimilarWorkId = similarWorkId;
        Score = score;
        Status = status;
        WordCloudUrl = wordCloudUrl;
        CreatedAt = createdAt;
        DocumentHash = documentHash;
    }
}