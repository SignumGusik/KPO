namespace FileStorageService.UseCases.Works.GetWork;

internal sealed class GetWorkRequestHandler : IGetWorkRequestHandler
{
    private readonly IGetWorkRepository _repository;

    public GetWorkRequestHandler(IGetWorkRepository repository)
    {
        _repository = repository;
    }

    public GetWorkResponse? Handle(GetWorkRequest request)
    {
        var work = _repository.GetById(request.Id);
        return work?.ToDto();
    }
}