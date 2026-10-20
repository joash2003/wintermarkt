using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace EchoMaze.Editor {
public static class EchoMazeProjectBootstrap {
 const string Base="Assets/_EchoMaze/";
 [MenuItem("Tools/Echo Maze/Validate Levels")]
 public static void Validate(){for(int i=1;i<=3;i++){var map=GridMap.Load(i);Debug.Log("Validated level "+i+": "+map.Width+"x"+map.Height);}}
 [MenuItem("Tools/Echo Maze/Create or Repair Project")]
 public static void Create(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
 Directory.CreateDirectory(Base+"Scenes");Directory.CreateDirectory(Base+"Resources");
 if(!File.Exists(Base+"Resources/Square.png")){var tex=new Texture2D(16,16);var pixels=new Color[256];Array.Fill(pixels,Color.white);tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(Base+"Resources/Square.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(Base+"Resources/Square.png");var importer=(TextureImporter)AssetImporter.GetAtPath(Base+"Resources/Square.png");importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=16;importer.filterMode=FilterMode.Point;importer.SaveAndReimport();}
 if(!AssetDatabase.LoadAssetAtPath<GameSettings>(Base+"Resources/GameSettings.asset"))AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameSettings>(),Base+"Resources/GameSettings.asset");
 var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layer=tags.FindProperty("layers").GetArrayElementAtIndex(8);if(!string.IsNullOrEmpty(layer.stringValue)&&layer.stringValue!="Wall")throw new InvalidOperationException("Layer 8 is already occupied; preserve it and resolve Wall layer manually.");layer.stringValue="Wall";tags.ApplyModifiedProperties();
 var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedProperties();}
 PlayerSettings.companyName="EchoMaze Team";PlayerSettings.productName="Echo Maze";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
 foreach(var name in new[]{"MainMenu","Game","Victory"}){string path=Base+"Scenes/"+name+".unity";if(File.Exists(path))continue;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=7;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.015f,.025f,.04f);var manager=new GameObject("GameManager").AddComponent<GameManager>();manager.Settings=Resources.Load<GameSettings>("GameSettings");EditorSceneManager.SaveScene(scene,path);}
 EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Base+"Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(Base+"Scenes/Game.unity",true),new EditorBuildSettingsScene(Base+"Scenes/Victory.unity",true)};AssetDatabase.SaveAssets();Validate();EditorSceneManager.OpenScene(Base+"Scenes/MainMenu.unity");Debug.Log("Echo Maze bootstrap complete. Existing assets preserved.");}
}
public static class EchoMazeBuildAutomation {
 public static void BuildRelease(){EchoMazeProjectBootstrap.Create();Directory.CreateDirectory("Builds/EchoMaze_macOS");var report=UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Array.ConvertAll(EditorBuildSettings.scenes,s=>s.path),locationPathName="Builds/EchoMaze_macOS/EchoMaze.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.None});if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("Build succeeded: "+report.summary.outputPath);}
}
}
