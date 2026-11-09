using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.App.Services;
using Gearloop.Core.Models;
using Gearloop.Core.Text;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>Adds or edits a cost attached to one item, such as a repair.</summary>
public partial class ItemExpenseViewModel(GearloopDatabase database) : ObservableObject, IQueryAttributable
{
    private Item? _item;
    private ItemExpense? _expense;

    public IReadOnlyList<Choice<ItemExpenseType>> Types => Choices.ItemExpenseTypes;

    [ObservableProperty] private string _title = "Add expense";
    [ObservableProperty] private string _itemName = "";
    [ObservableProperty] private Choice<ItemExpenseType>? _type;
    [ObservableProperty] private string _amount = "";
    [ObservableProperty] private DateTime? _date = DateTime.Today;
    [ObservableProperty] private string _note = "";
    [ObservableProperty] private bool _isExisting;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("itemId", out var id) || id is not long itemId) return;
        _item = await database.GetItemAsync(itemId);
        if (_item is null) return;
        ItemName = _item.Name;
        Type = Choices.Find(Types, ItemExpenseType.Repair);

        if (query.TryGetValue("expenseId", out var eid) && eid is long expenseId)
        {
            _expense = _item.Expenses.FirstOrDefault(e => e.Id == expenseId);
            if (_expense is not null)
            {
                Title = "Edit expense";
                IsExisting = true;
                Type = Choices.Find(Types, _expense.Type);
                Amount = MoneyText.ToInput(_expense.Amount);
                Date = _expense.Date.ToDateTime(TimeOnly.MinValue);
                Note = _expense.Note ?? "";
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_item is null || Type is null) return;
        if (!MoneyText.TryParse(Amount, out var amount))
        {
            await Dialogs.ShowAsync("Amount", "Enter what it cost, like 85 or 85.50.");
            return;
        }

        var expense = _expense ?? new ItemExpense();
        expense.Type = Type.Value;
        expense.Amount = amount;
        expense.Date = DateOnly.FromDateTime(Date ?? DateTime.Today);
        expense.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();
        if (_expense is null) _item.Expenses.Add(expense);

        await database.SaveItemAsync(_item);
        await Routes.BackAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_item is null || _expense is null) return;
        if (!await Dialogs.ConfirmAsync("Delete expense?", $"{Choices.Label(_expense.Type)}, {Format.Money(_expense.Amount)}", "Delete"))
            return;

        _item.Expenses.Remove(_expense);
        await database.SaveItemAsync(_item);
        await Routes.BackAsync();
    }
}
