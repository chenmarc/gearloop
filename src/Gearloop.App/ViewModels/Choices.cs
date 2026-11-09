using Gearloop.Core.Models;

namespace Gearloop.App.ViewModels;

/// <summary>One option in a picker: the stored value and the words shown for it.</summary>
public sealed record Choice<T>(T Value, string Label)
{
    public override string ToString() => Label;
}

/// <summary>Friendly names for the enums shown in pickers and lists.</summary>
public static class Choices
{
    public static IReadOnlyList<Choice<Category>> Categories { get; } = For<Category>();
    public static IReadOnlyList<Choice<PurchaseSource>> Sources { get; } = For<PurchaseSource>();
    public static IReadOnlyList<Choice<SaleChannel>> Channels { get; } = For<SaleChannel>();
    public static IReadOnlyList<Choice<ItemExpenseType>> ItemExpenseTypes { get; } = For<ItemExpenseType>();
    public static IReadOnlyList<Choice<GeneralExpenseType>> GeneralExpenseTypes { get; } = For<GeneralExpenseType>();

    public static string Label<T>(T value) where T : struct, Enum => value switch
    {
        PurchaseSource.EBay => "eBay",
        PurchaseSource.GuitarCenter => "Guitar Center",
        PurchaseSource.FacebookMarketplace or SaleChannel.FacebookMarketplace => "Facebook Marketplace",
        ItemExpenseType.ShippingSupplies => "Shipping supplies",
        _ => value.ToString(),
    };

    public static Choice<T> Find<T>(IReadOnlyList<Choice<T>> choices, T value) where T : struct, Enum =>
        choices.First(c => EqualityComparer<T>.Default.Equals(c.Value, value));

    private static List<Choice<T>> For<T>() where T : struct, Enum =>
        Enum.GetValues<T>().Select(v => new Choice<T>(v, Label(v))).ToList();
}
