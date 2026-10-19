using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Gearloop.Data;

/// <summary>
/// Converts values to and from their SQLite form. Money is stored as invariant-culture TEXT
/// so it round-trips exactly; SQLite's REAL type would introduce floating-point error.
/// </summary>
internal static class SqliteValues
{
    private const string DateFormat = "yyyy-MM-dd";

    public static object ToDb(decimal value) => value.ToString(CultureInfo.InvariantCulture);
    public static object ToDb(decimal? value) => value is { } v ? ToDb(v) : DBNull.Value;
    public static object ToDb(DateOnly value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);
    public static object ToDb(DateOnly? value) => value is { } v ? ToDb(v) : DBNull.Value;
    public static object ToDb(string? value) => value is null ? DBNull.Value : value;
    public static object ToDb(int? value) => value is { } v ? v : DBNull.Value;

    public static string? GetStringOrNull(this SqliteDataReader r, string column) =>
        r.IsDBNull(r.GetOrdinal(column)) ? null : r.GetString(r.GetOrdinal(column));

    public static decimal GetMoney(this SqliteDataReader r, string column) =>
        decimal.Parse(r.GetString(r.GetOrdinal(column)), CultureInfo.InvariantCulture);

    public static decimal? GetMoneyOrNull(this SqliteDataReader r, string column) =>
        r.GetStringOrNull(column) is { } s ? decimal.Parse(s, CultureInfo.InvariantCulture) : null;

    public static DateOnly GetDate(this SqliteDataReader r, string column) =>
        DateOnly.ParseExact(r.GetString(r.GetOrdinal(column)), DateFormat, CultureInfo.InvariantCulture);

    public static DateOnly? GetDateOrNull(this SqliteDataReader r, string column) =>
        r.GetStringOrNull(column) is { } s ? DateOnly.ParseExact(s, DateFormat, CultureInfo.InvariantCulture) : null;

    public static int? GetIntOrNull(this SqliteDataReader r, string column) =>
        r.IsDBNull(r.GetOrdinal(column)) ? null : r.GetInt32(r.GetOrdinal(column));

    public static TEnum GetEnum<TEnum>(this SqliteDataReader r, string column) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(r.GetString(r.GetOrdinal(column)));

    public static void Add(this SqliteParameterCollection parameters, string name, object value) =>
        parameters.AddWithValue(name, value);
}
