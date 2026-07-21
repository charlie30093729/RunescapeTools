# RunescapeTools — Application Requirements

| Field | Value |
| --- | --- |
| Document version | 1.2 |
| Application | RunescapeTools / GE Ledger |
| Status | WPF MVP baseline |
| Date | 21 July 2026 |
| Target platform | Windows 10 2004+ x64; .NET 8 WPF desktop application |

## 1. Purpose

This document defines the functional, data, integration, quality, and acceptance requirements for RunescapeTools, presented in the user interface as **GE Ledger**.

The application provides a focused personal workspace for Old School RuneScape Grand Exchange information. It intentionally limits general market noise by displaying favourite items and items required by registered money-making methods. It also supplies a reusable calculation model for GP per hour and future experience-per-hour or cost-per-hour features.

## 2. Product goals

The application shall:

1. Let a user maintain a small list of favourite Grand Exchange items.
2. Show current prices and seven days of hourly history without operating a continuous data collector.
3. Calculate GP per hour from explicit item inputs and outputs using current market prices.
4. Make money-making methods modular so that adding or removing a method does not require navigation or calculator rewrites.
5. Keep application, infrastructure, persistence, and calculation logic independent from both front ends.
6. Calculate level-aware XP goals, active hours, and reviewed GP/XP economics from a dated EHP catalogue.

## 3. Stakeholders and users

### 3.1 Primary user

The primary user is an Old School RuneScape player running the application locally to monitor selected items and estimate the profitability of personal money-making methods.

### 3.2 Maintainer

The maintainer adds or adjusts item mappings, money-making definitions, calculation rules, UI features, and external API compatibility.

### 3.3 External service owner

The OSRS Wiki real-time price API is an external dependency. The application must identify itself appropriately and avoid unnecessary requests.

## 4. Scope

### 4.1 MVP scope

- Native WPF executable as the primary front end.
- Generic Host-based dependency injection, configuration, and logging.
- Profile, Dashboard, Favourites, Money Makers, and XP Planner navigation areas.
- JSON-backed favourite persistence.
- OSRS Wiki item mapping, latest-price, and time-series integration.
- Seven-day hourly history graphs.
- Shared item-flow GP-per-hour calculator.
- Automatic discovery of concrete money-making method classes.
- Vyrewatch Sentinels as the first registered method.
- Startup warming and in-memory API caching.
- Graceful handling of temporary market-data failures.
- Self-contained, single-file `win-x64` distribution.
- A parked Razor/Blazor front end that remains buildable but receives no new UI work.

### 4.2 Out of scope for the MVP

- User accounts, authentication, or role management.
- Cloud-hosted favourite synchronization.
- Automated trading or interaction with a RuneScape account.
- A continuously running historical-price ingestion service.
- Price alerts or push notifications.
- Editing money-making method definitions through the UI.
- Formal investment, trading, or profit guarantees.
- Full economic coverage for every EHP training band.
- Mobile-native applications.

## 5. Definitions and business terms

| Term | Definition |
| --- | --- |
| Favourite | A Grand Exchange item explicitly selected by the user for display and history retrieval. |
| Instant buy | The latest high price reported by the Wiki price API. |
| Instant sell | The latest low price reported by the Wiki price API. |
| Midpoint | The average of the latest high and low prices. If only one side is available, that value is used. |
| Item flow | An item quantity consumed as an input or produced as an output. |
| Per action | A quantity multiplied by the method's configured actions per hour. |
| Per hour | A quantity already expressed as an hourly amount. |
| GE tax | The configured percentage deducted from taxable output value. |
| Weekly history | Price points from the latest seven-day window using one-hour API data. |
| EHP rate band | A dated skill XP/hour rate beginning at a configured XP threshold and ending at the next band. |
| Economic coverage | The portion of remaining XP whose resource inputs, outputs, and fixed costs have been reviewed. |

## 6. Functional requirements

