namespace Gearloop.Core.Profit;

/// <summary>An inclusive range of dates used for running totals.</summary>
public sealed record DateRange(DateOnly Start, DateOnly End)
{
    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public static DateRange AllTime { get; } = new(DateOnly.MinValue, DateOnly.MaxValue);

    public static DateRange Month(DateOnly anyDayInMonth)
    {
        var start = new DateOnly(anyDayInMonth.Year, anyDayInMonth.Month, 1);
        return new(start, start.AddMonths(1).AddDays(-1));
    }

    public static DateRange Year(DateOnly anyDayInYear) =>
        new(new DateOnly(anyDayInYear.Year, 1, 1), new DateOnly(anyDayInYear.Year, 12, 31));
}
