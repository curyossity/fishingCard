using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Exercises hit attribution through actual strikes, inspection, handoffs and failure with persistence disabled.</summary>
public static class LandingStrikeFeedbackChecks
{
    public const string Output = "Docs/Validation/LandingStrikeFeedback/";
    private const BindingFlags Private = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Validates all authoring references and metadata without starting a run.</summary>
    public static int ValidateAssets()
    {
        checks=0;
        foreach(string path in new[]{LandingChallengeSetup.PrefabPath,RigBuildingSetup.PrefabPath})
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);var host=root.GetComponent<LandingStrikeFeedback>();
            Check(host!=null,"Missing phase feedback: "+path); var data=new SerializedObject(host);
            foreach(string field in new[]{"chips","chipPrefab","rowPrefab","activationPrefab","pulseLayer","equation","attemptFeedback"})
                Check(data.FindProperty(field).objectReferenceValue!=null,"Missing feedback binding: "+field);
            var layer=(RectTransform)data.FindProperty("pulseLayer").objectReferenceValue;
            Check(!layer.GetComponent<CanvasGroup>().blocksRaycasts,"Activation layer blocks input.");
            Check(root.GetComponent<TackleCardTooltip>()!=null,"Contribution inspection has no overlay.");
        }
        var rig=new RigRuntime();var sardine=Card("Sardine");var minnow=Card("Glass Minnow");var mackerel=Card("Mackerel");
        Acquire(rig,sardine);Acquire(rig,minnow);Acquire(rig,mackerel);
        var snapshot=rig.SnapshotForLanding(out _,out _);var rule=snapshot[0].Rules.Single();
        Check(rule.Explanation.Contains("2 matching creatures"),"Resolved positional count missing.");
        Check(rule.SupportingIds.SequenceEqual(new[]{rig.Members[1].InstanceId,rig.Members[2].InstanceId}),"Wrong Schooling enablers.");
        string frozen = rule.Explanation; decimal amount = rule.Amount;
        Acquire(rig,sardine);Check(rule.Explanation==frozen&&rig.SnapshotForLanding(out _,out _)[0].Rules.Single().Amount>amount,"Frozen explanation changed after upgrade.");
        return checks;
    }

    /// <summary>Starts a fresh real-phase fixture without writing player progression.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";checks=0;var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        owner.SetDebugSaveResetPending(true);Application.runInBackground=true;owner.StartCoroutine(Run());
    }

    /// <summary>Checks direct versus enabling creatures, offscreen sources, assistance, attempts and terminal receipt ownership.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        var prep=new SerializedObject(harbor.SlotPreparation);
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Glass Minnow","Mackerel","Cuttlefish","Peacock Worm","Squid","Cleaner Shrimp","Sea Sponge","Moonlit Ray","Flounder"})Acquire(owner.Rig,Card(name));
        Force(owner,Card("Cod"));yield return Accept(owner);
        var landing=UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();var feedback=landing.GetComponent<LandingStrikeFeedback>();
        var data=new SerializedObject(feedback);
        string assistanceText=((TMP_Text)data.FindProperty("assistanceTotals").objectReferenceValue).text;
        Check(assistanceText.Contains("GOOD +")&&assistanceText.Contains("PERFECT +")&&assistanceText.Contains("SLOWER"),"Active assistance is not visible.");
        yield return Strike(owner,LandingAccuracy.Perfect);
        var receipt=owner.LastLandingStrike;Check(receipt!=null&&receipt.Score==owner.Landing.LastScore,"Strike receipt missing.");
        Check(receipt.Score.Contributions.Any(c=>c.Bonus>0)&&receipt.Score.Contributions.Any(c=>c.Multiplier>1),"Fixture lacks additions and multipliers.");
        Check(feedback.Shown==receipt&&feedback.ContributorCount==receipt.Score.Contributions.Count(c=>c.Bonus!=0||c.Multiplier!=1),"Score sources were omitted or assistance was counted as score.");
        decimal points=owner.Landing.Points;int attempts=owner.Landing.AttemptsRemaining;long revision=owner.Landing.Revision;
        var cards=landing.GetComponentsInChildren<TackleRigCardView>();
        Check(cards.First(c=>c.BoundInstance.Definition.DisplayName=="Cuttlefish").transform.position.y<cards.First().transform.position.y-500,"Fixture lacks an offscreen scorer.");
        var chipContent=(RectTransform)data.FindProperty("chips").objectReferenceValue;
        var chip=chipContent.GetComponentsInChildren<LandingContributionView>().First(v=>v.Title=="Sardine");
        EventSystem.current.SetSelectedGameObject(chip.gameObject);
        Check(landing.GetComponent<TackleCardTooltip>().IsVisible,"Source inspection missing.");
        var tooltip=new SerializedObject(landing.GetComponent<TackleCardTooltip>());var panel=(RectTransform)tooltip.FindProperty("panel").objectReferenceValue;
        var stage=landing.transform.Find("1920x1080 Stage") as RectTransform;
        var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(stage,panel);
        var track=(RectTransform)new SerializedObject(landing).FindProperty("track").objectReferenceValue;
        var trackBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(stage,track);
        Check(bounds.min.y>trackBounds.max.y,"Contribution explanation covers the timing track.");
        Capture(landing.gameObject,"Inspected-Source-1920x1080.png",1920,1080);
        EventSystem.current.SetSelectedGameObject(null);
        // Freeze only the presentation clock for reproducible screenshots of the pulse peak.
        typeof(LandingStrikeFeedback).GetField("started",Instance).SetValue(feedback,Time.realtimeSinceStartupAsDouble-.12);
        foreach(var size in new[]{new Vector2Int(1280,800),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})Capture(landing.gameObject,"Strike-"+size.x+"x"+size.y+".png",size.x,size.y);
        Check(owner.Landing.Points==points&&owner.Landing.AttemptsRemaining==attempts&&owner.Landing.Revision==revision,"Feedback changed gameplay state.");
        CheckButton((UnityEngine.UI.Button)new SerializedObject(landing.GetComponent<LandingChallengeView>()).FindProperty("strike").objectReferenceValue);
        double feedbackUntil=Time.realtimeSinceStartupAsDouble+4;
        while(owner.LandingFeedbackPending&&Time.realtimeSinceStartupAsDouble<feedbackUntil)yield return null;
        Check(!landing.GetComponentsInChildren<LandingActivationView>().Any(),"Activation did not return to rest.");
        yield return Strike(owner,LandingAccuracy.Miss);
        Check(owner.Landing.LastScore.Contributions.Any(c=>c.Attempts>0),"Recovery grant was not attributed.");
        Check(!owner.Landing.LastScore.Contributions.Any(c=>c.Bonus!=0||c.Multiplier!=1),"Ordinary Miss credited scoring sources.");
        Check(((TMP_Text)data.FindProperty("attemptFeedback").objectReferenceValue).text.Contains("Cleaner Shrimp"),"Attempt grant source missing.");
        var recoveryLabel=(TMP_Text)data.FindProperty("attemptFeedback").objectReferenceValue;recoveryLabel.ForceMeshUpdate();
        Check(!recoveryLabel.isTextOverflowing,"Recovery attribution is clipped inside the attempts display.");
        Capture(landing.gameObject,"Attempt-Recovery.png",1920,1080);
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Private).Invoke(null,new object[]{owner,true});yield return null;
        var build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();var buildFeedback=build.GetComponent<LandingStrikeFeedback>();
        var winning=owner.LastLandingStrike;
        Check(buildFeedback.Shown==winning&&winning.Score.Points>0,"Winning hit was lost during Build handoff.");
        Check(winning.Members.All(m=>m.Definition.UniqueId!=owner.PendingRigCreature.UniqueId),"New creature credited its own acquisition.");
        Capture(build.gameObject,"Winning-Hit-Build.png",1920,1080);
        var commit=(UnityEngine.UI.Button)new SerializedObject(build).FindProperty("commit").objectReferenceValue;CheckButton(commit);commit.onClick.Invoke();yield return null;
        Check(owner.Landing.LastScore==null&&owner.LastLandingStrike==winning,"Following encounter changed the frozen winning receipt.");
        Force(owner,Card("Sardine"));yield return Accept(owner);
        Check(feedback.Shown==null&&feedback.ContributorCount==0&&!landing.GetComponentsInChildren<LandingActivationView>().Any(),"A new encounter inherited old hit feedback.");
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Private).Invoke(null,new object[]{owner,true});yield return null;
        var upgraded=owner.Rig.FindSpecies(Card("Sardine").UniqueId);
        Check(upgraded.Level==2&&owner.RigUpgradeFeedbackVisible,"Duplicate did not use automatic upgrade feedback.");
        Check(owner.LastLandingStrike.Members.Single(m=>m.Definition==Card("Sardine")).Level==1,"Upgrade rewrote pre-hit source level.");
        Check(buildFeedback.Shown==owner.LastLandingStrike,"Duplicate handoff lost its winning hit.");
        Capture(build.gameObject,"Winning-Hit-Upgrade.png",1920,1080);
        double until=Time.realtimeSinceStartupAsDouble+4;while(owner.RigUpgradeFeedbackVisible&&Time.realtimeSinceStartupAsDouble<until)yield return null;
        yield return Accept(owner);
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Private).Invoke(null,new object[]{owner,false});yield return null;
        Check(!owner.RunActive&&owner.Rig.Members.Length==0,"Failure did not clear the temporary rig.");
        Check(owner.LastLandingStrike!=null&&owner.LastLandingStrike.Members.Count>0,"Failure cleanup lost source identities.");
        Result="Passed "+checks+" live strike-feedback checks";
        File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");Debug.Log(Result);
    }
    /// <summary>Captures authored phase geometry and all native feedback layers.</summary>
    private static void Capture(GameObject phase,string name,int width,int height)
    { Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(phase.GetComponent<Canvas>(),name,width,height,Output); }
    /// <summary>Checks the real raycast destination while the overlay is visible.</summary>
    private static void CheckButton(UnityEngine.UI.Button button)
    {
        var rect=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject)==button.gameObject,"Feedback blocks an existing gameplay button.");
    }
    /// <summary>Waits for an actual robust strike alignment without changing motion or score.</summary>
    private static IEnumerator Strike(FishingRunController owner,LandingAccuracy accuracy)
    {
        var model=owner.Landing;double until=Time.realtimeSinceStartupAsDouble+30;long revision=model.Revision;
        while(model.Revision==revision&&Time.realtimeSinceStartupAsDouble<until)
        {
            model.AdvanceTo(Time.realtimeSinceStartupAsDouble);double distance=Math.Abs(model.FishPosition-model.ZonePosition);
            if(model.CanStrike&&(accuracy==LandingAccuracy.Perfect?distance<model.PerfectWidth*.15:distance>(model.GoodWidth+model.FishWidth)*.5+.05))owner.TryStrikeLanding(model.Occurrence,model.Revision);
            if(model.Revision==revision)yield return null;
        }
        Check(model.Revision>revision,"Timed out waiting for strike.");
    }
    /// <summary>Uses the existing deterministic selector fixture without editing creature definitions.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",Private).Invoke(null,new object[]{owner,card});
    /// <summary>Uses real Discover acceptance after its presentation has settled.</summary>
    private static IEnumerator Accept(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept",Private).Invoke(null,new object[]{owner});
    /// <summary>Loads installed species content without altering authoring.</summary>
    private static CardDefinition Card(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Seeds only the disposable test rig through its transactional owner.</summary>
    private static void Acquire(RigRuntime rig,CardDefinition card)=>Check(rig.TryAcquire(rig.Revision,card,rig.Members.Length,out _,out _),"Could not seed feedback fixture.");
    /// <summary>Retains a precise failure status for asynchronous editor checks.</summary>
    private static void Check(bool condition,string message){if(!condition){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
