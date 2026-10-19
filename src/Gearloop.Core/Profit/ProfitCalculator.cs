using Gearloop.Core.Models;

namespace Gearloop.Core.Profit;

public static class ProfitCalculator
{
    /// <summary>Price paid plus sales tax, shipping in and every expense attached to the item.</summary>
    public static decimal TotalCost(Item item) =>
        item.PricePaid + item.SalesTax + item.ShippingIn + item.Expenses.Sum(e => e.Amount);

    /// <summary>What you kept from a sale: price plus shipping charged, minus shipping paid and fees.</summary>
    public static decimal NetProceeds(Sale sale) =>
        sale.SalePrice + sale.ShippingCharged - sale.ShippingCost - sale.Fees;

    /// <summary>Net proceeds minus total cost, or null while the item is unsold.</summary>
    public static decimal? Profit(Item item) =>
        item.Sale is { } sale ? NetProceeds(sale) - TotalCost(item) : null;

    /// <summary>Profit as a fraction of total cost (0.25 = 25%), or null if unsold or free.</summary>
    public static decimal? ReturnOnInvestment(Item item)
    {
        var cost = TotalCost(item);
        return Profit(item) is { } profit && cost != 0m ? profit / cost : null;
    }

    /// <summary>Days from purchase to sale, or null while the item is unsold.</summary>
    public static int? DaysHeld(Item item) =>
        item.Sale is { } sale ? sale.Date.DayNumber - item.PurchaseDate.DayNumber : null;
}
