using Gearloop.App.ViewModels;

namespace Gearloop.App.Views;

public partial class ItemExpensePage : ContentPage
{
    public ItemExpensePage(ItemExpenseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
