using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Wintermarkt.Core;

namespace Wintermarkt.Editor
{
    /// <summary>
    /// Plays the whole evening at high speed and saves a picture of each moment to Logs/shots, so the look can be checked without a person at the mouse.
    /// Run: bash tools/unity.sh shots   (the editor must not have this project open). It helps stalls, sweeps snow and throws snowballs like a player would.
    /// It only proves that the evening runs and what it looks like; it says nothing about how the game feels.
    /// </summary>
    [InitializeOnLoad]
    static class PlayModeShots
    {
        static readonly HashSet<string> done = new HashSet<string>();
        static bool placed; static double endAt, upgradedAt; static MarketBuilding upgraded;
        static readonly string[] order = { "punsch", "pastry", "craft", "carousel", "wheel" };   // pictured in this order, one level at a time
        static int shownWave = -1; static double waveAt;
        static bool Has(string flag) { return System.Array.IndexOf(System.Environment.GetCommandLineArgs(), flag) >= 0; }
        static PlayModeShots() { if (Has("-shot")) EditorApplication.update += Tick; }

        static void Tick()
        {
            if (EditorApplication.timeSinceStartup > 420) EditorApplication.Exit(1);   // never hang a terminal
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode) { EditorSceneManager.OpenScene("Assets/_Wintermarkt/Scenes/Market.unity"); EditorApplication.isPlaying = true; }
                return;
            }
            var game = Object.FindAnyObjectByType<MarketGame>(); if (!game || game.Simulation == null) return;
            var cam = Camera.main; var sim = game.Simulation; Time.timeScale = 8; string reason;
            // `-demo` (MarketGame) has placed a Punsch Hütte and a bakery; add a crafts stall so some visitors can be happy.
            if (!placed) { placed = true; var e = sim.Site.Entrance; for (int d = 4; d < 14 && sim.Buildings.Count < 3; d++) for (int x = -d; x <= d && sim.Buildings.Count < 3; x++) sim.TryPlace("craft", new Cell(e.x + x, e.y + d), out reason); }

            if (sim.Wave == 0 && sim.Remaining < 95 && !done.Contains("market"))
            {
                Vector3 home = cam.transform.position; float zoom = cam.orthographicSize; Save(cam, "market");
                foreach (var v in sim.Visitors) if (v.State == VisitorState.Walking || v.State == VisitorState.Leaving) { cam.transform.position = game.World.World(v.Position) + Vector3.up * 1.5f - cam.transform.forward * 220; cam.orthographicSize = 7; Save(cam, "visitor"); break; }
                cam.transform.position = new Vector3(-30, 0, -60) - cam.transform.forward * 220; cam.orthographicSize = 110; Save(cam, "park");
                cam.transform.position = home; cam.orthographicSize = zoom;
            }
            // Issue 44: upgrade each kind of building in turn and picture every level close up, plus one frame mid-pop. The three stalls stand from the start;
            // a carousel and a wheel are placed when their turn comes. An evening rarely affords all that, so the coins are topped up through the private
            // setter whenever they run short: this is a picture, not a playtest. The wheel waits until enough visitors have been welcomed to unlock it.
            if (done.Contains("market") && !sim.Finished)
            {
                double since = EditorApplication.timeSinceStartup - upgradedAt;
                if (upgraded != null && since > .2 && since < .5) Close(cam, game, upgraded, "upgrade-pop-" + upgraded.Definition.id + "-" + (upgraded.Level + 1));
                if (upgraded == null || since > 1.5)
                {
                    if (upgraded != null) Close(cam, game, upgraded, upgraded.Definition.id + "-" + (upgraded.Level + 1));
                    MarketBuilding next = null; string missing = null;
                    foreach (string id in order)
                    {
                        MarketBuilding found = null; foreach (var b in sim.Buildings) if (b.Definition.id == id) found = b;
                        if (found == null) { missing = id; break; }
                        if (found.Level < found.Definition.levels.Length - 1) { next = found; break; }
                    }
                    if (next != null)
                    {
                        Close(cam, game, next, next.Definition.id + "-" + (next.Level + 1));
                        if (sim.TryUpgrade(next.Id, out reason)) { upgraded = next; upgradedAt = EditorApplication.timeSinceStartup; }
                        else if (reason == "Not enough coins.") typeof(MarketSimulation).GetProperty("Coins").SetValue(sim, 1500);
                    }
                    else if (missing != null)
                    {
                        typeof(MarketSimulation).GetProperty("Coins").SetValue(sim, 1500);
                        var e = sim.Site.Entrance; int had = sim.Buildings.Count;
                        for (int d = 4; d < 14 && sim.Buildings.Count == had; d++) for (int x = -d; x <= d && sim.Buildings.Count == had; x++) sim.TryPlace(missing, new Cell(e.x + x, e.y + d), out reason);
                    }
                }
            }
            // Issue 47: the day card in its hold phase; it runs on unscaled time, so about 1 to 2.5 s after the day starts whatever the time scale.
            if (sim.Wave != shownWave) { shownWave = sim.Wave; waveAt = EditorApplication.timeSinceStartup; }
            double sinceDay = EditorApplication.timeSinceStartup - waveAt;
            if (sinceDay > 1.2 && sinceDay < 2.4) Save(cam, "day-" + (sim.Wave + 1));
            // Issue 14: a queue of three or more, close up, to see that it stands in joining order.
            if (sinceDay > 3.5) foreach (var b in sim.Buildings) if (sim.QueueCount(b) >= 3) { Close(cam, game, b, "queue"); break; }   // after the day card has gone
            MarketBuilding calling = null; foreach (var b in sim.Buildings) if (b.NeedsHelp) calling = b;
            int hunting = 0; foreach (var k in sim.Krampuses) if (!k.Fleeing) hunting++;
            if (calling != null && calling.HelpRemaining < 6) Save(cam, "help-call");
            if (sim.Upcoming != null) Save(cam, "event-warning");
            if (sim.ActiveEvent != null && sim.EventRemaining < sim.ActiveEvent.seconds - 8) Save(cam, "snowstorm");
            if (sim.SnowPiles.Count >= 4) Save(cam, "snow-piles");
            if (sim.LastEvent != null && sim.LastEvent.kind == "powercut" && calling != null && calling.HelpRemaining < 9) Save(cam, "power-cut");
            if (hunting >= 2 && sim.Scared > 1) Save(cam, "krampus");

