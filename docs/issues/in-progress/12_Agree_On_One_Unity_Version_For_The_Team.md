---
type: chore
priority: high
created: 2026-10-20
assignee: Joash Johnson Samuel
branch: main
---

# 12 — Agree on one Unity version for the team

- Dependencies: None

## Context

The project is pinned to Unity 6000.6.4f1, but the course setup and at least one team machine use Unity 6.3 LTS (6000.3.25f1). Opening the project in a different editor version prompts an upgrade or downgrade and can rewrite ProjectSettings and package versions. If two people use different versions, their commits keep overwriting each other's settings.

## Goal

Everyone on the team opens the project in the same Unity version.

## Where is the issue?

ProjectSettings/ProjectVersion.txt (`m_EditorVersion: 6000.6.4f1`), Packages/manifest.json, README.md (setup says 6000.6.4f1).

## Observations

- 2026-10-20: this Mac has only 6000.3.25f1 installed under /Applications/Unity/Hub/Editor. The 6000.6.4f1 pin comes from the machine the prototype was first written on.
- The pure C# compile check and the 8 core tests pass against 6000.3.25f1's references (`tools/quality-check.sh`). The project has not been opened in the 6000.3 editor yet, so package compatibility is unverified.

## Tasks

1. The team picks one version. The 6.3 LTS that the course setup uses is the safer choice.
2. One person opens the project in that version, accepts the version change, runs Tools → Wintermarkt → Create or Repair Project and presses Play.
3. Commit the changed ProjectVersion.txt, Packages/manifest.json and packages-lock.json together, and update the version in README.md.
4. Everyone installs that exact version through Unity Hub.

## Acceptance criteria

- [x] ProjectVersion.txt and README.md name the agreed version.
- [x] The project opens in that version without errors and the market scene plays.
- [x] `tools/quality-check.sh` passes.
- [ ] Another contributor reviewed the change.

## Work log

- 2026-10-20: found while porting the quality gate; compile and test tools now detect the installed Unity instead of a fixed version path.
- 2026-10-20: team decision (Joash): Unity 6000.3.25f1 (6.3 LTS). Changed ProjectVersion.txt and Packages/manifest.json (ugui 2.6.0 -> 2.0.0, test-framework 1.8.0 -> 1.6.0, the versions 6.3 ships); committed the new packages-lock.json. `tools/unity.sh setup`, `test` (24/24 EditMode) and `build` pass in 6.3, and the built app runs (see TESTING.md). README, readme.txt and CONTRIBUTING.md name 6.3 LTS. Remaining: Alibek and Bobby install 6000.3.25f1 and confirm the project opens, then close this.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._