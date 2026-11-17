---
type: bug
priority: high
created: 2026-10-20
assignee:
branch: 9-market-balance
closed: 2026-11-17
---

# 13 — Visitors arriving before anything is built count as welcomed and lost

- Dependencies: None

## Context

Visitors start arriving before the player has placed any building. Each one is counted as welcomed (the festival score) and immediately as abandoned, and each costs `abandonmentLoss` reputation. The opening seconds inflate the score while also punishing the player for not having built anything yet.

Expected: either no visitors arrive until the first building exists, or arrivals with nowhere to go do not count towards the score or cost reputation.

## Goal

Visitors only count towards the score and reputation once they can actually be served.

## Where is the issue?

Assets/_Wintermarkt/Core/MarketSimulation.cs, `SpawnVisitor()`: `Welcomed++` runs before a destination is chosen, and `chosen == null` then increments `Abandoned` and lowers `Reputation`.

## Observations

- From code reading, confirmed by the 18 Oct scripted run "build 1 stall, then do nothing" (80 welcomed, 8 abandoned, reputation 99).
- `chosen == null` also happens when every queue slot is full. Turning those visitors away may be intended, since full queues should cost reputation. Decide that case separately.

## Tasks

1. Decide the rule: hold arrivals until the first building exists, or do not count a visitor as welcomed until they reach a queue slot.
2. Change `SpawnVisitor()` (or the arrival timer) accordingly. Keep the full-queue case working as decided above.
3. Add a test in Assets/_Wintermarkt/Tests/MarketTestCases.cs: tick a market with no buildings for 30 s and require Welcomed, Abandoned and Reputation to be unchanged. Confirm the test fails before the fix.

## Acceptance criteria

- [ ] New test fails on the old code and passes on the fix.
- [ ] `tools/quality-check.sh` passes.
- [ ] Another contributor reviewed the change.

## Work log

- 2026-10-20: filed from the 18 Oct review of the simulation.
- 2026-11-17: test `NoArrivalsBeforeFirstBuilding` added and confirmed failing on the old code, then fixed.

## Outcome

The arrival countdown only runs once at least one building exists (MarketSimulation.Tick), so nobody is welcomed, lost or charged to reputation at an empty market. Covered by `NoArrivalsBeforeFirstBuilding` in MarketTestCases and the EditMode wrapper. Left as it was on purpose: a visitor who arrives when every queue place is taken still counts as arrived and lost and costs reputation, because full queues should hurt. Since issue 9 changed the score to visitors served, those visitors no longer add to the score.