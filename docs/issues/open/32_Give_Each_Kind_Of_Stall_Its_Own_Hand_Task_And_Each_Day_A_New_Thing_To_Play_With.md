---
type: feature
priority: high
created: 2026-11-18
assignee:
branch:
---

# 32 — Give each kind of stall its own hand task and each day a new thing to play with

## Context

On 18 Nov 2026 Joash described the game as 'boring' and 'repeaty'. Observed cause: every stall's help call is the same click on a yellow ball (issue 23), and every day the player does what they did the day before. Claude proposed the change below; Joash has not yet said yes or no to it. Intended owner by the team poll: Farhad (rules) with Joash (Unity).

## Goal

Helping a stall is a different small action for each stall type, and each of the four days introduces one new thing, so minute 8 does not play like minute 2.

## Acceptance criteria

- [ ] Joash and the team have agreed or changed the proposal before work starts.
- [ ] Punsch Hütte: hold the mouse to pour and let go when the cup is full; too early or too late counts as a poor help.
- [ ] Sweet Bakery: click while an oven timer is in its green zone, or the batch burns.
- [ ] Christmas Crafts: drag the present of the right colour to the visitor who wants it.
- [ ] Each day unlocks one new thing: for example the carousel on Krampus night, the wheel on Christmas Eve, fireworks the player sets off on New Year's Eve.
- [ ] Core tests cover each task's success and failure; the bots are updated.

## Tasks

- [ ] Decide the proposal.
- [ ] Simulation: a help task type per building definition and a quality result instead of a plain click.
- [ ] Runtime: one small on-screen control per task.
- [ ] Re-tune help intervals and star targets.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
