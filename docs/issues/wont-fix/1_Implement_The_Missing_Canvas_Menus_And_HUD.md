---
type: feature
priority: high
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 1 — Implement the missing Canvas menus and HUD

- Dependencies: None.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Implement the missing Canvas menus and HUD. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Implement the missing Canvas menus and HUD.

## Where is the issue?

`Assets/_EchoMaze/Scripts/GameManager.cs; missing Scripts/MazeUI.cs` (paths relative to repository root).

## Observations

GameManager declares MazeUI, adds it in Start, and calls Initialize and Refresh. No MazeUI class exists in the project. This is a source inspection finding; Unity compilation has not run.

## Tasks

1. Implement MazeUI as a MonoBehaviour using Unity Canvas, CanvasScaler and standard UI text/buttons.
2. Provide Initialize(GameManager) and Refresh().
3. Build menu, instructions, HUD, pause, caught and victory panels.
4. Wire Play, Retry, Menu, TogglePause and ToggleMute to GameManager.
5. Update energy while playing; show only the panel for the current state.
6. Keep UI independent of destroyed runtime level objects.

## Acceptance criteria

Unity compiles without missing-type errors. At 1280×720 and 1920×1080, all buttons work; energy updates; pause, retry and victory navigation work.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 1-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17: Added MazeUI.cs with Canvas menus, instructions, HUD, pause/caught/victory panels and game callbacks. Runtime and resolution checks pending.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
