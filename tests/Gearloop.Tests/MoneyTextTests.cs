using Gearloop.Core.Text;

namespace Gearloop.Tests;

public class MoneyTextTests
{
    [Theory]
    [InlineData("1200", 1200)]
    [InlineData("$1,200.50", 1200.50)]
    [InlineData(" 45.5 ", 45.50)]
    [InlineData("0.005", 0.01)]
    [InlineData(".99", 0.99)]
    public void Parses_typed_amounts(string text, double expected)
    {
        Assert.True(MoneyText.TryParse(text, out var amount));
        Assert.Equal((decimal)expected, amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("-5")]
    [InlineData("abc")]
    [InlineData("12.3.4")]
    public void Rejects_blank_negative_and_garbage(string? text)
    {
        Assert.False(MoneyText.TryParse(text, out _));
    }

    [Fact]
    public void Optional_treats_blank_as_zero_but_still_rejects_garbage()
    {
        Assert.True(MoneyText.TryParseOptional("", out var blank));
        Assert.Equal(0m, blank);
        Assert.False(MoneyText.TryParseOptional("abc", out _));
    }

    [Theory]
    [InlineData(1200, "1200")]
    [InlineData(1200.5, "1200.50")]
    [InlineData(0.99, "0.99")]
    public void Formats_for_editing(double amount, string expected)
    {
        Assert.Equal(expected, MoneyText.ToInput((decimal)amount));
    }

    [Fact]
    public void Optional_formats_zero_and_null_as_blank()
    {
        Assert.Equal("", MoneyText.ToOptionalInput(0m));
        Assert.Equal("", MoneyText.ToOptionalInput(null));
        Assert.Equal("45", MoneyText.ToOptionalInput(45m));
    }
}