### 6.1 Application shell and navigation

| ID | Requirement |
| --- | --- |
| FR-NAV-001 | The application shall provide navigation to Profile, Dashboard, Favourites, Money Makers, and XP Planner views. |
| FR-NAV-002 | The active navigation destination shall be visually identifiable. |
| FR-NAV-003 | The WPF layout shall remain usable at the 1100 × 720 minimum window size, using contained scrolling where necessary. |
| FR-NAV-004 | The application shall identify the market-data source in the interface. |

### 6.2 Dashboard

| ID | Requirement |
| --- | --- |
| FR-DASH-001 | The dashboard shall display the number of favourite items. |
| FR-DASH-002 | The dashboard shall display the number of registered money-making methods. |
| FR-DASH-003 | The dashboard shall display the seven-day history-window summary. |
| FR-DASH-004 | The dashboard shall show the current midpoint for each favourite when live prices are available. |
| FR-DASH-005 | The dashboard shall provide direct navigation to favourite history and money-making calculators. |
| FR-DASH-006 | A live-price failure shall not prevent the user's stored favourites from being shown as safe. |

### 6.3 Favourite management

| ID | Requirement |
| --- | --- |
| FR-FAV-001 | The user shall be able to search Grand Exchange item mappings by a partial, case-insensitive item name. |
| FR-FAV-002 | Search shall begin only after at least two non-whitespace characters are supplied. |
| FR-FAV-003 | Search results shall prioritize names that begin with the search term. |
| FR-FAV-004 | Items already present in the favourite list shall not be offered as add candidates. |
| FR-FAV-005 | The user shall be able to add a search result to favourites. |
| FR-FAV-006 | Duplicate favourite item IDs shall not be persisted. |
| FR-FAV-007 | The user shall be able to remove a favourite independently from selecting it. |
| FR-FAV-008 | The favourite list shall remain sorted by item name, ignoring case. |
| FR-FAV-009 | Adding an item shall select it and load its current price and weekly history. |
| FR-FAV-010 | Removing the selected item shall select the first remaining favourite when one exists. |
| FR-FAV-011 | Favourite selection and removal controls shall have distinct accessible names and keyboard actions. |

### 6.4 Market prices and history

| ID | Requirement |
| --- | --- |
| FR-MKT-001 | The application shall retrieve the OSRS Wiki item mapping for item search and display metadata. |
| FR-MKT-002 | The application shall retrieve the latest high, low, and timestamp values for priced items. |
| FR-MKT-003 | The application shall calculate a midpoint from available high and low values. |
| FR-MKT-004 | Requests for current prices shall return only the item IDs required by the calling view or method. |
| FR-HIST-001 | The application shall request one-hour time-series data for a selected favourite. |
| FR-HIST-002 | The application shall restrict the displayed history to points within the latest seven days. |
| FR-HIST-003 | The history view shall display the weekly percentage change when at least two valid midpoint values exist. |
| FR-HIST-004 | The history view shall display current midpoint, instant-buy price, instant-sell price, point count, and tracked volume. |
| FR-HIST-005 | The application shall render a graph only when at least two valid midpoint points exist. |
| FR-HIST-006 | Graph points shall expose local timestamp and GP value details. |
| FR-HIST-007 | The application shall warm latest prices and weekly history for persisted favourites during startup. |

### 6.5 Profile dashboard and shared profile state

