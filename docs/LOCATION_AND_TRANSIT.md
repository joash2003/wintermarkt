# Volksgarten / Goethekreuzung: location and transit brief

## User requirement

Recreate the location as similarly as possible, with recognizable Linz trams and buses moving in the background. This supersedes the earlier proposal to freely compress the park. Geographic fidelity is a core requirement; detailed scenery can still be built after a simple functional blockout.

## What must match

- Park perimeter, entrance at Goethekreuzung, principal paths and the relationship between the park and adjacent streets.
- Surrounding building footprints, approximate heights and characteristic silhouettes, including the Musiktheater where visible from the chosen camera.
- Tram track alignment, stops/platforms and transition between visible and hidden portions of the route, verified from mapping and dated photos before modeling.
- Bus street alignment, travel direction, stopping positions and pedestrian crossing connections, verified before route animation.
- Recognizable local vehicle proportions, articulation, windows, doors, destination displays and observed livery, modeled as original assets from references.
- Existing trees and permanent site features that define the space. Snow, festive lighting and market buildings sit within this framework.

Keep real proportions and use camera pan/zoom to frame the play area. Define a documented origin, north direction and meters-per-Unity-unit before placing assets. Do not invent detailed coordinates from a schematic transport map. Use a hidden grid for placement within actual usable market areas; streets, tracks, paths and permanent features are reserved.

Mariendom and Hauptplatz remain at their real separate locations. Do not relocate the cathedral beside the park to put it in shot. Only include a skyline landmark if the selected view and actual sight line support it; otherwise show it on the city overview.

## Reference baseline and evidence

The exact reference year and detailed geometry have not yet been frozen. Prefer recent dated photographs and a georeferenced base map, cross-checked with official plans. Record the date and source of each reference. Do not blend a historical stall layout, a proposed redevelopment rendering and present-day transport geometry as though they were one observed site.

Sources inspected during this planning pass:

| Source | What it supports | Limitations |
|---|---|---|
| [Official arrival information](https://christkindlmarkt-linz.at/allgemeines/anreise) | Goethekreuzung stop is at the Volksgarten market entrance | General visitor directions, not measured geometry or a complete current transit inventory |
| [City market-area plan](https://www.linz.at/serviceguide/brochure.php?id=408), Volksgarten section | Boundary mapping suitable for further reference inspection | PDF was identified and text extracted; screenshot retrieval failed in this session, so detailed shape has not been visually checked |
| [Official market-plan page](https://christkindlmarkt-linz.at/linz-weihnachtsmarkt/essen-trinken/8-volksgarten/129-die-beschicker-am-volksgarten) | Published market plan and attraction categories | Detailed plan geometry and reference year still need visual verification |
| [Historical Volksgarten plan](https://christkindlmarkt-linz.at/images/stories/volksgarten/plan/Plan%20_VG.pdf) | A plan explicitly labeled 2018 | Historical only; not a current stall-placement source |
| [City market rearrangement notice, 10 Nov 2025](https://www.linz.at/medienservice/2025/202511_133138.php) | Market arrangement changed and theater forecourt became more involved | Does not establish the final 2026 arrangement |
| [City entrance redevelopment page](https://www.linz.at/stadtentwicklung/136136.php) | Entrance redesign planning exists | Proposed renderings are not proof of construction completion |
| [LINZ AG network map](https://www.linzag.at/media/dokumente/linien_1/infomaterial_1/linien-linienfahrplan.pdf) | Official transit network reference; indexed version states January 2026 | Schematic rather than street-scale geometry; inspect actual map and latest stop information before final route signage |
| [LINZ AG 2025 timetable](https://www.linzag.at/media/dokumente/online_services_2/fahrplanbuch-2025.pdf) | Historical Goethekreuzung inventory: trams 1–4; buses 12, 17, 19, plus N82 | Historical line inventory, not verified current operating details |

Maps and photos are reference material. Recreate geometry and vehicles with original meshes. Record attribution/license terms for any actual map data or imagery incorporated into project assets; a visible web image is not automatically a reusable game texture.

## Background vehicle behavior

Use authored world-space paths based on verified road/track alignment, with configurable speed, stop points and dwell time. One reusable tram and one reusable bus are enough for the first pass. Spawn outside the camera, travel naturally, stop, then continue and recycle. Add basic spacing so vehicles do not pass through each other. Prevent obvious intersection conflicts with a small scripted phase system if the selected routes cross.

Vehicles are ambient scenery in the requested scope. They do not cost player money, require route planning or run exact real-world timetables. Small groups alighting and walking to the entrance are a possible later visual enhancement; do not couple arrival scoring or passenger counts to transit without an explicit design decision. Pause freezes background motion consistently with the game.

Pick the camera by inspecting a blockout: keep the market readable and allow Goethekreuzung/transit activity to be visible along the far side where the real geometry permits. Do not move streets to satisfy a camera composition. Pan and zoom can reveal surroundings when one view cannot show everything.

## Build and review steps

1. Assemble dated overhead and ground-level reference views, documenting uncertain features and the proposed baseline year.
2. Lay out the park, entrance, streets, tracks and major building masses at a consistent scale using simple shapes. Mark unknown geometry rather than inventing detail.
3. Compare overhead and eye-level screenshots of the blockout against reference views. Correct location/scale errors before detailed art. Get a local person's recognition feedback when available.
4. Add a simple tram and bus traveling along verified paths with stops; review routes from the gameplay camera.
5. Integrate buildable market cells and visitor navigation without modifying fixed geography. Upgrade stalls inside their footprint to avoid invading protected paths.
6. Replace simple shapes with original detailed assets, lighting and Christmas dressing. Check visibility, performance and recognition again.

## Acceptance criteria

- Reference inventory and dated baseline recorded, with unresolved current changes explicit.
- Main paths, entrance, street/track alignment and major surrounding silhouettes match reviewed evidence; deviations documented.
- The park is recognizable to someone familiar with Goethekreuzung, pending actual user review.
- Both a Linz-style tram and a bus move, stop and continue on plausible verified routes without crossing stalls or clipping buildings.
- Market remains legible and playable; camera pan/zoom shows the surroundings.
- No claim of an exact replica, current route lineup or completed geographic verification until checks actually occur.

Current status: the repo includes a clearly labeled 32 × 36 development blockout, generic surrounding geometry, and moving tram/bus-shaped ambient props. Their dimensions, street/track placement, stops, silhouettes and routes are invented for early camera/gameplay work and have not been compared against a measured map or verified transit references. The location acceptance criteria above are not met.
