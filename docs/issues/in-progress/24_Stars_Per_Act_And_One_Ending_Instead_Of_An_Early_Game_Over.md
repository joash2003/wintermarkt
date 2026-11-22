---
type: feature
priority: high
created: 2026-11-17
assignee: Joash Johnson Samuel
branch: 24-stars-ending
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

## Work log

- 2026-11-17: issue 20's two wave tests rewritten and `ReputationZeroEndsTheEvening` added first (10 compile errors against the old code). A met target gives a star and the evening always runs to the end of the last act; `Failed` now only means reputation reached zero. `WaveResults` records happy visitors per act. Ending card: a verdict by stars, the stars as diamonds, happy visitors, the result of each act, calls helped, the other numbers; HUD shows the stars so far. `abandonmentLoss` 3 -> 2, because at 3 a reasonable bot dropped to zero reputation in the rush and the evening ended in act 2, which is what this ticket is meant to prevent.
- Bots: both mixed strategies that answer help calls get 3 stars; the one-sided ones play the whole evening with 0 stars; the mixed strategy that never helps closes in act 2.
- Not done: the big tree lighting up at the end. Not checked yet: the ending card in Unity.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._