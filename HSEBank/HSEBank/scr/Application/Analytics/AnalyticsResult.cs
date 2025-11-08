namespace HSEBank.scr.Application.Analytics;

/// Результат аналитики:
/// - заголовок отчёта,
/// - табличные данные,
/// - общий итог (может быть рассчитан автоматически или задан вручную).
public class AnalyticsResult
{
    // Заголовок отчёта
    public string Title { get; set; } = "";
    
    // Основные данные отчёта: доходы -> расходы.
    public Dictionary<string, double> Data { get; set; } = new();
    
    /// Если задано, используется как итог вместо суммы всех значений Data.
    /// Нужно, чтобы не было двойного учёта
    public double? TotalOverride { get; set; }

    // Итоговое значение отчёта.
    public double Total => TotalOverride ?? Data.Values.Sum();

    public void Print()
    {
        Console.WriteLine($"\n{Title}");
        foreach (var kv in Data)
            Console.WriteLine($" - {kv.Key}: {kv.Value}");
        Console.WriteLine($"Итого: {Total}\n");
    }
}