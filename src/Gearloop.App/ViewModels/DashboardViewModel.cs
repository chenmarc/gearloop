using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.Core.Profit;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

public partial class DashboardViewModel(GearloopDatabase database) : ObservableObject
{
    [ObservableProperty] private string _monthProfit = Format.Money(0);
    [ObservableProperty] private string _yearProfit = Format.Money(0);
    [ObservableProperty] private string _allTimeProfit = Format.Money(0);
    [ObservableProperty] private string _moneyInInventory = Format.Money(0);
    [ObservableProperty] private string _monthDetail = "";
    [ObservableProperty] private string _inventoryDetail = "";

    public async Task LoadAsync()
    {
        var items = await database.GetItemsAsync();
        var expenses = await database.GetGeneralExpensesAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var month = ProfitSummary.Calculate(items, expenses, DateRange.Month(today));
        var year = ProfitSummary.Calculate(items, expenses, DateRange.Year(today));
        var allTime = ProfitSummary.Calculate(items, expenses, DateRange.AllTime);

        MonthProfit = Format.Money(month.RealizedProfit);
        YearProfit = Format.Money(year.RealizedProfit);
        AllTimeProfit = Format.Money(allTime.RealizedProfit);
        MoneyInInventory = Format.Money(allTime.MoneyInInventory);
        MonthDetail = month.ItemsSold == 1 ? "1 item sold" : $"{month.ItemsSold} items sold";
        InventoryDetail = $"{allTime.OwnedCount} owned · {allTime.ListedCount} listed";
    }

    [RelayCommand]
    private Task LogBuyAsync() => Routes.GoToAsync(Routes.EditItem);
}
