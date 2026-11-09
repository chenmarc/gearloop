using Gearloop.App.ViewModels;

namespace Gearloop.App.Views;

public partial class ItemEditPage : ContentPage
{
    private readonly ItemEditViewModel _viewModel;

    public ItemEditPage(ItemEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnRemovePhotoClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: PhotoRow photo })
            _viewModel.RemovePhotoCommand.Execute(photo);
    }
}
