---
type: bug
priority: low
created: 2026-11-22
assignee:
branch:
---

# 46 — Stall name labels are oversized and cut off when the camera is close

## Context

Observed on 22 Nov 2026 in the pictures from `bash tools/unity.sh shots` (visitor.png, carousel-3.png in Logs/shots): the world-space name label of a stall ("Christmas Crafts - Level 1") is drawn at a fixed world size, so when the camera is close it fills the screen and is cut off at the edges, covering the stall and the visitors. At the default zoom (market.png) it reads fine. Not confirmed: how close a player zooms by hand; the pictures use the close-up camera of PlayModeShots.

## Goal

A stall label stays readable and inside the screen at every zoom the player can reach.

## Acceptance criteria

- [ ] At the closest zoom the label of a stall is no wider than the stall and fully on screen.
- [ ] At the default zoom it looks as before (market.png from 22 Nov).

## Tasks

- [ ] Scale the label with the camera distance in MarketWorld.cs, or cap its screen size.
- [ ] Re-run `bash tools/unity.sh shots` and compare visitor.png.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
