using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Verifies reconciled creature layouts, concealment, native emblems and hover/focus separation.</summary>
public static class JournalDesignSyncChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/Sync001/";
    private const string Root=MidnightFieldJournalAssetSetup.Root;
    private static int assertions;
    [Serializable] private sealed class Report { public bool passed;public int assertions,creatures; }

    /// <summary>Checks all eighteen central designs and captures six native-size three-card comparison groups.</summary>
    public static void Run()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Run static layout checks outside Play Mode.");
        assertions=0;Directory.CreateDirectory(Output);
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        Require(theme.CentralCards.Count==18,"Incomplete central roster");
        var cards=AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        Scene scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var root=new GameObject("Sync creature review",typeof(RectTransform),typeof(Canvas));SceneManager.MoveGameObjectToScene(root,scene);
            root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;root.GetComponent<RectTransform>().sizeDelta=new Vector2(3840,2160);
            var views=new CreatureCardView[3];
            for(int i=0;i<3;i++) { var o=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/CreatureCard.prefab"),root.transform);var rt=o.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(1260,1400);rt.anchoredPosition=new Vector2((i-1)*1260,0);views[i]=o.GetComponent<CreatureCardView>(); }
            for(int i=0;i<theme.CentralCards.Count;i++)
            {
                var entry=theme.CentralCards[i];var card=cards.Single(c=>c.UniqueId==entry.cardUniqueId);var view=views[i%3];
                Require(entry.sprite!=null&&entry.portraitBounds.width>0,"Missing imported portrait "+card.DisplayName);
                view.SetCard(card,0,9999,false);Canvas.ForceUpdateCanvases();
                var art=Field<UnityEngine.UI.Image>(view,"creatureArtwork");
                Require(art.sprite==entry.sprite&&art.enabled&&art.rectTransform.parent.GetComponent<RectTransform>().rect.width>0,"Hidden portrait "+card.DisplayName);
                Require(art.rectTransform.localScale==Vector3.one&&art.rectTransform.anchoredPosition==Vector2.zero,"Old framing leaked");
                Require(Text(view,"weightText").text=="0"&&Text(view,"valueText").text=="9999","Resolved stat values missing");
                Require(view.transform.Find("Rarity Footer/Rarity Name").GetComponent<TMP_Text>().text==card.Rarity.ToString().ToUpperInvariant(),"Rarity mismatch");
                foreach(string field in new[]{"cardNameText","weightText","valueText","catchAbilityText","baitAbilityText","effectText","baitEffectText"})Fit(Text(view,field),card.DisplayName);
                if(card.RulesText.Length==0)Require(Text(view,"catchAbilityText").text==""&&Text(view,"effectText").text=="","Invented Catch content");
                view.SetCard(card,2,4,true,true);
                Require(Text(view,"cardNameText").text=="UNKNOWN CREATURE"&&!art.enabled&&!view.transform.Find("Rarity Footer").gameObject.activeSelf,"Identity leak");
                Require(Text(view,"effectText").text==""&&Text(view,"baitEffectText").text==""&&Text(view,"catchAbilityText").text==""&&Text(view,"baitAbilityText").text=="","Hidden effects leak");
                view.SelectEffectChoice(EncounterEffectChoice.Bait,false);Require(view.SelectedEffectChoice==EncounterEffectChoice.Bait,"Hidden role unavailable");
                view.SetCard(card,card.Weight,card.Value,false);view.SelectEffectChoice(EncounterEffectChoice.Catch,false);
                if(i%3==2)Capture(root,scene,"Creatures"+(i/3+1)+"4K.png");
            }
            var eventView=views[0];eventView.SetCard(cards.First(c=>c.CardType==CardType.Hazard),0,0,false);
            Require(!eventView.transform.Find("Role Decoration").gameObject.activeSelf&&!eventView.transform.Find("Rarity Footer").gameObject.activeSelf,"Event has creature decoration");
            Require(!Field<RectTransform>(eventView,"baitRulesRegion").gameObject.activeSelf,"Event has Bait panel");
            eventView.SetBlank();Require(Text(eventView,"cardNameText").text==""&&!eventView.transform.Find("Rarity Footer").gameObject.activeSelf,"Blank leak");
            Hover(root.transform,cards);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        File.WriteAllText(Output+"CardValidation.json",JsonUtility.ToJson(new Report{passed=true,assertions=assertions,creatures=18},true));
    }

    /// <summary>Checks pointer entry/selection/exit cannot create a frame, while navigation and legal-target states remain.</summary>
    private static void Hover(Transform parent,CardDefinition[] cards)
    {
        var events=new GameObject("Sync EventSystem",typeof(EventSystem));SceneManager.MoveGameObjectToScene(events,parent.gameObject.scene);
        var pointer=new PointerEventData(events.GetComponent<EventSystem>());var navigation=new BaseEventData(events.GetComponent<EventSystem>());
        foreach(string action in new[]{"Descend","Surface"})
        {
            var o=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/"+action+"Button.prefab"),parent);
            var feedback=o.GetComponent<JournalActionFeedback>();var edge=o.transform.Find("Focus Edge").GetComponent<JournalCardOutline>();
            feedback.OnPointerEnter(pointer);feedback.OnSelect(pointer);Require(!edge.enabled,"Action hover frame");feedback.OnPointerExit(pointer);Require(!edge.enabled,"Pointer selected action retained frame");feedback.OnSelect(navigation);Require(edge.enabled,"Action navigation focus lost");UnityEngine.Object.DestroyImmediate(o);
        }
        var compact=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<CompactCatchCardView>(Root+"Prefabs/CompactCatchCard.prefab"),parent);
        compact.SetPreview("Test",null,1,2,null);compact.OnPointerEnter(pointer);compact.OnSelect(pointer);Require(!Field<UnityEngine.UI.Graphic>(compact,"hoverFrame").gameObject.activeSelf,"Chain hover frame");compact.OnPointerExit(pointer);compact.OnSelect(navigation);Require(Field<UnityEngine.UI.Graphic>(compact,"hoverFrame").gameObject.activeSelf,"Chain navigation lost");compact.SetState(CompactCatchCardState.ReleaseCandidate);Require(Field<UnityEngine.UI.Graphic>(compact,"releaseCandidateFrame").gameObject.activeSelf,"Legal target indicator lost");
        var technique=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<TechniqueCardView>(Root+"Prefabs/TechniqueCard.prefab"),parent);
        technique.SetCard(cards.First(c=>c.CardType==CardType.Technique&&!c.IsBackupTechnique),"TECHNIQUE",true);technique.OnPointerEnter(pointer);technique.OnSelect(pointer);Require(!Field<UnityEngine.UI.Graphic>(technique,"hoverFrame").gameObject.activeSelf,"Technique hover frame");technique.OnPointerExit(pointer);technique.OnSelect(navigation);Require(Field<UnityEngine.UI.Graphic>(technique,"hoverFrame").gameObject.activeSelf,"Technique navigation lost");
    }

    /// <summary>Rejects clipped or missing TMP glyphs while preserving effectless panels.</summary>
    private static void Fit(TMP_Text text,string card)
    { text.ForceMeshUpdate(true,true);if(text.text.Length>0)Require(!text.isTextOverflowing&&text.textInfo.characterInfo.Any(c=>c.isVisible),"Text overflow/missing: "+card+" / "+text.name); }
    /// <summary>Renders actual independent Unity elements at the target output resolution.</summary>
    private static void Capture(GameObject root,Scene scene,string name)
    {
        var o=new GameObject("Sync camera",typeof(Camera));SceneManager.MoveGameObjectToScene(o,scene);var c=o.GetComponent<Camera>();c.scene=scene;c.orthographic=true;c.orthographicSize=1080;c.transform.position=new Vector3(0,0,-100);c.nearClipPlane=.1f;c.farClipPlane=2000;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color32(17,30,37,255);root.GetComponent<Canvas>().worldCamera=c;
        var rt=new RenderTexture(3840,2160,24);var previous=RenderTexture.active;Texture2D pixels=null;
        try { c.targetTexture=rt;Canvas.ForceUpdateCanvases();foreach(var t in root.GetComponentsInChildren<TMP_Text>())t.ForceMeshUpdate(true,true);c.Render();RenderTexture.active=rt;pixels=new Texture2D(3840,2160,TextureFormat.RGBA32,false);pixels.ReadPixels(new Rect(0,0,3840,2160),0,0);pixels.Apply();File.WriteAllBytes(Output+name,pixels.EncodeToPNG()); }
        finally { RenderTexture.active=previous;c.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(o); }
    }
    /// <summary>Reads an existing serialized presentation reference.</summary>
    private static T Field<T>(UnityEngine.Object owner,string field) where T:UnityEngine.Object { return new SerializedObject(owner).FindProperty(field).objectReferenceValue as T; }
    /// <summary>Reads a bound native text field.</summary>
    private static TMP_Text Text(CreatureCardView view,string field) { return Field<TMP_Text>(view,field); }
    /// <summary>Stops on the first violated contract.</summary>
    private static void Require(bool condition,string message) { assertions++;if(!condition)throw new InvalidOperationException(message); }
}
