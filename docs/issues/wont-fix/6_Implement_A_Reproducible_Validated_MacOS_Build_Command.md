---
type: chore
priority: high
created: 2026-10-17
assignee:
branch:
closed: 2026-10-17
---

# 6 — Implement a reproducible validated macOS build command

- Dependencies: issue 1, issue 2; issue 3 before release.
- Evidence basis: Source inspection on 2026-10-17; no Unity execution claimed.

## Context

Implement a reproducible validated macOS build command. This work is required to meet the Unity handoff acceptance criteria.

## Goal

Implement a reproducible validated macOS build command.

## Where is the issue?

`Assets/_EchoMaze/Editor/; missing EchoMazeBuildAutomation.cs; Builds/` (paths relative to repository root).

## Observations

Unity 6000.6.4f1 executable is installed. No build automation or exported game currently exists; platform-module and license readiness are unverified.

## Tasks

1. Create EchoMaze.Editor.EchoMazeBuildAutomation.BuildRelease.
2. Require generated scenes/assets, validate all levels, and call BuildPipeline.BuildPlayer for the installed macOS target to Builds/EchoMaze_macOS/EchoMaze.app.
3. Check BuildReport for success and fail clearly otherwise.
4. Document the exact editor command and required module/license setup.
5. Run the command, inspect logs for compile/build errors, then launch the app and hand off full runtime checks to issue 5.

## Acceptance criteria

A repeatable command produces a macOS app, reports failures with nonzero exit status, and has recorded build logs. Launch verification is recorded separately from successful export.

## Contributor handoff

Claim this ticket in a separate commit by setting Owner and Status to In progress. Use branch `issue/issue 6-short-description`. Record observations, changed files, exact test commands/results and remaining limitations below. Open a PR referencing this ID and request another contributor's review. Mark Done only after review and recorded acceptance evidence.

## Work log

- 2026-10-17: Created from current source inspection. No implementation or test completion claimed.

- 2026-10-17: Added EchoMazeBuildAutomation.BuildRelease and tools/unity.sh build. No build command has run successfully; export/launch remains pending.

- User pivot: superseded by the Linz Wintermarkt direction. This is not a completion claim. Reuse relevant implementation or testing work only after assessing the new design.

## Outcome

Superseded on 2026-10-17 by the pivot from Echo Maze to Linz Wintermarkt (see DESIGN_LOG.md). The maze acceptance criteria no longer apply; the source stays in Assets/_EchoMaze.
