using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;


/// Контекст стратегии аналитики.
/// Хранит текущую стратегию (IAnalyticsStrategy) и
/// делегирует ей выполнение анализа.
public class AnalyticsContext
{
    // Текущая выбранная стратегия аналитики.
    private IAnalyticsStrategy _strategy;
    

    // Создаём контекст с начальной стратегией.
    public AnalyticsContext(IAnalyticsStrategy strategy)
    {
        _strategy = strategy;
    }

    // Позволяет переключить стратегию в рантайме (паттерн Strategy).
    public void SetStrategy(IAnalyticsStrategy strategy)
    {
        _strategy = strategy;
        Console.WriteLine($"Установлена стратегия: {strategy.Name}");
    }

    // Выполняет анализ переданных операций с помощью текущей стратегии.
    public AnalyticsResult Execute(IEnumerable<Operation>? operations)
    {
        return _strategy.Analyze(operations);
    }
}