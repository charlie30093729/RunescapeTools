# Planner pricing

## Ownership

- Skill/method definitions own resource IDs and quantities, not API calls.
- `TrainingSkillDefinition.MarketItemIds` discovers all method-band resources plus
  declared configuration extras, excluding untradeable resources.
- `PlannerPricingService` deduplicates those IDs with the selected money maker's
  effective definition. It delegates networking to the existing market service,
  reusing its HTTP client, User-Agent, retries and in-memory history cache.
- Core's `HistoricalPriceCalculator` computes separate high/low averages. Existing
  training calculations receive `ItemPrice` values with explicit pricing metadata.
- Infrastructure's `JsonPriceHistoryStore` owns the disk cache and saved mode.
- WPF owns the toggle, cancellation, loading/error messages and display only.

## Historical calculation

The window ends at the latest completed UTC six-hour boundary. Select its preceding
120 six-hour buckets, remove duplicate timestamps and reject misaligned timestamps.
Require at least 116 buckets (at most one day absent); otherwise the item is unpriced.

For each side independently:

`average = sum(bucket average price * bucket traded volume) / sum(bucket traded volume)`

Only positive prices with positive matching-side volume contribute. There is no
opposite-side fallback for historical quotes. Decimal GP is retained so averaging
cheap runes does not round away a material part of their cost. Prices are estimates
derived from the API's aggregated observations, not a reconstruction of every trade.

Inputs use the high average; outputs use the low average and existing applicable tax.
Independently weighted ingredient prices form a planning basket, not necessarily
the margin available at any single historical moment. Thirty-day averages can lag
genuine market changes; the mode does not forecast the cost of a future 200m grind.

## Requests, caching and failure handling

The existing all-items latest request cannot provide monthly history. The first
monthly selection requests six-hour timeseries per missing unique item, using
four concurrent requests at most. The existing history service filters to 31 days;
the calculator then selects the completed 30-day window.

Each item is saved in `data/price-history/{id}-6h-v1.json` under LocalAppData's
RunescapeTools directory; `settings.json` stores the global mode. Unique temporary
files and atomic replacement protect prior entries. Invalid JSON/invalid cached
history is a cache miss. No existing profile or training-plan format changes.

Fresh history is reused until the next UTC six-hour boundary. Refresh happens on
the next load, toggle, explicit refresh or money-maker change, not on a timer.
There is no automatic stale-to-live substitution. Cache reads and expired-history
refreshes can take time; cold loads are not promised to be instant. A cancelled
batch can leave independently completed cache entries available for retry.

The UI keeps the previous snapshot visible during a mode change, then replaces all
row prices on the UI thread before recalculating totals. Late cancelled requests
cannot replace newer results. On network failure it restores the applied toggle
and retains the previous snapshot with a warning. Legitimately insufficient
history yields explicitly unpriced items; an incompletely priced money maker is
excluded from total income rather than partially credited.

The money-maker selection passes its effective definition (including configured
inputs, custom action rate, accounts and tax), not just its precomputed live profit.
The planner uses high inputs/low outputs in both modes. The separate Money Makers
tab remains a live midpoint estimate, and is intentionally unaffected by this toggle.

## Verification

Automated tests exercise weighting, fractional prices, windows, duplicates, missing
sides, zero volumes, cache restart/expiry, atomic cancellation, corrupt files,
concurrency limits, request failures, toggle races, persisted mode, unchanged
quantities/hours and effective multi-account income. Existing WPF view construction
and the complete regression suite run alongside these tests.

A read-only live check on 2026-09-09 requested Aether rune (30843) with `timestep=6h`:
the Wiki returned 365 points, including all 120 completed monthly buckets with aligned
timestamps and `avgHighPrice`, `avgLowPrice`, `highPriceVolume`, `lowPriceVolume` fields.
This verifies response compatibility, not historical profit or future latency guarantees.

An isolated end-to-end check of the complete catalogue plus default Vyrewatch flows
loaded 128 unique items in 128 HTTP requests, with no incomplete items, in 7.70 seconds.
Recreating the pricing service and loading from its disk cache took 0.022 seconds
and made zero additional HTTP requests. These are one-run measurements on the
development machine, not promised user-facing timings. The check used a disposable
test cache, not the user's persisted application data.

API documentation: [OSRS Wiki real-time prices](https://oldschool.runescape.wiki/w/RuneScape:Real-time_Prices).
The documentation page was robots-restricted during this pass; the live API check
and existing client established the fields used by this implementation.
