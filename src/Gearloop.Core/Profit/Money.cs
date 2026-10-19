namespace Gearloop.Core.Profit;

public static class Money
{
    /// <summary>Rounds to whole cents, with halves rounded away from zero as people expect.</summary>
    public static decimal Round(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
