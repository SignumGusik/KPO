using Refit;

namespace ApiGateway.Infrastructure.FileStorage;

public interface IFileStorageApi
{
    [Multipart]
    [Post("/works")]
    Task<ApiResponse<UploadWorkResponse>> UploadWorkAsync(
        [AliasAs("file")] StreamPart file,
        [AliasAs("studentId")] Guid studentId,
        [AliasAs("assignmentId")] Guid assignmentId
    );
}