# Linz Wintermarkt contributor guidance

The user pivoted to a Linz Christmas-market management game. Read docs/LINZ_WINTERMARKT_DESIGN.md and the current ticket before implementation. It supersedes the Echo Maze handoff. Existing maze gameplay is legacy, not the active product. Preserve the Unity project and agreed scope. Do not claim compilation, playtesting or standalone verification without actual execution.

Project skills are bundled in `.agents/skills/`:

- `wintermarkt-unity-development`: Unity gameplay, UI, scenes and bootstrap.
- `legacy-maze-levels`: legacy Echo Maze maps, BFS, sonar, fog and enemy AI.
- `wintermarkt-testing-and-release`: checks, evidence and desktop builds.
- `wintermarkt-contributor-tickets`: issue and review workflow.
- `wintermarkt-project-docs`: beginner and course documentation.

Use the relevant skill when working in its scope. Paths in skills are relative to the repository root. Track contributions using CONTRIBUTING.md and docs/issues/README.md. Include Unity .meta files; exclude Library, Temp, logs and builds. Preserve existing work; coordinate shared scene edits.

The wintermarkt-* skills started as Echo Maze skills and still carry reusable Unity, testing, ticket and documentation practices. Their maze-specific mechanics, paths and three-level scope are historical and must not override the new design. Do not run the old bootstrap as market setup.
