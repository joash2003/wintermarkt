using System;
using System.Collections.Generic;

namespace Wintermarkt.Core
{
    public enum VisitorState { Walking, Waiting, Serving, Leaving, Gone }

    public sealed class MarketBuilding
    {
        public int Id { get; internal set; }
        public BuildingDefinition Definition { get; internal set; }
        public Cell Origin { get; internal set; }
        public int Level { get; internal set; }
        public BuildingLevel Stats { get { return Definition.levels[Level]; } }
        public Cell QueueCell(int slot) { return new Cell(Origin.x + slot % Definition.width, Origin.y + Definition.depth + slot / Definition.width); }
    }

    public sealed class MarketVisitor
    {
        public int Id { get; internal set; }
        public VisitorState State { get; internal set; }
        public Cell Position { get; internal set; }
        public MarketBuilding Destination { get; internal set; }
        public int Slot { get; internal set; }
        /// <summary>Building categories this visitor still wants to visit.</summary>
        public List<string> Wishes { get; internal set; }
        public float Waited { get; internal set; }
        public float ServiceRemaining { get; internal set; }
        public List<Cell> Route { get; internal set; }
        public int RouteIndex { get; internal set; }
        public float EdgeProgress { get; internal set; }
        internal long QueueOrder;
        public Cell NextCell { get { return Route != null && RouteIndex < Route.Count ? Route[RouteIndex] : Position; } }
    }

    /// <summary>Deterministic simulation, independent of Unity, UI and rendering.</summary>
    public sealed class MarketSimulation
    {
        public readonly MarketSettings Settings;
        public readonly SiteGrid Site;
        readonly List<MarketBuilding> buildings = new List<MarketBuilding>();
        readonly List<MarketVisitor> visitors = new List<MarketVisitor>();
        readonly HashSet<Cell> occupied = new HashSet<Cell>();
        readonly HashSet<Cell> reserved = new HashSet<Cell>();
        public IReadOnlyList<MarketBuilding> Buildings { get { return buildings; } }
        public IReadOnlyList<MarketVisitor> Visitors { get { return visitors; } }
        public int Coins { get; private set; }
        public int Welcomed { get; private set; }
        public int Served { get; private set; }
        public int Abandoned { get; private set; }
        /// <summary>The score: visitors who left having got everything they wanted.</summary>
        public int Happy { get; private set; }
        /// <summary>Visitors who left because something they wanted was not built.</summary>
        public int Disappointed { get; private set; }
        public int Earned { get; private set; }
        public float Reputation { get; private set; }
        public float Remaining { get; private set; }
        public bool Paused { get; set; }
        /// <summary>Index of the wave being played, time left in it is Remaining.</summary>
        public int Wave { get; private set; }
        public int WaveCount { get { return waves.Length; } }
        public WaveDefinition CurrentWave { get { return waves[Wave]; } }
        /// <summary>Visitors who left happy during the current wave, counted against its target.</summary>
        public int WaveHappy { get; private set; }
        public bool Failed { get; private set; }
        public bool Won { get; private set; }
        public bool Finished { get { return Failed || Won; } }
        readonly WaveDefinition[] waves;
        float arrivalCountdown = 2;
        int nextBuilding = 1, nextVisitor = 1;
        long nextOrder = 1;
        readonly Dictionary<string, int> missed = new Dictionary<string, int>();
        /// <summary>The category most visitors left without since it was last built, or null.</summary>
        public string MostWanted
        {
            get { string best = null; int most = 0; foreach (var pair in missed) if (pair.Value > most) { most = pair.Value; best = pair.Key; } return best; }
        }

        public MarketSimulation(MarketSettings settings, SiteData site)
        {
            settings.Validate(); Settings = settings; Site = new SiteGrid(site);
            waves = settings.waves != null && settings.waves.Length > 0 ? settings.waves
                : new[] { new WaveDefinition { title = "Festival", seconds = settings.festivalSeconds } };
            Coins = settings.startingCoins; Reputation = settings.initialReputation; Remaining = waves[0].seconds;
        }

