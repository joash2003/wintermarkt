# Linz Wintermarkt: rules for AI assistants

Using an AI assistant on this repo is optional. If you use one (Codex, Claude, Gemini, Copilot, Cursor, ChatGPT or anything else), it must follow this file. Codex and Cursor read it directly; Claude reads it through CLAUDE.md, Gemini CLI through GEMINI.md and GitHub Copilot through .github/copilot-instructions.md. In a chat website, paste this file in at the start.

## The project

- Linz Wintermarkt is a real-time Christmas-market management game in Unity for the JKU GameDev 2026 course. Team: Joash Johnson Samuel, Alibek Abylkasymov, Bobby Alinkal.
- The design spec is docs/LINZ_WINTERMARKT_DESIGN.md. It wins over every other document. Work comes from tickets in docs/issues/.
- Code lives in Assets/_Wintermarkt: Core (pure C#, no UnityEngine), Runtime (MonoBehaviours), Editor (menu tools), Tests. Tuning is in Resources/Wintermarkt/market.json and site.json.
- Assets/_EchoMaze is the abandoned Echo Maze prototype. Do not edit it, and never run `tools/unity.sh legacy-setup`.

## Language

- English for everything: code names, comments, commit messages, tickets, docs and in-game text.
- Game code is C# 9, the newest version Unity 6 compiles. The gate compiles with `-langversion:9.0`, so newer syntax fails. Core must not use UnityEngine, so its tests run without Unity.
- Tools are Python 3 (standard library only) or bash. They must run on macOS and on Windows through Git Bash: use forward slashes and relative paths, no macOS-only commands.
- Match the style of the file you are editing. No new packages, plugins or downloaded assets without a ticket and an ASSET_CREDITS.md entry with source and license.

## Before every commit: the quality gate

Install the hook once per clone: `git config core.hooksPath .githooks`. Every commit then runs `tools/quality-check.sh`, which takes a few seconds:

| Check | Catches |
|---|---|
| Secret scan (staged) | API keys, tokens and passwords in what you are committing |
| Whitespace + conflicts | leftover `<<<<<<<` merge markers and broken whitespace |
| JSON data | broken market.json, site.json or other JSON under Assets |
| Issue board | a ticket file missing from docs/issues/README.md |
| C# compile (Unity API) | Core, Runtime and Editor code that would not compile in Unity |
| Core tests | the simulation tests in Assets/_Wintermarkt/Tests |

What the gate needs:
- **macOS:** Unity through Unity Hub (found automatically), python3, and `brew install gitleaks`.
- **Windows:** Git for Windows (the hook runs in Git Bash), Unity through Unity Hub in the default `C:\Program Files\Unity\Hub\Editor` (found automatically), Python 3 from python.org with "Add to PATH" ticked, and `winget install gitleaks` (or the release zip from github.com/gitleaks/gitleaks, on PATH).
- **Unity installed somewhere else:** set `UNITY_APP` to `Unity.app/Contents` (macOS) or the version's `Editor` folder (Windows).

Rules:
- A red gate means fix the cause. Never use `git commit --no-verify`. Never delete, skip or weaken a check or a test to get green. If the gate itself is wrong, file a ticket.
- Run the gate as its own command. Never pipe it through head, tail or grep, because the pipe hides a failure.
- The gate checks the working tree, so stage everything you changed before committing.
- New logic in Core gets a test in Assets/_Wintermarkt/Tests/MarketTestCases.cs. Run the test against the old code first and confirm it fails.
- A green gate does not mean the game works: it never opens Unity. Never claim Play Mode, build or playtest results you did not run yourself.

## Every issue you find gets a ticket

- Any bug, balance problem, broken doc, missing asset or risk you notice goes into docs/issues/ in the same session. That includes problems outside your current task and ones you do not fix.
- New ticket: copy docs/issues/TEMPLATE.md to the next unused EM number and add a row to the board in docs/issues/README.md. The gate fails if you forget the board.
- If a ticket already covers it, add what you found to that ticket's Observations and Work log instead of opening a duplicate.
- Commit the ticket on its own, for example `Add EM-015: queue timer ignores pause`. A finding that exists only in chat is lost.
- Write what you observed separately from what you suspect. Never present a guess as confirmed.
- Before working on a ticket, set its owner and In progress in the ticket and on the board. Record what you changed and ran in its Work log.

## Safety

- Never commit secrets: API keys, tokens, passwords, .env files, private email addresses. The gate scans for them, but don't rely on it alone.
- The repo stays private. The course PDFs stay out of git because they contain lab access details. They are in .gitignore; never force-add them.
- Git: no `push --force`, no rewriting the history of main, no deleting branches you didn't create. Never run `git stash`, `git checkout --`, `git restore` or `git reset --hard` over uncommitted work: commit it first or ask.
- Change only the files your task needs. Unity scenes (*.unity) and prefabs merge badly, so only one person edits a scene at a time; note it in the ticket.
- Commit each asset's .meta file with it. Never commit Library/, Temp/, Logs/, Builds/ or UserSettings/.
- Ask the human first before deleting files, large refactors, changing the Unity version (see EM-012), adding collaborators, or anything that publishes or sends messages.

## Commits and branches

- One topic per commit, written as `EM-NNN: what changed` when it belongs to a ticket.
- One branch per ticket, for example `issue/EM-013-pre-build-arrivals`. See CONTRIBUTING.md for review and statuses.

## Project skills

Skill files live in `.agents/skills/`. Use the matching one when the task fits:

- `wintermarkt-unity-development`: Unity gameplay, UI, scenes and bootstrap.
- `wintermarkt-testing-and-release`: checks, evidence and desktop builds.
- `wintermarkt-contributor-tickets`: issue and review workflow.
- `wintermarkt-project-docs`: beginner and course documentation.
- `legacy-maze-levels`: legacy Echo Maze maps, BFS, sonar, fog and enemy AI.

The wintermarkt-* skills started as Echo Maze skills. Their maze-specific parts are historical and never override this file or the design spec.
