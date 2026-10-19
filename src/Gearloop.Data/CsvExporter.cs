using System.Globalization;
using Gearloop.Core.Models;
using Gearloop.Core.Profit;

namespace Gearloop.Data;

/// <summary>Writes items to CSV for backup or for opening in a spreadsheet.</summary>
public static class CsvExporter
{
    private static readonly string[] Header =
    [
        "Id", "Name", "Brand", "Model", "Year", "Category", "Status",
        "Purchase date", "Source", "Price paid", "Sales tax", "Shipping in", "Item expenses", "Total cost",
        "Listed date", "Listed price",
        "Sale date", "Channel", "Sale price", "Shipping charged", "Shipping cost", "Fees", "Net proceeds",
        "Profit", "Days held",
    ];

    public static void WriteItems(IEnumerable<Item> items, TextWriter writer)
    {
        WriteRow(writer, Header);
        foreach (var item in items)
        {
            var sale = item.Sale;
            WriteRow(writer,
            [
                item.Id.ToString(CultureInfo.InvariantCulture),
                item.Name,
                item.Brand,
                item.Model,
                item.Year?.ToString(CultureInfo.InvariantCulture),
                item.Category.ToString(),
                item.Status.ToString(),
                Format(item.PurchaseDate),
                item.Source.ToString(),
                Format(item.PricePaid),
                Format(item.SalesTax),
                Format(item.ShippingIn),
                Format(item.Expenses.Sum(e => e.Amount)),
                Format(ProfitCalculator.TotalCost(item)),
                Format(item.ListedDate),
                Format(item.ListedPrice),
                Format(sale?.Date),
                sale?.Channel.ToString(),
                Format(sale?.SalePrice),
                Format(sale?.ShippingCharged),
                Format(sale?.ShippingCost),
                Format(sale?.Fees),
                sale is null ? null : Format(ProfitCalculator.NetProceeds(sale)),
                Format(ProfitCalculator.Profit(item)),
                ProfitCalculator.DaysHeld(item)?.ToString(CultureInfo.InvariantCulture),
            ]);
        }
    }

    private static string? Format(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture);
    private static string? Format(DateOnly? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static void WriteRow(TextWriter writer, IEnumerable<string?> fields) =>
        writer.WriteLine(string.Join(",", fields.Select(Escape)));

    private static string Escape(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        return field.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? $"\"{field.Replace("\"", "\"\"")}\""
            : field;
    }
}
