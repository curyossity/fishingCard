using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Checks explicit per-occurrence role choice through the real controller and role-panel pointer callbacks.</summary>
public static class ExplicitAttachmentChoiceChecks
{
    private const string Output="Docs/Validation/ExplicitAttachmentChoice/";
    private static JournalModalHost runner;
    private static bool oldBackground;
    private static int assertions;
    public static string Status { get; private set; }="Not started";
    [Serializable] private sealed class Report { public bool passed;public int assertions;public string error; }
    /// <summary>Starts the finite actual-scene test with no saved-scene or authored-card mutations.</summary>
    public static void Start()
    {
        if(!Application.isPlaying||runner!=null)throw new InvalidOperationException("Enter Play Mode and start once.");
        oldBackground=Application.runInBackground;Application.runInBackground=true;assertions=0;Status="Running";
        runner=new GameObject("Explicit Attachment Choice Verification").AddComponent<JournalModalHost>();runner.StartCoroutine(Guarded());
    }
    /// <summary>Records errors and restores test-owned resources.</summary>
    private static IEnumerator Guarded()
    {
        var sequence=Check();Exception failure=null;
        while(true){bool more;try{more=sequence.MoveNext();}catch(Exception e){failure=e;break;}if(!more)break;yield return sequence.Current;}
        File.WriteAllText(Output+"PlayModeVerification.json",JsonUtility.ToJson(new Report{passed=failure==null,assertions=assertions,error=failure==null?null:failure.ToString()},true));
        Application.runInBackground=oldBackground;var owned=runner;runner=null;UnityEngine.Object.Destroy(owned.gameObject);Status=failure==null?"Passed":"Failed: "+failure;
        if(failure!=null)Debug.LogException(failure);
    }
    /// <summary>Exercises no-selection gating, both roles, repeated species, replacement and non-creature action windows.</summary>
    private static IEnumerator Check()
    {
        var c=UnityEngine.Object.FindFirstObjectByType<FishingRunController>();var view=UnityEngine.Object.FindFirstObjectByType<FishingRunView>();var hud=UnityEngine.Object.FindFirstObjectByType<JournalGameplayHud>();
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;var runtime=(EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime",flags).GetValue(c);
        Action refresh=()=>typeof(FishingRunController).GetMethod("RefreshViews",flags).Invoke(c,null);
        var sardine=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sardine.asset");Require(sardine!=null,"Fixture missing");
        c.StartRun();yield return null;Require(c.CanDescend&&c.IsOpeningBoatVisible,"Boat incorrectly requires an attachment choice");hud.DescendButton.onClick.Invoke();
        while(view.IsEncounterTransitionPlaying)yield return null;
        runtime.SetCurrentEncounter(sardine);refresh();yield return null;
        Require(c.SelectedEncounterChoice==EncounterEffectChoice.None&&view.SelectedEncounterEffectChoice==EncounterEffectChoice.None,"Creature has a default choice");
        Require(!c.CanDescend&&!hud.DescendButton.interactable,"Descend enabled without choice");
        int depth=c.CurrentDepth,count=c.CatchChain.Length;Require(!c.TryDescend()&&depth==c.CurrentDepth&&count==c.CatchChain.Length,"Unselected Descend mutated the run");Require(c.CanSurface&&hud.SurfaceButton.interactable,"Surface was gated by role selection");
        yield return new WaitForSecondsRealtime(.15f);
        ScreenCapture.CaptureScreenshot(Output+"Unselected4K.png");yield return null;
        var current=(CreatureCardView)typeof(FishingRunView).GetField("encounterCardView",flags).GetValue(view);
        var catchButton=(UnityEngine.UI.Button)typeof(CreatureCardView).GetField("catchEffectButton",flags).GetValue(current);
        var baitButton=(UnityEngine.UI.Button)typeof(CreatureCardView).GetField("baitEffectButton",flags).GetValue(current);
        Require(catchButton.IsInteractable()&&baitButton.IsInteractable(),"Disabled Descend locked the role panels");
        var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};catchButton.OnPointerClick(pointer);
        Require(c.SelectedEncounterChoice==EncounterEffectChoice.Catch&&c.CanDescend&&hud.DescendButton.interactable,"Catch click did not enable Descend");
        baitButton.OnPointerClick(pointer);Require(c.SelectedEncounterChoice==EncounterEffectChoice.Bait&&c.CanDescend,"Bait choice unavailable");refresh();Require(c.SelectedEncounterChoice==EncounterEffectChoice.Bait&&view.SelectedEncounterEffectChoice==EncounterEffectChoice.Bait,"Ordinary refresh cleared choice");
        int revision=c.CurrentEncounterRevision;runtime.SetCurrentEncounter(sardine);refresh();
        Require(c.SelectedEncounterChoice==EncounterEffectChoice.None&&!c.CanDescend&&view.SelectedEncounterEffectChoice==EncounterEffectChoice.None,"Same-species replacement retained the previous choice");Require(!c.TryChooseEncounterRole(EncounterEffectChoice.Catch,revision),"Stale occurrence choice accepted");Require(!c.TryChooseEncounterRole(EncounterEffectChoice.None),"None accepted as an explicit role");
        Require(c.TryChooseEncounterRole(EncounterEffectChoice.Bait),"Valid replacement choice rejected");yield return new WaitForSecondsRealtime(.15f);ScreenCapture.CaptureScreenshot(Output+"SelectedBait4K.png");yield return null;
        hud.DescendButton.onClick.Invoke();while(view.IsEncounterTransitionPlaying)yield return null;
        Require(c.CatchChain.Length==count+1&&c.CatchChain[count].IsBait,"Committed creature used the wrong role");
        runtime.SetCurrentEncounter(sardine);refresh();Require(!c.CanDescend&&c.SelectedEncounterChoice==EncounterEffectChoice.None,"Next creature inherited Bait");
        Require(c.TryChooseEncounterRole(EncounterEffectChoice.Catch)&&c.TryDescend(),"Explicit Catch could not commit");while(view.IsEncounterTransitionPlaying)yield return null;Require(c.CatchChain.Length==count+2&&!c.CatchChain[count+1].IsBait,"Catch attachment became Bait");
        runtime.SetCurrentEncounter(null);refresh();Require(c.CanDescend,"Empty/skipped encounter requires a choice");
        var hazard=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Reef Snag.asset");Require(hazard!=null,"Hazard fixture missing");
        runtime.SetCurrentEncounter(hazard);refresh();Require(c.CanDescend&&!c.TryChooseEncounterRole(EncounterEffectChoice.Catch),"Hazard gained a Catch/Bait requirement");
        var apex=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Stormjaw Tuna.asset");Require(apex!=null,"Apex fixture missing");runtime.SetCurrentEncounter(apex);refresh();Require(!c.CanDescend&&c.TryChooseEncounterRole(EncounterEffectChoice.Bait)&&c.CanDescend,"Apex creature did not require a role");
        runtime.GrantCreatureConcealment(JsonUtility.FromJson<CardEffectDefinition>("{\"amount\":1}"),sardine);runtime.SetCurrentEncounter(sardine);refresh();
        Require(c.CurrentEncounterIdentityHidden&&!c.CanDescend,"Concealed creature acquired a default choice");Require(c.TryChooseEncounterRole(EncounterEffectChoice.Bait)&&c.TryDescend(),"Hidden creature could not be committed explicitly");while(view.IsEncounterTransitionPlaying)yield return null;
        Require(c.CatchChain[c.CatchChain.Length-1].IsBait,"Concealment reveal changed selected role");
        c.StartRun();yield return null;Require(c.IsOpeningBoatVisible&&c.SelectedEncounterChoice==EncounterEffectChoice.None,"Restart retained a creature choice");
    }
    /// <summary>Stops at the first violated gameplay or input contract.</summary>
    private static void Require(bool value,string message){assertions++;if(!value)throw new InvalidOperationException(message);}
}