        public float Attraction
        {
            get
            {
                float sum = 0; var categories = new HashSet<string>();
                foreach (var b in buildings) { sum += b.Stats.attraction; categories.Add(b.Definition.category); }
                return sum + Math.Max(0, categories.Count - 1) * Settings.varietyBonus;
            }
        }
        public float ArrivalInterval { get { return Math.Max(Settings.minArrivalSeconds, Settings.baseArrivalSeconds / (1 + Attraction * .035f) / (.5f + Reputation / 100)) / CurrentWave.arrivalFactor; } }

        public BuildingDefinition FindDefinition(string id)
        { foreach (var d in Settings.buildings) if (d.id == id) return d; return null; }

        public MarketBuilding At(Cell c)
        {
            foreach (var b in buildings)
                if (c.x >= b.Origin.x && c.x < b.Origin.x + b.Definition.width && c.y >= b.Origin.y && c.y < b.Origin.y + b.Definition.depth) return b;
            return null;
        }

        public bool CanPlace(string id, Cell origin, out string reason)
        {
            if (Paused || Finished) { reason = "Resume the festival to build."; return false; }
            var d = FindDefinition(id);
            if (d == null) { reason = "Unknown building."; return false; }
            if (Welcomed < d.unlockVisitors) { reason = "Welcome " + d.unlockVisitors + " visitors to unlock."; return false; }
            if (Coins < d.levels[0].cost) { reason = "Not enough coins."; return false; }
            var footprint = new HashSet<Cell>();
            for (int y = 0; y < d.depth; y++) for (int x = 0; x < d.width; x++)
            {
                Cell c = new Cell(origin.x + x, origin.y + y);
                if (!Site.Buildable(c) || occupied.Contains(c) || reserved.Contains(c)) { reason = "Keep paths, trees and existing plots clear."; return false; }
                footprint.Add(c);
            }
            var candidate = new MarketBuilding { Origin = origin, Definition = d };
            var queueCells = new HashSet<Cell>();
            for (int i = 0; i < d.queueSlots; i++)
            {
                Cell c = candidate.QueueCell(i);
                if (!Site.Buildable(c) || occupied.Contains(c) || reserved.Contains(c)) { reason = "Leave clear space in front for the queue."; return false; }
                queueCells.Add(c);
            }
            // Protect committed walking paths and queue positions from new buildings.
            foreach (var v in visitors)
            {
                if (footprint.Contains(v.Position) || queueCells.Contains(v.Position)) { reason = "A visitor is using this space."; return false; }
                for (int i = v.RouteIndex; v.Route != null && i < v.Route.Count; i++)
                    if (footprint.Contains(v.Route[i]) || queueCells.Contains(v.Route[i])) { reason = "A visitor is walking through this space."; return false; }
            }
            var blocked = new HashSet<Cell>(occupied); blocked.UnionWith(footprint);
            var reachable = Site.Reachable(Site.Entrance, blocked);
            foreach (Cell c in reserved) if (!reachable.Contains(c)) { reason = "This would block an existing queue."; return false; }
            for (int i = 0; i < d.queueSlots; i++) if (!reachable.Contains(candidate.QueueCell(i))) { reason = "Connect the queue to the entrance."; return false; }
            foreach (var v in visitors) if (!reachable.Contains(v.Position)) { reason = "This would trap a visitor."; return false; }
            reason = "Click to build " + d.title + " · " + d.levels[0].cost + " coins"; return true;
        }

        public bool TryPlace(string id, Cell origin, out string reason)
        {
            if (!CanPlace(id, origin, out reason)) return false;
            var d = FindDefinition(id);
            var b = new MarketBuilding { Id = nextBuilding++, Definition = d, Origin = origin, Level = 0 };
            Coins -= b.Stats.cost; buildings.Add(b);
            for (int y = 0; y < d.depth; y++) for (int x = 0; x < d.width; x++) occupied.Add(new Cell(origin.x + x, origin.y + y));
            for (int i = 0; i < d.queueSlots; i++) reserved.Add(b.QueueCell(i));
            missed.Remove(d.category);
            reason = d.title + " opened!"; return true;
        }

