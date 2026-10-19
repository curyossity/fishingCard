using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Verifies complete event coverage, reusable geometry and event-to-creature presentation resets.</summary>
public static class JournalEventCardChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/Sync002/";
    private static int assertions;
    [Serializable] private sealed class Report { public bool passed,sceneUnchanged; public int assertions,events; }

    /// <summary>Checks actual definitions at their native 4K footprint and captures all five cards at both output sizes.</summary>
    public static void Run()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Run layout checks outside Play Mode.");
        assertions=0;Directory.CreateDirectory(Output);
        var active=SceneManager.GetActiveScene();bool dirty=active.isDirty;
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        var cards=AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        Require(theme.CentralEvents.Count==5,"Incomplete event mapping");
        Scene scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var root=new GameObject("Event review",typeof(RectTransform),typeof(Canvas));SceneManager.MoveGameObjectToScene(root,scene);
            root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;root.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);
            var views=new CreatureCardView[3];
            for(int i=0;i<3;i++)
            {
                var o=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(MidnightFieldJournalAssetSetup.Root+"Prefabs/CreatureCard.prefab"),root.transform);
                var rect=o.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2((i-1)*1260,0);rect.sizeDelta=new Vector2(1260,1400);views[i]=o.GetComponent<CreatureCardView>();
            }
            var creature=cards.Single(c=>c.UniqueId=="creature_coastal_cod");
            for(int i=0;i<theme.CentralEvents.Count;i++)
            {
                var entry=theme.CentralEvents[i];var card=cards.Single(c=>c.UniqueId==entry.cardUniqueId);var view=views[i%3];
                view.SetCard(creature,2,12,false);view.SelectEffectChoice(EncounterEffectChoice.Bait,false);
                view.SetCard(card,0,0,false);Canvas.ForceUpdateCanvases();
                var profile=Field<CreatureCardVisualProfile>(view,"visualProfile");
                Require(profile.eventAspectRatio==profile.catchAspectRatio&&view.GetComponent<RectTransform>().rect.size==new Vector2(1260,1400),"Event aspect differs from creature");
                Require(Field<Image>(view,"creatureArtwork").sprite==entry.sprite&&entry.sprite!=null,"Wrong event illustration");
                Require(Text(view,"cardNameText").text==card.DisplayName.ToUpperInvariant(),"Event name mismatch");
                Require(Normalize(Text(view,"effectText").text)==Normalize(card.RulesText),"Event rules changed");
                Require(Text(view,"cardTypeText").text=="E V E N T","Wrong event heading");
                Require(view.transform.Find("Event Decoration/Event Category").GetComponent<TMP_Text>().text.Replace(" ","")==card.CardType.ToString().ToUpperInvariant(),"Wrong event type");
                Require(view.transform.Find("Event Decoration").gameObject.activeSelf&&!view.transform.Find("Role Decoration").gameObject.activeSelf,"Wrong event decoration");
                Require(!view.transform.Find("Rarity Footer").gameObject.activeSelf&&!Text(view,"weightText").transform.parent.gameObject.activeSelf&&!Text(view,"valueText").transform.parent.gameObject.activeSelf,"Event shows creature metadata");
                Require(!Field<RectTransform>(view,"baitRulesRegion").gameObject.activeSelf&&!Field<RectTransform>(view,"rulesRegion").GetComponent<Button>().enabled,"Event accepts role input");
                foreach(var text in view.GetComponentsInChildren<TMP_Text>())
                {
                    if(text.text.Length==0)continue;text.ForceMeshUpdate(true,true);
                    Require(!text.isTextOverflowing&&text.textInfo.characterInfo.Any(c=>c.isVisible),"Missing/overflowing event text: "+card.DisplayName+"/"+text.name);
                }
                Require(Text(view,"effectText").fontSize>=32,"Event body shrank below approved minimum");
                if(i==2||i==4)
                {
                    if(i==4)views[2].gameObject.SetActive(false);
                    Capture(root,scene,3840,2160,"Events"+(i==2?1:2)+"4K.png");
                    Capture(root,scene,1920,1080,"Events"+(i==2?1:2)+"1080p.png");
                }
                view.SetCard(creature,2,12,false);
                Require(!view.transform.Find("Event Decoration").gameObject.activeSelf&&view.transform.Find("Role Decoration").gameObject.activeSelf,"Event decoration leaked to creature");
                Require(Text(view,"effectText").alignment==TextAlignmentOptions.TopLeft,"Creature text alignment not restored");
                view.SetBlank();Require(!view.transform.Find("Event Decoration").gameObject.activeSelf,"Blank has event decoration");
                view.SetCard(card,0,0,false);
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        bool unchanged=SceneManager.GetActiveScene()==active&&active.isDirty==dirty;Require(unchanged,"Checks changed active scene");
        File.WriteAllText(Output+"EventCardValidation.json",JsonUtility.ToJson(new Report{passed=true,assertions=assertions,events=5,sceneUnchanged=unchanged},true));
        Debug.Log("Journal event presentation passed: "+assertions+" assertions across five events.");
    }

    /// <summary>Captures independent native UI elements without changing the active scene or runtime sprites.</summary>
    private static void Capture(GameObject root,Scene scene,int width,int height,string file)
    {
        var o=new GameObject("Event review camera",typeof(Camera));SceneManager.MoveGameObjectToScene(o,scene);var c=o.GetComponent<Camera>();c.scene=scene;c.orthographic=true;c.orthographicSize=1080;c.transform.position=new Vector3(0,0,-100);c.nearClipPlane=.1f;c.farClipPlane=2000;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color32(17,30,37,255);root.GetComponent<Canvas>().worldCamera=c;
        var rt=new RenderTexture(width,height,24);var old=RenderTexture.active;Texture2D image=null;
        try { c.targetTexture=rt;Canvas.ForceUpdateCanvases();c.Render();RenderTexture.active=rt;image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+file,image.EncodeToPNG()); }
        finally { RenderTexture.active=old;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);if(image!=null)UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(o); }
    }
    /// <summary>Reads one existing serialized presentation binding.</summary>
    private static T Field<T>(CreatureCardView view,string field) where T:UnityEngine.Object { return new SerializedObject(view).FindProperty(field).objectReferenceValue as T; }
    /// <summary>Reads one dynamic text binding.</summary>
    private static TMP_Text Text(CreatureCardView view,string field) { return Field<TMP_Text>(view,field); }
    /// <summary>Ignores visual line breaks when comparing runtime text to the authored rule.</summary>
    private static string Normalize(string value) { return string.Join(" ",value.Split(new[]{' ','\n','\r','\t'},StringSplitOptions.RemoveEmptyEntries)); }
    /// <summary>Stops on the first violated contract.</summary>
    private static void Require(bool value,string message) { assertions++;if(!value)throw new InvalidOperationException(message); }
}
