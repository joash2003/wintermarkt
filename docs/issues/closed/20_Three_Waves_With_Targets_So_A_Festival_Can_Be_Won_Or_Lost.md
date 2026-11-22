---
type: feature
priority: high
created: 2026-11-17
assignee: Farhad Arezo
branch: 20-waves
closed: 2026-11-22
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

## Work log

- 2026-11-17: three tests written first (11 compile errors against the old code). WaveDefinition and `MarketSettings.waves`; wave state, `Failed`, `Won` and the arrival factor in MarketSimulation; HUD wave line, win or loss results card, menu text and a wave banner. market.json: Afternoon 150 s target 15, Evening rush 240 s x1.6 target 70, Closing time 210 s x1.2 target 60 (first values, tuned later with issue 21). First bot run: the four single-minded bots lose in wave 1, the two mixed bots win. 14 core tests pass. Not checked yet: the HUD in Unity.

## Outcome

Superseded. The three waves with a fail-on-miss rule were replaced by stars per act in issue 24 and by four days with real dates in issue 27. `MarketSettings.waves`, the arrival factor and the wave tests from this ticket live on under those. ending.png from 22 Nov (docs/submission/assets/shots-2026-11-22/) shows the result: four days, four stars. Nothing left to do here.