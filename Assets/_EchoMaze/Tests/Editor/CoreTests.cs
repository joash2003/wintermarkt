using System;
using NUnit.Framework;
using UnityEngine;
namespace EchoMaze.Tests {
public class CoreTests {
 [TestCase(1,13,9,0)] [TestCase(2,17,11,1)] [TestCase(3,21,13,2)]
 public void CompleteLevelsValidate(int n,int width,int height,int enemies){var map=GridMap.Load(n);Assert.AreEqual(width,map.Width);Assert.AreEqual(height,map.Height);Assert.AreEqual(enemies,map.Enemies.Count);var path=map.Path(map.Start,map.Exit);Assert.IsNotNull(path);foreach(var c in path)Assert.IsTrue(map.Floor(c));Assert.AreEqual(map.Exit,path[path.Count-1]);}
 LevelData Data(params string[] rows)=>new LevelData{rows=rows,enemyPatrols=Array.Empty<Patrol>()};
 [Test] public void ZeroStep(){var map=new GridMap(Data("#####","#P.X#","#####"));Assert.IsEmpty(map.Path(map.Start,map.Start));}
 [Test] public void UnreachableAndWallTargets(){var map=new GridMap(Data("#######","#P.X#.#","#######"));Assert.IsNull(map.Path(map.Start,new Vector2Int(5,1)));Assert.IsNull(map.Path(map.Start,Vector2Int.zero));}
 [TestCase("#..X#")] [TestCase("#P..#")] [TestCase("#PPX#")] [TestCase("#P?X#")]
 public void RejectsInvalidSymbolsAndSpawns(string middle){Assert.Throws<ArgumentException>(()=>new GridMap(Data("#####",middle,"#####")));}
 [Test] public void RejectsRagged(){Assert.Throws<ArgumentException>(()=>new GridMap(Data("#####","#PX#","#####")));}
 [Test] public void RejectsUnreachableExit(){Assert.Throws<ArgumentException>(()=>new GridMap(Data("#######","#P#..X#","#######")));}
 [Test] public void RejectsMissingPatrol(){Assert.Throws<ArgumentException>(()=>new GridMap(Data("######","#PEX.#","######")));}
 [TestCase(5.999f,true)] [TestCase(6f,true)] [TestCase(6.001f,false)]
 public void HearingBoundary(float distance,bool expected){Assert.AreEqual(expected,EnemyController.CanHear(Vector2.zero,new Vector2(distance,0),6));}
 [Test] public void EnergyCostCooldownAndRegen(){var s=ScriptableObject.CreateInstance<GameSettings>();try{var e=new SonarEnergy(s);Assert.IsTrue(e.Fire(0));Assert.AreEqual(70,e.Current);Assert.IsFalse(e.Fire(.9f));Assert.AreEqual(70,e.Current);Assert.IsTrue(e.Fire(1));Assert.IsTrue(e.Fire(2));Assert.IsFalse(e.Fire(3));Assert.AreEqual(10,e.Current);e.Tick(1);Assert.AreEqual(18,e.Current);e.Tick(100);Assert.AreEqual(100,e.Current);e.Tick(-1);Assert.AreEqual(100,e.Current);}finally{UnityEngine.Object.DestroyImmediate(s);}}
}
}
