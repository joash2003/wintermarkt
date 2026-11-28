# Contributing to Linz Wintermarkt

Start with [setup](docs/CONTRIBUTOR_SETUP.md) and the [project rules](#project-rules), then choose a ticket with `python3 tools/issue.py list` (Windows: `py tools/issue.py list`); see [the tracker](docs/issues/README.md). The project is incomplete; open tickets describe the current gaps.

1. Claim a ticket with `python3 tools/issue.py move N in-progress` and commit that. Coordinate with other contributors before claiming shared files.
2. Create a focused branch such as `8-market-loop`. Keep unrelated changes separate and coordinate Unity scene edits.
3. Follow the ticket fix procedure; record new observations when the evidence changes the proposed solution.
4. Install the quality-gate hook once per clone with `git config core.hooksPath .githooks`; every commit then runs `tools/quality-check.sh` (setup for macOS and Windows is under [Quality gate](#quality-gate)). Run other relevant checks too. Record actual results, commands, logs and unverified work in the ticket. Include Unity .meta files and exclude generated caches/builds.
5. Open a PR that references the issue number. Another contributor reviews it. After acceptance evidence and review, write the ticket's Outcome and `move N closed`.

Statuses are the folders in docs/issues/: open, in-progress, blocked, closed, wont-fix (decided against or superseded). File new issues with `python3 tools/issue.py new`, which reserves the next number. Every issue needs Context, Goal, Acceptance criteria and Tasks; closing needs an Outcome.

The source is published to the private repo https://github.com/joash2003/wintermarkt. Hosted Issues are not in use yet. GitHub forms become available after publishing this source to a GitHub repository with Issues enabled. Migrate local tickets explicitly and record links before using hosted Issues as the authoritative tracker. Do not maintain conflicting status boards.

## Project rules

- Linz Wintermarkt is a real-time Christmas-market management game in Unity for the JKU GameDev 2026 course. Team: Joash Johnson Samuel, Alibek Abylkasymov, Bobby Alinkal.
- The design spec is docs/LINZ_WINTERMARKT_DESIGN.md. It wins over every other document. Work comes from tickets in docs/issues/.
- Code lives in Assets/_Wintermarkt: Core (pure C#, no UnityEngine), Runtime (MonoBehaviours), Editor (menu tools), Tests. Tuning is in Resources/Wintermarkt/market.json and site.json.
- Assets/_EchoMaze is the abandoned Echo Maze prototype. Do not edit it, and never run `tools/unity.sh legacy-setup`.

### Language

- English for everything: code names, comments, commit messages, tickets, docs and in-game text.
- Game code is C# 9, the newest version Unity 6 compiles. The gate compiles with `-langversion:9.0`, so newer syntax fails. Core must not use UnityEngine, so its tests run without Unity.
- Tools are Python 3 (standard library only) or bash. They must run on macOS and on Windows through Git Bash: use forward slashes and relative paths, no macOS-only commands.
- Match the style of the file you are editing. No new packages, plugins or downloaded assets without a ticket and an ASSET_CREDITS.md entry with source and license.

### Quality gate

Install the hook once per clone: `git config core.hooksPath .githooks`. Every commit then runs `tools/quality-check.sh`, which takes a few seconds:

| Check | Catches |
|---|---|
| Secret scan (staged) | API keys, tokens and passwords in what you are committing |
| Unity version pinned | ProjectSettings/ProjectVersion.txt naming any Unity but the team's 6000.3.25f1, which means the project was opened in the wrong version (issue 12) |
| Whitespace + conflicts | leftover `<<<<<<<` merge markers and broken whitespace |
| JSON data | broken market.json, site.json or other JSON under Assets |
| Issue files | a ticket with a wrong filename, heading, front matter or missing section, or closed without an Outcome |
| Issue tool tests | breakage in tools/issue.py, the ticket tracker |
| C# compile (Unity API) | Core, Runtime and Editor code that would not compile in Unity |
| Core tests | the simulation tests in Assets/_Wintermarkt/Tests |

What the gate needs:
- **macOS:** Unity 6000.3.25f1 (6.3 LTS) through Unity Hub (found automatically), python3, and `brew install gitleaks`.
- **Windows:** Git for Windows (the hook runs in Git Bash), Unity 6000.3.25f1 (6.3 LTS) through Unity Hub in the default `C:\Program Files\Unity\Hub\Editor` (found automatically), Python 3 from python.org with "Add to PATH" ticked, and `winget install gitleaks` (or the release zip from github.com/gitleaks/gitleaks, on PATH).
- **Unity installed somewhere else:** set `UNITY_APP` to `Unity.app/Contents` (macOS) or the version's `Editor` folder (Windows).

Rules:
- A red gate means fix the cause. Never use `git commit --no-verify`. Never delete, skip or weaken a check or a test to get green. If the gate itself is wrong, file a ticket.
- Run the gate as its own command. Never pipe it through head, tail or grep, because the pipe hides a failure.
- The gate checks the working tree, so stage everything you changed before committing.
- New logic in Core gets a test in Assets/_Wintermarkt/Tests/MarketTestCases.cs. Run the test against the old code first and confirm it fails.
- A green gate does not mean the game works: it never opens Unity. Never claim Play Mode, build or playtest results you did not run yourself.

### Every issue you find gets a ticket

- Any bug, balance problem, broken doc, missing asset or risk you notice goes into docs/issues/ in the same session, including problems outside your current task and ones you do not fix.
- New ticket: `python3 tools/issue.py new "Title" --type bug --priority high` (Windows: `py` instead of `python3`). Never create ticket files by hand or guess a number. Then fill in Context, Goal, Acceptance criteria and Tasks.
- If a ticket already covers it, add what you found to that ticket instead of opening a duplicate.
- Commit the ticket on its own, for example `File issue 15: queue timer ignores pause`.
- Write what you observed separately from what you suspect. Never present a guess as confirmed.
- Record what you changed and ran in the ticket's Work log.

### Safety

- Never commit secrets: API keys, tokens, passwords, .env files, private email addresses. The gate scans for them, but don't rely on it alone.
- The repo stays private. The course PDFs stay out of git because they contain lab access details. They are in .gitignore; never force-add them.
- Git: no `push --force`, no rewriting the history of main, no deleting branches you didn't create. Never run `git stash`, `git checkout --`, `git restore` or `git reset --hard` over uncommitted work: commit it first.
- Change only the files your task needs. Unity scenes (*.unity) and prefabs merge badly, so only one person edits a scene at a time; note it in the ticket.
- Commit each asset's .meta file with it. Never commit Library/, Temp/, Logs/, Builds/ or UserSettings/.
- Discuss with the team before deleting files, large refactors, or changing the Unity version (see issue 12).

### Commits and branches

- One topic per commit, written as `Issue N: what changed` when it belongs to a ticket.
- One branch per ticket, for example `13-pre-build-arrivals`.
