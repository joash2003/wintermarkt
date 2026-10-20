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
        public static Action[] All { get { return new Action[] { PlacementIsAtomic, NavigationRejectsWalls, ServicePaysExactlyOnce, QueueSlotsAreUnique, UpgradeAndPause, UpgradeImprovesThroughput, FullRunStaysBounded, InvalidDataFails }; } }
    }
}
