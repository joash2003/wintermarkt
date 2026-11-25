---
type: feature
priority: medium
created: 2026-11-25
assignee: Joash Johnson Samuel
branch: 47-day-banner
---

# 47 — Show a banner with the day name and date when each day starts

## Context

When playing on 25 Nov 2026, Joash found that a new day is easy to miss. The day name changes only in the small HUD line (`MarketUI.cs`, the `stats` text) and in the footer message (`MarketGame.cs`, on wave change). The red banner shows events only: tram, Krampus, snowstorm, power cut. Issue 27 promised "real dates", but `market.json` has only the day titles, no dates.

## Goal

Each day opens with the red banner showing its number, name and date for a few seconds, so the player notices the new day.

## Acceptance criteria

- [ ] When a day starts (including day 1), the banner shows for about 4 seconds, e.g. "Day 2 · Krampus night · 5 December".
- [ ] The day banner wins over event text while it shows; afterwards the banner behaves as before.
- [x] Each wave in `market.json` has a `date`: First Advent 29 Nov, Krampus night 5 Dec, Christmas Eve 24 Dec, New Year's Eve 31 Dec (2026).
- [ ] The banner text fits at 1280x720 and 1920x1080.

## Tasks

- [x] Add `date` to the wave definition and to the four waves in `market.json`.
- [x] In `MarketUI.Update`, remember the last wave and show the day banner for 4 s when it changes.
- [ ] Check in Play Mode (or `bash tools/unity.sh shots`) and note the result here.

## Work log

- 2026-11-25: `WaveDefinition.date` added and set for the four days in `market.json`. `MarketUI.Update` shows "Day N · title · date" in the red banner for 4 s whenever the day changes or a new festival starts, ahead of event text. Quality gate passes (compile, 25 core tests). Not checked yet: Play Mode, and the text size at 1280x720 and 1920x1080.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._