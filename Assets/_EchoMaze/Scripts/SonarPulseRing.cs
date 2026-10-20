using UnityEngine;
namespace EchoMaze {
public sealed class SonarPulseRing:MonoBehaviour {
 LineRenderer line;GameSettings settings;float elapsed;Material material;
 public void Initialize(GameSettings s){settings=s;line=gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=64;line.widthMultiplier=.045f;line.sortingOrder=45;material=new Material(Shader.Find("Sprites/Default"));line.sharedMaterial=material;}
 void Update(){elapsed+=Time.deltaTime;float t=elapsed/settings.ringDuration;if(t>=1){Destroy(gameObject);return;}for(int i=0;i<64;i++){float angle=i*Mathf.PI*2/64;line.SetPosition(i,new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*settings.revealRadius*t);}line.startColor=line.endColor=new Color(0,1,1,1-t);}
 void OnDestroy(){if(material)Destroy(material);}
}
}
