using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Verifies the parchment controls, live depth guide and opt-in boat start against actual runtime owners.</summary>
public static class JournalLatestDesignChecks
{
    private const string Root="Assets/UIThemes/MidnightFieldJournal/";
    private static string Output="Docs/UIThemes/MidnightFieldJournal/Sync005/";
    private static int assertions;
    [Serializable] private sealed class Report { public bool passed; public int assertions; public bool sceneUnchanged; }

    /// <summary>Checks meaningful state transitions and renders the actual candidate canvas in a disposable scene.</summary>
    public static void Run(string output="Docs/UIThemes/MidnightFieldJournal/Sync005/")
    {
        if(Application.isPlaying)throw new InvalidOperationException("Validate in Edit Mode.");
        Output=output;Directory.CreateDirectory(Output);
        assertions=0;Scene active=SceneManager.GetActiveScene();bool dirty=active.isDirty;Scene preview=EditorSceneManager.NewPreviewScene();GameObject obj=null;
        try
        {
            obj=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/MidnightFieldJournalCanvas.prefab"));SceneManager.MoveGameObjectToScene(obj,preview);
            obj.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;obj.GetComponent<UnityEngine.UI.CanvasScaler>().enabled=false;obj.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);obj.transform.position=Vector3.zero;
            var helper=typeof(JournalGameplayHudChecks);foreach(string method in new[]{"Refresh","Populate"})helper.GetMethod(method,BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,method=="Refresh"?new object[]{obj,true}:new object[]{obj});
            Canvas.ForceUpdateCanvases();var hud=obj.GetComponent<JournalGameplayHud>();var guide=obj.GetComponentInChildren<JournalDepthGuide>();var biome=AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/Biomes/Coastal Waters.asset");
            foreach(int depth in new[]{0,7,8,12,15,16,20,21})
            {
                guide.Refresh(biome,depth);Canvas.ForceUpdateCanvases();var rows=guide.GetComponentsInChildren<JournalDepthTierView>();Require(rows.Length==biome.DepthTiers.Length,"Authored tier count");
                for(int i=0;i<rows.Length;i++)
                {
                    var label=rows[i].GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Authored Tier Name");Require(label.text==biome.DepthTiers[i].DisplayName.ToUpperInvariant(),"Tier label substitution");
                    bool current=biome.DepthTiers[i].ContainsDepth(depth);Require(label.color==(current?(Color)new Color32(243,200,136,255):new Color32(157,157,157,230)),"Stale active tier");
                    var engraving=rows[i].GetComponentsInChildren<UnityEngine.UI.Image>().Single();
                    Require(current?engraving.material==engraving.defaultMaterial:engraving.material.shader.name=="MidnightFieldJournal/UI/DepthGuideGrayscale","Tier saturation did not follow current depth");
                    Require(Mathf.Approximately(engraving.color.a,current?1:.42f),"Inactive tier opacity");
                    foreach(var t in rows[i].GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate(true,true);Require(!t.isTextOverflowing,"Depth guide text overflow");}
                    Require(rows[i].GetComponentsInChildren<UnityEngine.UI.Image>().Single().sprite!=null,"Missing tier engraving");
                    var node=rows[i].GetComponentInChildren<JournalDepthNode>();Require(node.GetComponent<CanvasRenderer>()!=null,"Marker renderer missing");
                }
            }
            guide.Refresh(null,0);Require(guide.GetComponentsInChildren<JournalDepthTierView>().Length==0,"Old biome rows leaked");
            var synthetic=UnityEngine.Object.Instantiate(biome);
            try
            {
                foreach(int count in new[]{0,1,2,6,12})
                {
                    var data=new SerializedObject(synthetic);var tiers=data.FindProperty("depthTiers");tiers.arraySize=count;
                    for(int i=0;i<count;i++){var row=tiers.GetArrayElementAtIndex(i);row.FindPropertyRelative("displayName").stringValue="AUTHORED TIER "+i;row.FindPropertyRelative("minimumDepth").intValue=i*10;row.FindPropertyRelative("maximumDepth").intValue=i*10+9;}
                    data.ApplyModifiedPropertiesWithoutUndo();guide.Refresh(synthetic,12);Require(guide.GetComponentsInChildren<JournalDepthTierView>().Length==count,"Guide has fixed tier count");
                    var positions=guide.GetComponentsInChildren<JournalDepthTierView>().Select(r=>((RectTransform)r.transform).anchoredPosition.y).ToArray();Require(positions.Distinct().Count()==count,"Tier rows overlap centers");
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(synthetic);}
            guide.Refresh(biome,12);var meter=obj.GetComponentInChildren<LineLoadMeterView>();var rail=(RectTransform)new SerializedObject(meter).FindProperty("pipRoot").objectReferenceValue;
            foreach(int cap in new[]{0,1,4,8,16,32,64})foreach(int load in new[]{0,cap,cap+2})
            {
                meter.SetLoad(load,cap);Canvas.ForceUpdateCanvases();var pips=rail.GetComponentsInChildren<JournalLoadSegment>();Require(pips.Length==cap,"Capacity is fixed");Require(Label(meter.gameObject,"Load Text").text==load+" / "+cap,"Load clamped");
                foreach(var pip in pips){var r=pip.rectTransform;Require(r.anchoredPosition.y==0&&r.anchoredPosition.x>=0&&r.anchoredPosition.x+r.rect.width<=rail.rect.width+.1f,"Pip bounds");}
                Require(Label(meter.gameObject,"Load Status").text.Contains(load>cap?"OVERLOADED":cap>0&&load==cap?"AT CAPACITY":"WITHIN CAPACITY"),"Load state");
            }
            meter.SetLoad(4,8);Require(Label(meter.gameObject,"Load Text").color==(Color)new Color32(38,53,57,255),"Parchment text unreadable");
            foreach(var b in new[]{hud.DescendButton,hud.SurfaceButton}){var face=b.transform.Find("Static Button Face").GetComponent<UnityEngine.UI.Image>();Require(face.sprite.texture.width==880&&face.sprite.texture.height==350,"Action face geometry");Require(b.GetComponentsInChildren<TMP_Text>().Length==0,"Duplicate static button label");}
            hud.SetOpeningVisible(true);var opening=obj.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Opening Boat Card").gameObject;Require(opening.activeSelf&&opening.GetComponentsInChildren<TMP_Text>().Length==0,"Opening has stats/text");Require(opening.GetComponentsInChildren<UnityEngine.UI.Image>().Length==2,"Opening art separation");
            Capture(obj,preview,3840,2160,"Opening4K.png");Capture(obj,preview,1920,1080,"Opening1080p.png");
            hud.SetOpeningVisible(false);Require(!opening.activeSelf,"Opening persists on encounter");Capture(obj,preview,3840,2160,"Gameplay4K.png");Capture(obj,preview,1920,1080,"Gameplay1080p.png");
            VerifyController(biome,true);VerifyController(biome,false);
        }
        finally{if(obj!=null)obj.GetComponentInChildren<JournalTechniqueHandPresentation>(true)?.ReleaseResources();EditorSceneManager.ClosePreviewScene(preview);}
        Require(SceneManager.GetActiveScene()==active&&active.isDirty==dirty,"Validation modified open scene");File.WriteAllText(Output+"Verification.json",JsonUtility.ToJson(new Report{passed=true,assertions=assertions,sceneUnchanged=true},true));Debug.Log("Latest journal designs passed "+assertions+" assertions.");
    }

