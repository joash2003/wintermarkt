---
type: feature
priority: medium
created: 2026-11-25
assignee: Joash Johnson Samuel
branch: 47-day-banner
closed: 2026-11-28
---

# 47 — Show a banner with the day name and date when each day starts

## Context

When playing on 25 Nov 2026, Joash found that a new day is easy to miss. The day name changes only in the small HUD line (`MarketUI.cs`, the `stats` text) and in the footer message (`MarketGame.cs`, on wave change). The red banner shows events only: tram, Krampus, snowstorm, power cut. Issue 27 promised "real dates", but `market.json` has only the day titles, no dates.

## Goal

Each day opens with the red banner showing its number, name and date for a few seconds, so the player notices the new day.

## Acceptance criteria

- [x] When a day starts (including day 1), a large card across the middle of the screen slams in, flashes, holds and fades within about 3 seconds: "DAY 2 / 4", "KRAMPUS NIGHT", "5 DECEMBER".
- [x] Each day shows off its own character on the card, set in `market.json` (`hero`, `pose`): day 1 a visitor nodding yes, day 2 the Krampus swaying, day 3 a visitor picking something up, day 4 a visitor jumping.
- [x] The card never blocks clicks, and the red event banner keeps working as before.
- [x] Each wave in `market.json` has a `date`: First Advent 29 Nov, Krampus night 5 Dec, Christmas Eve 24 Dec, New Year's Eve 31 Dec (2026).
- [x] The banner text fits at 1280x720 and 1920x1080.

## Tasks

- [x] Add `date` to the wave definition and to the four waves in `market.json`.
- [x] In `MarketUI.Update`, remember the last wave and show the day banner for 4 s when it changes.
- [x] Check in Play Mode (or `bash tools/unity.sh shots`) and note the result here.

## Work log

- 2026-11-25: `WaveDefinition.date` added and set for the four days in `market.json`. `MarketUI.Update` shows "Day N · title · date" in the red banner for 4 s whenever the day changes or a new festival starts, ahead of event text. Quality gate passes (compile, 28 core tests). Not checked yet: Play Mode, and the text size at 1280x720 and 1920x1080.

- 2026-11-25: the red-bar version was too flat, so the day now gets its own centre card in a round-banner style: the stripe opens from the centre (0.25 s), the title slams down from double size, a white flash on impact plus a second blink, hold, then fade and drift up from 2.6 s to 3.2 s, with the bell sound. The event banner is back to events only. Quality gate passes. Not checked yet: Play Mode, the feel, and the size at 1280x720 and 1920x1080.

- 2026-11-25: character showcase on the day card. `MarketWorld.ShowHero` builds the day's character on its own stage far off the market and films it with its own camera into a texture, which the card shows on the left, taller than the stripe, sliding in from the left and posing (its clip loops on unscaled time; the Krampus, which has no clips, sways). The Krampus figure is now built by one `Krampus()` method shared with the finale. Quality gate passes. Not checked yet: anything on screen, including whether the characters are lit well enough in the evening light and whether the transparent background works.

- 2026-11-28: `bash tools/unity.sh shots` now pictures each day card in its hold phase (Logs/shots/day-1.png to day-4.png) and ran twice with no exceptions, about 1.5 minutes each on Joash's PC. The first run showed the stripe, the title and the posed character with a clean cut-out, but the "DAY 1 / 4" and date lines were invisible: the panel skin redefines `gold` as berry red, so they were red on red. Fixed in ffed8b3 with an explicit gold; the second run shows all three lines and the gold edges. Seen: the Krampus is a dark silhouette with red eyes and horns, as in the market; the character overlaps the build menu on the left for the three seconds, which reads as intended. Not checked: 1280x720 and 1920x1080 (the pictures are 1600x900) and whether clicks pass through the card, which `CanvasGroup.blocksRaycasts = false` is meant to guarantee; both are a minute in the editor during issue 31.

- 2026-11-28, later: the shots run now also saves the day-1 card at 1280x720 and 1920x1080 (PlayModeShots asks the canvas scaler to recompute for the picture size first). Both pictures show the card and its three lines complete and in proportion: the canvas scales with screen size from a 1600x900 reference, so every 16:9 size lays out the same. Last box ticked.

## Outcome

Shipped: a day card that slams in, flashes, holds and fades within 3.2 s on unscaled time, with the day number, title and date on a red stripe and the day's character (market.json hero and pose) rendered by a second camera into a texture and animated with the Kenney clips; it never blocks clicks (CanvasGroup.blocksRaycasts off). Commits c8a5c6a ffed8b3 7cffad0 7b39b44 a7191c0 abd7266 2f33ce8 025d565 96f2990. Checked with the shots run on 2026-11-28: all four day cards at 1600x900, day 1 also at 1280x720 and 1920x1080, no exceptions; a colour bug found there (gold lines drawn in the panel's berry red) was fixed. Left out: a window that is not 16:9 was not pictured, and the click pass-through rests on the CanvasGroup setting rather than a test click.