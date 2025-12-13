using FileAnalysisService.UseCases.RunAnalysis;

namespace FileAnalysisService.UseCases.GetReportsForWork;

internal sealed class GetReportsForWorkRequestHandler : IGetReportsForWorkRequestHandler
{
    private readonly IReportsRepository _reportsRepository;

    public GetReportsForWorkRequestHandler(IReportsRepository reportsRepository)
    {
        _reportsRepository = reportsRepository;
    }

    public async Task<GetReportsForWorkResponse> HandleAsync(
        GetReportsForWorkRequest request,
        CancellationToken cancellationToken = default)
    {
        var reports = await _reportsRepository.GetByWorkIdAsync(request.WorkId, cancellationToken);

        var items = reports
            .Select(r => r.ToItem())
            .ToArray();

        return new GetReportsForWorkResponse(items);
    }
}