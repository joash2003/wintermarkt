---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 25 — Evening events: school class, snowstorm, power cut

## Context

The game is played once, for about ten minutes, by each classmate and the professor. On 17 Nov 2026 Joash said it has no "wow", minute 9 looks like minute 2, and the player mostly watches. He approved a plan for one evening in three acts with busy hands, events and a finale (issues 23 to 26). Without events every minute of the evening plays the same.

## Goal

Timed events from market.json change the rules for a short while and are announced on screen.

## Acceptance criteria

- [ ] Events are data: act, second, kind, duration and values.
- [ ] School class: a burst of visitors who want food. Snowstorm: shorter patience and everyone also wants a hot drink. Power cut: every stall calls for help at once.
- [ ] The current event is shown as a banner; the snowstorm is visible.
- [ ] Core tests cover each event kind.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] Simulation and data in Core.
- [ ] What the player sees and clicks in Runtime.
- [ ] Tune with `bash tools/balance-bots.sh` and record the result in the design spec.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
