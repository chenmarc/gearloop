using Gearloop.App.ViewModels;
using Gearloop.Data;

namespace Gearloop.App.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel, GearloopDatabase database)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

#if DEBUG
        ToolbarItems.Add(new ToolbarItem("Add sample data", null, async () =>
        {
            await SampleData.AddAsync(database);
            await _viewModel.LoadAsync();
        }));
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
