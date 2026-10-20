using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EchoMaze {
public enum GameState { MainMenu,Playing,Paused,Caught,Transitioning,Victory }
public sealed class GameManager:MonoBehaviour {
 public GameSettings Settings; public GameState State {get;private set;} public bool Playing=>State==GameState.Playing;
 public GridMap Map {get;private set;} public Transform Player {get;private set;} public SonarEnergy Energy {get;private set;}
 public int Level {get;private set;} public MazeUI UI; Transform root; Sprite square; AudioSource audioSource; AudioClip[] tones;
 readonly List<SpriteRenderer> fog=new List<SpriteRenderer>(); float[] expiry; float fogTick; readonly List<EnemyController> enemies=new List<EnemyController>();
 public bool Muted {get;private set;}
 void Start(){if(!Settings)Settings=Resources.Load<GameSettings>("GameSettings");if(!Settings)throw new System.InvalidOperationException("Run Tools > Echo Maze > Create/Repair Project.");
  square=Resources.Load<Sprite>("Square");audioSource=gameObject.AddComponent<AudioSource>();tones=new AudioClip[5];for(int i=0;i<5;i++)tones[i]=Tone(260+i*160);
  Muted=PlayerPrefs.GetInt("Muted",0)!=0;AudioListener.volume=Muted?0:1;
  UI=gameObject.AddComponent<MazeUI>();UI.Initialize(this);
  if(SceneManager.GetActiveScene().name=="Game")LoadLevel(1);else SetState(SceneManager.GetActiveScene().name=="Victory"?GameState.Victory:GameState.MainMenu);
 }
 public void SetState(GameState state){State=state;Time.timeScale=state==GameState.Paused?0:1; if(Player){var body=Player.GetComponent<Rigidbody2D>();if(state!=GameState.Playing)body.linearVelocity=Vector2.zero;}UI.Refresh();}
 public void Play(){LoadLevel(1);} public void Retry(){LoadLevel(Level);} public void Menu(){Clear();SetState(GameState.MainMenu);}
 public void TogglePause(){if(Playing)SetState(GameState.Paused);else if(State==GameState.Paused)SetState(GameState.Playing);}
 public void ToggleMute(){Muted=!Muted;AudioListener.volume=Muted?0:1;PlayerPrefs.SetInt("Muted",Muted?1:0);UI.Refresh();}
 void Clear(){Time.timeScale=1;if(root){root.gameObject.SetActive(false);Destroy(root.gameObject);}fog.Clear();enemies.Clear();Player=null;}
 public void LoadLevel(int n){SetState(GameState.Transitioning);Clear();Level=n;Map=GridMap.Load(n);Energy=new SonarEnergy(Settings);root=new GameObject("RuntimeLevelRoot").transform;
  expiry=new float[Map.Width*Map.Height];
  for(int y=0;y<Map.Height;y++)for(int x=0;x<Map.Width;x++) {var c=new Vector2Int(x,y);bool wall=!Map.Floor(c);var tile=Tile(wall?"Wall":"Floor",GridMap.World(c),wall?new Color(.18f,.27f,.37f):new Color(.035f,.075f,.105f),wall?10:0,.98f);
   if(wall){tile.gameObject.layer=8;tile.gameObject.AddComponent<BoxCollider2D>();}
   fog.Add(Tile("Fog",GridMap.World(c),new Color(0,0,0,Settings.fogBase),30,1));}
  Tile("Exit",GridMap.World(Map.Exit),new Color(.25f,1,.5f),15,.75f);
  var player=Tile("Player",GridMap.World(Map.Start),Color.cyan,40,Settings.playerSize);Player=player.transform;
  var body=player.gameObject.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.interpolation=RigidbodyInterpolation2D.Interpolate;
  player.gameObject.AddComponent<BoxCollider2D>();player.gameObject.AddComponent<PlayerController>().Game=this;
  for(int i=0;i<Map.Enemies.Count;i++){var sprite=Tile("Enemy",GridMap.World(Map.Enemies[i]),Color.red,20,.6f);var enemy=sprite.gameObject.AddComponent<EnemyController>();enemy.Initialize(this,Map.Data.enemyPatrols[i]);enemies.Add(enemy);}
  var camera=Camera.main;camera.transform.position=new Vector3((Map.Width-1)/2f,-(Map.Height-1)/2f,-10);camera.orthographicSize=Mathf.Max((Map.Height+3)/2f,(Map.Width+2)/(2f*camera.aspect));
  fogTick=0;SetState(GameState.Playing);UpdateFog();
 }
 SpriteRenderer Tile(string name,Vector2 position,Color color,int order,float size){var o=new GameObject(name);o.transform.SetParent(root);o.transform.position=position;o.transform.localScale=Vector3.one*size;var r=o.AddComponent<SpriteRenderer>();r.sprite=square;r.color=color;r.sortingOrder=order;return r;}
 void Update(){if(Input.GetKeyDown(KeyCode.M))ToggleMute();if(Input.GetKeyDown(KeyCode.Escape))TogglePause();if(!Playing)return;
  if(Input.GetKeyDown(KeyCode.R)){Retry();return;}Energy.Tick(Time.deltaTime);if(Time.time>=fogTick){UpdateFog();fogTick=Time.time+.1f;}
  // Resolve capture before exit so there is exactly one outcome per frame.
  foreach(var enemy in enemies)if(Vector2.Distance(enemy.transform.position,Player.position)<=Settings.catchDistance){Sound(2);SetState(GameState.Caught);return;}
  if(Vector2.Distance(Player.position,GridMap.World(Map.Exit))<=.45f){Sound(3);SetState(GameState.Transitioning);if(Level<3)LoadLevel(Level+1);else{Clear();SetState(GameState.Victory);}}
 }
 public void Pulse(){if(!Playing||!Energy.Fire(Time.time))return;Vector2 origin=Player.position;
  for(int i=0;i<fog.Count;i++)if(Vector2.Distance(fog[i].transform.position,origin)<=Settings.revealRadius)expiry[i]=Mathf.Max(expiry[i],Time.time+Settings.revealDuration);
  var ring=new GameObject("SonarRing");ring.transform.SetParent(root);ring.transform.position=origin;ring.AddComponent<SonarPulseRing>().Initialize(Settings);foreach(var e in enemies)e.Hear(origin);Sound(0);UpdateFog();}
 void UpdateFog(){for(int i=0;i<fog.Count;i++){float alpha=expiry[i]>Time.time?Settings.fogRevealed:Vector2.Distance(fog[i].transform.position,Player.position)<=Settings.localRadius?Settings.fogNear:Settings.fogBase;fog[i].color=new Color(0,0,0,alpha);}}
 public void Sound(int i){audioSource.PlayOneShot(tones[i],.16f);} static AudioClip Tone(float hz){int count=11025;var data=new float[count];for(int i=0;i<count;i++)data[i]=Mathf.Sin(2*Mathf.PI*hz*i/44100f)*Mathf.Pow(1-i/(float)count,3);var clip=AudioClip.Create("Original beep",count,1,44100,false);clip.SetData(data,0);return clip;}
 void OnDestroy(){Time.timeScale=1;if(tones!=null)foreach(var tone in tones)Destroy(tone);}
}
}
