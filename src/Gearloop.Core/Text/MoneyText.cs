using System.Globalization;
using Gearloop.Core.Profit;

namespace Gearloop.Core.Text;

/// <summary>Reads and writes dollar amounts typed into form fields.</summary>
public static class MoneyText
{
    /// <summary>
    /// Parses amounts like "1200", "$1,200.50" or " 45.5 ". Fails on blank, negative or
    /// non-numeric text. Results are rounded to whole cents.
    /// </summary>
    public static bool TryParse(string? text, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var cleaned = text.Trim().Replace("$", "").Replace(",", "").Replace(" ", "");
        if (!decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            return false;

        amount = Money.Round(value);
        return true;
    }

    /// <summary>Like <see cref="TryParse"/>, but blank text is a valid zero.</summary>
    public static bool TryParseOptional(string? text, out decimal amount)
    {
        amount = 0m;
        return string.IsNullOrWhiteSpace(text) || TryParse(text, out amount);
    }

    /// <summary>Formats an amount for editing: "1200" for whole dollars, otherwise "1200.50".</summary>
    public static string ToInput(decimal amount) =>
        amount == decimal.Truncate(amount)
            ? decimal.Truncate(amount).ToString(CultureInfo.InvariantCulture)
            : amount.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>Formats an optional amount for editing, with blank for zero or missing.</summary>
    public static string ToOptionalInput(decimal? amount) =>
        amount is { } value && value != 0m ? ToInput(value) : "";
}
