---
type: feature
priority: high
created: 2026-10-17
assignee:
branch:
---

# 9 — Add market attraction, stall variety and rides

- Dependencies: issue 8.
- Progress: Implemented; playtest and balance review pending

## Context

Add market attraction, stall variety and rides is required for the new Christmas-market direction.

## Goal

Add market attraction, stall variety and rides.

## Where is the issue?

Assets/_Wintermarkt/Resources/Wintermarkt/market.json, Core/MarketSimulation.cs, Runtime/MarketWorld.cs and Runtime/MarketUI.cs.

## Observations

Five configurable stall/ride definitions now have three upgrade levels, attraction and category variety. The simulation includes unlock thresholds, queues, capacity, patience, abandonment and a ten-minute results screen; carousel and wheel blockout meshes animate. No playtest or balance pass has been recorded, and all site scenery remains provisional.

18 Oct 2026, scripted players against the real simulation with the committed market.json and site.json (the bot script was not kept):

| Strategy | Welcomed | Abandoned | Reputation |
|---|---|---|---|
| Build 1 stall, then nothing | 80 | 8 | 99 |
| Cheapest stall, repeatedly | 175 | 0 | 100 |
| Rides and big attractions, no upgrades | 211 | 0 | 100 |
| Upgrade the busiest stall first | 180 | 0 | 100 |
| Variety first, then upgrades | 197 | 0 | 100 |

With any real effort no visitor ever leaves a queue, so the attraction-versus-capacity decision never comes up and building attractions always wins. Suspected cause: `minArrivalSeconds` = 2 caps arrivals at one every 2 s, which never overloads the queues. Bots react perfectly, so humans will do somewhat worse, but the structure holds.

## Tasks

1. Build on the verified issue 8 slice. Add hot-drink, pastry and craft stall definitions with three proposed upgrade levels, costs, footprint, service duration, capacity and attraction.
2. Add a carousel and Ferris wheel using simple looping animation, queue/service behavior and visible level changes. Unlock the larger ride through a clearly displayed visitor milestone.
3. Implement a bounded arrival formula using attraction, category variety and reputation, with a minimum arrival rate for recovery. Add queue patience and transparent feedback when visitors abandon a queue.
4. Implement an adjustable festival timer and results screen reporting unique arrivals, completed services, queue abandonments and income. Pause must freeze all simulation timers.
5. Compare attraction purchases with service upgrades in actual playtests. Record numbers, player understanding and changes; adjust one or a few parameters per iteration. Verify the final market can finish and restart without stale visitors or money.

## Acceptance criteria

Players can place and upgrade all proposed stall/ride categories, understand queue pressure, and complete a festival session. Scoring counts each arriving visitor once. Attraction and capacity offer observable different benefits.

## Work log

- Implemented in the Wintermarkt source as part of the first prototype pass. Direct Unity API compilation and eight core tests pass; editor/runtime playtest and balance review remain pending.
- 2026-10-20: recorded the 18 Oct scripted balance runs under Observations. Next: tune arrival pressure until capacity matters, then confirm with a human playtest.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
