using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Checks imported Technique identities, live gauge states and matched action controls with real prefab renders.</summary>
public static class JournalControlSyncChecks
{
    private const string Root=MidnightFieldJournalAssetSetup.Root,Output="Docs/UIThemes/MidnightFieldJournal/Sync005/Regression/";
    private static readonly BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic;
    private static int assertions;
    [Serializable] private sealed class Report { public bool passed,sceneUnchanged;public int assertions,techniques=8; }

    /// <summary>Exercises current candidate prefabs in a disposable scene and preserves all earlier validation evidence.</summary>
    public static void Run()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Validate outside Play Mode.");
        Directory.CreateDirectory(Output);assertions=0;Scene active=SceneManager.GetActiveScene();bool dirty=active.isDirty;Scene scene=EditorSceneManager.NewPreviewScene();GameObject hud=null,roster=null;
        try
        {
            hud=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/MidnightFieldJournalCanvas.prefab"));SceneManager.MoveGameObjectToScene(hud,scene);
            var canvas=hud.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;hud.GetComponent<CanvasScaler>().enabled=false;hud.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);hud.transform.position=Vector3.zero;
            foreach(string method in new[]{"Refresh","Populate"})typeof(JournalGameplayHudChecks).GetMethod(method,BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,method=="Refresh"?new object[]{hud,true}:new object[]{hud});
            Canvas.ForceUpdateCanvases();var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
            Require(theme.Sprites.Count==MidnightFieldJournalAssetSetup.LoadCatalog().sprites.Length,"Source mapping count");
            var meter=hud.GetComponentInChildren<LineLoadMeterView>();var rail=(RectTransform)new SerializedObject(meter).FindProperty("pipRoot").objectReferenceValue;
            foreach(int cap in new[]{0,1,4,8,10,16,32,64})foreach(int load in new[]{0,cap,cap+2})
            {
                meter.SetLoad(load,cap);Canvas.ForceUpdateCanvases();var segments=rail.GetComponentsInChildren<JournalLoadSegment>();
                Require(segments.Length==cap,"Capacity not data-driven");Require(Label(meter.gameObject,"Load Text").text==load+" / "+cap,"Overloaded numeral clamped");
                Require(Label(meter.gameObject,"Load Status").text.Contains(load>cap?"OVERLOADED":cap>0&&load==cap?"AT CAPACITY":"WITHIN CAPACITY"),"State text");
                foreach(var segment in segments)
                {
                    RectTransform r=segment.rectTransform;Require(r.anchoredPosition.y==0&&r.anchoredPosition.x>=0&&r.anchoredPosition.x+r.rect.width<=rail.rect.width+.1f,"Rail overflow or wrapping");
                    Require(!segment.raycastTarget&&r.rect.width>0,"Invisible or input-blocking segment");
                    if(load>cap)Require(segment.color==new SerializedObject(meter).FindProperty("dangerColor").colorValue,"Missing overload ink");
                }
            }
            meter.SetLoad(4,8);Require(Label(hud,"Haul Value").text=="21","Gauge changed haul");
            var actions=hud.GetComponent<JournalGameplayHud>();Rect a=actions.DescendButton.GetComponent<RectTransform>().rect,b=actions.SurfaceButton.GetComponent<RectTransform>().rect;
            Require(Vector2.Distance(a.size,b.size)<.1f,"Action sizes differ");
            var events=new GameObject("Sync input",typeof(EventSystem));SceneManager.MoveGameObjectToScene(events,scene);var pointer=new PointerEventData(events.GetComponent<EventSystem>());
            foreach(var button in new[]{actions.DescendButton,actions.SurfaceButton})
            {
                var image=button.transform.Find("Static Button Face").GetComponent<UnityEngine.UI.Image>();Require(image.sprite.texture.width==880&&image.sprite.texture.height==350,"Wrong static face");
                Require(button.GetComponentsInChildren<TMP_Text>().Length==0,"Baked title duplicated");Require(!image.raycastTarget&&button.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i=>i.raycastTarget&&i.color.a==0),"Action hit bounds");
                button.GetComponent<JournalActionFeedback>().OnPointerEnter(pointer);Require(!button.transform.Find("Focus Edge").GetComponent<JournalCardOutline>().enabled,"Hover frame returned");button.GetComponent<JournalActionFeedback>().OnPointerExit(pointer);
            }
            var cards=Cards();var ids=new[]{"clear_water","hold_steady","rerig","recast","cut_loose","slip_past","steady_line","prize_hook"};
            var hand=hud.GetComponentInChildren<TechniqueHandView>();hand.Refresh(ids.Take(4).Select(id=>cards.Single(c=>c.UniqueId==id)).ToArray(),new[]{true,true,true,true},new[]{"","","",""},8,0,i=>true,1);
            hud.GetComponentInChildren<JournalTechniqueHandPresentation>().RenderNow();
            Capture(hud,scene,3840,2160,"Gameplay4K.png");Capture(hud,scene,1920,1080,"Gameplay1080p.png");
            hud.SetActive(false);
            roster=new GameObject("Technique roster",typeof(RectTransform),typeof(Canvas));SceneManager.MoveGameObjectToScene(roster,scene);roster.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;roster.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);
            for(int i=0;i<ids.Length;i++)
            {
                var card=cards.Single(c=>c.UniqueId==ids[i]);Require(theme.TryGetArtwork(card.UniqueId,out Sprite art)&&art!=null,"Missing exact Technique art");
                var view=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<TechniqueCardView>(Root+"Prefabs/TechniqueCard.prefab"),roster.transform);var rect=view.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,1);rect.sizeDelta=new Vector2(620,640);rect.anchoredPosition=new Vector2(-1200+i%4*800,800-i/4*800);
                view.SetCard(card,"TECHNIQUE",true);view.SetArtwork(art);Canvas.ForceUpdateCanvases();
                var image=(UnityEngine.UI.Image)typeof(TechniqueCardView).GetField("artworkImage",Fields).GetValue(view);Require(image.enabled&&image.sprite==art,"Technique portrait hidden or substituted");
                foreach(var text in view.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate(true,true);Require(!text.isTextOverflowing,"Technique text overflow: "+card.DisplayName+" / "+text.name);}
            }
            Capture(roster,scene,3840,2160,"Techniques4K.png");Capture(roster,scene,1920,1080,"Techniques1080p.png");
        }
        finally
        {
            if(hud!=null)hud.GetComponentInChildren<JournalTechniqueHandPresentation>(true)?.ReleaseResources();EditorSceneManager.ClosePreviewScene(scene);
        }
        Require(SceneManager.GetActiveScene()==active&&active.isDirty==dirty,"Open scene changed");
        File.WriteAllText(Output+"ComponentVerification.json",JsonUtility.ToJson(new Report{passed=true,sceneUnchanged=true,assertions=assertions},true));Debug.Log("Current journal controls passed: "+assertions);
    }

    /// <summary>Renders actual Canvas pixels at one requested output resolution.</summary>
    private static void Capture(GameObject root,Scene scene,int width,int height,string file)
    {
        var obj=new GameObject("Sync capture",typeof(Camera));SceneManager.MoveGameObjectToScene(obj,scene);var camera=obj.GetComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.orthographicSize=1080;camera.transform.position=new Vector3(0,0,-100);camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(17,30,37,255);
        var target=new RenderTexture(width,height,24);var old=RenderTexture.active;Texture2D image=null;
        try{camera.targetTexture=target;foreach(var canvas in root.GetComponentsInChildren<Canvas>())canvas.worldCamera=camera;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+file,image.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);if(image!=null)UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(obj);}
    }
    /// <summary>Reads the authored card definitions without modifying deck membership.</summary>
    private static CardDefinition[] Cards(){return AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();}
    /// <summary>Finds an authored live field by its stable hierarchy name.</summary>
    private static TMP_Text Label(GameObject root,string name){return root.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name==name);}
    /// <summary>Stops at the first broken presentation contract.</summary>
    private static void Require(bool condition,string message){assertions++;if(!condition)throw new InvalidOperationException(message);}
}
