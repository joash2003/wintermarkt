---
type: feature
priority: medium
created: 2026-11-17
assignee:
branch:
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

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
