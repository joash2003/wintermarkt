---
type: feature
priority: medium
created: 2026-11-17
assignee:
branch: 18-kit-detail
---

# 18 — Stardew Valley-style detail pass for the market, visitors and park

## Context

On 17 Nov 2026 Joash described the look he wants: "we have to add a lot of details, my vision is like Stardew Valley". The current market is a geometric blockout: box huts, sphere trees, capsule visitors, flat ground and plain panels for the UI (see the snowy evening restyle in issue 11). It reads as a map, not as a cosy place.

Stardew Valley is a reference for the feeling only: a warm, hand-made, detailed small world where every object has character. Use original art. Do not copy its sprites, characters, music, fonts or interface (the design spec has the same rule for Clash of Clans).

What "like Stardew Valley" means in detail is Joash's to decide and is not written down yet. The list under Tasks is a starting proposal, not an agreed scope.

## Goal

The market, its visitors and the park look detailed and cosy enough that a first-time viewer describes the mood as warm and hand-made, without any change to the simulation rules.

## Acceptance criteria

- [ ] Joash has written or approved a short style note: 2D pixel art or stylised low-poly 3D, camera, palette, and three reference images with what to take from each.
- [ ] Each of the five buildings has a distinct, recognisable model or sprite per level, not a recoloured box.
- [ ] Visitors are recognisable people (body, head, winter clothes, a walk cycle) and show their mood (happy, impatient, leaving).
- [ ] The park has ground detail and props: benches, lamp posts, fences, a central Christmas tree, snow piles, footprints or similar.
- [ ] The HUD and menus use a consistent themed style, readable at 1280x720.
- [ ] Every asset is original or has an ASSET_CREDITS.md entry with source and licence.
- [ ] A screenshot before and after is recorded in this ticket, and the game still passes the quality gate and runs a full festival.

## Tasks

- [ ] Decide the art direction with the team first. 2D pixel art is the Stardew look but means replacing the 3D presentation in MarketWorld; stylised low-poly 3D keeps the current camera and code. This decision sets the size of everything below.
- [ ] Buildings: one model or sprite set per building and level, with awnings, signs, goods on the counter, chimney smoke.
- [ ] Visitors: characters with walk animation, varied coats and hats, mood icons above the head.
- [ ] Park dressing: props, varied trees, snow detail, the central Christmas tree from the design spec.
- [ ] Life and feedback: coin pop-ups on service, small particles (snowfall, steam from the Punsch hut), ambient sound and music.
- [ ] UI: themed panels, icons for coins, visitors, reputation and time, a readable build menu with pictures.
- [ ] Day and evening lighting pass once the assets exist.
- [ ] Split this ticket into smaller ones once the direction is decided; it is too large to do as one piece of work.

## Work log

- 2026-11-18: Joash asked for more detailed graphics. First slice, keeping the 3D camera (so the art-direction question in this ticket is answered in practice as stylised low-poly 3D, not 2D pixel art; the team has not discussed it). Added 23 models and the texture of Kenney's Holiday Kit 2.0 (CC0, credited in ASSET_CREDITS.md) under Resources/Wintermarkt/Kit with their .meta files. `MarketWorld.Prop()` loads a model, measures the kit's unit once and falls back to the old primitive shape when a model is missing. Now from the kit: stalls as a one-tile log cabin (serving window with awning, wreath wall, snowy gabled roof, coloured light string, more dressing per level), fir trees, one large decorated Christmas tree with presents at the tree nearest the middle of the plot, lanterns along the plot edge, snow piles, and snowmen, benches and reindeer beside some park trees. Visitors got a scarf, a beanie and a pom-pom from primitives.
- This departs from "original buildings and art" in the design spec for the listed props; the spec's rule against copying Clash of Clans or Stardew assets is untouched.
- Checked with Play Mode renders (close, wide, snowstorm, Krampus run) and a full four-day run at 8x: no exceptions, models at the intended size. Not checked: a player build (the models load from Resources, so they should be included, but no build was made), frame rate, the look of upgraded stalls and of the rides, which are still primitives.
- Still open in this ticket: proper models for the rides, visitors with a walk cycle, the Krampus, the city buildings, effects (steam, coin pop-ups), themed UI, and the team's decision on art direction.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._