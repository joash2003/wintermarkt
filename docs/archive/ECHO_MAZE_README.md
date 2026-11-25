# Echo Maze

A Unity 2D stealth maze game. Sonar reveals dark corridors but attracts enemies to the scan position. Escape three handcrafted levels.

**Status:** implementation prepared; Unity compilation, Play Mode and standalone build are not verified. Editor startup inside the original sandbox failed because Unity Package Manager could not create its local socket (`EPERM`). See [test results](TESTING.md) and the [issue board](docs/issues/README.md).

## Open and play

1. Use Unity Hub to install **6000.6.4f1**, the version already installed on the development Mac. Its LTS status has not been verified. Keep all team members on the same recorded version.
2. In Hub, choose Add project from disk and select this `EchoMaze` folder. Wait for package import and compilation. Check Console for errors.
3. Choose **Tools > Echo Maze > Create or Repair Project**. This generates the square sprite, GameSettings asset and MainMenu/Game/Victory scenes, registers scenes and configures legacy input and Wall layer 8. Existing scene and Resources files are preserved. Save any current scene when prompted.
4. Open `Assets/_EchoMaze/Scenes/MainMenu.unity` and press **Play**. Click Play in the menu.
5. WASD/arrows move; Space scans; Esc pauses; R retries; M toggles sound. Reach the green exit. Enemies appear starting in Level 2. If caught, click Retry.

The bootstrap source exists but has not executed successfully here; generated scene/assets are pending. Do not expect a runnable scene before step 3 completes without errors. If Unity asks to restart after input settings change, restart and run the menu again.

## Edit the game

- Levels: `Assets/_EchoMaze/Resources/Levels/level_01.json` through `level_03.json`. Rows are top-to-bottom; symbols are `#` wall, `.` floor, `P` player, `X` exit, `E` enemy. Patrol metadata must match enemy order. Use **Tools > Echo Maze > Validate Levels** after edits.
- Tuning: after generation, select `Assets/_EchoMaze/Resources/GameSettings.asset` in Unity and edit its Inspector fields. Shared gameplay values live here.
- Scripts: `Assets/_EchoMaze/Scripts/`; editor generation/build tools are in `Editor/`. UI and level objects are generated at runtime from readable code/data. Scenes are lightweight entry points. Prefab authoring is not yet implemented; this is a recorded simplification.

## Test and build on macOS

Close the Unity project before running batch commands. In Terminal, from this directory:

```bash
python3 tools/validate_levels.py
bash tools/unity.sh setup
bash tools/unity.sh test
bash tools/unity.sh build
open Builds/EchoMaze_macOS/EchoMaze.app
```

`tools/unity.sh` defaults to the installed Mac editor path; override `UNITY_EDITOR` if necessary. Setup calls `EchoMaze.Editor.EchoMazeProjectBootstrap.Create`; build calls `EchoMaze.Editor.EchoMazeBuildAutomation.BuildRelease`. Logs and test XML go to `Logs/`; inspect results rather than relying only on exit status. macOS is the initial target; verify the appropriate build support/license is installed. The resulting app must still pass the full gameplay checklist.

You can also run EditMode tests in Unity's **Window > General > Test Runner**. See [TESTING.md](TESTING.md) for unperformed checks and current evidence.

## Contribute

Read [setup](docs/CONTRIBUTOR_SETUP.md), [CONTRIBUTING.md](CONTRIBUTING.md), and the [tickets](docs/issues/README.md). No GitHub remote is configured yet. Assets are original geometry and generated sounds; see [credits](ASSET_CREDITS.md). Team roster, real human playtests, shareable weekly slide deck and final course submission remain pending.
