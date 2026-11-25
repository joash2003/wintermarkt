# Issues

The tracker is this folder, ported from the iam project. **The folder an issue sits in is its status**, so there is no separate status board to drift out of sync.

```
docs/issues/
├── open/          filed, not started
├── in-progress/   someone is on it (assignee + branch stamped)
├── blocked/       waiting on something outside the repo
├── closed/        done, Outcome section written
└── wont-fix/      decided against or superseded, Outcome says why
```

## Commands

The same commands work on macOS, Linux and Windows. On Windows use `py` (or `python`) instead of `python3`.

```bash
python3 tools/issue.py list                                          # the board
python3 tools/issue.py new "Queue timer ignores pause" --type bug --priority high
python3 tools/issue.py new "Real Volksgarten site" --type epic       # -> N.00
python3 tools/issue.py new "Tram line 1 and 2" --parent 11           # -> 11.01, listed in the epic
python3 tools/issue.py move 13 in-progress                           # stamps your name + branch
python3 tools/issue.py move 13 closed                                # the gate refuses it until Outcome is written
python3 tools/issue.py show 13
python3 tools/issue.py check                                         # the pre-commit gate runs it with --offline
python3 tools/issue.py prune                                         # drop reservation tags for issues now on main
```

After `new` or `move`, commit the change on its own, for example `File issue 15: queue timer ignores pause`.

## Numbers are reserved, not guessed

`new` pushes a create-only git tag `issue/N` to origin before writing the file. Git refuses a tag that already exists, so two people can never take the same number; the loser re-reads and takes the next one. Pushing the tag needs write access to the GitHub repo.

Sub-issues never reserve: `11.01`, `11.02` and so on share the epic's number.

Filing needs a connection to GitHub; without one, `new` refuses rather than guess a number that someone else may take meanwhile. The ticket cannot reach the team until you push anyway, so file it once you are online. `check` (online) flags any ticket file whose number was never reserved, for example one created by hand.

## Front matter

```yaml
---
type: bug            # feature | enhancement | bug | refactor | chore | epic   (required)
priority: high       # high | medium | low
created: 2026-10-20
assignee:            # stamped on move to in-progress
branch:              # stamped on move to in-progress
parent: '11'         # sub-issues only
closed:              # stamped on move to closed / wont-fix
---
```

## What `check` enforces

- filename `N_Title.md` / `N.00_Epic.md` / `N.01_Sub.md`; heading `# N — Title` matching it
- a valid `type`, `priority` and `created` date
- the five sections: Context, Goal, Acceptance criteria, Tasks, Outcome (extra sections such as Observations and Work log are welcome)
- `.00` files are `type: epic`; sub-issues carry `parent:` and have an epic
- no number appears twice across folders
- `closed` / `wont-fix` carry a `closed:` date; nothing else does
- **a closed issue has a written Outcome**; the placeholder is refused
- every base number is reserved on origin (skipped with `--offline`)

## Conventions

- One issue per concern. Bundle only under an epic.
- Write what you observed separately from what you suspect.
- Reference the issue in commits: `Issue 13: hold arrivals until the first building exists`.
- Close by writing Outcome first, then `move … closed`.
- Issues 1 to 7 are the old Echo Maze tickets, kept as `wont-fix` (superseded by the pivot). Tickets used to be called EM-001 to EM-014; the numbers stayed the same.
