---
type: feature
priority: high
created: 2026-11-18
assignee: Joash Johnson Samuel
branch: 28-snow-sweeping
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

## Work log

- 2026-11-18: test `SnowBuriesQueueSpotsUntilSwept` first (8 compile errors against the old code). A storm event's `count` is the number of piles; they land one after the other on free queue spots, going round the stalls. `SlotTaken` treats a piled spot as unusable; `TryClear` removes the snow on a cell and the cells next to it and counts it. In the game: white mounds on the spots, holding the mouse button and moving over them sweeps, the banner says so, the ending card counts the piles swept. Ten piles in the Christmas Eve storm.
- Checked with the Play Mode render at 8x: mounds in front of the stalls during the storm with the banner "sweep the snow!", and the four-star ending card with fireworks behind it (issue 27). The render showed the banner text and the card's numbers overflowing; the banner now shrinks its font to fit and the card uses four short lines. No exceptions. 25 core tests pass.
- Not checked: sweeping by hand (only the simulation call was exercised), whether ten piles is the right amount, the fireworks' size after the last tweak.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._