| ID | Requirement |
| --- | --- |
| FR-PRO-001 | `Profile` shall be the first desktop navigation item without replacing Dashboard as the startup page. |
| FR-PRO-002 | The profile page shall accept a trimmed RSN through its input, load button, or Enter key. |
| FR-PRO-003 | The first profile visit shall persist and load `bottleo` when no selected RSN exists. |
| FR-PRO-004 | Only a successfully fetched and parsed RSN shall replace the selected profile or its persisted preference. |
| FR-PRO-005 | The current profile shall be exposed through an injectable application-level `ICurrentProfileContext`. |
| FR-PRO-006 | Profile changes and same-RSN refreshes shall publish a `ProfileChanged` notification. |
| FR-PRO-007 | The desktop profile context shall have one application-wide singleton lifetime. |
| FR-PRO-008 | The profile page shall show Overall rank, total level, total experience, retrieval time, and all current skills. |
| FR-PRO-009 | Each skill shall show its API-defined name, level, experience, and rank in the shared canonical order. |
| FR-PRO-010 | Loading shall be asynchronous and cancellable, and duplicate requests shall be disabled while one is active. |
| FR-PRO-011 | Account-not-found, timeout, network, and malformed-response failures shall be distinguishable and shall retain valid state. |

### 6.6 Money-making calculations

| ID | Requirement |
| --- | --- |
| FR-CALC-001 | Every money-making method shall define a slug, name, description, actions per hour, account count, tax rate, and item flows. |
| FR-CALC-002 | Each item flow shall identify the item ID, display name, quantity, direction, quantity basis, and tax applicability. |
| FR-CALC-003 | A per-action quantity shall be multiplied by the configured actions per hour. |
| FR-CALC-004 | A per-hour quantity shall be used without action-rate multiplication. |
| FR-CALC-005 | Input cost shall equal the sum of hourly input quantity multiplied by midpoint price. |
| FR-CALC-006 | Gross revenue shall equal the sum of hourly output quantity multiplied by midpoint price. |
| FR-CALC-007 | GE tax shall apply only to taxable output value. |
| FR-CALC-008 | Profit per account shall equal gross revenue minus output tax and input cost. |
| FR-CALC-009 | Total profit shall equal profit per account multiplied by the configured account count. |
| FR-CALC-010 | A missing item price shall be visibly reported and shall contribute zero GP to the estimate. |
| FR-CALC-011 | The calculation view shall display a line-by-line ledger for every input and output. |
| FR-CALC-012 | The calculation model shall support optional experience rewards without requiring a GP formula rewrite. |

### 6.7 XP Planner

| ID | Requirement |
| --- | --- |
| FR-XP-001 | The XP Planner shall show all 24 current skills in canonical Hiscores order. |
| FR-XP-002 | Start XP shall default to the successfully loaded profile, and every new per-profile goal shall default to 200,000,000 XP. |
| FR-XP-003 | Hours shall be summed across every EHP rate band intersecting the start-to-goal range. |
| FR-XP-004 | The user shall be able to edit start XP, goal XP, and personal XP/hour and reset starts from the loaded profile. |
| FR-XP-005 | A personal rate override shall scale all route-band rates proportionally without changing resource quantities per XP. |
| FR-XP-006 | Training inputs shall use instant-buy/high prices and outputs shall use instant-sell/low prices, with visible fallback when only one quote side exists. |
| FR-XP-007 | Unknown or unreviewed economics shall be labelled as unpriced and shall not be silently included as zero GP. |
| FR-XP-008 | Summary totals shall show remaining XP, active hours, priced net GP, and economic coverage. |
| FR-XP-009 | Construction shall include reviewed oak- and mahogany-plank quantities plus Demon Butler fees for the EHP bands beginning at level 33. |
| FR-XP-010 | Hours shall represent active player time; the first release shall not claim calendar completion time for time-gated methods. |

### 6.8 Method modularity

| ID | Requirement |
| --- | --- |
| FR-MOD-001 | A money-making method shall implement the `IMoneyMakingMethod` contract in the Core project. |
| FR-MOD-002 | Concrete method implementations shall be discovered and registered automatically at application startup. |
| FR-MOD-003 | Adding or removing a method class shall automatically update the available method list after rebuild and restart. |
| FR-MOD-004 | Domain calculations shall not depend on WPF, Razor components, or infrastructure-specific API types. |

