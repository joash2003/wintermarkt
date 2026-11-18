using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Wintermarkt
{
    public sealed class MarketUI : MonoBehaviour
    {
        MarketGame game;
        RectTransform canvas;
        RectTransform hud;
        Transform menu, selection;
        Text stats, footer, details;
        Button upgrade;
        Font font;
        Color ink = new Color(.9f, .93f, .89f);
        Color gold = new Color(.96f, .77f, .4f);
        // Wooden panels and buttons from Kenney's UI Pack Adventure, sliced so the frame keeps its width at any size.
        Sprite panelSprite, buttonSprite, bannerSprite;
        Sprite Nine(string image, int border)
        {
            var texture = Resources.Load<Texture2D>("Wintermarkt/UI/" + image);
            return texture ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, Vector4.one * border * texture.width / 128f) : null;
        }
        Transform Framed(Transform panel, Sprite sprite)
        {
            if (!sprite) return panel;
            var image = panel.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Sliced; image.color = Color.white; return panel;
        }
        readonly Color navy = new Color(.055f, .12f, .15f, .97f);

        public void Initialize(MarketGame controller)
        {
            game = controller; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            panelSprite = Nine("panel_brown", 28); buttonSprite = Nine("button_brown", 22); bannerSprite = Nine("button_red", 22);
            // On the cream paper of the wooden panels the text is dark brown and the headings berry red.
            if (panelSprite) { ink = new Color(.25f, .16f, .1f); gold = new Color(.62f, .13f, .15f); }
            var obj = new GameObject("Wintermarkt UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); obj.transform.SetParent(transform);
            obj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            canvas = obj.GetComponent<RectTransform>();
            if (!FindAnyObjectByType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(transform);
            hud = Rect("Festival HUD", canvas, new Vector2(.5f, .5f), Vector2.zero, new Vector2(1600, 900));
            hud.anchorMin = Vector2.zero; hud.anchorMax = Vector2.one; hud.offsetMin = Vector2.zero; hud.offsetMax = Vector2.zero;
            var top = Framed(Panel("Header", hud, new Vector2(.5f, 1), new Vector2(0, -48), new Vector2(1560, 76), navy), panelSprite);
            Label("LINZ  WINTERMARKT", top, new Vector2(-555, 9), new Vector2(380, 32), 26, gold);
            Label("VOLKSGARTEN · GOETHEKREUZUNG", top, new Vector2(-555, -14), new Vector2(380, 18), 12, ink);
            stats = Label("", top, new Vector2(25, 0), new Vector2(760, 65), 20, ink);
            ActionButton("Pause", top, new Vector2(605, 0), new Vector2(105, 42), game.TogglePause);
            ActionButton("Sound", top, new Vector2(720, 0), new Vector2(95, 42), game.ToggleSound);
            var build = Framed(Panel("Build menu", hud, new Vector2(0, 1), new Vector2(140, -350), new Vector2(240, 500), navy), panelSprite);
            Label("GROW YOUR MARKET", build, new Vector2(0, 215), new Vector2(220, 32), 18, gold);
            Label("Build · Welcome · Upgrade", build, new Vector2(0, 182), new Vector2(220, 28), 14, ink);
            selection = Framed(Panel("Selected building", hud, new Vector2(1, 1), new Vector2(-150, -283), new Vector2(260, 365), navy), panelSprite);
            Label("YOUR ATTRACTION", selection, new Vector2(0, 148), new Vector2(240, 32), 19, gold);
            details = Label("Select a building to inspect it.", selection, new Vector2(0, 20), new Vector2(230, 210), 18, ink);
            upgrade = ActionButton("Upgrade", selection, new Vector2(-57, -140), new Vector2(108, 44), game.Upgrade);
            ActionButton("Remove", selection, new Vector2(57, -140), new Vector2(108, 44), game.Remove);
            var bottom = Framed(Panel("Status", hud, new Vector2(.5f, 0), new Vector2(0, 38), new Vector2(1560, 56), navy), panelSprite);
            footer = Label("", bottom, Vector2.zero, new Vector2(1510, 48), 17, ink);
            bannerPanel = Panel("Event banner", hud, new Vector2(.5f, 1), new Vector2(60, -128), new Vector2(820, 50), new Color(.6f, .13f, .15f, .95f));
            Framed(bannerPanel, bannerSprite); banner = Label("", bannerPanel, Vector2.zero, new Vector2(780, 38), 24, new Color(1, .96f, .88f)); banner.resizeTextForBestFit = true; banner.resizeTextMinSize = 13; banner.resizeTextMaxSize = 24; bannerPanel.gameObject.SetActive(false);
            // Add definition buttons after the simulation has been initialized by MarketGame.
            BuildMenu = build;
        }
        Transform BuildMenu, bannerPanel;
        Text banner;
        int seenEvents; float flashUntil;
        bool buildButtonsReady;
        RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var r = obj.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = pos; r.sizeDelta = size; return r;
        }
        Transform Panel(string title, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        { var r = Rect(title, parent, anchor, pos, size); r.gameObject.AddComponent<Image>().color = color; return r; }
        Text Label(string value, Transform parent, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            var r = Rect(value, parent, new Vector2(.5f, .5f), pos, size); var t = r.gameObject.AddComponent<Text>(); t.text = value; t.font = font; t.fontSize = fontSize; t.color = color; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; return t;
        }
        Button ActionButton(string title, Transform parent, Vector2 pos, Vector2 size, Action action)
        {
            var r = Rect(title, parent, new Vector2(.5f, .5f), pos, size); r.gameObject.AddComponent<Image>().color = new Color(.14f, .28f, .29f); Framed(r, buttonSprite);
            var button = r.gameObject.AddComponent<Button>(); var colors = button.colors; colors.highlightedColor = new Color(1, .88f, .64f); colors.pressedColor = new Color(.75f, .8f, .7f); button.colors = colors;
            button.onClick.AddListener(() => { game.PlayClick(); action(); }); Label(title, r, Vector2.zero, size - new Vector2(8, 4), 18, ink); return button;
        }
        public void Refresh()
        {
            if (!buildButtonsReady)
            {
                int i = 0; foreach (var definition in game.Simulation.Settings.buildings)
                {
                    string id = definition.id; string title = definition.title + "\n" + definition.levels[0].cost + " coins";
                    if (definition.unlockVisitors > 0) title += " · " + definition.unlockVisitors + " guests";
                    var button = ActionButton(title, BuildMenu, new Vector2(0, 120 - i++ * 68), new Vector2(215, 58), () => game.ChooseBuild(id));
                    // Legend: the same colour as the stall sign and the wish cubes above visitors.
                    Panel("Category colour", button.transform, new Vector2(0, .5f), new Vector2(10, 0), new Vector2(12, 46), MarketWorld.CategoryColor(definition.category)).GetComponent<Image>().raycastTarget = false;
                }
                Label("WASD / arrows: pan\nScroll: zoom · Middle drag: pan\nRight-click: cancel build", BuildMenu, new Vector2(0, -205), new Vector2(200, 54), 12, ink);
                buildButtonsReady = true;
            }
            hud.gameObject.SetActive(game.State == FestivalState.Playing || game.State == FestivalState.Paused);
            if (menu) { menu.gameObject.SetActive(false); Destroy(menu.gameObject); }
            RefreshSelection();
            if (game.State == FestivalState.Playing) return;
            // After a finished season the market stays visible behind the card, so the fireworks can be seen.
            menu = Panel("Screen overlay", canvas, new Vector2(.5f, .5f), Vector2.zero, new Vector2(4000, 2400), new Color(.02f, .07f, .1f, game.State == FestivalState.Results && game.Simulation.Won ? .35f : .85f));
            var card = Framed(Panel("Festival card", menu, new Vector2(.5f, .5f), Vector2.zero, new Vector2(650, 640), navy), panelSprite);
            if (game.State == FestivalState.Menu)
            {
                Label("LINZ\nWINTERMARKT", card, new Vector2(0, 200), new Vector2(600, 140), 54, gold);
                Label("A little square. A whole city of Christmas.", card, new Vector2(0, 90), new Vector2(590, 45), 21, ink);
                Label("Every visitor wants two or three different things.\nFour days, one star each: send enough of them home happy.\nEarn coins from service and reinvest them.", card, new Vector2(0, -5), new Vector2(580, 125), 20, ink);
                ActionButton("Open the market", card, new Vector2(0, -115), new Vector2(350, 54), game.StartFestival);
                ActionButton(game.Muted ? "Sound off" : "Sound on", card, new Vector2(-90, -185), new Vector2(170, 45), game.ToggleSound);
                ActionButton("Quit", card, new Vector2(90, -185), new Vector2(170, 45), game.Quit);
                Label("Early prototype · site layout under review", card, new Vector2(0, -275), new Vector2(600, 30), 15, ink);
            }
            else if (game.State == FestivalState.Paused)
            {
                Label("A QUIET MOMENT", card, new Vector2(0, 180), new Vector2(600, 80), 38, gold);
                Label("The festival is paused.", card, new Vector2(0, 100), new Vector2(560, 45), 22, ink);
                ActionButton("Resume", card, new Vector2(0, 15), new Vector2(350, 50), game.TogglePause);
                ActionButton("Restart festival", card, new Vector2(0, -55), new Vector2(350, 50), game.StartFestival);
                ActionButton("Main menu", card, new Vector2(0, -125), new Vector2(350, 50), game.Menu);
            }
            else
            {
                var sim = game.Simulation;
                string verdict = sim.Failed ? "THE MARKET CLOSED EARLY" : sim.KrampusHits > 0 && sim.DrivenOff == 0 && sim.Stars < 2 ? "THE KRAMPUS WON" : sim.Stars >= sim.WaveCount ? "THE BEST MARKET LINZ HAS SEEN" : sim.Stars >= sim.WaveCount - 1 ? "A GOOD MARKET" : sim.Stars >= 1 ? "A LONG, COLD WINTER" : "LINZ WENT HOME HUNGRY";
                Label(verdict, card, new Vector2(0, 250), new Vector2(620, 60), 34, gold);
                // Stars as gold or dim diamonds: the built-in font has no star glyph on every platform.
                for (int i = 0; i < sim.WaveCount; i++)
                    Panel("Star", card, new Vector2(.5f, .5f), new Vector2((i - (sim.WaveCount - 1) * .5f) * 70, 185), new Vector2(34, 34), i < sim.Stars ? new Color(.96f, .7f, .15f) : new Color(.72f, .66f, .56f)).localRotation = Quaternion.Euler(0, 0, 45);
                Label(sim.Happy + (sim.Happy == 1 ? " happy visitor" : " happy visitors"), card, new Vector2(0, 120), new Vector2(590, 50), 32, ink);
                string acts = "";
                for (int i = 0; i < sim.WaveResults.Count; i++) acts += (i == 0 ? "" : i == 2 ? "\n" : "   ·   ") + sim.Settings.waves[i].title + " " + sim.WaveResults[i] + "/" + sim.Settings.waves[i].target;
                if (sim.Failed) acts += (acts.Length > 0 ? "   ·   " : "") + "reputation fell to zero in " + sim.CurrentWave.title;
                Label(acts, card, new Vector2(0, 72), new Vector2(630, 46), 16, gold);
                Label(sim.Welcomed + " arrived  ·  " + sim.Disappointed + " wanted more  ·  " + sim.Abandoned + " gave up\n" + sim.Helped + " of " + (sim.Helped + sim.MissedHelp) + " calls helped  ·  " + sim.Swept + " snow piles swept\n"
                    + (sim.KrampusHits > 0 ? sim.DrivenOff + " Krampus driven off  ·  " + sim.Scared + " visitors scared away\n" : "") + sim.Earned + " coins earned  ·  final reputation " + Mathf.RoundToInt(sim.Reputation) + "%", card, new Vector2(0, -8), new Vector2(610, 120), 19, ink);
                ActionButton("Play again", card, new Vector2(0, -135), new Vector2(350, 50), game.StartFestival);
                ActionButton("Main menu", card, new Vector2(0, -190), new Vector2(350, 50), game.Menu);
            }
        }
        public void RefreshSelection()
        {
            if (!details || game.Simulation == null) return;
            var b = game.Selected;
            selection.gameObject.SetActive(b != null);
            if (b == null) return;
            bool max = b.Level + 1 >= b.Definition.levels.Length;
            details.text = b.Definition.title + "\n" + game.Simulation.QueueCount(b) + " visitors waiting/in line\nLEVEL " + (b.Level + 1) + " / " + b.Definition.levels.Length + "\n\n" + b.Stats.income + " coins / visitor\n" + b.Stats.serviceSeconds.ToString("0.#") + "s service · " + b.Stats.capacity + " at once\nAttraction +" + b.Stats.attraction.ToString("0.#") + "\n" + (max ? "Fully upgraded" : "Next upgrade: " + b.Definition.levels[b.Level + 1].cost + " coins") + "\nRemove: +" + game.Simulation.Refund(b) + " coins";
            upgrade.interactable = !max && game.State == FestivalState.Playing && game.Simulation.Coins >= b.Definition.levels[b.Level + 1].cost;
        }
        void Update()
        {
            if (game.Simulation == null) return;
            var sim = game.Simulation; int remaining = Mathf.CeilToInt(sim.Remaining);
            stats.text = "DAY " + (sim.Wave + 1) + "/" + sim.WaveCount + "  " + sim.CurrentWave.title.ToUpperInvariant() + "     " + sim.WaveHappy + " / " + sim.CurrentWave.target + " HAPPY     " + remaining / 60 + ":" + (remaining % 60).ToString("00") + "\n"
                + sim.Coins + " coins   ·   " + Mathf.RoundToInt(sim.Reputation) + "% reputation   ·   " + (sim.MostWanted != null ? "Visitors are missing: " + sim.MostWanted.ToUpperInvariant() : sim.Happy + " happy in total") + "   ·   stars " + sim.Stars + "/" + sim.WaveCount;
            // Event banner: a running event with its time left, else a warning of the next one, else the one that just happened.
            if (sim.EventCount != seenEvents) { if (sim.EventCount > seenEvents) { flashUntil = Time.unscaledTime + 5; game.PlayClick(); } seenEvents = sim.EventCount; }
            int hunting = 0; foreach (var k in sim.Krampuses) if (!k.Fleeing) hunting++;
            string news = hunting > 0 ? "Krampus run! Click them to throw snowballs   " + hunting + " left" : sim.ActiveEvent != null ? sim.ActiveEvent.title + "   " + Mathf.CeilToInt(sim.EventRemaining) + "s" + (sim.SnowPiles.Count > 0 ? "   ·   sweep the snow!" : "")
                : sim.Upcoming != null ? "In " + Mathf.CeilToInt(sim.UpcomingIn) + "s   " + sim.Upcoming.title
                : sim.SnowPiles.Count > 0 ? "Snow on " + sim.SnowPiles.Count + " queue spots: hold the mouse button and sweep it away"
                : sim.LastEvent != null && Time.unscaledTime < flashUntil ? sim.LastEvent.title : null;
            bannerPanel.gameObject.SetActive(news != null); if (news != null) banner.text = news;
            footer.text = game.Message; if (game.Muted) footer.text += "   ·   Sound off (M)";
            if (game.Selected != null) RefreshSelection();
        }
    }
}
