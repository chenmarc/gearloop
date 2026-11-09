using Gearloop.App.ViewModels;

namespace Gearloop.App.Views;

public partial class SalePage : ContentPage
{
    public SalePage(SaleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
