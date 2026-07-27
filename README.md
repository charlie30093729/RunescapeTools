# RunescapeTools

RunescapeTools, presented as **GE Ledger**, is a native Windows desktop workspace for Old School RuneScape profiles, market tracking, GP/hour calculations, and level-aware XP planning.

The WPF executable is the active front end. The original Razor/Blazor application remains buildable in the solution as a parked reference implementation. Both hosts use the same application, infrastructure, persistence, market-data, and calculation services.

## Current features

- Dashboard summary with saved favourites, current midpoint prices, registered calculators, and history coverage.
- Shared Profile Dashboard with normal-account OSRS Hiscores lookup, all 24 current skills (including Sailing), refresh, and last-profile restore.
- Debounced Grand Exchange item search with add, select, and remove favourite actions.
- Seven days of hourly Wiki price history rendered with LiveCharts2, including local-time tooltips, weekly change, and volume.
- Automatically discovered money-making methods with live repricing, adjustable per-method account quantities, and a complete input/output ledger.
- XP Planner with 21 planned skills, level-banded Main EHP rates, current-profile start XP, 99/200m goals, editable personal rates, active-hour totals, and per-RSN persistence. A selected Money Makers method can be allocated to specific skill hours through the clickable icon bar and included in Priced Net GP. Attack, Strength, and Hitpoints remain visible in profiles but are omitted from the planner as zero-time skills.
- Live GP/XP economics with explicit coverage states and reviewed processing, gathering, combat, Runecraft, Hunter, Construction, and Gwenith Glide routes.
- Shared Herblore potion economics include Prescription goggles, charged Alchemist's amulet output, and the live cost of Amulets of chemistry used for charges; reviewed Saradomin brews use this model.
- Vyrewatch Sentinels method with supplies, output tax, per-account profit, and an adjustable all-account total.
- Local JSON persistence, API-friendly caches, bounded history warmup, retry handling, and user-readable failure states.
- Single-instance desktop lifetime to prevent competing favourite-file writes.

## Project structure

| Project | Responsibility |
| --- | --- |
| `RunescapeTools.Core` | Domain records, API contracts, profile models, calculation rules, and money-making definitions. |
| `RunescapeTools.Application` | Market behavior, defensive hiscore parsing, current-profile state, and favourite-history warmup. |
| `RunescapeTools.Infrastructure` | Wiki and Hiscores HTTP clients, JSON persistence, configuration, per-skill training catalogues, and shared DI registration. |
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

XP goals, start overrides, and personal rates are stored per RSN at:

```text
%LocalAppData%\RunescapeTools\data\training-plans.json
```

The same training-plan file stores which skill hours are allocated to money making. The selected Money Makers method itself is session state: choose a successfully priced method in Money Makers, use its account arrows to set the currently running quantity, click the XP Planner summary card to change it, or use the card's Reset action to deselect it. Planner income uses the selected method's combined GP/hour across all chosen accounts and applies it exclusively to the active hours of highlighted skills.

The bundled EHP catalogue is a dated snapshot. Its level bands calculate the complete path from the selected start XP to the goal; GP totals clearly report how much of that path has reviewed economic data rather than treating unknown costs as zero.

Reviewed economic routes now include the first deterministic processing batch plus Grand-Coffin Hallowed Sepulchre, 1.5t teak Woodcutting, crystal-harpoon Fishing, 3t4g granite Mining, shooting-alt black chinchompas, solo mud Runecraft, Construction, rosewood-hull Gwenith Glide, efficient tree runs, defensive black chinchompas with cannon, zero-time Ranged, residual Ice Barrage, explicit break-even Slayer, and Gem Knights. Gem Knights value only expected Tokkul converted into live-priced uncut onyx at the Karamja-gloves shop rate; gems and variable trip consumables are excluded. Method notes disclose excluded outputs and calibrated assumptions; Agility values the Grand Coffin's expected tradeable loot and coins while excluding marks and clues, crystal-tool routes budget whole enhanced crystal teleport seeds, and Sailing values the reviewed shard-to-divine-potion conversion at live prices.

The Slayer route contributes a pending Magic XP credit to the plan and uses an explicit reviewed assumption of `0 gp/xp`. That credit reduces the Magic XP and Ice Barrage supplies still required after the planned Slayer goal, without changing the successfully loaded profile XP. Ranged and Magic retain live GP/XP calculations but contribute zero active hours to the planner total.

Farming economics buy the highest unlocked tree-run saplings and protection payments at live high prices and include gardener clearing fees. The reviewed schedule assumes one six-tree and six-fruit-tree run per day, four hardwood patches normalized by growth time, daily calquat and celastrus trees once unlocked, and the redwood patch normalized by its growth time. Efficient runs do not harvest or value fruit, bark, or logs; early quest XP remains visibly unpriced.

Each skill owns a catalogue file under `src\RunescapeTools.Infrastructure\Training\Skills`. `MainEhpCatalogue` only composes those definitions in canonical Hiscores order, while shared item IDs and construction helpers remain centralized. Training definitions expose a stable default method ID and can accept additional named method routes without returning to a monolithic catalogue.

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
