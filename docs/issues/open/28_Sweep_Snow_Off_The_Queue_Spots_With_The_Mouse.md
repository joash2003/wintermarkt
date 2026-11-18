---
type: feature
priority: high
created: 2026-11-18
assignee:
branch:
---

# 28 — Sweep snow off the queue spots with the mouse

## Context

On 17 Nov 2026 Farhad proposed "manually cleaning the snow with the mouse to make place for people". It fits the snowstorm event (issue 25) and the aim of keeping the player's hands busy (issue 23).

## Goal

During a snowstorm snow piles up on free queue spots and blocks them until the player sweeps it away with the mouse.

## Acceptance criteria

- [ ] A storm event with a count puts that many snow piles, one after the other, on free queue spots.
- [ ] A piled queue spot cannot be used by visitors.
- [ ] Holding the mouse button and moving over a pile removes it; that is counted.
- [ ] Piles are visible and the HUD says what to do.
- [ ] A core test covers piling, blocking and sweeping.

## Tasks

- [ ] Tests first where Core changes.
- [ ] Data, simulation and what the player sees.
- [ ] Bots, design spec, Play Mode render.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
