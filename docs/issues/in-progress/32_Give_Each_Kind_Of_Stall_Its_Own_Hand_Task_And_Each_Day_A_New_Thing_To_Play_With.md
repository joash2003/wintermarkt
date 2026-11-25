---
type: feature
priority: high
created: 2026-11-18
assignee: Joash Johnson Samuel
branch: 32-tap-to-cook
---

# 32 — Give each kind of stall its own hand task and each day a new thing to play with

## Context

On 18 Nov 2026 Joash described the game as "boring" and "repeaty". Observed cause: stalls serve by themselves and every help call is the same click on a yellow ball (issue 23). A different mini-task per stall type was proposed first. A teammate then proposed the idea below in the team chat, it was judged the better one, and Joash approved building it on 18 Nov with two adjustments (one tap per batch instead of one per portion; portions go out by themselves).

The teammate's idea, shortened: a Glühwein stall can prepare three portions at a time; the player taps to make them and they go to the customers. Customers' patience shrinks as the game goes on, so the player has to be faster and has to keep three or four stalls going at once. Upgrades raise the batch size or shorten the cooking time.

This replaces the regular help calls of issue 23 (the power cut still uses them) and the per-stall mini-tasks first written in this ticket. The second half of the original proposal, a new thing to play with each day, is not part of this work.

## Goal

Nothing is served unless the player taps: a tap on a stall cooks one batch, the batch is handed out to its queue, and patience shrinks from day to day.

## Acceptance criteria

- [x] With `tapToCook` on, a stall serves only from its stock; a tap starts cooking one batch (`capacity` portions in `serviceSeconds`), and it cannot be tapped again while cooking or while full.
- [x] Portions are handed to waiting visitors without further clicks.
- [x] Upgrades raise the batch size and shorten the cooking time.
- [x] Each day has a `patienceFactor`; later days are less patient.
- [x] The stall shows its stock, that it is cooking, and when it needs a tap.
- [x] Regular help calls are off; the power cut still makes every stall call.
- [x] Core tests cover cooking, stock and the patience factor; with `tapToCook` off the older tests are unchanged.
- [x] The bots tap with a limited number of actions per second, and the numbers are re-tuned.

## Tasks

- [x] Tests first.
- [x] Simulation and data.
- [x] Click handling and stall display.
- [x] Bots, tuning, design spec, Play Mode pictures.

## Work log

- 2026-11-18: three tests first (17 compile errors against the old code). `MarketSettings.tapToCook` and `handoverSeconds`; per building `Stock`, `CookRemaining`, `Batch`; `TryCook`; in tap mode `StartService` hands out stock only. `WaveDefinition.patienceFactor`. With `tapToCook` off nothing changes, so the 25 older tests are untouched. A level's `capacity` may now exceed the queue places in tap mode (it is a batch size there).
- Game: a click on or near a stall relights it if it calls, otherwise cooks; the stall shows a white pulsing ball (empty, people waiting), a growing green ball (cooking) and a row of cubes for its stock. Menu and status text mention tapping. market.json: tap to cook on, regular help calls off, patience 100/90/80/70 % over the four days, new batch sizes and cooking times per level.
- Bots now have one mouse (two hand actions a second). Table in the design spec. The acceptance boxes above are ticked from tests, bot runs and a Play Mode run of the whole evening with pictures of the tap and cooking markers; no exceptions.
- Not checked: tapping by hand; whether five or six stalls are manageable for a person; the stock cubes were hard to make out at the default zoom and were enlarged afterwards without a second look; star targets were not re-tuned.
- Not built: "each day brings a new thing to play with" from this ticket's first version.
- 2026-11-22: merged into main (b42af77) on Windows. MarketWorld.cs conflicted with issue 44 (upgrade pop, wish markers gone): kept main's side and added the stocks field, its Clear and Remove. Gate green; not played by hand after the merge.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._