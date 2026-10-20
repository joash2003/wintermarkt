# Contributor setup and bundled skills

Clone or download the entire source repository, including hidden `.agents` and `.github` directories. Open the repository root in your coding assistant. Five original project skills ship in `.agents/skills/`; no network download, account connection or global skill copy is required for those files. Codex supports repository-local skills; they should be available in the next turn when the repository is the active workspace. If missing, reopen the project/session and ask the assistant to read AGENTS.md and list the bundled skills. Discovery has not been verified in another contributor session.

Other assistants can read AGENTS.md and the relevant SKILL.md directly; automatic discovery varies. CLAUDE.md points Claude Code to the same source files. Players running the game do not need AI skills.

Install the Unity Editor version recorded in ProjectSettings/ProjectVersion.txt through Unity Hub, with your desktop build support, Git, and a C# editor. Python 3 is needed only for optional repository helper scripts. Skills provide workflows; they do not install Unity, license it, compile C# or add credentials.

Current game status: an implemented source prototype includes building definitions, placement, visitors, service income, upgrades, attraction, queues, rides, menu/HUD and a safe market-scene bootstrap. Unity API compilation and eight independent core tests pass. Unity Editor import, generated scene, Play Mode flow and exported build have not been verified in this environment. Run `bash tools/unity.sh setup`, then test and build from a normal Unity session.

Skill sources: all five project skills are original repository instructions derived from the supplied handoff. No third-party skills, paid plugins, MCP servers or external services are required for the MVP. Optional external skills can be evaluated later for a concrete task rather than installed on every contributor machine.

Repository-local skill approach: https://developers.openai.com/blog/skills-agents-sdk
