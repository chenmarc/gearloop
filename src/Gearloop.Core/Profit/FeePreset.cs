using Gearloop.Core.Models;

namespace Gearloop.Core.Profit;

/// <summary>
/// One part of a channel's fees: a percentage of the order total plus a fixed amount,
/// optionally clamped to a minimum and maximum.
/// </summary>
public sealed record FeeComponent(
    string Name,
    decimal Percent,
    decimal Fixed = 0m,
    decimal? Minimum = null,
    decimal? Maximum = null)
{
    public decimal Calculate(decimal orderTotal)
    {
        var fee = orderTotal * Percent / 100m + Fixed;
        if (Minimum is { } min && fee < min) fee = min;
        if (Maximum is { } max && fee > max) fee = max;
        return Money.Round(fee);
    }
}

/// <summary>The fees a sales channel usually charges, used to prefill a sale's fees.</summary>
public sealed record FeePreset(SaleChannel Channel, IReadOnlyList<FeeComponent> Components)
{
    /// <summary>Fees on an order, where the order total is sale price plus shipping charged.</summary>
    public decimal Calculate(decimal salePrice, decimal shippingCharged)
    {
        if (Components.Count == 0) return 0m;
        var orderTotal = salePrice + shippingCharged;
        return Components.Sum(c => c.Calculate(orderTotal));
    }
}

public static class FeePresets
{
    /// <summary>
    /// Reverb US standard seller rates as of June 2026: 5% selling fee on price plus shipping
    /// (min $0.50, max $500), and 3.19% + $0.49 payment processing.
    /// </summary>
    public static FeePreset Reverb { get; } = new(SaleChannel.Reverb,
    [
        new FeeComponent("Selling fee", 5m, Minimum: 0.50m, Maximum: 500m),
        new FeeComponent("Payment processing", 3.19m, Fixed: 0.49m),
    ]);

    public static IReadOnlyList<FeePreset> Defaults { get; } =
    [
        Reverb,
        new(SaleChannel.FacebookMarketplace, []),
        new(SaleChannel.Craigslist, []),
        new(SaleChannel.Other, []),
    ];

    public static FeePreset For(SaleChannel channel) =>
        Defaults.First(p => p.Channel == channel);
}
