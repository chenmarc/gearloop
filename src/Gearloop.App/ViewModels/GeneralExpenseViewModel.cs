using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.App.Services;
using Gearloop.Core.Models;
using Gearloop.Core.Text;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>Adds or edits a general business expense.</summary>
public partial class GeneralExpenseViewModel(GearloopDatabase database) : ObservableObject, IQueryAttributable
{
    private GeneralExpense? _expense;

    public IReadOnlyList<Choice<GeneralExpenseType>> Types => Choices.GeneralExpenseTypes;

    [ObservableProperty] private string _title = "Add expense";
    [ObservableProperty] private Choice<GeneralExpenseType>? _type = Choices.Find(Choices.GeneralExpenseTypes, GeneralExpenseType.Gas);
    [ObservableProperty] private string _amount = "";
    [ObservableProperty] private DateTime? _date = DateTime.Today;
    [ObservableProperty] private string _note = "";
    [ObservableProperty] private bool _isExisting;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("id", out var id) || id is not long expenseId) return;
        _expense = (await database.GetGeneralExpensesAsync()).FirstOrDefault(e => e.Id == expenseId);
        if (_expense is null) return;

        Title = "Edit expense";
        IsExisting = true;
        Type = Choices.Find(Types, _expense.Type);
        Amount = MoneyText.ToInput(_expense.Amount);
        Date = _expense.Date.ToDateTime(TimeOnly.MinValue);
        Note = _expense.Note ?? "";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Type is null) return;
        if (!MoneyText.TryParse(Amount, out var amount))
        {
            await Dialogs.ShowAsync("Amount", "Enter what it cost, like 18.50.");
            return;
        }

        var expense = _expense ?? new GeneralExpense();
        expense.Type = Type.Value;
        expense.Amount = amount;
        expense.Date = DateOnly.FromDateTime(Date ?? DateTime.Today);
        expense.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();

        await database.SaveGeneralExpenseAsync(expense);
        await Routes.BackAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_expense is null) return;
        if (!await Dialogs.ConfirmAsync("Delete expense?", $"{Choices.Label(_expense.Type)}, {Format.Money(_expense.Amount)}", "Delete"))
            return;

        await database.DeleteGeneralExpenseAsync(_expense.Id);
        await Routes.BackAsync();
    }
}
