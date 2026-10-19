using System.Globalization;

namespace Gearloop.App.ViewModels;

internal static class Format
{
    private static readonly CultureInfo Usd = CultureInfo.GetCultureInfo("en-US");

    public static string Money(decimal amount) => amount.ToString("C", Usd);

    public static string SignedMoney(decimal amount) =>
        amount > 0 ? "+" + Money(amount) : Money(amount);
}
