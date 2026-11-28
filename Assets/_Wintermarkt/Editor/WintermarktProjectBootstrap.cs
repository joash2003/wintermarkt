using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wintermarkt;
using Wintermarkt.Core;

namespace Wintermarkt.Editor
{
    public static class WintermarktProjectBootstrap
    {
        const string SceneDirectory = "Assets/_Wintermarkt/Scenes";
        const string ScenePath = SceneDirectory + "/Market.unity";
        const string BlockoutMaterialPath = "Assets/_Wintermarkt/Resources/Wintermarkt/Blockout.mat";
        const string GlowMaterialPath = "Assets/_Wintermarkt/Resources/Wintermarkt/Glow.mat";

        [MenuItem("Tools/Wintermarkt/Validate Market Data")]
        public static void ValidateData()
        {
            var config = Resources.Load<TextAsset>("Wintermarkt/market");
            var site = Resources.Load<TextAsset>("Wintermarkt/site");
            if (!config || !site) throw new InvalidOperationException("Missing Assets/_Wintermarkt/Resources/Wintermarkt JSON.");
            if (!Resources.Load<Material>("Wintermarkt/Blockout") || !Resources.Load<Material>("Wintermarkt/Glow"))
                throw new InvalidOperationException("Missing " + BlockoutMaterialPath + " or " + GlowMaterialPath + ". Run Tools > Wintermarkt > Create or Repair Project.");
            JsonUtility.FromJson<MarketSettings>(config.text).Validate();
            var grid = new SiteGrid(JsonUtility.FromJson<SiteData>(site.text));
            if (grid.Path(grid.Entrance, grid.Entrance, new System.Collections.Generic.HashSet<Cell>()) == null)
                throw new InvalidOperationException("Site entrance is not walkable.");
            Debug.Log("Wintermarkt settings and site validated: " + grid.Width + "x" + grid.Height + " cells. " + grid.Data.geographyStatus);
        }

        [MenuItem("Tools/Wintermarkt/Create or Repair Project")]
        public static void CreateOrRepair()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(SceneDirectory);
            if (!File.Exists(BlockoutMaterialPath)) AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")), BlockoutMaterialPath);
            if (!File.Exists(GlowMaterialPath))
            {
                // Emission is a shader keyword: a build only keeps it if a shipped material already uses it.
                var glow = new Material(Shader.Find("Standard")); glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor", Color.white);
                glow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; AssetDatabase.CreateAsset(glow, GlowMaterialPath);
            }
            ValidateData();
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera"; var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.72f, .82f, .85f);
                camera.transform.position = new Vector3(0, 180, -210); camera.transform.rotation = Quaternion.Euler(55, 30, 0);
                var light = new GameObject("Winter daylight", typeof(Light)); light.GetComponent<Light>().type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -35, 0); light.GetComponent<Light>().intensity = 1.15f;
                new GameObject("Market Game", typeof(MarketGame));
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            var configured = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            configured.Add(new EditorBuildSettingsScene(ScenePath, true));
            foreach (var old in EditorBuildSettings.scenes)
                if (old.enabled && old.path != ScenePath && File.Exists(old.path)) configured.Add(old);
            EditorBuildSettings.scenes = configured.ToArray();
            PlayerSettings.productName = "Linz Wintermarkt";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Wintermarkt entry scene is ready. Market surroundings are a provisional blockout; review docs/LOCATION_AND_TRANSIT.md.");
        }

        [MenuItem("Tools/Wintermarkt/Build macOS Prototype")]
        public static void BuildRelease()
        {
            ValidateData();
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (!scene) throw new InvalidOperationException("Run Tools > Wintermarkt > Create or Repair Project first.");
            // The build is for the platform the editor runs on (issue 37): Windows on a Windows PC, macOS on a Mac.
            bool windows = Application.platform == RuntimePlatform.WindowsEditor;
            string folder = windows ? "Builds/LinzWintermarkt_Windows" : "Builds/LinzWintermarkt_macOS";
            Directory.CreateDirectory(folder);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = folder + (windows ? "/LinzWintermarkt.exe" : "/LinzWintermarkt.app"),
                target = windows ? BuildTarget.StandaloneWindows64 : BuildTarget.StandaloneOSX, options = BuildOptions.None
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Wintermarkt build failed: " + report.summary.result);
            Debug.Log("Wintermarkt build succeeded: " + report.summary.outputPath + " (" + report.summary.totalSize + " bytes)");
        }
    }
}
