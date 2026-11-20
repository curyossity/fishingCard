using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks impact-driven score presentation and local assistance without changing production creature content or saves.</summary>
public static class LandingFeedbackPolishChecks
{
    public const string Output = "Docs/Validation/LandingFeedbackPolish/";
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags Fixture = BindingFlags.Static | BindingFlags.NonPublic;
    private static int checks;
    public static string Result { get; private set; } = "Not run";

    /// <summary>Checks pre-impact stability, exact arrivals, long-chain bounds and the saved native visual bindings.</summary>
    public static int Validate()
    {
        checks = 0;
        var score = new LandingScore(LandingAccuracy.Perfect,20,30,12,new[]{new LandingContribution(1,"Add",30,1,0,"",new[]{5}),new LandingContribution(2,"Multiply",0,3),new LandingContribution(3,"Multiply",0,4)});
        var sequence = new LandingScoreSequence(score);
        for (int i = 1; i < sequence.Steps.Count; i++)
        {
            var step = sequence.Steps[i];
            Check(step.Time < step.LaunchAt && step.LaunchAt < step.ImpactAt,"The operation has no anticipation/flight interval.");
            Check(sequence.ValueAt(step.ImpactAt-.0001f)==sequence.Steps[i-1].Value,"Score changed before the projectile arrived.");
            Check(sequence.ValueAt(step.ImpactAt)==step.Value,"Impact did not deliver the exact operation.");
            if(i+1<sequence.Steps.Count)Check(step.ImpactAt<sequence.Steps[i+1].Time,"A new source replaced an in-flight projectile.");
        }
        Check(sequence.ValueAt(sequence.Duration)==600&&score.Points==600,"Presentation changed final arithmetic.");
        Check(sequence.Steps[1].SupportingIds.SequenceEqual(new[]{5}),"An operation lost its specific enabler identity.");
        var many=Enumerable.Range(0,150).Select(i=>new LandingContribution(i,"Add",1,1)).ToArray();
        var longSequence=new LandingScoreSequence(new LandingScore(LandingAccuracy.Good,10,150,1,many));
        Check(longSequence.Duration<3.1f&&longSequence.ValueAt(longSequence.Duration)==160,"Long rigs extend or truncate the count-up.");
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath);
        var rush=new SerializedObject(root.GetComponent<LandingScoreRush>());
        var portrait=(UnityEngine.UI.Image)rush.FindProperty("sourcePortrait").objectReferenceValue;
        Check(portrait.rectTransform.sizeDelta.x>=72&&portrait.rectTransform.sizeDelta.y>=64,"Active source remains too small.");
        Check(rush.FindProperty("progressLight").objectReferenceValue!=null,"Missing score progress binding.");
        Check(portrait.transform.parent.name=="Active source"&&portrait.transform.parent.parent.Find("Source medallion")==null,"Source must use its original unframed bottom position.");
        Check(rush.FindProperty("marquee").arraySize==7,"Missing amplification lamps.");
        var effects=root.GetComponent<LandingEffectFeedback>();Check(effects!=null,"No local effect feedback component.");
        var data=new SerializedObject(effects);
        foreach(string field in new[]{"rush","goodEdge","perfectEdge","slowPortrait","recoveryPortrait","slowCaption","recoveryCaption","recovery","behavior","persistentRecovery"})
            Check(data.FindProperty(field).objectReferenceValue!=null,"Missing local effect binding: "+field);
        Check(!((CanvasGroup)data.FindProperty("recovery").objectReferenceValue).blocksRaycasts,"Recovery intercepts input.");
        var history=(UnityEngine.UI.ScrollRect)new SerializedObject(root.GetComponent<LandingStrikeFeedback>()).FindProperty("chipScroll").objectReferenceValue;
        Check(history.viewport.rect.height>=52,"History portraits remain too small.");
        return checks;
    }

    /// <summary>Runs the actual controller loop in a disposable Play session with persistence disabled.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";checks=0;Directory.CreateDirectory(Output);
        var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        owner.SetDebugSaveResetPending(true);Application.runInBackground=true;owner.StartCoroutine(Run());
    }
    /// <summary>Inspects launch/impact states on real receipts, genuine recovery and the normal winning handoffs.</summary>
    private static IEnumerator Run()
    {
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Glass Minnow","Mackerel","Cuttlefish","Peacock Worm","Squid","Cleaner Shrimp","Sea Sponge","Moonlit Ray","Flounder"})Acquire(owner,Card(name));
        Force(owner,Card("Cod"));yield return Accept(owner);
        var landing=UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();var rush=landing.GetComponent<LandingScoreRush>();var data=new SerializedObject(rush);
        var reduced=(UnityEngine.UI.Toggle)data.FindProperty("reducedMotionToggle").objectReferenceValue;
        var sound=(UnityEngine.UI.Toggle)data.FindProperty("soundToggle").objectReferenceValue;
        reduced.SetIsOnWithoutNotify(false);sound.SetIsOnWithoutNotify(true);
        yield return Strike(owner,LandingAccuracy.Perfect);
        var receipt=owner.LastLandingStrike;decimal points=owner.Landing.Points;int attempts=owner.Landing.AttemptsRemaining;long revision=owner.Landing.Revision;
        Check(rush.IsPlaying&&owner.LandingFeedbackPending,"Accepted strike did not begin its presentation.");
        foreach(string field in new[]{"sourceName","operation","caption"})
        { var text=(TMP_Text)data.FindProperty(field).objectReferenceValue;text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Active readout clips "+field); }
        var step=rush.Sequence.Steps[1];
        Render(rush,(step.LaunchAt+step.ImpactAt)*.5f);
        Check(rush.DisplayedStrike==rush.Sequence.Steps[0].Value,"Visible score grew while the projectile was still in flight.");
        Check(Field<int>(rush,"lastImpact")==0,"Impact sound fired at launch.");
        foreach(var size in new[]{new Vector2Int(1280,800),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})Capture(landing,"Flight-"+size.x+"x"+size.y+".png",size.x,size.y);
        Render(rush,step.ImpactAt+.025f);Check(rush.DisplayedStrike==step.Value,"Arriving projectile did not increase the number.");
        Check(Field<int>(rush,"lastImpact")==1&&landing.GetComponentsInChildren<AudioSource>().Any(s=>s.isPlaying),"Impact cue did not play.");
        Check(owner.Landing.Points==points&&owner.Landing.AttemptsRemaining==attempts&&owner.Landing.Revision==revision,"Presentation reran gameplay.");
        var effects=landing.GetComponent<LandingEffectFeedback>();Tick(effects);
        Check(effects.SlowVisible,"Actual in-Good slowing is not indicated at the track.");
        Capture(landing,"Addition-Impact.png",1920,1080);
        var multiplier=rush.Sequence.Steps.First(s=>s.Multiply);Render(rush,multiplier.ImpactAt+.04f);
        Check(rush.DisplayedStrike==multiplier.Value,"Multiplier impact is inaccurate.");
        Canvas.ForceUpdateCanvases();var mesh=((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).canvasRenderer.GetMesh();
        Check(mesh!=null&&mesh.vertexCount>0,"Multiplier ripple did not produce native geometry.");Capture(landing,"Multiplier-Impact.png",1920,1080);
        var chips=landing.GetComponentsInChildren<LandingContributionView>().Where(c=>c.Anchor.rect.height>=52).ToArray();
        Check(chips.Length>=2&&chips.Any(c=>Field<int>(c,"sequenceState")==1),"Completed operations do not leave a readable trace.");
        reduced.SetIsOnWithoutNotify(true);Render(rush,step.ImpactAt+.025f);Canvas.ForceUpdateCanvases();
        Check(((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).canvasRenderer.GetMesh().vertexCount==0,"Reduced motion retained particles.");
        Check(landing.GetComponentsInChildren<TackleRigCardView>().All(c=>c.transform.localScale==Vector3.one&&c.PortraitTransform.localScale==Vector3.one),"Reduced motion retained card/portrait scaling.");
        owner.CompleteLandingFeedback();yield return null;reduced.SetIsOnWithoutNotify(false);
        Check(!rush.IsPlaying&&rush.DisplayedTotal==receipt.PointsAfter,"Skip did not settle to the exact accumulated score.");
        yield return Strike(owner,LandingAccuracy.Miss);typeof(LandingEffectFeedback).GetField("recovered",Hidden).SetValue(effects,Time.realtimeSinceStartupAsDouble-.16);Tick(effects);
        Check(owner.LastLandingStrike.Score.Points==0&&effects.RecoveryVisible,"A genuine Miss recovery is missing or credits points.");
        var recoveryText=(TMP_Text)new SerializedObject(effects).FindProperty("recoveryCaption").objectReferenceValue;
        Check(recoveryText.text.Contains("CLEANER SHRIMP")&&recoveryText.text.Contains("+1"),"Recovered attempt has no source identity.");
        Capture(landing,"Attempt-Recovery.png",1920,1080);owner.CompleteLandingFeedback();yield return null;
        yield return Land(owner,true);
        var build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();Check(build!=null,"Winning hit did not enter Build.");
        ((UnityEngine.UI.Button)new SerializedObject(build).FindProperty("commit").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Cuttlefish","Stormjaw Tuna"})for(int i=0;i<29;i++)Acquire(owner,Card(name));
        Force(owner,Card("Sardine"));yield return Accept(owner);yield return Strike(owner,LandingAccuracy.Perfect);
        Check(rush.Sequence.Intensity>.8f&&owner.Landing.State==LandingChallengeState.Succeeded,"Large combo did not retain immediate authoritative success.");
        Render(rush,rush.Sequence.Steps.Last().ImpactAt+.025f);
        Check(Enumerable.Range(0,data.FindProperty("marquee").arraySize).Count(i=>((UnityEngine.UI.Image)data.FindProperty("marquee").GetArrayElementAtIndex(i).objectReferenceValue).enabled)>1,"Large combo does not energize the machine.");
        Capture(landing,"Large-Combo.png",1920,1080);Render(rush,rush.Sequence.PayoutAt+.12f);Capture(landing,"Payout.png",1920,1080);
        owner.CompleteLandingFeedback();yield return null;
        Check(owner.RigUpgradeFeedbackVisible&&!rush.IsPlaying,"Duplicate upgrade did not follow the finished payout.");
        Check(landing.GetComponentsInChildren<AudioSource>(true).All(s=>!s.isPlaying),"Hidden phase retained audio.");
        Result="Passed "+checks+" live feedback-polish checks";File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");Debug.Log(Result);
    }
    /// <summary>Samples only the visual clock to review launch and impact without altering model state.</summary>
    private static void Render(LandingScoreRush rush,float elapsed)=>typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{elapsed});
    /// <summary>Refreshes local assistance after a deterministic visual sample.</summary>
    private static void Tick(LandingEffectFeedback effects)=>typeof(LandingEffectFeedback).GetMethod("LateUpdate",Hidden).Invoke(effects,null);
    /// <summary>Inspects private presentation state for source/timing assertions without adding test APIs to runtime code.</summary>
    private static T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name,Hidden).GetValue(owner);
    /// <summary>Observes a real robust timing window before sending one revision-bound strike.</summary>
    private static IEnumerator Strike(FishingRunController owner,LandingAccuracy accuracy)
    {
        long revision=owner.Landing.Revision;double until=Time.realtimeSinceStartupAsDouble+40;
        while(owner.Landing.Revision==revision&&Time.realtimeSinceStartupAsDouble<until)
        {
            var model=owner.Landing;if(!owner.LandingFeedbackPending)model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance=Math.Abs(model.FishPosition-model.ZonePosition);
            if(!owner.LandingFeedbackPending&&model.CanStrike&&(accuracy==LandingAccuracy.Perfect?distance<(model.PerfectWidth+model.FishWidth)*.5-.01:distance>(model.GoodWidth+model.FishWidth)*.5+.05))owner.TryStrikeLanding(model.Occurrence,revision);
            if(owner.Landing.Revision==revision)yield return null;
        }
        Check(owner.Landing.Revision>revision,"No valid strike window was observed.");
    }
    /// <summary>Uses the existing full-loop success fixture, including its bounded feedback wait.</summary>
    private static IEnumerator Land(FishingRunController owner,bool success)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Fixture).Invoke(null,new object[]{owner,success});
    /// <summary>Accepts a real Discover result after its cosmetic spin.</summary>
    private static IEnumerator Accept(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept",Fixture).Invoke(null,new object[]{owner});
    /// <summary>Constrains disposable selection without modifying production creature assets.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",Fixture).Invoke(null,new object[]{owner,card});
    /// <summary>Loads one installed species definition for the disposable rig.</summary>
    private static CardDefinition Card(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Commits a fixture acquisition through the actual rig owner.</summary>
    private static void Acquire(FishingRunController owner,CardDefinition card)=>Check(owner.Rig.TryAcquire(owner.Rig.Revision,card,owner.Rig.Members.Length,out _,out _),"Could not seed fixture.");
    /// <summary>Captures real native graphics through the active project renderer.</summary>
    private static void Capture(TackleLandingPresentation landing,string file,int width,int height){Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(landing.GetComponent<Canvas>(),file,width,height,Output);}
    /// <summary>Retains a readable failure reason for asynchronous checks.</summary>
    private static void Check(bool valid,string message){if(!valid){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
