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
- Each building has three visible levels in the first version, and each level looks like a different building, as in Clash of Clans (issue 44): a stall goes from a low shed to the proper Hütte to a bigger, taller grand Hütte with a nutcracker and a tree; the carousel and the wheel gain seats, cabins and lights. On upgrade the new building pops up out of a burst of gold sparks. Upgrades change appearance, attraction, capacity or service rate. Data supports additional levels later; no unbounded upgrade system in the first prototype.
- One visitor behavior system. Visitors arrive with two or three wishes, visit a building for each, queue, receive service, and leave. No complex per-visitor economy or daily routines.
- End screen reports a win or a loss with the wave reached (issue 20), happy visitors (the score), visitors who arrived, who wanted more, who gave up, coins earned and final reputation.

All numbers, building counts and level limits above are provisional scope choices.

## Economy and visitor rules

Coins buy buildings and upgrades. Coins are earned by actual completed service, not by clicking a collection icon on a timer. No premium currency or real-money timers.

Arrival rate grows with the market's attraction rating. Reputation from visitor experience modifies that rate. Attraction should reward category variety so spamming the cheapest stall is not automatically optimal. Start with a transparent, tunable formula and show its contributors in the UI.

Each building has an entry/queue cell, a maximum queue length, service duration and service capacity. Visitors cannot queue indefinitely: show impatience, then let them leave, affecting reputation. A completed visit improves reputation. Clamp reputation and arrivals so the simulation stays bounded and a poor start can recover. Keep ordinary visitors arriving at a small minimum rate.

A new ride increases attraction but consumes space and money. An upgraded pastry stall serves faster and relieves queues but may bring fewer new visitors. Building two copies increases capacity; introducing another category offers a variety benefit. These trade-offs need actual playtesting.

## How the numbers work (17 Nov 2026)

The game is designed to be played once, for about ten minutes (decided 17 Nov 2026): four short days of one market season (18 Nov, issue 27). This is what the simulation calculates. The values are in `Resources/Wintermarkt/market.json`.

- **Four days, one star each.** First Advent (120 s, 5 happy visitors for the star), Krampus night (150 s, 18, arrivals x1.3), Christmas Eve (180 s, 48, arrivals x1.6), New Year's Eve (150 s, 48, arrivals x1.3). A missed target costs the star but the season goes on. It ends early only if reputation falls to zero. The ending card gives a verdict by stars, and fireworks play when the last day ends.
- **Happy visitor.** Every visitor arrives wanting two or three different categories (drinks, food, crafts, rides, landmark), in a fixed repeating pattern from the categories that are unlocked. They visit one building per wish, each time the one where queue plus walk is shortest. Only a visitor who got everything is happy.
- **How a visit ends.** Got everything: happy. Something they wanted is not built: "wanted more", reputation -0.5, and the HUD names the category visitors miss most. Waited over 35 s, or every queue for what they want is full: "gave up", reputation -2.
- **Stalls call for help.** A stall with a queue calls about every 25 s (more often in acts 2 and 3). Click it within 8 s: it serves twice as fast for 15 s and pays 5 coins. Miss it: it closes for 10 s. Every stall has its own timer, so a bigger market needs more of the player.
- **Events, one challenge per day.** First Advent: a full tram (6 visitors at once). Krampus night: the Krampus run. Christmas Eve: a school class (12 visitors who want only food, announced 20 s ahead) and a snowstorm (45 s, announced 15 s ahead: patience x0.6, everyone also wants a hot drink, and ten snow piles land on free queue spots). New Year's Eve: a packed last tram (10 visitors) and a power cut (every stall calls at once, 12 s to answer, no warning).
- **Snow piles.** A queue spot under snow cannot be used. Holding the mouse button and moving over a pile sweeps it and the cells next to it clear.
- **The Krampus run.** 50 s into Krampus night, announced 20 s ahead, four Krampus enter at the gate one after the other. Each walks to the nearest visitor and scares away everyone within two cells (reputation -0.5 each); visitors at a stall the player has just helped stay. A click on or near a Krampus throws a snowball; the third hit drives it off. Any still there run off when the day ends.
- **On the map.** Each category has one colour (sign and flag on the building, swatch in the build menu). Visitors carry no markers above their heads (Joash, 18 Nov 2026); their wishes and mood exist only in the simulation. A calling stall shows a yellow ball with a shrinking red ring.
- **Space.** Only the corner within 70 m of the Goethekreuzung entrance is the market: 224 buildable cells of 4 m, minus trees and paths. A stall needs 4 cells plus 4 for its queue, the carousel 9 + 6, the wheel 12 + 8. A building can be removed for half of what was spent on it.
- **Coins.** You start with 400, enough for the three stalls. A visitor pays the building's `income` each time a service finishes (4 to 15 coins).
- **Arrivals.** Nobody arrives until the first building stands. After that a new visitor arrives every `10 / (1 + attraction x 0.035) / (0.5 + reputation / 100)` seconds, never faster than one per second, divided by the act's arrival factor.
- **Attraction.** The sum of every building's attraction value, plus 15 for each different category beyond the first.
- **Reputation.** Starts at 60, stays between 0 and 100. Each service adds 0.5; see above for the losses.
- **Capacity.** A building serves `capacity` visitors at once, each taking `serviceSeconds`. An upgrade raises capacity (1, 2, 3), shortens service and raises income.
- **Unlocks.** The carousel unlocks after 25 arrivals and the wheel after 80. From then on some visitors want a ride or the landmark too, so room has to be kept for them.

Scripted players (`bash tools/balance-bots.sh`, 18 Nov 2026, these values, real site). They answer a help call after about 3 seconds and miss one in four, sweep one snow pile and throw one snowball a second, except the one marked otherwise. "Happy per day" is happy visitors against the star target:

| Strategy | Result | Happy per day | Happy | Wanted more | Gave up | Scared off | Krampus driven off |
|---|---|---|---|---|---|---|---|
| One stall, then nothing | 0 stars | 0/5, 0/18, 0/48, 0/48 | 0 | 107 | 5 | 2 | 4 |
| Cheapest stall, repeatedly | 0 stars | 0/5, 0/18, 0/48, 0/48 | 0 | 184 | 0 | 4 | 4 |
| Attractions only, no upgrades | 0 stars | 3/5, 16/18, 8/48, 8/48 | 35 | 172 | 60 | 9 | 4 |
| Upgrade the busiest first | 0 stars | 0/5, 0/18, 0/48, 0/48 | 0 | 162 | 0 | 3 | 4 |
| One of each, then upgrades | 4 stars | 7/5, 30/18, 77/48, 74/48 | 188 | 6 | 173 | 8 | 4 |
| One of each, then upgrades, never helps, sweeps or throws | Closed on day 3 | 5/5, 4/18 | 9 | 6 | 32 | 44 | 0 |
| One of each, then what visitors miss, duplicates when busy | 4 stars | 10/5, 24/18, 68/48, 76/48 | 178 | 39 | 187 | 7 | 4 |

A one-sided market plays the whole season without a star; the two mixed strategies get all four; the same mixed strategy with idle hands loses its reputation and closes on day 3. These are bots, not people: they never miss a snowball, and the targets were set at about two thirds of the weaker mixed bot. No human playtest supports the numbers yet. Known weak points: well over a hundred visitors still give up in a four-star run; coins are left over once the plot is full; queue positions still do not move up (issue 14).

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
