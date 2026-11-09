using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.Core.Profit;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>A general expense line in the Expenses tab.</summary>
public sealed record GeneralExpenseRow(long Id, string Title, string Detail, string Amount);

/// <summary>Business costs not tied to one item, such as gas or packing supplies.</summary>
public partial class ExpensesViewModel(GearloopDatabase database) : ObservableObject
{
    public ObservableCollection<GeneralExpenseRow> Expenses { get; } = [];

    [ObservableProperty] private string _monthTotal = Format.Money(0);
    [ObservableProperty] private string _yearTotal = Format.Money(0);

    public async Task LoadAsync()
    {
        var expenses = await database.GetGeneralExpensesAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var month = DateRange.Month(today);
        var year = DateRange.Year(today);

        MonthTotal = Format.Money(expenses.Where(e => month.Contains(e.Date)).Sum(e => e.Amount));
        YearTotal = Format.Money(expenses.Where(e => year.Contains(e.Date)).Sum(e => e.Amount));

        Expenses.Clear();
        foreach (var e in expenses)
            Expenses.Add(new GeneralExpenseRow(e.Id, e.Note ?? Choices.Label(e.Type),
                $"{Choices.Label(e.Type)} · {e.Date:MMM d, yyyy}", Format.Money(e.Amount)));
    }

    [RelayCommand]
    private Task AddAsync() => Routes.GoToAsync(Routes.GeneralExpense);

    public Task OpenAsync(GeneralExpenseRow row) => Routes.GoToAsync(Routes.GeneralExpense, ("id", row.Id));
}
