using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.App.Services;
using Gearloop.Core.Models;
using Gearloop.Core.Profit;
using Gearloop.Core.Text;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>
/// Records or edits an item's sale. Fees follow the channel's preset as the price and shipping
/// change, until the user types their own fee amount.
/// </summary>
public partial class SaleViewModel(GearloopDatabase database) : ObservableObject, IQueryAttributable
{
    private Item? _item;
    private bool _feesTypedByUser;
    private bool _settingFees;

    public IReadOnlyList<Choice<SaleChannel>> Channels => Choices.Channels;

    [ObservableProperty] private string _title = "Record sale";
    [ObservableProperty] private string _itemName = "";
    [ObservableProperty] private DateTime? _saleDate = DateTime.Today;
    [ObservableProperty] private Choice<SaleChannel>? _channel;
    [ObservableProperty] private string _salePrice = "";
    [ObservableProperty] private string _shippingCharged = "";
    [ObservableProperty] private string _shippingCost = "";
    [ObservableProperty] private string _fees = "";
    [ObservableProperty] private string _feesHint = "";
    [ObservableProperty] private string _netProceeds = "";
    [ObservableProperty] private string _profit = "";
    [ObservableProperty] private bool _isExistingSale;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("itemId", out var id) || id is not long itemId) return;
        _item = await database.GetItemAsync(itemId);
        if (_item is null) return;

        ItemName = _item.Name;
        if (_item.Sale is { } sale)
        {
            Title = "Edit sale";
            IsExistingSale = true;
            _feesTypedByUser = true;
            SaleDate = sale.Date.ToDateTime(TimeOnly.MinValue);
            Channel = Choices.Find(Channels, sale.Channel);
            SalePrice = MoneyText.ToInput(sale.SalePrice);
            ShippingCharged = MoneyText.ToOptionalInput(sale.ShippingCharged);
            ShippingCost = MoneyText.ToOptionalInput(sale.ShippingCost);
            SetFees(MoneyText.ToOptionalInput(sale.Fees));
        }
        else
        {
            Channel = Choices.Find(Channels, SaleChannel.Reverb);
            if (_item.ListedPrice is { } asking) SalePrice = MoneyText.ToInput(asking);
        }
        Recalculate();
    }

    partial void OnChannelChanged(Choice<SaleChannel>? value) => Recalculate();
    partial void OnSalePriceChanged(string value) => Recalculate();
    partial void OnShippingChargedChanged(string value) => Recalculate();
    partial void OnShippingCostChanged(string value) => Recalculate();

    partial void OnFeesChanged(string value)
    {
        if (!_settingFees) _feesTypedByUser = true;
        Recalculate();
    }

    [RelayCommand]
    private void UsePresetFees()
    {
        _feesTypedByUser = false;
        Recalculate();
    }

    private void SetFees(string text)
    {
        _settingFees = true;
        Fees = text;
        _settingFees = false;
    }

    private void Recalculate()
    {
        if (_item is null || Channel is null) return;

        MoneyText.TryParseOptional(SalePrice, out var price);
        MoneyText.TryParseOptional(ShippingCharged, out var charged);
        MoneyText.TryParseOptional(ShippingCost, out var cost);

        var preset = FeePresets.For(Channel.Value);
        if (!_feesTypedByUser)
            SetFees(MoneyText.ToOptionalInput(preset.Calculate(price, charged)));

        FeesHint = preset.Components.Count == 0
            ? "No fees for this channel by default."
            : _feesTypedByUser
                ? "Using your amount."
                : $"Estimated from {Choices.Label(Channel.Value)}'s standard fees. Change it to match your payout.";

        MoneyText.TryParseOptional(Fees, out var fees);
        var sale = new Sale { SalePrice = price, ShippingCharged = charged, ShippingCost = cost, Fees = fees };
        var net = ProfitCalculator.NetProceeds(sale);
        NetProceeds = Format.Money(net);
        Profit = Format.SignedMoney(net - ProfitCalculator.TotalCost(_item));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_item is null || Channel is null) return;

        var problems = new List<string>();
        if (!MoneyText.TryParse(SalePrice, out var price)) problems.Add("Enter the sale price, like 650.");
        if (!MoneyText.TryParseOptional(ShippingCharged, out var charged)) problems.Add("Shipping charged should be an amount, or left blank.");
        if (!MoneyText.TryParseOptional(ShippingCost, out var cost)) problems.Add("Shipping paid should be an amount, or left blank.");
        if (!MoneyText.TryParseOptional(Fees, out var fees)) problems.Add("Fees should be an amount, or left blank.");
        var date = DateOnly.FromDateTime(SaleDate ?? DateTime.Today);
        if (date < _item.PurchaseDate) problems.Add("The sale date is before the purchase date.");

        if (problems.Count > 0)
        {
            await Dialogs.ShowAsync("Almost there", string.Join("\n", problems));
            return;
        }

        _item.Sale = new Sale
        {
            Date = date,
            Channel = Channel.Value,
            SalePrice = price,
            ShippingCharged = charged,
            ShippingCost = cost,
            Fees = fees,
        };
        await database.SaveItemAsync(_item);
        await Routes.BackAsync();
    }

    [RelayCommand]
    private async Task RemoveSaleAsync()
    {
        if (_item is null) return;
        if (!await Dialogs.ConfirmAsync("Remove sale?", "The item goes back to your inventory.", "Remove"))
            return;

        _item.Sale = null;
        await database.SaveItemAsync(_item);
        await Routes.BackAsync();
    }
}
