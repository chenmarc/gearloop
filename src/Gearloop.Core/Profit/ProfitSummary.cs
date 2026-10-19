using Gearloop.Core.Models;

namespace Gearloop.Core.Profit;

/// <summary>Running totals for a period, as shown on the dashboard.</summary>
public sealed record ProfitSummary(
    DateRange Period,
    int ItemsSold,
    decimal ItemProfit,
    decimal GeneralExpenses,
    decimal MoneyInInventory,
    int OwnedCount,
    int ListedCount,
    double? AverageDaysHeld)
{
    /// <summary>Profit on items sold in the period, minus general expenses dated in the period.</summary>
    public decimal RealizedProfit => ItemProfit - GeneralExpenses;

    /// <summary>
    /// Sales and general expenses are filtered to <paramref name="period"/>. Inventory figures
    /// (money in inventory, owned and listed counts) always describe what you hold right now.
    /// </summary>
    public static ProfitSummary Calculate(
        IEnumerable<Item> items,
        IEnumerable<GeneralExpense> generalExpenses,
        DateRange period)
    {
        var all = items.ToList();
        var sold = all.Where(i => i.Sale is { } s && period.Contains(s.Date)).ToList();
        var unsold = all.Where(i => i.Sale is null).ToList();

        return new ProfitSummary(
            Period: period,
            ItemsSold: sold.Count,
            ItemProfit: sold.Sum(i => ProfitCalculator.Profit(i)!.Value),
            GeneralExpenses: generalExpenses.Where(e => period.Contains(e.Date)).Sum(e => e.Amount),
            MoneyInInventory: unsold.Sum(ProfitCalculator.TotalCost),
            OwnedCount: unsold.Count(i => i.Status == ItemStatus.Owned),
            ListedCount: unsold.Count(i => i.Status == ItemStatus.Listed),
            AverageDaysHeld: sold.Count == 0 ? null : sold.Average(i => ProfitCalculator.DaysHeld(i)!.Value));
    }
}
