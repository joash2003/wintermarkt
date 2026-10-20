# Verification record

Date: 2026-10-17. Project: initial EchoMaze source. Environment: macOS; Unity executable reports 6000.6.4f1. No Unity runtime success is claimed.

| Check actually executed | Result |
|---|---|
| Unity `-version` | Reports `6000.6.4f1` |
| `python3 tools/validate_levels.py` | PASS: all three JSON levels have expected dimensions, closed borders, allowed symbols, one player/exit, reachable exits and correct reachable enemy patrol metadata |
| `bash -n tools/unity.sh` | PASS: shell syntax |
| Unity batch startup with bootstrap method | BLOCKED before compilation: Package Manager socket creation failed with `listen EPERM: operation not permitted /tmp/Unity-Upm-10033.sock`; local player connection sockets were also denied |
| Ruby YAML parse of five skill frontmatters and GitHub issue form | PASS structural check |
| Skill creator's Python validator | Not run successfully: PyYAML is missing |

Unity command actually attempted:

```bash
/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Users/joashjohnson/EchoMaze -executeMethod EchoMaze.Editor.EchoMazeProjectBootstrap.Create -logFile /Users/joashjohnson/EchoMaze/Logs/bootstrap.log
```

The process stalled after Package Manager failure and was stopped. Local evidence is in `Logs/bootstrap.log` (ignored by Git). `python3` validates data independently; it does not execute the C# parser or prove runtime correctness.

## Continue outside the assistant sandbox

Close any Unity instance for this project. From your normal Terminal run `bash tools/unity.sh setup`, then `bash tools/unity.sh test`. Check `Logs/bootstrap.log`, `Logs/editmode.log` and `Logs/editmode.xml`. Resolve any compile or package errors before proceeding. Run `bash tools/unity.sh build`, inspect `Logs/build.log`, then launch the exported app. Commands are prepared; tests and build have not run here.

The EditMode suite covers all map sizes/counts, BFS wall/unreachable/zero-step cases, invalid symbols/spawns/rows/exits/patrol count, energy/cooldown/recharge/clamping and hearing boundaries. Coverage for invalid individual patrol waypoints and runtime scene flow remains to be added.

## Manual acceptance (all pending)

- [ ] Project compiles and bootstrap runs; repeat generation preserves edited assets.
- [ ] Menu and instructions work at 1280×720 and 1920×1080.
- [ ] WASD/arrows, normalized diagonals and wall/corner collision work.
- [ ] Pulse ring appears, fog reveals/expires, overlap timers work, player stays visible.
- [ ] Sonar costs 30 once, respects cooldown and regenerates energy without negative values.
- [ ] Enemies patrol around corners, hear original pulse positions only within range, and investigate/wait/resume.
- [ ] Chase takes priority; walls block sight; enemies never cut corners or get stuck.
- [ ] Capture/exit resolves once; retry resets all local state.
- [ ] Pause freezes movement, energy, fog and AI; menus resume/retry correctly.
- [ ] Mute works; sound effects are audible when enabled.
- [ ] All three levels complete through victory; replay/menu paths work.
- [ ] Exported app launches and passes the same complete flow without critical errors.
- [ ] Real external playtests recorded with dates, build ID, completion times, captures and feedback.

Legacy maze checks above remain historical. Market JSON and Unity bootstrap source now exist, while the bootstrap-generated scene is not committed. Direct API compilation and pure simulation tests are verified; Unity scene import and runtime behavior remain unverified. GitHub publication, human playtesting, desktop build verification and course deck publication remain required.


## Linz Wintermarkt prototype checks (2026-10-17)

| Check | Result |
|---|---|
| `python3 tools/compile-market.py` | PASS: Wintermarkt.Core, Wintermarkt.Runtime and Wintermarkt.Editor compile against the installed Unity 6000.6.4f1 API assemblies. This direct compiler check does not import the project in Unity. |
| `bash tools/test-market-core.sh` | PASS: 8 simulation tests covering placement atomicity, navigation, service income, queue slots, pause/upgrade, throughput, bounded full run and invalid configuration. |
| Unity Editor bootstrap/import, EditMode run and player build | Not run successfully here. The earlier Unity batch attempt was blocked when Package Manager could not create `/tmp/Unity-Upm-10033.sock` (`listen EPERM`). |

For a local editor acceptance run, open this project in Unity Hub 6000.6.4f1, allow package import, select **Tools → Wintermarkt → Create or Repair Project**, press Play, place a stall, observe a visitor complete service, collect income and buy an upgrade. Then run `bash tools/unity.sh test` and `bash tools/unity.sh build`. Record results before marking EM-008 fully accepted. Geography and transit remain provisional; see `docs/LOCATION_AND_TRANSIT.md`.
