namespace HSEBank.scr.Reports;

public class ConsoleReportBuilder : IReportBuilder
{
    private Report _report = new();

    public void AddTitle(string title)
    {
        _report.AddComponent(new ReportTitle(title));
    }
    

    public void AddTable(Dictionary<string, double> data)
    {
        _report.AddComponent(new ReportTable(data));
    }

    public void AddSummary(string label, double total)
    {
        _report.AddComponent(new ReportSummary(label, total));
    }

    public Report GetReport()
    {
        var built = _report;
        _report = new Report(); // сброс для нового
        return built;
    }
}