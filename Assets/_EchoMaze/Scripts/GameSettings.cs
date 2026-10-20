using UnityEngine;
namespace EchoMaze {
[CreateAssetMenu(menuName="Echo Maze/Settings")]
public sealed class GameSettings:ScriptableObject {
 public float playerSpeed=3,playerSize=.55f,localRadius=1.2f,revealRadius=5,revealDuration=1.8f,ringDuration=.45f;
 public float maxEnergy=100,energyCost=30,recharge=8,cooldown=1;
 public float hearingRadius=6,patrolSpeed=1.5f,investigateSpeed=2,chaseSpeed=2.5f,sightDistance=2.7f,catchDistance=.55f,investigateWait=1.5f,chaseMemory=1.5f,repathInterval=.4f;
 [Range(0,1)] public float fogBase=.96f,fogNear=.35f,fogRevealed=.08f;
}
}
