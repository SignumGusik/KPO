using FileStorageService.Entities.Models;

namespace FileStorageService.UseCases.Works.GetWork;

internal static class GetWorkMapper
{
    public static GetWorkResponse ToDto(this Work work) =>
        new(
            work.Id,
            work.StudentId,
            work.AssignmentId,
            work.CreatedAt,
            work.FilePath
        );
}