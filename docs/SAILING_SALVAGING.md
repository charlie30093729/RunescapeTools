# Sailing: active salvaging

## Planning assumptions

One dropdown route, **Salvaging**, progresses through the following estimates. One hook is operated by the player; from level 50 a second is crew-operated. Banking travel, boosts and tick manipulation are excluded. The source flags its rates as potentially outdated; these are not confirmed maximum rates. [Wiki training guide](https://oldschool.runescape.wiki/w/Sailing_training).

| Level | Wreck / hook | Base XP/hour |
| --- | --- | ---: |
| 15 | Small / bronze | 2,800 |
| 21 | Small / iron | 3,200 |
| 27 | Fisherman's / steel | 5,800 |
| 35 | Barracuda / steel | 11,000 |
| 44 | Barracuda / mithril | 14,500 |
| 50 | Barracuda / two mithril | 16,000 |
| 53 | Large / two mithril | 24,000 |
| 59 | Large / two adamant | 25,000 |
| 60 | Large / Jenkins / adamant | 30,000 |
| 64 | Pirate / adamant | 47,000 |
| 73 | Mercenary / adamant | 60,000 |
| 74 | Mercenary / rune | 70,000 |
| 80 | Fremennik / rune | 85,000 |
| 87 | Merchant / dragon | 95,000 |

Lower endpoints are chosen before Jenkins; these are conservative assumptions, not measured crew-specific curves. Missing iron-Fisherman and dragon-Fremennik rates retain the previous supported combination. Hook upgrades assume sufficient Construction and schematics; Jenkins requires Dragon Slayer I. [Hook requirements](https://oldschool.runescape.wiki/w/Salvaging_hook), [level unlocks](https://oldschool.runescape.wiki/w/Sailing/Level_up_table).

Extractor: level 73 Sailing/67 Construction, 250 XP every 63 seconds, adding approximately 14,286 XP/hour. Merchant combined: approximately 109,286, not a verified 130,000. [Extractor cadence](https://oldschool.runescape.wiki/w/Sailing_training#Crystal_extractor).

## Application behavior and scope

- The extractor is enabled by default for Salvaging only. A custom 90,000 base becomes approximately 104,286 while enabled; toggling off restores 90,000. The bonus is never multiplied by the personal-rate adjustment.
- Each goal is integrated across the rate bands it traverses. Extractor XP starts only at its unlock, even when the plan starts below that level.
- Method selection, configuration and personal base rate use existing per-RSN JSON persistence. The shared calculator supports additive rate bonuses; WPF only presents the resulting state.
- The old Gwenith method remains the default with unchanged pricing and rate. Its legacy pre-15 band is retained only to preserve existing early-plan behavior; it is not a claim that Gwenith is playable at those levels. Salvaging is labelled as locked until 15.
- Salvaging loot, equipment costs, consumables and extractor income remain **unpriced**, not zero-profit. No new item IDs or speculative market flows are registered.
- The extractor can also yield crystal shards after Song of the Elves; yield depends on waters. That income needs a separate economic model. [Crystal-shard mechanics](https://oldschool.runescape.wiki/w/Crystal_shard#Using_a_Crystal_extractor).
- The supplied Opulent Salvage screenshot implies approximately 96,899 average XP/hour across its 0–200m projection. It also enables Horizon's Lure and supplies its own loot assumptions. We do not treat those aggregate figures as a measured Merchant base rate, copy its rewards, or introduce a new Lure option in this change.

## Validation

NUnit coverage checks every band boundary, stable labels while changing XP, extractor unlock and cross-band hours, additive personal rates, repeated configuration toggles, preference restoration, reset behavior, unpriced loot, and unchanged Gwenith rates/flows. Full-solution tests also protect existing multiplicative outfit and Daeyalt configurations.
