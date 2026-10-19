using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Gearloop.Core.Models;
using Gearloop.Core.Profit;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>One line in the inventory list.</summary>
public sealed record InventoryRow(long Id, string Name, string Detail, string Amount, string AmountCaption);

public partial class InventoryViewModel(GearloopDatabase database) : ObservableObject
{
    public ObservableCollection<InventoryRow> Items { get; } = [];

    public async Task LoadAsync()
    {
        var items = await database.GetItemsAsync();

        Items.Clear();
        foreach (var item in items)
            Items.Add(ToRow(item));
    }

    private static InventoryRow ToRow(Item item)
    {
        var detail = $"{item.Status} · {item.Category} · bought {item.PurchaseDate:MMM d, yyyy}";
        return ProfitCalculator.Profit(item) is { } profit
            ? new InventoryRow(item.Id, item.Name, detail, Format.SignedMoney(profit), "profit")
            : new InventoryRow(item.Id, item.Name, detail, Format.Money(ProfitCalculator.TotalCost(item)), "cost");
    }
}
