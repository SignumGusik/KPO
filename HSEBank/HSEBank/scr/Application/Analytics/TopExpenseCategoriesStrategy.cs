using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;

// считает топ-3 категорий по расходам.
public class TopExpenseCategoriesStrategy : IAnalyticsStrategy
{
    private readonly IEnumerable<Category> _categories;
    public string Name => "Топ-3 категорий по расходам";

    public TopExpenseCategoriesStrategy(IEnumerable<Category> categories)
    {
        _categories = categories;
    }

    public AnalyticsResult Analyze(IEnumerable<Operation>? operations)
    {
        // идентификация по [Имя, Тип]
        var categoryNameTypeById = _categories.ToDictionary(
            c => c.Id,
            c => $"{c.Name}, {c.Type}"
        );
        
        // считаем сумму, сортируем по убыванию и берём первые 3.
        Dictionary<string, double> groups = operations
            .Where(o => o.Type == "Expense")
            .GroupBy(o => categoryNameTypeById.ContainsKey(o.CategoryId)
                ? categoryNameTypeById[o.CategoryId]
                : "Неизвестно"
            )
            .Select(g => new
            {
                Category = g.Key,
                Sum = g.Sum(o => o.Amount)
            })
            .OrderByDescending(x => x.Sum)
            .Take(3)
            .ToDictionary(x => x.Category, x => x.Sum);

        return new AnalyticsResult
        {
            Title = "Топ-3 расходов по категориям и типу",
            Data = groups,
            TotalOverride = groups.Values.Sum()
        };
    }
}