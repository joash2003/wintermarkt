---
type: feature
priority: medium
created: 2026-11-28
assignee: Farhad Arezo
branch:
---

# 48 — A generator with limited power that the player upgrades to run more stalls

## Context

Farhad proposed this in the team chat on 28 Nov 2026. Today a stall costs coins once and then runs for free, so after the first minute the only question is which stall to build next. The lecturer's feedback on 25 Nov was that the player has nothing to do and the game is boring; part of that is that there is no decision to make. The power cut event (issue 25) already exists, but there is no power model behind it.

## Goal

Every stall draws power from one generator with a limited supply, and the player has to split coins between stalls and generator upgrades.

## Acceptance criteria

- [ ] `market.json` has a generator with `power` and `cost` per level, and every stall level has a `power` draw.
- [ ] Building or upgrading a stall is refused with a clear reason when the draw would exceed the supply; the generator upgrade is bought like a stall upgrade.
- [ ] The HUD shows power used / available; the generator is visible on the plot and shows its level.
- [ ] The power cut event (issue 25) is unchanged.
- [ ] Core tests cover the refusal and the generator upgrade; the bots are re-run and the star targets checked (issue 33).
- [ ] Played by hand once; what it felt like is written in the ticket.

## Tasks

- [ ] Data: generator and power values in `market.json`, `MarketData.cs`.
- [ ] Core: supply and draw in `MarketSimulation` (`CanPlace`, `TryUpgrade`), a `TryUpgradeGenerator`.
- [ ] Runtime: generator model on the plot, HUD line, upgrade button in the selection panel.
- [ ] Tests, bot run, numbers in the design spec.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
