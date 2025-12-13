namespace FileAnalysisService.UseCases.RunAnalysis;

public sealed record RunAnalysisResponse(Guid ReportId, string Status);