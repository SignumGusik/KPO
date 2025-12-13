namespace FileStorageService.UseCases.Works.UploadWork;

public interface IUploadWorkRequestHandler
{
    UploadWorkResponse Handle(UploadWorkCommand command);
}