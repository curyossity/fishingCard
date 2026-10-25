using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Checks resolver provenance, exact imported resources, clipping and real tooltip input lifecycle.</summary>
public static class JournalStatTooltipChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/Sync007/";
    private static int assertions;
    public static string Status {get;private set;}="Not started";
    private static GameObject fixture;
    private static JournalModalHost runner;
    private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    private static JournalStatInspectionTrigger productionTrigger;
    private static FishingRunView productionView;

    /// <summary>Prepares a disposable Play Mode state through real runtime owners, then hovers the actual main-card number.</summary>
    public static void BeginProductionSmoke()
    {
        var controller=UnityEngine.Object.FindFirstObjectByType<FishingRunController>();
        if(!Application.isPlaying||controller==null)throw new InvalidOperationException("Open the candidate game in Play Mode.");
        var encounters=(EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime",Flags).GetValue(controller);
        var chain=(CatchChainRuntime)typeof(FishingRunController).GetField("catchChainRuntime",Flags).GetValue(controller);
        var resolver=(EffectResolver)typeof(FishingRunController).GetField("effectResolver",Flags).GetValue(controller);
        var techniques=(TechniqueEffectRuntime)typeof(FishingRunController).GetField("techniqueEffectRuntime",Flags).GetValue(controller);
        encounters.SetCurrentEncounter(Card("Cod"));var prize=Card("Prize Hook");
        Assert(techniques.Approved.Apply(prize.Effects[0],0,null,encounters,chain,resolver,null,null,new System.Random(5),false,out _,prize),"Production Prize grant failed");
        Assert(controller.TryChooseEncounterRole(EncounterEffectChoice.Catch),"Production explicit Catch choice failed");
        productionView=UnityEngine.Object.FindFirstObjectByType<FishingRunView>();
        var main=(CreatureCardView)typeof(FishingRunView).GetField("encounterCardView",Flags).GetValue(productionView);
        var value=(TMP_Text)typeof(CreatureCardView).GetField("valueText",Flags).GetValue(main);productionTrigger=value.GetComponent<JournalStatInspectionTrigger>();
        Canvas.ForceUpdateCanvases();var hits=new System.Collections.Generic.List<RaycastResult>();
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,value.rectTransform.TransformPoint(value.rectTransform.rect.center))};
        EventSystem.current.RaycastAll(pointer,hits);Assert(hits.Any(h=>h.gameObject==value.gameObject),"Production stat is not raycastable");
    }

    /// <summary>Hovers after the actual encounter transition has settled, preserving normal input locks.</summary>
    public static void HoverProductionNumber()
    {
        productionTrigger.OnPointerEnter(new PointerEventData(EventSystem.current));
    }

    /// <summary>Verifies the production overlay receives input and preserves actual pending Technique attribution.</summary>
    public static void FinishProductionSmoke()
    {
        var host=productionView.GetComponent<JournalStatTooltipHost>();Assert(host!=null&&host.Tooltip!=null&&host.Tooltip.gameObject.activeSelf,"Production hover did not open tooltip");
        var tip=host.Tooltip;Assert(tip.transform.parent==productionView.TooltipLayer.transform,"Tooltip uses the wrong overlay layer");
        var hits=new System.Collections.Generic.List<RaycastResult>();var r=(RectTransform)tip.transform;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};
        EventSystem.current.RaycastAll(pointer,hits);Assert(hits.Any(h=>h.gameObject.GetComponentInParent<JournalStatTooltip>()==tip),"Production tooltip cannot receive scrolling input");
        Assert(productionTrigger.TryRead(out var data)&&data.Reasons.Any(x=>x.Source==Card("Prize Hook")&&x.Amount==8),"Production preview lost Technique provenance");
        ScreenCapture.CaptureScreenshot(Output+"MainTooltip4K.png");
        File.WriteAllText(Output+"ProductionSmoke.json","{\"passed\":true,\"mainNumberRaycast\":true,\"tooltipLayerInput\":true,\"pendingTechniqueProvenance\":true}");
        productionTrigger.OnPointerExit(pointer);productionTrigger=null;productionView=null;
    }

    /// <summary>Tests production cards and numerical conservation without changing card assets or the open scene.</summary>
    public static void Run()
    {
        assertions=0;Directory.CreateDirectory(Output);var resolver=new EffectResolver();var chain=new CatchChainRuntime();
        var mussel=chain.Add(Card("Mussel"),resolver);chain.Add(Card("Spiny Lobster"),resolver,CatchAttachmentRole.Bait);
        Assert(mussel.CurrentWeight==0,"Lobster reduction changed gameplay");
        var d=new CardStatBreakdown(mussel,CreatureStat.Weight,1);Conserves(d);
        Assert(d.Reasons.Single().Source==Card("Spiny Lobster")&&d.Reasons[0].Amount==-1,"Ongoing source missing");
        chain.Add(Card("Spiny Lobster"),resolver,CatchAttachmentRole.Bait);d=new CardStatBreakdown(mussel,CreatureStat.Weight,1);Conserves(d);
        Assert(d.Reasons.Length==1&&d.Reasons[0].Amount==-1,"Zero-floor reduction fabricated contribution");
        var reef=Card("Reef Snag");mussel.AddEventModifier(1,1,0,reef.Effects[0],reef);chain.Recalculate(resolver);
        d=new CardStatBreakdown(mussel,CreatureStat.Weight,1);Conserves(d);Assert(d.Reasons.Any(r=>r.Source==reef&&r.Amount==1),"Event provenance missing");
        Assert(mussel.TryRemoveTrackedModifier(mussel.AppliedModifiers[0]),"Event expiry failed");chain.Recalculate(resolver);Conserves(new CardStatBreakdown(mussel,CreatureStat.Weight,1));
        chain.Reset();var gar=chain.Add(Card("Garfish"),resolver);chain.Add(Card("Tidepool Eel"),resolver);
        d=new CardStatBreakdown(gar,CreatureStat.Value,1);Conserves(d);Assert(d.Reasons.Any(r=>r.Source==gar.Definition&&r.Amount==3),"Self scaling attribution missing");
        var snapshot=gar.CreateSnapshot();Assert(new CardStatBreakdown(snapshot,CreatureStat.Value,1).Reasons.Length==d.Reasons.Length,"Snapshot lost sources");
        chain.Reset();var lion=chain.Add(Card("Lionfish"),resolver);chain.Add(Card("Cod"),resolver);
        chain.TryRelease(1,resolver,out _,out _,out _);d=new CardStatBreakdown(lion,CreatureStat.Value,1);Conserves(d);
        Assert(d.Reasons.Any(r=>r.Source==lion.Definition&&r.Amount==-5),"Release penalty source missing");
        var approved=new ApprovedTechniqueRuntime();var encounters=new EncounterRuntime();encounters.SetCurrentEncounter(Card("Cod"));var prize=Card("Prize Hook");
        Assert(approved.Apply(prize.Effects[0],0,null,encounters,chain,resolver,null,null,new System.Random(2),false,out _,prize),"Prize grant unavailable");
        var preview=chain.CreateResolvedCatchPreview(Card("Cod"),resolver,CatchAttachmentRole.Catch,approved.PreviewPendingPrizes);
        d=new CardStatBreakdown(preview,CreatureStat.Value,encounters.EncounterRevision);Conserves(d);
        Assert(d.Reasons.Single(r=>r.Source==prize).Amount==8&&approved.PendingPrizeCount==1,"Prize preview consumed grant or lost source");
        Conserves(new CardStatBreakdown(preview,CreatureStat.Weight,1));
        var events=new EventPersistenceRuntime();events.Bind(chain,resolver);var upwelling=Card("Gentle Upwelling");
        events.QueuePending(1,upwelling,upwelling.Effects[0],EventEffectLifetime.UntilRunEnds());
        preview=chain.CreateResolvedCatchPreview(Card("Cod"),resolver,CatchAttachmentRole.Bait);events.PreviewPendingBaitWeightReductions(preview);
        d=new CardStatBreakdown(preview,CreatureStat.Weight,1);Conserves(d);Assert(d.Reasons.Any(r=>r.Source==upwelling&&r.Amount==-1),"Pending event source missing");
        var style=AssetDatabase.LoadAssetAtPath<JournalStatTooltipStyle>(JournalStatTooltipDesignSync.StylePath);
        Assert(style.frame.border==new Vector4(64,64,64,64),"Wrong nine-slice borders");
        Assert(style.eventIcon!=null&&style.techniqueIcon!=null&&style.creatureIcons.All(x=>x.sprite!=null),"Missing glyph exports");
        Assert(style.GetIcon(Card("Lionfish"),out bool silhouette)==style.creatureIcons[0].sprite&&!silhouette,"Exact creature glyph registry mismatch");
        foreach(string name in new[]{"CreatureCard","CompactCatchCard"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(MidnightFieldJournalAssetSetup.Root+"Prefabs/"+name+".prefab");
            Assert(prefab.GetComponentsInChildren<JournalStatInspectionTrigger>(true).Length==2,"Prefab missing independent stat targets");
        }
        File.WriteAllText(Output+"ComponentVerification.json","{\"passed\":true,\"assertions\":"+assertions+"}");
        Status="Components passed: "+assertions;
    }

    /// <summary>Starts a finite actual-frame UI check with wheel input, arrow limits, pointer travel and masking.</summary>
    public static void StartInteraction()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");
        Status="Running interaction checks";runner=new GameObject("Stat Tooltip Verification Runner").AddComponent<JournalModalHost>();runner.StartCoroutine(Guard());
    }

    /// <summary>Writes failure evidence and destroys test-only UI without saving a scene.</summary>
    private static IEnumerator Guard()
    {
        var test=Interaction();Exception error=null;
        while(true){bool more;try{more=test.MoveNext();}catch(Exception e){error=e;break;}if(!more)break;yield return test.Current;}
        File.WriteAllText(Output+"InteractionVerification.json","{\"passed\":"+(error==null?"true":"false")+",\"assertions\":"+assertions+",\"error\":\""+(error?.ToString()??"").Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r","").Replace("\n","\\n")+"\"}");
        if(fixture!=null)UnityEngine.Object.Destroy(fixture);UnityEngine.Object.Destroy(runner.gameObject);runner=null;
        Status=error==null?"Passed":"Failed: "+error;if(error!=null)Debug.LogException(error);
    }

    /// <summary>Checks the real imported prefab and pooled host instead of a mock tooltip API.</summary>
    private static IEnumerator Interaction()
    {
        fixture=new GameObject("Modifier tooltip fixture",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(CanvasGroup));
        var canvas=fixture.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30000;
        var scaler=fixture.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(3840,2160);
        var face=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<CompactCatchCardView>(MidnightFieldJournalAssetSetup.Root+"Prefabs/CompactCatchCard.prefab"),fixture.transform);
        var fr=(RectTransform)face.transform;fr.anchorMin=fr.anchorMax=new Vector2(.5f,.5f);fr.anchoredPosition=new Vector2(-700,0);
        var sample=FixtureCard();face.SetCard(sample);
        var value=(TMP_Text)typeof(CompactCatchCardView).GetField("valueText",Flags).GetValue(face);var trigger=value.GetComponent<JournalStatInspectionTrigger>();
        trigger.OnSelect(new BaseEventData(EventSystem.current));yield return null;yield return null;
        var host=fixture.GetComponent<JournalStatTooltipHost>();Assert(host!=null&&host.Tooltip!=null&&host.Tooltip.gameObject.activeSelf,"Focused modified number did not open tooltip");
        var tip=host.Tooltip;Assert(!tip.UpVisible&&tip.DownVisible,"First scroll cue wrong");
        Assert(tip.GetComponentsInChildren<JournalStatScrollArrow>().All(a=>a.canvasRenderer!=null),"Arrow missing its CanvasRenderer");
        foreach(var text in tip.GetComponentsInChildren<TMP_Text>())
        {text.ForceMeshUpdate();Assert(text.textInfo.characterCount>0&&text.textInfo.characterInfo.Any(c=>c.isVisible),"Tooltip field has no visible glyphs: "+text.name);}
        Assert(tip.Scroll.verticalScrollbar==null&&tip.Scroll.horizontalScrollbar==null,"Scrollbar unexpectedly added");
        Assert(tip.Scroll.viewport.rect.height==216&&tip.Scroll.content.childCount==5,"Three-row viewport or source count wrong");
        tip.OnPointerEnter(new PointerEventData(EventSystem.current));trigger.OnDeselect(new BaseEventData(EventSystem.current));
        yield return new WaitForSecondsRealtime(.3f);Assert(tip.gameObject.activeSelf,"Pointer travel closed scrolling tooltip");
        tip.Scroll.OnScroll(new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,-1)});yield return null;
        Assert(tip.UpVisible&&tip.DownVisible,"Middle scroll cue wrong");
        ScreenCapture.CaptureScreenshot(Output+"TooltipInGame.png");yield return null;
        tip.Scroll.verticalNormalizedPosition=0;yield return null;Assert(tip.UpVisible&&!tip.DownVisible,"Last scroll cue wrong");
        fixture.GetComponent<CanvasGroup>().interactable=false;yield return null;Assert(!tip.gameObject.activeSelf,"Input lock did not hide tooltip");
        fixture.GetComponent<CanvasGroup>().interactable=true;
        var main=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<CreatureCardView>(MidnightFieldJournalAssetSetup.Root+"Prefabs/CreatureCard.prefab"),fixture.transform);
        main.SetCard(sample.Definition,sample.CurrentWeight,sample.CurrentValue,false);main.SetStatInspection(sample,10);
        Assert(main.TryGetStatBreakdown(CreatureStat.Value,out var data)&&data.Reasons.Length==5,"Main card lost actual source rows");
        main.SetCard(sample.Definition,sample.CurrentWeight,sample.CurrentValue,true);main.SetStatInspection(sample,10);
        Assert(!main.TryGetStatBreakdown(CreatureStat.Value,out _),"Hidden main value leaked provenance");
        sample.SetAttachmentRole(CatchAttachmentRole.Bait);face.SetCard(sample);
        Assert(!face.TryGetStatBreakdown(CreatureStat.Value,out _),"Bait zero treated as a modified sale value");
    }

    /// <summary>Creates layout-only mixed-source data; these mutations are confined to the disposable fixture.</summary>
    private static CardInstance FixtureCard()
    {
        var card=new CardInstance(990,Card("Cod"));
        foreach(string name in new[]{"Prize Hook","Lionfish","Moonlit Ray","Reef Snag","Gentle Upwelling"})
        {var source=Card(name);var effect=source.Effects.FirstOrDefault()??source.BaitEffects.FirstOrDefault();card.AddPermanentModifiers(0,2,effect,source);}
        new EffectResolver().ResolveCatchChain(new[]{card},Array.Empty<ActiveCatchEffectRecord>());return card;
    }

    /// <summary>Verifies all displayed reasons reconcile exactly with the authoritative value, including clamping.</summary>
    private static void Conserves(CardStatBreakdown d){Assert(d.Baseline+d.Reasons.Sum(r=>r.Amount)==d.Current,"Breakdown does not conserve "+d.Stat);}
    /// <summary>Finds production definitions by display name without depending on legacy asset filenames.</summary>
    private static CardDefinition Card(string name)
    {return AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c=>c.DisplayName==name);}
    /// <summary>Counts meaningful behavior assertions and fails at the first broken contract.</summary>
    private static void Assert(bool condition,string message){assertions++;if(!condition)throw new InvalidOperationException(message);}
}
