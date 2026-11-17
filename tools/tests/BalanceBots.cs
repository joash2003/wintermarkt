using System; using System.Collections.Generic; using System.IO; using System.Linq; using System.Web.Script.Serialization; using Wintermarkt.Core;
// Balance harness: scripted players against the real simulation, market.json and site.json. Run: bash tools/balance-bots.sh
static class BalanceBots
{
    static List<Cell> cells;
    static bool Place(MarketSimulation s, string id) { string r; if (s.Coins < s.FindDefinition(id).levels[0].cost || s.Welcomed < s.FindDefinition(id).unlockVisitors) return false; foreach (var c in cells) if (s.TryPlace(id, c, out r)) return true; return false; }
    static bool Upgrade(MarketSimulation s, Func<MarketBuilding, float> key) { string r; foreach (var b in s.Buildings.OrderByDescending(key)) if (s.TryUpgrade(b.Id, out r)) return true; return false; }
    static bool Any(MarketSimulation s, string id) { return s.Buildings.Any(b => b.Definition.id == id); }
    static int Count(MarketSimulation s, string id) { return s.Buildings.Count(b => b.Definition.id == id); }
    static bool Busy(MarketSimulation s) { return s.Buildings.Any(b => s.QueueCount(b) >= b.Definition.queueSlots - 1); }
    static readonly string[] ByAttraction = { "wheel", "carousel", "craft", "pastry", "punsch" };
    static readonly Dictionary<string, Action<MarketSimulation>> Strategies = new Dictionary<string, Action<MarketSimulation>> {
        { "one stall, then nothing", s => { if (s.Buildings.Count == 0) Place(s, "punsch"); } },
        { "cheapest stall, repeatedly", s => Place(s, "punsch") },
        { "attractions only, no upgrades", s => { if (s.Buildings.Count == 0) { Place(s, "punsch"); return; } foreach (var id in ByAttraction) { var d = s.FindDefinition(id); if (s.Welcomed >= d.unlockVisitors) { Place(s, id); return; } } } },
        { "upgrade busiest first", s => { if (s.Buildings.Count == 0) { Place(s, "punsch"); return; } if (!Upgrade(s, b => s.QueueCount(b))) { if (s.Buildings.All(b => b.Level + 1 >= b.Definition.levels.Length)) Place(s, "punsch"); } } },
        { "variety first, then upgrades", s => { foreach (var id in new[] { "punsch", "pastry", "craft", "carousel", "wheel" }) if (!Any(s, id)) { var d = s.FindDefinition(id); if (s.Welcomed >= d.unlockVisitors) { Place(s, id); return; } } Upgrade(s, b => s.QueueCount(b)); } },
        { "listen: one of each, then what is missing, serve when busy, else attract", s => {
            foreach (var d in s.Settings.buildings.OrderBy(d => d.levels[0].cost)) if (s.Welcomed >= d.unlockVisitors && !Any(s, d.id) && s.Coins >= d.levels[0].cost && Place(s, d.id)) return;
            if (s.MostWanted != null) { foreach (var d in s.Settings.buildings) if (d.category == s.MostWanted && Place(s, d.id)) return; }
            // A player who reads the build menu keeps room for the rides: no duplicates before every kind stands.
            bool complete = s.Settings.buildings.All(d => Any(s, d.id));
            if (Busy(s)) { if (Upgrade(s, b => s.QueueCount(b))) return; var busiest = s.Buildings.OrderByDescending(b => s.QueueCount(b)).First(); if (complete && Place(s, busiest.Definition.id)) return; }
            if (complete) foreach (var id in ByAttraction) { if (Place(s, id)) return; }
            Upgrade(s, b => s.QueueCount(b)); } },
    };
    static int Main(string[] a)
    {
        var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var site = json.Deserialize<SiteData>(File.ReadAllText(a[1]));
        var grid = new SiteGrid(site); var e = grid.Entrance; cells = new List<Cell>();
        for (int y = 0; y < grid.Height; y++) for (int x = 0; x < grid.Width; x++) if (Math.Abs(x - e.x) + Math.Abs(y - e.y) > 3 && grid.Buildable(new Cell(x, y))) cells.Add(new Cell(x, y));
        cells = cells.OrderBy(c => Math.Abs(c.x - e.x) + Math.Abs(c.y - e.y)).Take(700).ToList();
        Console.WriteLine("{0,-74}{1,-16}{2,8}{3,7}{4,8}{5,8}{6,5}{7,7}{8,6}{9,6}", "strategy", "result", "arrived", "happy", "wanted+", "gave up", "rep", "coins", "bldgs", "room");
        foreach (var pair in Strategies)
        {
            var s = new MarketSimulation(json.Deserialize<MarketSettings>(File.ReadAllText(a[0])), site); var perWave = new List<string>(); int wave = 0, happy = 0;
            bool can = true; string r = null;
            while (!s.Finished) { pair.Value(s); happy = s.WaveHappy; wave = s.Wave; if (s.Remaining < 1.5f && s.Coins >= 100) can = cells.Any(c => s.CanPlace("punsch", c, out r)); s.Tick(1); if (s.Wave != wave || s.Finished) perWave.Add((s.Finished && s.Wave == wave ? s.WaveHappy : happy) + "/" + s.Settings.waves[wave].target); }
            Console.WriteLine("{0,-74}{1,-16}{2,8}{3,7}{4,8}{5,8}{6,5:0}{7,7}{8,6}{9,6}", pair.Key, (s.Won ? "WON " : "lost ") + string.Join(" ", perWave), s.Welcomed, s.Happy, s.Disappointed, s.Abandoned, s.Reputation, s.Coins, s.Buildings.Count, s.Coins < 100 ? "?" : can ? "yes" : "FULL");
        }
        return 0;
    }
}
