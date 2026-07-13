# RuneScape Price Checker

RuneScape Price Checker (shown as **GE Ledger** in the UI) is a local Blazor app for focused Old School RuneScape market tracking and modular GP-per-hour calculators.

## Current features

- A favourites-only dashboard backed by a local JSON store.
- Seven days of hourly price history from the OSRS Wiki real-time price API.
- Startup history warming and short-lived in-memory caches for API-friendly refreshes.
- A shared item-flow calculation engine for supplies, outputs, Grand Exchange tax, multiple accounts, and future XP rewards.
- A self-registering Vyrewatch Sentinels method ported from the legacy calculator.

The Wiki API already provides recent time-series data, so the app can render weekly graphs when it starts. It does not need a continuously running website or its own historical-price collector for this view.

## Documentation

- [Application requirements](APPLICATION_REQUIREMENTS.md)

## Run locally

Requirements: the .NET 8 SDK or newer.

```powershell
dotnet run --project src\RunescapePriceChecker.Web\RunescapePriceChecker.Web.csproj
```

The HTTP launch profile opens at `http://localhost:5142`.

Before publishing the app, update `OsrsWiki:UserAgent` in `src/RunescapePriceChecker.Web/appsettings.json` with an appropriate contact value for the Wiki API maintainers.

## Verify

```powershell
dotnet build RunescapePriceChecker.sln
dotnet run --project tests\RunescapePriceChecker.Tests\RunescapePriceChecker.Tests.csproj
```

## Add a money-making method

Create a class under `src/RunescapePriceChecker.Core/MoneyMaking/Methods` that implements `IMoneyMakingMethod`. Describe each consumed or produced item with an `ItemFlow`; the app discovers concrete method classes automatically and the shared calculator handles current prices, quantities, tax, and per-account totals.

Local favourites live in `src/RunescapePriceChecker.Web/data/favourites.json`. ASP.NET data-protection keys and build output are intentionally ignored by Git.
