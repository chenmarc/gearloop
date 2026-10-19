namespace Gearloop.Core.Models;

public enum Category
{
    Guitar,
    Bass,
    Amp,
    Pedal,
    Keys,
    Drums,
    Other,
}

/// <summary>Where an item was bought.</summary>
public enum PurchaseSource
{
    Reverb,
    EBay,
    GuitarCenter,
    FacebookMarketplace,
    Craigslist,
    Other,
}

/// <summary>Where an item was sold.</summary>
public enum SaleChannel
{
    Reverb,
    FacebookMarketplace,
    Craigslist,
    Other,
}

public enum ItemStatus
{
    Owned,
    Listed,
    Sold,
}

public enum ItemExpenseType
{
    Repair,
    Parts,
    Setup,
    Cleaning,
    Case,
    ShippingSupplies,
    Other,
}

public enum GeneralExpenseType
{
    Gas,
    Supplies,
    Subscriptions,
    Tools,
    Other,
}
