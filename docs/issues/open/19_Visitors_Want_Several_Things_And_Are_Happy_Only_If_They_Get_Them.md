---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 19 — Visitors want several things and are happy only if they get them

## Context

Each visitor goes to exactly one building, pays and leaves (MarketSimulation.SpawnVisitor). Consequences seen on 17 Nov 2026 from the code and the bot runs in issue 9: variety only matters through a flat attraction bonus, layout and walking distance barely matter, nobody needs the rides, and the player has few decisions after the first stalls. Joash agreed on 17 Nov to change this first, ahead of the other scope changes.

## Goal

A visitor arrives wanting two or three different kinds of thing, visits a building for each, and counts as a happy visitor (the score) only if they got all of them.

## Acceptance criteria

- [ ] A visitor's wishes are categories from the buildings currently unlocked, chosen deterministically so runs stay repeatable.
- [ ] After being served, a visitor walks on to the next wanted building instead of leaving.
- [ ] A visitor who got everything counts as happy; one who leaves because something they wanted is not built counts as disappointed; one who gives up in a queue or finds every wanted queue full counts as left unserved, as before.
- [ ] The score shown in the HUD and on the results screen is happy visitors.
- [ ] The HUD tells the player which category visitors are missing most.
- [ ] Core tests cover a visitor fulfilling two wishes and a visitor missing one; existing tests still pass.
- [ ] `bash tools/balance-bots.sh` is re-run and the table recorded here and in the design spec.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] Wishes, the next-stop choice and the happy/disappointed counters in MarketSimulation; `maxWishes` and `disappointmentLoss` in MarketSettings and market.json.
- [ ] HUD and results screen text.
- [ ] Re-tune arrivals and income if visitors now pay two or three times.
- [ ] Update "How the numbers work" in the design spec.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
