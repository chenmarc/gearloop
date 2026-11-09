using Gearloop.App.Views;

namespace Gearloop.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Routes.Item, typeof(ItemDetailPage));
        Routing.RegisterRoute(Routes.EditItem, typeof(ItemEditPage));
        Routing.RegisterRoute(Routes.ItemExpense, typeof(ItemExpensePage));
        Routing.RegisterRoute(Routes.Sale, typeof(SalePage));
        Routing.RegisterRoute(Routes.GeneralExpense, typeof(GeneralExpensePage));
    }
}
