namespace FileAnalysisService.UseCases.GetReportsForWork;

public sealed record ReportItem(
    Guid Id,
    Guid WorkId,
    bool PlagiarismFlag,
    Guid? SimilarWorkId,
    double Score,
    string Status,
    string? WordCloudUrl,
    DateTime CreatedAt
);