### 6.9 Persistence and error handling

| ID | Requirement |
| --- | --- |
| FR-DATA-001 | Favourites shall persist locally in a JSON file under the configured data directory. |
| FR-DATA-002 | Favourite writes shall replace the target file atomically through a temporary file. |
| FR-DATA-003 | Concurrent favourite operations in one application process shall be serialized. |
| FR-DATA-004 | The desktop host shall allow only one running application instance per Windows user session. |
| FR-DATA-005 | On first launch only, the desktop host shall seed favourites from its embedded snapshot when no desktop file exists. |
| FR-DATA-006 | The first-run seed shall never replace an existing desktop favourites file. |
| FR-DATA-007 | When the renamed desktop data file does not yet exist, the host shall preserve an existing legacy favourites file by copying it to the new LocalAppData location before applying the seed. |
| FR-DATA-008 | The selected RSN shall persist atomically in `%LocalAppData%\RunescapeTools\data\profile.json`. |
| FR-DATA-009 | XP goals and overrides shall persist atomically per normalized RSN in `%LocalAppData%\RunescapeTools\data\training-plans.json`. |
| FR-ERR-001 | Item search, latest-price, history, and calculator failures shall produce user-readable messages. |
| FR-ERR-002 | Temporary API failures shall not delete or overwrite stored favourites. |
| FR-ERR-003 | HTTP 429 and server-error responses shall be retried up to three attempts with a delay. |
| FR-ERR-004 | Application shutdown cancellation shall not be logged as a startup-warming failure. |

## 7. Business rules

| ID | Rule |
| --- | --- |
| BR-001 | Midpoint = `(high + low) / 2` when both values exist. |
| BR-002 | When only one market side exists, midpoint equals the available value. |
| BR-003 | When neither market side exists, the item is treated as missing a price. |
| BR-004 | Weekly change = `(last midpoint - first midpoint) / first midpoint × 100`; it is omitted when fewer than two values exist or the first value is zero. |
| BR-005 | Tracked volume is the sum of high-side and low-side volume across displayed history points. |
| BR-006 | The MVP Vyrewatch method uses 102 actions per hour, five accounts, and a 2% output tax. |
| BR-007 | The MVP prices calculations using the current high/low midpoint, so results are estimates rather than guaranteed realized profit. |

## 8. External interface requirements

### 8.1 OSRS Wiki real-time price API

The application depends on these logical operations under the OSRS endpoint:

| Operation | Purpose |
| --- | --- |
| `mapping` | Searchable item IDs and metadata. |
| `latest` | Current high, low, and update timestamps. |
| `timeseries?id={itemId}&timestep=1h` | Hourly price and volume history for a selected item. |

Requirements:

- Requests shall accept JSON responses.
- Requests shall include an identifiable `User-Agent` configured through application settings.
- The HTTP client timeout shall be finite; the MVP target is 20 seconds.
- API schema changes shall be handled as integration failures rather than silently producing incorrect calculations.
- The application shall not require Wiki credentials or store third-party secrets.

### 8.2 Old School Hiscores API

- Normal accounts shall use `https://secure.runescape.com/m=hiscore_oldschool/index_lite.ws?player=X`.
- The RSN query value shall be URL-encoded.
- The response shall be parsed as headerless CSV with `rank,level,experience` for Overall and skill rows.
- The canonical shared mapping shall preserve the documented API order: Overall, Attack, Defence, Strength, Hitpoints, Ranged, Prayer, Magic, Cooking, Woodcutting, Fletching, Fishing, Firemaking, Crafting, Smithing, Mining, Herblore, Agility, Thieving, Slayer, Farming, Runecraft, Hunter, Construction, and Sailing.
- Activity rows following the complete skill block shall not be misinterpreted as skills.
- Missing, empty, incomplete, or malformed skill rows shall fail parsing rather than produce zero-level data.
- HTTP 404 shall be treated as account not found; other HTTP, timeout, and parsing failures shall remain integration errors.
- Requests shall use a finite timeout and the existing identifiable `RunescapeTools/0.1 (contact: Discord bottleo)` User-Agent.

