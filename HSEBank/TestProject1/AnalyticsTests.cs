using HSEBank.scr.Application.Analytics;
using HSEBank.scr.Domain.Entities;

namespace TestProject1
{
    public class AnalyticsTests
    {
        private static List<Operation> MakeOps()
        {
            var acc = Guid.NewGuid();
            var catIncome = Guid.NewGuid();
            var catExpense = Guid.NewGuid();

            return new List<Operation>
            {
                new Operation { Id = Guid.NewGuid(), AccountId = acc, CategoryId = catIncome, Type = "Income", Amount = 200, Date = new DateTime(2025, 1, 10) },
                new Operation { Id = Guid.NewGuid(), AccountId = acc, CategoryId = catExpense, Type = "Expense", Amount = 50, Date = new DateTime(2025, 1, 11) },
                new Operation { Id = Guid.NewGuid(), AccountId = acc, CategoryId = catIncome, Type = "Income", Amount = 100, Date = new DateTime(2025, 2, 5) },
            };
        }

        [Fact]
        public void IncomeVsExpenseStrategy_ComputesCorrectly()
        {
            var ops = MakeOps();
            var strategy = new IncomeVsExpenseStrategy();

            var result = strategy.Analyze(ops);

            Assert.Equal("Доходы и расходы", result.Title);
            Assert.Equal(300, result.Data["Доходы"]);
            Assert.Equal(50, result.Data["Расходы"]);
            Assert.Equal(250, result.Data["Баланс"]);
            Assert.Equal(250, result.Total);
        }

        [Fact]
        public void MonthlyTrendStrategy_GroupsByMonth()
        {
            var ops = MakeOps();
            var strategy = new MonthlyTrendStrategy();

            var result = strategy.Analyze(ops);

            Assert.Equal("Баланс по месяцам", result.Title);
            Assert.Equal(150, result.Data["2025-01"]);
            Assert.Equal(100, result.Data["2025-02"]);
            Assert.Equal(250, result.Total);
        }

        [Fact]
        public void ByCategoryStrategy_UsesNameAndType()
        {
            var catIncome = new Category { Id = Guid.NewGuid(), Name = "Зарплата", Type = "Income" };
            var catExpense = new Category { Id = Guid.NewGuid(), Name = "Еда", Type = "Expense" };

            var ops = new List<Operation>
            {
                new Operation { Id = Guid.NewGuid(), CategoryId = catIncome.Id, Type = "Income", Amount = 200, Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), CategoryId = catExpense.Id, Type = "Expense", Amount = 50, Date = DateTime.Today },
            };

            var strategy = new ByCategoryStrategy(new[] { catIncome, catExpense });

            var result = strategy.Analyze(ops);

            Assert.Equal("Аналитика по категориям и типу", result.Title);
            Assert.Equal(200, result.Data["Зарплата, Income"]);
            Assert.Equal(-50, result.Data["Еда, Expense"]);
            Assert.Equal(150, result.Total);
        }

        [Fact]
        public void AnalyticsContext_SwitchStrategy()
        {
            var ops = MakeOps();
            var ctx = new AnalyticsContext(new IncomeVsExpenseStrategy());

            var r1 = ctx.Execute(ops);
            Assert.Equal(250, r1.Total);

            ctx.SetStrategy(new MonthlyTrendStrategy());
            var r2 = ctx.Execute(ops);
            Assert.Equal(250, r2.Total);
        }
    }
}