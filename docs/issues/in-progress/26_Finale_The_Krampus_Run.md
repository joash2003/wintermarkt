---
type: feature
priority: high
created: 2026-11-17
assignee: Aidan
branch: 26-krampus
---

# 26 — Finale: the Krampus run

## Context

The game is played once, for about ten minutes, by each classmate and the professor. On 17 Nov 2026 Joash said it has no "wow", minute 9 looks like minute 2, and the player mostly watches. He approved a plan for one evening in three acts with busy hands, events and a finale (issues 23 to 26). The evening needs a finale that everyone remembers.

## Goal

In the last act a Krampus run crosses the market, scares visitors away, and the player drives it off with snowballs.

## Acceptance criteria

- [ ] Krampus figures enter at the gate and walk through the market in the last act.
- [ ] Visitors near a Krampus leave scared; that is counted and shown at the end.
- [ ] A click on a Krampus hits it; after a set number of hits it runs off.
- [ ] A core test covers scaring and driving off.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] Simulation and data in Core.
- [ ] What the player sees and clicks in Runtime.
- [ ] Tune with `bash tools/balance-bots.sh` and record the result in the design spec.

## Work log

- 2026-11-17: test `KrampusScaresAndIsDrivenOff` first (14 compile errors against the old code). Event kind `krampus`; `MarketKrampus` walks with the grid pathfinding to the nearest visitor, re-targets every 2 s and scares visitors within two cells unless they stand at a boosted stall; `TryHit` counts snowballs and sends it back to the gate at more than twice the speed. New mood `Scared`, counters `Scared` and `DrivenOff`. In the game: a dark horned figure with red eyes and one white ball per hit, a click near one throws a snowball before any other click action, a banner with the number still hunting, a line on the ending card and the verdict "The Krampus won" when some are still there at the end with fewer than two stars. market.json: five Krampus, three hits each, 100 s into act 3.
- Checked with the Play Mode render at 8x: three Krampus in the market with the banner, snowball markers above one, and the ending card with "5 Krampus driven off, 6 visitors scared away"; no exceptions. 24 core tests pass.
- Not checked: clicking a moving Krampus by hand (the hit radius is two cells and may be too generous or too tight), whether five with three hits is the right amount, readability of the dark figure on the dark ground.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._