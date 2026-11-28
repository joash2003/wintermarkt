---
type: bug
priority: low
created: 2026-11-22
assignee: Joash Johnson Samuel
branch: issue-45-pinned-unity
---

# 45 — compile-market.py compiles against the newest installed Unity, not the pinned one

## Context

Observed on 2026-11-22 on Joash's Windows PC, which has both 6000.3.25f1 and 6000.6.5f1 installed under C:/Program Files/Unity/Hub/Editor. tools/compile-market.py picks `installed[-1]` from a sorted glob, so the gate's C# compile check used the 6000.6.5f1 references although the project pins 6000.3.25f1 (issue 12). tools/unity.sh already prefers the pinned version. Not confirmed: whether any current code compiles differently between the two; the gate passed either way so far. At stake: the gate can pass code that uses a 6.6 API that 6.3 lacks, or the reverse, and a future 6000.10 sorts before 6000.9.

## Goal

The compile check uses the Unity named in ProjectSettings/ProjectVersion.txt when it is installed, and only otherwise falls back to the newest.

## Acceptance criteria

- [ ] On a machine with 6000.3.25f1 and a newer Unity installed, `python3 tools/compile-market.py` reports the pinned version.
- [ ] With only another version installed it still runs, as today.
- [ ] UNITY_APP still overrides both.

## Tasks

- [ ] Read the pin from ProjectVersion.txt in compile-market.py and prefer it, the way tools/unity.sh does.
- [ ] Do the same in tools/test-market-core.sh, which also takes the first Unity it finds for mono.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._