using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gearloop.App.Services;
using Gearloop.Core.Models;
using Gearloop.Core.Text;
using Gearloop.Data;

namespace Gearloop.App.ViewModels;

/// <summary>A photo shown in a form: its stored relative path and the file to display.</summary>
public sealed record PhotoRow(string RelativePath)
{
    public string FullPath => PhotoStore.FullPath(RelativePath);
}

/// <summary>Logs a new buy, or edits an existing item's details.</summary>
public partial class ItemEditViewModel(GearloopDatabase database) : ObservableObject, IQueryAttributable
{
    private Item? _item;
    private readonly List<string> _removedPhotos = [];

    public IReadOnlyList<Choice<Category>> Categories => Choices.Categories;
    public IReadOnlyList<Choice<PurchaseSource>> Sources => Choices.Sources;
    public ObservableCollection<PhotoRow> Photos { get; } = [];

    [ObservableProperty] private string _title = "Log a buy";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private Choice<Category>? _category;
    [ObservableProperty] private Choice<PurchaseSource>? _source;
    [ObservableProperty] private string _pricePaid = "";
    [ObservableProperty] private DateTime? _purchaseDate = DateTime.Today;
    [ObservableProperty] private string _salesTax = "";
    [ObservableProperty] private string _shippingIn = "";
    [ObservableProperty] private string _brand = "";
    [ObservableProperty] private string _model = "";
    [ObservableProperty] private string _year = "";
    [ObservableProperty] private string _condition = "";
    [ObservableProperty] private string _serialNumber = "";
    [ObservableProperty] private string _notes = "";
    [ObservableProperty] private bool _showMoreDetails;

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && id is long itemId && itemId != 0)
        {
            _item = await database.GetItemAsync(itemId);
            if (_item is not null) Load(_item);
        }
    }

    private void Load(Item item)
    {
        Title = "Edit item";
        Name = item.Name;
        Category = Choices.Find(Categories, item.Category);
        Source = Choices.Find(Sources, item.Source);
        PricePaid = MoneyText.ToInput(item.PricePaid);
        PurchaseDate = item.PurchaseDate.ToDateTime(TimeOnly.MinValue);
        SalesTax = MoneyText.ToOptionalInput(item.SalesTax);
        ShippingIn = MoneyText.ToOptionalInput(item.ShippingIn);
        Brand = item.Brand ?? "";
        Model = item.Model ?? "";
        Year = item.Year?.ToString() ?? "";
        Condition = item.Condition ?? "";
        SerialNumber = item.SerialNumber ?? "";
        Notes = item.Notes ?? "";
        ShowMoreDetails = item.Brand is not null || item.Model is not null || item.Year is not null
            || item.Condition is not null || item.SerialNumber is not null || item.Notes is not null;

        Photos.Clear();
        foreach (var path in item.PhotoPaths) Photos.Add(new PhotoRow(path));
    }

    [RelayCommand]
    private void ToggleMoreDetails() => ShowMoreDetails = !ShowMoreDetails;

    [RelayCommand]
    private async Task AddPhotoAsync()
    {
        var fromCamera = false;
        if (PhotoStore.CanUseCamera)
        {
            var choice = await Dialogs.ChooseAsync("Add a photo", "Take photo", "Choose from library");
            if (choice is null or "Cancel") return;
            fromCamera = choice == "Take photo";
        }

        try
        {
            foreach (var path in await PhotoStore.AddAsync(fromCamera))
                Photos.Add(new PhotoRow(path));
        }
        catch (PermissionException)
        {
            await Dialogs.ShowAsync("No access", "gearloop needs permission to use your photos. You can allow it in Settings.");
        }
    }

    [RelayCommand]
    private void RemovePhoto(PhotoRow photo)
    {
        Photos.Remove(photo);
        _removedPhotos.Add(photo.RelativePath);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(Name)) problems.Add("Add a name.");
        if (Category is null) problems.Add("Pick a category.");
        if (Source is null) problems.Add("Pick where you bought it.");
        if (!MoneyText.TryParse(PricePaid, out var pricePaid)) problems.Add("Enter the price you paid, like 450 or 450.00.");
        if (!MoneyText.TryParseOptional(SalesTax, out var salesTax)) problems.Add("Sales tax should be an amount, or left blank.");
        if (!MoneyText.TryParseOptional(ShippingIn, out var shippingIn)) problems.Add("Shipping should be an amount, or left blank.");

        int? year = null;
        if (!string.IsNullOrWhiteSpace(Year))
        {
            if (int.TryParse(Year.Trim(), out var parsedYear) && parsedYear is >= 1850 and <= 2100) year = parsedYear;
            else problems.Add("Year should look like 1978, or be left blank.");
        }

        if (problems.Count > 0)
        {
            await Dialogs.ShowAsync("Almost there", string.Join("\n", problems));
            return;
        }

        var item = _item ?? new Item { Name = "" };
        item.Name = Name.Trim();
        item.Category = Category!.Value;
        item.Source = Source!.Value;
        item.PricePaid = pricePaid;
        item.PurchaseDate = DateOnly.FromDateTime(PurchaseDate ?? DateTime.Today);
        item.SalesTax = salesTax;
        item.ShippingIn = shippingIn;
        item.Brand = Blank(Brand);
        item.Model = Blank(Model);
        item.Year = year;
        item.Condition = Blank(Condition);
        item.SerialNumber = Blank(SerialNumber);
        item.Notes = Blank(Notes);
        item.PhotoPaths = Photos.Select(p => p.RelativePath).ToList();

        await database.SaveItemAsync(item);
        PhotoStore.Delete(_removedPhotos);

        if (_item is null)
            await Shell.Current.GoToAsync($"../{Routes.Item}", new Dictionary<string, object> { ["id"] = item.Id });
        else
            await Routes.BackAsync();
    }

    private static string? Blank(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
