using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.App.Services;
using Gearloop.Core.Models;
using Gearloop.Core.Profit;
using Gearloop.Core.Text;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>A label and an amount, one line in a cost or sale breakdown.</summary>
public sealed record AmountRow(string Label, string Amount);

/// <summary>An expense line on the item detail screen.</summary>
public sealed record ExpenseRow(long Id, string Title, string Detail, string Amount);

/// <summary>Everything about one item: costs, listing, sale and profit.</summary>
public partial class ItemDetailViewModel(GearloopDatabase database) : ObservableObject, IQueryAttributable
{
    private long _itemId;
    private Item? _item;

    public ObservableCollection<PhotoRow> Photos { get; } = [];
    public ObservableCollection<AmountRow> CostRows { get; } = [];
    public ObservableCollection<ExpenseRow> Expenses { get; } = [];
    public ObservableCollection<AmountRow> SaleRows { get; } = [];

    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _status = "";
    [ObservableProperty] private string _totalCost = "";
    [ObservableProperty] private string _listing = "";
    [ObservableProperty] private bool _isListed;
    [ObservableProperty] private bool _isSold;
    [ObservableProperty] private string _profit = "";
    [ObservableProperty] private string _profitDetail = "";
    [ObservableProperty] private bool _hasPhotos;
    [ObservableProperty] private bool _hasExpenses;
    [ObservableProperty] private bool _hasNotes;
    [ObservableProperty] private string _notes = "";

    public bool IsNotSold => !IsSold;
    public bool CanList => !IsSold && !IsListed;

    partial void OnIsSoldChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotSold));
        OnPropertyChanged(nameof(CanList));
    }

    partial void OnIsListedChanged(bool value) => OnPropertyChanged(nameof(CanList));

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && id is long itemId)
        {
            _itemId = itemId;
            await LoadAsync();
        }
    }

    public async Task LoadAsync()
    {
        if (_itemId == 0) return;
        _item = await database.GetItemAsync(_itemId);
        if (_item is null) return;
        var item = _item;

        Name = item.Name;
        var details = new[] { item.Year?.ToString(), item.Brand, item.Model }.Where(s => !string.IsNullOrEmpty(s));
        Summary = string.Join(" · ", new[] { string.Join(" ", details), Choices.Label(item.Category) }.Where(s => s.Length > 0));
        Status = item.Status.ToString();
        Notes = item.Notes ?? "";
        HasNotes = item.Notes is not null;

        Photos.Clear();
        foreach (var path in item.PhotoPaths) Photos.Add(new PhotoRow(path));
        HasPhotos = Photos.Count > 0;

        CostRows.Clear();
        CostRows.Add(new AmountRow($"Paid · {Choices.Label(item.Source)} · {item.PurchaseDate:MMM d, yyyy}", Format.Money(item.PricePaid)));
        if (item.SalesTax != 0) CostRows.Add(new AmountRow("Sales tax", Format.Money(item.SalesTax)));
        if (item.ShippingIn != 0) CostRows.Add(new AmountRow("Shipping in", Format.Money(item.ShippingIn)));

        Expenses.Clear();
        foreach (var e in item.Expenses)
            Expenses.Add(new ExpenseRow(e.Id, Choices.Label(e.Type), $"{e.Date:MMM d}{(e.Note is null ? "" : " · " + e.Note)}", Format.Money(e.Amount)));
        HasExpenses = Expenses.Count > 0;

        TotalCost = Format.Money(ProfitCalculator.TotalCost(item));

        IsListed = item.Status == ItemStatus.Listed;
        Listing = item.ListedDate is { } listed
            ? $"Listed {listed:MMM d}{(item.ListedPrice is { } price ? " at " + Format.Money(price) : "")}"
            : "";

        IsSold = item.Sale is not null;
        SaleRows.Clear();
        if (item.Sale is { } sale)
        {
            SaleRows.Add(new AmountRow($"Sold · {Choices.Label(sale.Channel)} · {sale.Date:MMM d, yyyy}", Format.Money(sale.SalePrice)));
            if (sale.ShippingCharged != 0) SaleRows.Add(new AmountRow("Shipping charged", Format.Money(sale.ShippingCharged)));
            if (sale.ShippingCost != 0) SaleRows.Add(new AmountRow("Shipping paid", "−" + Format.Money(sale.ShippingCost)));
            if (sale.Fees != 0) SaleRows.Add(new AmountRow("Fees", "−" + Format.Money(sale.Fees)));
            SaleRows.Add(new AmountRow("Net proceeds", Format.Money(ProfitCalculator.NetProceeds(sale))));

            Profit = Format.SignedMoney(ProfitCalculator.Profit(item)!.Value);
            var roi = ProfitCalculator.ReturnOnInvestment(item);
            var days = ProfitCalculator.DaysHeld(item)!.Value;
            ProfitDetail = (roi is { } r ? $"{r:P0} return · " : "") + (days == 1 ? "held 1 day" : $"held {days} days");
        }
    }

    [RelayCommand]
    private Task EditAsync() => Routes.GoToAsync(Routes.EditItem, ("id", _itemId));

    [RelayCommand]
    private Task AddExpenseAsync() => Routes.GoToAsync(Routes.ItemExpense, ("itemId", _itemId));

    [RelayCommand]
    private Task EditExpenseAsync(ExpenseRow row) =>
        Routes.GoToAsync(Routes.ItemExpense, ("itemId", _itemId), ("expenseId", row.Id));

    [RelayCommand]
    private Task RecordSaleAsync() => Routes.GoToAsync(Routes.Sale, ("itemId", _itemId));

    [RelayCommand]
    private async Task MarkListedAsync()
    {
        if (_item is null) return;
        var text = await Dialogs.PromptAsync("Mark as listed", "Asking price (optional)");
        if (text is null) return;

        decimal? price = null;
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (!MoneyText.TryParse(text, out var parsed))
            {
                await Dialogs.ShowAsync("Asking price", "Enter an amount like 450, or leave it blank.");
                return;
            }
            price = parsed;
        }

        _item.ListedDate = DateOnly.FromDateTime(DateTime.Today);
        _item.ListedPrice = price;
        await database.SaveItemAsync(_item);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task UnlistAsync()
    {
        if (_item is null) return;
        _item.ListedDate = null;
        _item.ListedPrice = null;
        await database.SaveItemAsync(_item);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_item is null) return;
        if (!await Dialogs.ConfirmAsync("Delete item?", $"This removes {_item.Name} and everything logged for it.", "Delete"))
            return;

        await database.DeleteItemAsync(_item.Id);
        PhotoStore.Delete(_item.PhotoPaths);
        await Routes.BackAsync();
    }
}
