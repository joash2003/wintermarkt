---
type: bug
priority: high
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 4 — Verify and correct enemy route changes and hearing

- Dependencies: issue 1, issue 2.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Verify and correct enemy route changes and hearing. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Verify and correct enemy route changes and hearing.

## Where is the issue?

`Assets/_EchoMaze/Scripts/EnemyController.cs` (paths relative to repository root).

## Observations

Enemy routing intentionally waits for tile centers before repathing. Hear changes state and target while an existing route remains. Runtime response, corner handling, wall sight blocking and chase priority have not been tested. This is a verification task, not a confirmed runtime defect.

## Tasks

1. After issue 1 and issue 2, test hearing at 6 units, beyond 6, and during chase; pulse then move and verify investigation uses original origin.
2. Test mid-edge target changes, patrol corners and lost sight.
3. If response follows an old route past the next center, invalidate the pending route at that center without snapping position or cutting corners.
4. Add a safe, logged fallback for a null BFS route.
5. Check layer-8 wall raycasts and all movement speeds.
6. Record actual findings and add focused regression coverage for confirmed defects.

## Acceptance criteria

Enemy reaches original pulse tile, waits then resumes patrol; chase overrides hearing; walls block sight and movement; pause freezes movement; no corner cutting or indefinite stuck routes.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 4-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17: Source review: added inclusive hearing predicate and test cases plus logged patrol fallback for a null route. Runtime hearing, corners and chase checks remain pending.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
