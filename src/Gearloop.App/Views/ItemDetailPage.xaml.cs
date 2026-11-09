using Gearloop.App.ViewModels;

namespace Gearloop.App.Views;

public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private void OnExpenseTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: ExpenseRow row })
            _viewModel.EditExpenseCommand.Execute(row);
    }
}
