using FileAnalysisService.Entities.Models;
using FileAnalysisService.UseCases.RunAnalysis;

namespace FileAnalysisService.UseCases.GetReportsForWork;

internal static class GetReportsForWorkMapper
{
    public static ReportItem ToItem(this Report report) =>
        new(
            report.Id,
            report.WorkId,
            report.PlagiarismFlag,
            report.SimilarWorkId,
            report.Score,
            report.Status,
            report.WordCloudUrl,
            report.CreatedAt
        );
}