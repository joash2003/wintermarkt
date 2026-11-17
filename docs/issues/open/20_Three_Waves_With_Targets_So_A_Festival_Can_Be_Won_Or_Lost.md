---
type: feature
priority: high
created: 2026-11-17
assignee:
branch:
---

# 20 — Three waves with targets so a festival can be won or lost

## Context

At the 17 Nov 2026 progress presentation the professor's verdict was that this is not a game yet. In the code a festival is one 10-minute timer with no target and no way to lose: `MarketSimulation.Finished` is only `Remaining <= 0`. Joash approved a plan on 17 Nov: waves first, then a small plot (issue 21), then visible wishes (issue 22).

## Goal

A festival is three waves, each with a target number of happy visitors; missing a target loses the festival and passing all three wins it.

## Acceptance criteria

- [ ] `MarketSettings.waves` holds title, seconds, arrivalFactor and target per wave; no waves means one wave of `festivalSeconds` with target 0.
- [ ] At the end of a wave the simulation fails if fewer visitors left happy in that wave than its target, otherwise it advances; after the last wave it is won.
- [ ] Arrivals are multiplied by the current wave's `arrivalFactor`.
- [ ] The HUD shows wave number, name, happy count against the target and time left in the wave; the results screen shows a win or a loss with the wave reached.
- [ ] Core tests cover a missed target, met targets through to a win, and the arrival factor.

## Tasks

- [ ] Tests first in MarketTestCases.cs.
- [ ] WaveDefinition and validation in MarketData.cs; wave state in MarketSimulation.cs.
- [ ] HUD, results card and menu text in MarketUI.cs; wave banner through MarketGame.Message.
- [ ] `waves` in market.json; tune targets with `bash tools/balance-bots.sh` together with issue 21.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
