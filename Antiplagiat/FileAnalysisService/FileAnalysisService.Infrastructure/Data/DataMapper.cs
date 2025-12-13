using FileAnalysisService.Entities.Models;
using FileAnalysisService.Infrastructure.Data.Dtos;

namespace FileAnalysisService.Infrastructure.Data;

internal static class DataMapper
{
    public static ReportDto ToDto(this Report report) =>
        new()
        {
            Id = report.Id,
            WorkId = report.WorkId,
            StudentId = report.StudentId,
            AssignmentId = report.AssignmentId,
            PlagiarismFlag = report.PlagiarismFlag,
            SimilarWorkId = report.SimilarWorkId,
            Score = report.Score,
            Status = report.Status,
            WordCloudUrl = report.WordCloudUrl,
            CreatedAt = report.CreatedAt,
            DocumentHash = report.DocumentHash
        };

    public static Report ToEntity(this ReportDto dto) =>
        new(
            dto.Id,
            dto.WorkId,
            dto.StudentId,
            dto.AssignmentId,
            dto.PlagiarismFlag,
            dto.SimilarWorkId,
            dto.Score,
            dto.Status,
            dto.WordCloudUrl,
            dto.CreatedAt,
            dto.DocumentHash
        );
}