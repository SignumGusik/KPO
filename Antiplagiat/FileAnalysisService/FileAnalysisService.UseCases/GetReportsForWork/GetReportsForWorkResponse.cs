namespace FileAnalysisService.UseCases.GetReportsForWork;

public sealed record GetReportsForWorkResponse(IReadOnlyList<ReportItem> Reports);