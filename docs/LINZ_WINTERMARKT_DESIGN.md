# Linz Wintermarkt — working design

Status: initial design deck v0.1 prepared on 17 Oct 2026. A source prototype now implements the market simulation and original blockout visuals; Unity scene import, Play Mode acceptance, team playtesting and balance are unverified. The MVP and its tuning are proposed baselines, not tested conclusions. Working title only. This document supersedes the Echo Maze gameplay handoff. The existing maze code is retained as legacy work.

## User's chosen direction

A Christmas-market building and management game centered on Volksgarten at the Goethekreuzung entrance in Linz, with Mariendom and Hauptplatz establishing the wider city setting. The objective is to attract as many people as possible. Shops can level up and upgrade; rides are part of the market. Clash of Clans is a reference for readable building placement, visible upgrades and a growing settlement. Use original buildings, UI, economy and art.

## Pitch and core loop

Build Linz's most inviting Christmas market: place stalls and rides, welcome visitors, earn money, and reinvest in attractions and service capacity before the festival ends.

Place a stall → visitors arrive and walk to it → service earns coins → upgrade or build → more visitors arrive → queues grow → improve capacity and layout → repeat.

The main decision is whether to spend on attracting more people or serving the crowd already present. A spectacular ride can draw a crowd, but a market with long queues loses its appeal.

## Proposed first playable version

- Unity desktop game, a geographically faithful Volksgarten market area entered from Goethekreuzung, fixed angled overhead camera, simple original low-poly buildings. Orthographic 3D presentation with grid-based simulation; retain the recorded editor version. No new asset/rendering packages needed merely for the camera style.
- A ten-minute festival in three waves with a target each (issue 20), adjustable in settings. The time limit gives the attraction objective a measurable end and makes spending decisions matter. This is a playtest target, not a course requirement.
- Start with coins, one entrance and a clear walking area. Build, inspect and upgrade with mouse clicks. Pause and restart are available.
- Three stall categories: hot drinks, pastries and crafts. Two rides: carousel and Ferris wheel. Unlock the larger ride after the market has welcomed enough visitors.
- Each building has three visible levels in the first version. Upgrades change appearance, attraction, capacity or service rate. Data supports additional levels later; no unbounded upgrade system in the first prototype.
- One visitor behavior system. Visitors arrive with two or three wishes, visit a building for each, queue, receive service, and leave. No complex per-visitor economy or daily routines.
- End screen reports a win or a loss with the wave reached (issue 20), happy visitors (the score), visitors who arrived, who wanted more, who gave up, coins earned and final reputation.

All numbers, building counts and level limits above are provisional scope choices.

## Economy and visitor rules

Coins buy buildings and upgrades. Coins are earned by actual completed service, not by clicking a collection icon on a timer. No premium currency or real-money timers.

Arrival rate grows with the market's attraction rating. Reputation from visitor experience modifies that rate. Attraction should reward category variety so spamming the cheapest stall is not automatically optimal. Start with a transparent, tunable formula and show its contributors in the UI.

Each building has an entry/queue cell, a maximum queue length, service duration and service capacity. Visitors cannot queue indefinitely: show impatience, then let them leave, affecting reputation. A completed visit improves reputation. Clamp reputation and arrivals so the simulation stays bounded and a poor start can recover. Keep ordinary visitors arriving at a small minimum rate.

A new ride increases attraction but consumes space and money. An upgraded pastry stall serves faster and relieves queues but may bring fewer new visitors. Building two copies increases capacity; introducing another category offers a variety benefit. These trade-offs need actual playtesting.

## How the numbers work (17 Nov 2026)

The game is designed to be played once, for about ten minutes (decided 17 Nov 2026): one evening in three acts. This is what the simulation calculates. The values are in `Resources/Wintermarkt/market.json`.

