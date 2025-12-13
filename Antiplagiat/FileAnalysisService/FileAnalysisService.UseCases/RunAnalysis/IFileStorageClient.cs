namespace FileAnalysisService.UseCases.RunAnalysis;

public interface IFileStorageClient
{
    Task<WorkMetadata?> GetWorkMetadataAsync(Guid workId, CancellationToken cancellationToken);
    Task<byte[]?> GetWorkContentAsync(Guid workId, CancellationToken cancellationToken);
}

// модель, с которой работает use case
public sealed record WorkMetadata(
    Guid Id,
    Guid StudentId,
    Guid AssignmentId
);