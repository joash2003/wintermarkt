# Contributing to Linz Wintermarkt

Start with [setup and bundled skills](docs/CONTRIBUTOR_SETUP.md), then choose a ticket from the [issue board](docs/issues/README.md). The project is incomplete; open tickets describe the current gaps.

1. Claim a ticket by setting your name and In progress in both its file and the board. Coordinate with other contributors before claiming shared files.
2. Create a focused branch such as `issue/EM-008-market-loop`. Keep unrelated changes separate and coordinate Unity scene edits.
3. Follow the ticket fix procedure; record new observations when the evidence changes the proposed solution.
4. Run relevant checks. Record actual results, commands, logs and unverified work in the ticket. Include Unity .meta files and exclude generated caches/builds.
5. Open a PR using the template and reference the EM ID. Another contributor reviews it. Move to In review while review is pending and Done only after acceptance evidence and review. Update the board and ticket together.

Statuses: Open, In progress, Blocked (state the dependency), In review, Done, Superseded (replaced by a design change; not completed). New issues use docs/issues/TEMPLATE.md and the next unused permanent number. Every issue needs the problem, location, observations and a concrete fix or diagnostic procedure, followed by verification criteria.

The local origin is configured as https://github.com/joash2003/EchoMaze.git. The local main branch has no commits yet; source publication and hosted Issues have not been verified. GitHub forms become available after publishing this source to a GitHub repository with Issues enabled. Migrate local tickets explicitly and record links before using hosted Issues as the authoritative tracker. Do not maintain conflicting status boards.
