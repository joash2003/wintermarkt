---
type: chore
priority: high
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 5 — Run full game-flow and standalone smoke tests

- Dependencies: issue 1, issue 2, issue 3, issue 4; issue 6 for standalone checks.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Run full game-flow and standalone smoke tests. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Run full game-flow and standalone smoke tests.

## Where is the issue?

`Scripts/GameManager.cs; Scripts/PlayerController.cs; all generated scenes; TESTING.md` (paths relative to repository root).

## Observations

No Unity compilation, Play Mode run or standalone build has been verified. Existing scripts alone are not evidence of a playable game.

## Tasks

1. After issue 1 through issue 4, import in Unity and resolve compiler/runtime errors.
2. Follow handoff section 13.2: movement/diagonal speed/walls, sonar radius/timers/energy, enemy hearing/sight/capture, retry reset, pause timing, mute, all levels and victory/replay.
3. Add meaningful PlayMode smoke tests.
4. Record each check with actual result and evidence; create separate defect tickets for failures.
5. Repeat the flow in the exported macOS build after issue 6.
6. External playtests remain separate and must use real participants.

## Acceptance criteria

All core checks have recorded results, no known P0/P1 defects remain, and an exported macOS build completes all three levels. Human playtesting and unperformed checks remain explicitly pending.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 5-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
