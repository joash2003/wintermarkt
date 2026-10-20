using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace EchoMaze {
public sealed class MazeUI:MonoBehaviour {
 GameManager game; RectTransform canvas; GameObject panel; Text energy; Font font;
 public void Initialize(GameManager manager){game=manager;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");var obj=new GameObject("UI Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));obj.transform.SetParent(transform);obj.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas=obj.GetComponent<RectTransform>();var scaler=obj.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
 if(!FindFirstObjectByType<EventSystem>()){var events=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform);}}
 Text Label(string value,Transform parent,float y,int size=24){var obj=new GameObject("Label",typeof(RectTransform),typeof(Text));obj.transform.SetParent(parent,false);var rect=obj.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(1000,90);rect.anchoredPosition=new Vector2(0,y);var text=obj.GetComponent<Text>();text.font=font;text.fontSize=size;text.color=new Color(.75f,.94f,1);text.alignment=TextAnchor.MiddleCenter;text.text=value;return text;}
 void Button(string title,float y,Action action){var obj=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));obj.transform.SetParent(panel.transform,false);var rect=obj.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(340,48);rect.anchoredPosition=new Vector2(0,y);obj.GetComponent<Image>().color=new Color(.06f,.22f,.29f,.98f);obj.GetComponent<Button>().onClick.AddListener(()=>{game.Sound(4);action();});var label=Label(title,obj.transform,0,22);label.rectTransform.sizeDelta=rect.sizeDelta;}
 public void Refresh(){if(!canvas)return;if(panel){panel.SetActive(false);Destroy(panel);}panel=new GameObject("State "+game.State,typeof(RectTransform));panel.transform.SetParent(canvas,false);var root=panel.GetComponent<RectTransform>();root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;energy=null;
 if(game.Playing){var title=Label("LEVEL "+game.Level+" / 3   ·   "+game.Map.Data.title,panel.transform,310,25);energy=Label("",panel.transform,275,20);Label(game.Map.Data.hint+"\nESC pause · R retry · M sound",panel.transform,-310,19);return;}
 var background=panel.AddComponent<Image>();background.color=new Color(.015f,.03f,.055f,.96f);
 switch(game.State){case GameState.MainMenu:Label("ECHO MAZE",panel.transform,220,56);Label("Escape the Darkness",panel.transform,155,25);Button("Play",65,game.Play);Button("How to play",5,Instructions);Button(game.Muted?"Sound: OFF":"Sound: ON",-55,game.ToggleMute);Button("Quit",-115,()=>Application.Quit());break;
 case GameState.Paused:Label("PAUSED",panel.transform,180,44);Button("Resume",60,game.TogglePause);Button("Retry",0,game.Retry);Button("Main menu",-60,game.Menu);break;
 case GameState.Caught:Label("CAUGHT",panel.transform,180,44);Label("The enemy found you. Try a different timing or route.",panel.transform,115,22);Button("Retry",0,game.Retry);Button("Main menu",-60,game.Menu);break;
 case GameState.Victory:Label("YOU ESCAPED",panel.transform,180,48);Label("Your last transmission made it through.",panel.transform,110,23);Button("Play again",0,game.Play);Button("Main menu",-60,game.Menu);break;}}
 void Instructions(){Refresh();foreach(Transform child in panel.transform)Destroy(child.gameObject);Label("HOW TO PLAY",panel.transform,230,40);Label("WASD / arrows: move     SPACE: sonar\nSonar reveals nearby corridors, enemies and the green exit.\nEach scan uses 30 energy. Energy recharges automatically.\nEnemies hear nearby scans and investigate where you scanned.\nWalls block enemy sight. Keep moving to escape.\nReach the green exit in all three levels.\nESC: pause     R: retry     M: toggle sound",panel.transform,20,23).rectTransform.sizeDelta=new Vector2(1050,360);Button("Back",-230,Refresh);}
 void Update(){if(energy&&game.Energy!=null)energy.text="SONAR  "+Mathf.FloorToInt(game.Energy.Current)+" / "+game.Settings.maxEnergy+"    ·    SPACE to scan";}
}
}
