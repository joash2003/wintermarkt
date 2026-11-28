---
type: bug
priority: low
created: 2026-11-22
assignee: Joash Johnson Samuel
branch:
closed: 2026-11-28
---

# 45 — compile-market.py compiles against the newest installed Unity, not the pinned one

## Context

Observed on 2026-11-22 on Joash's Windows PC, which has both 6000.3.25f1 and 6000.6.5f1 installed under C:/Program Files/Unity/Hub/Editor. tools/compile-market.py picks `installed[-1]` from a sorted glob, so the gate's C# compile check used the 6000.6.5f1 references although the project pins 6000.3.25f1 (issue 12). tools/unity.sh already prefers the pinned version. Not confirmed: whether any current code compiles differently between the two; the gate passed either way so far. At stake: the gate can pass code that uses a 6.6 API that 6.3 lacks, or the reverse, and a future 6000.10 sorts before 6000.9.

## Goal

The compile check uses the Unity named in ProjectSettings/ProjectVersion.txt when it is installed, and only otherwise falls back to the newest.

## Acceptance criteria

- [x] On a machine with 6000.3.25f1 and a newer Unity installed, `python3 tools/compile-market.py` reports the pinned version.
- [x] With only another version installed it still runs, as today.
- [x] UNITY_APP still overrides both.

## Tasks

- [x] Read the pin from ProjectVersion.txt in compile-market.py and prefer it, the way tools/unity.sh does.
- [x] Do the same in tools/test-market-core.sh, which also takes the first Unity it finds for mono.

## Outcome

Shipped in c27031b: `tools/compile-market.py` and `tools/test-market-core.sh` prefer the Unity named in `ProjectSettings/ProjectVersion.txt` when it is installed and only otherwise take the newest found; `UNITY_APP` and `UNITY_MONO_ROOT` still override. The compile check now prints the version it used. Checked 2026-11-28 on Joash's Windows PC with 6000.3.25f1 and 6000.6.5f1 installed: `compiled against 6000.3.25f1`. Not checked: a machine with only another version installed, which takes the old, unchanged code path. A parallel fix of the same ticket (b3cfc17, pushed from the wintermarkt-2 clone on 2026-11-28) was merged in and replaced by this version, which also carries issue 29.
