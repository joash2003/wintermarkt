using System.Collections.Generic;
using UnityEngine;
using Wintermarkt.Core;

namespace Wintermarkt
{
    /// <summary>Original geometric art. Site dressing is a provisional blockout, not surveyed geometry.</summary>
    public sealed class MarketWorld : MonoBehaviour
    {
        readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        readonly Dictionary<int, Transform> buildings = new Dictionary<int, Transform>();
        readonly Dictionary<int, int> levels = new Dictionary<int, int>();
        readonly Dictionary<int, Transform> people = new Dictionary<int, Transform>();
        readonly List<Transform> movingRides = new List<Transform>();
        readonly List<AmbientVehicle> traffic = new List<AmbientVehicle>();
        Transform content, preview;
        public MarketSimulation Simulation { get; private set; }
        public Vector3 World(Cell c) { return new Vector3((c.x - (Simulation.Site.Width - 1) * .5f) * CellSize, 0, ((Simulation.Site.Height - 1) * .5f - c.y) * CellSize); }
        public float CellSize { get { return Simulation.Site.Data.metersPerCell; } }
        public Cell Grid(Vector3 p) { return new Cell(Mathf.RoundToInt(p.x / CellSize + (Simulation.Site.Width - 1) * .5f), Mathf.RoundToInt((Simulation.Site.Height - 1) * .5f - p.z / CellSize)); }

