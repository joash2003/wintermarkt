using System;
using System.Collections.Generic;
using Wintermarkt.Core;

namespace Wintermarkt.Tests
{
    // The same assertions run in Unity EditMode and the offline C# runner.
    public static class MarketTestCases
    {
        public static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        public static MarketSimulation Create(float duration = 300)
        {
            var settings = new MarketSettings { startingCoins = 500, festivalSeconds = duration, baseArrivalSeconds = 2, minArrivalSeconds = 1, walkSecondsPerCell = .1f, patienceSeconds = 30,
                buildings = new[] { new BuildingDefinition { id = "stall", title = "Stall", category = "food", width = 2, depth = 2, queueSlots = 4,
                    levels = new[] { new BuildingLevel { cost = 100, capacity = 1, income = 10, serviceSeconds = 2, attraction = 5 },
                                     new BuildingLevel { cost = 120, capacity = 2, income = 10, serviceSeconds = 1, attraction = 8 } } } } };
            return new MarketSimulation(settings, new SiteData { rows = new[] { "##########", "#Eppppppp#", "#........#", "#........#", "#........#", "#........#", "#........#", "#........#", "##########" } });
        }
        public static void PlacementIsAtomic()
        {
            var sim = Create(); string reason;
            Require(!sim.TryPlace("stall", sim.Site.Entrance, out reason), "Entrance must be protected");
            Require(sim.Coins == 500 && sim.Buildings.Count == 0, "Rejected placement spent money");
            Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            Require(sim.Coins == 400, "Spend exactly once");
            Require(!sim.TryPlace("stall", new Cell(2, 2), out reason), "Overlap accepted");
            Require(!sim.TryPlace("stall", new Cell(2, 4), out reason), "Reserved queue overlap accepted");
            Require(sim.Coins == 400 && sim.Buildings.Count == 1, "Rejected overlap mutated state");
        }
        public static void NavigationRejectsWalls()
        {
            var sim = Create(); var blocked = new HashSet<Cell>();
            Require(sim.Site.Path(sim.Site.Entrance, sim.Site.Entrance, blocked).Count == 0, "Zero path");
            Require(sim.Site.Path(sim.Site.Entrance, new Cell(0, 0), blocked) == null, "Wall path");
            for (int x = 1; x <= 8; x++) blocked.Add(new Cell(x, 2));
            Require(sim.Site.Path(sim.Site.Entrance, new Cell(5, 4), blocked) == null, "Unreachable path");
        }
        public static void ServicePaysExactlyOnce()
        {
            var sim = Create(); sim.Settings.baseArrivalSeconds = 1000; sim.Settings.minArrivalSeconds = 1000;
            string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            sim.Tick(25);
            Require(sim.Welcomed == 1 && sim.Served == 1, "One visitor must complete service");
            Require(sim.Coins == 410 && sim.Earned == 10, "Service must pay once");
            sim.Tick(25); Require(sim.Coins == 410, "Departure paid again");
        }
        public static void QueueSlotsAreUnique()
        {
            var sim = Create(); sim.Settings.buildings[0].levels[0].serviceSeconds = 80;
            string reason; sim.TryPlace("stall", new Cell(2, 2), out reason); sim.Tick(15);
            var slots = new HashSet<int>();
            foreach (var v in sim.Visitors) if (v.State != VisitorState.Leaving && v.State != VisitorState.Gone)
                Require(slots.Add(v.Slot), "Queue slot double-booked");
            Require(slots.Count <= 4 && sim.Welcomed > slots.Count, "Queue capacity not respected");
        }
        public static void UpgradeAndPause()
        {
            var sim = Create(); string reason; sim.TryPlace("stall", new Cell(2, 2), out reason);
            Require(sim.TryUpgrade(1, out reason), reason); Require(sim.Coins == 280 && sim.Buildings[0].Stats.capacity == 2, "Upgrade wrong");
            Require(!sim.TryUpgrade(1, out reason) && sim.Coins == 280, "Max upgrade charged");
            sim.Paused = true; sim.Tick(40); Require(sim.Remaining == 300 && sim.Welcomed == 0, "Pause advanced time");
            Require(!sim.TryPlace("stall", new Cell(5, 2), out reason), "Built while paused");
            sim.Paused = false; sim.Tick(5); Require(sim.Welcomed > 0, "Resume failed");
        }
        public static void UpgradeImprovesThroughput()
        {
            var basic = Create(); var better = Create(); string reason;
            foreach (var sim in new[] { basic, better }) { sim.Settings.baseArrivalSeconds = 1; sim.Settings.minArrivalSeconds = 1; sim.Settings.buildings[0].levels[0].serviceSeconds = 8; sim.TryPlace("stall", new Cell(2, 2), out reason); }
            better.TryUpgrade(1, out reason); basic.Tick(90); better.Tick(90);
            Require(better.Served > basic.Served, "Upgrade did not increase actual completed service");
        }
        public static void FullRunStaysBounded()
        {
            var sim = Create(600); string reason; sim.TryPlace("stall", new Cell(2, 2), out reason); sim.TryPlace("stall", new Cell(5, 2), out reason);
            for (int i = 0; i < 6000; i++) { sim.Tick(.1f); Require(sim.Visitors.Count <= sim.Settings.maxVisitors, "Visitor leak"); Require(sim.Coins >= 0 && sim.Reputation >= 0 && sim.Reputation <= 100, "Invalid economy bounds"); }
            sim.Tick(1); Require(sim.Finished && sim.Served > 0, "Festival did not finish");
            int welcomed = sim.Welcomed; sim.Tick(100); Require(sim.Welcomed == welcomed, "Ended festival spawned visitors");
        }
        public static void InvalidDataFails()
        {
            bool rejected = false;
            try { new SiteGrid(new SiteData { rows = new[] { "###", "#E", "###" } }); } catch (ArgumentException) { rejected = true; }
            Require(rejected, "Ragged map accepted");
            var sim = Create(); sim.Settings.buildings[0].levels[0].serviceSeconds = 0;
            rejected = false; try { sim.Settings.Validate(); } catch (ArgumentException) { rejected = true; }
            Require(rejected, "Zero service duration accepted");
        }
        public static void NoArrivalsBeforeFirstBuilding()
        {
            var sim = Create(); sim.Tick(30);
            Require(sim.Welcomed == 0 && sim.Abandoned == 0 && sim.Reputation == sim.Settings.initialReputation, "Visitors arrived before anything was built");
            string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            sim.Tick(10); Require(sim.Welcomed > 0, "No arrivals after the first building");
        }
        // One visitor (arrivals are switched off after the first) in a market with two categories: food and drinks.
        static MarketSimulation CreateTwoCategories()
        {
            var sim = Create(); sim.Settings.baseArrivalSeconds = 1000; sim.Settings.minArrivalSeconds = 1000;
            var drink = new BuildingDefinition { id = "drink", title = "Drink", category = "drinks", width = 2, depth = 2, queueSlots = 4,
                levels = new[] { new BuildingLevel { cost = 100, capacity = 1, income = 7, serviceSeconds = 2, attraction = 5 } } };
            sim.Settings.buildings = new[] { sim.Settings.buildings[0], drink };
            return sim;
        }
        public static void VisitorVisitsEveryWish()
        {
            var sim = CreateTwoCategories(); string reason;
            Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason); Require(sim.TryPlace("drink", new Cell(6, 2), out reason), reason);
            sim.Tick(60);
            Require(sim.Welcomed == 1 && sim.Served == 2, "The visitor must be served at both buildings");
            Require(sim.Happy == 1 && sim.Disappointed == 0 && sim.Abandoned == 0, "A visitor who got everything is happy");
            Require(sim.Earned == 17, "Each stop pays once");
        }
        public static void MissingWishDisappoints()
        {
            var sim = CreateTwoCategories(); string reason;
            Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            sim.Tick(60);
            Require(sim.Welcomed == 1 && sim.Served == 1, "The visitor is still served where possible");
            Require(sim.Happy == 0 && sim.Disappointed == 1 && sim.Abandoned == 0, "A missing category disappoints, it is not a queue loss");
            Require(sim.Reputation == sim.Settings.initialReputation + sim.Settings.happinessGain - sim.Settings.disappointmentLoss, "Reputation: one service, one disappointment");
            Require(sim.MostWanted == "drinks", "The missing category is reported");
            Require(sim.TryPlace("drink", new Cell(6, 2), out reason), reason); Require(sim.MostWanted == null, "Building it clears the report");
        }
        // Two 30-second waves on the one-stall test market.
        static MarketSimulation CreateWaves(int firstTarget, float secondFactor = 1)
        {
            var basis = Create();
            basis.Settings.waves = new[] { new WaveDefinition { title = "One", seconds = 30, target = firstTarget },
                                           new WaveDefinition { title = "Two", seconds = 30, target = 0, arrivalFactor = secondFactor } };
            return new MarketSimulation(basis.Settings, basis.Site.Data);
        }
        public static void MissedWaveTargetEndsFestival()
        {
            var sim = CreateWaves(1000); string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            sim.Tick(29); Require(!sim.Finished && sim.Wave == 0, "The first wave ended early");
            sim.Tick(2); Require(sim.Failed && sim.Finished && !sim.Won && sim.Wave == 0, "A missed target must end the festival as a loss");
            int welcomed = sim.Welcomed; sim.Tick(30); Require(sim.Welcomed == welcomed, "A lost festival kept running");
        }
        public static void MetTargetsAdvanceAndWin()
        {
            var sim = CreateWaves(1); string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            sim.Tick(31); Require(!sim.Finished && sim.Wave == 1 && sim.Remaining > 28 && sim.Remaining <= 30, "A met target must start the next wave");
            Require(sim.WaveHappy < sim.Happy, "The wave count must restart");
            sim.Tick(31); Require(sim.Won && sim.Finished && !sim.Failed, "Passing the last wave wins");
        }
        public static void ArrivalFactorSpeedsArrivals()
        {
            var slow = CreateWaves(0, 1); var fast = CreateWaves(0, 4); string reason;
            foreach (var sim in new[] { slow, fast }) { sim.TryPlace("stall", new Cell(2, 2), out reason); sim.Tick(30.5f); }
            Require(slow.Welcomed == fast.Welcomed, "The first wave is identical");
            int before = slow.Welcomed; slow.Tick(25); fast.Tick(25);
            Require(fast.Welcomed - before > slow.Welcomed - before, "A higher arrival factor must bring more visitors");
        }
        public static void RemoveRefundsAndFreesSpace()
        {
            var sim = Create(); string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            Require(sim.TryUpgrade(1, out reason), reason); Require(sim.Coins == 280, "Setup");
            sim.Tick(6); Require(sim.Visitors.Count > 0, "Visitors must be on their way");
            Require(sim.TryRemove(1, out reason), reason);
            Require(sim.Coins > 280 + 109 && sim.Coins < 280 + 140 && sim.Buildings.Count == 0, "Removing refunds half of what was spent on the building");
            Require(!sim.TryRemove(1, out reason), "Removed twice");
            foreach (var v in sim.Visitors) Require(v.State == VisitorState.Leaving || v.State == VisitorState.Gone, "Visitors of a removed building must leave");
            sim.Tick(5); Require(sim.Visitors.Count == 0, "Visitors of a removed building must walk out");
            Require(sim.TryPlace("stall", new Cell(2, 2), out reason), "The space must be free again: " + reason);
            sim.Tick(30); Require(sim.Served > 0, "The market must keep working after a removal");
        }
        public static void MoodIsRecorded()
        {
            string reason; var seen = new HashSet<VisitorMood>();
            var happy = CreateTwoCategories(); happy.TryPlace("stall", new Cell(2, 2), out reason); happy.TryPlace("drink", new Cell(6, 2), out reason);
            var missing = CreateTwoCategories(); missing.TryPlace("stall", new Cell(2, 2), out reason);
            for (int i = 0; i < 300; i++) { happy.Tick(.1f); foreach (var v in happy.Visitors) seen.Add(v.Mood); }
            Require(seen.Contains(VisitorMood.None) && seen.Contains(VisitorMood.Happy) && seen.Count == 2, "A visitor who got everything leaves happy");
            seen.Clear(); for (int i = 0; i < 300; i++) { missing.Tick(.1f); foreach (var v in missing.Visitors) seen.Add(v.Mood); }
            Require(seen.Contains(VisitorMood.Disappointed) && !seen.Contains(VisitorMood.Happy), "A visitor who missed something leaves disappointed");
            var slow = Create(); slow.Settings.buildings[0].levels[0].serviceSeconds = 200; slow.TryPlace("stall", new Cell(2, 2), out reason);
            seen.Clear(); for (int i = 0; i < 450; i++) { slow.Tick(.1f); foreach (var v in slow.Visitors) seen.Add(v.Mood); }
            Require(seen.Contains(VisitorMood.GaveUp), "A visitor who waited too long gave up");
        }
        // The one-stall market with help calls switched on: a call every few seconds while visitors wait.
        static MarketSimulation CreateHelp()
        {
            var sim = Create(); sim.Settings.helpIntervalSeconds = 6; sim.Settings.helpWindowSeconds = 4; sim.Settings.boostSeconds = 10; sim.Settings.closedSeconds = 5; sim.Settings.helpTip = 3;
            sim.Settings.buildings[0].levels[0].serviceSeconds = 4;
            return sim;
        }
        static MarketBuilding WaitForCall(MarketSimulation sim)
        {
            for (int i = 0; i < 400; i++) { sim.Tick(.1f); foreach (var b in sim.Buildings) if (b.NeedsHelp) return b; }
            throw new Exception("No stall called for help");
        }
        public static void HelpedStallIsBoosted()
        {
            var sim = CreateHelp(); string reason; Require(sim.TryPlace("stall", new Cell(2, 2), out reason), reason);
            Require(!sim.TryHelp(1, out reason), "A stall that is not calling cannot be helped");
            var b = WaitForCall(sim); int coins = sim.Coins;
            Require(sim.TryHelp(b.Id, out reason), reason);
            Require(!b.NeedsHelp && b.BoostRemaining > 9 && sim.Coins == coins + 3 && sim.Helped == 1, "Helping boosts the stall and pays the tip once");
            Require(!sim.TryHelp(b.Id, out reason) && sim.Coins == coins + 3, "Helped twice");
            var plain = CreateHelp(); plain.Settings.helpIntervalSeconds = 0; plain.TryPlace("stall", new Cell(2, 2), out reason);
            var boosted = CreateHelp(); boosted.TryPlace("stall", new Cell(2, 2), out reason);
            for (int i = 0; i < 600; i++) { plain.Tick(.1f); boosted.Tick(.1f); foreach (var x in boosted.Buildings) if (x.NeedsHelp) boosted.TryHelp(x.Id, out reason); }
            Require(boosted.Served > plain.Served, "A stall that is always helped must serve more than one without help calls");
        }
        public static void MissedHelpClosesStall()
        {
            var sim = CreateHelp(); string reason; sim.TryPlace("stall", new Cell(2, 2), out reason);
            var b = WaitForCall(sim); sim.Tick(4.2f);
            Require(!b.NeedsHelp && b.ClosedRemaining > 4 && sim.MissedHelp == 1 && b.BoostRemaining <= 0, "A missed call closes the stall");
            sim.Tick(4.5f); int served = sim.Served; sim.Tick(.3f); Require(sim.Served == served && b.ClosedRemaining > 0, "A closed stall serves nobody");
            sim.Tick(12); Require(sim.Served > served, "The stall must reopen");
        }
        public static void HelpCallsGrowWithStalls()
        {
            var one = CreateHelp(); var two = CreateHelp(); string reason;
            one.TryPlace("stall", new Cell(2, 2), out reason); two.TryPlace("stall", new Cell(2, 2), out reason); Require(two.TryPlace("stall", new Cell(6, 2), out reason), reason);
            one.Tick(120); two.Tick(120);
            Require(one.MissedHelp > 0 && two.MissedHelp > one.MissedHelp, "Two stalls must call more often than one");
        }
        public static Action[] All { get { return new Action[] { HelpedStallIsBoosted, MissedHelpClosesStall, HelpCallsGrowWithStalls, MoodIsRecorded, RemoveRefundsAndFreesSpace, MissedWaveTargetEndsFestival, MetTargetsAdvanceAndWin, ArrivalFactorSpeedsArrivals, VisitorVisitsEveryWish, MissingWishDisappoints, NoArrivalsBeforeFirstBuilding, PlacementIsAtomic, NavigationRejectsWalls, ServicePaysExactlyOnce, QueueSlotsAreUnique, UpgradeAndPause, UpgradeImprovesThroughput, FullRunStaysBounded, InvalidDataFails }; } }
    }
}
