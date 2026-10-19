using Gearloop.Core.Models;
using Gearloop.Core.Profit;

namespace Gearloop.Tests;

public class ProfitSummaryTests
{
    private static readonly DateOnly Today = new(2026, 4, 20);

    private static List<Item> Inventory()
    {
        var soldInApril = TestData.SoldBass(); // profit 237.18, held 45 days

        var soldInJanuary = new Item
        {
            Name = "Boss DS-1",
            Category = Category.Pedal,
            PurchaseDate = new DateOnly(2026, 1, 2),
            PricePaid = 20m,
            Sale = new Sale { Date = new DateOnly(2026, 1, 12), Channel = SaleChannel.FacebookMarketplace, SalePrice = 45m },
        }; // profit 25, held 10 days

        var listed = new Item
        {
            Name = "Fender Blues Junior",
            Category = Category.Amp,
            PurchaseDate = new DateOnly(2026, 4, 1),
            PricePaid = 300m,
            ShippingIn = 25m,
            ListedDate = new DateOnly(2026, 4, 5),
        };

        var owned = new Item
        {
            Name = "Yamaha FG-180",
            Category = Category.Guitar,
            PurchaseDate = new DateOnly(2026, 4, 18),
            PricePaid = 150m,
        };

        return [soldInApril, soldInJanuary, listed, owned];
    }

    private static List<GeneralExpense> Expenses() =>
    [
        new() { Date = new DateOnly(2026, 4, 2), Type = GeneralExpenseType.Gas, Amount = 18.50m },
        new() { Date = new DateOnly(2026, 2, 14), Type = GeneralExpenseType.Supplies, Amount = 40m },
    ];

    [Fact]
    public void Month_counts_only_sales_and_expenses_in_that_month()
    {
        var summary = ProfitSummary.Calculate(Inventory(), Expenses(), DateRange.Month(Today));

        Assert.Equal(1, summary.ItemsSold);
        Assert.Equal(237.18m, summary.ItemProfit);
        Assert.Equal(18.50m, summary.GeneralExpenses);
        Assert.Equal(218.68m, summary.RealizedProfit);
        Assert.Equal(45d, summary.AverageDaysHeld);
    }

    [Fact]
    public void Year_includes_every_sale_and_expense_this_year()
    {
        var summary = ProfitSummary.Calculate(Inventory(), Expenses(), DateRange.Year(Today));

        Assert.Equal(2, summary.ItemsSold);
        Assert.Equal(262.18m, summary.ItemProfit);
        Assert.Equal(58.50m, summary.GeneralExpenses);
        Assert.Equal(203.68m, summary.RealizedProfit);
        Assert.Equal(27.5d, summary.AverageDaysHeld);
    }

    [Fact]
    public void Inventory_figures_cover_all_unsold_items_regardless_of_period()
    {
        var summary = ProfitSummary.Calculate(Inventory(), Expenses(), DateRange.Month(new DateOnly(2025, 6, 1)));

        Assert.Equal(475m, summary.MoneyInInventory);
        Assert.Equal(1, summary.OwnedCount);
        Assert.Equal(1, summary.ListedCount);
        Assert.Equal(0, summary.ItemsSold);
        Assert.Null(summary.AverageDaysHeld);
    }

    [Fact]
    public void Empty_inventory_is_all_zeros()
    {
        var summary = ProfitSummary.Calculate([], [], DateRange.AllTime);

        Assert.Equal(0m, summary.RealizedProfit);
        Assert.Equal(0m, summary.MoneyInInventory);
        Assert.Equal(0, summary.ItemsSold);
    }

    [Fact]
    public void Month_range_handles_february_and_year_end()
    {
        Assert.Equal(new DateRange(new(2028, 2, 1), new(2028, 2, 29)), DateRange.Month(new(2028, 2, 10)));
        Assert.Equal(new DateRange(new(2026, 12, 1), new(2026, 12, 31)), DateRange.Month(new(2026, 12, 31)));
    }
}
