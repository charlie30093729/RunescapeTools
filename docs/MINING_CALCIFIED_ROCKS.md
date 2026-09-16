# Calcified rocks — crystal pickaxe

## Rate preset

Low-attention, non-tick-manipulation Mining. The method is selected through Mining's existing dropdown; granite remains the default. No frontend-specific calculation is introduced.

| Mining level | Base preset | Default with Prospector |
| --- | ---: | ---: |
| 71–79 | 34,000 | 34,850 |
| 80–89 | 39,000 | 39,975 |
| 90–98 | 44,000 | 45,100 |
| 99+ | 49,000 | 50,225 |

The base presets adopt the [Wiki's non-tick-manipulation estimates](https://oldschool.runescape.wiki/w/Pay-to-play_Mining_training#Levels_41–99:_Calcified_rocks), not a measured crystal-only curve. For this app, they are treated as pre-outfit planning rates; the guide does not specify the exact setup for each row. The level-70 estimate is carried forward to 71; no extra crystal-speed multiplier is stacked on it. Full Prospector defaults on in the Mining cog and applies its 2.5% bonus, including to personal base rates. This is an explicit calibration assumption, not an independently verified 50,225 XP/hour benchmark.

The [crystal pickaxe](https://oldschool.runescape.wiki/w/Crystal_pickaxe) requires 71 Mining and Song of the Elves. Cam Torum access requires starting Perilous Moons. The rocks themselves unlock at 41, but this equipment-specific route retains existing Mining bands before 71 rather than pretending a crystal pickaxe is usable earlier. Quest completion is documented, not inferred from Hiscores.

## Quantities and GP

[Calcified rocks](https://oldschool.runescape.wiki/w/Calcified_rocks) yield 1–3 blessed bone shards on 74/75 successful rolls and one deposit on 1/75. The respective base XP awards are 33 and 36. The planner uses its shared multiplicative outfit model (without per-action XP-display rounding):

```
average XP per main-resource success = (33 + 3/75) × 1.025 = 33.866
successes = remaining method XP / 33.04
bone shards = successes × (74/75) × 2
unopened deposits = successes / 75
enhanced teleport seeds = successes / 15,000
```

Charging costs estimate one charge per successful main-resource roll. A [crystal shard](https://oldschool.runescape.wiki/w/Crystal_pickaxe#Charge_consumption) restores 100 charges; an [enhanced teleport seed](https://oldschool.runescape.wiki/w/Enhanced_crystal_teleport_seed) yields 150 shards. Seed quantities are proportional equivalents; the item popup rounds purchases up while GP calculations retain fractional consumption, consistent with the other crystal-tool routes.

Seeds (23959) use the selected live or 30-day buy-high price. Missing prices keep the method unpriced. Blessed bone shards (29381) and calcified deposits (29088) are shown as untradeable **GAIN** rows, never requested as GE-priced items or sold/taxed. Their identifiers were checked against Wiki item source data.

## Deliberate boundaries

- Full Prospector is the default; disabling it restores the base rates and 33.04 XP per expected success. No signet charge saving or pre-owned starting charges are assumed. The rate is a planning preset, not a success-chance simulation of equipment and watery veins.
- Tool purchase/creation, travel, rare gems and incidental rewards are excluded. The recharge estimate does not attempt to model rare gem pre-rolls.
- Deposits remain unopened. Their subsequent crushing, moth sales, extra shards and Smithing XP are not counted.
- Shards do not grant Prayer XP while mining. Offering time, wine and Prayer requirements must be modelled separately before assigning any secondary Prayer credit.
- User XP/hour changes affect hours and hourly cost/output, not the per-goal resource quantities or GP/XP.

## Structure and validation

Mining methods live in `Skills/Mining/Methods`, with shared crystal conversion constants, Prospector configuration and fallback assembly in `Global.cs`. Item identities remain method-local. The saved default-on outfit option applies only to Calcified rocks; existing granite rates, flows and saved `main-ehp` identifiers are preserved.

Tests cover level boundaries and fallback hours, high-price charging costs, reward quantities, missing prices, exclusion of untradeables from price discovery, custom rates, full-route integration, saved selections, reset, level-dependent labels and popup rendering.
