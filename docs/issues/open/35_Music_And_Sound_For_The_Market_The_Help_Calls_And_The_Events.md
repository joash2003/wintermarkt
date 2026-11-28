---
type: feature
priority: high
created: 2026-11-18
assignee: Aidan
branch:
---

# 35 — Music and sound for the market, the help calls and the events

## Context

The game has one generated bell tone (MarketGame). Music and sound were named in the future plan presented on 17 Nov 2026. Intended owner by the team poll: Aidan.

## Goal

The evening has music and distinct sounds, so the player can hear what is happening without looking.

## Acceptance criteria

- [ ] A music track that changes or builds across the four days.
- [ ] Distinct sounds for: a stall calling, a stall helped, a stall closing, coins, each event start, a snowball hit, a Krampus running off, a star earned, the ending and the fireworks.
- [ ] A market ambience loop.
- [ ] The existing mute setting silences everything.
- [ ] Every sound is original or has an ASSET_CREDITS.md entry with source and licence.

## Tasks

- [ ] Choose or make the sounds (Kenney has CC0 audio packs).
- [ ] Farhad produces the clips with ElevenLabs Pro (offered 28 Nov); Aidan wires them in. AI-generated audio goes into ASSET_CREDITS.md as such, and whether the course allows it is asked together with issue 42.
- [ ] An audio manager in Runtime instead of the single AudioSource.
- [ ] Volume balance on laptop speakers.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
