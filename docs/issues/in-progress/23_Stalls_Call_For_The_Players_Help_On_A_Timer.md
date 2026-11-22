---
type: feature
priority: high
created: 2026-11-17
assignee: Farhad Arezo
branch: 23-help-calls
---

# 23 — Stalls call for the player's help on a timer

## Context

The game is played once, for about ten minutes, by each classmate and the professor. On 17 Nov 2026 Joash said it has no "wow", minute 9 looks like minute 2, and the player mostly watches. He approved a plan for one evening in three acts with busy hands, events and a finale (issues 23 to 26). This part is his own idea: each stall needs the player's help on a timer, for example pouring drinks.

## Goal

Stalls with a queue call for help; a click in time boosts the stall, a missed call closes it for a while.

## Acceptance criteria

- [ ] A stall with visitors waiting calls for help at intervals set in market.json; the call lasts a set number of seconds.
- [ ] Helping in time makes the stall serve twice as fast for a while and pays a tip; a missed call closes the stall for a while.
- [ ] Each stall has its own timer, so more stalls mean more calls; calls come more often in later acts.
- [ ] The call, the boost and the closed state are visible on the stall, and a click on a calling stall helps it.
- [ ] With `helpIntervalSeconds` 0 nothing changes, so older tests keep their meaning.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] Simulation and data in Core.
- [ ] What the player sees and clicks in Runtime.
- [ ] Tune with `bash tools/balance-bots.sh` and record the result in the design spec.

## Work log

- 2026-11-17: three tests first (23 compile errors against the old code). Per building: `HelpRemaining`, `BoostRemaining`, `ClosedRemaining` and its own staggered timer; `TryHelp`; a closed stall starts no service and a boosted one serves in half the time. Settings in market.json: a call every 25 s (divided by the act's helpFactor 1 / 1.4 / 1.2), 8 s to answer, 15 s boost, 10 s closed, 5 coins tip. In the game a click on or near a calling stall helps it; the stall shows a pulsing yellow ball with a shrinking red ring, a CLOSED slab or a green boost light. Bots answer after about 3 s and miss one call in four; the same strategy that never helps loses in act 2 (44 happy against 213). 19 core tests pass. Not checked yet: the look and the click in Unity.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._