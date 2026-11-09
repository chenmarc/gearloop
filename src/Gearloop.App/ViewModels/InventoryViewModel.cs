using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.Core.Models;
using Gearloop.Core.Profit;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>One line in the inventory list.</summary>
public sealed record InventoryRow(long Id, string Name, string Detail, string Amount, string AmountCaption);

public partial class InventoryViewModel(GearloopDatabase database) : ObservableObject
{
    private const string AllStatuses = "All";
    private List<Item> _items = [];

    public ObservableCollection<InventoryRow> Items { get; } = [];

    public IReadOnlyList<string> StatusFilters { get; } =
        [AllStatuses, nameof(ItemStatus.Owned), nameof(ItemStatus.Listed), nameof(ItemStatus.Sold)];

    [ObservableProperty] private string _statusFilter = AllStatuses;
    [ObservableProperty] private string _searchText = "";

    partial void OnStatusFilterChanged(string value) => ApplyFilter();
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    public async Task LoadAsync()
    {
        _items = await database.GetItemsAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = SearchText.Trim();
        var matches = _items.Where(i =>
            (StatusFilter == AllStatuses || i.Status.ToString() == StatusFilter)
            && (search.Length == 0
                || i.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (i.Brand?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (i.Model?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || Choices.Label(i.Category).Contains(search, StringComparison.OrdinalIgnoreCase)));

        Items.Clear();
        foreach (var item in matches)
            Items.Add(ToRow(item));
    }

    [RelayCommand]
    private Task LogBuyAsync() => Routes.GoToAsync(Routes.EditItem);

    public Task OpenAsync(InventoryRow row) => Routes.GoToAsync(Routes.Item, ("id", row.Id));

    private static InventoryRow ToRow(Item item)
    {
        var detail = $"{item.Status} · {Choices.Label(item.Category)} · bought {item.PurchaseDate:MMM d, yyyy}";
        return ProfitCalculator.Profit(item) is { } profit
            ? new InventoryRow(item.Id, item.Name, detail, Format.SignedMoney(profit), "profit")
            : new InventoryRow(item.Id, item.Name, detail, Format.Money(ProfitCalculator.TotalCost(item)), "cost");
    }
}
