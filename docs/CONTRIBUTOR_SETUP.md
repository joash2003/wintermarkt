# Contributor setup

Install the Unity Editor version recorded in ProjectSettings/ProjectVersion.txt through Unity Hub, with your desktop build support, Git, and a C# editor. Python 3 is needed for the repository helper scripts.

Current game status: an implemented source prototype includes building definitions, placement, visitors, service income, upgrades, attraction, queues, rides, menu/HUD and a safe market-scene bootstrap. Unity API compilation and eight independent core tests pass. Unity Editor import, generated scene, Play Mode flow and exported build have not been verified in this environment. Run `bash tools/unity.sh setup`, then test and build from a normal Unity session.

## Moving to a new computer (written 18 Nov 2026)

Everything needed is in the repository; nothing has to be copied from the old machine. Library/, Logs/, Builds/ and the generated scene are not in git and are rebuilt.

1. **Clone.** The repo is private under the GitHub account `joash2003`: `gh auth login` with that account, then `gh repo clone joash2003/wintermarkt`.
2. **Unity 6000.3.25f1 (6.3 LTS).** Install Unity Hub and that editor version, sign in and activate a licence. On macOS from a terminal: `brew install --cask unity-hub unity-cli`, `unity auth login`, `unity license activate --personal --accept-eula`, `unity install 6000.3.25f1 -c e1dba0a9aba4 -a arm64`. Unity Hub 4 no longer installs editors from its own command line; the separate `unity` CLI does.
3. **Create the scene.** `bash tools/unity.sh setup`. It imports the packages and models (several minutes the first time) and creates `Assets/_Wintermarkt/Scenes/Market.unity`, which is deliberately not committed.
4. **Play.** Open the folder in Unity Hub, open the Market scene if it is not open, press Play.
5. **Before committing.** `brew install gitleaks` (Windows: `winget install gitleaks`) and `git config core.hooksPath .githooks`. Without the hook a commit goes through even when `bash tools/quality-check.sh` fails.
6. **Checks.** `bash tools/test-market-core.sh` (rules), `bash tools/balance-bots.sh` (scripted players), `bash tools/unity.sh shots` (plays the evening at 8x with the editor closed and writes pictures to Logs/shots).
7. **Where things stand.** `python3 tools/issue.py list` shows every open and planned piece of work; `docs/LINZ_WINTERMARKT_DESIGN.md`, section "How the numbers work", describes the rules as built; ASSET_CREDITS.md lists the third-party assets.
