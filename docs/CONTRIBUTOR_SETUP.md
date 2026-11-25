# Contributor setup and bundled skills

Clone or download the entire source repository, including hidden `.agents` and `.github` directories. Open the repository root in your coding assistant. Five original project skills ship in `.agents/skills/`; no network download, account connection or global skill copy is required for those files. Codex supports repository-local skills; they should be available in the next turn when the repository is the active workspace. If missing, reopen the project/session and ask the assistant to read AGENTS.md and list the bundled skills. Discovery has not been verified in another contributor session.

Other assistants can read AGENTS.md and the relevant SKILL.md directly; automatic discovery varies. GEMINI.md and .github/copilot-instructions.md point Gemini CLI and GitHub Copilot to the same rules. Players running the game do not need AI skills.

Install the Unity Editor version recorded in ProjectSettings/ProjectVersion.txt through Unity Hub, with your desktop build support, Git, and a C# editor. Python 3 is needed only for optional repository helper scripts. Skills provide workflows; they do not install Unity, license it, compile C# or add credentials.

Current game status: an implemented source prototype includes building definitions, placement, visitors, service income, upgrades, attraction, queues, rides, menu/HUD and a safe market-scene bootstrap. Unity API compilation and eight independent core tests pass. Unity Editor import, generated scene, Play Mode flow and exported build have not been verified in this environment. Run `bash tools/unity.sh setup`, then test and build from a normal Unity session.

Skill sources: all five project skills are original repository instructions derived from the supplied handoff. No third-party skills, paid plugins, MCP servers or external services are required for the MVP. Optional external skills can be evaluated later for a concrete task rather than installed on every contributor machine.

Repository-local skill approach: https://developers.openai.com/blog/skills-agents-sdk

## Moving to a new computer (written 18 Nov 2026)

Everything needed is in the repository; nothing has to be copied from the old machine. Library/, Logs/, Builds/ and the generated scene are not in git and are rebuilt.

1. **Clone.** The repo is private under the GitHub account `joash2003`: `gh auth login` with that account, then `gh repo clone joash2003/wintermarkt`.
2. **Unity 6000.3.25f1 (6.3 LTS).** Install Unity Hub and that editor version, sign in and activate a licence. On macOS from a terminal: `brew install --cask unity-hub unity-cli`, `unity auth login`, `unity license activate --personal --accept-eula`, `unity install 6000.3.25f1 -c e1dba0a9aba4 -a arm64`. Unity Hub 4 no longer installs editors from its own command line; the separate `unity` CLI does.
3. **Create the scene.** `bash tools/unity.sh setup`. It imports the packages and models (several minutes the first time) and creates `Assets/_Wintermarkt/Scenes/Market.unity`, which is deliberately not committed.
4. **Play.** Open the folder in Unity Hub, open the Market scene if it is not open, press Play.
5. **Before committing.** `brew install gitleaks` (Windows: `winget install gitleaks`) and `git config core.hooksPath .githooks`. Without the hook a commit goes through even when `bash tools/quality-check.sh` fails.
6. **Checks.** `bash tools/test-market-core.sh` (rules), `bash tools/balance-bots.sh` (scripted players), `bash tools/unity.sh shots` (plays the evening at 8x with the editor closed and writes pictures to Logs/shots).
7. **Where things stand.** `python3 tools/issue.py list` shows every open and planned piece of work; `docs/LINZ_WINTERMARKT_DESIGN.md`, section "How the numbers work", describes the rules as built; ASSET_CREDITS.md lists the third-party assets.
8. **AI assistants, optional.** An assistant's chat history does not move with the repo: the tickets and the design spec are the record.