- **Three acts, one star each.** Afternoon (150 s, 6 happy visitors for the star), Evening rush (240 s, 35, arrivals x1.6), Closing time (210 s, 60, arrivals x1.2). A missed target costs the star but the evening goes on. It ends early only if reputation falls to zero. The ending card gives a verdict by stars.
- **Happy visitor.** Every visitor arrives wanting two or three different categories (drinks, food, crafts, rides, landmark), in a fixed repeating pattern from the categories that are unlocked. They visit one building per wish, each time the one where queue plus walk is shortest. Only a visitor who got everything is happy.
- **How a visit ends.** Got everything: happy. Something they wanted is not built: "wanted more", reputation -0.5, and the HUD names the category visitors miss most. Waited over 35 s, or every queue for what they want is full: "gave up", reputation -2.
- **Stalls call for help.** A stall with a queue calls about every 25 s (more often in acts 2 and 3). Click it within 8 s: it serves twice as fast for 15 s and pays 5 coins. Miss it: it closes for 10 s. Every stall has its own timer, so a bigger market needs more of the player.
- **Events.** Act 1: a full tram (6 visitors at once). Act 2: a school class (12 visitors who want only food, announced 20 s ahead), a snowstorm (45 s: patience x0.6 and everyone also wants a hot drink, announced 15 s ahead), a power cut (every stall calls at once, 12 s to answer, no warning). Act 3: a packed last tram (10 visitors).
- **On the map.** Each category has one colour (sign and flag on the building, swatch in the build menu). A visitor carries one lit cube per remaining wish, a gold ball when leaving happy and a red bar when they gave up. A calling stall shows a yellow ball with a shrinking red ring.
- **Space.** Only the corner within 70 m of the Goethekreuzung entrance is the market: 224 buildable cells of 4 m, minus trees and paths. A stall needs 4 cells plus 4 for its queue, the carousel 9 + 6, the wheel 12 + 8. A building can be removed for half of what was spent on it.
- **Coins.** You start with 400, enough for the three stalls. A visitor pays the building's `income` each time a service finishes (4 to 15 coins).
- **Arrivals.** Nobody arrives until the first building stands. After that a new visitor arrives every `10 / (1 + attraction x 0.035) / (0.5 + reputation / 100)` seconds, never faster than one per second, divided by the act's arrival factor.
- **Attraction.** The sum of every building's attraction value, plus 15 for each different category beyond the first.
- **Reputation.** Starts at 60, stays between 0 and 100. Each service adds 0.5; see above for the losses.
- **Capacity.** A building serves `capacity` visitors at once, each taking `serviceSeconds`. An upgrade raises capacity (1, 2, 3), shortens service and raises income.
- **Unlocks.** The carousel unlocks after 25 arrivals and the wheel after 80. From then on some visitors want a ride or the landmark too, so room has to be kept for them.

Scripted players (`bash tools/balance-bots.sh`, 17 Nov 2026, these values, real site). They answer a help call after about 3 seconds and miss one in four, except the one marked "never helps". "Happy per act" is happy visitors against the star target:

| Strategy | Result | Happy per act | Arrived | Happy | Wanted more | Gave up | Calls helped |
|---|---|---|---|---|---|---|---|
| One stall, then nothing | 0 stars | 0/6, 0/35, 0/60 | 112 | 0 | 105 | 6 | 16/21 |
| Cheapest stall, repeatedly | 0 stars | 0/6, 0/35, 0/60 | 191 | 0 | 190 | 0 | 65/81 |
| Attractions only, no upgrades | 0 stars | 3/6, 18/35, 7/60 | 249 | 28 | 152 | 60 | 90/102 |
| Upgrade the busiest first | 0 stars | 0/6, 0/35, 0/60 | 166 | 0 | 165 | 0 | 40/45 |
| One of each, then upgrades | 3 stars | 9/6, 76/35, 103/60 | 396 | 188 | 6 | 179 | 98/104 |
| One of each, then upgrades, never helps | Closed in act 2 | 8/6 | 76 | 10 | 4 | 46 | 0/14 |
| One of each, then what visitors miss, duplicates when busy | 3 stars | 13/6, 61/35, 130/60 | 451 | 204 | 37 | 182 | 121/130 |

A one-sided market plays the whole evening without a star; the two mixed strategies get all three; the same mixed strategy without helping the stalls loses its reputation and closes in act 2. These are bots, not people, and the targets were set from their results: no human playtest supports the numbers yet. Known weak points: well over a hundred visitors still give up in a three-star run, so the evening rush overloads even a full plot; coins are left over once the plot is full; queue positions still do not move up (issue 14); the finale (issue 26) is not built.

## Placement and navigation

Buildings occupy grid footprints. Green/red previews show valid/invalid placement. Require clear cells and a route from the entrance to the new building's entry. Reject placements that block entry routes to existing buildings or trap visitors. Recheck placement when confirming so money is spent only once on a valid placement.

Use a small four-neighbor grid and BFS for visitor routes. Animate movement smoothly between tile centers. Use reserved queue slots to prevent every visitor standing in one location. No physics-based crowds.

First prototype uses a fixed walkable square with buildings as obstacles. Player-built roads and decorative bonuses are later possibilities, not MVP dependencies.

## Visual and local identity

Warm wooden stalls, snowy roofs, string lights, a central Christmas tree, colorful ride motion and small crowds. Levels visibly add details: awnings, lights, ornaments and larger displays. Carousel rotation and Ferris wheel rotation can be simple animations rather than ride physics.

The primary map follows the real Volksgarten and Goethekreuzung layout as closely as the reference evidence allows, preserving proportions and using camera pan/zoom. Keep the park, paths, trees, entrance and surrounding urban character legible; building placement inside it is player-controlled. This is not an exact reproduction of a real market plan or real vendors. Research local architecture and permitted reference images before producing detailed environment art. Do not use Clash of Clans assets, branding or copied interface layouts.

## Course alignment

