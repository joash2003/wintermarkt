---
type: bug
priority: medium
created: 2026-11-18
assignee: Joash Johnson Samuel
branch:
closed: 2026-11-28
---

# 29 — The compile check passes code that uses an engine module the project has not enabled

## Context

Observed on 18 Nov 2026 while adding animated visitors (issue 18): `python3 tools/compile-market.py`, and with it the quality gate, reported the Runtime assembly as compiling, but the Unity import of the same code failed with CS1069 ("AnimationClip ... has been forwarded to assembly UnityEngine.AnimationModule"). The project's Packages/manifest.json did not list `com.unity.modules.animation` at that point.

Cause, from reading the tool: it references every DLL in the editor's `UnityEngine` module folder, whatever the manifest enables. So the gate is green for code Unity will reject, and the mistake only shows when someone opens the editor.

## Goal

The compile check only references the engine modules the project has enabled, so it fails where Unity would fail.

## Acceptance criteria

- [x] `tools/compile-market.py` derives its module references from Packages/manifest.json (the `com.unity.modules.*` entries and what they depend on), or documents clearly why it cannot.
- [x] Removing `com.unity.modules.animation` from the manifest makes the check fail on MarketWorld.cs as Unity does.

## Tasks

- [x] Map manifest module names to `UnityEngine.<Name>Module.dll` and reference only those plus the core module.
- [x] Try it against the current code with and without the animation module.

## Outcome

Shipped in c27031b: `tools/compile-market.py` walks the dependency graph from `Packages/manifest.json` (edges from `packages-lock.json`, else the built-in packages' `package.json`) and references, for the Runtime assembly, only the engine modules that are enabled plus the modules that have no built-in package at all (Core, InputLegacy, TextRendering and so on, which Unity always references). The Editor assembly keeps every module, which is what Unity does. Checked 2026-11-28 against Unity's own compiler response file `Library/Bee/artifacts/*/Wintermarkt.Runtime.rsp`: the same 49 modules, no difference. With `com.unity.modules.animation` removed from the manifest the check fails on `MarketWorld.cs` with CS1069 on `AnimationClip`, exactly the error Unity gave on 18 Nov; with the manifest as it is, it passes with 7 enabled modules. Left out: nothing.