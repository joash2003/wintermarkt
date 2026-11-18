---
type: bug
priority: medium
created: 2026-11-18
assignee:
branch:
---

# 29 — The compile check passes code that uses an engine module the project has not enabled

## Context

Observed on 18 Nov 2026 while adding animated visitors (issue 18): `python3 tools/compile-market.py`, and with it the quality gate, reported the Runtime assembly as compiling, but the Unity import of the same code failed with CS1069 ("AnimationClip ... has been forwarded to assembly UnityEngine.AnimationModule"). The project's Packages/manifest.json did not list `com.unity.modules.animation` at that point.

Cause, from reading the tool: it references every DLL in the editor's `UnityEngine` module folder, whatever the manifest enables. So the gate is green for code Unity will reject, and the mistake only shows when someone opens the editor.

## Goal

The compile check only references the engine modules the project has enabled, so it fails where Unity would fail.

## Acceptance criteria

- [ ] `tools/compile-market.py` derives its module references from Packages/manifest.json (the `com.unity.modules.*` entries and what they depend on), or documents clearly why it cannot.
- [ ] Removing `com.unity.modules.animation` from the manifest makes the check fail on MarketWorld.cs as Unity does.

## Tasks

- [ ] Map manifest module names to `UnityEngine.<Name>Module.dll` and reference only those plus the core module.
- [ ] Try it against the current code with and without the animation module.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
