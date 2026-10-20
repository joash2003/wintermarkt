# ECHO MAZE
## Complete Game Design, Unity Implementation, Testing, and Delivery Handoff for Claude Code

**Document version:** 1.0  
**Prepared:** 17 October 2026  
**Project:** Echo Maze: Escape the Darkness  
**Course:** JKU Game Development, winter semester 2026/27  
**Target audience:** Claude Code implementing the project, plus three students who are new to game development  
**Status:** Game specification and build plan. **The game has not yet been implemented or tested.**

---

# 0. START HERE: INSTRUCTIONS FOR CLAUDE CODE

You are the lead Unity/C# implementation assistant for **Echo Maze**, a small, polished, single-player 2D stealth-puzzle game. Build it as a **real, editable, runnable Unity project**, not as a web game, Python mock-up, slide deck, collection of C# snippets, or prototype that only works in theory.

**Read this entire document first.** The gameplay specification and the priority order below are the source of truth. Do not add features or significantly change mechanics without asking. The people working on this game have **never built a Unity game before**, so prioritize simplicity, clean architecture, legible code, and step-by-step editor instructions.

### What you must do

1. Inspect the current repository and environment first. Determine whether Unity, its editor executable, C#, and Git are available. **Never claim to have opened Unity, compiled code, tested play mode, or produced a working build unless you actually did.**
2. If this directory is empty, create a Unity-ready project skeleton and, if possible, use the Unity Editor to generate scenes, prefabs, assets, and project settings. If Unity is unavailable, write the source/configuration and a safe editor bootstrapper to generate missing scene assets when Unity is installed. Clearly list what is unverified.
3. Prefer a supported **Unity 6 LTS editor** that the team can all install. Confirm the actual editor version and record it in `README.md`; never silently change an existing project's editor version. Use **2D Core / standard 2D rendering**. No purchased assets, shaders, third-party AI frameworks, external web services, or multiplayer.
4. Implement the **core loop** first: move, reveal nearby environment with sonar, cause enemies to investigate pulse noise, evade enemies, reach exit, retry on capture, progress through three levels.
5. Make the experience genuinely playable. Provide title, instructions, pause, level HUD, caught/retry, victory, sound on/off, and a usable keyboard layout.
6. Keep all gameplay values configurable in one place, and keep levels in readable data files that teammates can edit without coding.
7. Create **all required code and assets**, not merely a plan. Use minimal original geometric graphics if real art is unavailable; the game must not depend on fetching images from the internet.
8. Add validation/tests and documentation, and run the tests/build in the actual Unity environment wherever possible. Provide exact logs/results. If execution is blocked by missing software or credentials, stop at a reproducible checkpoint and give precise manual steps to continue.
9. Complete work in milestones with a usable game at each milestone. Do not spend time on polish until the first complete level is playable.
10. Explain how a beginner can start the editor, press Play, test each control, make simple changes, and create the final standalone build.

### Definition of done

The project counts as finished only when **all three levels can be played from the main menu through the victory screen, sonar and enemy investigation actually work, no known game-breaking bugs remain, build assets are present, the design document and credits are complete, and a reproducible desktop build has been created and verified**. If the machine cannot run Unity, mark the final build **not verified**, never finished.

### Suggested message to begin a Claude Code session

> Read `Echo_Maze_Claude_Code_Handoff.md` completely. Inspect this repository and available Unity tools. Start by summarizing what can be built and tested here. Then implement Phase 1 (project skeleton, level loading, first playable level) and continue milestone by milestone. Generate actual Unity assets and C# scripts whenever the tools allow. If Unity is unavailable, create a documented editor-bootstrap workflow and distinguish unverified code from tested functionality. Never invent test results or bypass acceptance criteria. Keep the game beginner-friendly and obey the exact MVP scope.

---

# 1. EXECUTIVE GAME CONCEPT

**One-sentence pitch:** *Navigate a dark maze using short sonar pulses that reveal hidden corridors but attract dangerous patrolling enemies.*

