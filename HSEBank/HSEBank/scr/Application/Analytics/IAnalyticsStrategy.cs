using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;

// Общий интерфейс для всех стратегий аналитики.
public interface IAnalyticsStrategy
{
    string Name { get; }
    AnalyticsResult Analyze(IEnumerable<Operation>? operations);
}