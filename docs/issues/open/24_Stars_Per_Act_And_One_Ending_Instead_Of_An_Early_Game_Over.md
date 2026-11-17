---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 24 — Stars per act and one ending instead of an early game over

## Context

The game is played once, for about ten minutes, by each classmate and the professor. On 17 Nov 2026 Joash said it has no "wow", minute 9 looks like minute 2, and the player mostly watches. He approved a plan for one evening in three acts with busy hands, events and a finale (issues 23 to 26). A player who sees the game once must not be sent home after the first act, which the early game over from issue 20 does.

## Goal

Missing an act's target costs a star instead of ending the evening, and the evening ends with one card that reflects how it went.

## Acceptance criteria

- [ ] A missed target does not end the festival; each act met gives a star.
- [ ] The evening ends early only if reputation falls to zero.
- [ ] The ending card shows 0 to 3 stars, a one-line verdict and the numbers of the evening.
- [ ] The tests from issue 20 are changed to the new rule.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] Simulation and data in Core.
- [ ] What the player sees and clicks in Runtime.
- [ ] Tune with `bash tools/balance-bots.sh` and record the result in the design spec.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
