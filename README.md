# Linz Wintermarkt (working title)

A Unity management-game prototype about growing a Christmas market in Linz’s Volksgarten at Goethekreuzung. Place and upgrade food stalls, craft shops and rides; keep queues moving; welcome as many visitors as possible during a festival. The prototype includes a playable market loop and simple original 3D blockout geometry.

**Geography is provisional.** The current 32 × 36 grid, roads, paths, trees, theater mass and transit motion are invented blockout elements, not a measured reconstruction of Volksgarten or verified Linz vehicle routes. See [location brief](docs/LOCATION_AND_TRANSIT.md) for the reference work still needed.

## Open in Unity

Use Unity 6000.3.25f1 (6.3 LTS), the version the whole team uses (issue 12). Open this folder in Unity Hub, let the package import finish, then use **Tools → Wintermarkt → Create or Repair Project**. This creates `Assets/_Wintermarkt/Scenes/Market.unity` only when it is missing, configures the market scene for the player build, and opens it. Press Play. The existing `_EchoMaze` project is retained as legacy source.

From Terminal, `bash tools/unity.sh setup` runs the same bootstrap, `bash tools/unity.sh data` validates configuration, `bash tools/unity.sh test` runs EditMode tests, and `bash tools/unity.sh build` exports a macOS prototype. Some restricted environments prevent Unity Package Manager from opening its local socket; if setup stalls, open the project in the regular Unity Hub/editor and let package import finish there.

The pure market simulation tests can run without launching Unity: `bash tools/test-market-core.sh`. `python3 tools/compile-market.py` compiles market runtime/editor assemblies against the installed Unity API references; that verifies compilation but does not import a scene or prove Play Mode behavior.

## Project notes

- [Initial design-document deck v0.1](docs/submission/Linz_Wintermarkt_Initial_Design_Document_v0.1.pptx) — editable 9-slide PowerPoint; generated with `python3 tools/create_design_deck.py`
- [Game concept and MVP](docs/LINZ_WINTERMARKT_DESIGN.md)
- [Location/transit reference brief](docs/LOCATION_AND_TRANSIT.md)
- [Issue tracker](docs/issues/README.md): `python3 tools/issue.py list`
- [Contributor workflow](CONTRIBUTING.md) and [bundled contributor skills](docs/CONTRIBUTOR_SETUP.md)
- [Verification record](TESTING.md)

Source assets and generated project setup remain separate: no generated scene, measured site data or exported build is included yet.
