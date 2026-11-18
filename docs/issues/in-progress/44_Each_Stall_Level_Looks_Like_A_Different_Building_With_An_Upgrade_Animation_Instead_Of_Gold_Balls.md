---
type: feature
priority: high
created: 2026-11-18
assignee: Joash Johnson
branch: 44-stall-levels-look-different
---

# 44 — Each stall level looks like a different building, with an upgrade animation instead of gold balls

## Context

Joash played the market on 18 Nov 2026 and does not like how upgrades look. Every building had three levels (issue 9), but a stall looked almost the same at every level: the same cabin with a few more decorations, and one to three gold balls floating over the roof to show the level. The carousel and the wheel did not change at all apart from the balls. The model was swapped instantly, so an upgrade had no moment.

Joash wants it like Clash of Clans: three levels, three buildings that clearly look different, and an animation each time a building goes up a level, instead of the gold balls.

The gameplay numbers stay as they are: cost, capacity, service time, income and attraction per level in `market.json` do not change, so the star targets, the core tests and the balance bots stay valid.

## Goal

At a glance the player can tell a level-1, level-2 and level-3 building apart by its shape, and each upgrade is a visible moment.

## Acceptance criteria

- [ ] No gold balls above buildings; the name label says "Level N".
- [ ] Stalls (Punsch, pastry, crafts): level 1 is a low shed, level 2 the proper Hütte, level 3 a bigger, taller grand Hütte with extra dressing. All stay inside their footprint.
- [ ] The carousel and the Ferris wheel gain seats, cabins and lights at each level.
- [ ] On upgrade the new building grows out of a burst of sparks over about half a second; newly placed buildings appear as before.
- [ ] `bash tools/unity.sh shots` saves close-ups of the Punsch Hütte at each level and one frame mid-upgrade.
- [ ] Core tests and balance bots unchanged.
- [ ] Joash has upgraded a stall by hand in Play Mode and agrees the three looks and the animation work.

## Tasks

- [ ] Three cabin looks, carousel and wheel looks, and the caption in `MarketWorld.BuildShop`.
- [ ] Upgrade pop and spark burst in `MarketWorld.Synchronize`; spark update shared with the fireworks.
- [ ] Upgrade close-ups in `Editor/PlayModeShots.cs`.
- [ ] Design doc: describe the three looks.

## Work log

- 2026-11-18: three cabin looks (4.5 m low shed without lights; 5.25 m Hütte with lights on three sides, candy cane and lantern; 6 m grand Hütte with wreath, presents, nutcracker, tree and a bigger sign), carousel 6/8/12 seats, wheel 8/12/16 cabins, gold balls removed, caption "Level N". Upgrade pop: the new model grows with an ease-out-back over 0.6 s out of 24 sparks; the spark update is now shared with the fireworks.
- Checked with `bash tools/unity.sh shots` on Windows: level-1/2/3 close-ups of the Punsch Hütte look different, nothing is outside the footprint, the whole evening still runs to the ending. A first try left level 1 without a roof (the snow-point piece is the roof); fixed. Core tests 25/25; balance bots identical to main (the table in the design doc was already stale before this change).
- Not checked: the pop animation itself. The shots run at a few frames per second, so the mid-pop frame shows the finished building and no sparks. The carousel and wheel levels were not pictured (the demo never builds them). Joash has not upgraded by hand yet.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._