using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Exercises newest-catch following and manual scroll retention through the real chain prefab.</summary>
public static class JournalChainFollowChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/CatchChainFollow/";
    private static int assertions;
    [Serializable] private sealed class Report { public bool passed,sceneUnchanged;public int assertions; }

    /// <summary>Verifies wheel/drag intent, new attachments, return-to-end, clearing and responsive extents.</summary>
    public static void Run()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Run component checks outside Play Mode.");
        Directory.CreateDirectory(Output);assertions=0;Scene active=SceneManager.GetActiveScene();bool dirty=active.isDirty;Scene scene=EditorSceneManager.NewPreviewScene();GameObject obj=null;
        var flags=BindingFlags.Static|BindingFlags.NonPublic;var helper=typeof(JournalLatestDesignChecks);var output=helper.GetField("Output",flags);var oldOutput=output.GetValue(null);
        try
        {
            obj=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UIThemes/MidnightFieldJournal/Prefabs/MidnightFieldJournalCanvas.prefab"));SceneManager.MoveGameObjectToScene(obj,scene);
            obj.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;obj.GetComponent<UnityEngine.UI.CanvasScaler>().enabled=false;obj.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);obj.transform.position=Vector3.zero;
            typeof(JournalGameplayHudChecks).GetMethod("Refresh",flags).Invoke(null,new object[]{obj,true});typeof(JournalGameplayHudChecks).GetMethod("Populate",flags).Invoke(null,new object[]{obj});
            var view=obj.GetComponentInChildren<CatchChainView>();var scroll=obj.GetComponentInChildren<JournalCatchChainScrollRect>();var chain=view.GetComponent<JournalCatchChainPresentation>();
            var card=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");Require(card!=null,"Fixture card missing");
            var catches=Enumerable.Range(1,16).Select(i=>new CardInstance(i,card)).ToArray();
            Action<int> refresh=count=>{view.Refresh(catches.Take(count).ToArray(),null,count*2,32,-1,i=>{});Canvas.ForceUpdateCanvases();};
            Action assertBottom=()=>Require(Mathf.Abs(scroll.content.anchoredPosition.y-Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height))<.5f,"Newest catch is hidden");
            refresh(0);refresh(8);Require(scroll.FollowingNewest,"Not following initially");assertBottom();refresh(9);assertBottom();
            var e=new PointerEventData(null){scrollDelta=new Vector2(0,3),button=PointerEventData.InputButton.Left};scroll.OnScroll(e);Require(!scroll.FollowingNewest,"Wheel-away intent ignored");float offset=scroll.content.anchoredPosition.y;refresh(10);Require(Mathf.Abs(scroll.content.anchoredPosition.y-offset)<.5f,"New catch stole manual scroll position");refresh(10);Require(Mathf.Abs(scroll.content.anchoredPosition.y-offset)<.5f,"Stat refresh stole manual scroll position");
            e.scrollDelta=new Vector2(0,-10000);scroll.OnScroll(e);Require(scroll.FollowingNewest,"Returning to bottom did not resume following");refresh(11);assertBottom();
            scroll.OnInitializePotentialDrag(e);scroll.OnBeginDrag(e);Require(!scroll.FollowingNewest,"Drag did not suspend following");offset=scroll.content.anchoredPosition.y;refresh(12);Require(Mathf.Abs(scroll.content.anchoredPosition.y-offset)<.5f,"Append moved an active drag");scroll.OnEndDrag(e);Require(!scroll.FollowingNewest,"Finishing away from bottom resumed following");
            e.scrollDelta=new Vector2(0,-10000);scroll.OnScroll(e);refresh(13);assertBottom();
            Vector2 oldSize=scroll.viewport.sizeDelta;scroll.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,scroll.viewport.rect.height-100);refresh(13);assertBottom();scroll.viewport.sizeDelta=oldSize;refresh(13);assertBottom();
            refresh(1);Require(scroll.content.anchoredPosition.y==0&&scroll.FollowingNewest,"Short chain did not reset following");refresh(8);assertBottom();
            e.scrollDelta=new Vector2(0,10000);scroll.OnScroll(e);Require(!scroll.FollowingNewest,"Manual top inspection ignored");refresh(0);refresh(12);assertBottom();Require(scroll.FollowingNewest,"New run retained stale manual override");
            var last=chain.Cards[11].GetComponent<RectTransform>();Vector3[] corners=new Vector3[4];last.GetWorldCorners(corners);float min=corners.Min(c=>scroll.viewport.InverseTransformPoint(c).y),max=corners.Max(c=>scroll.viewport.InverseTransformPoint(c).y);Require(min>=scroll.viewport.rect.yMin-.5f&&max<=scroll.viewport.rect.yMax+.5f,"Newest card not fully visible");
            output.SetValue(null,Output);foreach(int width in new[]{3840,1920})helper.GetMethod("Capture",flags).Invoke(null,new object[]{obj,scene,width,width*9/16,width==3840?"NewestCatch4K.png":"NewestCatch1080p.png"});
        }
        finally{output.SetValue(null,oldOutput);if(obj!=null)obj.GetComponentInChildren<JournalTechniqueHandPresentation>(true)?.ReleaseResources();EditorSceneManager.ClosePreviewScene(scene);}
        Require(SceneManager.GetActiveScene()==active&&active.isDirty==dirty,"Validation changed the scene");File.WriteAllText(Output+"Verification.json",JsonUtility.ToJson(new Report{passed=true,sceneUnchanged=true,assertions=assertions},true));Debug.Log("Chain follow passed "+assertions+" checks.");
    }
    /// <summary>Stops on the first behavior regression.</summary>
    private static void Require(bool value,string message){assertions++;if(!value)throw new InvalidOperationException(message);}
}
