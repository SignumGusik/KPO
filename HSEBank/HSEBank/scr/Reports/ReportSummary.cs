namespace HSEBank.scr.Reports;

public class ReportSummary : IReportComponent
{
    private readonly double _total;
    private readonly string _label;

    public ReportSummary(string label, double total)
    {
        _total = total;
        _label = label;
    }

    public void Render()
    {
        Console.WriteLine($"\n{_label}: {_total}\n");
    }
}