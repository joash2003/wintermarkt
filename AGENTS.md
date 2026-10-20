# Linz Wintermarkt contributor guidance

The user pivoted to a Linz Christmas-market management game. Read docs/LINZ_WINTERMARKT_DESIGN.md and the current ticket before implementation. It supersedes the Echo Maze handoff. Existing maze gameplay is legacy, not the active product. Preserve the Unity project and agreed scope. Do not claim compilation, playtesting or standalone verification without actual execution.

Project skills are bundled in `.agents/skills/`:

- `echo-unity-development`: Unity gameplay, UI, scenes and bootstrap.
- `echo-levels-and-enemies`: maps, BFS, sonar, fog and enemy AI.
- `echo-testing-and-release`: checks, evidence and desktop builds.
- `echo-contributor-tickets`: issue and review workflow.
- `echo-project-docs`: beginner and course documentation.

Use the relevant skill when working in its scope. Paths in skills are relative to the repository root. Track contributions using CONTRIBUTING.md and docs/issues/README.md. Include Unity .meta files; exclude Library, Temp, logs and builds. Preserve existing work; coordinate shared scene edits.

Existing echo-* skills remain available for reusable Unity, testing, tickets and documentation practices. Their maze-specific mechanics, paths and three-level scope are historical and must not override the new design. Do not run the old bootstrap as market setup.
