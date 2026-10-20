# Design decisions

## Active direction — Linz Wintermarkt

The user replaced Echo Maze with a Linz Christmas-market management game: maximize visitors, upgrade shops and include rides. See docs/LINZ_WINTERMARKT_DESIGN.md. The proposed defining trade-off is attraction versus service capacity. Existing maze source is retained, and all earlier maze decisions below are historical. Market gameplay is not implemented.


## 2026-10-20

The team uses Unity 6000.3.25f1 (6.3 LTS), the course setup version, instead of 6000.6.4f1 (issue 12). Package versions follow what 6.3 ships: ugui 2.0.0, test-framework 1.6.0.

## 2026-10-17

Preserve the handoff MVP: exactly three fixed maps, one sonar and one enemy type. Scans grant information but attract enemies. Shared settings retain proposed handoff values; no balancing changes are justified before actual playtesting.

Use existing Unity 6000.6.4f1 without silently upgrading. LTS status is unverified. Initial desktop target is macOS because this is the available development machine; team-wide target remains subject to confirmation.

Use lightweight scene entry points with runtime-generated geometry and Canvas UI. Reusable prefabs are not yet authored; code shares rendering/AI behavior instead. Geometry and beep audio are original, with no external art dependency.

Enemy repathing happens at tile centers to prevent corner cutting. Sonar remembers its original origin. A null route now logs a warning and returns to patrol; behavior needs runtime verification.

Scene generation preserves existing scene/settings/sprite assets. Bootstrap is not yet executed successfully due to sandbox Package Manager socket denial. Written code is not counted as a completed milestone.

Team roster, actual playtests, weekly slide link and course submissions are pending. The supplied handoff remains the specification, not proof of finished work.

## Location clarification

The user places the game at Goethekreuzung and identifies Mariendom and Hauptplatz as important city references. The first playable market is Volksgarten at its Goethekreuzung entrance, consistent with official market directions. Mariendom and Hauptplatz remain distinct locations on the proposed overview; additional playable maps are deferred until the first market works. See the design document for sources and the implementation sequence.

## Geographic fidelity and ambient transport

User requests the closest possible likeness to the real location plus Linz tram/bus background motion. Preserve real proportions, routes and landmarks using dated reference evidence. Use pan/zoom rather than freely compressing the park. Ambient vehicles are required, with scripted routes and stops; a transport-management system is not requested. Detailed modeling and current-layout verification remain pending.
