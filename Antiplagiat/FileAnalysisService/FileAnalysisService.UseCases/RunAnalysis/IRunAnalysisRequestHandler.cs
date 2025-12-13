namespace FileAnalysisService.UseCases.RunAnalysis;

public interface IRunAnalysisRequestHandler
{
    Task<RunAnalysisResponse> HandleAsync(RunAnalysisCommand command, CancellationToken cancellationToken = default);
}