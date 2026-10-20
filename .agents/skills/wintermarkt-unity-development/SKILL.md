---
name: wintermarkt-unity-development
description: Implement or repair Wintermarkt Unity C# gameplay, scenes, prefabs, UI and editor bootstrap. Use for game code changes, not unrelated software.
---

The current game is Linz Wintermarkt; docs/LINZ_WINTERMARKT_DESIGN.md is the specification. Maze details below (levels, sonar, enemies, handoff sections) describe the legacy Echo Maze in Assets/_EchoMaze.

Read the relevant sections of docs/archive/Echo_Maze_Claude_Code_Handoff.md and the current ticket before editing. The game is a real editable Unity 2D project with exactly three fixed levels, one sonar mechanic and one enemy type.

Inspect ProjectSettings/ProjectVersion.txt and installed editors; preserve the recorded version. The team uses 6000.3.25f1 (6.3 LTS); see issue 12. Keep runtime code free of UnityEditor references. Use the standard 2D renderer and original geometric sprites; no paid assets or new packages without a concrete need.

GameManager centralizes Playing, Paused, Caught, Transitioning and Victory. Stop movement/input outside Playing, freeze scaled timers on pause, and resolve only one outcome per frame. Use Rigidbody2D movement in FixedUpdate with normalized keyboard input. Ensure legacy input is enabled before relying on Input.GetKey.

Use Canvas and CanvasScaler for menus and HUD. GameManager currently references a missing MazeUI: see issue 1. Required Resources settings/sprite and scenes are also pending: see issue 2. Reinspect before assuming those gaps still exist.

Editor bootstrap must preserve hand-edited assets, configure layer 8 as Wall, generate required Resources assets and register scenes. Include generated .meta files. See the testing skill for verification; report compilation, Play Mode and standalone results separately.
