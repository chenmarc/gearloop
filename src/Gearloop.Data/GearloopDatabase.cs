using Gearloop.Core.Models;
using Microsoft.Data.Sqlite;

namespace Gearloop.Data;

/// <summary>Stores items and general expenses in a local SQLite file.</summary>
public sealed class GearloopDatabase
{
    private const string ItemColumns = """
        name, brand, model, year, category, condition, serial_number, notes,
        purchase_date, source, price_paid, sales_tax, shipping_in, listed_price, listed_date,
        sale_date, sale_channel, sale_price, shipping_charged, shipping_cost, fees
        """;

    private readonly string _connectionString;
    private readonly Lazy<Task> _initialization;

    public GearloopDatabase(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
        }.ToString();
        _initialization = new Lazy<Task>(MigrateAsync);
    }

    /// <summary>
    /// Creates the database file if needed and applies any pending migrations. Runs once per
    /// instance; every other method calls it first, so callers never need to.
    /// </summary>
    public Task InitializeAsync() => _initialization.Value;

    private async Task MigrateAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var versionCommand = connection.CreateCommand();
        versionCommand.CommandText = "PRAGMA user_version;";
        var version = Convert.ToInt32(await versionCommand.ExecuteScalarAsync());

        for (var next = version; next < Schema.Migrations.Length; next++)
        {
            await using var transaction = connection.BeginTransaction();
            await using var migrate = connection.CreateCommand();
            migrate.Transaction = transaction;
            migrate.CommandText = $"{Schema.Migrations[next]}\nPRAGMA user_version = {next + 1};";
            await migrate.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        await using var connection = await OpenAsync();
        var items = await ReadItemsAsync(connection, whereClause: null, id: null);
        await LoadChildrenAsync(connection, items);
        return items;
    }

    public async Task<Item?> GetItemAsync(long id)
    {
        await using var connection = await OpenAsync();
        var items = await ReadItemsAsync(connection, "WHERE id = $id", id);
        await LoadChildrenAsync(connection, items);
        return items.SingleOrDefault();
    }

    /// <summary>Inserts a new item (Id 0) or updates an existing one, including its expenses and photos.</summary>
    public async Task SaveItemAsync(Item item)
    {
        await using var connection = await OpenAsync();
        await using var transaction = connection.BeginTransaction();

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = item.Id == 0
                ? $"""
                   INSERT INTO items ({ItemColumns}) VALUES (
                       $name, $brand, $model, $year, $category, $condition, $serial_number, $notes,
                       $purchase_date, $source, $price_paid, $sales_tax, $shipping_in, $listed_price, $listed_date,
                       $sale_date, $sale_channel, $sale_price, $shipping_charged, $shipping_cost, $fees)
                   RETURNING id;
                   """
                : """
                  UPDATE items SET
                      name = $name, brand = $brand, model = $model, year = $year, category = $category,
                      condition = $condition, serial_number = $serial_number, notes = $notes,
                      purchase_date = $purchase_date, source = $source, price_paid = $price_paid,
                      sales_tax = $sales_tax, shipping_in = $shipping_in, listed_price = $listed_price,
                      listed_date = $listed_date, sale_date = $sale_date, sale_channel = $sale_channel,
                      sale_price = $sale_price, shipping_charged = $shipping_charged,
                      shipping_cost = $shipping_cost, fees = $fees
                  WHERE id = $id
                  RETURNING id;
                  """;
            AddItemParameters(command.Parameters, item);
            var result = await command.ExecuteScalarAsync()
                ?? throw new InvalidOperationException($"Item {item.Id} does not exist.");
            item.Id = Convert.ToInt64(result);
        }

        await ExecuteAsync(connection, transaction, "DELETE FROM item_expenses WHERE item_id = $item_id;",
            p => p.Add("$item_id", item.Id));
        foreach (var expense in item.Expenses)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO item_expenses (item_id, date, type, amount, note)
                VALUES ($item_id, $date, $type, $amount, $note)
                RETURNING id;
                """;
            command.Parameters.Add("$item_id", item.Id);
            command.Parameters.Add("$date", SqliteValues.ToDb(expense.Date));
            command.Parameters.Add("$type", expense.Type.ToString());
            command.Parameters.Add("$amount", SqliteValues.ToDb(expense.Amount));
            command.Parameters.Add("$note", SqliteValues.ToDb(expense.Note));
            expense.Id = Convert.ToInt64(await command.ExecuteScalarAsync());
        }

        await ExecuteAsync(connection, transaction, "DELETE FROM item_photos WHERE item_id = $item_id;",
            p => p.Add("$item_id", item.Id));
        for (var position = 0; position < item.PhotoPaths.Count; position++)
        {
            var photoPosition = position;
            await ExecuteAsync(connection, transaction,
                "INSERT INTO item_photos (item_id, position, path) VALUES ($item_id, $position, $path);",
                p =>
                {
                    p.Add("$item_id", item.Id);
                    p.Add("$position", photoPosition);
                    p.Add("$path", item.PhotoPaths[photoPosition]);
                });
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteItemAsync(long id)
    {
        await using var connection = await OpenAsync();
        await ExecuteAsync(connection, null, "DELETE FROM items WHERE id = $id;", p => p.Add("$id", id));
    }

    public async Task<List<GeneralExpense>> GetGeneralExpensesAsync()
    {
        await using var connection = await OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, date, type, amount, note FROM general_expenses ORDER BY date DESC, id DESC;";

        var expenses = new List<GeneralExpense>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            expenses.Add(new GeneralExpense
            {
                Id = reader.GetInt64(0),
                Date = reader.GetDate("date"),
                Type = reader.GetEnum<GeneralExpenseType>("type"),
                Amount = reader.GetMoney("amount"),
                Note = reader.GetStringOrNull("note"),
            });
        }
        return expenses;
    }

    /// <summary>Inserts a new general expense (Id 0) or updates an existing one.</summary>
    public async Task SaveGeneralExpenseAsync(GeneralExpense expense)
    {
        await using var connection = await OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = expense.Id == 0
            ? """
              INSERT INTO general_expenses (date, type, amount, note)
              VALUES ($date, $type, $amount, $note)
              RETURNING id;
              """
            : """
              UPDATE general_expenses SET date = $date, type = $type, amount = $amount, note = $note
              WHERE id = $id
              RETURNING id;
              """;
        command.Parameters.Add("$id", expense.Id);
        command.Parameters.Add("$date", SqliteValues.ToDb(expense.Date));
        command.Parameters.Add("$type", expense.Type.ToString());
        command.Parameters.Add("$amount", SqliteValues.ToDb(expense.Amount));
        command.Parameters.Add("$note", SqliteValues.ToDb(expense.Note));
        var result = await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException($"General expense {expense.Id} does not exist.");
        expense.Id = Convert.ToInt64(result);
    }

    public async Task DeleteGeneralExpenseAsync(long id)
    {
        await using var connection = await OpenAsync();
        await ExecuteAsync(connection, null, "DELETE FROM general_expenses WHERE id = $id;", p => p.Add("$id", id));
    }

    private async Task<SqliteConnection> OpenAsync()
    {
        await InitializeAsync();
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task ExecuteAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        Action<SqliteParameterCollection> addParameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        addParameters(command.Parameters);
        await command.ExecuteNonQueryAsync();
    }

    private static void AddItemParameters(SqliteParameterCollection p, Item item)
    {
        p.Add("$id", item.Id);
        p.Add("$name", item.Name);
        p.Add("$brand", SqliteValues.ToDb(item.Brand));
        p.Add("$model", SqliteValues.ToDb(item.Model));
        p.Add("$year", SqliteValues.ToDb(item.Year));
        p.Add("$category", item.Category.ToString());
        p.Add("$condition", SqliteValues.ToDb(item.Condition));
        p.Add("$serial_number", SqliteValues.ToDb(item.SerialNumber));
        p.Add("$notes", SqliteValues.ToDb(item.Notes));
        p.Add("$purchase_date", SqliteValues.ToDb(item.PurchaseDate));
        p.Add("$source", item.Source.ToString());
        p.Add("$price_paid", SqliteValues.ToDb(item.PricePaid));
        p.Add("$sales_tax", SqliteValues.ToDb(item.SalesTax));
        p.Add("$shipping_in", SqliteValues.ToDb(item.ShippingIn));
        p.Add("$listed_price", SqliteValues.ToDb(item.ListedPrice));
        p.Add("$listed_date", SqliteValues.ToDb(item.ListedDate));

        var sale = item.Sale;
        p.Add("$sale_date", SqliteValues.ToDb(sale?.Date));
        p.Add("$sale_channel", SqliteValues.ToDb(sale?.Channel.ToString()));
        p.Add("$sale_price", SqliteValues.ToDb(sale?.SalePrice));
        p.Add("$shipping_charged", SqliteValues.ToDb(sale?.ShippingCharged));
        p.Add("$shipping_cost", SqliteValues.ToDb(sale?.ShippingCost));
        p.Add("$fees", SqliteValues.ToDb(sale?.Fees));
    }

    private static async Task<List<Item>> ReadItemsAsync(SqliteConnection connection, string? whereClause, long? id)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT id, {ItemColumns} FROM items {whereClause} ORDER BY purchase_date DESC, id DESC;";
        if (id is { } value) command.Parameters.Add("$id", value);

        var items = new List<Item>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = new Item
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Brand = reader.GetStringOrNull("brand"),
                Model = reader.GetStringOrNull("model"),
                Year = reader.GetIntOrNull("year"),
                Category = reader.GetEnum<Category>("category"),
                Condition = reader.GetStringOrNull("condition"),
                SerialNumber = reader.GetStringOrNull("serial_number"),
                Notes = reader.GetStringOrNull("notes"),
                PurchaseDate = reader.GetDate("purchase_date"),
                Source = reader.GetEnum<PurchaseSource>("source"),
                PricePaid = reader.GetMoney("price_paid"),
                SalesTax = reader.GetMoney("sales_tax"),
                ShippingIn = reader.GetMoney("shipping_in"),
                ListedPrice = reader.GetMoneyOrNull("listed_price"),
                ListedDate = reader.GetDateOrNull("listed_date"),
            };

            if (reader.GetDateOrNull("sale_date") is { } saleDate)
            {
                item.Sale = new Sale
                {
                    Date = saleDate,
                    Channel = reader.GetEnum<SaleChannel>("sale_channel"),
                    SalePrice = reader.GetMoney("sale_price"),
                    ShippingCharged = reader.GetMoney("shipping_charged"),
                    ShippingCost = reader.GetMoney("shipping_cost"),
                    Fees = reader.GetMoney("fees"),
                };
            }

            items.Add(item);
        }
        return items;
    }

    private static async Task LoadChildrenAsync(SqliteConnection connection, List<Item> items)
    {
        if (items.Count == 0) return;
        var byId = items.ToDictionary(i => i.Id);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, item_id, date, type, amount, note FROM item_expenses ORDER BY date, id;";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!byId.TryGetValue(reader.GetInt64(1), out var item)) continue;
                item.Expenses.Add(new ItemExpense
                {
                    Id = reader.GetInt64(0),
                    Date = reader.GetDate("date"),
                    Type = reader.GetEnum<ItemExpenseType>("type"),
                    Amount = reader.GetMoney("amount"),
                    Note = reader.GetStringOrNull("note"),
                });
            }
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT item_id, path FROM item_photos ORDER BY item_id, position;";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (byId.TryGetValue(reader.GetInt64(0), out var item))
                    item.PhotoPaths.Add(reader.GetString(1));
            }
        }
    }
}
