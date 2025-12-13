namespace FileAnalysisService.UseCases.GetReportsForWork;

public interface IGetReportsForWorkRequestHandler
{
    Task<GetReportsForWorkResponse> HandleAsync(GetReportsForWorkRequest request, CancellationToken cancellationToken = default);
}