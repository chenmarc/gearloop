using Gearloop.App.ViewModels;
using Gearloop.App.Views;
using Gearloop.Data;
using Microsoft.Extensions.Logging;

namespace Gearloop.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(_ =>
            new GearloopDatabase(Path.Combine(FileSystem.AppDataDirectory, "gearloop.db")));

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<InventoryViewModel>();
        builder.Services.AddTransient<InventoryPage>();

        return builder.Build();
    }
}
