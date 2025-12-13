namespace ApiGateway.Infrastructure.FileAnalysis;

public sealed class RunAnalysisResponseDto
{
    public Guid ReportId { get; init; }
    public string Status { get; init; } = string.Empty;
}