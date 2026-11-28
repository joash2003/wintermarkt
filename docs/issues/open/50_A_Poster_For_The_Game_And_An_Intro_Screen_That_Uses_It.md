---
type: feature
priority: medium
created: 2026-11-28
assignee: Farhad Arezo
branch:
---

# 50 — A poster for the game and an intro screen that uses it

## Context

Farhad offered on 28 Nov 2026 to make a poster for the game and the intro with it. The game currently opens on a plain menu overlay (`MarketUI`, `FestivalState.Menu`) with text only. The final presentation on 26 Jan and the design deck (issue 10) can use the poster too.

## Goal

A poster of the Linz Wintermarkt is the game's title screen, with the game name, a Play button and the team's names.

## Acceptance criteria

- [ ] The poster is a PNG in `Resources/Wintermarkt/UI`; the source file or its link is in `docs/`; everything used in it, AI-generated images included, is recorded in `ASSET_CREDITS.md`.
- [ ] The menu screen shows it full-screen at 1280x720 and 1920x1080 without stretching, with the Play button and the team's names readable on top.
- [ ] The team names match issue 41.
- [ ] A Play Mode picture of the title screen is in the ticket.

## Tasks

- [ ] Make the poster.
- [ ] Load it as the menu background in `MarketUI.Refresh` for the Menu state.
- [ ] Credits, picture, ticket.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
