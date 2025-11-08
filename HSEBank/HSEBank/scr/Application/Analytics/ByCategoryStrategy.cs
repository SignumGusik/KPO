using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;

/// Стратегия аналитики
/// считает суммарный баланс по каждой категории с учётом типа
public class ByCategoryStrategy : IAnalyticsStrategy
{
    private readonly IEnumerable<Category>? _categories;
    public string Name => "Сумма по категориям и типу";
    
    public ByCategoryStrategy(IEnumerable<Category>? categories)
    {
        _categories = categories;
    }

    // Сопоставляем Id категории -> Имя, Тип
    public AnalyticsResult Analyze(IEnumerable<Operation>? operations)
    {
        var categoryNameTypeById = _categories.ToDictionary(
            c => c.Id,
            c => $"{c.Name}, {c.Type}"
        );

        var groups = operations
            .GroupBy(o => categoryNameTypeById.TryGetValue(o.CategoryId, out var catType)
                ? catType
                : "Неизвестная категория")
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.Type == "Income" ? o.Amount : -o.Amount)
            );

        return new AnalyticsResult
        {
            Title = "Аналитика по категориям и типу",
            Data = groups,
            // сумма всех категорий
            TotalOverride = groups.Values.Sum()
        };
    }
}