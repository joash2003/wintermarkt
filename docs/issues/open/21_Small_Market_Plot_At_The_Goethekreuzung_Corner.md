---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 21 — Small market plot at the Goethekreuzung corner

## Context

The buildable area is about 1,800 cells (the whole traced Volksgarten, issue 11) for a market of 10 to 30 buildings, so space never constrains the player and most of the screen is empty. Joash approved on 17 Nov 2026 to play only at the Goethekreuzung corner and keep the rest of the park as scenery. This is a decided deviation from "the whole park is playable" in docs/LOCATION_AND_TRANSIT.md.

## Goal

Only the park cells within a set radius of the Goethekreuzung entrance are buildable and walkable; the rest of the Volksgarten is drawn as scenery.

## Acceptance criteria

- [ ] `tools/build_site.py` has a `PLOT_RADIUS`; site.json is regenerated from it, never hand-edited.
- [ ] The grid covers only the plot; trees outside it are scenery features, so the park still has its trees.
- [ ] The plot is visibly marked in the game.
- [ ] The camera can still pan over the whole park.
- [ ] The radius is chosen with the bots so the best strategy runs out of space before it runs out of coins.

## Tasks

- [ ] Crop the grid and re-centre the features in build_site.py; add `plot` and `tree` features.
- [ ] Draw the plot and scenery trees in MarketWorld.cs; widen the camera clamp in MarketGame.cs.
- [ ] Update docs/LOCATION_AND_TRANSIT.md and the design spec.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
