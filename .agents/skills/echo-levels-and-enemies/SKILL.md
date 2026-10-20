---
name: echo-levels-and-enemies
description: Edit EchoMaze JSON maps, grid validation, BFS, fog, sonar or enemy state behavior. Use for maze and navigation mechanics.
---

Read handoff sections 2–5 and 8 and inspect Scripts/LevelData.cs and EnemyController.cs.

Level files are Assets/_EchoMaze/Resources/Levels/level_01.json through level_03.json. Rows are top to bottom; world coordinates are (column, -row). Validate rectangular closed maps, allowed #.PEX symbols, exactly one P and X, reachable exit, and enemy patrol count/order/spawns plus reachable floor waypoints. Preserve the three handcrafted maps unless a documented playtest finding justifies a change.

BFS uses four neighbors and must never cross walls. An empty path means already at destination; null means unreachable. Follow tile centers; a target change mid-edge must not cut a corner or snap the enemy through a wall.

Hearing uses a successful pulse's original position within the configured inclusive radius. Failed pulses emit no noise. Chase overrides investigation, sight requires wall-blocked line of sight, and lost sight uses memory before investigation/patrol. Fog changes human visibility only. Player and pulse draw above fog; enemies draw below it. Refresh overlapping reveal expiration with the maximum timestamp, using scaled time.

Keep tuning in GameSettings. Verify geometry and gameplay separately: BFS connectivity does not establish enemy fairness. Use actual Unity tests and playtests for runtime claims.
