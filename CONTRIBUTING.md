# Contributing to Linz Wintermarkt

Start with [setup and bundled skills](docs/CONTRIBUTOR_SETUP.md), then choose a ticket with `python3 tools/issue.py list` (Windows: `py tools/issue.py list`); see [the tracker](docs/issues/README.md). The project is incomplete; open tickets describe the current gaps.

1. Claim a ticket with `python3 tools/issue.py move N in-progress` and commit that. Coordinate with other contributors before claiming shared files.
2. Create a focused branch such as `8-market-loop`. Keep unrelated changes separate and coordinate Unity scene edits.
3. Follow the ticket fix procedure; record new observations when the evidence changes the proposed solution.
4. Install the quality-gate hook once per clone with `git config core.hooksPath .githooks`; every commit then runs `tools/quality-check.sh` (setup for macOS and Windows is in AGENTS.md). Run other relevant checks too. Record actual results, commands, logs and unverified work in the ticket. Include Unity .meta files and exclude generated caches/builds.
5. Open a PR that references the issue number. Another contributor reviews it. After acceptance evidence and review, write the ticket's Outcome and `move N closed`.

Statuses are the folders in docs/issues/: open, in-progress, blocked, closed, wont-fix (decided against or superseded). File new issues with `python3 tools/issue.py new`, which reserves the next number. Every issue needs Context, Goal, Acceptance criteria and Tasks; closing needs an Outcome.

The source is published to the private repo https://github.com/joash2003/wintermarkt. Hosted Issues are not in use yet. GitHub forms become available after publishing this source to a GitHub repository with Issues enabled. Migrate local tickets explicitly and record links before using hosted Issues as the authoritative tracker. Do not maintain conflicting status boards.
