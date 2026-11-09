using Gearloop.App.ViewModels;

namespace Gearloop.App.Views;

public partial class GeneralExpensePage : ContentPage
{
    public GeneralExpensePage(GeneralExpenseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
