namespace FileAnalysisService.Infrastructure.Data.Dtos;

public sealed class ReportDto
{
    public Guid Id { get; set; }

    public Guid WorkId { get; set; }

    public Guid StudentId { get; set; }

    public Guid AssignmentId { get; set; }

    public bool PlagiarismFlag { get; set; }

    public Guid? SimilarWorkId { get; set; }

    public double Score { get; set; }

    public string Status { get; set; } = null!;

    public string? WordCloudUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? DocumentHash { get; set; }   // <== важно
}