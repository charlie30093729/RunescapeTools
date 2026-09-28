# Hunter chinchompa configuration

The dropdown labels are **Red chinchompas** and **Black chinchompas**. Method IDs
remain `red-chinchompas` and `main-ehp`, respectively, so saved selections and
personal rates require no migration. The earlier pre-chin route is retained.

The Hunter cog stores skill-wide settings per profile using the existing JSON
preferences. 3-ticking defaults on; the black-chin shooting alt defaults on.
The alt is unavailable for red chins, Herbiboar and Aerial Fishing. Turning off
3-ticking immediately unchecks/disables the alt and saves it as off. Turning
3-ticking back on leaves the alt off until explicitly selected. Reset restores
the enabled defaults. Herbiboar/Aerial Fishing calculations are unchanged.

## Presets

| Method at level 99 | 3-ticking | Shooting alt | XP/hour |
|---|---|---|---:|
| Red chinchompas | On | Not applicable | 210,000 |
| Red chinchompas | Off | Not applicable | 170,000 |
| Black chinchompas | On | On | 265,000 |
| Black chinchompas | On | Off | 225,000 |
| Black chinchompas | Off | Disabled | 200,000 |

The red 210k preset is retained from the user's reviewed route, now without a
separate red-alt requirement. The 170k non-tick preset is the user's explicit
planning rate, not a newly measured benchmark. The lower non-tick red bands use
61k/72k/115k/136k at levels 63/70/80/90 from the Wiki's dense Tlati-area table,
held until the next band. Location/quest access and player execution matter;
non-tick hunting is not an AFK method. No new horn-of-plenty or outfit bonuses
are assumed.

Black 3-tick rates use the Wiki's high-level 265k alt/225k solo benchmarks. The
non-tick preset is the user-approved 200k XP/hour planning estimate, approximately
635 catches/hour. It is not a confirmed Wiki benchmark or personally measured
rate. The generic money-making guide's 350-catch baseline was rejected as too
conservative for this efficient high-level preset.
These are deliberately coarse presets: the existing constant black-chin band
from level 73 is retained, not upgraded to a verified lower-level rate table in
this change. Personal rates can account for level, interruptions and execution.
The existing pre-unlock routes and their unpriced economics are unchanged.

## Calculation and architecture

Each successful red catch grants 265 XP and one red chinchompa (10034); black
grants 315 XP and one black chinchompa (11959). Outputs sell low after GE tax in
the selected Live/30-day pricing mode. Changes to technique affect time and
hourly GP, not the catches or total GP for a fixed XP goal. Alt ammunition,
tick-manipulation consumables, travel, deaths and lost chinchompas are excluded.

Custom rates are retained relative to the original enabled preset. For example,
a personal red rate of 105k (half the 210k default) becomes 85k when 3-ticking is
turned off (half 170k), returning to 105k when switched back on.

`Hunter/Global.cs` owns configuration, rate choices and throughput transforms.
Item definitions remain in their method classes. A reusable `RequiredToggleKey`
in the shared configuration schema enforces dependencies when loading settings;
the generic dialog reflects that rule immediately. No Hunter-specific parsing,
calculations or item rules are added to XAML or view models.

## Sources checked

- [Hunter training](https://oldschool.runescape.wiki/w/Hunter_training): black
  solo/alt tick-manipulation rates and red non-tick level bands.
- [Hunting black chinchompas](https://oldschool.runescape.wiki/w/Money_making_guide/Hunting_black_chinchompas):
  the generic money-making baseline, not the selected efficient non-tick preset.
- [Black chinchompa guide](https://www.gamingelephant.com/osrs-black-chinchompas-guide/):
  reports up to 220k without tick manipulation; not treated as a verified sustained
  solo benchmark. The selected 200k is an explicit user-approved assumption.

NUnit covers toggle combinations, dependent-option UI state, saved invalid-state
normalization, persistence/reset, custom-rate scaling, output quantities/tax,
pre-unlock preservation and isolation from other Hunter methods.
