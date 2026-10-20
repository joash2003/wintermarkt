using System.Collections.Generic;
using UnityEngine;
namespace EchoMaze {
public enum EnemyState {Patrol,Investigate,Chase}
public sealed class EnemyController:MonoBehaviour {
 GameManager game; Patrol patrol; public EnemyState State {get;private set;} Vector2 target; float lastSeen=-100,repath,waitUntil;int patrolIndex;List<Vector2Int> path;int step;Vector2Int anchor;
 public void Initialize(GameManager g,Patrol p){game=g;patrol=p;anchor=GridMap.Cell(transform.position);target=GridMap.World(p.waypoints[0].Point);}
 public static bool CanHear(Vector2 position,Vector2 origin,float radius)=>Vector2.Distance(position,origin)<=radius;
 public void Hear(Vector2 origin){if(State==EnemyState.Chase||!CanHear(transform.position,origin,game.Settings.hearingRadius))return;State=EnemyState.Investigate;target=origin;waitUntil=0;repath=0;game.Sound(1);}
 void Update(){if(!game.Playing)return;var s=game.Settings;Vector2 position=transform.position,player=game.Player.position;
  bool sees=Vector2.Distance(position,player)<=s.sightDistance&&!Physics2D.Linecast(position,player,1<<8);
  if(sees){if(State!=EnemyState.Chase)game.Sound(1);State=EnemyState.Chase;lastSeen=Time.time;target=player;waitUntil=0;}
  else if(State==EnemyState.Chase&&Time.time-lastSeen>s.chaseMemory){State=EnemyState.Investigate;waitUntil=0;}
  GetComponent<SpriteRenderer>().color=State==EnemyState.Chase?new Color(1,.1f,.35f):State==EnemyState.Investigate?new Color(1,.45f,.1f):Color.red;
  // Repath only at a tile center: changing the route mid-edge would cut wall corners.
  if(Vector2.Distance(position,GridMap.World(anchor))<.01f&&(path==null||step>=path.Count||Time.time>=repath)){
   path=game.Map.Path(anchor,GridMap.Cell(target));step=0;repath=Time.time+s.repathInterval;
   if(path==null){Debug.LogWarning("Enemy could not route to "+target+"; returning to patrol.",this);State=EnemyState.Patrol;target=GridMap.World(patrol.waypoints[patrolIndex].Point);return;}
  }
  if(path!=null&&step<path.Count){Vector2 next=GridMap.World(path[step]);float speed=State==EnemyState.Chase?s.chaseSpeed:State==EnemyState.Investigate?s.investigateSpeed:s.patrolSpeed;
   transform.position=Vector2.MoveTowards(position,next,speed*Time.deltaTime);if(Vector2.Distance(transform.position,next)<.001f){anchor=path[step++];}return;}
  if(State==EnemyState.Patrol){patrolIndex=(patrolIndex+1)%patrol.waypoints.Length;target=GridMap.World(patrol.waypoints[patrolIndex].Point);repath=0;}
  else if(State==EnemyState.Investigate){if(waitUntil==0)waitUntil=Time.time+s.investigateWait;if(Time.time>=waitUntil){State=EnemyState.Patrol;target=GridMap.World(patrol.waypoints[patrolIndex].Point);repath=0;waitUntil=0;}}
 }
}
}
