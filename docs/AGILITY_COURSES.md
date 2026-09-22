# Standalone Agility courses

Verified against the OSRS Wiki on 2026-09-22. Rates are planning estimates, not
personal in-game benchmarks. No offering spells or secondary-skill training are
included in these two Agility methods.

## Rates and access

| Course | Unboosted Agility | Default XP/hour |
|---|---:|---:|
| Prifddinas | 75-79 | 54,000 |
| Prifddinas | 80-84 | 58,000 |
| Prifddinas | 85-89 | 62,000 |
| Prifddinas | 90+ | 66,000 |
| Ardougne rooftop | 90+ | 70,000 |

Prif uses the course article's explicit rate table rather than its approximate
65k prose estimate or the training guide's differing 75-level estimate. The table
values are held until the next band. Portals and Song of the Elves are assumed;
the course becomes failproof at 91. Quest completion is disclosed, not checked
against Hiscores.

Ardougne uses a rounded efficient preset, beneath the perfect-lap ceiling of
approximately 70,184 XP/hour (889 XP per 45.6 seconds). Failures below 95 and
inattention can reduce performance. Boosted access is not modelled. Personal
XP/hour can be edited for either method.

The existing Sepulchre method remains the default with its original `main-ehp`
ID, rates and economics. Pre-unlock alternatives retain that legacy fallback
for compatibility. Its existing Floor 5 assumption is **not a verified low-level
training route**. Unlock labels distinguish the new course from the fallback.

## Rewards and GP

Prif expects 1,340.6 Agility XP and 0.94 crystal shards per lap. Shards (23962)
appear as untradeable gains, never direct GE income. All shards are assumed
converted at 97 Herblore: 2.5 super combat potion(4) (12695) per shard become
2.5 divine super combat potion(4) (23685). Inputs buy high; outputs sell low after
GE tax. Only the conversion margin is counted. Changing XP/hour changes hours,
not the shards or potion quantities needed for a fixed XP goal.

Ardougne assumes **no elite diary** and 18 marks/hour, a rounded estimate within
the Wiki's 16-18.1 base range. Ten marks buy 100 amylase crystals (12640), so the
ledger sells 180 crystals/hour, low after tax. Marks are time-gated rather than
a fixed reward per lap; a custom XP rate therefore changes total time and total
amylase. This fixed mark-rate approximation does not simulate spawn timers or
long idle periods. Elite diary uplift is not included.

Both methods use the planner's selected Live/30-day pricing mode. Missing market
quotes remain visibly unpriced. Travel, optional food/run-energy supplies,
exchange/conversion time and Herblore XP are excluded. Marks are not reserved
for buying graceful equipment.

## Prayer remains separate

Selecting a standalone Agility course never changes the Prayer selection, the
Prayer-at-Prif lap estimate, its spell inputs, its rewards, or its generated XP.
The existing projected Agility credit reduces the standalone Agility XP still
needed. That remaining XP alone produces standalone hours and rewards.

For example, the existing 100-lap Frost-bone Prayer scenario projects 134,060
Agility XP. With a remaining Agility goal of 200,060 XP, selecting high-level Prif
leaves 66,000 XP and one standalone hour, in addition to Prayer's own hours. Only
that standalone hour's rewards are added to Prayer's rewards. If Prayer covers
the entire Agility goal, standalone hours and rewards are zero. Switching Prayer
to Bank removes its Agility credit without changing the chosen Agility course.
The loaded profile's XP is never mutated by these projections.

## Structure and verification

Agility's catalogue composes one file per method under `Agility/Methods`.
`Global.cs` owns common level thresholds and pre-unlock route composition. All
calculations use existing shared services; no new view-model or XAML logic is
required. NUnit tests cover bands/unlocks, custom rates, pricing/tax, missing
quotes, method persistence/reset, labels after XP edits, and Prayer isolation.

## Sources

- [Prifddinas Agility Course](https://oldschool.runescape.wiki/w/Prifddinas_Agility_Course)
- [Ardougne Rooftop Course](https://oldschool.runescape.wiki/w/Ardougne_Rooftop_Course)
- [Mark of grace](https://oldschool.runescape.wiki/w/Mark_of_grace)
- [Amylase crystal](https://oldschool.runescape.wiki/w/Amylase_crystal)
- [Amylase pack](https://oldschool.runescape.wiki/w/Amylase_pack)
- [Crystal shard](https://oldschool.runescape.wiki/w/Crystal_shard)