    /// <summary>Exercises opt-in and legacy startup, first Descend, restart and Surface without scene dependencies.</summary>
    private static void VerifyController(BiomeDefinition biome,bool boat)
    {
        var go=new GameObject("Transient boat-start check");go.SetActive(false);
        try
        {
            var controller=go.AddComponent<FishingRunController>();var data=new SerializedObject(controller);data.FindProperty("startingBiome").objectReferenceValue=biome;data.FindProperty("beginWithBoatCard").boolValue=boat;data.FindProperty("useRandomSeed").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
            controller.StartRun();Require(controller.IsOpeningBoatVisible==boat,"Opening startup flag");Require((controller.CurrentEncounter==null)==boat,"Initial encounter reveal timing");Require(controller.CatchChain.Length==0,"Boat created a Catch");int start=controller.CurrentDepth;
            Require(controller.TryDescend(),"First Descend unavailable");Require(!controller.IsOpeningBoatVisible&&controller.CurrentDepth==start+1,"First Descend timing/depth");
            if(boat)Require(controller.CatchChain.Length==0&&controller.CurrentEncounter!=null,"First Descend committed an invisible creature");
            controller.StartRun();Require(controller.IsOpeningBoatVisible==boat,"Restart did not restore opening");Require(controller.TrySurface()&&!controller.IsOpeningBoatVisible&&!controller.RunActive,"Surface from opening");
        }
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
    /// <summary>Finds a named live field without depending on fixed hierarchy depths.</summary>
    private static TMP_Text Label(GameObject obj,string name){return obj.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name==name);}
    /// <summary>Stops on a failed contract before producing successful verification evidence.</summary>
    private static void Require(bool value,string message){assertions++;if(!value)throw new InvalidOperationException(message);}
    /// <summary>Renders actual Canvas and projected hand pixels at the requested output size.</summary>
    private static void Capture(GameObject root,Scene scene,int width,int height,string file)
    {
        var obj=new GameObject("Latest design capture",typeof(Camera));SceneManager.MoveGameObjectToScene(obj,scene);var camera=obj.GetComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.orthographicSize=1080;camera.transform.position=new Vector3(0,0,-100);camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(17,30,37,255);
        var target=new RenderTexture(width,height,24);var old=RenderTexture.active;Texture2D image=null;
        try{camera.targetTexture=target;foreach(var canvas in root.GetComponentsInChildren<Canvas>())canvas.worldCamera=camera;Canvas.ForceUpdateCanvases();root.GetComponentInChildren<JournalTechniqueHandPresentation>().RenderNow();camera.Render();RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+file,image.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);if(image!=null)UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(obj);}
    }
}
