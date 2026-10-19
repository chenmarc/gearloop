namespace Gearloop.Core.Models;

/// <summary>One instrument or piece of gear, from purchase to sale.</summary>
public sealed class Item
{
    public long Id { get; set; }

    public required string Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public Category Category { get; set; }
    public string? Condition { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }

    public DateOnly PurchaseDate { get; set; }
    public PurchaseSource Source { get; set; }
    public decimal PricePaid { get; set; }
    public decimal SalesTax { get; set; }
    public decimal ShippingIn { get; set; }

    public decimal? ListedPrice { get; set; }
    public DateOnly? ListedDate { get; set; }

    public Sale? Sale { get; set; }

    public List<ItemExpense> Expenses { get; set; } = [];

    /// <summary>File paths of photos stored on the device.</summary>
    public List<string> PhotoPaths { get; set; } = [];

    /// <summary>Sold once a sale is recorded, Listed once it has a listing date, otherwise Owned.</summary>
    public ItemStatus Status =>
        Sale is not null ? ItemStatus.Sold
        : ListedDate is not null ? ItemStatus.Listed
        : ItemStatus.Owned;
}