        public bool TryUpgrade(int id, out string reason)
        {
            if (Paused || Finished) { reason = "Resume the festival to upgrade."; return false; }
            MarketBuilding b = null; foreach (var item in buildings) if (item.Id == id) b = item;
            if (b == null) { reason = "Select a building."; return false; }
            if (b.Level + 1 >= b.Definition.levels.Length) { reason = "Maximum level reached."; return false; }
            var next = b.Definition.levels[b.Level + 1];
            if (Coins < next.cost) { reason = "Not enough coins."; return false; }
            Coins -= next.cost; b.Level++; reason = b.Definition.title + " upgraded to level " + (b.Level + 1) + "!"; return true;
        }

        /// <summary>Coins returned when a building is removed: half of what was spent on it.</summary>
        public int Refund(MarketBuilding b) { int spent = 0; for (int i = 0; i <= b.Level; i++) spent += b.Definition.levels[i].cost; return spent / 2; }

        // On a small plot the player must be able to make room again. Visitors on their way to the building look for another stop.
        public bool TryRemove(int id, out string reason)
        {
            if (Paused || Finished) { reason = "Resume the festival to remove."; return false; }
            MarketBuilding b = null; foreach (var item in buildings) if (item.Id == id) b = item;
            if (b == null) { reason = "Select a building."; return false; }
            Coins += Refund(b); buildings.Remove(b);
            for (int y = 0; y < b.Definition.depth; y++) for (int x = 0; x < b.Definition.width; x++) occupied.Remove(new Cell(b.Origin.x + x, b.Origin.y + y));
            for (int i = 0; i < b.Definition.queueSlots; i++) reserved.Remove(b.QueueCell(i));
            foreach (var v in visitors)
                if (v.Destination == b && v.State != VisitorState.Leaving && v.State != VisitorState.Gone) { v.Slot = -1; Continue(v); }
            reason = b.Definition.title + " removed."; return true;
        }

        public int QueueCount(MarketBuilding b)
        { int count = 0; foreach (var v in visitors) if (v.Destination == b && v.State != VisitorState.Leaving && v.State != VisitorState.Gone) count++; return count; }
        bool SlotTaken(MarketBuilding b, int slot)
        { foreach (var v in visitors) if (v.Destination == b && v.Slot == slot && v.State != VisitorState.Leaving && v.State != VisitorState.Gone) return true; return false; }

        public void Tick(float seconds)
        {
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException("seconds");
            if (Paused || Finished || seconds == 0) return;
            // Fixed substeps make service/arrival behavior independent of render frame rate.
            while (seconds > .00001f && !Finished)
            {
                float dt = Math.Min(.1f, Math.Min(seconds, Remaining)); seconds -= dt; Remaining = Math.Max(0, Remaining - dt);
                // Nobody arrives at an empty market: the festival score and reputation start with the first building.
                if (buildings.Count > 0) arrivalCountdown -= dt;
                if (arrivalCountdown <= 0) { if (visitors.Count < Settings.maxVisitors) SpawnVisitor(); arrivalCountdown = ArrivalInterval; }
                foreach (var v in visitors)
                {
                    if (v.State == VisitorState.Walking || v.State == VisitorState.Leaving) Move(v, dt);
                    if (v.State == VisitorState.Waiting)
                    {
                        v.Waited += dt;
                        if (v.Waited >= Settings.patienceSeconds) { Abandoned++; Reputation = Math.Max(0, Reputation - Settings.abandonmentLoss); Leave(v); }
                    }
                    if (v.State == VisitorState.Serving)
                    {
                        v.ServiceRemaining -= dt;
                        if (v.ServiceRemaining <= 0)
                        {
                            int income = v.Destination.Stats.income; Coins += income; Earned += income; Served++;
                            Reputation = Math.Min(100, Reputation + Settings.happinessGain);
                            v.Wishes.Remove(v.Destination.Definition.category); v.Slot = -1; Continue(v);
                        }
                    }
                }
                foreach (var b in buildings) StartService(b);
                visitors.RemoveAll(v => v.State == VisitorState.Gone);
                if (Remaining <= 0)
                {
                    // End of a wave: a missed target loses the festival, the last wave passed wins it.
                    if (WaveHappy < CurrentWave.target) Failed = true;
                    else if (Wave + 1 >= waves.Length) Won = true;
                    else { Wave++; WaveHappy = 0; Remaining = CurrentWave.seconds; }
                }
            }
        }

