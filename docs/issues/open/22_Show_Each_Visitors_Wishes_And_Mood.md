---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 22 — Show each visitor's wishes and mood

## Context

A visitor's wishes exist only in the simulation (`MarketVisitor.Wishes`, issue 19). The player sees a hint in the HUD for the most-missed category but cannot see what any single visitor wants or how they left, so there is nothing to react to on the map.

## Goal

The player can read each visitor's remaining wishes and how they left directly on the visitor.

## Acceptance criteria

- [ ] Each category has one colour, used for the building sign and for the wish markers.
- [ ] A visitor shows one marker per remaining wish; a visitor leaving shows whether they were happy or gave up.
- [ ] The build menu shows the category colour of each building.
- [ ] `MarketVisitor.Mood` is set by the simulation and covered by a core test.

## Tasks

- [ ] Mood enum and field in MarketSimulation.cs, test first.
- [ ] Category colours, wish markers and mood markers in MarketWorld.cs.
- [ ] Colour swatches on the build-menu buttons in MarketUI.cs.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
