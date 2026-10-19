using Gearloop.Core.Models;
using Gearloop.Data;
using Microsoft.Data.Sqlite;

namespace Gearloop.Tests;

public sealed class GearloopDatabaseTests : IAsyncLifetime
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"gearloop-test-{Guid.NewGuid():N}.db");
    private GearloopDatabase _db = null!;

    public async Task InitializeAsync()
    {
        _db = new GearloopDatabase(_path);
        await _db.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Saved_item_round_trips_with_sale_expenses_and_photos()
    {
        var item = TestData.SoldBass();
        item.ListedPrice = 1899.99m;
        item.ListedDate = new DateOnly(2026, 3, 20);
        item.PhotoPaths = ["photos/front.jpg", "photos/back.jpg"];

        await _db.SaveItemAsync(item);
        var loaded = await _db.GetItemAsync(item.Id);

        Assert.NotNull(loaded);
        Assert.Equal(item.Name, loaded.Name);
        Assert.Equal(1978, loaded.Year);
        Assert.Equal(Category.Bass, loaded.Category);
        Assert.Equal(PurchaseSource.Craigslist, loaded.Source);
        Assert.Equal(1899.99m, loaded.ListedPrice);
        Assert.Equal(152.82m, loaded.Sale!.Fees);
        Assert.Equal(SaleChannel.Reverb, loaded.Sale.Channel);
        Assert.Equal(new DateOnly(2026, 4, 15), loaded.Sale.Date);
        Assert.Equal("Fret level", Assert.Single(loaded.Expenses).Note);
        Assert.Equal(["photos/front.jpg", "photos/back.jpg"], loaded.PhotoPaths);
        Assert.Equal(237.18m, Gearloop.Core.Profit.ProfitCalculator.Profit(loaded));
    }

    [Fact]
    public async Task Updating_an_item_replaces_its_expenses_and_can_clear_the_sale()
    {
        var item = TestData.SoldBass();
        await _db.SaveItemAsync(item);

        item.Sale = null;
        item.Expenses.Add(new ItemExpense { Date = new DateOnly(2026, 3, 12), Type = ItemExpenseType.Parts, Amount = 35.10m });
        await _db.SaveItemAsync(item);

        var loaded = (await _db.GetItemsAsync()).Single();
        Assert.Null(loaded.Sale);
        Assert.Equal([150m, 35.10m], loaded.Expenses.Select(e => e.Amount));
    }

    [Fact]
    public async Task Deleting_an_item_removes_it()
    {
        var item = TestData.Bass();
        await _db.SaveItemAsync(item);

        await _db.DeleteItemAsync(item.Id);

        Assert.Empty(await _db.GetItemsAsync());
    }

    [Fact]
    public async Task General_expenses_save_update_and_delete()
    {
        var expense = new GeneralExpense { Date = new DateOnly(2026, 4, 2), Type = GeneralExpenseType.Gas, Amount = 18.50m };
        await _db.SaveGeneralExpenseAsync(expense);

        expense.Amount = 21.25m;
        await _db.SaveGeneralExpenseAsync(expense);
        Assert.Equal(21.25m, Assert.Single(await _db.GetGeneralExpensesAsync()).Amount);

        await _db.DeleteGeneralExpenseAsync(expense.Id);
        Assert.Empty(await _db.GetGeneralExpensesAsync());
    }

    [Fact]
    public async Task Initialize_is_safe_to_run_again()
    {
        await _db.SaveItemAsync(TestData.Bass());

        await new GearloopDatabase(_path).InitializeAsync();

        Assert.Single(await _db.GetItemsAsync());
    }
}
