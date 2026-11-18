---
type: feature
priority: high
created: 2026-11-18
assignee:
branch: 27-four-days
---

# 27 — Four short days with real dates instead of three acts, and fireworks at the end

## Context

On 18 Nov 2026 Farhad proposed days instead of one ten-minute block, a challenge for each day, around Christmas and New Year. The game is still played once for about ten minutes, so the days are short. Joash approved turning the three acts (issue 20) into four dated days.

## Goal

The evening becomes four short days (First Advent, Krampus night, Christmas Eve, New Year's Eve), each with its own challenge and star, in about ten minutes in total, ending with fireworks.

## Acceptance criteria

- [ ] market.json has four waves with these titles and the events spread over them: tram on First Advent, Krampus run on Krampus night, school class and snowstorm on Christmas Eve, last tram and power cut on New Year's Eve.
- [ ] The HUD, messages and ending card say "day" and show four stars.
- [ ] Fireworks play over the market when the last day ends.
- [ ] Star targets are set with the bots and recorded in the design spec.

## Tasks

- [ ] Tests first where Core changes.
- [ ] Data, simulation and what the player sees.
- [ ] Bots, design spec, Play Mode render.

## Work log

- 2026-11-18: market.json now has four waves: First Advent (120 s), Krampus night (150 s, x1.3), Christmas Eve (180 s, x1.6), New Year's Eve (150 s, x1.3), 600 s in total. Events: tram on day 1; four Krampus on day 2 (they run off when the day ends); school class and snowstorm on day 3; last tram and power cut on day 4. HUD and messages say "day", the ending card shows four stars and the days on two lines, and the verdict scales with the number of days. Fireworks (bursts of lit cubes) play over the market once the last day ends, and the ending overlay is lighter then so they can be seen. No Core rule changed except the Krampus leaving at day end, so no new test.
- Star targets from a bot run with targets at zero (the two mixed bots scored 7/30/75/72 and 10/24/71/78 happy per day): 5, 18, 48, 48, about two thirds of the weaker bot.
- Not checked yet: the fireworks and the four-star card in Unity (render follows with issue 28).

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._