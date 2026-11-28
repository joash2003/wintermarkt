---
type: chore
priority: high
created: 2026-10-20
assignee: Joash Johnson Samuel
branch: main
closed: 2026-11-28
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
5. Review by Farhad or Alibek (about ten minutes): install 6000.3.25f1 through Unity Hub, `git pull`, open the project (no version prompt may appear), run Tools > Wintermarkt > Create or Repair Project, press Play and place a stall. Then tick the last box, add a work log line with name and date, and run `py tools/issue.py move 12 closed` with an Outcome; if anything fails, write what happened here instead.

## Acceptance criteria

- [x] ProjectVersion.txt and README.md name the agreed version.
- [x] The project opens in that version without errors and the market scene plays.
- [x] `tools/quality-check.sh` passes.
- [ ] Another contributor reviewed the change.

## Work log

- 2026-10-20: found while porting the quality gate; compile and test tools now detect the installed Unity instead of a fixed version path.
- 2026-10-20: team decision (Joash): Unity 6000.3.25f1 (6.3 LTS). Changed ProjectVersion.txt and Packages/manifest.json (ugui 2.6.0 -> 2.0.0, test-framework 1.8.0 -> 1.6.0, the versions 6.3 ships); committed the new packages-lock.json. `tools/unity.sh setup`, `test` (24/24 EditMode) and `build` pass in 6.3, and the built app runs (see TESTING.md). README, readme.txt and CONTRIBUTING.md name 6.3 LTS. Remaining: Alibek and Bobby install 6000.3.25f1 and confirm the project opens, then close this.

- 2026-11-28: the pin has held for five weeks: ProjectVersion.txt is unchanged since 5d25915 (20 Oct) and the only package change since, 18db4aa on 18 Nov, added com.unity.modules.animation. Every commit so far is Joash's, so nobody else has opened the project yet. GitHub: Farhad-JKU and AlibekAby have push access, Aidan's invitation of 25 Nov is still unaccepted, Bobby has left the team (issue 41). The compile gate now uses the pinned version (issue 45), and today's `bash tools/unity.sh shots` and `build` ran in 6000.3.25f1 on Joash's Windows PC (issues 37 and 47). The 6000.6.4f1 lines in TESTING.md are the dated 17 Oct record, not a current claim. Remaining: task 5.

## Outcome

Unity 6000.3.25f1 (6.3 LTS) is the team version: pinned in ProjectSettings/ProjectVersion.txt since 5d25915 (20 Oct) with the packages 6.3 ships, named in README.md, readme.txt and CONTRIBUTING.md, and used by the compile gate (issue 45). Closed on 2026-11-28 by Joash's decision: the team agrees the version in the group chat instead of a review on a second machine, so the last acceptance box stays unticked. Left out: no other contributor has opened the project yet. If the first open in 6000.3.25f1 shows a version prompt or an error, reopen this or file a bug.