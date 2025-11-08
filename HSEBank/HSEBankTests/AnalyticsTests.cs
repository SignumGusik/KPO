using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HSEBank.scr.Application.Analytics;
using HSEBank.scr.Domain.Entities;
using Xunit;

namespace HSEBank.Tests.Analytics;

public class IncomeVsExpenseStrategyTests
{
    [Fact]
    public void Computes_income_expense_balance_and_totaloverride_is_balance()
    {
        var ops = new[]
        {
            new Operation{ Type="Income",  Amount=100, Date=new DateTime(2025,11,1)},
            new Operation{ Type="Expense", Amount=30,  Date=new DateTime(2025,11,2)},
        };
        var s = new IncomeVsExpenseStrategy();
        var r = s.Analyze(ops);

        r.Data["Доходы"].Should().Be(100);
        r.Data["Расходы"].Should().Be(30);
        r.Data["Баланс"].Should().Be(70);
        r.Total.Should().Be(70); // TotalOverride
    }
}

public class ByCategoryStrategyTests
{
    [Fact]
    public void Sums_by_category_with_sign_and_total_is_sum()
    {
        var food = new Category{ Id=Guid.NewGuid(), Name="Food", Type="Expense"};
        var sal  = new Category{ Id=Guid.NewGuid(), Name="Salary", Type="Income"};
        var cats = new[]{ food, sal };

        var ops = new[]
        {
            new Operation{ CategoryId=sal.Id,  Type="Income",  Amount=100 },
            new Operation{ CategoryId=food.Id, Type="Expense", Amount=40  },
        };

        var s = new ByCategoryStrategy(cats);
        var r = s.Analyze(ops);

        r.Data.Should().ContainKey("Salary, Income").WhoseValue.Should().Be(100);
        r.Data.Should().ContainKey("Food, Expense").WhoseValue.Should().Be(-40);
        r.Total.Should().Be(60);
    }
}

public class MonthlyTrendStrategyTests
{
    [Fact]
    public void Groups_by_yyyy_MM_and_applies_sign()
    {
        var ops = new[]
        {
            new Operation{ Type="Income",  Amount=100, Date=new DateTime(2025,10,1)},
            new Operation{ Type="Expense", Amount=30,  Date=new DateTime(2025,10,2)},
            new Operation{ Type="Income",  Amount=50,  Date=new DateTime(2025,11,1)},
        };
        var s = new MonthlyTrendStrategy();
        var r = s.Analyze(ops);

        r.Data.Should().Contain(new KeyValuePair<string,double>("2025-10", 70));
        r.Data.Should().Contain(new KeyValuePair<string,double>("2025-11", 50));
        r.Total.Should().Be(120);
    }
}

public class AnalyticsResultTests
{
    [Fact]
    public void Total_uses_override_when_set()
    {
        var r = new AnalyticsResult
        {
            Data = new Dictionary<string,double>{{"A",10},{"B",10}},
            TotalOverride = 1
        };
        r.Total.Should().Be(1);
    }
}