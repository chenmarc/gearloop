#if DEBUG
using Gearloop.Core.Models;
using Gearloop.Core.Profit;
using Gearloop.Data;

namespace Gearloop.App;

/// <summary>Debug-only sample flips so the screens have something to show.</summary>
internal static class SampleData
{
    public static async Task AddAsync(GearloopDatabase database)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var bassSale = new Sale
        {
            Date = today.AddDays(-3),
            Channel = SaleChannel.Reverb,
            SalePrice = 1800m,
            ShippingCharged = 60m,
            ShippingCost = 75m,
        };
        bassSale.Fees = FeePresets.Reverb.Calculate(bassSale.SalePrice, bassSale.ShippingCharged);

        Item[] items =
        [
            new()
            {
                Name = "1978 Fender Precision Bass",
                Brand = "Fender",
                Model = "Precision Bass",
                Year = 1978,
                Category = Category.Bass,
                PurchaseDate = today.AddDays(-48),
                Source = PurchaseSource.Craigslist,
                PricePaid = 1200m,
                ShippingIn = 0m,
                Expenses = [new() { Date = today.AddDays(-40), Type = ItemExpenseType.Repair, Amount = 150m, Note = "Fret level" }],
                Sale = bassSale,
            },
            new()
            {
                Name = "Fender Blues Junior",
                Category = Category.Amp,
                PurchaseDate = today.AddDays(-14),
                Source = PurchaseSource.FacebookMarketplace,
                PricePaid = 300m,
                ListedPrice = 449m,
                ListedDate = today.AddDays(-5),
            },
            new()
            {
                Name = "Boss DS-1",
                Category = Category.Pedal,
                PurchaseDate = today.AddDays(-2),
                Source = PurchaseSource.GuitarCenter,
                PricePaid = 25m,
                SalesTax = 2.06m,
            },
        ];

        foreach (var item in items)
            await database.SaveItemAsync(item);

        await database.SaveGeneralExpenseAsync(new GeneralExpense
        {
            Date = today.AddDays(-3),
            Type = GeneralExpenseType.Supplies,
            Amount = 18.50m,
            Note = "Bass box and bubble wrap",
        });
    }
}
#endif
