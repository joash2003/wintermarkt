using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Wintermarkt.Core;

namespace Wintermarkt
{
    /// <summary>Original geometric art. Park, streets and buildings come from site.json (tools/build_site.py), an approximate blockout, not surveyed geometry.</summary>
    public sealed class MarketWorld : MonoBehaviour
    {
        static readonly Color Wood = new Color(.42f, .27f, .15f), Snow = new Color(.8f, .8f, .8f), WarmLight = new Color(1f, .78f, .42f), CaptionColor = new Color(.93f, .9f, .8f);
        readonly Dictionary<(Color, float), Material> materials = new Dictionary<(Color, float), Material>();
        readonly Dictionary<Color, Material> glows = new Dictionary<Color, Material>();
        readonly Dictionary<int, Transform> buildings = new Dictionary<int, Transform>();
        readonly Dictionary<int, int> levels = new Dictionary<int, int>();
        readonly Dictionary<int, Transform> people = new Dictionary<int, Transform>();
        readonly Dictionary<int, Transform> marks = new Dictionary<int, Transform>();
        readonly Dictionary<int, Transform> signals = new Dictionary<int, Transform>();
        readonly Dictionary<int, Transform> beasts = new Dictionary<int, Transform>();
        readonly Dictionary<int, int> beastHits = new Dictionary<int, int>();
        readonly Dictionary<int, int> signalState = new Dictionary<int, int>();
        readonly Dictionary<int, int> shown = new Dictionary<int, int>();
        /// <summary>One colour per building category: the stall sign, the wish markers above visitors and the build menu all use it.</summary>
        public static Color CategoryColor(string category)
        {
            switch (category)
            {
                case "drinks": return new Color(.9f, .2f, .22f);
                case "food": return new Color(1f, .62f, .12f);
                case "crafts": return new Color(.25f, .55f, 1f);
                case "rides": return new Color(.3f, .85f, .45f);
                case "landmark": return new Color(.78f, .45f, .95f);
                default: return new Color(.7f, .7f, .7f);
            }
        }
        readonly List<Transform> movingRides = new List<Transform>();
        readonly List<AmbientVehicle> traffic = new List<AmbientVehicle>();
        Transform content, preview, snow;
        public MarketSimulation Simulation { get; private set; }
        public Vector3 World(Cell c) { return new Vector3((c.x - (Simulation.Site.Width - 1) * .5f) * CellSize, 0, ((Simulation.Site.Height - 1) * .5f - c.y) * CellSize); }
        public float CellSize { get { return Simulation.Site.Data.metersPerCell; } }
        public Cell Grid(Vector3 p) { return new Cell(Mathf.RoundToInt(p.x / CellSize + (Simulation.Site.Width - 1) * .5f), Mathf.RoundToInt((Simulation.Site.Height - 1) * .5f - p.z / CellSize)); }

