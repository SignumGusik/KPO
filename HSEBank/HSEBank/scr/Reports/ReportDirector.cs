using HSEBank.scr.Application.Analytics;

namespace HSEBank.scr.Reports;

public class ReportDirector
{
    private readonly IReportBuilder _builder;

    public ReportDirector(IReportBuilder builder)
    {
        _builder = builder;
    }

    public Report BuildAnalyticsReport(AnalyticsResult analytics)
    {
        _builder.AddTitle(analytics.Title);
        _builder.AddTable(analytics.Data);
        _builder.AddSummary("Общий итог", analytics.Total);
        return _builder.GetReport();
    }
}