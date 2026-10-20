---
type: bug
priority: medium
created: 2026-10-20
assignee: Joash Johnson Samuel
branch: main
---

# 15 — tools/unity.sh only works on macOS with Unity 6000.6.4f1

## Context

`tools/unity.sh` (setup, data, test, build) defaults to `/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity`. That path only exists on a Mac with exactly that Unity version. On this Mac (6000.3.25f1 only) and on every Windows machine the script stops with "Set UNITY_EDITOR …", so teammates cannot run the bootstrap, EditMode tests or the build from the command line. `tools/compile-market.py` and `tools/test-market-core.sh` already find the installed Unity on macOS, Windows and Linux; this script was not updated with them.

## Goal

`bash tools/unity.sh setup|data|test|build` works on macOS and Windows (Git Bash) with whichever Unity version Unity Hub installed.

## Where is the issue?

tools/unity.sh, line 4 (`unity_editor` default).

## Acceptance criteria

- [ ] Without `UNITY_EDITOR` set, the script finds Unity under `/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity` (macOS) and `/c/Program Files/Unity/Hub/Editor/*/Editor/Unity.exe` (Git Bash on Windows).
- [ ] `UNITY_EDITOR` still overrides the search.
- [ ] `bash tools/unity.sh data` runs on a Mac and on a Windows machine, with the output recorded in the Work log.

## Tasks

- [ ] Replace the fixed default with the same search loop `tools/test-market-core.sh` uses, preferring the version pinned in ProjectSettings/ProjectVersion.txt when it is installed (see issue 12).
- [ ] Ask a Windows teammate to run `bash tools/unity.sh data` and record the result.

## Work log

- 2026-10-20: found while making the quality gate cross-platform. Not fixed yet.
- 2026-10-20: tools/unity.sh now searches the Unity Hub folders on macOS, Windows (Git Bash) and Linux, preferring the version pinned in ProjectVersion.txt; checked on macOS with fake installs (pinned wins, otherwise newest).
- 2026-10-20: first run of the new manual workflow `.github/workflows/windows-check.yml` on a real Windows machine (run 39422975573): 6 of 7 gate checks passed; Core tests failed because tools/test-market-core.sh ran Unity's extensionless `bin/mono`, which is not a Windows program ("Exec format error"). Unity ships both `mono` and `mono.exe` on every OS, so the script now picks `mono.exe` only when `OS=Windows_NT`. Windows has python3, python and py.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._