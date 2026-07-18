# RunescapeTools

RunescapeTools, presented as **GE Ledger**, is a native Windows desktop workspace for focused Old School RuneScape market tracking and modular GP-per-hour calculations.

The WPF executable is the active front end. The original Razor/Blazor application remains buildable in the solution as a parked reference implementation. Both hosts use the same application, infrastructure, persistence, market-data, and calculation services.

## Current features

- Dashboard summary with saved favourites, current midpoint prices, registered calculators, and history coverage.
- Shared Profile Dashboard with normal-account OSRS Hiscores lookup, all 24 current skills (including Sailing), refresh, and last-profile restore.
- Debounced Grand Exchange item search with add, select, and remove favourite actions.
- Seven days of hourly Wiki price history rendered with LiveCharts2, including local-time tooltips, weekly change, and volume.
- Automatically discovered money-making methods with live repricing and a complete input/output ledger.
- Vyrewatch Sentinels method with supplies, output tax, per-account profit, and five-account total.
- Local JSON persistence, API-friendly caches, bounded history warmup, retry handling, and user-readable failure states.
- Single-instance desktop lifetime to prevent competing favourite-file writes.

## Project structure

| Project | Responsibility |
| --- | --- |
| `RunescapeTools.Core` | Domain records, API contracts, profile models, calculation rules, and money-making definitions. |
| `RunescapeTools.Application` | Market behavior, defensive hiscore parsing, current-profile state, and favourite-history warmup. |
| `RunescapeTools.Infrastructure` | Wiki and Hiscores HTTP clients, JSON persistence, configuration, and shared DI registration. |
| `RunescapeTools.Wpf` | Active Windows front end, Generic Host composition, MVVM view-models, and LiveCharts UI. |
| `RunescapeTools.Web` | Parked Razor front end; retained and kept buildable. |
| `RunescapeTools.Tests` | Calculator, service, persistence, retry, and view-model regression harness. |

## Run the desktop app

Requirements for development: Windows 10 version 2004 or newer and the .NET 8 SDK.

```powershell
dotnet run --project src\RunescapeTools.Wpf\RunescapeTools.Wpf.csproj
```

Desktop favourites are stored at:

```text
%LocalAppData%\RunescapeTools\data\favourites.json
```

The last successfully loaded RSN is stored separately at:

```text
%LocalAppData%\RunescapeTools\data\profile.json
```

The first Profile visit creates this preference with `bottleo` when no saved RSN exists. A new RSN is persisted only after its complete hiscore response has been fetched and parsed successfully.

On first launch after the rename, the app first copies an existing legacy favourites file when available. Otherwise, it seeds this file from the embedded MVP snapshot. Existing desktop data is never replaced. The current seed includes Blood shard, Tanzanite fang, and Scythe of vitur (uncharged).

## Verify

```powershell
dotnet build RunescapeTools.sln
dotnet run --project tests\RunescapeTools.Tests\RunescapeTools.Tests.csproj
```

## Publish the Windows executable

```powershell
dotnet publish src\RunescapeTools.Wpf\RunescapeTools.Wpf.csproj -c Release -r win-x64 -p:PublishProfile=win-x64
```

The profile produces a self-contained, single-file `RunescapeTools.exe` under the WPF project's `bin\Release` publish directory. The target computer does not need the .NET Desktop Runtime installed. Trimming is intentionally disabled for WPF, LiveCharts2, SkiaSharp, and reflection-based method discovery.

## Parked Razor app

The Web front end is not receiving new UI work, but it can still be run for comparison:

```powershell
dotnet run --project src\RunescapeTools.Web\RunescapeTools.Web.csproj
```

Its data remains under `src\RunescapeTools.Web\data` and is separate from desktop state.

## Add a money-making method

Create a class under `src\RunescapeTools.Core\MoneyMaking\Methods` that implements `IMoneyMakingMethod`. Describe each consumed or produced item with an `ItemFlow`; shared dependency registration discovers concrete methods automatically and the calculator handles current prices, quantities, tax, and per-account totals.

See [APPLICATION_REQUIREMENTS.md](APPLICATION_REQUIREMENTS.md) for the full product and technical requirements.
