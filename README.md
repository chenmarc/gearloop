# gearloop

Track buys, costs and sales of used and vintage instruments, and see true profit per item and in total.
One .NET MAUI app for macOS and iPhone (Windows later), on .NET 10.

## Layout

| Project | What it holds |
| --- | --- |
| `src/Gearloop.Core` | Data models and all profit math. No UI or database code. |
| `src/Gearloop.Data` | SQLite storage (Microsoft.Data.Sqlite) and CSV export. |
| `src/Gearloop.App` | The MAUI app: screens and view models (CommunityToolkit.Mvvm). |
| `tests/Gearloop.Tests` | xUnit tests for the profit math, fee presets, storage and CSV export. |

## Profit rules

- **Total cost** = price paid + sales tax + shipping in + item expenses
- **Net proceeds** = sale price + shipping charged − shipping cost paid − fees
- **Profit** = net proceeds − total cost; **ROI** = profit ÷ total cost
- **Realized profit** for a period = profit on items sold in the period − general expenses dated in it
- **Money in inventory** = total cost of everything not yet sold

Fees are prefilled per sales channel (`FeePresets`) and can always be overwritten. Reverb's preset is the US
standard seller rate as of June 2026: 5% selling fee on price plus shipping (min $0.50, max $500) and
3.19% + $0.49 payment processing. Local channels default to no fees.

Money is stored as exact decimals (TEXT in SQLite) and rounded to cents with halves rounded away from zero.

## Setting up your Mac

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Install Xcode from the Mac App Store, open it once and let it finish installing components.
3. Install the MAUI workload:
   ```sh
   sudo dotnet workload install maui
   ```

## Running

```sh
# Tests (also run on Linux and Windows; no Xcode needed)
dotnet test tests/Gearloop.Tests

# Mac app
dotnet build src/Gearloop.App -t:Run -f net10.0-maccatalyst

# iPhone simulator
dotnet build src/Gearloop.App -t:Run -f net10.0-ios
```

Debug builds show an **Add sample data** button on the dashboard so the screens have something to show.

The database lives in the app's data folder as `gearloop.db`. Schema changes go in `Schema.Migrations`
in `Gearloop.Data`; add new entries at the end and never edit ones that have shipped.
