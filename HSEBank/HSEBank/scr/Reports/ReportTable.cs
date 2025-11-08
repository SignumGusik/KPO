namespace HSEBank.scr.Reports;

public class ReportTable : IReportComponent
{
    private readonly Dictionary<string, double> _data;

    public ReportTable(Dictionary<string, double> data)
    {
        _data = data;
    }

    public void Render()
    {
        foreach (var kv in _data)
            Console.WriteLine($"{kv.Key.PadRight(20)} : {kv.Value}");
    }
}