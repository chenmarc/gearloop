namespace Gearloop.App;

/// <summary>Shell routes for pages pushed on top of the tabs.</summary>
public static class Routes
{
    public const string Item = "item";
    public const string EditItem = "edit-item";
    public const string ItemExpense = "item-expense";
    public const string Sale = "sale";
    public const string GeneralExpense = "general-expense";

    public static Task GoToAsync(string route, params (string Key, object Value)[] query)
    {
        var parameters = query.ToDictionary(q => q.Key, q => q.Value);
        return Shell.Current.GoToAsync(route, parameters);
    }

    public static Task BackAsync() => Shell.Current.GoToAsync("..");
}
