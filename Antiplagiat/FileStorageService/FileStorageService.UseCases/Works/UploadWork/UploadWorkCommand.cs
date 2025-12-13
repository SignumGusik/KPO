using System.IO;

namespace FileStorageService.UseCases.Works.UploadWork;

public sealed record UploadWorkCommand(
    Guid StudentId,
    Guid AssignmentId,
    string FileName,
    Stream Content
);