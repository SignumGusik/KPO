namespace HSEBank.scr.Reports;

public class ReportTitle : IReportComponent
{
    private readonly string _title;

    public ReportTitle(string title)
    {
        _title = title;
    }

    public void Render()
    {
        Console.WriteLine($"\n===== {_title} =====");
    }
}