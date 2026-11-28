---
type: feature
priority: medium
created: 2026-11-28
assignee: Farhad Arezo
branch:
---

# 49 — A stall catches fire: click to put it out, then rebuild it

## Context

Farhad proposed this in the team chat on 28 Nov 2026. The lecturer's feedback on 25 Nov was that the player has nothing to do; a fire is a hand task with a clear goal. The power cut event already works the same way ("click every stall to relight it"), so there is an event kind to copy in `MarketSimulation` and `MarketGame`.

## Goal

During a day a stall catches fire; the player clicks it several times to put the fire out, then pays to rebuild it, and until then it serves nobody.

## Acceptance criteria

- [ ] A new event kind `fire` in `market.json`: `count` clicks to put it out, a rebuild cost as a factor of the stall's build cost, scheduled per day like the other events.
- [ ] A burning stall serves nobody, shows flames and the clicks still needed; each click plays the click sound; its queue leaves with the normal abandonment loss.
- [ ] When the fire is out the stall stays closed until the player pays to rebuild it from the selection panel; the banner announces the fire like the other events.
- [ ] Core tests cover the clicks, the closed state and the rebuild; the bots handle a fire.
- [ ] Played by hand once with a Play Mode picture of a burning stall.

## Tasks

- [ ] Core: `fire` event kind, per-building `FireRemaining` / `Burnt` state, `TryExtinguish`, `TryRebuild`.
- [ ] Runtime: click handling in `MarketGame` next to the relight click, flames and counter in `MarketWorld`, Rebuild button in `MarketUI`.
- [ ] Data, tests, bot run.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
