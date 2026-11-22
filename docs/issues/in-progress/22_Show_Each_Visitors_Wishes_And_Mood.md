---
type: feature
priority: high
created: 2026-11-17
assignee: Joash Johnson Samuel
branch: 22-visible-wishes
---

# 22 — Show each visitor's wishes and mood

## Context

A visitor's wishes exist only in the simulation (`MarketVisitor.Wishes`, issue 19). The player sees a hint in the HUD for the most-missed category but cannot see what any single visitor wants or how they left, so there is nothing to react to on the map.

## Goal

The player can read each visitor's remaining wishes and how they left directly on the visitor.

## Acceptance criteria

- [ ] Each category has one colour, used for the building sign and for the wish markers.
- [ ] ~~A visitor shows one marker per remaining wish; a visitor leaving shows whether they were happy or gave up.~~ Dropped on 18 Nov 2026: Joash does not want markers above visitors.
- [ ] The build menu shows the category colour of each building.
- [ ] `MarketVisitor.Mood` is set by the simulation and covered by a core test.

## Tasks

- [ ] Mood enum and field in MarketSimulation.cs, test first.
- [ ] Category colours, wish markers and mood markers in MarketWorld.cs.
- [ ] Colour swatches on the build-menu buttons in MarketUI.cs.

## Work log

- 2026-11-17: test `MoodIsRecorded` first (9 compile errors against the old code). `VisitorMood` set in the simulation; `MarketWorld.CategoryColor` used for the lit sign and roof flag, the wish cubes above visitors and the build-menu swatches; visitors drawn 1.4 times larger so the cubes are readable.
- Checked with a Play Mode render of the scratch copy at 6x speed, with the HUD in the picture: wave line and "Visitors are missing" hint fit, wish cubes and swatches are visible at the default zoom, the plot edge and its string of lights show from far, and the loss screen appears when wave 1 is missed. The render also caught an out-of-range crash in `Plot()` (issue 21 code), fixed here.
- Not checked: a win screen, rides on screen, the Remove button by hand, a human playing.
- 2026-11-18: Joash played it and decided there should be nothing above visitors' heads, with no replacement. Removed the wish cubes and mood markers from MarketWorld.cs; `MarketVisitor.Mood`, the category colours and the build-menu swatches stay. The visitors stay 1.4 times larger.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._