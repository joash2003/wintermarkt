---
type: bug
priority: low
created: 2026-10-20
assignee: Joash Johnson Samuel
branch:
---

# 14 — Queue positions do not move up

- Dependencies: issue 8 (needs the market running in Unity to judge)

## Context

A visitor keeps the queue slot they reserved until they leave, and is served at that slot rather than at the counter. When the visitor at the front is served, the people behind them do not step forward. The simulation still works, but on screen the queue may look frozen or out of order.

## Goal

Decide from a Play Mode check whether queues must visibly move up, and fix it if so.

## Where is the issue?

Assets/_Wintermarkt/Core/MarketSimulation.cs: slot reservation in `SpawnVisitor()` and service start in `StartService()`. Runtime/MarketWorld.cs draws visitors at their slot cell.

## Observations

Suspected from code reading on 18 Oct. Not yet observed in Play Mode, because the market has not run in Unity (issue 8).

## Tasks

1. Run the market in Play Mode and watch a queue of three or more visitors. Record whether it reads as wrong to a player.
2. If it does, move waiting visitors to the lowest free slot when a slot frees up, and start service only at slot 0. Keep `QueueSlotsAreUnique` passing and add a test that a freed front slot is refilled.

## Acceptance criteria

- [ ] Decision recorded after a Play Mode check.
- [ ] If changed: new test fails before and passes after; `tools/quality-check.sh` passes.
- [ ] Another contributor reviewed the change.

## Work log

- 2026-10-20: filed from the 18 Oct code review.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
