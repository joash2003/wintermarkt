using System;
using System.Collections.Generic;

namespace Wintermarkt.Core
{
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int x, y;
        public Cell(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(Cell other) { return x == other.x && y == other.y; }
        public override bool Equals(object obj) { return obj is Cell && Equals((Cell)obj); }
        public override int GetHashCode() { return (x * 397) ^ y; }
        public static bool operator ==(Cell a, Cell b) { return a.Equals(b); }
        public static bool operator !=(Cell a, Cell b) { return !a.Equals(b); }
        public override string ToString() { return x + "," + y; }
    }

    [Serializable]
    public sealed class BuildingLevel
    {
        public int cost, capacity, income;
        public float serviceSeconds, attraction;
    }

    [Serializable]
    public sealed class BuildingDefinition
    {
        public string id, title, category;
        public int width = 2, depth = 2, queueSlots = 4, unlockVisitors;
        public BuildingLevel[] levels;
    }

    [Serializable]
    public sealed class MarketSettings
    {
        public int startingCoins = 350, maxVisitors = 60;
        public float festivalSeconds = 600, baseArrivalSeconds = 12, minArrivalSeconds = 2;
        public float walkSecondsPerCell = .6f, patienceSeconds = 35;
        public float initialReputation = 60, happinessGain = 1, abandonmentLoss = 2;
        public float varietyBonus = 3, disappointmentLoss = 1;
        public int maxWishes = 3;
        public BuildingDefinition[] buildings;

        public void Validate()
        {
            if (startingCoins < 0 || maxVisitors < 1 || festivalSeconds <= 0 || minArrivalSeconds <= 0 ||
                baseArrivalSeconds < minArrivalSeconds || walkSecondsPerCell <= 0 || patienceSeconds <= 0 ||
                initialReputation < 0 || initialReputation > 100 || happinessGain < 0 || abandonmentLoss < 0 || varietyBonus < 0 || disappointmentLoss < 0 || maxWishes < 1)
                throw new ArgumentException("Invalid market simulation settings.");
            if (buildings == null || buildings.Length == 0) throw new ArgumentException("At least one building is required.");
            var ids = new HashSet<string>();
            foreach (var b in buildings)
            {
                if (b == null || string.IsNullOrEmpty(b.id) || !ids.Add(b.id) || b.width < 1 || b.depth < 1 ||
                    b.queueSlots < 1 || b.unlockVisitors < 0 || b.levels == null || b.levels.Length == 0)
                    throw new ArgumentException("Invalid or duplicate building definition.");
                foreach (var l in b.levels)
                    if (l == null || l.cost < 0 || l.capacity < 1 || l.capacity > b.queueSlots || l.income < 0 ||
                        l.serviceSeconds <= 0 || l.attraction < 0)
                        throw new ArgumentException("Invalid level values for " + b.id);
            }
        }
    }

    [Serializable]
    public sealed class SiteData
    {
        public string name, geographyStatus;
        public float metersPerCell = 4;
        public string[] rows;
        public SiteFeature[] features = new SiteFeature[0];
    }

    /// <summary>Scenery around the grid in world metres: park (outline), path, street, building, label, tram or bus. Points are x,z pairs.</summary>
    [Serializable]
    public sealed class SiteFeature
    {
        public string kind, name;
        public float[] points;
        public float width, height, stopAt;
    }

    public sealed class SiteGrid
    {
        public readonly SiteData Data;
        public int Width { get { return Data.rows[0].Length; } }
        public int Height { get { return Data.rows.Length; } }
        public Cell Entrance { get; private set; }
        static readonly Cell[] Directions = { new Cell(1, 0), new Cell(0, 1), new Cell(-1, 0), new Cell(0, -1) };

        public SiteGrid(SiteData data)
        {
            if (data == null || data.rows == null || data.rows.Length < 3 || string.IsNullOrEmpty(data.rows[0]) || data.metersPerCell <= 0)
                throw new ArgumentException("Site needs rows and a positive metersPerCell.");
            Data = data;
            int entrances = 0;
            for (int y = 0; y < Height; y++)
            {
                if (data.rows[y] == null || data.rows[y].Length != Width) throw new ArgumentException("Site rows must be rectangular.");
                for (int x = 0; x < Width; x++)
                {
                    char tile = data.rows[y][x];
                    if ("#.pET".IndexOf(tile) < 0) throw new ArgumentException("Unknown site tile: " + tile);
                    if (tile == 'E') { Entrance = new Cell(x, y); entrances++; }
                }
            }
            if (entrances != 1) throw new ArgumentException("Site needs exactly one E entrance.");
        }

        public bool Inside(Cell c) { return c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height; }
        public bool Walkable(Cell c) { return Inside(c) && Data.rows[c.y][c.x] != '#' && Data.rows[c.y][c.x] != 'T'; }
        public bool Buildable(Cell c) { return Inside(c) && Data.rows[c.y][c.x] == '.'; }
        public HashSet<Cell> Reachable(Cell start, HashSet<Cell> blocked)
        {
            var seen = new HashSet<Cell>();
            if (!Walkable(start) || blocked.Contains(start)) return seen;
            var queue = new Queue<Cell>(); queue.Enqueue(start); seen.Add(start);
            while (queue.Count > 0)
            {
                Cell c = queue.Dequeue();
                foreach (Cell d in Directions)
                {
                    Cell next = new Cell(c.x + d.x, c.y + d.y);
                    if (Walkable(next) && !blocked.Contains(next) && seen.Add(next)) queue.Enqueue(next);
                }
            }
            return seen;
        }

        // Excludes start; empty means already there, null means no route.
        public List<Cell> Path(Cell start, Cell end, HashSet<Cell> blocked)
        {
            if (!Walkable(start) || !Walkable(end) || blocked.Contains(start) || blocked.Contains(end)) return null;
            var parents = new Dictionary<Cell, Cell>(); var queue = new Queue<Cell>();
            parents[start] = start; queue.Enqueue(start);
            while (queue.Count > 0)
            {
                Cell c = queue.Dequeue();
                if (c == end)
                {
                    var result = new List<Cell>();
                    while (c != start) { result.Add(c); c = parents[c]; }
                    result.Reverse(); return result;
                }
                foreach (Cell d in Directions)
                {
                    Cell next = new Cell(c.x + d.x, c.y + d.y);
                    if (Walkable(next) && !blocked.Contains(next) && !parents.ContainsKey(next))
                    { parents[next] = c; queue.Enqueue(next); }
                }
            }
            return null;
        }
    }
}
