using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks exact score presentation, interruption cleanup and real terminal handoffs with persistence disabled.</summary>
public static class LandingScoreRushChecks
{
    public const string Output = "Docs/Validation/LandingScoreRush/";
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags Fixture = BindingFlags.Static | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Verifies score ordering, precision, bounded pacing and held-clock invariants independently of rendering.</summary>
    public static int ValidateRules()
    {
        checks = 0;
        var score = new LandingScore(LandingAccuracy.Perfect, 20, 30, 12, new[] {
            new LandingContribution(2,"Multiply first in rig",0,3), new LandingContribution(1,"Add",30,1),
            new LandingContribution(3,"Multiply",0,4), new LandingContribution(4,"Recovery",0,1,1) });
        var sequence = new LandingScoreSequence(score);
        Check(sequence.Steps.Select(s => s.Value).SequenceEqual(new decimal[] { 20, 50, 150, 600 }), "Incorrect canonical score progression.");
        Check(sequence.Steps[1].SourceId == 1 && sequence.Steps[2].SourceId == 2, "Multipliers ran before bonuses.");
        Check(sequence.Steps.Count == 4, "Attempt recovery became a scoring operation.");
        Check(sequence.StepAt(.01f) == 0 && sequence.StepAt(99) == 3, "Sequence time lookup failed.");
        Check(sequence.Duration < 4.8f && sequence.PayoutAt < sequence.Duration, "Combo feedback is not bounded.");
        var ordinary = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect,20,6,1.3m,new[] {new LandingContribution(1,"Add",6,1),new LandingContribution(2,"Multiply",0,1.3m)}));
        Check(ordinary.Steps.Last().Value == 33.8m, "Fractional score lost precision.");
        Check(ordinary.Duration < sequence.Duration && ordinary.Intensity < sequence.Intensity, "Ordinary hits do not feel distinct.");
        var miss = new LandingScoreSequence(new LandingScore(LandingAccuracy.Miss,0,0,1,Array.Empty<LandingContribution>()));
        Check(miss.Steps.Count == 1 && miss.Intensity == 0 && miss.Duration < 1, "An ordinary Miss earned a score celebration.");
        var many = Enumerable.Range(0, 150).Select(i => new LandingContribution(i,"Add",1,1)).ToArray();
        var longSequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Good,10,150,1,many));
        Check(longSequence.OperationCount == 150 && longSequence.Steps.Skip(1).Sum(s=>s.SourceIds.Count)==150 && longSequence.Steps.Last().Value == 160 && longSequence.Duration < 4.8f, "Long rigs truncate contributions or extend feedback indefinitely.");
        foreach (var step in sequence.Steps.Skip(1))
        {
            Check(step.Time < step.LaunchAt && step.LaunchAt < step.ImpactAt && step.ImpactAt < step.EndAt, "Missing anticipation, flight or recovery.");
            Check(sequence.ValueAt(step.ImpactAt-.0001f) != step.Value && sequence.ValueAt(step.ImpactAt)==step.Value,"Score did not change exactly on impact.");
        }
        var decimalItems = Enumerable.Range(0,12).Select(i=>new LandingContribution(i,"Fraction",.03m,1.07m)).ToArray();
        decimal product = 1; foreach(var item in decimalItems) product *= item.Multiplier;
        var precise = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect,20,.36m,product,decimalItems));
        Check(precise.Steps.Last().Value == precise.Score.Points,"Animation changed the authoritative decimal arithmetic order.");
        Check(LandingScoreRush.Format(600) == "600" && LandingScoreRush.Format(12000000) == "12M", "Large readout formatting failed.");
        var model = new LandingChallengeRuntime(); double clock = 0;
        model.Begin(1,100000,LandingChallengeChecks.Settings(),Array.Empty<LandingRigMember>(),new System.Random(1),clock);
        LandingChallengeChecks.Strike(model,LandingAccuracy.Perfect,ref clock);
        double fish = model.FishPosition, zone = model.ZonePosition; long revision = model.Revision; decimal points = model.Points; int attempts = model.AttemptsRemaining;
        model.HoldPresentationClock(clock + 2); model.AdvanceTo(clock + 2);
        Check(model.FishPosition == fish && model.ZonePosition == zone,"Held presentation advanced motion.");
        Check(model.Points == points && model.AttemptsRemaining == attempts && model.Revision == revision,"Presentation changed gameplay state.");
        model.AdvanceTo(clock + 2.4);
        Check(model.FishPosition != fish || model.ZonePosition != zone,"Motion did not resume after the hold.");
        return checks;
    }

    /// <summary>Verifies serialized UI, audio routing and non-intercepting feedback on the actual saved prefab.</summary>
    public static int ValidateAssets()
    {
        checks=0; var root=AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath);
        var host=root.GetComponent<LandingScoreRush>(); Check(host!=null,"Missing score-rush component.");var data=new SerializedObject(host);
        foreach(string field in new[]{"readout","value","operation","sourceName","caption","sourcePortrait","glow","machine","totalLabel","particles","feedback","soundToggle","reducedMotionToggle","hit","addition","multiplier","payoutSmall","payoutBig","miss"})
            Check(data.FindProperty(field).objectReferenceValue!=null,"Missing score rush binding: "+field);
        var group=(CanvasGroup)data.FindProperty("readout").objectReferenceValue;
        Check(!group.blocksRaycasts&&!group.interactable&&!group.gameObject.activeSelf,"Transient readout starts visible or blocks input.");
        Check(!((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).raycastTarget,"Trails block input.");
        Check(((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).GetComponent<CanvasRenderer>()!=null,"Trails have no native canvas renderer.");
        var voices=data.FindProperty("voices");Check(voices.arraySize>=4,"No bounded audio voice pool.");
        for(int i=0;i<voices.arraySize;i++)
        {
            var source=(AudioSource)voices.GetArrayElementAtIndex(i).objectReferenceValue;
            Check(source!=null&&!source.playOnAwake&&source.spatialBlend==0&&source.outputAudioMixerGroup!=null,"Voice missing mixer routing or plays unexpectedly.");
        }
        foreach(string cue in new[]{"hit","addition","multiplier","payoutSmall","payoutBig","miss"})
        {
            var clip=(AudioClip)data.FindProperty(cue).objectReferenceValue;var samples=new float[clip.samples];clip.GetData(samples,0);
            Check(samples.Any(s=>Mathf.Abs(s)>.01f)&&samples.All(s=>Mathf.Abs(s)<=.86f),"Silent or clipped cue: "+cue);
        }
        return checks;
    }

    /// <summary>Starts an isolated run using actual gameplay commands and installed creature abilities.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";checks=0;var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"LiveVerification.json","{\"passed\":false,\"running\":true}");
        owner.SetDebugSaveResetPending(true);Application.runInBackground=true;owner.StartCoroutine(Run());
    }

    /// <summary>Exercises source motion, sound, skip, new placement, duplicates and immediate full-value failure settlement.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        var prep=new SerializedObject(harbor.SlotPreparation);
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Glass Minnow","Mackerel","Cuttlefish","Peacock Worm","Squid","Cleaner Shrimp","Sea Sponge","Moonlit Ray","Flounder"})
            Check(owner.Rig.TryAcquire(owner.Rig.Revision,Card(name),owner.Rig.Members.Length,out _,out _),"Could not seed source fixture.");
        Force(owner,Card("Cod"));yield return Accept(owner);
        var landing=UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();var rush=landing.GetComponent<LandingScoreRush>();var data=new SerializedObject(rush);
        var reduced=(UnityEngine.UI.Toggle)data.FindProperty("reducedMotionToggle").objectReferenceValue;
        var sound=(UnityEngine.UI.Toggle)data.FindProperty("soundToggle").objectReferenceValue;
        reduced.SetIsOnWithoutNotify(false);sound.SetIsOnWithoutNotify(true);
        yield return Strike(owner,LandingAccuracy.Perfect);
        var receipt=owner.LastLandingStrike;Check(owner.LandingFeedbackPending&&rush.IsPlaying,"Accepted strike did not start a sequence.");
        Check(rush.Sequence.Steps.Count>=3,"Fixture lacks an addition and multiplier.");
        decimal points=owner.Landing.Points;long revision=owner.Landing.Revision;int attempts=owner.Landing.AttemptsRemaining;
        Check(!owner.TryStrikeLanding(owner.Landing.Occurrence,revision),"Duplicate strike accepted during animation.");
        Check(!owner.TryCommitRigPlacement(owner.BuildOccurrence,owner.Rig.Revision,0),"Placement accepted during count-up.");
        var cards=landing.GetComponentsInChildren<TackleRigCardView>();
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps[1].ImpactAt+.015f});
        Check(((TMP_Text)data.FindProperty("sourceName").objectReferenceValue).text.Length>0,"Named source cue missing.");
        Check(((CanvasGroup)data.FindProperty("readout").objectReferenceValue).alpha>0,"Growing readout is invisible.");
        Check(landing.GetComponentsInChildren<AudioSource>().Any(s=>s.isPlaying),"Musical step did not start a routed audio voice.");
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,800),new Vector2Int(2560,1080)})
            Capture(landing,"Combo-"+size.x+"x"+size.y+".png",size.x,size.y);
        var outside=receipt.Score.Contributions.FirstOrDefault(c=>cards.Any(v=>v.BoundInstance.InstanceId==c.SourceId&&v.transform.position.y<cards[0].transform.position.y-600));
        Check(outside!=null,"Fixture lacks an offscreen source.");
        int outsideIndex=Array.FindIndex(rush.Sequence.Steps.ToArray(),s=>s.SourceId==outside.SourceId);
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps[outsideIndex].Time+.1f});
        Check(((UnityEngine.UI.Image)data.FindProperty("sourcePortrait").objectReferenceValue).enabled,"Offscreen source portrait missing.");
        Capture(landing,"Offscreen-Source.png",1920,1080);
        reduced.SetIsOnWithoutNotify(true);rush.Tick();
        Check(rush.ReducedMotion,"Reduced-motion choice was ignored.");
        Check(owner.Landing.Points==points&&owner.Landing.Revision==revision&&owner.Landing.AttemptsRemaining==attempts,"Presentation reran scoring or attempts.");
        owner.CompleteLandingFeedback();yield return null;
        Check(!rush.IsPlaying,"Skip left animation running.");
        Check(landing.GetComponentsInChildren<AudioSource>().All(s=>!s.isPlaying),"Skip left audio playing.");
        Check(cards.All(c=>c.transform.localScale==Vector3.one&&c.transform.localRotation==Quaternion.identity),"Card transforms did not return to rest.");
        if(owner.Landing.State==LandingChallengeState.Active)yield return Land(owner,true);
        else yield return WaitFeedback(owner);
        var build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        Check(build!=null&&owner.AwaitingRigPlacement&&!landing.gameObject.activeInHierarchy,"Winning score did not hand off exclusively to Build.");
        var buildData=new SerializedObject(build);((UnityEngine.UI.Button)buildData.FindProperty("commit").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Cuttlefish","Stormjaw Tuna"})
            for(int i=0;i<29;i++) Check(owner.Rig.TryAcquire(owner.Rig.Revision,Card(name),owner.Rig.Members.Length,out _,out _),"Could not upgrade the big-score fixture.");
        Force(owner,Card("Sardine"));yield return Accept(owner);int priorLevel=owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level;
        reduced.SetIsOnWithoutNotify(false);yield return Strike(owner,LandingAccuracy.Perfect);
        Check(owner.Landing.State==LandingChallengeState.Succeeded&&owner.LandingFeedbackPending,"Winning strike did not resolve immediately with feedback pending.");
        Check(owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level==priorLevel,"Duplicate upgraded before its score was presented.");
        Check(rush.Sequence.Intensity>.8f&&rush.Sequence.Score.Points>500,"Big-score fixture did not amplify the result.");
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps.Last().Time+.05f});
        Capture(landing,"Big-Combo.png",1920,1080);
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.PayoutAt+.12f});
        Canvas.ForceUpdateCanvases();
        var sparks=(LandingRushGraphic)data.FindProperty("particles").objectReferenceValue;
        Check(sparks.canvasRenderer.GetMesh()!=null&&sparks.canvasRenderer.GetMesh().vertexCount>0,"Payout spark mesh did not render.");
        Capture(landing,"Big-Payout.png",1920,1080);
        yield return WaitFeedback(owner);yield return null;
        Check(owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level==priorLevel+1&&owner.RigUpgradeFeedbackVisible,"Duplicate did not upgrade automatically after the payout.");
        Check(owner.LastLandingStrike.Members.Single(m=>m.Definition==Card("Sardine")).Level==priorLevel,"Winning receipt was rewritten by upgrade.");
        double deadline=Time.realtimeSinceStartupAsDouble+5;while(owner.RigUpgradeFeedbackVisible&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        Force(owner,Card("Cod"));yield return Accept(owner);int gold=owner.TotalGold,haul=owner.Rig.HaulValue;
        while(owner.Landing.State==LandingChallengeState.Active){yield return Strike(owner,LandingAccuracy.Miss);if(owner.RunActive)yield return WaitFeedback(owner);}
        Check(!owner.RunActive&&owner.LandingFeedbackPending&&owner.Rig.Members.Length==0,"Failure did not resolve immediately while its feedback remained visible.");
        Check(owner.TotalGold==gold+haul&&owner.LastHaulValue==haul,"Failure changed the full haul payout.");
        Check(owner.LastLandingStrike.Members.Count>0&&landing.GetComponentsInChildren<TackleRigCardView>().Length>0,"Failure lost frozen source cards before feedback completed.");
        Capture(landing,"Failure-Feedback.png",1920,1080);yield return WaitFeedback(owner);
        Check(owner.TotalGold==gold+haul&&!owner.LandingFeedbackPending,"Feedback repeated settlement or failed to finish.");
        Check(!landing.GetComponentsInChildren<LandingActivationView>().Any()&&!rush.IsPlaying,"Finished feedback retained transient effects.");
        Capture(landing,"Failure-Settled.png",1920,1080);
        Result="Passed "+checks+" live score-rush checks";File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");Debug.Log(Result);
    }
    /// <summary>Waits for a robust observed alignment and uses the guarded real strike command.</summary>
    private static IEnumerator Strike(FishingRunController owner,LandingAccuracy accuracy)
    {
        double deadline=Time.realtimeSinceStartupAsDouble+30;long revision=owner.Landing.Revision;
        while(owner.Landing.Revision==revision&&Time.realtimeSinceStartupAsDouble<deadline)
        {
            var model=owner.Landing;
            if(!owner.LandingFeedbackPending)model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance=Math.Abs(model.FishPosition-model.ZonePosition);
            if(!owner.LandingFeedbackPending&&model.CanStrike&&(accuracy==LandingAccuracy.Perfect?distance<(model.PerfectWidth+model.FishWidth)*.5-.01:distance>(model.GoodWidth+model.FishWidth)*.5+.05))owner.TryStrikeLanding(model.Occurrence,model.Revision);
            if(owner.Landing.Revision==revision)yield return null;
        }
        Check(owner.Landing.Revision>revision,"Strike alignment timed out.");
    }
    /// <summary>Waits on the bounded controller gate rather than guessing animation duration.</summary>
    private static IEnumerator WaitFeedback(FishingRunController owner)
    { double until=Time.realtimeSinceStartupAsDouble+5;while(owner.LandingFeedbackPending&&Time.realtimeSinceStartupAsDouble<until)yield return null;Check(!owner.LandingFeedbackPending,"Feedback gate timed out."); }
    /// <summary>Uses existing end-to-end fixture logic for the remaining successful attempts.</summary>
    private static IEnumerator Land(FishingRunController owner,bool success)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Fixture).Invoke(null,new object[]{owner,success});
    /// <summary>Accepts the real Discover result after its cosmetic reels settle.</summary>
    private static IEnumerator Accept(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept",Fixture).Invoke(null,new object[]{owner});
    /// <summary>Constrains only disposable encounter selection, preserving real ability and scoring data.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",Fixture).Invoke(null,new object[]{owner,card});
    /// <summary>Loads installed species without editing their definitions.</summary>
    private static CardDefinition Card(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Captures the actual native Landing canvas using the project render pipeline.</summary>
    private static void Capture(TackleLandingPresentation landing,string name,int width,int height){Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(landing.GetComponent<Canvas>(),name,width,height,Output);}
    /// <summary>Retains an actionable status if a live invariant fails.</summary>
    private static void Check(bool valid,string message){if(!valid){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
