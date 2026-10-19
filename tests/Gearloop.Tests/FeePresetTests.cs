using Gearloop.Core.Models;
using Gearloop.Core.Profit;

namespace Gearloop.Tests;

public class FeePresetTests
{
    [Fact]
    public void Reverb_charges_selling_fee_and_processing_on_price_plus_shipping()
    {
        // Order total $1,860: selling 5% = $93.00, processing 3.19% + $0.49 = $59.82.
        Assert.Equal(152.82m, FeePresets.Reverb.Calculate(salePrice: 1800m, shippingCharged: 60m));
    }

    [Fact]
    public void Reverb_selling_fee_has_a_minimum()
    {
        // Order total $5: selling fee $0.25 rises to the $0.50 minimum, processing $0.16 + $0.49.
        Assert.Equal(1.15m, FeePresets.Reverb.Calculate(salePrice: 5m, shippingCharged: 0m));
    }

    [Fact]
    public void Reverb_selling_fee_is_capped()
    {
        // Order total $20,000: selling fee $1,000 is capped at $500, processing $638.00 + $0.49.
        Assert.Equal(1138.49m, FeePresets.Reverb.Calculate(salePrice: 20000m, shippingCharged: 0m));
    }

    [Theory]
    [InlineData(SaleChannel.FacebookMarketplace)]
    [InlineData(SaleChannel.Craigslist)]
    [InlineData(SaleChannel.Other)]
    public void Local_channels_have_no_fees(SaleChannel channel)
    {
        Assert.Equal(0m, FeePresets.For(channel).Calculate(500m, 0m));
    }

    [Fact]
    public void Every_sale_channel_has_a_preset()
    {
        foreach (var channel in Enum.GetValues<SaleChannel>())
            Assert.NotNull(FeePresets.For(channel));
    }

    [Theory]
    [InlineData(0.005, 0.01)]
    [InlineData(0.004, 0.00)]
    [InlineData(2.675, 2.68)]
    public void Money_rounds_half_cents_away_from_zero(double input, double expected)
    {
        Assert.Equal((decimal)expected, Money.Round((decimal)input));
    }
}