        Material Material(Color c)
        {
            Material m; if (!materials.TryGetValue(c, out m))
            {
                m = new Material(Shader.Find("Standard")); m.color = c; m.SetFloat("_Glossiness", .12f); materials[c] = m;
            }
            return m;
        }
        Transform Shape(string title, Transform parent, PrimitiveType shape, Vector3 position, Vector3 size, Color color)
        {
            var obj = GameObject.CreatePrimitive(shape); obj.name = title; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = Material(color);
            var collider = obj.GetComponent<Collider>(); if (collider) Destroy(collider);
            return obj.transform;
        }
        Transform Box(string name, Transform parent, Vector3 pos, Vector3 size, Color c) { return Shape(name, parent, PrimitiveType.Cube, pos, size, c); }
        void Caption(string title, Transform parent, Vector3 pos, float size = 1.4f)
        {
            var obj = new GameObject(title); obj.transform.SetParent(parent, false); obj.transform.localPosition = pos;
            obj.transform.localRotation = Quaternion.Euler(55, 0, 0);
            var text = obj.AddComponent<TextMesh>(); text.text = title; text.fontSize = 40; text.characterSize = size; text.anchor = TextAnchor.MiddleCenter; text.color = new Color(.12f, .22f, .27f);
        }
        public void Initialize(MarketSimulation sim)
        {
            Simulation = sim;
            if (content) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            buildings.Clear(); levels.Clear(); people.Clear(); movingRides.Clear(); traffic.Clear();
            content = new GameObject("Volksgarten development blockout").transform; content.SetParent(transform);
            float width = sim.Site.Width * CellSize, height = sim.Site.Height * CellSize;
            Box("Snow ground", content, new Vector3(0, -.55f, 0), new Vector3(width + 100, 1, height + 80), new Color(.78f, .86f, .88f));
            Box("Park lawn under snow", content, new Vector3(0, -.03f, 0), new Vector3(width, .1f, height), new Color(.66f, .76f, .72f));
            for (int y = 0; y < sim.Site.Height; y++) for (int x = 0; x < sim.Site.Width; x++)
            {
                Cell cell = new Cell(x, y); char tile = sim.Site.Data.rows[y][x]; Vector3 p = World(cell);
                if (tile == 'p' || tile == 'E') Box("Public path", content, p + Vector3.up * .03f, new Vector3(CellSize, .12f, CellSize), new Color(.86f, .83f, .75f));
                if (tile == 'T') Tree(p, 1);
            }
            // These roads and building masses establish orientation only; see LOCATION_AND_TRANSIT.md.
            float north = height * .5f + 12, east = width * .5f + 12;
            Box("Goethekreuzung road context — provisional", content, new Vector3(0, .03f, north), new Vector3(width + 90, .2f, 12), new Color(.21f, .28f, .31f));
            Box("Landstraße context — provisional", content, new Vector3(east, .03f, height * .25f), new Vector3(12, .2f, height * .7f), new Color(.21f, .28f, .31f));
            for (int i = -8; i <= 8; i++) Box("Road dash", content, new Vector3(i * 10, .16f, north), new Vector3(4, .03f, .22f), Color.white);
            for (int i = 0; i < 2; i++)
                Box("Tram rail — provisional alignment", content, new Vector3(east - 2 + i * 2, .2f, height * .3f), new Vector3(.14f, .1f, height * .65f), new Color(.55f, .61f, .64f));
            Box("Goethekreuzung platform", content, new Vector3(east - 8, .4f, north - 5), new Vector3(3, .8f, 15), new Color(.74f, .75f, .7f));
            Box("Stop sign", content, new Vector3(east - 8, 4, north - 9), new Vector3(.25f, 7, .25f), new Color(.2f, .25f, .29f));
            Caption("GOETHEKREUZUNG", content, new Vector3(east - 14, 5, north - 9), .65f);
            for (int i = 0; i < 5; i++)
            {
                float x = -width * .5f + 8 + i * 25;
                Box("Surrounding building mass — provisional", content, new Vector3(x, 8 + i % 2 * 2, north + 23), new Vector3(20, 16 + i % 2 * 4, 15), new Color(.56f + i * .025f, .58f, .55f));
                for (int floor = 0; floor < 3; floor++) for (int window = 0; window < 4; window++)
                    Box("Warm window", content, new Vector3(x - 7 + window * 4.5f, 4 + floor * 4, north + 15.4f), new Vector3(2, 2.4f, .08f), new Color(.97f, .76f, .4f));
            }
            Box("Musiktheater mass — provisional proportions", content, new Vector3(10, 10, -height * .5f - 17), new Vector3(76, 20, 24), new Color(.67f, .65f, .57f));
            for (int i = 0; i < 13; i++) Box("Theater facade rhythm", content, new Vector3(-25 + i * 5.5f, 9, -height * .5f - 4.8f), new Vector3(.6f, 16, .5f), new Color(.85f, .83f, .73f));
            Caption("MUSIKTHEATER", content, new Vector3(12, 20.5f, -height * .5f - 10), 1);
            Caption("VOLKSGARTEN", content, World(new Cell(15, 24)) + Vector3.up * .25f, 1.1f);
            BuildTraffic(east, north, height);
            preview = new GameObject("Placement preview").transform; preview.SetParent(content);
        }
        void Tree(Vector3 p, float scale)
        {
            Shape("Trunk", content, PrimitiveType.Cylinder, p + Vector3.up * 2 * scale, new Vector3(.8f, 2 * scale, .8f), new Color(.29f, .2f, .13f));
            for (int i = 0; i < 3; i++)
                Shape("Snowy evergreen", content, PrimitiveType.Sphere, p + Vector3.up * (4 + i * 2) * scale, new Vector3(7 - i * 1.8f, 4, 7 - i * 1.8f) * scale, i == 2 ? new Color(.85f, .91f, .86f) : new Color(.12f, .28f, .23f));
        }
        void BuildTraffic(float east, float north, float height)
        {
            var tram = new GameObject("Ambient tram — prototype route").transform; tram.SetParent(content);
            for (int i = -1; i <= 1; i++)
            {
                Box("Articulated body", tram, new Vector3(0, 2, i * 6.2f), new Vector3(2.7f, 3.2f, 6), new Color(.92f, .93f, .89f));
                Box("Window band", tram, new Vector3(0, 2.5f, i * 6.2f), new Vector3(2.8f, 1.4f, 5.5f), new Color(.12f, .26f, .32f));
                Box("Red lower band", tram, new Vector3(0, .9f, i * 6.2f), new Vector3(2.8f, .6f, 5.8f), new Color(.77f, .13f, .16f));
            }
            traffic.Add(new AmbientVehicle(tram, new Vector3(east - 1, 0, height * .5f + 35), new Vector3(east - 1, 0, height * .1f), .32f, 9, 4));
            var bus = new GameObject("Ambient bus — prototype route").transform; bus.SetParent(content);
            Box("Bus body", bus, new Vector3(0, 2, 0), new Vector3(2.8f, 3.2f, 10), new Color(.9f, .91f, .87f));
            Box("Bus windows", bus, new Vector3(0, 2.5f, 0), new Vector3(2.9f, 1.3f, 8.7f), new Color(.1f, .25f, .32f));
            Box("Bus livery", bus, new Vector3(0, 1, 0), new Vector3(2.9f, .7f, 9.8f), new Color(.77f, .13f, .16f));
            traffic.Add(new AmbientVehicle(bus, new Vector3(-110, 0, north - 3), new Vector3(110, 0, north - 3), .6f, 10, 5));
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
                    buildings[b.Id] = BuildShop(b); levels[b.Id] = b.Level;
                }
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
                person.localScale = Vector3.one * (v.State == VisitorState.Serving ? 1.08f : 1);
            }
            var removed = new List<int>(); foreach (var pair in people) if (!active.Contains(pair.Key)) { Destroy(pair.Value.gameObject); removed.Add(pair.Key); }
            foreach (int id in removed) people.Remove(id);
            for (int i = movingRides.Count - 1; i >= 0; i--) { if (!movingRides[i]) { movingRides.RemoveAt(i); continue; } movingRides[i].Rotate(Vector3.up, 20 * dt, Space.Self); }
            foreach (var vehicle in traffic) vehicle.Tick(dt);
        }
        Transform BuildShop(MarketBuilding b)
        {
            var root = new GameObject(b.Definition.title + " level " + (b.Level + 1)).transform; root.SetParent(content); root.position = World(b.Origin) + new Vector3((b.Definition.width - 1) * CellSize / 2, 0, -(b.Definition.depth - 1) * CellSize / 2);
            float w = b.Definition.width * CellSize - .5f, d = b.Definition.depth * CellSize - .5f;
            Color roof = b.Definition.id == "punsch" ? new Color(.57f, .15f, .19f) : b.Definition.id == "pastry" ? new Color(.2f, .37f, .36f) : new Color(.25f, .28f, .48f);
            Box("Wood base", root, new Vector3(0, 1.8f, 0), new Vector3(w, 3.6f, d), new Color(.4f, .26f, .15f));
            Box("Snow roof", root, new Vector3(0, 4.3f, 0), new Vector3(w + .7f, .4f, d + .7f), new Color(.94f, .95f, .87f));
            Box("Canopy", root, new Vector3(0, 3.3f, -d / 2 - .3f), new Vector3(w, .55f, 1.5f), roof);
            Box("Service window", root, new Vector3(0, 2.1f, -d / 2 - .05f), new Vector3(w * .7f, 1.7f, .12f), new Color(.97f, .77f, .36f));
            if (b.Definition.id == "carousel" || b.Definition.id == "wheel")
            {
                var ride = new GameObject("Ride motion").transform; ride.SetParent(root, false); ride.localPosition = Vector3.up * 5;
                if (b.Definition.id == "carousel")
                {
                    Shape("Carousel canopy", ride, PrimitiveType.Cylinder, Vector3.up * 2, new Vector3(w * .9f, .4f, w * .9f), roof);
                    for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3; Box("Ride seat", ride, new Vector3(Mathf.Cos(a) * 3, 0, Mathf.Sin(a) * 3), new Vector3(1, 1.2f, 1), Color.yellow); }
                }
                else
                {
                    // Local Y rotation becomes world Z rotation for this wheel plane.
                    ride.localRotation = Quaternion.Euler(90, 0, 0); ride.localPosition = Vector3.up * 12;
                    for (int i = 0; i < 12; i++) { float a = i * Mathf.PI / 6; var spoke = Box("Wheel spoke", ride, Vector3.zero, new Vector3(.18f, .18f, 12), new Color(.8f, .67f, .34f)); spoke.localRotation = Quaternion.Euler(0, i * 30, 0); Shape("Cabin", ride, PrimitiveType.Sphere, new Vector3(Mathf.Sin(a) * 6, 0, Mathf.Cos(a) * 6), new Vector3(1.7f, 1.3f, 1.7f), roof); }
                    Box("Wheel support", root, Vector3.up * 7, new Vector3(.8f, 14, .8f), new Color(.8f, .67f, .34f));
                }
                movingRides.Add(ride);
            }
            for (int i = 0; i <= b.Level; i++) Shape("Upgrade star", root, PrimitiveType.Sphere, new Vector3(-w / 3 + i * 1.3f, 4.9f, -d / 2), Vector3.one * .65f, new Color(1, .77f, .2f));
            Caption(b.Definition.title + "  " + (b.Level + 1), root, new Vector3(0, 5.6f, -d / 2 - 1), .38f);
            for (int i = 0; i < b.Definition.queueSlots; i++)
            {
                Vector3 marker = root.InverseTransformPoint(World(b.QueueCell(i)) + Vector3.up * .08f);
                Box("Queue marker", root, marker, new Vector3(1.1f, .1f, 1.1f), new Color(.75f, .64f, .4f));
            }
            return root;
        }
        void OnDestroy() { foreach (var m in materials.Values) if (m) Destroy(m); }

        sealed class AmbientVehicle
        {
            readonly Transform transform; readonly Vector3 start, end; readonly float stopFraction, speed, dwell;
            float progress, waiting; bool stopped;
            public AmbientVehicle(Transform t, Vector3 a, Vector3 b, float fraction, float speed, float dwell) { transform = t; start = a; end = b; stopFraction = fraction; this.speed = speed; this.dwell = dwell; transform.position = a; transform.rotation = Quaternion.LookRotation(b - a); }
            public void Tick(float dt)
            {
                if (waiting > 0) { waiting -= dt; return; }
                progress += dt * speed / Vector3.Distance(start, end);
                if (!stopped && progress >= stopFraction) { progress = stopFraction; stopped = true; waiting = dwell; }
                if (progress >= 1) { progress = 0; stopped = false; waiting = 10; }
                transform.position = Vector3.Lerp(start, end, progress);
            }
        }
    }
}