The workshop's meaningful-decision criteria (pp. 16–17) apply to attraction versus capacity: both options offer benefits and costs, with visible effects. MDA (p. 4): building/upgrade/queue mechanics create crowd-flow and investment decisions, intended to evoke a lively, cozy market and manageable pressure. These are design hypotheses.

The kickoff requires a living weekly slide deck (p. 8), documented individual responsibilities and planned/actual completion dates (p. 14), and documented playtesting (pp. 9–10). Show real evidence of confusing queues, upgrade usefulness and layout decisions, then record changes and retests.

## Milestones

1. Vertical slice: square, one stall, placement, visitors, queue/service, coins and one upgrade. Demonstrate that visitors reach the stall and that an upgrade improves throughput.
2. Management loop: attraction/reputation, building variety, placement safeguards, feedback and a complete timed session.
3. Market identity: remaining stalls, carousel, Ferris wheel and visible level changes. Add polish only once the core loop works.
4. Test and deliver: real playtesting, measured balance changes, standalone build, weekly deck and final demonstration.

Out of scope for the first version: combat, raids, multiplayer, clans, mobile port, offline progression, staff scheduling, supply chains, seasons/weather simulation and realistic crowds. The intended shared reference is the satisfying growth of a town, while the game itself is about managing a Christmas market.

## Current implementation status

Wintermarkt source now includes a deterministic market simulation, runtime presentation, Unity bootstrap, data files and focused tests. Direct Unity API compilation and eight pure-core tests pass. Unity Editor import, generated scene, Play Mode and an exported build remain unverified because this environment cannot create the local Package Manager socket. Assets/_EchoMaze remains legacy source; its setup is separate. The current market geometry and ambient routes are provisional, not surveyed. The first editable slide deck is [Linz_Wintermarkt_Initial_Design_Document_v0.1.pptx](submission/Linz_Wintermarkt_Initial_Design_Document_v0.1.pptx); update it weekly with real team owners, dates and playtest evidence.

## Location decision: Volksgarten / Goethekreuzung

The user specifies Goethekreuzung as the ride-market location and also wants the identity of Mariendom and the Hauptplatz Christmas market recognized. Official market arrival information places the Volksgarten entrance at the Goethekreuzung stop. Use the player-facing label **Volksgarten · Goethekreuzung**.

- **Volksgarten / Goethekreuzung:** the first and required playable market, focused on stalls and rides. Arrivals can enter from the Goethekreuzung side; moving Linz trams and buses are required background features; implement them on verified fixed routes after the basic location blockout.
- **Mariendom / Domplatz:** recognizable separate city landmark on the overview screen. Its real Advent am Dom market provides context. A quieter crafts-focused playable market is a possible later expansion, not yet part of the first build.
- **Hauptplatz:** label the separate central Christmas market on the overview. A later market scenario could emphasize stall variety and the central tree; do not promise it before the first market is playtested.

Do not place Mariendom immediately beside Volksgarten or imply all three are one real square. A stylized city overview can show their relationship while the playable view focuses on one site. The attraction types in our game are design choices; the proposed Ferris wheel is not a claim about a particular year's real ride lineup.

Sources checked for location context:
- Official market arrival information: https://christkindlmarkt-linz.at/allgemeines/anreise
- Linz Tourism Christmas-market guide (2022, historical context for rides and market character): https://www.linztourismus.at/assets/Uploads/Linz-Magazin-2-2022.pdf
- City of Linz account identifying the separate markets: https://www.linz.at/medienservice/2025/202512_133764.php

## Concrete build sequence

1. **Block out Volksgarten:** create a small grid, paths, fixed trees, Goethekreuzung-side entrance and an angled camera. Test zoom, selection and readable placement with simple shapes.
2. **Prove the loop:** place one food stall, spawn visitors, route them to reserved queue positions, complete service, earn coins and buy one upgrade. This is issue 8's first playable checkpoint.
3. **Make management matter:** implement attraction, limited service capacity, patience and clear feedback. Test whether buying another stall and upgrading an existing one are both useful.
4. **Add market content:** introduce the three stall categories, carousel and Ferris wheel, visible upgrades and a timed festival result. Tune counts/costs only after observing playtests.
5. **Add Linz character:** original stylized local environment, Christmas lighting and sound, plus the overview naming Volksgarten, Mariendom and Hauptplatz. Verify local references before detailed modeling.
6. **Verify delivery:** beginner playtests, regression checks, exported desktop run and weekly design-deck updates. Expand to another playable location only if the core market is complete and the course workload permits it.

## Updated requirement: geographic fidelity and background transit

The user requires the location to be as similar as possible to the real site, with Linz trams and buses moving in the background. This replaces free compression of the park and upgrades ambient transit from optional to required. See [location and transit brief](LOCATION_AND_TRANSIT.md) for sources, geometry rules, implementation steps and evidence gaps. Keep the site fixed while allowing players to build and upgrade within viable market plots. issue 11 tracks this work.
