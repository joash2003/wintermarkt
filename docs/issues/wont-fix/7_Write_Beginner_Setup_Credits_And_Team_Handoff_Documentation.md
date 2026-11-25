---
type: chore
priority: medium
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 7 — Write beginner setup, credits and team handoff documentation

- Dependencies: Can start immediately; update after verification.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Write beginner setup, credits and team handoff documentation. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Write beginner setup, credits and team handoff documentation.

## Where is the issue?

`Missing README.md, TESTING.md, ASSET_CREDITS.md, readme.txt, DESIGN_LOG.md and CHANGELOG.md` (paths relative to repository root).

## Observations

Only the original handoff and initial implementation files exist. No team names, external tester results, slide link or final submission evidence have been supplied.

## Tasks

1. Write README with installed Unity version 6000.6.4f1, macOS initial target, legacy input choice, Hub import, bootstrap menu, scene opening, Play controls, JSON edits and exact build/test commands.
2. Do not call this version LTS without verification.
3. Create testing status and known limitations, original-asset credits, release readme and design/change logs.
4. Include task ownership placeholders and slide-deck outline using handoff section 11.
5. Keep real names, weekly deck publication, playtests and Moodle submission pending until confirmed.

## Acceptance criteria

A new contributor can follow the documented setup. Every test/build claim links to evidence; asset provenance is accurate; unpublished slides and missing course/team information are explicitly marked pending.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 7-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17: Added README, TESTING, credits, release readme, design log and changelog. Beginner workflow is documented but requires real Unity verification; slide deck and team details pending.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
