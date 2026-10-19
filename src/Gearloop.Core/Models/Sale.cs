namespace Gearloop.Core.Models;

public sealed class Sale
{
    public DateOnly Date { get; set; }
    public SaleChannel Channel { get; set; }
    public decimal SalePrice { get; set; }

    /// <summary>Shipping the buyer paid you.</summary>
    public decimal ShippingCharged { get; set; }

    /// <summary>Shipping you paid to send the item.</summary>
    public decimal ShippingCost { get; set; }

    /// <summary>Platform and payment fees, usually prefilled from a <see cref="Profit.FeePreset"/>.</summary>
    public decimal Fees { get; set; }
}
