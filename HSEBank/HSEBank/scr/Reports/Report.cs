namespace HSEBank.scr.Reports;

public class Report : IReportComponent
{
    private readonly List<IReportComponent> _components = new();

    public void AddComponent(IReportComponent component)
    {
        _components.Add(component);
    }

    public void Render()
    {
        Console.WriteLine("\n================ ОТЧЁТ ================");
        foreach (var c in _components)
            c.Render();
        Console.WriteLine("=======================================");
    }
}