**Genre:** Single-player, top-down, 2D maze exploration + light stealth + puzzle.  
**Platform:** Desktop, keyboard first (Windows or macOS, depending on the team's installed Unity build support).  
**Engine:** Unity with C#.  
**Camera:** Fixed top-down orthographic camera.  
**Art:** Minimal dark sci-fi geometry, bright cyan player/pulses, red enemies, green exit.  
**Session length:** Roughly 5–10 minutes to complete three compact levels, depending on player skill. This is a design target, not a tested result.  
**Target audience:** Casual players who like short atmospheric puzzle and stealth games.

## 1.1 Fantasy and atmosphere

The player wakes in a dark research facility after the lights fail. Their handheld scanner emits sonar pulses. Each pulse briefly exposes the architecture and threats around them, but enemies can hear it. The player has to make a trade-off: **more information versus more danger**.

Keep the story deliberately minimal. There are no dialogues, inventory, combat, complicated character animations, cutscenes, or quest systems.

## 1.2 What makes this more than a basic maze

- The environment is mostly hidden by darkness.
- A sonar pulse reveals a wide area for a short period.
- A pulse is **helpful**, but also creates sound that enemies investigate.
- The limited energy meter prevents pressing Space constantly.
- Enemies patrol, investigate suspicious sounds, and pursue when they spot the player.
- Three handcrafted levels introduce the mechanics gradually.

The core question for the player is: **"Do I scan now and risk attracting an enemy, or keep moving with limited visibility?"**

## 1.3 Non-goals: do NOT implement

No online multiplayer; no procedural or infinite levels; no complicated lighting shaders; no real acoustic simulation; no combat; no inventory or crafting; no NPC dialogue; no dynamic physics puzzles; no advanced behavior trees; no neural-network AI; no save slots or cloud services; no controller support required; no mobile port; no monetization; no Steam integration. Avoid cosmetic settings screens, elaborate animations, and new enemy species until all core acceptance tests pass.

---

# 2. MVP GAMEPLAY SPECIFICATION

## 2.1 Core gameplay loop

1. The player starts in a dark maze. Their position and a **very small safety zone** around them are always visible.
2. They move through corridors using **W/A/S/D** or arrow keys.
3. They press **Space** to emit a sonar pulse. Nearby maze geometry, enemies, and the exit become visible for a short time.
4. Every pulse drains sonar energy and emits noise. Enemies near the pulse location investigate it.
5. Patrolling enemies may also notice the player at close range when there is an unobstructed line of sight.
6. Touching an enemy's capture radius triggers the **Caught** state. The player can restart the same level.
7. The player reaches the exit to finish the level and move to the next level.
8. Completing Level 3 leads to a **You Escaped** victory screen.

**Design rule:** The player is never required to use sonar while standing next to an enemy. At least one safe route to the exit must exist. Difficulties should come from navigating and managing information, not from random unavoidable death.

## 2.2 Controls

| Input | Behavior |
|---|---|
| W / Up arrow | Move up |
| S / Down arrow | Move down |
| A / Left arrow | Move left |
| D / Right arrow | Move right |
| Space | Fire sonar if enough energy and off cooldown |
| Esc | Pause/resume gameplay |
| R | Restart current level (optional shortcut, show on instructions) |
| Mouse | Click menu and pause buttons |
| M | Toggle sound mute (optional shortcut) |

Normalize diagonal movement to avoid faster movement diagonally. Input should not move the player when paused, caught, or transitioning between levels.

## 2.3 Default tuning values (editable)

These are **proposed starting values**, not course requirements or playtested final settings.

| Property | Starting value | Notes |
|---|---:|---|
| Grid tile width/height | 1 Unity unit | Same size in all levels |
| Player movement speed | 3.0 units/sec | Smooth, normalized |
| Player collision size | 0.55 x 0.55 units | Fit 1-unit corridors |
| Always-visible local radius | 1.2 units | Helps avoid frustration |
| Sonar reveal radius | 5.0 units | Circular, no complex shaders |
| Sonar reveal duration | 1.8 sec | Fog returns afterwards |
| Sonar pulse ring expansion | 0.45 sec | Simple cosmetic ring |
| Maximum energy | 100 | HUD displays energy |
| Sonar energy cost | 30 | Each successful pulse |
| Passive energy recharge | 8 units/sec | Allows more pulses over time |
| Sonar cooldown | 1.0 sec | Prevent double-firing |
| Enemy hearing radius | 6.0 units | From sonar origin |
| Enemy patrol speed | 1.5 units/sec | Slower than player |
| Enemy investigate speed | 2.0 units/sec | Uses grid pathfinding |
| Enemy chase speed | 2.5 units/sec | Slower than player |
| Enemy sight distance | 2.7 units | Requires clear line of sight |
| Enemy catch distance | 0.55 units | Triggers caught state |
| Enemy investigation wait | 1.5 sec | After reaching sound source |
| Enemy chase memory | 1.5 sec | Return to patrol if target lost |
| Navigation repath interval | 0.4 sec | For moving chase target |
| Fog base opacity | 0.96 | Almost black, hides layout |
| Nearby fog opacity | 0.35 | Just enough to avoid blind collision |
| Revealed fog opacity | 0.08 | Visibly clear after pulse |

**Important game-design distinction:** Sonar does not teleport, damage, stun, or blind enemies. Its only two gameplay effects are **temporary visual reveal** and **enemy hearing/investigation**. The sonar energy meter regenerates automatically; there is no consumable pickup system.

## 2.4 Rules and win/loss conditions

- Players can only walk through traversable floor cells; walls are solid.
- The camera stays fixed on the entire compact level, or fits the level by orthographic size.
- A sonar pulse requires both sufficient energy and the cooldown to have elapsed. Failed attempts consume no energy and do not alert enemies.
- Enemies are not initially chasing; they follow patrol waypoints.
- On sonar, enemies within hearing radius receive an investigation target at the **player's position at pulse time**, not the player's continuously updated position.
- If the enemy can see the player at close range, they chase the player's most recently visible position.
- Reaching the level exit wins the level; being caught loses that attempt.
- Restart resets enemy positions, energy, reveals, and current-level progress.
- There is no overall game-over limit or player health bar.

## 2.5 State transitions

```text
APP START
  -> Main Menu
  -> Play
  -> Level 1 (Playing)
      -> Caught -> Retry Level 1
      -> Reached Exit -> Level 2
  -> Level 2 (Playing)
      -> Caught -> Retry Level 2
      -> Reached Exit -> Level 3
  -> Level 3 (Playing)
      -> Caught -> Retry Level 3
      -> Reached Exit -> Victory Screen
  -> Back to Main Menu

At any Playing state:
  Esc -> Paused -> Resume / Restart / Main Menu
```

Transition state changes must be centralized so caught/win cannot both trigger from the same frame. Prevent two scene loads for one event.

---

# 3. LEVEL DESIGN

**Exactly three handcrafted levels** for the MVP. Do not add random generation. These sample maps are proposed fixed designs that Claude Code can place into JSON level data and adjust after playtesting. Symbols and coordinate conventions below are part of the implementation contract.

## 3.1 ASCII grid format

Each line represents a row of cells, **top to bottom**. Characters are **left to right**.

| Character | Meaning |
|---|---|
| `#` | Wall: visible when revealed, impassable |
| `.` | Floor: traversable |
| `P` | Player spawn, traversable |
| `X` | Exit tile, traversable |
| `E` | Enemy spawn, traversable |

Use `x = column index (starting at 0)`, `y = row index (starting at 0)`. Convert grid cells to world positions using **`(x, -y, 0)`**. Start with one tile per Unity world unit and a one-unit player movement reference. The JSON level parser must verify rectangular rows, supported symbols, exactly one player and exit, and valid enemy spawn/patrol data. Convert `P`, `X`, and `E` to floor after recording their locations.

## 3.2 Level 1: Signal Lost (tutorial)

- **Goal:** Find the exit using sonar.
- **Enemies:** None.
- **Teaches:** Movement, darkness, energy, pulse, exit.
- **Visual theme:** Cool blue corridors.
- **HUD hint:** `WASD to move. Press SPACE to scan.`
- **Size:** 13 x 9 cells.

```text
#############
#P#.........#
#.#.###.###.#
#...#.......#
#####.#.##.##
#.#...#.....#
#.#.#.###.#.#
#.........#X#
#############
```

The team may adjust corridors if the route feels overly long. The path must remain reachable and contain no forced obstacle that needs an unimplemented mechanic.

## 3.3 Level 2: Something Heard You (one enemy)

- **Goal:** Reach the exit without being caught.
- **Enemies:** One enemy patrolling the map.
- **Introduces:** Hearing from sonar, investigation, capture/retry.
- **HUD hint:** `Your sonar attracts nearby enemies.`
- **Size:** 17 x 11 cells.

```text
#################
#P........#E....#
###.#.#####.###.#
#...#.......#...#
#.#####.#####...#
#.#.......#...#.#
#...#.#####.###.#
#...#.....#.#...#
#.#.##..#.#.#.###
#...#.......#..X#
#################
```

Enemy 0 initial position = `(11,1)`. Patrol stops at `(11,1)` and `(14,1)` using grid pathfinding. If waypoints are redesigned, all must be on reachable floor tiles.

## 3.4 Level 3: Last Transmission (two enemies)

- **Goal:** Navigate a larger maze with two patrolling enemies and escape.
- **Enemies:** Two with identical simple behavior, different patrol routes.
- **Introduces:** Choosing when and where to scan near more than one threat.
- **HUD hint:** `Watch your energy and time your pulses.`
- **Size:** 21 x 13 cells.

```text
#####################
#P........#.........#
###.###.#.####..#.#.#
#.#.....#E.....E#.#.#
#.#############...#.#
#...#.............#.#
#.#.#.#.#####.#.#.###
#.#...#.#.....#.#...#
#.#####.#.#.#.#.#.#.#
#...#...#.#.........#
#.#.###.#.###.#####.#
#.#.........#......X#
#####################
```

Enemy 0 initial = `(9,3)`, patrol = `(9,3)` and `(12,3)`.  
Enemy 1 initial = `(15,3)`, patrol = `(15,3)` and `(17,4)`.  
Use the same enemy AI component for both; do not create two kinds of enemy.

## 3.5 Required level-data representation

Store editable files at:

```text
Assets/_EchoMaze/Resources/Levels/level_01.json
Assets/_EchoMaze/Resources/Levels/level_02.json
Assets/_EchoMaze/Resources/Levels/level_03.json
```

Suggested schema compatible with Unity `JsonUtility` (use public serializable classes/arrays, not `Dictionary`):

```json
{
  "id": 2,
  "title": "Something Heard You",
  "rows": [
    "#################",
    "#P........#E....#"
  ],
  "enemyPatrols": [
    {
      "spawnX": 11,
      "spawnY": 1,
      "waypoints": [
        {"x": 11, "y": 1},
        {"x": 14, "y": 1}
      ]
    }
  ]
}
```

**The JSON above is a schema example with truncated `rows`**, not a runnable level file. Claude must write all complete rows from the maps above. The number/order of `E` characters must agree with `enemyPatrols`. The loader must validate waypoint reachability and report clear errors. A small editor validation menu should validate every level before Play.

## 3.6 Level progression and scope cuts

Difficulty increases via maze size and number of enemies. **Do not** add locked doors, keys, switches, pressure plates, combat, traps, collectibles, randomly shifting walls, or boss encounters. If time is tight, make Level 2 a satisfying complete game and use Levels 1 and 3 as variations of the same systems, not as excuses to create new ones.

---

# 4. VISUAL DESIGN AND SOUND

## 4.1 Art direction

- Top-down 2D, readable tile grid, no 3D models.
- Background/ground: nearly black charcoal or navy.
- Walls: blue-grey blocks with subtle outlines, visible when scanned.
- Player: cyan circle or diamond with a small glow.
- Enemy: red square/robot icon; distinguishable at a glance.
- Exit: green doorway/rectangle, revealed by sonar or nearby proximity.
- Pulse: cyan circular outline expanding outward and fading.
- GUI: minimal flat design, with large legible labels.

**First implementation must use original geometry and runtime/editor-generated simple sprites** (plain square/circle/line). It should play correctly without any downloaded asset. After gameplay is stable, replace with properly credited CC0/free assets if desired.

## 4.2 Fog of war: exact minimum approach

**Use a grid of overlay sprites, not a custom shader.**

1. Create one fog square for every non-border map cell (or every cell for simplicity), aligned to the tile grid.
2. Render floor, wall, exit, and enemies **below** the fog overlay. Render the player and pulse ring **above** it, so the player is never invisible.
3. Fog alpha should be around `0.96` by default, `0.35` for nearby cells, and `0.08` for tiles that have been revealed by sonar.
4. Update local visibility and pulse reveal at a reasonable rate (for example 10 times/sec; a few hundred cells are acceptable at this scale).
5. When a pulse occurs, mark all cells within radius 5 with a reveal-expiration timestamp (`Time.time + 1.8f`). Show the expanding ring for 0.45 sec.
6. After expiration, the tile returns to its default/local fog state. Avoid allocation-heavy LINQ in `Update`.
7. On pause, timing must effectively freeze; Unity scaled `Time.time` handles pause if `Time.timeScale = 0`.
8. The player can see near themselves even when sonar energy is depleted, preventing totally blind play.

No obstacle-occlusion calculation is required for sonar. Sonar can reveal tiles through walls in a radius; treat it as scanning the local layout, not visible-light line-of-sight. **Enemy sight**, however, *must* be blocked by walls.

If full fog spawning causes visual issues, the fallback is to hide/show individual sprite renderers on a grid. Preserve the same gameplay behavior and write down the simplification.

## 4.3 Sorting layers and rendering order

Use dedicated sorting layers or consistent sprite `sortingOrder` values:

```text
Ground       0
Walls       10
Exit        15
Enemies     20
Fog         30
Player      40
Pulse       45
UI          screen-space overlay canvas
```

An enemy outside the visible region should be hidden under fog until a pulse or local visibility reveals them. Pause/HUD/menu are never hidden by fog.

## 4.4 Sound design

Minimum sounds: sonar ping, enemy alert, caught, exit/win, simple menu click. Optional ambient hum is low priority. Use simple procedurally generated beeps or assets with demonstrably compatible licenses. The sonar ping must be distinct; enemy investigation should have a subtle alert cue. Include adjustable mute (at least a menu toggle).

**Asset license rule:** Any external textures, fonts, audio, or sprites require their source and license in final `readme.txt` / `ASSET_CREDITS.md`. Original/generated geometry requires no external attribution; document that it was generated in the project.

---

# 5. ENEMY ARTIFICIAL INTELLIGENCE (SMALL STATE MACHINE)

Use a **single enemy prefab and one state machine**, no NavMesh and no complex behavior tree.

## 5.1 States

| State | Meaning | Transition out |
|---|---|---|
| `Patrol` | Follow a small loop of assigned waypoints | Heard sonar -> Investigate; saw player -> Chase |
| `Investigate` | Move to position where sonar was fired, wait 1.5 sec | Saw player -> Chase; investigation done -> Patrol |
| `Chase` | Follow player/last known tile while player is in sight | Lost sight for 1.5 sec -> Investigate last seen tile, then Patrol |
| `Disabled` | Game caught/paused/end state | No movement |

Priority: **Chase > Investigate > Patrol**. If a new sonar is heard while investigating, replace investigation destination with the most recent pulse location (unless chasing). Make state transitions visible through enemy color/intensity or small alert icon if simple to implement.

## 5.2 Hearing

When player **successfully** fires sonar, emit one `SonarEmitted(Vector2 worldPosition)` gameplay event. Each active enemy checks distance from its current location to the ping origin. If distance `<= 6.0f`, it receives an investigation command.

Do **not** automatically chase the player's updated position because of sonar. The noise only reveals where the player **was when they pinged**. Enemies may later spot the player normally.

## 5.3 Vision

Once per update interval, enemy checks:

- Player within 2.7 units?
- Unobstructed line of sight? Use `Physics2D.Linecast`/`Raycast` with a **Wall** layer mask (ensure all wall colliders are in that layer). Or test traversability along the line if necessary.

If yes, enter Chase. The player does not become invisible to the enemy merely because the *screen* fog is covering the map. Fog is a visual mechanic for the human player, not a physical wall or stealth field.

## 5.4 Pathfinding

Implement **grid-based breadth-first search (BFS)** on passable map cells. Each tile has up to four neighbors: north, south, east, west; no diagonal movement and no terrain weights. For this small grid BFS is sufficient and easy to test.

- Validate start and destination are traversable.
- Convert target world position to nearest valid grid tile.
- Return shortest sequence of cells as waypoint centers.
- Enemies follow these waypoints smoothly at their state-specific speed.
- Recalculate chase route every ~0.4 sec and/or when the player's grid cell changes.
- Patrol route waypoint-to-waypoint using BFS, including routes around corners.
- If no route exists (bad data), log an actionable warning and safely return to patrol rather than freeze the entire game.

**Avoid enemy collision complexity:** Enemies follow tile centers on grid paths. Use a distance-based catch check rather than requiring physical enemy/ player collision components. Keep enemies from visually overlapping walls by using BFS.

## 5.5 Player capture and fairness

If the distance between an active enemy and player is `<= 0.55f` while playing, trigger Caught only once. Pause motion, show `Caught! Your sonar gave you away.` and offer `Retry` and `Main Menu` buttons.

During playtesting, confirm there is a reasonable escape opportunity; a newly spawned enemy must not instantly catch the player, and the victory exit must remain reachable by walking.

---

# 6. UNITY PROJECT STRUCTURE

Use this repository layout (some files are generated by Unity):

```text
EchoMaze/
├── README.md
├── CLAUDE.md
├── Echo_Maze_Claude_Code_Handoff.md
├── readme.txt                   # Final submission / external asset credits
├── ASSET_CREDITS.md
├── CHANGELOG.md
├── TESTING.md
├── DESIGN_LOG.md
├── .gitignore
├── Assets/
│   └── _EchoMaze/
│       ├── Scenes/
│       │   ├── MainMenu.unity
│       │   ├── Game.unity
│       │   └── Victory.unity
│       ├── Scripts/
│       │   ├── Core/
│       │   │   ├── GameManager.cs
│       │   │   ├── GameState.cs
│       │   │   ├── GameSettings.cs
│       │   │   └── GameBootstrap.cs
│       │   ├── Grid/
│       │   │   ├── LevelData.cs
│       │   │   ├── LevelLoader.cs
│       │   │   ├── LevelValidator.cs
│       │   │   ├── GridMap.cs
│       │   │   └── GridPathfinder.cs
│       │   ├── Player/
│       │   │   ├── PlayerController.cs
│       │   │   ├── SonarController.cs
│       │   │   └── SonarEnergy.cs
│       │   ├── Enemies/
│       │   │   ├── EnemyController.cs
│       │   │   ├── EnemyState.cs
│       │   │   └── EnemyVision.cs
│       │   ├── World/
│       │   │   ├── FogOfWar.cs
│       │   │   ├── SonarPulseRing.cs
│       │   │   └── ExitController.cs
│       │   ├── UI/
│       │   │   ├── MainMenuUI.cs
│       │   │   ├── HUDController.cs
│       │   │   ├── PauseMenuUI.cs
│       │   │   └── EndScreenUI.cs
│       │   └── Audio/
│       │       └── AudioManager.cs
│       ├── Resources/
│       │   └── Levels/
│       │       ├── level_01.json
│       │       ├── level_02.json
│       │       └── level_03.json
│       ├── Prefabs/
│       │   ├── Player.prefab
│       │   ├── Enemy.prefab
│       │   ├── Wall.prefab
│       │   ├── Floor.prefab
│       │   ├── FogCell.prefab
│       │   ├── Exit.prefab
│       │   └── SonarRing.prefab
│       ├── Materials/
│       ├── Sprites/
│       ├── Audio/
│       ├── Editor/
│       │   ├── EchoMazeProjectBootstrap.cs
│       │   ├── EchoMazeLevelValidationMenu.cs
│       │   └── EchoMazeBuildAutomation.cs
│       └── Tests/
│           ├── EditMode/
│           │   ├── LevelValidatorTests.cs
│           │   ├── GridPathfinderTests.cs
│           │   └── SonarEnergyTests.cs
│           └── PlayMode/
│               └── GameFlowSmokeTests.cs
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
└── ProjectSettings/
    └── ProjectVersion.txt
```

Do not make empty placeholder files just to match this tree. Add functional scripts and assets incrementally. Smaller combinations of related scripts are acceptable if they make the beginner project easier to maintain, but preserve clear module boundaries.

**Git:** Commit Assets, their `.meta` files, Packages, and ProjectSettings. Exclude generated `Library/`, `Temp/`, `Obj/`, `Logs/`, `Build/`, `Builds/`, `.vs/` and machine-specific IDE caches. Start from a standard Unity `.gitignore`. A teammate should be able to clone the repository, open it in the recorded editor version, and run it.

---

# 7. SCENES, PREFABS, COMPONENTS, AND CONFIGURATION

## 7.1 Scene 1: MainMenu

Suggested hierarchy:

```text
MainMenu
├── Main Camera (Orthographic)
├── UI Canvas (Screen Space Overlay)
│   ├── Title: ECHO MAZE
│   ├── Subtitle: Escape the Darkness
│   ├── Play Button
│   ├── How To Play Button (panel)
│   ├── Mute Toggle
│   └── Quit Button
├── UI EventSystem
└── AudioManager (or persistent object)
```

The `How To Play` panel must explain movement, sonar, energy, enemies, exit, retry, pause. Buttons require actual click handlers. Quit must work in a standalone build; in Editor, it can log that quit was requested.

## 7.2 Scene 2: Game

**Use one reusable game scene for all three levels, populated from JSON by `LevelLoader`.** Avoid three copied near-identical game scenes.

```text
Game
├── Main Camera (Orthographic)
├── GameManager
├── LevelLoader
├── RuntimeLevelRoot (generated at level load)
│   ├── FloorTiles
│   ├── WallTiles (Wall layer)
│   ├── Exit
│   ├── Player
│   ├── Enemies
│   ├── FogGrid
│   └── SonarRings
├── UI Canvas (Screen Space Overlay)
│   ├── Level Name
│   ├── Energy Meter
│   ├── Contextual Instructions
│   ├── Pause Panel (inactive at first)
│   ├── Caught Panel (inactive at first)
│   └── Level Complete Panel (if used)
├── UI EventSystem
└── AudioManager
```

Use **Rigidbody2D with zero gravity and frozen Z rotation** for player movement, and a 2D collider. Solid wall squares use static `BoxCollider2D` and are placed on the `Wall` physics layer. Keep a player collision box small enough to move through 1-tile corridors. Use `FixedUpdate` for Rigidbody movement, input gathering in `Update`, and normalize diagonal input. Build empty or inactive overlays into the scene, rather than instantiating entire menus per button click.

## 7.3 Scene 3: Victory

```text
Victory
├── Main Camera
├── UI Canvas
│   ├── YOU ESCAPED
│   ├── Short completion message
│   ├── Play Again Button
│   └── Main Menu Button
└── UI EventSystem
```

All three scenes must be included in Unity build settings / build profiles. If Claude chooses to implement the victory screen as a panel in the Game scene instead, that is acceptable if it is fully functional; record the deviation.

## 7.4 Inspector wiring

Expose `[SerializeField]` references or use a lightweight bootstrap so scene objects are reliably connected. Avoid brittle `GameObject.Find("Some Arbitrary Name")` calls for critical systems. Serialize prefabs, HUD references, wall layer mask, runtime root, and configuration. Fail fast with clear errors when required references are missing.

## 7.5 Unity input and UI dependency

For the simplest reproducible beginner build, prefer keyboard polling with standard Unity `Input.GetKey` and `Input.GetKeyDown` **after confirming Active Input Handling is set to `Both` or the legacy Input Manager**. If the team's Unity version defaults to the new Input System only, either set the project appropriately or switch consistently to the Input System package; never mix working and nonworking APIs. Document the selection in README.

For UI use normal Unity Canvas + UI components. TextMeshPro is fine if present and its essential assets are installed; do not assume a missing TMP dependency has been resolved. Do not let a font/package issue block gameplay.

## 7.6 Editor project generator / bootstrap

Claude may not be able to manipulate Unity's GUI directly. Implement an **Editor-only** class accessible through `Tools > Echo Maze > Create/Repair Project` that can:

1. Generate simple sprite textures/materials and the reusable prefabs.
2. Create/configure scenes with cameras, sorting, canvases, buttons, and game systems.
3. Create correct tags/layers or show clear manual instructions if a project setting cannot be generated safely.
4. Include scenes in build settings.
5. Set `MainMenu` as starting scene.
6. Run level-data validation.
7. Detect existing user assets and **never silently overwrite hand-edited work**. Prefer idempotent operations or an explicit confirmation/back-up step.

The same bootstrap should be callable by a documented batch-mode Editor method when feasible. `UnityEditor` APIs must not appear in player runtime assemblies.

---

# 8. C# SYSTEM CONTRACTS

These responsibilities are **interfaces/behavior contracts**, not exact required method signatures. Claude may rename methods, but every functionality must exist and be testable.

## 8.1 `GameManager`

- Owns current state: `MainMenu`, `Playing`, `Paused`, `Caught`, `Transitioning`, `Victory`.
- Tracks current level index 1 to 3.
- Loads level from JSON; resets local state on retry.
- Receives `OnPlayerCaught` and `OnExitReached` exactly once per outcome.
- Controls pause and menus; restores `Time.timeScale = 1` before loading/restarting scenes.
- Exposes events to HUD and audio to avoid deep cross-system dependencies.

## 8.2 `LevelData`, `GridMap`, `LevelLoader`, `LevelValidator`

- Parse ASCII rows and enemy patrol metadata from JSON.
- Distinguish static walls, walkable tiles, player start, exit, enemy starts.
- Build floor/wall sprites and static collision geometry from cells.
- Position player, enemy instances, exit, and fog overlay with correct grid-to-world conversion.
- Fit the orthographic camera to show the level with margins at a typical desktop aspect ratio; avoid letting the player move off-screen. Offer letterboxing/zoom as appropriate.
- Validate maps with BFS: exactly one `P` and `X`, consistent row lengths, connected entry-to-exit route, valid enemy spawn locations, every patrol waypoint walkable/reachable, matching enemy count.
- Give clear logs and stop an invalid level from running silently.

## 8.3 `PlayerController`

- Reads WASD/arrows (plus any chosen input bindings).
- Move using Rigidbody2D, wall collisions, normalized diagonal velocity.
- Calls SonarController on Space **only while Playing**.
- Is visible over fog; may add a subtle pulsing outline without requiring animation assets.

## 8.4 `SonarEnergy` / `SonarController`

- Holds energy range `[0,100]` with passive recharge at 8/sec while Playing.
- On Space request: check state, cooldown, and energy >= 30. If valid, subtract 30 **once**, set cooldown, emit pulse event with origin, start pulse ring animation, reveal fog, play sound, notify HUD.
- Invalid pulse requests cause no noise and consume no energy. Optionally play a quiet blocked-input cue.
- Clamp energy within limits, including large time-step changes.
- Expose current and max energy for a progress bar; do not tie game logic to the UI.

## 8.5 `FogOfWar`

- Owns fog cells and expiration times.
- Distinguishes always-nearby visibility from temporal sonar-reveal visibility.
- Supports `RevealPulse(origin, radius, duration)` and `ResetFog()`.
- Handles overlapping pulses predictably: refreshing a tile's reveal expiry must **not** prematurely hide a tile revealed by a later pulse.
- Does not affect physics or enemy pathfinding.
- Uses world positions consistent with the loader.

## 8.6 `SonarPulseRing`

- Visual circle expands from 0 to radius ~5 over 0.45 sec and fades out.
- Use `LineRenderer` with a circular set of segments and appropriate 2D sorting, or a generated transparent sprite scaled up.
- **No particle-system requirement.** Pooling is optional; a few pulses/sec are inexpensive.
- Destroy/disable itself after animation; no leaking objects.

## 8.7 `EnemyController` / `EnemyVision`

- One state machine for patrol, investigate, chase.
- Separate movement/pathfinding from state-selection where practical.
- Reacts to sonar **origin**, not a hidden global player target.
- Uses grid BFS for movement around walls.
- Line-of-sight uses the Wall layer and 2D raycasts; check layer mask correctness.
- Uses safe repathing intervals and stops movement while Paused, Caught, Transitioning.
- Emits capture once when within distance threshold.
- Starts at appropriate waypoints, does not run into walls or get stuck indefinitely.

## 8.8 `ExitController`

- Detects the player at the exit (trigger collider or distance <= 0.5 cell).
- Asks GameManager to finish level; cannot trigger a second time.
- May produce a subtle green flash/sound on success.

## 8.9 UI controllers

- Main menu: Play, instructions, mute, Quit.
- HUD: level name, sonar energy, one short tutorial hint.
- Pause: Resume, Retry, Main Menu.
- Caught: short message, Retry, Main Menu.
- Victory: Play Again, Main Menu.
- Button callbacks must never reference scene objects destroyed during transition.
- Use `CanvasScaler` for common desktop resolutions and consistent UI spacing.

## 8.10 `AudioManager`

- One small reusable service; store volume/mute preference optionally via PlayerPrefs.
- Sound effects are short; avoid heavy audio dependencies.
- Don't allow audio to spam on invalid sonar presses.

---

# 9. STEP-BY-STEP IMPLEMENTATION PLAN FOR CLAUDE CODE

**Key development principle:** At the end of each phase there must be a checkpoint that someone can run and demonstrate. Do not write all scripts first and only then discover that none of the scenes are connected.

## Phase 0: Verify tools and create repository

Tasks:

1. Check Unity Editor availability/version, C# support, operating system, Git.
2. Create or open Unity **2D Core** project `EchoMaze`.
3. Create folders, `.gitignore`, README, `CLAUDE.md`, `DESIGN_LOG.md`, `TESTING.md`, and short issue board.
4. Establish collaboration: one Git repository, unique feature branches if used, no commits of Unity Library/temp files.
5. Document the exact editor version and setup steps.
6. Set 16:9-oriented presentation, appropriate desktop target, wall physics layer, and scene list.

**Acceptance:** Project opens with no missing package/script error and a baseline scene runs in Unity. If Unity cannot be opened in the current environment, state this explicitly and generate editor steps/bootstrapper instead.

## Phase 1: First playable level without enemies

Tasks:

1. Implement JSON level classes, loader, grid conversion, and validator.
2. Add Level 1 data, floor/wall sprites, camera, exit and player spawn.
3. Add player keyboard movement and solid walls.
4. Add a minimal on-screen instruction label and exit detection.
5. Make exiting Level 1 show a simple completion screen/panel (temporarily acceptable).
6. Confirm level entry/exit connectivity with BFS validation.

**Acceptance:** Press Play, move around the first maze, cannot pass through walls, reach exit successfully, no runtime exceptions.

## Phase 2: Darkness and sonar (main unique mechanic)

Tasks:

1. Generate fog-cell sprites aligned to grid.
2. Make a tiny region around player visible without pulse.
3. Add sonar energy, recharge, cooldown and `Space` input.
4. Reveal fog in a circle and smoothly return it after the timer.
5. Add cyan sonar pulse ring and a simple pulse sound.
6. Add energy HUD and clear feedback for successful pulse.

**Acceptance:** Sonar visibly reveals nearby map, energy drops by exactly 30, recharges while Playing, cannot fire below 30, overlapping reveals expire correctly, cooldown works, game is completable.

## Phase 3: Enemy behavior and Level 2

Tasks:

1. Implement BFS grid pathfinding with edit-mode tests.
2. Create one enemy prefab, patrol state and two patrol waypoints.
3. Add hearing event and movement to sonar origin (Investigate).
4. Add unobstructed short-range sight and Chase.
5. Add capture/retry screen and verify full reset.
6. Load Level 2 from JSON and confirm gameplay is fair.

**Acceptance:** Enemy patrols around walls; sonar within hearing range causes investigation; sonar far away does not; short-range line of sight triggers chase, walls block vision, catching player shows Retry, and exit advances to Level 3.

## Phase 4: Third level and complete game flow

Tasks:

1. Add Level 3 with two instances of the **same** enemy prefab.
2. Polish player/exit UI and level introductions.
3. Add complete main menu, pause, caught, and victory screens.
4. Add menu navigation and transitions; prevent repeated loads.
5. Add sound on/off and basic sound effects.
6. Test replay of all three levels from start to finish.

**Acceptance:** A player can launch the game, learn controls, complete all three levels, retry after being caught, pause/resume, and return to main menu without game-breaking errors.

## Phase 5: Playtesting and refinement

Tasks:

1. Conduct 2–3 internal smoke tests, then 3–5 playtests with people unfamiliar with the project if available.
2. Observe confusion, repeated deaths, difficulty, and level completion time.
3. Log issues with evidence and prioritize serious issues.
4. Adjust energy, hearing radius, enemy speed, reveal radius, or maze geometry **one/few at a time**.
5. Improve controls, tutorial clarity, readable contrast, sound feedback, and transition pacing.
6. Avoid adding new systems. Only add cosmetic polish once playtest problems are addressed.

**Acceptance:** External testers can understand the controls and finish the game with manageable difficulty. Results are recorded without claiming invented data.

## Phase 6: Build, quality assurance and final course handoff

Tasks:

1. Fix compile errors, broken references, scene order, invalid paths and UI sizing.
2. Run edit-mode tests and manual smoke tests; if available run Unity play-mode tests.
3. Build desktop standalone for the chosen operating system using Unity Build Profiles/Build Settings.
4. Launch **the exported build**, not only Unity Play Mode, and complete at least one full run of all levels.
5. Verify audio, fonts, texture rendering, scene transitions, input and build path.
6. Update README, `readme.txt`, asset attribution, change log, and weekly design slides.
7. Remove unused assets and development-only packages as appropriate.
8. Package the exported build and any requested source files following Moodle's actual final submission instructions; confirm these instructions with the course team if unclear.
9. Prepare a short demonstration/recording showing the mechanic, enemy hearing, and a complete level.

**Acceptance:** Standalone build launches and is playable through victory on the stated target OS, required documentation exists, credits are complete, team can reproduce the build, and no critical bugs remain.

---

# 10. PROJECT MILESTONES MATCHING THE COURSE

The deadlines below come from the uploaded 2026 kickoff lecture and assignment screenshot. **October 19 is the actual first submission deadline.** The workshop encourages fast prototypes and weekly iteration.

| Date | Course event | What Echo Maze should show |
|---|---|---|
| **19 Oct 2026, 23:59** | Initial design document due | Google Slides link: title, concept, gameplay, rules, sketches, workload and schedule. **Game code not required for this first submission.** |
| **20 Oct 2026** | Progress presentation | Pitch, gameplay flow, plan for a first playable level |
| 27 Oct + 3 Nov 2026 | Unity lectures | Project boots, basic top-down movement, walls, exit |
| 10–17 Nov 2026 | Lectures and progress presentation | Sonar/fog working, enemy pathfinding or early AI, evidence of progress |
| **1 Dec 2026** | Playtesting: playable prototype | One complete playable loop: sonar, enemy, exit, retry; ideally all three levels in rough form |
| **12 Jan 2027** | Playtesting: almost-finished game | All three levels, UI, polished mechanics, few major bugs |
| 19 Jan 2027 | Progress presentation and Q&A | Show improvements based on playtesting |
| **26 Jan 2027** | Final presentation | Present and let others play; short compelling pitch/video |
| **1 Feb 2027, 23:59** | Final submission (kickoff schedule) | Finished game build and requested hand-in files |

**Course-slide inconsistency:** Another kickoff assessment paragraph refers to a past year (`January 21, 2025`) for a game hand-in. Treat the **2026/27 dated schedule** as the planning reference, but verify the final actual Moodle deadline and required final files before submission.

## 10.1 Workload and team size

The kickoff slides describe **4.5 ECTS**, roughly **75 hours per student without lectures**, or **225 hours for a three-student group**. The same slides say groups normally have **three students** and that initial design-document submission requires at least three. **Confirm team size now:** if the group is four people total, ask the course team whether that is allowed rather than assuming it is.

## 10.2 Equal division of responsibilities

Use actual team member names when they are confirmed. The following is an initial plan, not evidence of completed work.

| Member | Primary owner | Also contributes to |
|---|---|---|
| **Person A** | Player controller, sonar energy, pulse ring, fog integration | Gameplay tuning, code reviews, playtesting |
| **Person B** | Level loader, map layouts, BFS, enemy patrol/hearing/chase | Level balancing, code reviews, playtesting |
| **Person C** | Menus/HUD, sound, aesthetic polish, editor/build pipeline, documentation | Integration, QA, usability testing |

Aim for **comparable total hours**, not identical complexity per script. Person C's role includes substantial integration, builds and QA. Rebalance based on weekly logged time.

Track every task in a table like:

| Task | Owner | Planned completion | Actual completion | Status | Evidence |
|---|---|---|---|---|---|
| Initial design deck | A+B+C | 19 Oct | Pending | Planned | Slides URL |
| Unity project skeleton | A | 27 Oct | Pending | Planned | Git commit |
| First maze/exit | B | 3 Nov | Pending | Planned | Demo/commit |
| HUD and menu shell | C | 10 Nov | Pending | Planned | Screenshot/commit |
| Sonar and fog | A | 10 Nov | Pending | Planned | Demo/commit |
| Enemy and Level 2 | B | 17 Nov | Pending | Planned | Demo/commit |
| First integrated prototype | A+B+C | 1 Dec | Pending | Planned | Playtest build |
| Levels/UI/polish | A+B+C | 12 Jan | Pending | Planned | Playtest build |
| Test fixes and final build | A+B+C | 26 Jan | Pending | Planned | Build archive |
| Final submission | A+B+C | 1 Feb | Pending | Planned | Submission proof |

The owner/date entries here are **proposals**. Update with the team's agreement. Never record tasks as completed until verified.

---

# 11. INITIAL DESIGN DOCUMENT: CONTENT FOR GOOGLE SLIDES

The actual graded **design document is a slide deck updated every week**, not this Markdown file. Use this document to prepare the slides. Do **not** treat the game specification as something Moodle automatically accepts instead of the slide link.

Suggested initial deck (8–10 slides; the professor did not provide a fixed slide count):

1. **Title + team**: Echo Maze: Escape the Darkness, three students, date.
2. **Concept**: One-sentence pitch, premise, target experience.
3. **Core loop**: Move -> scan -> reveal -> enemy investigates -> evade -> exit.
4. **Controls and game rules**: WASD/arrows, Space, win/lose, energy.
5. **Formal game elements**: Player, objective, procedures, rules, resource, conflict, boundaries, outcome.
6. **Mechanics/dynamics/aesthetics (MDA)**: Pulse/energy/hearing -> information-versus-danger tension -> suspense/discovery.
7. **Visual and sound direction**: Rough maze drawing, palette, pulse effect, references clearly marked as references.
8. **Technical approach and MVP scope**: Unity 2D, C#, grid, no advanced AI/multiplayer, three levels.
9. **Team responsibilities**: Who owns what, milestones, planned/actual dates.
10. **Roadmap/next steps**: Show December prototype milestone and weekly update plan.

The slides should document **why** design decisions were made, not just list features. Each week append or update a small `Progress this week / Problems found / Decisions made / Next steps` section, with screenshots and playtest findings when they exist.

**Initial deck deadline:** Monday **19 October 2026 at 23:59**. Submit a **shareable, accessible slide-deck link** in Moodle (not a fabricated URL). Verify link access in a private/incognito window or with a teammate.

---

# 12. SOFTWARE SETUP FOR FIRST-TIME UNITY DEVELOPERS

## 12.1 Install once per student

1. Install **Unity Hub** from the official Unity website.
2. Install the same current supported **Unity 6 LTS** Editor version for all members. Record the exact minor/patch version in `ProjectSettings/ProjectVersion.txt` and README.
3. Use the **2D Core** template for a new project; do not install heavy 3D/URP packages unless there is a real requirement.
4. Install a C# editor such as Visual Studio Community or VS Code with appropriate C# tooling.
5. Install Git and choose a collaboration repository.
6. Open the project in Unity Hub; allow the first asset import to complete.
7. Use `Tools > Echo Maze > Create/Repair Project` if the scenes/assets have not been generated yet (provided by the Claude-generated Editor bootstrap).
8. Open `Assets/_EchoMaze/Scenes/MainMenu.unity`, press the triangle **Play** button.
9. Test movement/sonar in the Game scene and confirm no red errors in the Console.

**macOS note:** Visual Studio for Mac is no longer the default option. VS Code + appropriate C# tooling or another compatible IDE is fine. A Windows standalone build requires the right Unity build-support module even if development occurs on macOS.

## 12.2 How the team works in Git

- Pull latest before starting a session.
- Choose an assigned issue, create a branch if appropriate.
- Make one focused change, test it, commit with a descriptive message.
- Merge only after someone else checks it, or use an agreed small-team workflow.
- Do not edit the same Unity scene concurrently without coordinating.
- Do not commit machine-generated `Library/`, `Temp/`, editor caches or personal settings.
- Always include Unity `.meta` files; missing `.meta` files can break prefab references.

## 12.3 What the team needs to understand in Unity

- **Scene:** One screen/environment (main menu, reusable game, victory).
- **GameObject:** Any object placed in the scene.
- **Component:** Behavior attached to a GameObject, such as `SpriteRenderer` or a C# script.
- **Prefab:** Reusable saved object template, e.g. enemy or wall.
- **Rigidbody2D + Collider2D:** Movement and walls that block movement.
- **SpriteRenderer:** Draws a sprite in 2D.
- **Canvas:** Draws menus and HUD.
- **Inspector:** Edits component settings and references.
- **Console:** Shows compile errors and debug logs.

Documentation should use these terms and include concise beginner screenshots/instructions if the final README warrants them.

---

# 13. TEST PLAN: FUNCTIONAL, TECHNICAL, AND PLAYTESTING

## 13.1 Automated EditMode tests

Create real Unity Test Framework tests where possible:

| Test | Expected result |
|---|---|
| Parses each full level JSON | No exceptions, expected width/height |
| Validates all row lengths | Every row in a level has same width |
| Rejects missing `P` or `X` | Actionable validation error |
| Rejects unknown grid character | Actionable validation error |
| Exit reachable from player spawn | BFS finds nonempty path in all 3 levels |
| Patrol waypoints are on floor | All waypoint coordinates valid and reachable |
| BFS respects solid walls | Returned path never traverses `#` |
| BFS unreachable target | Safely reports no path |
| BFS start equals destination | Correct zero-step path |
| Sonar energy spend | Successful pulse subtracts 30 once |
| Sonar energy below 30 | Pulse rejected; no energy spent |
| Energy regeneration/clamping | Stays within `[0, 100]` |
| Enemy hearing distance boundary | At or inside 6 reacts, outside 6 does not |

Refactor pure logic out of MonoBehaviours as needed so these tests don't depend on a running graphical scene.

## 13.2 Manual / PlayMode checklist

- [ ] Project opens without compiler errors.
- [ ] Main menu appears with working Play and How To Play buttons.
- [ ] WASD and arrows move player correctly; diagonal not faster.
- [ ] Player cannot walk through walls or slip through corners.
- [ ] Fog hides walls/enemies except local region and pulse region.
- [ ] Successful Space press makes a visible expanding ring.
- [ ] Pulse reveals nearby map for ~1.8 seconds, then fog returns.
- [ ] Repeated pulse obeys 1.0-second cooldown and energy rules.
- [ ] No negative energy; when energy regenerates, pulse becomes available.
- [ ] Enemy patrol moves around corners rather than through walls.
- [ ] Enemy hears a pulse within 6 units and investigates its **original** position.
- [ ] Enemy does not investigate a too-distant pulse.
- [ ] Enemy vision is blocked by wall colliders.
- [ ] Enemy catches nearby player and shows correct retry UI exactly once.
- [ ] Retry restores initial player/enemy/fog/energy state for same level.
- [ ] Reaching exit changes level precisely once.
- [ ] Level 1 has zero enemies, Level 2 one, Level 3 two.
- [ ] Finish Level 3 -> Victory -> Main Menu -> Play again works.
- [ ] Pausing freezes movement, timers, fog expiry and enemy behavior.
- [ ] All menus are clickable at 1280x720 and 1920x1080.
- [ ] Muting audio actually mutes sound and does not break effects.
- [ ] No critical errors in Unity Console during complete run.
- [ ] A **standalone exported build** passes the same game-flow test.

## 13.3 Playtesting protocol

Invite participants unfamiliar with the game. Do not explain anything beyond the in-game controls unless they get stuck. Record:

- Participant ID (not necessarily real name), test date, game build/commit.
- Whether controls and sonar were understood.
- Time to finish each level, number of captures/retries.
- Whether they recognized **sonar attracts enemies**.
- Where they got lost or frustrated.
- One positive and one negative qualitative comment.
- Change proposed, owner, priority, and whether later retested.

**Suggested usability targets (not measured claims):** most testers understand Space in Level 1 without outside explanation; no softlocks; nobody encounters a broken exit; typical playthrough fits a short class demonstration. Change values only using actual observations.

## 13.4 Bug triage

Use simple severity labels:

- **P0:** Crashes, cannot start game, invalid scene, trapped player, impossible exit.
- **P1:** Broken sonar/enemy/retry mechanics, severe controls failure.
- **P2:** Confusing tutorial, bad balancing, audio/UI issue.
- **P3:** Cosmetic flaw, minor alignment or animation.

Fix all P0 and P1 before adding optional features or preparing final submission.

---

# 14. BUILD AND RELEASE PROCEDURE

## 14.1 Build configuration

- Desktop platform selected by the group; document OS and architecture.
- Scene order: `MainMenu` -> `Game` -> `Victory` (if implemented as separate scene).
- Default resolution approximately 1280 x 720, scalable UI, windowed mode acceptable.
- Use Unity's current **Build Profiles** (or Build Settings on editor versions using that UI) and include all three scenes.
- Ensure the `Resources/Levels/*.json` files are included in the player build.
- Use no files stored only in `Assets/Editor` for runtime gameplay.
- Save final game to `Builds/EchoMaze_<OS>/` as appropriate.

## 14.2 Repeatable Editor build command

Implement `EchoMaze.Editor.EchoMazeBuildAutomation.BuildRelease` (or comparable fully qualified static method) so it can validate levels and call `BuildPipeline.BuildPlayer`, fail clearly on error, and log output path. Where the installed Unity command-line executable is accessible, support a documented command of the form:

```bash
"<PATH_TO_UNITY_EDITOR>" -batchmode -nographics -quit \
  -projectPath "<ABSOLUTE_PATH_TO_ECHOMAZE_PROJECT>" \
  -executeMethod EchoMaze.Editor.EchoMazeBuildAutomation.BuildRelease \
  -logFile "<ABSOLUTE_PATH_TO_BUILD_LOG>"
```

The placeholders must be replaced with real paths and OS-specific shell syntax. A successful process exit is **not** enough; inspect compile/build logs and launch the final build.

## 14.3 Deliverables to keep ready

- A working standalone desktop game build.
- Unity project source with scene files, prefabs, assets, scripts and project settings.
- Shareable weekly design-document slide deck with change history.
- `README.md`: gameplay summary, install/open/build/run instructions, known limitations.
- `readme.txt` and `ASSET_CREDITS.md`: asset sources, creators and license terms when external resources are used.
- `TESTING.md`: honest test results, setup, issues and playtest improvements.
- `CHANGELOG.md` / `DESIGN_LOG.md`: what changed and why.
- A short demo video or presentation materials as required by the course.

**Do not assume** the final Moodle upload file type/name/ZIP layout. Check current course instructions near final submission.

---

# 15. QUALITY TARGETS AND COURSE ALIGNMENT

The kickoff lecture describes grading as: **game 70%, design document 20%, final presentation 10%**; the game rubric allocates **50 points game design, 25 technology, 15 playtesting, 10 creativity**. All three major components must be passed. This project should make those visible:

| Rubric area | How Echo Maze addresses it |
|---|---|
| **Game Design** | Clear goal, learned controls, sonar information-risk choice, readable feedback, increasing challenge and compact levels |
| **Technology** | Proper Unity scene flow, 2D physics, reusable enemies, BFS navigation, robust fog and event-driven sonar, maintainable C# |
| **Playtesting** | Recorded external sessions, findings, one-change-at-a-time iteration, regression testing |
| **Creativity** | Sonar is both the player's vision and the signal that attracts enemies; strong simple visual/audio identity |
| **Design Document** | Weekly living slide deck capturing actual progress, decisions, tasks and timestamps |
| **Presentation** | Clear pitch, fast mechanic demonstration, playable build, visuals that show the unique tension |

**Important:** These are design intentions, not a promise of a grade or evidence of accomplished work. Prioritize quality and finishing a reliable game over more features.

---

# 16. RISK REGISTER AND FALLBACKS

| Risk | Impact | Mitigation |
|---|---|---|
| Team is new to Unity | High | Simplest 2D Core setup; weekly runnable milestones; documented bootstrap |
| Too much scope | High | Hard cap: 3 levels, 1 enemy type, 1 sonar ability, no multiplayer |
| Unity not installed on Claude machine | High | Generate editor bootstrap and code; give manual compile/test steps; label unverified |
| Fog looks wrong / hides player | Medium | Dedicated sorting order; simple grid fog; always-visible player |
| Enemy stuck behind walls | High | Validated BFS routes and waypoints; fallback to patrol on no path |
| Enemy too difficult | Medium | Player faster than enemies; shorter sight radius; test and tune |
| Complicated art production | Medium | Original simple geometric shapes first; optional licensed assets later |
| Unity merge conflicts | High | Coordinate scene editing, commit `.meta`, Git ownership for prefabs |
| Presentation forgotten | High | Weekly slide updates as tracked tasks and screenshots |
| No time for 3 polished levels | Medium | Shared systems across levels, no new mechanics in later levels |
| Build works in Editor but not standalone | High | Make/test exported build before final week |
| Group size differs from three | High | Confirm with lecturer; don't assume 4 allowed |
| Tool/AI use policy unclear | Medium | Ask teaching team if code-assistance policy is unclear; team must understand and own final implementation |

**Fallback order if time runs out:** Cut ambient audio, decorative effects, extended menus, optional key shortcuts, and extra visual polish **before** cutting sonar, enemy hearing, level completion, retry, basic HUD, or playtesting. Never cut the game's defining tension.

---

# 17. HOW CLAUDE CODE SHOULD REPORT PROGRESS

At the end of **every phase**, give a compact engineering report:

```text
PHASE: <number and name>
STATUS: complete / partially complete / blocked
FILES CREATED OR MODIFIED: <paths>
FUNCTIONALITY IMPLEMENTED: <concrete tested behavior>
TESTS ACTUALLY EXECUTED: <command and results, or "not run">
UNITY EDITOR VERIFIED: yes / no
STANDALONE BUILD VERIFIED: yes / no
BLOCKERS: <specific issue>
MANUAL STEPS FOR THE TEAM: <exact editor/menu actions>
NEXT PHASE: <one sentence>
```

**No fabricated verification.** If code is written but Unity is missing, say `implementation prepared; compilation and Play Mode not verified`. If editor bootstrap still needs execution, explain the exact action instead of saying scenes are already usable.

### First concrete Claude Code task (begin here)

1. Read the specification and inspect tool availability.
2. Create/initialize the Unity repository and minimal documentation.
3. Implement the level parser, validator and all three complete level JSON files.
4. Implement the grid level renderer with simple procedural sprites and static wall colliders.
5. Implement player controller and exit interaction.
6. Create or generate MainMenu and Game scenes; set up a clearly documented scene bootstrap.
7. Run available validation and, if Unity is available, start Level 1 and confirm it can be finished.
8. Report verified outcomes and ask for input only if an essential environmental decision is blocked (such as target OS or an existing incompatible Unity project).

Do not jump straight to sophisticated enemy AI or graphics before completing these steps.

---

# 18. SOURCES, ASSUMPTIONS AND DECISIONS

## 18.1 Course sources supplied with the request

1. **`GameDev 2026 - Kickoff.pdf`**: schedule (p. 6), attendance and submission rules (p. 7), 70/20/10 grading and living slide deck (p. 8), game assessment rubric (pp. 9–10), group size (p. 13), team responsibility tracking (p. 14), workload (p. 15), Unity introduction and installation (later pages).
2. **`gamedev2026-designwkp2.pdf`**: MDA mechanics/dynamics/aesthetics (p. 4), formal/dramatic/dynamic game elements (pp. 6–9), challenge and meaningful player decisions (pp. 13, 16–17), iterative design and prototype-first process (pp. 18–22), scope warning (p. 20), core mechanics and gameplay visualization (pp. 29–30), and prototyping principles (later pages).
3. **Screenshot of Moodle assignment**, `Initial Design Document [due Oct. 19 23:59]`: requires a slide deck link and weekly updates, with initial submission by **19 Oct 2026**.

The uploaded lecture materials **do not** mandate Echo Maze, three levels, the exact numbers, any script architecture, or the precise slide sequence in this document. Those are our own proposed design and engineering choices.

## 18.2 Explicit decisions and things requiring team confirmation

- **Game idea:** Echo Maze, selected in group-message draft in this conversation.
- **Game engine:** Unity and C# (course context).
- **Scope:** One short 2D game with three levels, one enemy type, sonar/darkness risk mechanic.
- **Team members:** Placeholder Persons A/B/C; replace with actual names after confirmation.
- **Team size:** Planned for three total students, not four. Verify actual roster; course kickoff normally specifies three.
- **Target desktop OS:** Not yet chosen. Select the OS all three members can build/test.
- **Unity patch version:** Not yet chosen. All members should match a supported LTS release.
- **Playtested balance:** None yet; numerical values must be tuned after building.
- **Asset sourcing:** Start with original geometry; document later external sources.
- **Final Moodle hand-in format:** Verify near the deadline; not fully specified in the referenced kickoff materials.

---

# FINAL REMINDER FOR THE IMPLEMENTER

**Make a modest game that actually works.** It is better to deliver three small polished levels and a reliable, meaningful sonar decision than an ambitious half-finished project. The sonar should feel useful, and the sound risk should be noticeable and fair. The team should be able to explain the game, navigate its Unity project, demo it, test it, and submit it without depending on Claude Code being present during the final presentation.
