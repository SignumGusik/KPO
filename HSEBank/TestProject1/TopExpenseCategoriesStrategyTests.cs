using HSEBank.scr.Application.Analytics;
using HSEBank.scr.Domain.Entities;

namespace TestProject1
{
    public class TopExpenseCategoriesStrategyTests
    {
        [Fact]
        public void Analyze_TakesOnlyExpenseOperations_AndReturnsTop3BySum()
        {
            var catFood   = new Category { Id = Guid.NewGuid(), Name = "Еда",   Type = "Expense" };
            var catFun    = new Category { Id = Guid.NewGuid(), Name = "Развлечения", Type = "Expense" };
            var catBills  = new Category { Id = Guid.NewGuid(), Name = "Коммуналка",  Type = "Expense" };
            var catMisc   = new Category { Id = Guid.NewGuid(), Name = "Прочее", Type = "Expense" };
            var catIncome = new Category { Id = Guid.NewGuid(), Name = "Зарплата", Type = "Income" };

            var cats = new[] { catFood, catFun, catBills, catMisc, catIncome };

            Guid accId = Guid.NewGuid();

            var ops = new List<Operation>
            {
 
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catFood.Id, Type = "Expense", Amount = 120, Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catFood.Id, Type = "Expense", Amount = 80,  Date = DateTime.Today },

                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catFun.Id,  Type = "Expense", Amount = 50,  Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catFun.Id,  Type = "Expense", Amount = 100, Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catBills.Id,Type = "Expense", Amount = 90,  Date = DateTime.Today },

                // Прочее: 10 (должно НЕ попасть в топ-3)
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catMisc.Id, Type = "Expense", Amount = 10, Date = DateTime.Today },

                // Доход — должен игнорироваться
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = catIncome.Id, Type = "Income", Amount = 1000, Date = DateTime.Today },
            };

            var strategy = new TopExpenseCategoriesStrategy(cats);
            
            var result = strategy.Analyze(ops);

            Assert.Equal("Топ-3 расходов по категориям и типу", result.Title);
            Assert.DoesNotContain(result.Data.Keys, k => k.StartsWith("Зарплата"));
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(200, result.Data["Еда, Expense"]);
            Assert.Equal(150, result.Data["Развлечения, Expense"]);
            Assert.Equal(90,  result.Data["Коммуналка, Expense"]);
            Assert.DoesNotContain("Прочее, Expense", result.Data.Keys);
            Assert.Equal(200 + 150 + 90, result.Total);
        }

        [Fact]
        public void Analyze_WhenLessThan3ExpenseCategories_WorksCorrectly()
        {
            var cat1 = new Category { Id = Guid.NewGuid(), Name = "Кат1", Type = "Expense" };
            var cat2 = new Category { Id = Guid.NewGuid(), Name = "Кат2", Type = "Expense" };

            var cats = new[] { cat1, cat2 };

            Guid accId = Guid.NewGuid();

            var ops = new List<Operation>
            {
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = cat1.Id, Type = "Expense", Amount = 10, Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = cat2.Id, Type = "Expense", Amount = 20, Date = DateTime.Today },
            };

            var strategy = new TopExpenseCategoriesStrategy(cats);

            var result = strategy.Analyze(ops);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(30, result.Total);
        }

        [Fact]
        public void Analyze_UnknownCategory_GoesToUnknownKey()
        {
            var cat = new Category { Id = Guid.NewGuid(), Name = "Известная", Type = "Expense" };
            var cats = new[] { cat };

            Guid accId = Guid.NewGuid();

            var ops = new List<Operation>
            {
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = cat.Id, Type = "Expense", Amount = 10, Date = DateTime.Today },
                new Operation { Id = Guid.NewGuid(), AccountId = accId, CategoryId = Guid.NewGuid(), Type = "Expense", Amount = 5, Date = DateTime.Today },
            };

            var strategy = new TopExpenseCategoriesStrategy(cats);
            var result = strategy.Analyze(ops);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(10, result.Data["Известная, Expense"]);
            Assert.Equal(5, result.Data["Неизвестно"]);
        }
    }
}