using Gearloop.Core.Models;
using Gearloop.Core.Profit;

namespace Gearloop.Tests;

public class ProfitCalculatorTests
{
    [Fact]
    public void TotalCost_includes_price_tax_shipping_and_expenses()
    {
        var item = TestData.Bass();
        item.SalesTax = 10m;

        Assert.Equal(1405m, ProfitCalculator.TotalCost(item));
    }

    [Fact]
    public void NetProceeds_adds_shipping_charged_and_subtracts_shipping_cost_and_fees()
    {
        var sale = TestData.SoldBass().Sale!;

        Assert.Equal(1632.18m, ProfitCalculator.NetProceeds(sale));
    }

    [Fact]
    public void Profit_is_net_proceeds_minus_total_cost()
    {
        Assert.Equal(237.18m, ProfitCalculator.Profit(TestData.SoldBass()));
    }

    [Fact]
    public void Profit_can_be_negative()
    {
        var item = TestData.SoldBass();
        item.Sale!.SalePrice = 1000m;

        Assert.Equal(-562.82m, ProfitCalculator.Profit(item));
    }

    [Fact]
    public void Unsold_item_has_no_profit_roi_or_days_held()
    {
        var item = TestData.Bass();

        Assert.Null(ProfitCalculator.Profit(item));
        Assert.Null(ProfitCalculator.ReturnOnInvestment(item));
        Assert.Null(ProfitCalculator.DaysHeld(item));
    }

    [Fact]
    public void ReturnOnInvestment_is_profit_over_cost()
    {
        var roi = ProfitCalculator.ReturnOnInvestment(TestData.SoldBass())!.Value;

        Assert.Equal(0.1700m, Math.Round(roi, 4));
    }

    [Fact]
    public void ReturnOnInvestment_is_null_when_item_cost_nothing()
    {
        var item = new Item
        {
            Name = "Free amp from a neighbor",
            Sale = new Sale { SalePrice = 50m },
        };

        Assert.Equal(50m, ProfitCalculator.Profit(item));
        Assert.Null(ProfitCalculator.ReturnOnInvestment(item));
    }

    [Fact]
    public void DaysHeld_counts_days_from_purchase_to_sale()
    {
        Assert.Equal(45, ProfitCalculator.DaysHeld(TestData.SoldBass()));
    }

    [Fact]
    public void Status_follows_listing_and_sale()
    {
        var item = TestData.Bass();
        Assert.Equal(ItemStatus.Owned, item.Status);

        item.ListedDate = new DateOnly(2026, 3, 20);
        Assert.Equal(ItemStatus.Listed, item.Status);

        item.Sale = new Sale();
        Assert.Equal(ItemStatus.Sold, item.Status);
    }
}