### 8.3 Desktop interface

- The active interface shall be a native WPF application targeting `net8.0-windows10.0.19041.0`.
- The default window shall be approximately 1280 × 800 with a 1100 × 720 minimum.
- Feature behavior shall reside in CommunityToolkit.Mvvm view-models; code-behind shall be limited to initialization and application lifetime.
- Weekly history shall be rendered with LiveCharts2 WPF, with tooltip timestamps converted to local time.
- The Razor application shall remain in the solution as a buildable, parked front end and shall consume the same shared services.

## 9. Data requirements

### 9.1 Favourite item

| Field | Type | Constraint |
| --- | --- | --- |
| Item ID | Integer | Positive and unique within favourites. |
| Name | String | Required display name. |
| Added at | Date/time with offset | Stored in UTC when added by the application. |

### 9.2 Market data

- Item mappings contain item ID, name, examine text, membership flag, optional buy limit, and icon name.
- Latest prices contain optional high and low values and their optional update times.
- History points contain timestamp, optional average high and low, and high/low volumes.

### 9.3 Money-making method data

- Method slugs shall be stable and unique.
- Required item IDs shall be derived from distinct item flows.
- Monetary calculation values shall use decimal arithmetic after API integer prices are read.
- Experience rewards shall identify the skill and experience per action.

### 9.4 Local and generated data

- Mutable desktop state shall be stored under `%LocalAppData%\RunescapeTools`.
- Desktop favourites shall be stored at `%LocalAppData%\RunescapeTools\data\favourites.json`.
- Per-RSN training plans shall be stored at `%LocalAppData%\RunescapeTools\data\training-plans.json`.
- The EHP catalogue shall identify its source snapshot and verification date; rate changes shall be reviewed code/data changes rather than runtime scraping.
- The renamed desktop host shall copy an existing legacy favourites file into this location once, without overwriting new state.
- The WPF assembly shall embed the versioned MVP favourites snapshot as first-run seed data, including the current Scythe favourite.
- The parked Web host may retain its own `data/favourites.json`; it shall not share mutable state with WPF.
- ASP.NET data-protection keys are runtime-generated Web state and shall be excluded from Git.
- Build outputs and IDE-specific state shall be excluded from Git.

## 10. Caching and request-efficiency requirements

| ID | Requirement |
| --- | --- |
| NFR-CACHE-001 | Latest-price data shall be cached for approximately one minute. |
| NFR-CACHE-002 | Item mapping data shall be cached for approximately twelve hours. |
| NFR-CACHE-003 | Weekly history shall be cached per item for approximately fifteen minutes. |
| NFR-CACHE-004 | Concurrent cache refreshes of the same category shall be serialized within the application process. |
| NFR-CACHE-005 | Startup warming shall use bounded parallelism; the MVP maximum is three history requests at once. |

## 11. Non-functional requirements

### 11.1 Performance

| ID | Requirement |
| --- | --- |
| NFR-PERF-001 | Desktop navigation using cached data should update without recreating the application host. |
| NFR-PERF-002 | Search shall return no more than a small bounded result set; the MVP limit is eight items. |
| NFR-PERF-003 | The UI shall remain responsive while API requests are in progress and shall display loading states where appropriate. |

### 11.2 Reliability

| ID | Requirement |
| --- | --- |
| NFR-REL-001 | Failure of the external price service shall not prevent application startup. |
| NFR-REL-002 | Failure of startup history warming shall be logged as a warning and shall not terminate the application. |
| NFR-REL-003 | Local favourite writes shall avoid leaving a partially written target file. |

### 11.3 Security and privacy

