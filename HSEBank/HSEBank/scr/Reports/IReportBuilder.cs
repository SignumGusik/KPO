namespace HSEBank.scr.Reports;

public interface IReportBuilder
{
    void AddTitle(string title);
    void AddTable(Dictionary<string, double> data);
    void AddSummary(string label, double total);
    Report GetReport();
}