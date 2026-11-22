---
type: feature
priority: high
created: 2026-11-17
assignee: Aidan
branch: 25-events
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

## Work log

- 2026-11-17: three tests first (18 compile errors against the old code). `EventDefinition` and `MarketSettings.events`; `RunEvents` in the simulation with `Upcoming` (warning), `ActiveEvent` (storm) and `LastEvent`. Burst visitors arrive 0.6 s apart and want only the event's category when one is set; the storm multiplies patience and adds its category to every new visitor's wishes; the power cut makes every open stall call with a longer window. Help calls now count down even when regular calls are switched off. HUD: a red banner with the warning, the running event and its seconds, or the event that just happened. Snowstorm drawn as falling cubes. Five events in market.json, listed in the design spec.
- Checked with a Play Mode render of the whole evening at 8x speed with a script answering help calls: pictures of a help call, the school-class warning, the snowstorm, the power cut with all stalls calling, and the ending card (1 star, "A long, cold evening"); no exceptions. Bots still get 3 stars with the events.
- Not checked: whether the events are fun or fair for a person; the banner at 1280x720.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._