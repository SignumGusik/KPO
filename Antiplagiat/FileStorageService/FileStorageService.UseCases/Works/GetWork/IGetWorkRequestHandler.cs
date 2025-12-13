namespace FileStorageService.UseCases.Works.GetWork;

public interface IGetWorkRequestHandler
{
    GetWorkResponse? Handle(GetWorkRequest request);
}