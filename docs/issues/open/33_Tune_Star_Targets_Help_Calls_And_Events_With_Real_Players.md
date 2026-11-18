---
type: chore
priority: high
created: 2026-11-18
assignee:
branch:
---

# 33 — Tune star targets, help calls and events with real players

## Context

All numbers in market.json were set from scripted bots (design spec, 'How the numbers work'). Star targets are about two thirds of the weaker mixed bot; bots answer help calls after 3 s, miss one in four and never miss a snowball. Known from the bot runs: more than a hundred visitors give up even in a four-star run, coins are left over once the market is built, and reputation loss per lost visitor was moved three times (3, 2, 1.5) to stop early closures. Intended owner by the team poll: Farhad.

## Goal

The numbers are adjusted from what people actually manage, and the reasons are recorded.

## Acceptance criteria

- [ ] At least three people's full runs are recorded: stars, happy per day, calls helped and missed, Krampus driven off, visitors lost.
- [ ] Star targets, help interval and window, event sizes and incomes are changed where the runs show a problem, each change with its reason in this ticket.
- [ ] `bash tools/balance-bots.sh` is re-run and the table in the design spec replaced.

## Tasks

- [ ] Record runs.
- [ ] Decide whether visitors giving up in the rush is acceptable or arrivals must fall.
- [ ] Decide what coins are for late in the game (see the decorations ticket).

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