            MarketBuilding empty = null, cooking = null;
            foreach (var b in sim.Buildings) { if (b.CookRemaining > 0 && b.CookRemaining < b.Stats.serviceSeconds * .5f) cooking = b; if (b.Stock == 0 && b.CookRemaining <= 0 && sim.QueueCount(b) > 1) empty = b; }
            if (empty != null) Save(cam, "tap-me");
            if (cooking != null && done.Contains("tap-me")) Save(cam, "cooking");
            if (Time.frameCount % 6 == 0) foreach (var b in sim.Buildings) if (b.Stock == 0 && b.CookRemaining <= 0 && sim.QueueCount(b) > 0 && sim.TryCook(b.Id, out reason)) break;
            // Play like a person who is a little late: help near the end of the ring, sweep and throw every few frames.
            foreach (var b in sim.Buildings) if (b.NeedsHelp && b.HelpRemaining < 4) sim.TryHelp(b.Id, out reason);
            if (done.Contains("snow-piles") && Time.frameCount % 10 == 0) foreach (Cell pile in sim.SnowPiles) { sim.TryClear(pile, out reason); break; }
            if (done.Contains("krampus") && Time.frameCount % 12 == 0) foreach (var k in sim.Krampuses) if (!k.Fleeing) { sim.TryHit(k.Position, out reason); break; }

            if (sim.Finished && endAt == 0) endAt = EditorApplication.timeSinceStartup + 4;   // let the fireworks start
            if (sim.Finished && EditorApplication.timeSinceStartup > endAt) { Save(cam, "ending"); EditorApplication.Exit(0); }
        }

        static void Close(Camera cam, MarketGame game, MarketBuilding b, string name)
        {
            if (done.Contains(name)) return;
            Vector3 home = cam.transform.position; float zoom = cam.orthographicSize;
            float cell = game.World.CellSize;   // the origin is the near-left cell; aim at the middle of the footprint
            cam.transform.position = game.World.World(b.Origin) + new Vector3((b.Definition.width - 1) * cell / 2, 3, -(b.Definition.depth - 1) * cell / 2) - cam.transform.forward * 220; cam.orthographicSize = 13; Save(cam, name);
            cam.transform.position = home; cam.orthographicSize = zoom;
        }

        static void Save(Camera cam, string name)
        {
            if (!done.Add(name)) return;
            // The HUD is a screen overlay, which a camera render does not include: draw it through the camera for the picture.
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 2; }
            Canvas.ForceUpdateCanvases();
            var target = new RenderTexture(1600, 900, 24); cam.targetTexture = target; cam.Render();
            RenderTexture.active = target; var picture = new Texture2D(1600, 900, TextureFormat.RGB24, false); picture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); picture.Apply();
            cam.targetTexture = null; RenderTexture.active = null;
            Directory.CreateDirectory("Logs/shots"); File.WriteAllBytes("Logs/shots/" + name + ".png", picture.EncodeToPNG());
        }
    }
}
