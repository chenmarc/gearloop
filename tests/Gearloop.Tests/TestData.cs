using Gearloop.Core.Models;

namespace Gearloop.Tests;

internal static class TestData
{
    /// <summary>A bass bought for $1,200 with $45 shipping and a $150 repair; unsold.</summary>
    public static Item Bass() => new()
    {
        Name = "1978 Fender Precision Bass",
        Brand = "Fender",
        Model = "Precision Bass",
        Year = 1978,
        Category = Category.Bass,
        PurchaseDate = new DateOnly(2026, 3, 1),
        Source = PurchaseSource.Craigslist,
        PricePaid = 1200m,
        ShippingIn = 45m,
        Expenses =
        [
            new ItemExpense { Date = new DateOnly(2026, 3, 10), Type = ItemExpenseType.Repair, Amount = 150m, Note = "Fret level" },
        ],
    };

    /// <summary>The bass sold on Reverb for $1,800 plus $60 shipping charged, $75 shipping paid, $152.82 fees.</summary>
    public static Item SoldBass()
    {
        var item = Bass();
        item.Sale = new Sale
        {
            Date = new DateOnly(2026, 4, 15),
            Channel = SaleChannel.Reverb,
            SalePrice = 1800m,
            ShippingCharged = 60m,
            ShippingCost = 75m,
            Fees = 152.82m,
        };
        return item;
    }
}
