using FileStorageService.Entities.Models;
using FileStorageService.Infrastructure.Data.Dtos;

namespace FileStorageService.Infrastructure.Data;

internal static class DataMapper
{
    public static WorkDto ToDto(this Work work) =>
        new()
        {
            Id = work.Id,
            StudentId = work.StudentId,
            AssignmentId = work.AssignmentId,
            CreatedAt = work.CreatedAt,
            FilePath = work.FilePath,
            Hash = work.FileHash
        };

    public static Work ToEntity(this WorkDto dto) =>
        new(
            dto.Id,
            dto.StudentId,
            dto.AssignmentId,
            dto.CreatedAt,
            dto.FilePath,
            dto.Hash
        );
}