using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
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
        readonly Dictionary<int, Transform> signals = new Dictionary<int, Transform>();
        readonly Dictionary<int, Transform> beasts = new Dictionary<int, Transform>();
        readonly Dictionary<int, int> beastHits = new Dictionary<int, int>();
        readonly Dictionary<int, int> signalState = new Dictionary<int, int>();
        // An upgraded stall pops up from nothing (issue 44): its new model and the seconds since the upgrade.
        readonly Dictionary<Transform, float> popping = new Dictionary<Transform, float>();
        /// <summary>One colour per building category: the stall sign and the build menu both use it.</summary>
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
        readonly List<Transform> gondolas = new List<Transform>();   // wheel cabins, kept upright while their wheel turns
        readonly List<AmbientVehicle> traffic = new List<AmbientVehicle>();
        Transform content, preview, snow;
        readonly List<Transform> sparks = new List<Transform>(); readonly List<Vector3> sparkSpeed = new List<Vector3>(); readonly List<float> sparkLife = new List<float>();
        float fireworkClock; int fireworkCount;
        readonly Dictionary<Cell, Transform> mounds = new Dictionary<Cell, Transform>();
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
        readonly Dictionary<Material, Material> kitMaterials = new Dictionary<Material, Material>(); float kitUnit;
        // A model from the CC0 Kenney Holiday Kit (Resources/Wintermarkt/Kit, see ASSET_CREDITS.md), scaled so one kit unit is `unit` metres.
        // Returns null when the model is missing, and the caller draws its primitive version instead.
        Transform Prop(string model, Transform parent, Vector3 localPosition, float unit, float yaw = 0)
        {
            var prefab = Resources.Load<GameObject>("Wintermarkt/Kit/" + model); if (!prefab) return null;
            if (kitUnit == 0)
            {
                // The imported size depends on the FBX unit settings, so measure the one-unit-wide wall once.
                var wall = Resources.Load<GameObject>("Wintermarkt/Kit/cabin-wall"); kitUnit = 1;
                if (wall) { var probe = Instantiate(wall); var r = probe.GetComponentInChildren<Renderer>(); if (r) kitUnit = Mathf.Max(r.bounds.size.x, r.bounds.size.z); probe.SetActive(false); Destroy(probe); }
            }
            var holder = new GameObject(model).transform; holder.SetParent(parent, false);
            holder.localPosition = localPosition; holder.localRotation = Quaternion.Euler(0, yaw, 0); holder.localScale = Vector3.one * unit / kitUnit;
            foreach (var renderer in Instantiate(prefab, holder, false).GetComponentsInChildren<Renderer>())
            {
                // Every model of one kit uses the same colour-map texture: one matte material per kit.
                Material matte; var imported = renderer.sharedMaterial; if (!imported) continue;
                if (!kitMaterials.TryGetValue(imported, out matte)) { matte = new Material(imported); matte.SetFloat("_Glossiness", .08f); kitMaterials[imported] = matte; }
                renderer.sharedMaterial = matte;
            }
            return holder;
        }
        // Visitors are animated characters from Kenney's Mini Characters: the walk and idle clips of the model are mixed by a small playable graph each.
        static readonly string[] People = { "character-female-a", "character-male-a", "character-female-c", "character-male-c", "character-female-e", "character-male-f" };
        struct Walker { public PlayableGraph graph; public AnimationMixerPlayable mixer; public AnimationClipPlayable walk, idle; public float walkLength, idleLength; }
        readonly Dictionary<int, Walker> walkers = new Dictionary<int, Walker>();
        readonly Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
        AnimationClip Clip(string model, string take)
        {
            AnimationClip clip;
            if (clips.TryGetValue(model + "/" + take, out clip)) return clip;
            foreach (var c in Resources.LoadAll<AnimationClip>("Wintermarkt/Kit/" + model)) if (!c.name.StartsWith("__preview") && c.name.EndsWith(take)) clip = c;
            clips[model + "/" + take] = clip; return clip;
        }
        void Animate(int id, Transform body, string model)
        {
            AnimationClip walk = Clip(model, "walk"), idle = Clip(model, "idle");
            if (!walk || !idle || body.childCount == 0) return;
            // The models are imported without an avatar, so they carry no Animator; generic clips only need one on the model's root.
            var animator = body.GetComponentInChildren<Animator>(); if (!animator) animator = body.GetChild(0).gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;
            var w = new Walker { graph = PlayableGraph.Create("Visitor " + id), walkLength = walk.length, idleLength = idle.length };
            w.mixer = AnimationMixerPlayable.Create(w.graph, 2); w.walk = AnimationClipPlayable.Create(w.graph, walk); w.idle = AnimationClipPlayable.Create(w.graph, idle);
            w.graph.Connect(w.walk, 0, w.mixer, 0); w.graph.Connect(w.idle, 0, w.mixer, 1);
            AnimationPlayableOutput.Create(w.graph, "Pose", animator).SetSourcePlayable(w.mixer); w.graph.Play(); walkers[id] = w;
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
            foreach (var w in walkers.Values) if (w.graph.IsValid()) w.graph.Destroy();
            walkers.Clear(); buildings.Clear(); levels.Clear(); people.Clear(); popping.Clear(); signals.Clear(); signalState.Clear(); beasts.Clear(); beastHits.Clear(); sparks.Clear(); sparkSpeed.Clear(); sparkLife.Clear(); mounds.Clear(); movingRides.Clear(); gondolas.Clear(); traffic.Clear();
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
            // The tree nearest the middle of the market is its decorated Christmas tree.
            Cell star = new Cell(-1, -1); float nearest = float.MaxValue;
            for (int y = 0; y < site.Height; y++) for (int x = 0; x < site.Width; x++)
                if (site.Data.rows[y][x] == 'T' && World(new Cell(x, y)).sqrMagnitude < nearest) { nearest = World(new Cell(x, y)).sqrMagnitude; star = new Cell(x, y); }
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
                for (int x = 0; x < site.Width; x++) if (site.Data.rows[y][x] == 'T') Tree(World(new Cell(x, y)), new Cell(x, y) == star);
            }
            foreach (var f in site.Data.features ?? new SiteFeature[0])
            {
                var pts = new List<Vector3>(); for (int i = 0; i + 1 < f.points.Length; i += 2) pts.Add(new Vector3(f.points[i], 0, f.points[i + 1]));
                if (f.kind == "street") Street(f, pts);
                else if (f.kind == "plot") Plot(f);
                else if (f.kind == "tree")
                    for (int i = 0; i < pts.Count; i++)
                    {
                        // Park scenery: firs, with a snowman, a bench or a reindeer beside some of them.
                        Tree(pts[i]);
                        if (i % 5 == 0) Prop(i % 2 == 0 ? "snowman" : "snowman-hat", content, pts[i] + new Vector3(4.5f, 0, 2), 3.2f, i * 70);
                        if (i % 7 == 3) Prop("bench", content, pts[i] + new Vector3(-4.5f, 0, 3), 3.4f, i * 40);
                        if (i % 11 == 5) Prop("reindeer", content, pts[i] + new Vector3(3.5f, 0, -4.5f), 3.2f, i * 55);
                    }
                else if (f.kind == "path")
                {
                    Strip("Path", pts, f.width, .1f, .08f, Material(new Color(.42f, .34f, .27f), .4f));
                    // A lit lantern every 20 m, alternating sides, so the paths read as walks through a park at night.
                    for (int i = 1, n = 0; i < pts.Count; i++)
                    {
                        Vector3 along = (pts[i] - pts[i - 1]).normalized, side = Vector3.Cross(Vector3.up, along);
                        for (float s = 10; s < Vector3.Distance(pts[i - 1], pts[i]); s += 20, n++)
                        {
                            Vector3 p = pts[i - 1] + along * s + side * (n % 2 == 0 ? 1 : -1) * (f.width / 2 + 1.2f);
                            if (Prop("lantern", content, p, 3, n * 90)) Lamp("Lantern glow", content, p + Vector3.up * 4.4f, Vector3.one * .65f);
                        }
                    }
                }
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
            // Patches of fresh snow break up the trodden ground of the market.
            for (int i = 0; i < 46; i++)
            {
                Vector3 p = gate + new Vector3(Mathf.Sin(i * 2.4f), 0, Mathf.Cos(i * 2.4f)) * (12 + i * 2.1f);
                if (Simulation.Site.Walkable(Grid(p))) Prop(i % 3 == 0 ? "snow-flat-large" : "snow-flat", content, p + Vector3.up * .12f, 4.5f + i % 4, i * 47);
            }
            for (int i = 0; i < edge.Length; i++)
            {
                if (Mathf.Abs(Vector3.Distance(new Vector3(f.points[i * 2], 0, f.points[i * 2 + 1]), gate) - f.width) > 3) continue;
                if (Prop("lantern", content, edge[i], 3.2f, i * 90) == null) Shape("Light post", content, PrimitiveType.Cylinder, edge[i] + Vector3.up * 1.7f, new Vector3(.3f, 1.8f, .3f), post);
                else Lamp("Lantern glow", content, edge[i] + Vector3.up * 4.7f, Vector3.one * .7f);
                if (last >= 0 && last == i - 1)
                {
                    // Coloured bulbs from the kit between the posts; the plain warm strip is the fallback.
                    Vector3 span = edge[i] - edge[last];
                    if (!Prop("lights-colored", content, (edge[last] + edge[i]) / 2 + Vector3.up * 3.6f, span.magnitude, Mathf.Atan2(span.x, span.z) * Mathf.Rad2Deg + 90))
                        Segment("String of lights", edge[last] + Vector3.up * 4.2f, edge[i] + Vector3.up * 4.2f, .14f, 0, .14f, Glow(WarmLight));
                }
                last = i;
            }
        }
        // An unnamed city block as a row of town houses from Kenney's City Kit, squashed or stretched to the block's height, with a lit shop front on both sides.
        bool Terrace(SiteFeature f, Vector3 a, Vector3 b)
        {
            Vector3 along = (b - a).normalized, across = Vector3.Cross(Vector3.up, along); float length = Vector3.Distance(a, b);
            int count = Mathf.Max(1, Mathf.RoundToInt(length / f.width)); float step = length / count, unit = Mathf.Min(step, f.width) * 1.08f, yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg + 90;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = a + along * (i + .5f) * step;
                var house = Prop("City/building-" + "abcdefgh"[(i * 3 + (int)Mathf.Abs(a.x)) % 8], content, p, unit, yaw + (i % 2) * 180);
                if (!house) return false;
                house.localScale = Vector3.Scale(house.localScale, new Vector3(1, f.height / (1.29f * unit), 1));
                for (int side = -1; side <= 1; side += 2)
                    Lamp("Shop front", content, p + across * side * (unit * .46f) + Vector3.up * 1.9f, new Vector3(.12f, 2.2f, unit * .45f)).rotation = Quaternion.LookRotation(along);
            }
            return true;
        }
        static readonly Color[] Plaster = { new Color(.56f, .47f, .38f), new Color(.5f, .43f, .42f), new Color(.45f, .47f, .52f), new Color(.6f, .53f, .42f) };
        void Building(SiteFeature f, Vector3 a, Vector3 b)
        {
            if (string.IsNullOrEmpty(f.name) && f.width <= 32 && Terrace(f, a, b)) return;
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
        static readonly string[] Firs = { "tree-snow-a", "tree-snow-b", "tree-snow-c" };
        // A snowy fir from the kit with a few fairy lights; `big` is the decorated Christmas tree of the market.
        void Tree(Vector3 p, bool big = false)
        {
            float s = big ? 2 : .85f + Mathf.Repeat(p.x * 3.1f + p.z * 1.7f, 1) * .5f;
            if (Prop(big ? "tree-decorated-snow" : Firs[(int)Mathf.Repeat(p.x * .7f + p.z * 1.3f, 3)], content, p, 4.6f * s, Mathf.Repeat(p.x * 37 + p.z * 11, 360)) == null)
            {
                Shape("Trunk", content, PrimitiveType.Cylinder, p + Vector3.up * 2.5f * s, new Vector3(.7f, 2.5f * s, .7f), new Color(.2f, .15f, .11f));
                Shape("Crown", content, PrimitiveType.Sphere, p + Vector3.up * 7 * s, new Vector3(7, 5.5f, 7) * s, new Color(.13f, .25f, .19f));
                Shape("Snow cap", content, PrimitiveType.Sphere, p + Vector3.up * 8.6f * s, new Vector3(4.6f, 3, 4.6f) * s, Snow);
            }
            for (int i = 0; i < (big ? 16 : 6); i++)
            {
                float angle = i * 2.4f + p.x, height = (1.6f + i % 4 * 1.5f) * s, radius = (2.5f - i % 4 * .45f) * s;
                Lamp("Fairy light", content, p + new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius), Vector3.one * (big ? .7f : .45f));
            }
            if (!big) return;
            string[] gifts = { "present-a-cube", "present-b-round", "candy-cane-red", "present-a-cube", "nutcracker", "present-b-round", "sled" };
            for (int i = 0; i < gifts.Length; i++) Prop(gifts[i], content, p + new Vector3(Mathf.Cos(i * .9f) * 6.5f, 0, Mathf.Sin(i * .9f) * 6.5f), 3.2f, i * 50);
        }
        Transform Tram()
        {
            var tram = new GameObject("Ambient tram").transform; tram.SetParent(content);
            // Two coupled cars from Kenney's Train Kit, with a strip of lit windows; the box tram is the fallback.
            if (Prop("Train/train-tram-modern", tram, new Vector3(0, 0, 3.5f), 2.5f))
            {
                Prop("Train/train-tram-modern", tram, new Vector3(0, 0, -3.5f), 2.5f, 180);
                for (int car = -1; car <= 1; car += 2) Lamp("Lit windows", tram, new Vector3(0, 2.3f, car * 3.5f), new Vector3(2.86f, .7f, 4.6f));
                return tram;
            }
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
                    bool upgraded = buildings.ContainsKey(b.Id);
                    if (upgraded) { popping.Remove(buildings[b.Id]); buildings[b.Id].gameObject.SetActive(false); Destroy(buildings[b.Id].gameObject); }
                    buildings[b.Id] = BuildShop(b); levels[b.Id] = b.Level; signals.Remove(b.Id); signalState.Remove(b.Id);
                    if (upgraded) { buildings[b.Id].localScale = Vector3.zero; popping[buildings[b.Id]] = 0; Burst(buildings[b.Id].position + Vector3.up * 4); }
                }
                Signal(b);
            }
            Pop(Time.unscaledDeltaTime); TickSparks(Time.unscaledDeltaTime);
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
                    string model = "People/" + People[v.Id % People.Length]; var body = Prop(model, person, Vector3.zero, 3.3f);
                    if (body) { Animate(v.Id, body, model); goto dressed; }
                    Color coat = Color.HSVToRGB((v.Id * .137f) % 1, .6f, .7f);
                    Shape("Coat", person, PrimitiveType.Capsule, Vector3.up * 1.1f, new Vector3(.75f, .8f, .75f), coat);
                    Shape("Head", person, PrimitiveType.Sphere, Vector3.up * 2.05f, Vector3.one * .65f, new Color(.95f, .76f, .52f));
                    Shape("Scarf", person, PrimitiveType.Cylinder, Vector3.up * 1.72f, new Vector3(.66f, .09f, .66f), Color.HSVToRGB((v.Id * .53f) % 1, .75f, .95f));
                    Shape("Beanie", person, PrimitiveType.Sphere, Vector3.up * 2.28f, new Vector3(.7f, .45f, .7f), Color.HSVToRGB((v.Id * .31f) % 1, .7f, .85f));
                    Shape("Pom-pom", person, PrimitiveType.Sphere, Vector3.up * 2.56f, Vector3.one * .24f, Color.white);
                    dressed:;
                }
                Walker walker;
                if (walkers.TryGetValue(v.Id, out walker))
                {
                    // The clips are not imported as looping, so wrap their time here; walking shows while the visitor is on the move.
                    bool moving = v.State == VisitorState.Walking || v.State == VisitorState.Leaving;
                    walker.mixer.SetInputWeight(0, moving ? 1 : 0); walker.mixer.SetInputWeight(1, moving ? 0 : 1);
                    if (walker.walk.GetTime() >= walker.walkLength) walker.walk.SetTime(walker.walk.GetTime() % walker.walkLength);
                    if (walker.idle.GetTime() >= walker.idleLength) walker.idle.SetTime(walker.idle.GetTime() % walker.idleLength);
                }
                person.position = Vector3.Lerp(World(v.Position), World(v.NextCell), v.EdgeProgress);
                if (v.NextCell != v.Position) person.rotation = Quaternion.LookRotation(World(v.NextCell) - World(v.Position));
                person.localScale = Vector3.one * (v.State == VisitorState.Serving ? 1.5f : 1.4f);
            }
            var removed = new List<int>(); foreach (var pair in people) if (!active.Contains(pair.Key)) { Destroy(pair.Value.gameObject); removed.Add(pair.Key); }
            foreach (int id in removed) { people.Remove(id); Walker gone; if (walkers.TryGetValue(id, out gone)) { if (gone.graph.IsValid()) gone.graph.Destroy(); walkers.Remove(id); } }
            for (int i = movingRides.Count - 1; i >= 0; i--) { if (!movingRides[i]) { movingRides.RemoveAt(i); continue; } movingRides[i].Rotate(Vector3.up, 20 * dt, Space.Self); }
            for (int i = gondolas.Count - 1; i >= 0; i--) { if (!gondolas[i]) gondolas.RemoveAt(i); else gondolas[i].rotation = Quaternion.identity; }
            foreach (var vehicle in traffic) vehicle.Tick(dt);
            Beasts();
            // Snow piles on queue spots (issue 28): one white mound per buried cell.
            foreach (Cell pile in Simulation.SnowPiles)
                if (!mounds.ContainsKey(pile))
                {
                    var mound = Prop("snow-pile", content, World(pile) + Vector3.up * .12f, 3.4f, pile.x * 40);
                    if (mound) mound.localScale = Vector3.Scale(mound.localScale, new Vector3(1, 4, 1));
                    mounds[pile] = mound ? mound : Shape("Snow pile", content, PrimitiveType.Sphere, World(pile) + Vector3.up * .5f, new Vector3(3.6f, 1.9f, 3.6f), Material(new Color(.97f, .98f, 1f)));
                }
            if (mounds.Count != Simulation.SnowPiles.Count)
                foreach (Cell pile in new List<Cell>(mounds.Keys)) if (!((ICollection<Cell>)Simulation.SnowPiles).Contains(pile)) { Destroy(mounds[pile].gameObject); mounds.Remove(pile); }
            if (Simulation.Won) Fireworks(Time.unscaledDeltaTime);
            Snowfall(Simulation.ActiveEvent != null && Simulation.ActiveEvent.kind == "storm", dt);
        }
        // New Year's Eve: when the last day ends, rockets burst over the market for as long as the ending card is up.
        void Fireworks(float dt)
        {
            if ((fireworkClock -= dt) <= 0)
            {
                fireworkClock = .45f; fireworkCount++;
                Vector3 centre = World(Simulation.Site.Entrance) * .3f + new Vector3(Mathf.Sin(fireworkCount * 2.4f) * 45, 34 + fireworkCount % 3 * 7, Mathf.Cos(fireworkCount * 1.7f) * 35);
                Color colour = Color.HSVToRGB(Mathf.Repeat(fireworkCount * .23f, 1), .75f, 1);
                for (int i = 0; i < 16; i++)
                {
                    // Directions spread evenly over a sphere with the golden angle.
                    float y = 1 - (i + .5f) / 8f, r = Mathf.Sqrt(1 - y * y), a = i * 2.4f;
                    sparks.Add(Shape("Spark", content, PrimitiveType.Cube, centre, Vector3.one * .9f, Glow(colour))); sparkSpeed.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r) * 16); sparkLife.Add(1.3f);
                }
            }
        }
        // Fireworks and upgrade bursts: every spark flies, falls and shrinks until its life runs out.
        void TickSparks(float dt)
        {
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                sparkLife[i] -= dt; sparkSpeed[i] += Vector3.down * 9 * dt; sparks[i].position += sparkSpeed[i] * dt; sparks[i].localScale = Vector3.one * Mathf.Max(.2f, sparkLife[i] * 1.6f);
                if (sparkLife[i] <= 0) { Destroy(sparks[i].gameObject); sparks.RemoveAt(i); sparkSpeed.RemoveAt(i); sparkLife.RemoveAt(i); }
            }
        }
        // An upgrade: a fountain of gold and white sparks out of the stall, like a building finishing in Clash of Clans.
        void Burst(Vector3 centre)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = i * 2.4f, up = .6f + (i % 4) * .15f;   // golden-angle spread, mostly upwards
                sparks.Add(Shape("Upgrade spark", content, PrimitiveType.Cube, centre, Vector3.one * .6f, Glow(i % 3 == 0 ? Color.white : new Color(1, .8f, .25f))));
                sparkSpeed.Add(new Vector3(Mathf.Cos(a), up * 2, Mathf.Sin(a)) * 6); sparkLife.Add(.9f);
            }
        }
        // The new model grows from nothing, overshoots a little and settles, over 0.6 seconds.
        void Pop(float dt)
        {
            const float length = .6f, back = 1.70158f;
            foreach (var shop in new List<Transform>(popping.Keys))
            {
                if (!shop) { popping.Remove(shop); continue; }
                float t = Mathf.Min(1, (popping[shop] += dt) / length) - 1;
                shop.localScale = Vector3.one * (1 + (back + 1) * t * t * t + back * t * t);   // ease-out-back
                if (t >= 0) popping.Remove(shop);
            }
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
            // Each category is its own building (issue 44, Joash on 18 Nov): the Punsch Hütte is a log cabin from the kit, the bakery a gingerbread house and the
            // crafts stall an open booth. Each has three levels that look like three buildings. `rise` is how far the roof reaches above the 4.6 m flag line.
            // The cabin: serving window with awning at the front, wreath at the back, snowy gabled roof; the kit's proportions are tall, so it is squashed.
            // A low shed, the proper Hütte, then a bigger, taller grand Hütte. Dressing stays at the level-2 spots (e = 5).
            int level = b.Level; const float e = 5;
            float pitch = 28.6f, rise = d / 2 * Mathf.Tan(pitch * Mathf.Deg2Rad), u = 4.5f + .75f * level, squash = level == 0 ? .55f : level == 1 ? .65f : .8f;
            bool carousel = b.Definition.id == "carousel", isRide = carousel || b.Definition.id == "wheel";
            var hut = new GameObject("Cabin").transform; hut.SetParent(root, false); hut.localScale = new Vector3(1, squash, 1);
            if (!isRide && b.Definition.category == "food") rise = Bakery(root, level, sign, w, d);
            else if (!isRide && b.Definition.category == "crafts") rise = Crafts(root, level, sign, w, d);
            else if (!isRide && Prop("cabin-window-large", hut, Vector3.zero, u, 180) != null)
            {
                Prop(level == 0 ? "cabin-wall" : "cabin-wall-wreath", hut, Vector3.zero, u); Prop("cabin-wall", hut, Vector3.zero, u, 90); Prop("cabin-wall", hut, Vector3.zero, u, 270);
                for (int corner = 0; corner < 4; corner++) Prop("cabin-corner", hut, Vector3.zero, u, corner * 90);
                Prop("cabin-wall-roof-center", hut, Vector3.up * u, u); Prop("cabin-wall-roof-center", hut, Vector3.up * u, u, 180);
                Prop("cabin-roof-snow-point", hut, Vector3.up * u, u);
                if (level >= 1)
                {
                    // The bare shed of level 1 has no lights.
                    Prop("lights-colored", hut, new Vector3(0, u * .98f, -u * .68f), u);
                    Prop("lights-colored", hut, new Vector3(-u * .68f, u * .98f, 0), u, 90); Prop("lights-colored", hut, new Vector3(u * .68f, u * .98f, 0), u, 90);
                    Prop("candy-cane-red", root, new Vector3(-e * .75f, 0, -e * .8f), 3.4f, 20); Prop("lantern", root, new Vector3(e * .55f, 0, -e * .8f), 3);
                }
                if (level >= 2)
                {
                    Prop("wreath-decorated", hut, new Vector3(-u * .1f, u * 1.25f, -u * .56f), u * .9f, 180); Prop("present-a-cube", root, new Vector3(e * .8f, 0, -e * .85f), 3.2f, 30); Prop("present-b-round", root, new Vector3(e * .95f, 0, -e * .45f), 2.8f, 70);
                    Prop("nutcracker", root, new Vector3(-e * .85f, 0, -e * .35f), 3.4f, 160); Prop("tree-decorated-snow", root, new Vector3(e * .7f, 0, e * .65f), 4.5f, 40);
                }
                rise = u * squash * 1.44f - .9f;
                Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, u * squash + .75f, -u * .53f), new Vector3(u * (level == 2 ? .62f : .46f), level == 2 ? .8f : .6f, .16f), Glow(sign));
            }
            else if (!isRide)
            {
                Box("Wooden hut", root, new Vector3(0, 1.8f, 0), new Vector3(w, 3.6f, d), Wood);
                // Pitched roof with the ridge along the front; light strips trace the eaves and ridge like the real huts.
                for (int side = -1; side <= 1; side += 2)
                {
                    Box("Roof", root, new Vector3(0, 3.6f + rise / 2, side * d / 4), new Vector3(w + .8f, .25f, d / 2 / Mathf.Cos(pitch * Mathf.Deg2Rad) + .6f), Snow).localRotation = Quaternion.Euler(side * pitch, 0, 0);
                    Lamp("Eave lights", root, new Vector3(0, 3.55f, side * (d / 2 + .3f)), new Vector3(w + .8f, .15f, .15f));
                }
                Lamp("Ridge lights", root, new Vector3(0, 3.75f + rise, 0), new Vector3(w + .8f, .15f, .15f));
                Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, 3.2f, -d / 2 - .3f), new Vector3(w * .8f, .7f, .2f), Glow(sign));
                Lamp("Lit counter", root, new Vector3(0, 2.1f, -d / 2 - .05f), new Vector3(w * .7f, 1.5f, .12f));
            }
            if (!isRide) Shape("Roof flag", root, PrimitiveType.Cube, new Vector3(0, 4.6f + rise, 0), Vector3.one * 1.1f, Glow(sign));
            // A warm pool of light on the snow in front of the counter.
            var pool = new GameObject("Counter light", typeof(Light)).GetComponent<Light>(); pool.transform.SetParent(root, false); pool.transform.localPosition = new Vector3(0, 3, -d / 2 - 3);
            pool.type = LightType.Point; pool.color = WarmLight; pool.range = 16; pool.intensity = 1.4f;
            if (isRide)
            {
                var ride = new GameObject("Ride motion").transform; ride.SetParent(root, false);
                if (carousel) Carousel(ride, level, sign); else Wheel(root, ride, level, sign, w, d);
                movingRides.Add(ride);
            }
            Caption(b.Definition.title + " · Level " + (b.Level + 1), root, root.position + new Vector3(0, 6 + rise, -d / 2 - 1), .38f);
            for (int i = 0; i < b.Definition.queueSlots; i++)
            {
                Vector3 marker = root.InverseTransformPoint(World(b.QueueCell(i)) + Vector3.up * .08f);
                Box("Queue marker", root, marker, new Vector3(1.1f, .1f, 1.1f), new Color(.75f, .64f, .4f));
            }
            return root;
        }
        // The Sweet Bakery is a gingerbread house: warm brown walls with white icing along the eaves and ridge, a chimney with smoke, a red-and-white awning over
        // the counter and trays of pastries. Level 1 is a bakery cart on wheels; 2 the house with lit windows and candy canes; 3 a bigger house with two chimneys,
        // gumdrops along the front eave and a snowman outside. Returns how far the roof reaches above the 4.6 m flag line.
        float Bakery(Transform root, int level, Color sign, float w, float d)
        {
            Color ginger = new Color(.6f, .36f, .18f), dark = new Color(.45f, .25f, .12f), icing = new Color(.97f, .96f, .92f), red = new Color(.8f, .15f, .12f);
            Color[] gumdrops = { new Color(.85f, .15f, .15f), new Color(.2f, .6f, .3f), new Color(1, .8f, .25f) };
            float bw = level == 0 ? 4.2f : level == 1 ? 6 : w, bd = level == 0 ? 2.6f : level == 1 ? 5 : 6, bh = level == 0 ? 2.4f : level == 1 ? 3.4f : 4, lift = level == 0 ? .7f : 0;
            float pitch = 30, rise = bd / 2 * Mathf.Tan(pitch * Mathf.Deg2Rad), eave = lift + bh, front = -bd / 2;
            Box("Walls", root, new Vector3(0, lift + bh / 2, 0), new Vector3(bw, bh, bd), ginger);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Roof", root, new Vector3(0, eave + rise / 2, side * bd / 4), new Vector3(bw + .6f, .22f, bd / 2 / Mathf.Cos(pitch * Mathf.Deg2Rad) + .5f), dark).localRotation = Quaternion.Euler(side * pitch, 0, 0);
                Box("Icing", root, new Vector3(0, eave + .05f, side * (bd / 2 + .25f)), new Vector3(bw + .7f, .3f, .3f), icing);
            }
            Box("Ridge icing", root, new Vector3(0, eave + rise + .1f, 0), new Vector3(bw + .7f, .3f, .35f), icing);
            for (int c = 0; c < (level == 2 ? 2 : 1); c++)
            {
                float cx = level == 2 ? (c == 0 ? -1 : 1) * bw * .3f : bw * .3f;
                Box("Chimney", root, new Vector3(cx, eave + rise * .6f + .6f, bd * .15f), new Vector3(.8f, 1.6f, .8f), dark);
                for (int s = 0; s < 3; s++) Shape("Smoke", root, PrimitiveType.Sphere, new Vector3(cx + s * .25f, eave + rise * .6f + 1.8f + s * .7f, bd * .15f), Vector3.one * (.5f + s * .25f), icing);
            }
            Box("Awning", root, new Vector3(0, eave - .2f, front - .8f), new Vector3(bw * .9f, .12f, 1.8f), red).localRotation = Quaternion.Euler(-20, 0, 0);
            Box("Awning edge", root, new Vector3(0, eave - .5f, front - 1.6f), new Vector3(bw * .9f, .25f, .12f), icing);
            Box("Counter", root, new Vector3(0, lift + .5f, front - .3f), new Vector3(bw * .9f, 1, .7f), Wood);
            for (int p = 0; p < 3 + level; p++) Shape("Pastry", root, PrimitiveType.Sphere, new Vector3((p - (2 + level) / 2f) * .7f, lift + 1.1f, front - .3f), new Vector3(.45f, .3f, .45f), new Color(.85f, .6f, .3f));
            if (level == 0) for (int side = -1; side <= 1; side += 2) Shape("Cart wheel", root, PrimitiveType.Cylinder, new Vector3(side * (bw / 2 + .12f), .6f, 0), new Vector3(1.2f, .12f, 1.2f), Material(Wood)).localRotation = Quaternion.Euler(0, 0, 90);
            if (level >= 1)
            {
                for (int side = -1; side <= 1; side += 2) Lamp("Window", root, new Vector3(side * bw * .3f, lift + bh * .55f, front - .03f), new Vector3(1.1f, 1, .06f));
                Prop("candy-cane-red", root, new Vector3(-w * .42f, 0, -d * .42f), 3.2f, 20); Prop("candy-cane-red", root, new Vector3(w * .42f, 0, -d * .42f), 3.2f, -20);
            }
            if (level >= 2)
            {
                for (int i = 0; i < 7; i++) Shape("Gumdrop", root, PrimitiveType.Sphere, new Vector3((i - 3) * bw / 7, eave + .35f, front - .3f), Vector3.one * .4f, gumdrops[i % 3]);
                Prop("snowman", root, new Vector3(w * .4f, 0, d * .3f), 3, -30); Prop("present-b-round", root, new Vector3(-w * .42f, 0, d * .35f), 2.8f, 70);
            }
            Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, eave + rise + .9f, front + .2f), new Vector3(bw * .6f, .7f, .16f), Glow(sign));
            return eave + rise + .9f - 4.6f;
        }
        // Christmas Crafts is an open market booth: poles holding a canvas roof with a white edge, a display table with a cloth in the stall colour and baubles
        // hanging in a row under the eave. Level 1 is a table under a flat canopy; 2 a booth with wooden walls, a pitched canvas roof, a wreath and a lantern;
        // 3 a wider, taller booth with two rows of baubles, a lit beam across the front, a star on top, a nutcracker and a tree.
        float Crafts(Transform root, int level, Color sign, float w, float d)
        {
            Color canvas = new Color(.3f, .45f, .7f), trim = new Color(.97f, .96f, .92f); Color[] baubles = { new Color(.85f, .15f, .15f), new Color(1, .8f, .25f), new Color(.2f, .6f, .3f) };
            float bw = level == 0 ? 4.5f : level == 1 ? 6 : w, bd = level == 0 ? 3.5f : level == 1 ? 5 : 6, h = level == 0 ? 2.8f : level == 1 ? 3.2f : 3.8f, pitch = 25;
            float rise = level == 0 ? .2f : bd / 2 * Mathf.Tan(pitch * Mathf.Deg2Rad), front = -bd / 2;
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2) Shape("Pole", root, PrimitiveType.Cylinder, new Vector3(x * (bw / 2 - .2f), h / 2, z * (bd / 2 - .2f)), new Vector3(.2f, h / 2, .2f), Wood);
            if (level >= 1)
            {
                Box("Back wall", root, new Vector3(0, h / 2, bd / 2 - .15f), new Vector3(bw, h, .3f), Wood);
                for (int side = -1; side <= 1; side += 2) Box("Side wall", root, new Vector3(side * (bw / 2 - .15f), h / 2, bd * .1f), new Vector3(.3f, h, bd * .8f), Wood);
                for (int side = -1; side <= 1; side += 2) Box("Canvas roof", root, new Vector3(0, h + rise / 2, side * bd / 4), new Vector3(bw + .6f, .15f, bd / 2 / Mathf.Cos(pitch * Mathf.Deg2Rad) + .4f), canvas).localRotation = Quaternion.Euler(side * pitch, 0, 0);
            }
            else Box("Canopy", root, new Vector3(0, h + .1f, 0), new Vector3(bw + .4f, .15f, bd + .4f), canvas);
            Box("Front trim", root, new Vector3(0, h - .1f, front - .2f), new Vector3(bw + .6f, .35f, .1f), trim);
            Box("Table", root, new Vector3(0, .5f, front + .6f), new Vector3(bw - .6f, .9f, 1.1f), Wood);
            Box("Cloth", root, new Vector3(0, .97f, front + .6f), new Vector3(bw - .5f, .06f, 1.25f), sign);
            int row = 4 + 2 * level;
            for (int r = 0; r <= (level == 2 ? 1 : 0); r++) for (int i = 0; i < row; i++)
            {
                float x = (i - (row - 1) / 2f) * (bw - 1) / (row - 1);
                Shape("String", root, PrimitiveType.Cube, new Vector3(x, h - .45f - r * .9f, front - .2f), new Vector3(.03f, .5f, .03f), trim);
                Shape("Bauble", root, PrimitiveType.Sphere, new Vector3(x, h - .85f - r * .9f, front - .2f), Vector3.one * .38f, baubles[(i + r) % 3]);
            }
            for (int i = 0; i < 2 + level; i++) Prop(i % 2 == 0 ? "present-a-cube" : "present-b-round", root, new Vector3((i - (1 + level) / 2f) * 1.1f, 1, front + .6f), 1.3f, i * 40);
            if (level >= 1) { Prop("wreath-decorated", root, new Vector3(0, h * .55f, bd / 2 - .4f), 2.4f, 180); Prop("lantern", root, new Vector3(bw / 2 - .4f, 0, front - .5f), 3); }
            if (level >= 2)
            {
                Lamp("Lit beam", root, new Vector3(0, h + .3f, front - .25f), new Vector3(bw + .4f, .18f, .18f));
                Shape("Star", root, PrimitiveType.Sphere, new Vector3(0, h + rise + .7f, 0), Vector3.one * .8f, Glow(new Color(1, .85f, .3f)));
                Prop("nutcracker", root, new Vector3(-w * .45f, 0, -d * .35f), 3.4f, 160); Prop("tree-decorated-snow", root, new Vector3(w * .42f, 0, d * .38f), 4, 40);
            }
            Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, h + rise + (level == 2 ? 1.5f : .8f), front + .3f), new Vector3(bw * .55f, .65f, .16f), Glow(sign));
            return h + rise + 1 - 4.6f;
        }
        // A fairground galloper after Joash's reference picture (issue 44): a round deck with a red rim, chrome poles, the kit's reindeer instead of horses,
        // a cream canopy with a white dome and warm bulbs along the eave. Level 1 is a small plain one with four animals; 2 is wider with six, painted
        // panels and a gold finial; 3 is wider again with eight, lit poles, a second tier and a glowing crown. The whole thing turns.
        void Carousel(Transform ride, int level, Color sign)
        {
            int animals = 4 + 2 * level; float radius = 4 + .6f * level, eave = 5, span = radius * 2 + .8f; Color gold = new Color(1, .8f, .25f);
            Material cream = Material(new Color(.95f, .92f, .84f)), white = Material(new Color(.96f, .96f, .96f)), red = Material(new Color(.75f, .12f, .1f)), chrome = Material(new Color(.8f, .82f, .88f), .9f);
            Shape("Deck", ride, PrimitiveType.Cylinder, Vector3.up * .3f, new Vector3(radius * 2, .3f, radius * 2), cream);
            Shape("Deck rim", ride, PrimitiveType.Cylinder, Vector3.up * .55f, new Vector3(radius * 2 + .2f, .08f, radius * 2 + .2f), red);
            Shape("Column", ride, PrimitiveType.Cylinder, Vector3.up * 3, new Vector3(1.6f, 2.4f, 1.6f), cream);
            Shape("Column band", ride, PrimitiveType.Cylinder, Vector3.up * 2.2f, new Vector3(1.75f, .15f, 1.75f), level >= 1 ? Glow(WarmLight) : red);
            for (int i = 0; i < animals; i++)
            {
                float a = i * 2 * Mathf.PI / animals; var spot = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (radius - 1.1f);
                Shape("Pole", ride, PrimitiveType.Cylinder, spot + Vector3.up * 2.8f, new Vector3(.12f, 2.3f, .12f), chrome);
                // Every other animal rides higher, as on a galloper; each faces the way the deck turns.
                Prop("reindeer", ride, spot + Vector3.up * (i % 2 == 0 ? 1.1f : 1.5f), 2.4f, 180 - a * Mathf.Rad2Deg);
                if (level >= 2) for (int k = 1; k <= 3; k++) Shape("Pole light", ride, PrimitiveType.Sphere, spot + Vector3.up * (1.5f + k), Vector3.one * .22f, Glow(WarmLight));
            }
            Shape("Frieze", ride, PrimitiveType.Cylinder, Vector3.up * (eave + .45f), new Vector3(span, .45f, span), cream);
            Shape("Dome", ride, PrimitiveType.Sphere, Vector3.up * (eave + 1.3f), new Vector3(span - .2f, 2.6f, span - .2f), white);
            int bulbs = 16 + 8 * level;
            for (int i = 0; i < bulbs; i++) { float a = i * 2 * Mathf.PI / bulbs; Shape("Eave bulb", ride, PrimitiveType.Sphere, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (span / 2) + Vector3.up * eave, Vector3.one * .28f, Glow(WarmLight)); }
            if (level >= 1) for (int i = 0; i < animals * 2; i++)
            {
                // Painted panels around the frieze, in the ride's colour and a winter blue, each turned to face outwards.
                float a = i * Mathf.PI / animals;
                var panel = Shape("Painted panel", ride, PrimitiveType.Cube, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (span / 2) + Vector3.up * (eave + .45f), new Vector3(1, .55f, .08f), i % 2 == 0 ? sign : new Color(.45f, .65f, .9f));
                panel.localRotation = Quaternion.Euler(0, 90 - a * Mathf.Rad2Deg, 0);
            }
            if (level == 1) Shape("Finial", ride, PrimitiveType.Sphere, Vector3.up * (eave + 2.75f), Vector3.one * .6f, Material(gold, .6f));
            if (level >= 2)
            {
                Shape("Upper frieze", ride, PrimitiveType.Cylinder, Vector3.up * (eave + 2.3f), new Vector3(span * .45f, .3f, span * .45f), cream);
                Shape("Upper dome", ride, PrimitiveType.Sphere, Vector3.up * (eave + 2.9f), new Vector3(span * .42f, 1.6f, span * .42f), white);
                Shape("Crown", ride, PrimitiveType.Sphere, Vector3.up * (eave + 3.9f), Vector3.one * .9f, Glow(gold));
            }
        }
        // The Festival Wheel, in the same spirit (issue 44): a low platform with a ticket booth, two A-frames meeting at the axle, half-length spokes and gondolas
        // that hang upright as the wheel turns (Synchronize resets their rotation). Level 1: eight gondolas on plain steel; 2: twelve on a bigger wheel with lit spokes,
        // a ring of bulbs and a lit hub; 3: sixteen, bigger again, with lit frames, a second ring and a big glowing hub star. The wheel plane faces the camera.
        void Wheel(Transform root, Transform ride, int level, Color sign, float w, float d)
        {
            int count = 8 + 4 * level; float radius = 5 + level, hub = 12; Color gold = new Color(1, .8f, .25f);
            Material steel = Material(new Color(.55f, .58f, .62f), .5f);
            Box("Platform", root, Vector3.up * .3f, new Vector3(w, .6f, d), new Color(.95f, .92f, .84f));
            Box("Ticket booth", root, new Vector3(0, 1.9f, -d / 2 + 1.4f), new Vector3(3.2f, 2.6f, 2.4f), Wood);
            Shape("Sign", root, PrimitiveType.Cube, new Vector3(0, 3.5f, -d / 2 + .2f), new Vector3(3.4f, .7f, .2f), Glow(sign));
            for (int side = -1; side <= 1; side += 2) for (int leg = -1; leg <= 1; leg += 2)
            {
                Vector3 foot = new Vector3(leg * 4.5f, .6f, side * 1.4f), top = new Vector3(0, hub, side * 1.4f);
                Shape("Frame", root, PrimitiveType.Cube, (foot + top) / 2, new Vector3(.45f, .45f, Vector3.Distance(foot, top)), level == 2 ? Glow(WarmLight) : steel).localRotation = Quaternion.LookRotation(top - foot);
            }
            Shape("Axle", root, PrimitiveType.Cylinder, Vector3.up * hub, new Vector3(.9f, 1.7f, .9f), steel).localRotation = Quaternion.Euler(90, 0, 0);
            if (level >= 1) Shape("Hub light", root, PrimitiveType.Sphere, Vector3.up * hub, Vector3.one * (level == 2 ? 2.6f : 1.5f), Glow(gold));
            // Local Y rotation becomes world Z rotation for this wheel plane.
            ride.localRotation = Quaternion.Euler(90, 0, 0); ride.localPosition = Vector3.up * hub;
            for (int i = 0; i < count; i++)
            {
                float a = i * 2 * Mathf.PI / count; var rim = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                Shape("Spoke", ride, PrimitiveType.Cube, rim * radius / 2, new Vector3(.16f, .16f, radius), level >= 1 ? Glow(WarmLight) : steel).localRotation = Quaternion.Euler(0, i * 360f / count, 0);
                var pivot = new GameObject("Gondola").transform; pivot.SetParent(ride, false); pivot.localPosition = rim * radius; gondolas.Add(pivot);
                Box("Cabin roof", pivot, new Vector3(0, -.1f, 0), new Vector3(1.8f, .25f, 1.5f), new Color(.92f, .92f, .92f));
                Box("Cabin", pivot, new Vector3(0, -.9f, 0), new Vector3(1.6f, 1.4f, 1.3f), sign);
                Lamp("Window", pivot, new Vector3(0, -.8f, -.66f), new Vector3(1.2f, .6f, .06f));
            }
            for (int ring = 1; ring <= level; ring++) for (int i = 0; i < 16 + 8 * level; i++)
            {
                float a = i * 2 * Mathf.PI / (16 + 8 * level);
                Lamp("Rim light", ride, new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * (radius + .5f * ring), Vector3.one * .4f);
            }
        }
        void OnDestroy() { foreach (var m in materials.Values) if (m) Destroy(m); foreach (var m in glows.Values) if (m) Destroy(m); foreach (var m in kitMaterials.Values) if (m) Destroy(m); foreach (var w in walkers.Values) if (w.graph.IsValid()) w.graph.Destroy(); }

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
