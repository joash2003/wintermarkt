---
name: wintermarkt-testing-and-release
description: Validate Wintermarkt changes, run Unity tests, reproduce gameplay defects or create desktop releases with evidence.
---

The current game is Linz Wintermarkt; docs/LINZ_WINTERMARKT_DESIGN.md is the specification. Maze details below (levels, sonar, enemies, handoff sections) describe the legacy Echo Maze in Assets/_EchoMaze.

Inspect installed Unity and current project state before choosing a command. Read handoff sections 13–14 and the relevant EM ticket. The team editor is 6000.3.25f1 (6.3 LTS) and tools/unity.sh finds it; contributors must use their own editor path. Do not require this absolute path on other machines.

Run EditMode tests through Unity Test Runner or an appropriate batch command with -projectPath, -runTests, -testPlatform EditMode, -testResults and -logFile. Inspect XML and logs, including compiler/package/license failures. Do not use -quit if it terminates asynchronous test execution early. If no test assembly exists, implement it before claiming results (issue 3).

Manual checks cover movement/walls, sonar cost/cooldown/recharge/reveal, hearing origin and boundary, wall-blocked sight, capture/retry resets, pause timing, mute, all three exits, victory and replay. Keep external human playtests separate from automated smoke checks.

Build automation belongs in an Editor-only assembly and must validate maps and required scenes, check BuildReport, and report failures. BuildRelease is planned in issue 6; confirm it exists before invoking it. Launch the exported desktop app and run through victory before saying the standalone game is verified. A successful export alone is insufficient.

Record exact commands, editor/OS, actual counts, log locations, untested items and known defects in TESTING.md and the ticket. Exclude generated logs/builds from source commits. Never invent testing or participant evidence.
