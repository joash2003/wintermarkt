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
- [ ] Each stall category is its own building (Joash, 18 Nov: "each category its own building and not all the stall look the same"), with three levels each: the Punsch Hütte a low log shed, the proper Hütte, then a bigger grand Hütte; the Sweet Bakery a cart on wheels, a gingerbread house with a smoking chimney and a red-and-white awning, then a bigger one with two chimneys and gumdrops; Christmas Crafts a table under a canopy, a wooden booth with a canvas roof and hanging baubles, then a taller booth with two rows of baubles, a lit beam and a star. All stay inside their footprint.
- [ ] The carousel is a fairground galloper after Joash's reference picture of 18 Nov (reindeer on chrome poles, a cream canopy with a white dome and warm bulbs along the eave, a red-rimmed deck) and grows from four to eight animals, with painted panels, lit poles, a second tier and a gold crown at the top levels.
- [ ] The Ferris wheel (Joash, 18 Nov: "do the same for the ferris wheel") stands on a low platform with a ticket booth instead of a hut: two A-frames meet at the axle, the gondolas hang upright as it turns, and it grows from eight to sixteen gondolas with lit spokes, rings of bulbs, lit frames and a glowing hub.
- [ ] On upgrade the new building grows out of a burst of sparks over about half a second; newly placed buildings appear as before.
- [ ] `bash tools/unity.sh shots` saves close-ups of the Punsch Hütte, a carousel and a wheel at each level and one frame mid-upgrade.
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
- 2026-11-18, later: Joash sent a photo of a real fairground carousel (white ornate canopy, painted panels, string lights, horses on chrome poles) and asked for the wheel to get the same treatment. Both rides lose the hut underneath. Carousel: cream deck with a red rim, chrome poles, the kit's reindeer as the animals (facing the way the deck turns), cream frieze, white dome, 16/24/32 bulbs on the eave; level 2 adds painted panels and a gold finial, level 3 lit poles, a second tier and a glowing crown; 4/6/8 animals, radius 4/4.6/5.2 m. Wheel: low platform with a ticket booth, two A-frames, half-length spokes, gondolas hanging upright (rotation reset each frame in `Synchronize`); 8/12/16 gondolas on a 5/6/7 m wheel; level 2 lights the spokes and hub and adds a ring of bulbs, level 3 lights the frames, adds a second ring and a big hub star. `PlayModeShots` now also places a carousel and (once 80 visitors are welcomed) a wheel, with coins topped up through reflection, and pictures each level.
- Checked on a scratch copy of the project (the editor had the real one open): `carousel-1..3`, `wheel-1..3` and the pop frames; all three levels of each ride are clearly different, nothing leaves the footprint, the mid-pop frame of the carousel caught the sparks, and the evening still runs to the ending. The core compile passes. The first scratch run failed with a "missing script" because the Library was copied from the running editor; a fresh import fixed it.
- Not checked: the rides in the real project in Play Mode, the wheel at a distance, and how close the carousel feels to the photo for Joash. Rides still turn at 20°/s, which is fast for a Ferris wheel.
- 2026-11-18, later still: Joash: "each category its own building and not all the stall look the same". The Punsch Hütte keeps the kit log cabin. The Sweet Bakery is now a gingerbread house from primitives (`MarketWorld.Bakery`): a cart on wheels with a red awning at level 1, a house with icing eaves, a smoking chimney, lit windows and candy canes at level 2, a bigger house with two chimneys, gumdrops and a snowman at level 3. Christmas Crafts is an open booth (`MarketWorld.Crafts`): a table under a flat canopy with a row of baubles at level 1, a wooden booth with a pitched canvas roof, hanging baubles, a wreath and a lantern at level 2, a taller booth with two rows of baubles, a lit beam, a star, a nutcracker and a tree at level 3. The glowing counter strip of the no-kit fallback no longer appears in front of these. `PlayModeShots` now upgrades each kind in a fixed order (punsch, pastry, craft, carousel, wheel), so the pictures are named `id-N` and `upgrade-pop-id-N`.
- Checked on the scratch copy: `pastry-1..3` and `craft-1..3` are three clearly different buildings each, distinct from the cabin and from each other; nothing leaves the footprint; the evening runs to the ending; the core compile passes.
- Not checked: the bakery's windows are hidden under the awning from the game camera; the booth's own star sits close to the roof flag. Joash has not seen the stalls in Play Mode yet.
- 2026-11-18: merged to main at Joash's request after he saw the pictures. The by-hand check in Play Mode stays open, as for issues 19 to 30 (issue 31).

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._