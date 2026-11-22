---
type: feature
priority: high
created: 2026-10-17
assignee: Joash Johnson Samuel
branch:
closed: 2026-11-22
---

# 8 — Build the market vertical slice

- Dependencies: None; use the new design document.
- Progress: Implemented; Unity runtime acceptance pending

## Context

Build and test a playable first market-management loop for the new Christmas-market direction.

## Goal

Build the market vertical slice.

## Where is the issue?

New Assets/_Wintermarkt runtime, editor and tests; existing Assets/_EchoMaze remains legacy.

## Observations

The market simulation and presentation source now provide configurable buildings, placement previews, bounded visitors, pathfinding, queue reservations, service income, upgrades, attraction, rides, festival timer, pause/restart and results. Eight deterministic core tests pass; Unity assemblies compile against the installed Unity API. Unity scene import, Play Mode loop and exported build still need verification.

## Tasks

1. Create a separate Wintermarkt runtime assembly and a safe editor bootstrap for the Market scene, orthographic camera, grid, provisional Goethekreuzung-side entrance and original blockout geometry. Keep the existing editor version and preserve existing user assets.
2. Implement configurable building definitions and a money balance. Provide a green/red placement preview, grid footprints, entrance and service entry cells. Reject occupied or route-blocking placements and spend coins only after successful placement.
3. Implement bounded visitor spawning, four-neighbor navigation, reserved queue slots, timed service, coin income and exit. Count unique visitors once on arrival. Use one stall type first.
4. Add a selected-building panel and a visible upgrade that changes service throughput with one atomic coin spend. Show money, arrivals and queue status; support pause and restart.
5. Add focused tests for placement rejection without coin loss, routes, queue reservations, income once per service and upgrade affordability. Run Unity compilation and a manual entrance-to-service-to-exit loop; record actual logs and unresolved blockers.

## Acceptance criteria

In Unity, place a stall, watch a visitor walk to it and leave after service, earn coins, and buy an upgrade that demonstrably improves throughput. Invalid placement and unaffordable upgrades change neither money nor state. Pause and restart work.

## Work log

- Implemented the simulation, runtime presentation, menu/HUD, rides and Unity bootstrap.
- Corrected placement checks so new buildings cannot overlap existing visitor paths or queue positions.
- `python3 tools/compile-market.py`: PASS for Core, Runtime and Editor against installed Unity 6000.6.4f1 API references (does not replace Unity import/Play Mode verification).
- `bash tools/test-market-core.sh`: PASS, 8 tests.
- Unity Editor import, manual playtest and build remain pending due to Package Manager socket restrictions in this environment.

- Location clarification: base the first playable map on Volksgarten at Goethekreuzung. Mariendom and Hauptplatz are separate locations on the proposed city overview, not additional required maps for this ticket.
- 2026-10-20: first real Unity run (6000.3.25f1): import, EditMode tests (24/24) and macOS build pass. The first build crashed in `MarketWorld.Material` (Shader.Find("Standard") is null in a player) and rendered magenta; the materials now copy Resources/Wintermarkt/Blockout.mat, which the bootstrap creates and ValidateData requires. The UI was also magenta because ProjectSettings/GraphicsSettings.asset had every built-in shader slot blank (UI/Default missing from builds); Unity regenerated the defaults. The third build renders the market, HUD and build menu with no exceptions. Still open: a human places a stall and plays a festival to the results screen.

## Outcome

Shipped: the market runs in Unity. First real run on 20 Oct (c6bad0c: import, 24 EditMode tests, macOS build, the built app), then the 17 and 18 Nov work on top. Pictures from `bash tools/unity.sh shots` on 22 Nov 2026 (Windows, Unity 6000.3.25f1, main at 31dc71e), kept in docs/submission/assets/shots-2026-11-22/. market.png shows three stalls with queues, visitors, coins, reputation, the HUD and the build menu; the upgrade-pop-*.png series in Logs/shots shows an upgrade. Left out: the original 32 x 36 blockout and the single-stall economy were replaced by the Volksgarten plot (issues 11, 21, 30) and the tap-to-cook loop (issue 32).