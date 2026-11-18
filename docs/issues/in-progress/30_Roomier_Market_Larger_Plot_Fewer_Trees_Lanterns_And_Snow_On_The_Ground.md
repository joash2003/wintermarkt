---
type: feature
priority: high
created: 2026-11-18
assignee:
branch: 30-roomier-market
---

# 30 — Roomier market: larger plot, fewer trees, lanterns and snow on the ground

## Context

On 18 Nov 2026, after playing the build with the 70 m plot (issue 21) and the kit models (issue 18), Joash said the game feels "crammed, plain, boring, repeaty". This ticket covers the first two words. The 70 m plot was chosen so space would constrain the player; with cabins, trees and the Christmas tree in it, it reads as crammed instead.

## Goal

The market has visibly more room and its ground and paths are no longer one flat surface.

## Acceptance criteria

- [x] The plot radius is larger and most trees inside the plot are cleared, in `tools/build_site.py`.
- [x] The default camera shows more of the market.
- [x] Paths have lanterns, the plot edge has coloured lights, and the market ground has snow patches.
- [x] Bots still reach all stars with a mixed market.

## Tasks

- [x] Generator, camera, decoration, bot run, Play Mode render.

## Work log

- 2026-11-18: `PLOT_RADIUS` 70 -> 105 (41 x 29 cells, 535 buildable instead of 224) and two in three trees inside the plot cleared (9 left). Default zoom 45 -> 52. A lantern with a glow every 20 m along every path, alternating sides; coloured bulb strings from the kit between the posts on the plot edge; 46 snow patches from the kit on walkable market ground. Three more Holiday Kit models added (snow-flat, snow-flat-large, floor-stone; the last is not used yet).
- With the larger plot the bot that keeps adding buildings ran its reputation to zero, because more attraction brings more visitors than it can serve. `abandonmentLoss` 2 -> 1.5; at 1 reputation stayed high, arrivals rose and far more visitors gave up, so 1.5 was kept. Both mixed bots get 4 stars again. This undoes the tight-space idea of issue 21 on purpose.
- Checked with a Play Mode render (market view and wide view): no exceptions. 25 core tests pass. Not checked: a person playing it, frame rate with the extra props.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._