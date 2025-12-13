namespace FileStorageService.UseCases.Works.GetWork;

public sealed record GetWorkResponse(
    Guid Id,
    Guid StudentId,
    Guid AssignmentId,
    DateTime CreatedAt,
    string FilePath
);