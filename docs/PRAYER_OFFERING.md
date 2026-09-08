# Prayer offering assumptions

## Mechanics and sources

Bones use **Sinister Offering**, not the ashes-only Demonic Offering. A full cast
uses three bones, one blood rune (565), and one wrath rune (21880), granting three
times their burial Prayer XP and 180 Magic XP. The spell requires 92 Magic,
A Kingdom Divided, and the Arceuus spellbook. Its nine-tick cooldown gives a
theoretical ceiling above the practical **600 full casts/hour** bank default.

Sources consulted for this implementation:

- [OSRS Wiki: Sinister Offering](https://oldschool.runescape.wiki/w/Sinister_Offering)
  ([accessible article mirror](https://osrsindex.com/wiki/sinister-offering?site=osrs_wiki)):
  spell requirements, runes, XP, and practical bank casting rate.
- [OSRS Wiki: Prifddinas Agility Course](https://oldschool.runescape.wiki/w/Prifddinas_Agility_Course)
  ([accessible article mirror](https://osrsindex.com/wiki/prifddinas-agility-course?site=osrs_wiki)):
  course requirements, approximately 1,340.6 XP and 0.94 expected shards per lap.
- [OSRS Wiki: Crystal shard](https://oldschool.runescape.wiki/w/Crystal_shard):
  ten dust per shard and one dust per potion dose; four-dose potion conversion
  therefore uses 0.4 shards per potion.
- The user-supplied screenshot: approximately 633 hours for 200m Prayer XP with
  Frost dragon bones, 40m bonus Magic XP, and 37.24m bonus Agility XP.

Direct Wiki access was restricted in the research environment; the linked article
mirrors exposed the spell and course text. The **82-second lap is a calibrated
planning assumption**, not a documented or personally measured offering lap.

## Default rates

| Bones | Prayer XP/bone with spell | Bank XP/hour | Prif XP/hour |
|---|---:|---:|---:|
| Dragon bones (536) | 216 | 388,800 | 227,590 |
| Frost dragon bones (31729) | 300 | 540,000 | 316,098 |
| Superior dragon bones (22124) | 450 | 810,000 | 474,146 |

Rates in the table are rounded for display; calculations retain decimal precision.
Banking assumes 1,800 bones/hour. Prif assumes **24 bones/eight casts per lap**,
with **82 seconds per lap including banking**, 1,340.6 Agility XP, and 0.94 shards.
Superior bones require 70 Prayer (737,627 XP); new offering routes use dragon bones
below this threshold. Existing altar calculations are unchanged.

## Frost bones, 0 to 200m Prayer XP

- Bank: approximately 370.4 hours.
- Prif: approximately **632.7 hours**.
- Both: approximately 666,666.67 bones, 222,222.22 of each spell rune, and 40m Magic XP.
- Prif additionally: approximately 37,238,888.89 Agility XP and 26,111.11 crystal shards.
- Converting those shards uses approximately 65,277.78 super combat potion(4),
  producing the same quantity of divine super combat potion(4).

These are expected fractional quantities, not an instruction to purchase fractional
items. The item dialog rounds requirements upwards. The screenshot uses roughly
one shard per lap (27,778 total); this implementation uses the Wiki's 0.94 expected
shards instead. Consequently its GP result will not exactly reproduce the screenshot,
even with matching market prices. Live GP also includes the spell runes.

## Economics, requirements, and exclusions

- Bones and spell runes buy at current high quotes. Missing quotes leave the route
  visibly unpriced rather than granting free inputs.
- All Prif shards are assumed converted at 97 Herblore: buy super combat potion(4)
  (12695), sell divine super combat potion(4) (23685) at low quotes after GE tax.
- Shards (23962) are displayed as untradeable gains with no independent GE value;
  only the conversion margin contributes GP, so shards are not priced twice.
- Course assumptions require Song of the Elves and at least 75 Agility, but the
  efficient lap estimate does **not** model failures below the failproof level.
- No Zealot's robes, rune-saving equipment, run-energy supplies, conversion time,
  or Herblore XP from potion conversion are included.
- Magic/Agility/quest/Herblore requirements are disclosed assumptions, not automatic
  eligibility checks against the user's profile.
- Personal XP/hour overrides alter throughput/hours. Material and secondary-XP
  quantities remain tied to Prayer XP; a faster Prif override assumes faster laps
  with the same eight-cast lap pattern.

## Shared implementation and validation

Prayer's `Global.cs` owns the spell, lap, and conversion rules; each bone method
supplies its own bone ID, burial XP, and unlock threshold. Both configured options
use the existing shared resource and secondary-XP calculator. WPF renders the
existing configuration schema without special Prayer code in XAML or code-behind.

Secondary XP is projected onto the other skills' remaining goals and capped there;
it never edits the loaded Hiscores profile. Switching away removes the projection.
The selected offering location uses the existing per-RSN JSON configuration storage
and restores Gilded Altar on reset.

Tests cover each bone and location, the full Prif benchmark, high/low price flows,
missing rune prices, option-only quote discovery on initial load and refresh,
superior-bone unlocking, personal rate overrides, secondary-XP rounding, WPF
credit removal, item-dialog shard presentation, and JSON persistence.
