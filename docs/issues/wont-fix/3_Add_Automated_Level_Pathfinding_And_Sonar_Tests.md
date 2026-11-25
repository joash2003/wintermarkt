---
type: chore
priority: high
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 3 — Add automated level, pathfinding and sonar tests

- Dependencies: issue 2 for importable assets.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Add automated level, pathfinding and sonar tests. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Add automated level, pathfinding and sonar tests.

## Where is the issue?

`Assets/_EchoMaze/Tests/Editor/ (currently empty); Scripts/LevelData.cs` (paths relative to repository root).

## Observations

Parser, BFS and SonarEnergy are written but no automated tests or Unity test results exist.

## Tasks

1. Create runtime and EditMode test assembly definitions with correct Test Framework references.
2. Test all three Resources levels and dimensions, missing/duplicate P or X, unknown symbols, ragged rows, blocked exits, enemy count/order and invalid waypoints.
3. Test BFS wall avoidance, unreachable target and start==destination.
4. Test energy cost once, rejected requests, cooldown boundary, recharge and clamping.
5. Add hearing boundary tests after extracting a small testable hearing predicate if needed.
6. Run Unity EditMode tests and retain XML/log evidence.

## Acceptance criteria

All tests pass in Unity 6000.6.4f1. Commit test source and record actual command, result counts and log locations in TESTING.md; do not label source inspection as a test pass.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 3-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17: Added runtime/editor/test assembly definitions and CoreTests.cs. Tests have not executed: Unity startup blocked. Independent Python map validation passed for all three levels; this does not verify C#.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
