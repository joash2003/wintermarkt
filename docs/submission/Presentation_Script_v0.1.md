# Linz Wintermarkt: presentation script (20 Oct 2026)

About 5 minutes in total. Speakers: Joash (slides 1-3), Bobby (slides 4-5), Alibek (slides 6-8), Joash closes (slide 9). Speak it in your own words; the text is a guide, not something to read word for word.

## Slide 1: Cover (Joash, ~20 s)

Hi, we are Joash, Alibek and Bobby. Our game is called Linz Wintermarkt, which is a working title. It's a small management game where you build and run a Christmas market in Linz, in the Volksgarten next to Goethekreuzung.

## Slide 2: Objective (Joash, ~40 s)

The goal is simple: welcome as many visitors as possible before the market closes. A festival lasts about ten minutes.

The loop goes like this. You look at the space and your budget, you place a stall or a ride, visitors arrive and pick a queue, they get served, and you earn coins. Then you spend those coins on more buildings or on upgrades.

Your score is the number of unique visitors who came to your market. We also show how many were served, how many gave up in a queue, how many coins you earned, and your reputation.

## Slide 3: Meaningful decisions (Joash, ~45 s)

The main decision in the game is this: do you spend money to attract more people, or to serve the people who are already there?

A ride like the carousel or the Ferris wheel makes the market more attractive, so more visitors come. But it's expensive and takes up a lot of space. Upgrading a stall makes it serve faster and handle more people at once, so queues get shorter and visitors stay happy, but it doesn't bring many new people in.

If you only build attractions, the queues get too long, people leave and your reputation drops. If you only upgrade, the market stays quiet. We wrote a first simulation of these rules and ran it with simple automated strategies. At our current numbers the queues almost never overflow, so building attractions always wins. That tells us the balance isn't right yet, and fixing it is our next design task.

## Slide 4: Chosen location (Bobby, ~45 s)

We set the game at Goethekreuzung, with the market inside the Volksgarten. That's where the real Christmas market with rides is, and the tram stop is right at the entrance.

This picture is a concept mockup, not a screenshot from the game yet. It shows the camera angle we're aiming for: you look down at the park, the stalls and rides are inside it, and trams and buses drive past in the background. Mariendom and Hauptplatz stay at their real locations and appear on a city overview, not inside the park.

Paths, trees and the entrance are fixed, and players build only on the free plots. The layout you see is still a rough blockout. We still need to compare it with maps and photos of the real site.

## Slide 5: Game structure (Bobby, ~35 s)

Here are the formal elements from the design workshop. There is one player, the market manager. The objective is to welcome as many visitors as possible. You place, inspect and upgrade buildings, and your resources are coins, space and time.

The conflict is attraction against capacity, which is the decision from slide 3. The boundary is one fixed park, and the outcome is the festival result at the end.

## Slide 6: MDA (Alibek, ~40 s)

In MDA terms, the mechanics are the rules the game calculates: placing buildings, spending coins, visitors choosing a queue, service earning money, and reputation affecting how many people arrive.

From those rules we expect certain dynamics. More attraction brings more visitors, more visitors make queues longer, good service raises reputation, and income lets you build more.

The feeling we want is a cozy, busy market: you're proud of what you built, and you feel some pressure without getting frustrated. Only playtests can show whether people actually feel that.

## Slide 7: First playable (Alibek, ~35 s)

Our first goal is a small version of the whole loop: one park, three stall types, two rides, one entrance, coins from service, attraction, patience and reputation, a ten-minute festival with a results screen, and pause and restart.

The simulation behind this is already written and has automated tests. The next step is getting it running inside Unity as a playable scene, so we can test it in class on the 1st of December.

## Slide 8: Team responsibilities (Alibek, ~30 s)

We divided the work equally. Joash handles the game rules, the economy and the balancing, and coordinates the weekly deck. I'm responsible for gameplay and the technical side in Unity: visitor flow, placement, shops, rides, upgrades and builds. Bobby works on the world and the interface: the Volksgarten location, the trams and buses, the visual style and the UI.

All three of us share the design deck, the playtests and the presentations.

## Slide 9: Thank you (Joash, ~10 s)

That's our plan for Linz Wintermarkt. Thank you, and we're happy to take questions.

## Likely questions

- **Why not a real map of Linz?** We want the park to be recognizable, so we'll match the main paths, the entrance and the streets to maps and photos. We won't reproduce every building or the real stall plan.
- **Is the game playable yet?** The core simulation is written and tested. The Unity scene is the next step, and our target for a playable version is the playtest on 1 December.
- **What if one strategy always wins?** It already does in our first simulation. That's why balancing is a named task, and we'll check it in playtests.
- **Why ten minutes?** It gives the game a clear end so spending decisions matter. We'll change it if playtests show it's too long or too short.
