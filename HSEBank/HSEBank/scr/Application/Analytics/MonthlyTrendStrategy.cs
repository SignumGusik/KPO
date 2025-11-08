using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;

/// агрегирует операции по месяцам в формате yyyy-MM
/// и считает для каждого месяца баланс
public class MonthlyTrendStrategy : IAnalyticsStrategy
{
    public string Name => "Динамика по месяцам";
    public AnalyticsResult Analyze(IEnumerable<Operation>? operations)
    {
        var groups = operations
            .GroupBy(o => o.Date.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.Type == "Income" ? o.Amount : -o.Amount)
            );

        return new AnalyticsResult
        {
            Title = "Баланс по месяцам",
            Data = groups,
            TotalOverride = groups.Values.Sum()
        };
    }
}