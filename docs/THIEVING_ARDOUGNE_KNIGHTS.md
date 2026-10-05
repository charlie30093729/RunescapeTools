# Ardougne knights

Ardougne knights are an alternative Thieving route; Gem knights remain the
default with the existing `main-ehp` ID. Each method has its own class under
`Training/Skills/Thieving/Methods`. The new `ardougne-knights` ID uses the existing
dropdown, per-profile persistence, personal rates, reset and price-mode behavior.
No Thieving calculations are added to XAML or view models.

## Rate and equipment assumptions

The method requires level 55 Thieving (166,636 XP). Rates use the OSRS Wiki's
diary-only pickpocket experience chart, sampling the following levels and holding
each rate until the next band. Assume the medium Ardougne Diary's 10% success
bonus and full Rogue equipment; neither is inferred from the Hiscores profile.
No dodgy necklaces or Shadow Veil are assumed.

| Starting level | Starting XP | XP/hour |
|---|---:|---:|
| 55 | 166,636 | 82,369 |
| 60 | 273,742 | 93,894 |
| 65 | 449,428 | 105,889 |
| 70 | 737,627 | 121,154 |
| 75 | 1,210,421 | 139,152 |
| 80 | 1,986,068 | 158,557 |
| 85 | 3,258,594 | 184,303 |
| 90 | 5,346,332 | 216,281 |
| 95+ | 8,771,558 | 252,900 |

The 95+ preset is the tick-perfect ceiling of 3,000 successful pickpockets per
hour. Coin-pouch handling, movement, missed clicks and interruptions can reduce
the rate. Users can enter their measured rate. This is a click-intensive method.
The lower bands are step estimates rather than continuous per-level interpolation.

The existing Gem knights projection is retained below 55 for compatibility with
other catalogue routes. That inherited projection is not a verified early-level
training path and must not be interpreted as Ardougne knight access below 55.

## Coin calculation

Each success grants 84.3 Thieving XP and 50 coins, doubled to 100 by full Rogue
equipment. The shared calculator therefore uses fixed output of `100 / 84.3`
GP per XP, approximately **+1.18624 GP/XP**. Coins have no GE tax and need no live
or historical quote. The method adds no market-item API requests once unlocked.

At the 252,900-XP/hour preset this produces **300,000 coins/hour**. From level 99
(13,034,431 XP) to 200m, the route takes about **739.29 hours** and produces about
**221.79m coins**. A custom rate changes hours and hourly income, while the total
coins for a fixed XP goal remain the same.

Food/healing below 95, optional necklaces/spells, equipment acquisition, travel,
and pet value are excluded. Lower-level economics represent gross coin yield
before those costs. Full Rogue equipment is an explicit preset assumption rather
than a new configuration toggle. There are no secondary XP credits.

## Sources and verification

Checked on 2026-10-05:

- [OSRS Wiki experience-rate chart](https://oldschool.runescape.wiki/w/Template:Skilling_experience_rate_chart):
  the Knight of Ardougne diary-only dataset supplies the exact rate samples above
  and the 252,900 no-fail ceiling. The indexed chart was accessible; direct article
  access was blocked by the browsing service.
- [Knight of Ardougne](https://oldschool.runescape.wiki/w/Knight_of_Ardougne):
  level-55 access, 84.3 XP and 50 base coins. The article details were cross-checked
  through the [current reference copy](https://danielpgleason.com/osrs/reference/knight-of-ardougne-37a718e8/)
  and [Ardy knight calculator](https://runetools.com/calculators/ardy-knight).
- [Thieving training](https://oldschool.runescape.wiki/w/Thieving_training):
  full Rogue equipment doubles coins without extra XP and medium diary enables
  no-fail pickpocketing at 95. These details were checked against the
  [indexed article copy](https://osrsindex.com/wiki/thieving-training?site=osrs_wiki).

NUnit covers every sampled rate, the level-55 transition, a later rate transition,
fixed untaxed coin yield without quotes, post-99 totals, personal-rate effects,
selection/persistence/reset and stable unlock labels. Existing Gem knight tests
continue to cover its unchanged rate and Tokkul-to-onyx economics.
