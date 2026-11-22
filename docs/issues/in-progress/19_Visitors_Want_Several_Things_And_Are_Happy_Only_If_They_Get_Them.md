---
type: feature
priority: high
created: 2026-11-17
assignee: Joash Johnson Samuel
branch: 19-multi-stop-visitors
---

# 19 — Visitors want several things and are happy only if they get them

## Context

Each visitor goes to exactly one building, pays and leaves (MarketSimulation.SpawnVisitor). Consequences seen on 17 Nov 2026 from the code and the bot runs in issue 9: variety only matters through a flat attraction bonus, layout and walking distance barely matter, nobody needs the rides, and the player has few decisions after the first stalls. Joash agreed on 17 Nov to change this first, ahead of the other scope changes.

## Goal

A visitor arrives wanting two or three different kinds of thing, visits a building for each, and counts as a happy visitor (the score) only if they got all of them.

## Acceptance criteria

- [x] A visitor's wishes are categories from the buildings currently unlocked, chosen deterministically so runs stay repeatable.
- [x] After being served, a visitor walks on to the next wanted building instead of leaving.
- [x] A visitor who got everything counts as happy; one who leaves because something they wanted is not built counts as disappointed; one who gives up in a queue or finds every wanted queue full counts as left unserved, as before.
- [x] The score shown in the HUD and on the results screen is happy visitors.
- [x] The HUD tells the player which category visitors are missing most.
- [x] Core tests cover a visitor fulfilling two wishes and a visitor missing one; existing tests still pass.
- [x] `bash tools/balance-bots.sh` is re-run and the table recorded here and in the design spec.

## Tasks

- [x] Tests first in MarketTestCases.cs.
- [x] Wishes, the next-stop choice and the happy/disappointed counters in MarketSimulation; `maxWishes` and `disappointmentLoss` in MarketSettings and market.json.
- [x] HUD and results screen text.
- [x] Re-tune arrivals and income if visitors now pay two or three times.
- [x] Update "How the numbers work" in the design spec.

## Work log

- 2026-11-17: tests `VisitorVisitsEveryWish` and `MissingWishDisappoints` written first; they did not compile against the old code (no Happy, Disappointed or MostWanted), which is the failing state. Implemented wishes and `Continue()` in MarketSimulation, `maxWishes` and `disappointmentLoss` in MarketSettings. HUD shows happy visitors, reputation and the most-missed category; results screen and menu text updated. First bot run with the issue 9 numbers: almost nobody happy and reputation at 0 for every strategy, because the three stalls were not affordable at the start and rides were wanted after 10 arrivals. Tuned market.json: startingCoins 350 -> 400, baseArrivalSeconds 8 -> 10, minArrivalSeconds 0.5 -> 1, disappointmentLoss 0.5, carousel unlock 10 -> 25, wheel unlock 35 -> 80. Bot table and rules are in the design spec under "How the numbers work". Quality gate passes with 11 core tests.
- Not done or not checked: no run in Unity (Play Mode, HUD text fit, frame rate); no human playtest; incomes unchanged; the acceptance boxes are ticked from tests and bot runs only.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._