| ID | Requirement |
| --- | --- |
| NFR-SEC-001 | The application shall not collect RuneScape credentials. |
| NFR-SEC-002 | Runtime data-protection keys shall not be committed to source control. |
| NFR-SEC-003 | No API secrets shall be embedded in source code or documentation. |
| NFR-SEC-004 | User-controlled values shall be rendered through normal WPF data binding or Razor encoding without being interpreted as markup. |
| NFR-SEC-005 | The application is intended for trusted local use unless a future deployment adds an explicit authentication and security design. |

### 11.4 Accessibility and usability

| ID | Requirement |
| --- | --- |
| NFR-A11Y-001 | Primary navigation and interactive actions shall be keyboard operable. |
| NFR-A11Y-002 | Remove actions shall expose an item-specific accessible name. |
| NFR-A11Y-003 | The price graph shall expose point values through keyboard/mouse tooltips and accompanying textual quote metrics. |
| NFR-A11Y-004 | Positive, negative, input, and output meaning shall not rely solely on position. |
| NFR-A11Y-005 | Focus indicators shall be visible for favourite selection and removal controls. |

### 11.5 Maintainability and testability

| ID | Requirement |
| --- | --- |
| NFR-MAINT-001 | Domain models and calculations shall remain in the Core project. |
| NFR-MAINT-002 | Market behavior and use cases shall remain in the Application project; external HTTP and JSON persistence shall remain in Infrastructure. |
| NFR-MAINT-003 | WPF view-models and Razor components shall consume shared services and domain results rather than reproduce calculation rules. |
| NFR-MAINT-004 | Shared dependency registration shall configure HTTP, persistence, caches, calculator, and discovered methods for either host. |
| NFR-TEST-001 | The regression harness shall cover calculation rules, caching, history filtering, search ordering, retries, warmup, JSON persistence, and WPF view-model behavior. |
| NFR-TEST-002 | A release candidate shall build with zero compiler errors. |

### 11.6 Compatibility

| ID | Requirement |
| --- | --- |
| NFR-COMP-001 | Shared and Web projects shall target .NET 8; WPF and its view-model tests shall target `net8.0-windows10.0.19041.0`. |
| NFR-COMP-002 | The supported desktop platform shall initially be Windows 10 version 2004 or newer on x64. |
| NFR-COMP-003 | The layout shall avoid page-level horizontal overflow at supported desktop widths. |
| NFR-COMP-004 | Tables may use contained horizontal scrolling when their readable minimum width exceeds the available viewport. |
| NFR-COMP-005 | The release executable shall be self-contained and shall not require the .NET Desktop Runtime on the target machine. |

## 12. Configuration requirements

| Setting | Purpose | Default behavior |
| --- | --- | --- |
| `OsrsWikiOptions.BaseAddress` | Selects the OSRS Wiki price API root. | `https://prices.runescape.wiki/api/v1/osrs/`. |
| `OsrsWikiOptions.UserAgent` | Identifies the application and maintainer to the Wiki API. | Uses the configured application value. |
| `OsrsWikiOptions.Timeout` | Limits an individual HTTP request. | 20 seconds. |
| `OsrsWikiOptions.MaxRetryAttempts` | Limits transient 429/server-error attempts. | Three attempts. |
| `FavouriteStoreOptions.FilePath` | Selects the host-specific JSON file. | WPF uses the LocalAppData path; Web uses its content-root data directory. |
| `FavouriteStoreOptions.SeedJson` | Supplies optional first-run seed content. | WPF passes the embedded snapshot; Web passes no seed. |
| `MarketDataOptions` | Controls latest, mapping, history, window, and warmup cache behavior. | Shared defaults. |
| `TrainingPlanOptions.FilePath` | Selects the per-RSN XP Planner JSON file. | WPF uses the LocalAppData data directory. |

### 12.1 Release packaging

