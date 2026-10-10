# 07Tools

**An unofficial Old School RuneScape companion, created by bottleo.**

Track your account, follow Grand Exchange prices, compare money makers, and plan the time and GP needed to reach your XP goals with this Windows desktop app.

## What it does

- **Profiles and dashboard:** look up an RSN, view skill progress, and check favourite items at a glance.
- **Favourites:** search items, save and reorder a watch list, and explore price and volume history from one day to one month.
- **Money Makers:** compare item costs and profits with adjustable account counts, action rates, and method settings.
- **XP Planner:** choose training methods, equipment, and XP goals; estimate hours, item quantities, and GP/XP using live or 30-day average prices. Allocate money-maker income to selected training hours.

Prices and Hiscores come from the OSRS Wiki and official OSRS Hiscores APIs. Calculations are estimates; missing prices and unverified economics are shown explicitly.

## Architecture

| Project | Responsibility |
| --- | --- |
| `Core` | Domain models, contracts, item flows, and calculation rules. |
| `Application` | Use cases, profile parsing/state, market caching/search, and pricing coordination. |
| `Infrastructure` | HTTP clients, JSON stores, per-skill method catalogues, and dependency registration. |
| `Wpf` | Desktop application, XAML views, MVVM view-models, and application startup. |
| `Tests` | NUnit tests organized by layer and feature, including WPF smoke checks. |

XAML views bind to view-model properties and commands. View-models call shared services, which use Core calculations and Infrastructure integrations. Each training skill has its own folder; individual methods own their item definitions, while `Global.cs` holds rules shared within that skill.

Projects and assemblies use `07Tools`; C# namespaces retain `RunescapeTools` because identifiers cannot start with a digit.

Settings, favourites, plans, and caches are stored under `%LocalAppData%\07Tools\data`. Replacing the executable preserves this data.

## Development

Use Windows 10 version 2004 or newer (x64) with the .NET 8 SDK.

```powershell
dotnet build 07Tools.sln
dotnet test 07Tools.sln --no-build --blame-hang-timeout 2m
dotnet run --project src\07Tools.Wpf\07Tools.Wpf.csproj
```

GitHub Actions builds and tests pull requests and pushes to `main`. See the [testing guide](tests/07Tools.Tests/README.md) for focused test runs and conventions.

## Publish

```powershell
dotnet publish src\07Tools.Wpf\07Tools.Wpf.csproj -c Release -r win-x64 -p:PublishProfile=win-x64
```

The self-contained executable is produced at:

```text
src\07Tools.Wpf\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\07Tools.exe
```

Users do not need the .NET runtime installed. The repository-root `07Tools.exe` is a local shortcut target and is excluded from Git. A local `RunescapeTools.exe` compatibility copy keeps older desktop shortcuts working.

## Documentation and contact

- [Application requirements](APPLICATION_REQUIREMENTS.md)
- [Method assumptions and technical notes](docs/)
- [Report a bug or suggest a feature](https://github.com/charlie30093729/RunescapeTools/issues)
- Maintainer: **bottleo** on Discord.

This application is not affiliated with, endorsed, or otherwise approved by Jagex Ltd. RuneScape and Old School RuneScape are trademarks of Jagex Ltd.
