using System;
using System.Collections.Generic;
using UnityEngine;
namespace EchoMaze {
[Serializable] public class Cell { public int x,y; public Vector2Int Point => new Vector2Int(x,y); }
[Serializable] public class Patrol { public int spawnX,spawnY; public Cell[] waypoints; }
[Serializable] public class LevelData { public int id; public string title,hint; public string[] rows; public Patrol[] enemyPatrols; }
public sealed class GridMap {
 public readonly LevelData Data; public int Width=>Data.rows[0].Length; public int Height=>Data.rows.Length;
 public Vector2Int Start,Exit; public readonly List<Vector2Int> Enemies=new List<Vector2Int>();
 public GridMap(LevelData data) { Data=data; Validate(); }
 public bool Floor(Vector2Int c)=>c.x>=0&&c.y>=0&&c.x<Width&&c.y<Height&&Data.rows[c.y][c.x]!='#';
 public static Vector2 World(Vector2Int c)=>new Vector2(c.x,-c.y);
 public static Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.RoundToInt(p.x),Mathf.RoundToInt(-p.y));
 public List<Vector2Int> Path(Vector2Int start,Vector2Int end) {
  if(!Floor(start)||!Floor(end)) return null;
  var queue=new Queue<Vector2Int>(); var parent=new Dictionary<Vector2Int,Vector2Int>(); queue.Enqueue(start);parent[start]=start;
  var dirs=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
  while(queue.Count>0) { var c=queue.Dequeue(); if(c==end) { var path=new List<Vector2Int>(); while(c!=start){path.Add(c);c=parent[c];}path.Reverse();return path; }
   foreach(var d in dirs){var next=c+d;if(Floor(next)&&!parent.ContainsKey(next)){parent[next]=c;queue.Enqueue(next);}} }
  return null;
 }
 void Validate(){
  if(Data==null||Data.rows==null||Data.rows.Length<3||string.IsNullOrEmpty(Data.rows[0]))throw new ArgumentException("Level requires at least three nonempty rows.");
  int players=0,exits=0;
  for(int y=0;y<Height;y++){if(Data.rows[y]==null||Data.rows[y].Length!=Width)throw new ArgumentException("Nonrectangular row "+y);
   for(int x=0;x<Width;x++){char c=Data.rows[y][x];var point=new Vector2Int(x,y);if("#.PEX".IndexOf(c)<0)throw new ArgumentException("Unsupported tile "+c);
    if((x==0||y==0||x==Width-1||y==Height-1)&&c!='#')throw new ArgumentException("Map border must be closed.");
    if(c=='P'){Start=point;players++;}if(c=='X'){Exit=point;exits++;}if(c=='E')Enemies.Add(point);}}
  if(players!=1||exits!=1)throw new ArgumentException("Exactly one P and X required.");
  if(Path(Start,Exit)==null)throw new ArgumentException("Exit unreachable.");
  if(Data.enemyPatrols==null||Data.enemyPatrols.Length!=Enemies.Count)throw new ArgumentException("Enemy count does not match patrols.");
  for(int i=0;i<Enemies.Count;i++){var p=Data.enemyPatrols[i];if(p==null||new Vector2Int(p.spawnX,p.spawnY)!=Enemies[i])throw new ArgumentException("Patrol spawn/order mismatch.");
   if(p.waypoints==null||p.waypoints.Length==0)throw new ArgumentException("Patrol needs waypoints.");
   foreach(var w in p.waypoints)if(w==null||Path(Enemies[i],w.Point)==null)throw new ArgumentException("Patrol waypoint unreachable.");}
 }
 public static GridMap Load(int n){var asset=Resources.Load<TextAsset>("Levels/level_"+n.ToString("00"));if(!asset)throw new ArgumentException("Missing level "+n);return new GridMap(JsonUtility.FromJson<LevelData>(asset.text));}
}
public sealed class SonarEnergy {
 public float Current {get;private set;} public readonly GameSettings Settings; float ready;
 public SonarEnergy(GameSettings settings){Settings=settings;Current=settings.maxEnergy;}
 public bool Fire(float now){if(now<ready||Current<Settings.energyCost)return false;Current-=Settings.energyCost;ready=now+Settings.cooldown;return true;}
 public void Tick(float dt){Current=Mathf.Clamp(Current+Settings.recharge*Mathf.Max(0,dt),0,Settings.maxEnergy);}
}
}
