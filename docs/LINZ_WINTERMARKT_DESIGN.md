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
- A ten-minute festival session, adjustable in settings. The time limit gives the attraction objective a measurable end and makes spending decisions matter. This is a playtest target, not a course requirement.
- Start with coins, one entrance and a clear walking area. Build, inspect and upgrade with mouse clicks. Pause and restart are available.
- Three stall categories: hot drinks, pastries and crafts. Two rides: carousel and Ferris wheel. Unlock the larger ride after the market has welcomed enough visitors.
- Each building has three visible levels in the first version. Upgrades change appearance, attraction, capacity or service rate. Data supports additional levels later; no unbounded upgrade system in the first prototype.
- One visitor behavior system. Visitors choose reachable attractions, queue, receive service, optionally visit another category, and leave. No complex per-visitor economy or daily routines.
- End screen reports visitors served (primary score, changed from visitors welcomed on 17 Nov 2026), visitors who arrived, visitors who left unserved, coins earned and final reputation. A visitor who is turned away or gives up does not count towards the score.

All numbers, building counts and level limits above are provisional scope choices.

## Economy and visitor rules

Coins buy buildings and upgrades. Coins are earned by actual completed service, not by clicking a collection icon on a timer. No premium currency or real-money timers.

Arrival rate grows with the market's attraction rating. Reputation from visitor experience modifies that rate. Attraction should reward category variety so spamming the cheapest stall is not automatically optimal. Start with a transparent, tunable formula and show its contributors in the UI.

Each building has an entry/queue cell, a maximum queue length, service duration and service capacity. Visitors cannot queue indefinitely: show impatience, then let them leave, affecting reputation. A completed visit improves reputation. Clamp reputation and arrivals so the simulation stays bounded and a poor start can recover. Keep ordinary visitors arriving at a small minimum rate.

A new ride increases attraction but consumes space and money. An upgraded pastry stall serves faster and relieves queues but may bring fewer new visitors. Building two copies increases capacity; introducing another category offers a variety benefit. These trade-offs need actual playtesting.

## How the numbers work (17 Nov 2026)

This is what the simulation actually calculates. The values are in `Resources/Wintermarkt/market.json`.

- **Score: visitors served.** The festival lasts 10 minutes (`festivalSeconds` 600). Every visitor who is served counts once. Coins are not the score; they are only what you build with.
- **Coins.** You start with 350. A visitor pays the building's `income` at the moment service finishes (9 to 30 coins depending on building and level). Nothing else earns or costs coins except building and upgrading.
- **Arrivals.** Nobody arrives until the first building stands. After that a new visitor arrives every `8 / (1 + attraction x 0.035) / (0.5 + reputation / 100)` seconds, never faster than one every 0.5 s. More attraction and a better reputation both mean more visitors.
- **Attraction.** The sum of every building's attraction value, plus 15 for each different category beyond the first (drinks, food, crafts, rides, landmark). Rides have the most; upgrades add some.
- **What a visitor does.** They pick the building where queue time plus walking time is shortest, walk there, wait, get served, pay and leave. They visit one building. Each building has a fixed number of queue places (4 to 8).
- **Reputation (the HAPPY % in the HUD).** Starts at 60, stays between 0 and 100. Each served visitor adds 0.5. Each visitor who waits longer than 35 s and gives up, or arrives when every queue place is taken, removes 3. So one lost visitor undoes six served ones.
- **Capacity.** A building serves `capacity` visitors at once, each taking `serviceSeconds`. An upgrade raises capacity (1, 2, 3), shortens service and raises income.
- **The decision.** Attraction brings more people; capacity turns them into score and coins. Attraction without capacity fills the queues, visitors are lost, reputation falls and arrivals slow down.

Scripted players (`bash tools/balance-bots.sh`, 17 Nov 2026, these values, real site):

| Strategy | Arrived | Served (score) | Left unserved | Reputation | Coins earned |
|---|---|---|---|---|---|
| One stall, then nothing | 100 | 83 | 13 | 63 | 747 |
| Cheapest stall, repeatedly | 347 | 327 | 0 | 100 | 2943 |
| Attractions only, no upgrades | 396 | 276 | 55 | 33 | 4632 |
| Upgrade the busiest first | 306 | 296 | 0 | 100 | 4440 |
| Variety first, then upgrades | 566 | 535 | 13 | 100 | 9933 |
| Serve when busy, else attract | 503 | 478 | 0 | 100 | 7068 |

Mixed strategies now beat single-minded ones. These are bots that react perfectly, not people: no human playtest supports the numbers yet. Known weak points: reputation still reaches 100 easily for any careful strategy, visitors ignore attraction when choosing where to queue (so nobody needs to ride the rides), and late in the festival coins pile up with nothing left to buy.

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