- The WPF publish profile shall target `win-x64` with `SelfContained=true` and `PublishSingleFile=true`.
- Native libraries shall be included for self-extraction, symbols shall be embedded, and trimming shall remain disabled.
- The release artifact shall be named `RunescapeTools.exe` and shall include the GE monogram application icon.
- The supported release command is `dotnet publish src\RunescapeTools.Wpf\RunescapeTools.Wpf.csproj -c Release -r win-x64 -p:PublishProfile=win-x64`.

The maintainer shall replace placeholder contact information before public distribution or hosted deployment.

## 13. MVP acceptance criteria

The MVP is accepted when all of the following are true:

1. The solution builds successfully with no compiler errors.
2. The calculation regression harness passes all included checks.
3. The dashboard loads persisted favourites and live midpoint prices when the Wiki service is available.
4. The user can search for an item, add it, select it, see weekly history, and remove it.
5. Favourite changes persist across application restarts.
6. Weekly history contains only the latest seven-day window and renders at least two valid points when data is available.
7. The Vyrewatch method displays live-priced inputs, outputs, tax, per-account profit, and total five-account profit.
8. Adding another `IMoneyMakingMethod` implementation makes it available without a manual navigation registration.
9. Market-service failures display understandable fallback messages without deleting favourites.
10. The WPF views remain usable at 1100 × 720 without uncontrolled page-level overflow.
11. A second desktop instance exits cleanly without opening a competing favourites store.
12. First launch preserves an existing legacy favourites file when available, otherwise creates the LocalAppData file from the embedded snapshot; later launches do not replace it.
13. The `win-x64` Release publish produces a self-contained single-file `RunescapeTools.exe` with trimming disabled.
14. The parked Razor project continues to compile as part of the complete solution.
15. Runtime data-protection keys and build outputs remain ignored by Git.
16. XP Planner hours span every applicable level band and reproduce the reviewed Construction 0-to-200m benchmark within rounding tolerance.
17. Unpriced training segments remain visible through economic-coverage states, and per-RSN goals survive restart.

## 14. Risks and dependencies

| Risk or dependency | Impact | Mitigation |
| --- | --- | --- |
| Wiki API outage or throttling | Current prices, search, or history may be unavailable. | Cache responses, retry limited transient failures, and show fallback messages. |
| API schema or endpoint changes | Parsing can fail or data can become incomplete. | Keep integration isolated in `OsrsWikiPriceClient` and validate failures visibly. |
| Thinly traded items | Midpoint and history may be sparse or misleading. | Show unavailable states and require two valid points for a graph. |
| Current midpoint differs from realized trade price | GP/hour is an estimate. | Label the pricing rule and expose input/output line values. |
| JSON storage is local and single-instance | No cloud synchronization and limited multi-process coordination. | Enforce one WPF process per user session; introduce a database only when broader concurrency is required. |
| WPF, reflection-based discovery, LiveCharts2, and SkiaSharp are trimming-sensitive | A trimmed release may fail at runtime. | Keep `PublishTrimmed=false` until a separately tested trimming design exists. |
| Game mechanics or tax rules change | Method estimates become inaccurate. | Keep quantities and tax rates explicit in method definitions and regression tests. |

## 15. Future requirements candidates

- Additional reviewed GP/XP coverage for lower-level and alternative training methods.
- Secondary-XP session ownership and crediting without double-counting hours or GP.
- UI-driven method creation and parameter overrides.
- Price and profit alerts.
- Optional historical persistence beyond the Wiki API window.
- Import/export of favourites and method definitions.
- Database-backed local storage.
- Authentication and deployment hardening for hosted use.
- Automated unit, integration, accessibility, and browser test projects in CI.

## 16. Requirements governance

- Requirement IDs shall remain stable after publication.
- Changed behavior shall update this document and its affected acceptance criteria in the same pull request or commit.
- New features shall declare whether they extend MVP scope or a future release.
- Business-rule changes affecting profitability shall include regression coverage before release.
