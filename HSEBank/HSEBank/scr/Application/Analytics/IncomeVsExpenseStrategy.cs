using HSEBank.scr.Domain.Entities;

namespace HSEBank.scr.Application.Analytics;

// считает общую сумму доходов, общую сумму расходов и чистый баланс
public class IncomeVsExpenseStrategy : IAnalyticsStrategy
{
    public string Name => "Сравнение доходов и расходов";

    public AnalyticsResult Analyze(IEnumerable<Operation>? operations)
    {
        double income  = operations.Where(o => o.Type == "Income").Sum(o => o.Amount);
        double expense = operations.Where(o => o.Type == "Expense").Sum(o => o.Amount);
        double balance = income - expense;

        return new AnalyticsResult
        {
            Title = "Доходы и расходы",
            Data = new Dictionary<string, double>
            {
                { "Доходы",  income  },
                { "Расходы", expense },
                { "Баланс",  balance }
            },
            TotalOverride = balance
        };
    }
}