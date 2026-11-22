---
type: feature
priority: high
created: 2026-10-17
assignee: Farhad Arezo
branch:
closed: 2026-11-22
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
- 2026-11-17: Joash asked to adjust happiness, objective and coins. Rebuilt the bot script and kept it this time (`bash tools/balance-bots.sh`, source in tools/tests/BalanceBots.cs). Baseline reproduced the 18 Oct result (0 abandoned, reputation 100 for every real strategy). Changes: score is visitors served instead of visitors welcomed (HUD, results screen, menu text, design spec); market.json maxVisitors 60 -> 200, baseArrivalSeconds 12 -> 8, minArrivalSeconds 2 -> 0.5, happinessGain 1 -> 0.5, abandonmentLoss 2 -> 3, varietyBonus 3 -> 15. Result table and plain-language rules are in the design spec under "How the numbers work". Attractions-only now loses (276 served, reputation 33) to mixed strategies (478 and 535 served). Not done: human playtest; reputation still saturates at 100; building costs and incomes untouched; 200 visitors on screen not checked for frame rate in Unity.

## Outcome

Shipped: five categories (Punsch Huette, Sweet Bakery, Christmas Crafts, Winter Carousel, Festival Wheel) with three levels each, arrivals from attraction and reputation, queue patience, four days with a results card (issues 19, 23, 24, 27, 32, 44 built on this). Pictures from `bash tools/unity.sh shots` on 22 Nov 2026 (Windows, Unity 6000.3.25f1, main at 31dc71e), kept in docs/submission/assets/shots-2026-11-22/. carousel-3.png shows the carousel at level 3, ending.png a finished evening with four stars; krampus.png in Logs/shots shows all five at level 3. Left out: task 5, comparing attraction purchases with upgrades in human playtests; that is issue 33 and the playtests 38 and 39.