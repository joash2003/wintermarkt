---
type: chore
priority: high
created: 2026-11-18
assignee: Joash Johnson Samuel
branch:
---

# 37 — Player builds for macOS and Windows with the kit assets, and a frame-rate check

## Context

No player build has been made since the kit models, the animated visitors and the animation module were added on 18 Nov 2026. Models load from Resources, so they should be included, but that is unverified. The first real build on 20 Oct failed twice on shaders missing from the player (issue 8 work log). Up to 200 visitors each run their own playable graph. Intended owner by the team poll: Joash.

## Goal

A macOS and a Windows build of the current game start, look like the editor version and run smoothly.

## Acceptance criteria

- [ ] `bash tools/unity.sh build` succeeds on macOS and the app shows stalls, trees, visitors, tram, houses and the wooden UI, not fallback shapes or magenta.
- [x] A Windows build is made and started on a real Windows machine.
- [ ] Frame rate with a full market in the rush is measured and written down; if it is poor, animated visitors are capped.
- [x] Results are recorded in TESTING.md.

## Tasks

- [ ] Build and run both.
- [ ] Fix what is missing from the builds.

## Work log

- 2026-11-28: `BuildRelease` builds for the platform the editor runs on (Windows exe on Windows, macOS app on a Mac), commit e612cd6. `bash tools/unity.sh build` on Joash's Windows PC: 90 MB exe, no errors in the build log; started for 20 s, window alive, Player.log clean. Table in TESTING.md. Not done: a look at the running game (the screen capture caught the Start menu), the frame rate, and the macOS build, which needs a Mac.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