        void SpawnVisitor()
        {
            var v = new MarketVisitor { Id = nextVisitor++, Position = Site.Entrance, State = VisitorState.Leaving, Slot = -1 };
            // Two or three different wishes from the categories the player can build right now; the pattern repeats so runs are deterministic.
            var unlocked = new List<string>(); v.Wishes = new List<string>();
            foreach (var d in Settings.buildings) if (Welcomed >= d.unlockVisitors && !unlocked.Contains(d.category)) unlocked.Add(d.category);
            int count = Math.Max(1, Math.Min(unlocked.Count, Settings.maxWishes - v.Id % 2));
            for (int i = 0; i < count && unlocked.Count > 0; i++) v.Wishes.Add(unlocked[(v.Id * 7 + i) % unlocked.Count]);
            Welcomed++; visitors.Add(v); Continue(v);
        }

        // Next stop: the wanted building with the shortest queue plus walk. With nothing left to visit the visitor goes home.
        void Continue(MarketVisitor v)
        {
            MarketBuilding chosen = null; int slot = -1; List<Cell> route = null; float best = float.MaxValue; bool full = false;
            foreach (var b in buildings)
            {
                if (!v.Wishes.Contains(b.Definition.category)) continue;
                bool free = false;
                for (int i = 0; i < b.Definition.queueSlots; i++)
                {
                    if (SlotTaken(b, i)) continue;
                    free = true;
                    var path = Site.Path(v.Position, b.QueueCell(i), occupied); if (path == null) continue;
                    float score = QueueCount(b) * b.Stats.serviceSeconds / b.Stats.capacity + path.Count * Settings.walkSecondsPerCell;
                    if (score < best) { best = score; chosen = b; slot = i; route = path; }
                    break;
                }
                if (!free) full = true;
            }
            if (chosen != null)
            {
                v.Destination = chosen; v.Slot = slot; v.Route = route; v.RouteIndex = 0; v.EdgeProgress = 0; v.Waited = 0; v.QueueOrder = nextOrder++;
                v.State = route.Count == 0 ? VisitorState.Waiting : VisitorState.Walking; return;
            }
            if (full) { Abandoned++; Reputation = Math.Max(0, Reputation - Settings.abandonmentLoss); }
            else if (v.Wishes.Count == 0) { Happy++; WaveHappy++; }
            else
            {
                Disappointed++; Reputation = Math.Max(0, Reputation - Settings.disappointmentLoss);
                foreach (string wish in v.Wishes) { int n; missed.TryGetValue(wish, out n); missed[wish] = n + 1; }
            }
            Leave(v);
        }

        void Move(MarketVisitor v, float dt)
        {
            if (v.Route == null) { v.State = VisitorState.Gone; return; }
            v.EdgeProgress += dt / Settings.walkSecondsPerCell;
            while (v.EdgeProgress >= 1 && v.RouteIndex < v.Route.Count)
            { v.Position = v.Route[v.RouteIndex++]; v.EdgeProgress -= 1; }
            if (v.RouteIndex >= v.Route.Count)
            { v.EdgeProgress = 0; v.State = v.State == VisitorState.Leaving ? VisitorState.Gone : VisitorState.Waiting; }
        }

        void StartService(MarketBuilding b)
        {
            int active = 0; foreach (var v in visitors) if (v.Destination == b && v.State == VisitorState.Serving) active++;
            while (active < b.Stats.capacity)
            {
                MarketVisitor first = null;
                foreach (var v in visitors)
                    if (v.Destination == b && v.State == VisitorState.Waiting && (first == null || v.QueueOrder < first.QueueOrder)) first = v;
                if (first == null) break;
                first.State = VisitorState.Serving; first.ServiceRemaining = b.Stats.serviceSeconds; active++;
            }
        }
        void Leave(MarketVisitor v)
        {
            v.State = VisitorState.Leaving; v.Slot = -1; v.EdgeProgress = 0; v.RouteIndex = 0;
            v.Route = Site.Path(v.Position, Site.Entrance, occupied);
            if (v.Route == null) throw new InvalidOperationException("Visitor exit was blocked despite placement validation.");
        }
    }
}
