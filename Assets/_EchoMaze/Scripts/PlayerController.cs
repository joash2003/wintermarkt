using UnityEngine;
namespace EchoMaze {
public sealed class PlayerController:MonoBehaviour {
 public GameManager Game; Rigidbody2D body; Vector2 direction;
 void Awake(){body=GetComponent<Rigidbody2D>();}
 void Update(){direction=Vector2.zero;if(!Game||!Game.Playing)return;
 direction=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0)).normalized;
 if(Input.GetKeyDown(KeyCode.Space))Game.Pulse();}
 void FixedUpdate(){body.linearVelocity=Game&&Game.Playing?direction*Game.Settings.playerSpeed:Vector2.zero;}
}
}
