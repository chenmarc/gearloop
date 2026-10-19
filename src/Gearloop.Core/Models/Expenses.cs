namespace Gearloop.Core.Models;

/// <summary>A cost attached to one item, such as a repair or new parts.</summary>
public sealed class ItemExpense
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public ItemExpenseType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

/// <summary>A business cost not tied to any one item, such as gas or packing supplies.</summary>
public sealed class GeneralExpense
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public GeneralExpenseType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
