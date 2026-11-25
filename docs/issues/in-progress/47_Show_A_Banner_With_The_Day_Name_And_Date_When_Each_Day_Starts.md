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

- [ ] When a day starts (including day 1), a large card across the middle of the screen slams in, flashes, holds and fades within about 3 seconds: "DAY 2 / 4", "KRAMPUS NIGHT", "5 DECEMBER".
- [ ] Each day shows off its own character on the card, set in `market.json` (`hero`, `pose`): day 1 a visitor nodding yes, day 2 the Krampus swaying, day 3 a visitor picking something up, day 4 a visitor jumping.
- [ ] The card never blocks clicks, and the red event banner keeps working as before.
- [x] Each wave in `market.json` has a `date`: First Advent 29 Nov, Krampus night 5 Dec, Christmas Eve 24 Dec, New Year's Eve 31 Dec (2026).
- [ ] The banner text fits at 1280x720 and 1920x1080.

## Tasks

- [x] Add `date` to the wave definition and to the four waves in `market.json`.
- [x] In `MarketUI.Update`, remember the last wave and show the day banner for 4 s when it changes.
- [ ] Check in Play Mode (or `bash tools/unity.sh shots`) and note the result here.

## Work log

- 2026-11-25: `WaveDefinition.date` added and set for the four days in `market.json`. `MarketUI.Update` shows "Day N · title · date" in the red banner for 4 s whenever the day changes or a new festival starts, ahead of event text. Quality gate passes (compile, 28 core tests). Not checked yet: Play Mode, and the text size at 1280x720 and 1920x1080.

- 2026-11-25: the red-bar version was too flat, so the day now gets its own centre card in a round-banner style: the stripe opens from the centre (0.25 s), the title slams down from double size, a white flash on impact plus a second blink, hold, then fade and drift up from 2.6 s to 3.2 s, with the bell sound. The event banner is back to events only. Quality gate passes. Not checked yet: Play Mode, the feel, and the size at 1280x720 and 1920x1080.

- 2026-11-25: character showcase on the day card. `MarketWorld.ShowHero` builds the day's character on its own stage far off the market and films it with its own camera into a texture, which the card shows on the left, taller than the stripe, sliding in from the left and posing (its clip loops on unscaled time; the Krampus, which has no clips, sways). The Krampus figure is now built by one `Krampus()` method shared with the finale. Quality gate passes. Not checked yet: anything on screen, including whether the characters are lit well enough in the evening light and whether the transparent background works.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._