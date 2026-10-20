---
type: chore
priority: high
created: 2026-10-17
assignee: Codex
branch:
closed: 2026-10-17
---

# 2 — Generate scenes, sprites, settings and editor bootstrap

- Dependencies: None; coordinate scene setup with issue 1.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Generate scenes, sprites, settings and editor bootstrap. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Generate scenes, sprites, settings and editor bootstrap.

## Where is the issue?

`Assets/_EchoMaze/Editor/ (currently empty); Resources; missing Scenes` (paths relative to repository root).

## Observations

GameManager requires Resources/GameSettings and Resources/Square, and uses Camera.main. Those assets and scene files do not yet exist. Start throws when settings are absent.

## Tasks

1. Add Editor-only EchoMazeProjectBootstrap with Tools > Echo Maze > Create/Repair Project and a batch callable method.
2. Generate an original square sprite with Sprite import settings and one unit size, GameSettings.asset, orthographic tagged Main Camera, and MainMenu/Game/Victory scenes with GameManager.
3. Set the Wall layer to index 8, legacy input support and desktop defaults.
4. Register scenes in build settings.
5. Preserve existing assets; skip them or require an explicit backup/overwrite action.
6. Validate all levels and save generated .meta files.

## Acceptance criteria

Fresh import followed by bootstrap produces runnable scenes and required Resources assets. Repeat bootstrap preserves edits. MainMenu opens; Play loads Level 1; player collides with walls; exit advances once.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 2-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17 (Codex): Added Editor bootstrap preserving existing assets, generating Resources/scene entry points, setting Wall/input and registering scenes. Unity batch startup failed before compilation because UPM socket creation was denied (EPERM). Scene/asset generation remains pending.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