        Material Material(Color c, float gloss = .12f)
        {
            Material m; if (!materials.TryGetValue((c, gloss), out m))
            {
                // A Resources material ships its shader in builds; Shader.Find("Standard") returns null there.
                m = new Material(Resources.Load<Material>("Wintermarkt/Blockout")); m.color = c; m.SetFloat("_Glossiness", gloss); materials[(c, gloss)] = m;
            }
            return m;
        }
        Material Glow(Color c)
        {
            Material m; if (!glows.TryGetValue(c, out m)) { m = new Material(Resources.Load<Material>("Wintermarkt/Glow")); m.color = c * .2f; m.SetColor("_EmissionColor", c); glows[c] = m; }
            return m;
        }
        Transform Shape(string title, Transform parent, PrimitiveType shape, Vector3 position, Vector3 size, Material material)
        {
            var obj = GameObject.CreatePrimitive(shape); obj.name = title; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            var collider = obj.GetComponent<Collider>(); if (collider) Destroy(collider);
            return obj.transform;
        }
        Transform Shape(string title, Transform parent, PrimitiveType shape, Vector3 position, Vector3 size, Color color) { return Shape(title, parent, shape, position, size, Material(color)); }
        Transform Box(string name, Transform parent, Vector3 pos, Vector3 size, Color c) { return Shape(name, parent, PrimitiveType.Cube, pos, size, c); }
        Transform Lamp(string name, Transform parent, Vector3 pos, Vector3 size) { return Shape(name, parent, PrimitiveType.Cube, pos, size, Glow(WarmLight)); }
        Transform Segment(string name, Vector3 a, Vector3 b, float width, float y, float height, Material m)
        {
            var t = Shape(name, content, PrimitiveType.Cube, (a + b) / 2 + Vector3.up * (y + height / 2), new Vector3(width, height, Vector3.Distance(a, b)), m);
            t.rotation = Quaternion.LookRotation(b - a); return t;
        }
        void Caption(string title, Transform parent, Vector3 pos, float size = 1.4f)
        {
            var obj = new GameObject(title); obj.transform.SetParent(parent, false); obj.transform.position = pos;
            obj.transform.rotation = Quaternion.Euler(55, 30, 0);
            var text = obj.AddComponent<TextMesh>(); text.text = title; text.fontSize = 40; text.characterSize = size; text.anchor = TextAnchor.MiddleCenter; text.color = CaptionColor;
        }
        public void Initialize(MarketSimulation sim)
        {
            Simulation = sim;
            if (content) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            buildings.Clear(); levels.Clear(); people.Clear(); marks.Clear(); shown.Clear(); signals.Clear(); signalState.Clear(); beasts.Clear(); beastHits.Clear(); movingRides.Clear(); traffic.Clear();
            content = new GameObject("Volksgarten").transform; content.SetParent(transform);
            Dusk();
            var site = sim.Site; float width = site.Width * CellSize, height = site.Height * CellSize;
            Box("City ground", content, new Vector3(0, -.55f, 0), new Vector3(width + 400, 1, height + 400), new Color(.21f, .22f, .26f));
            SiteFeature park = null; foreach (var f in site.Data.features ?? new SiteFeature[0]) if (f.kind == "park") park = f;
            if (park != null)
            {
                var edge = Ground(park, "Snowy lawn", .08f, 1.02f, Snow); var kerb = Material(new Color(.42f, .42f, .46f));
                for (int i = 0; i < edge.Length; i++) Segment("Park kerb", edge[i] - Vector3.up * .08f, edge[(i + 1) % edge.Length] - Vector3.up * .08f, .8f, 0, .4f, kerb);
            }
            for (int y = 0; y < site.Height; y++)
            {
                // Each row is drawn as runs of equal ground, so the full-size park stays a few hundred objects.
                for (int x = 1, start = 0; park == null && x <= site.Width; x++)
                {
                    char ground = Ground(site.Data.rows[y][start]);
                    if (x < site.Width && Ground(site.Data.rows[y][x]) == ground) continue;
                    Vector3 a = World(new Cell(start, y)), b = World(new Cell(x - 1, y));
                    if (ground == 'p') Shape("Wet path", content, PrimitiveType.Cube, (a + b) / 2 + Vector3.up * .06f, new Vector3(b.x - a.x + CellSize, .12f, CellSize), Material(new Color(.4f, .4f, .43f), .6f));
                    else if (ground == '.') Box("Lawn", content, (a + b) / 2 + Vector3.up * .02f, new Vector3(b.x - a.x + CellSize, .1f, CellSize), new Color(.25f, .35f, .26f));
                    start = x;
                }
                for (int x = 0; x < site.Width; x++) if (site.Data.rows[y][x] == 'T') Tree(World(new Cell(x, y)));
            }
            foreach (var f in site.Data.features ?? new SiteFeature[0])
            {
                var pts = new List<Vector3>(); for (int i = 0; i + 1 < f.points.Length; i += 2) pts.Add(new Vector3(f.points[i], 0, f.points[i + 1]));
                if (f.kind == "street") Street(f, pts);
                else if (f.kind == "plot") Plot(f);
                else if (f.kind == "tree") foreach (var p in pts) Tree(p);
                else if (f.kind == "path") Strip("Path", pts, f.width, .1f, .08f, Material(new Color(.42f, .34f, .27f), .4f));
                else if (f.kind == "building") Building(f, pts[0], pts[1]);
                else if (f.kind == "label") Caption(f.name, content, pts[0] + Vector3.up * 3, 1.1f);
                else if (f.kind == "tram") traffic.Add(new AmbientVehicle(Tram(), pts, f.stopAt, 9, 6));
                else if (f.kind == "bus") traffic.Add(new AmbientVehicle(Bus(), pts, f.stopAt, 10, 5));
            }
            preview = new GameObject("Placement preview").transform; preview.SetParent(content);
        }
        static char Ground(char tile) { return tile == 'p' || tile == 'E' ? 'p' : tile == '#' ? '#' : '.'; }
        void Dusk()
        {
            // Evening market from the reference photos: dim blue sky, the warm lamps carry the scene.
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.36f, .38f, .48f);
            if (Camera.main) { Camera.main.clearFlags = CameraClearFlags.SolidColor; Camera.main.backgroundColor = new Color(.1f, .13f, .22f); }
            var moon = FindFirstObjectByType<Light>(); if (moon) { moon.color = new Color(.8f, .85f, 1f); moon.intensity = .8f; moon.shadows = LightShadows.Soft; moon.shadowStrength = .55f; }
            QualitySettings.shadowDistance = 600; QualitySettings.pixelLightCount = 8;
        }
        void Street(SiteFeature f, List<Vector3> pts)
        {
            Strip("Street " + f.name, pts, f.width, -.06f, .1f, Material(new Color(.12f, .13f, .16f), .5f));
            if (!string.IsNullOrEmpty(f.name)) Caption(f.name.ToUpperInvariant(), content, (pts[0] + pts[1]) / 2 + Vector3.up * 2, .5f);
        }
        void Strip(string title, List<Vector3> pts, float width, float y, float height, Material m)
        {
            for (int i = 1; i < pts.Count; i++) Segment(title, pts[i - 1], pts[i], width, y, height, m);
            for (int i = 1; i < pts.Count - 1; i++) Shape(title + " corner", content, PrimitiveType.Cylinder, pts[i] + Vector3.up * (y + height / 2), new Vector3(width, height / 2, width), m);
        }
        // A site polygon as one smooth mesh (a fan from the centre: the outlines are close to convex). Returns the drawn outline.
        Vector3[] Ground(SiteFeature f, string title, float y, float scale, Color color)
        {
            int n = f.points.Length / 2; var v = new Vector3[n + 1]; var triangles = new int[n * 3]; float area = 0;
            for (int i = 0; i < n; i++) { v[i] = new Vector3(f.points[i * 2], y, f.points[i * 2 + 1]); v[n] += v[i] / n; }
            // Grid cells belong to an area by their centre, so the drawn ground reaches half a cell further out.
            for (int i = 0; i < n; i++) { v[i] = v[n] + (v[i] - v[n]) * scale; }
            for (int i = 0; i < n; i++) { Vector3 a = v[i], b = v[(i + 1) % n]; area += a.x * b.z - b.x * a.z; }
            for (int i = 0; i < n; i++) { int j = (i + 1) % n; triangles[i * 3] = n; triangles[i * 3 + 1] = area > 0 ? j : i; triangles[i * 3 + 2] = area > 0 ? i : j; }
            var mesh = new Mesh { vertices = v, triangles = triangles }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var ground = new GameObject(title, typeof(MeshFilter), typeof(MeshRenderer)); ground.transform.SetParent(content, false);
            ground.GetComponent<MeshFilter>().sharedMesh = mesh; ground.GetComponent<MeshRenderer>().sharedMaterial = Material(color);
            var outline = new Vector3[n]; System.Array.Copy(v, outline, n); return outline;
        }
        // The playable market: trodden snow, with a string of lights on posts along its open edge (f.width is the plot radius).
        void Plot(SiteFeature f)
        {
            var edge = Ground(f, "Market ground", .1f, 1.03f, new Color(.7f, .66f, .6f)); Vector3 gate = World(Simulation.Site.Entrance);
            var post = Material(new Color(.2f, .15f, .11f)); int last = -1;
            for (int i = 0; i < edge.Length; i++)
            {
                if (Mathf.Abs(Vector3.Distance(new Vector3(f.points[i * 2], 0, f.points[i * 2 + 1]), gate) - f.width) > 3) continue;
                Shape("Light post", content, PrimitiveType.Cylinder, edge[i] + Vector3.up * 1.7f, new Vector3(.3f, 1.8f, .3f), post);
                if (last >= 0 && last == i - 1) Segment("String of lights", edge[last] + Vector3.up * 3.4f, edge[i] + Vector3.up * 3.4f, .14f, 0, .14f, Glow(WarmLight));
                last = i;
            }
        }
        static readonly Color[] Plaster = { new Color(.56f, .47f, .38f), new Color(.5f, .43f, .42f), new Color(.45f, .47f, .52f), new Color(.6f, .53f, .42f) };
        void Building(SiteFeature f, Vector3 a, Vector3 b)
        {
            Color wall = f.name == "Musiktheater" ? new Color(.7f, .68f, .64f) : Plaster[(int)(Mathf.Abs(a.x) + Mathf.Abs(a.z)) % Plaster.Length];
            Segment("Building " + f.name, a, b, f.width, 0, f.height, Material(wall));
            Segment("Roof", a, b, f.width + 1, f.height, .6f, Material(new Color(.36f, .38f, .44f)));
            // Lit windows on both long sides; the pattern is deterministic so every session looks the same.
            Vector3 along = (b - a).normalized, across = Vector3.Cross(Vector3.up, along); float length = Vector3.Distance(a, b);
            for (float s = 3; s < length - 2; s += 5)
                for (float h = 3; h < f.height - 2; h += 4)
                    for (int side = -1; side <= 1; side += 2)
                    {
                        if ((int)(s * 7 + h * 13 + side * 5 + Mathf.Abs(a.x)) % 3 == 0) continue;
                        Lamp("Lit window", content, a + along * s + across * side * (f.width / 2 + .05f) + Vector3.up * h, new Vector3(.1f, 2, 2.4f)).rotation = Quaternion.LookRotation(along);
                    }
        }
        void Tree(Vector3 p)
        {
            float s = .85f + Mathf.Repeat(p.x * 3.1f + p.z * 1.7f, 1) * .5f;
            Shape("Trunk", content, PrimitiveType.Cylinder, p + Vector3.up * 2.5f * s, new Vector3(.7f, 2.5f * s, .7f), new Color(.2f, .15f, .11f));
            Shape("Crown", content, PrimitiveType.Sphere, p + Vector3.up * 7 * s, new Vector3(7, 5.5f, 7) * s, new Color(.13f, .25f, .19f));
            Shape("Snow cap", content, PrimitiveType.Sphere, p + Vector3.up * 8.6f * s, new Vector3(4.6f, 3, 4.6f) * s, Snow);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 2.4f + p.x;
                Lamp("Fairy light", content, p + new Vector3(Mathf.Cos(angle) * 3.2f * s, (5.4f + i % 3 * 1.2f) * s, Mathf.Sin(angle) * 3.2f * s), Vector3.one * .6f);
            }
        }
        Transform Tram()
        {
            var tram = new GameObject("Ambient tram").transform; tram.SetParent(content);
            for (int i = -1; i <= 1; i++)
            {
                Box("Articulated body", tram, new Vector3(0, 2, i * 6.2f), new Vector3(2.7f, 3.2f, 6), new Color(.92f, .93f, .89f));
                Lamp("Lit window band", tram, new Vector3(0, 2.5f, i * 6.2f), new Vector3(2.8f, 1.4f, 5.5f));
                Box("Red lower band", tram, new Vector3(0, .9f, i * 6.2f), new Vector3(2.8f, .6f, 5.8f), new Color(.77f, .13f, .16f));
            }
            return tram;
        }
        Transform Bus()
        {
            var bus = new GameObject("Ambient bus").transform; bus.SetParent(content);
            Box("Bus body", bus, new Vector3(0, 2, 0), new Vector3(2.8f, 3.2f, 10), new Color(.9f, .91f, .87f));
            Lamp("Lit windows", bus, new Vector3(0, 2.5f, 0), new Vector3(2.9f, 1.3f, 8.7f));
            Box("Bus livery", bus, new Vector3(0, 1, 0), new Vector3(2.9f, .7f, 9.8f), new Color(.77f, .13f, .16f));
            return bus;
        }
        public void Preview(BuildingDefinition definition, Cell origin, bool valid)
        {
            for (int i = preview.childCount - 1; i >= 0; i--) { preview.GetChild(i).gameObject.SetActive(false); Destroy(preview.GetChild(i).gameObject); }
            if (definition == null) return;
            var p = World(origin); Color c = valid ? new Color(.3f, .82f, .55f) : new Color(.95f, .26f, .24f);
            Box("Footprint", preview, p + new Vector3((definition.width - 1) * CellSize / 2, .18f, -(definition.depth - 1) * CellSize / 2), new Vector3(definition.width * CellSize - .2f, .2f, definition.depth * CellSize - .2f), c);
            for (int i = 0; i < definition.queueSlots; i++)
                Box("Queue reservation", preview, World(new Cell(origin.x + i % definition.width, origin.y + definition.depth + i / definition.width)) + Vector3.up * .2f, new Vector3(CellSize - .4f, .2f, CellSize - .4f), c * .8f);
        }
        public void Synchronize(float dt)
        {
            foreach (var b in Simulation.Buildings)
            {
                if (!buildings.ContainsKey(b.Id) || levels[b.Id] != b.Level)
                {
                    if (buildings.ContainsKey(b.Id)) { buildings[b.Id].gameObject.SetActive(false); Destroy(buildings[b.Id].gameObject); }
                    buildings[b.Id] = BuildShop(b); levels[b.Id] = b.Level; signals.Remove(b.Id); signalState.Remove(b.Id);
                }
                Signal(b);
            }
            if (buildings.Count != Simulation.Buildings.Count)
            {
                var standing = new HashSet<int>(); foreach (var b in Simulation.Buildings) standing.Add(b.Id);
                foreach (int id in new List<int>(buildings.Keys)) if (!standing.Contains(id)) { movingRides.Remove(buildings[id].Find("Ride motion")); Destroy(buildings[id].gameObject); buildings.Remove(id); levels.Remove(id); signals.Remove(id); signalState.Remove(id); }
            }
            var active = new HashSet<int>();
            foreach (var v in Simulation.Visitors)
            {
                active.Add(v.Id); Transform person;
                if (!people.TryGetValue(v.Id, out person))
                {
                    person = new GameObject("Visitor " + v.Id).transform; person.SetParent(content); people[v.Id] = person;
                    Color coat = Color.HSVToRGB((v.Id * .137f) % 1, .6f, .7f);
                    Shape("Coat", person, PrimitiveType.Capsule, Vector3.up * 1.1f, new Vector3(.75f, .8f, .75f), coat);
                    Shape("Hat", person, PrimitiveType.Sphere, Vector3.up * 2.05f, Vector3.one * .65f, new Color(.95f, .76f, .52f));
                }
                person.position = Vector3.Lerp(World(v.Position), World(v.NextCell), v.EdgeProgress);
                if (v.NextCell != v.Position) person.rotation = Quaternion.LookRotation(World(v.NextCell) - World(v.Position));
                person.localScale = Vector3.one * (v.State == VisitorState.Serving ? 1.5f : 1.4f);
                // Above the head: one lit cube per remaining wish in its category colour, then a star (happy) or a red block (gave up).
                int signature = v.Wishes.Count + 10 * (int)v.Mood, old; Transform mark;
                if (!shown.TryGetValue(v.Id, out old) || old != signature)
                {
                    if (marks.TryGetValue(v.Id, out mark)) Destroy(mark.gameObject);
                    mark = new GameObject("Wishes").transform; mark.SetParent(person, false); mark.localPosition = Vector3.up * 3.4f; marks[v.Id] = mark; shown[v.Id] = signature;
                    if (v.Mood == VisitorMood.Happy) Shape("Happy", mark, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.1f, Glow(new Color(1, .82f, .2f)));
                    else if (v.Mood == VisitorMood.Scared) Shape("Scared", mark, PrimitiveType.Cube, Vector3.zero, new Vector3(.4f, 1.3f, .4f), Glow(new Color(.6f, .8f, 1f)));
                    else if (v.Mood == VisitorMood.GaveUp) Shape("Gave up", mark, PrimitiveType.Cube, Vector3.zero, new Vector3(1.2f, .35f, .35f), Glow(new Color(1, .1f, .1f)));
                    else for (int i = 0; i < v.Wishes.Count; i++) Shape("Wish " + v.Wishes[i], mark, PrimitiveType.Cube, new Vector3((i - (v.Wishes.Count - 1) * .5f) * .95f, 0, 0), Vector3.one * .8f, Glow(CategoryColor(v.Wishes[i])));
                }
                marks[v.Id].rotation = Quaternion.Euler(0, 30, 0);   // the row stays level on screen, the camera looks along yaw 30
            }
            var removed = new List<int>(); foreach (var pair in people) if (!active.Contains(pair.Key)) { Destroy(pair.Value.gameObject); removed.Add(pair.Key); }
            foreach (int id in removed) { people.Remove(id); marks.Remove(id); shown.Remove(id); }
            for (int i = movingRides.Count - 1; i >= 0; i--) { if (!movingRides[i]) { movingRides.RemoveAt(i); continue; } movingRides[i].Rotate(Vector3.up, 20 * dt, Space.Self); }
            foreach (var vehicle in traffic) vehicle.Tick(dt);
            Beasts();
            Snowfall(Simulation.ActiveEvent != null && Simulation.ActiveEvent.kind == "storm", dt);
        }
        // The Krampus of the finale: a dark horned figure with glowing eyes; each snowball that hit it shows as a white ball above its head.
        void Beasts()
        {
            var present = new HashSet<int>();
            foreach (var k in Simulation.Krampuses)
            {
                present.Add(k.Id); Transform beast; int hits;
                if (!beasts.TryGetValue(k.Id, out beast))
                {
                    beast = new GameObject("Krampus " + k.Id).transform; beast.SetParent(content); beast.localScale = Vector3.one * 1.7f; beasts[k.Id] = beast;
                    Color fur = new Color(.13f, .09f, .08f), horn = new Color(.82f, .76f, .6f);
                    Shape("Body", beast, PrimitiveType.Capsule, Vector3.up * 1.5f, new Vector3(1.5f, 1.5f, 1.5f), fur);
                    Shape("Head", beast, PrimitiveType.Sphere, Vector3.up * 3.3f, Vector3.one * 1.25f, fur);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Shape("Horn", beast, PrimitiveType.Cylinder, new Vector3(side * .55f, 4.2f, 0), new Vector3(.22f, .65f, .22f), horn).localRotation = Quaternion.Euler(0, 0, -side * 28);
                        Shape("Eye", beast, PrimitiveType.Sphere, new Vector3(side * .28f, 3.4f, .55f), Vector3.one * .28f, Glow(new Color(1, .1f, .05f)));
                    }
                    Shape("Tongue", beast, PrimitiveType.Cube, new Vector3(0, 2.9f, .6f), new Vector3(.3f, .6f, .15f), Glow(new Color(.9f, .1f, .15f)));
                }
                if (!beastHits.TryGetValue(k.Id, out hits) || hits != k.Hits)
                {
                    var old = beast.Find("Snow"); if (old) Destroy(old.gameObject);
                    var snowballs = new GameObject("Snow").transform; snowballs.SetParent(beast, false); beastHits[k.Id] = k.Hits;
                    for (int i = 0; i < k.Hits; i++) Shape("Snowball", snowballs, PrimitiveType.Sphere, new Vector3((i - (k.Hits - 1) * .5f) * .8f, 5.4f, 0), Vector3.one * .7f, Glow(Color.white));
                }
                beast.position = Vector3.Lerp(World(k.Position), World(k.NextCell), k.EdgeProgress);
                if (k.NextCell != k.Position) beast.rotation = Quaternion.LookRotation(World(k.NextCell) - World(k.Position));
            }
            foreach (int id in new List<int>(beasts.Keys)) if (!present.Contains(id)) { Destroy(beasts[id].gameObject); beasts.Remove(id); beastHits.Remove(id); }
        }
        // The snowstorm event: plain falling cubes, because a particle shader may be missing from a build (see issue 8).
        void Snowfall(bool storm, float dt)
        {
            if (!storm) { if (snow) { Destroy(snow.gameObject); snow = null; } return; }
            if (!snow)
            {
                snow = new GameObject("Snowstorm").transform; snow.SetParent(content);
                for (int i = 0; i < 420; i++)
                    Shape("Flake", snow, PrimitiveType.Cube, new Vector3(Mathf.Repeat(i * 61.8f, 200) - 100, Mathf.Repeat(i * 17.3f, 50), Mathf.Repeat(i * 37.1f, 200) - 100), Vector3.one * .55f, Glow(new Color(.9f, .93f, 1f)));
            }
            foreach (Transform flake in snow)
            {
                Vector3 p = flake.localPosition + new Vector3(9, -16, 3) * dt; if (p.y < 0) { p.y += 50; p.x -= 28; p.z -= 9; }
                flake.localPosition = p;
            }
        }
        // Above the roof: a pulsing yellow ball with a shrinking red ring while the stall calls for help, a dark slab while it is closed, a green light while boosted.
        void Signal(MarketBuilding b)
        {
            int state = b.NeedsHelp ? 1 : b.ClosedRemaining > 0 ? 2 : b.BoostRemaining > 0 ? 3 : 0, old; Transform signal;
            if (!signalState.TryGetValue(b.Id, out old) || old != state)
            {
                if (signals.TryGetValue(b.Id, out signal) && signal) Destroy(signal.gameObject);
                signal = new GameObject("Signal").transform; signal.SetParent(buildings[b.Id], false); signal.localPosition = Vector3.up * (b.Definition.id == "wheel" ? 21 : 9.5f);
                signals[b.Id] = signal; signalState[b.Id] = state;
                if (state == 1)
                {
                    Shape("Call", signal, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 2.6f, Glow(new Color(1, .85f, .1f)));
                    Shape("Ring", signal, PrimitiveType.Cylinder, Vector3.down * 2, new Vector3(5, .12f, 5), Glow(new Color(1, .2f, .1f)));
                }
                else if (state == 2) { Box("Closed", signal, Vector3.zero, new Vector3(4, .8f, 1.2f), new Color(.12f, .08f, .08f)); Caption("CLOSED", signal, signal.position + Vector3.up * 1.4f, .45f); }
                else if (state == 3) Shape("Boost", signal, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 1.3f, Glow(new Color(.3f, 1, .5f)));
            }
            if (state != 1) return;
            signal = signals[b.Id]; float left = b.HelpRemaining / Simulation.Settings.helpWindowSeconds;
            signal.GetChild(0).localScale = Vector3.one * (2.6f + .5f * Mathf.Sin(Time.unscaledTime * 9));
            signal.GetChild(1).localScale = new Vector3(5 * left, .12f, 5 * left);
        }
        Transform BuildShop(MarketBuilding b)
        {
            var root = new GameObject(b.Definition.title + " level " + (b.Level + 1)).transform; root.SetParent(content); root.position = World(b.Origin) + new Vector3((b.Definition.width - 1) * CellSize / 2, 0, -(b.Definition.depth - 1) * CellSize / 2);
            float w = b.Definition.width * CellSize - .5f, d = b.Definition.depth * CellSize - .5f;
            Color sign = CategoryColor(b.Definition.category);
            Box("Wooden hut", root, new Vector3(0, 1.8f, 0), new Vector3(w, 3.6f, d), Wood);
            // Pitched roof with the ridge along the front; light strips trace the eaves and ridge like the real huts.
            float pitch = 28.6f, rise = d / 2 * Mathf.Tan(pitch * Mathf.Deg2Rad);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Roof", root, new Vector3(0, 3.6f + rise / 2, side * d / 4), new Vector3(w + .8f, .25f, d / 2 / Mathf.Cos(pitch * Mathf.Deg2Rad) + .6f), Snow).localRotation = Quaternion.Euler(side * pitch, 0, 0);
                Lamp("Eave lights", root, new Vector3(0, 3.55f, side * (d / 2 + .3f)), new Vector3(w + .8f, .15f, .15f));
            }
            Lamp("Ridge lights", root, new Vector3(0, 3.75f + rise, 0), new Vector3(w + .8f, .15f, .15f));
            Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, 3.2f, -d / 2 - .3f), new Vector3(w * .8f, .7f, .2f), Glow(sign));
            Shape("Roof flag", root, PrimitiveType.Cube, new Vector3(0, 4.6f + rise, 0), Vector3.one * 1.1f, Glow(sign));
            // A warm pool of light on the snow in front of the counter.
            var pool = new GameObject("Counter light", typeof(Light)).GetComponent<Light>(); pool.transform.SetParent(root, false); pool.transform.localPosition = new Vector3(0, 3, -d / 2 - 3);
            pool.type = LightType.Point; pool.color = WarmLight; pool.range = 16; pool.intensity = 1.4f;
            Lamp("Lit counter", root, new Vector3(0, 2.1f, -d / 2 - .05f), new Vector3(w * .7f, 1.5f, .12f));
            if (b.Definition.id == "carousel" || b.Definition.id == "wheel")
            {
                var ride = new GameObject("Ride motion").transform; ride.SetParent(root, false); ride.localPosition = Vector3.up * 5;
                if (b.Definition.id == "carousel")
                {
                    Shape("Carousel canopy", ride, PrimitiveType.Cylinder, Vector3.up * 2, new Vector3(w * .9f, .4f, w * .9f), sign);
                    for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3; Lamp("Ride seat", ride, new Vector3(Mathf.Cos(a) * 3, 0, Mathf.Sin(a) * 3), new Vector3(1, 1.2f, 1)); }
                }
                else
                {
                    // Local Y rotation becomes world Z rotation for this wheel plane.
                    ride.localRotation = Quaternion.Euler(90, 0, 0); ride.localPosition = Vector3.up * 12;
                    for (int i = 0; i < 12; i++) { float a = i * Mathf.PI / 6; var spoke = Lamp("Lit spoke", ride, Vector3.zero, new Vector3(.18f, .18f, 12)); spoke.localRotation = Quaternion.Euler(0, i * 30, 0); Shape("Cabin", ride, PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 6, 0, Mathf.Cos(a) * 6), new Vector3(1.7f, 1.3f, 1.7f), sign); }
                    Box("Wheel support", root, Vector3.up * 7, new Vector3(.8f, 14, .8f), new Color(.8f, .67f, .34f));
                }
                movingRides.Add(ride);
            }
            for (int i = 0; i <= b.Level; i++) Shape("Upgrade star", root, PrimitiveType.Sphere, new Vector3(-w / 3 + i * 1.3f, 4.9f + rise, -d / 4), Vector3.one * .65f, Glow(new Color(1, .77f, .2f)));
            Caption(b.Definition.title + "  " + (b.Level + 1), root, root.position + new Vector3(0, 6 + rise, -d / 2 - 1), .38f);
            for (int i = 0; i < b.Definition.queueSlots; i++)
            {
                Vector3 marker = root.InverseTransformPoint(World(b.QueueCell(i)) + Vector3.up * .08f);
                Box("Queue marker", root, marker, new Vector3(1.1f, .1f, 1.1f), new Color(.75f, .64f, .4f));
            }
            return root;
        }
        void OnDestroy() { foreach (var m in materials.Values) if (m) Destroy(m); foreach (var m in glows.Values) if (m) Destroy(m); }

        sealed class AmbientVehicle
        {
            readonly Transform transform; readonly List<Vector3> route; readonly float length, stopAt, speed, dwell;
            float distance, waiting; bool stopped;
            public AmbientVehicle(Transform t, List<Vector3> points, float stopFraction, float speed, float dwell)
            {
                transform = t; route = points; this.speed = speed; this.dwell = dwell;
                for (int i = 1; i < route.Count; i++) length += Vector3.Distance(route[i - 1], route[i]);
                stopAt = stopFraction * length; Place();
            }
            public void Tick(float dt)
            {
                if (waiting > 0) { waiting -= dt; return; }
                distance += dt * speed;
                if (!stopped && distance >= stopAt) { distance = stopAt; stopped = true; waiting = dwell; }
                if (distance >= length) { distance = 0; stopped = false; waiting = 10; }
                Place();
            }
            void Place()
            {
                float d = distance;
                for (int i = 1; i < route.Count; i++)
                {
                    float segment = Vector3.Distance(route[i - 1], route[i]);
                    if (d > segment && i < route.Count - 1) { d -= segment; continue; }
                    transform.position = Vector3.Lerp(route[i - 1], route[i], Mathf.Clamp01(d / segment));
                    transform.rotation = Quaternion.LookRotation(route[i] - route[i - 1]); return;
                }
            }
        }
    }
}
