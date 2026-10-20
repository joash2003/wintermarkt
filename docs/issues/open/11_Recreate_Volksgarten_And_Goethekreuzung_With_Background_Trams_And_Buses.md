---
type: feature
priority: high
created: 2026-10-17
assignee:
branch:
---

# 11 — Recreate Volksgarten and Goethekreuzung with background trams and buses

- Dependencies: Location blockout can begin now; coordinate gameplay integration with issue 8.

## Context

The user requires a location closely matching the real Volksgarten/Goethekreuzung, including moving Linz trams and buses. Earlier planning allowed a compressed generic site and treated transit as optional.

## Goal

Recreate Volksgarten and Goethekreuzung with background trams and buses.

## Where is the issue?

Planned Assets/_Wintermarkt environment scene, geography data, camera and ambient transit components. Reference requirements are in docs/LOCATION_AND_TRANSIT.md. No corresponding market assets exist yet.

## Observations

Official sources place the market entrance at Goethekreuzung and document market/entrance changes. Historical stall plans cannot be assumed current. Transit references identify the network but do not replace measured track/stop geometry. No detailed environment or transit implementation has been verified.

## Tasks

1. Assemble dated overhead/ground references and record a consistent baseline year, source and usage terms. Identify uncertain changes at the entrance and market forecourt.
2. Set a coordinate origin, north direction and meter scale. Block out park boundary, paths, major buildings, road/track alignments and stops. Verify overhead and camera views before detailing.
3. Add camera pan/zoom and orient the default view to show background transport where actual geography permits. Keep other Linz landmarks in their proper locations.
4. Author original simplified tram and bus assets, route paths and stop dwell points. Add bounded spawning/recycling, spacing and scripted intersection coordination where needed. Freeze motion on pause.
5. Reserve fixed site features and transport areas from market placement. Integrate buildable plots and visitor routes with issue 8; enforce upgrade footprints.
6. Compare scene screenshots with references, collect actual local recognition feedback, and test camera readability, vehicle stopping, clipping, pause and repeated sessions. Record evidence and deviations.

## Acceptance criteria

Meet docs/LOCATION_AND_TRANSIT.md acceptance criteria. Both vehicle types move and stop in the background; geography follows reviewed references. Simulation is bounded and does not interfere with market placement or visitor scoring. No current route or exact-replica claims without verification.

## Work log

- Requirement and official reference inventory recorded. Implementation and visual comparison remain pending.

## Outcome

_Filled in when closed: what shipped, the commit(s), and what was left out and why._
