using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks real preview consequences, committed presentation, upgrades and interruption without touching player progression.</summary>
public static class RigBuildPolishChecks
{
    public const string Output="Docs/Validation/BuildPolish/";
    private const BindingFlags Hidden=BindingFlags.Static|BindingFlags.NonPublic;
    public static string Result {get;private set;}="Not run";
    private static int checks;
    /// <summary>Verifies model-backed deltas and installed native presentation ownership.</summary>
    public static int Validate()
    {
        checks=0;var rig=new RigRuntime();foreach(string name in new[]{"Sardine","Mackerel","Glass Minnow"})Acquire(rig,Card(name));
        var before=rig.Members.Select(m=>m.CreateSnapshot()).ToArray();long revision=rig.Revision;int haul=rig.HaulValue;
        Check(rig.TryPreview(Card("Cod"),1,out var after,out _,out _),"Cannot preview school break.");
        var changes=RigBuildChange.Compare(before,after);var sardine=changes.Single(c=>c.SourceId==before[0].InstanceId);
        Check(sardine.ShortText=="Links 2 → 0"&&sardine.Description.Contains("Bonus 6 → 0")&&sardine.Direction<0,"School break delta is not derived from actual counts/amounts.");
        Check(changes.Single(c=>c.SourceId==before[1].InstanceId).Description.Contains("Sale 20 → 15"),"Continuous recipient sale change missing.");
        Check(rig.Revision==revision&&rig.HaulValue==haul&&rig.Members.Length==3,"Comparison mutated the committed rig.");
        Check(RigBuildChange.Compare(before,before).Length==0,"Unchanged rig gained synthetic consequences.");
        Acquire(rig,Card("Sardine"));var upgraded=RigBuildChange.Compare(before,rig.Members).Single(c=>c.SourceId==before[0].InstanceId);
        Check(upgraded.Description.Contains("Bonus 6 → 8")&&before[0].Level==1&&rig.Members[0].Level==2,"Upgrade comparison lost the frozen previous power.");
        var root=AssetDatabase.LoadAssetAtPath<GameObject>(RigBuildingSetup.PrefabPath);var view=root.GetComponent<RigBuildPresentation>();Check(view!=null,"Missing Build feedback owner.");
        var data=new SerializedObject(view);foreach(string field in new[]{"flightLayer","changeTemplate","receiptChange","levelLabel","voice","clipSnap","upgradeSound"})Check(data.FindProperty(field).objectReferenceValue!=null,"Missing Build binding: "+field);
        Check(!((RectTransform)data.FindProperty("flightLayer").objectReferenceValue).GetComponent<CanvasGroup>().blocksRaycasts,"Flight layer intercepts input.");
        Check(!((TMP_Text)data.FindProperty("changeTemplate").objectReferenceValue).raycastTarget,"Change labels intercept card input.");
        var voice=(AudioSource)data.FindProperty("voice").objectReferenceValue;Check(!voice.playOnAwake&&voice.spatialBlend==0&&voice.outputAudioMixerGroup!=null,"Build sound is unrouted or plays on load.");
        return checks;
    }
    /// <summary>Starts the actual Discover/Land/Build flow in a disposable run with persistence disabled.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";checks=0;Directory.CreateDirectory(Output);Application.runInBackground=true;
        var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;owner.SetDebugSaveResetPending(true);owner.StartCoroutine(Run());
    }
    /// <summary>Exercises native ghost layout, consequence labels, exact-once placement, distinct upgrade feedback and view disable cleanup.</summary>
    private static IEnumerator Run()
    {
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Mackerel","Glass Minnow"})Acquire(owner.Rig,Card(name));
        Force(owner,Card("Cod"));yield return Accept(owner);yield return Land(owner);
        var build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();var data=new SerializedObject(build);var presentation=build.GetComponent<RigBuildPresentation>();
        var rail=(RectTransform)data.FindProperty("railContent").objectReferenceValue;var ghost=presentation.Ghost;
        Check(ghost!=null&&ghost.gameObject.activeInHierarchy&&ghost.BoundInstance.Definition==Card("Cod"),"Selected insertion has no candidate ghost.");
        Check(!ghost.GetComponent<CanvasGroup>().blocksRaycasts&&!ghost.InspectionTarget.interactable&&ghost.GetComponent<CanvasGroup>().alpha<1,"Ghost is not a non-intercepting preview.");
        int haul=owner.Rig.HaulValue;long revision=owner.Rig.Revision;var original=owner.Rig.Members.Select(m=>m.InstanceId).ToArray();
        var gaps=rail.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.gameObject.activeSelf).ToArray();Check(gaps.Length==4,"Ghost introduced an extra legal insertion command.");
        gaps[1].onClick.Invoke();yield return new WaitForSecondsRealtime(.20f);
        Check(owner.Rig.Revision==revision&&owner.Rig.HaulValue==haul&&original.SequenceEqual(owner.Rig.Members.Select(m=>m.InstanceId)),"Ghost preview acquired or reordered creatures.");
        var cards=rail.GetComponentsInChildren<TackleRigCardView>().Where(c=>c!=ghost&&c.BoundInstance!=null).ToArray();
        float left=cards.Single(c=>c.BoundInstance.Definition.DisplayName=="Sardine").transform.localPosition.x,right=cards.Single(c=>c.BoundInstance.Definition.DisplayName=="Mackerel").transform.localPosition.x;
        Check(ghost.transform.localPosition.x>left&&ghost.transform.localPosition.x<right,"Ghost is not between the selected neighbors.");
        Check(build.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Links 2 → 0"&&t.enabled),"Affected creature has no visible placement consequence label.");
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,800),new Vector2Int(2560,1080)})
            SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(),"Ghost-"+size.x+"x"+size.y+".png",size.x,size.y,Output);
        long occurrence=owner.BuildOccurrence;int depth=owner.CurrentDepth;
        ((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).onClick.Invoke();
        Check(owner.RigPlacementFeedbackVisible&&owner.Rig.Members.Length==4&&!owner.AwaitingRigPlacement,"Placement animation precedes authoritative acquisition.");
        Check(!owner.TryCommitRigPlacement(occurrence,revision,1)&&!owner.TryAcceptSlotMachine(owner.SlotMachine.Revision),"Repeated placement or next encounter bypasses the feedback gate.");
        Check(owner.LastRigAcquisition.PreviousMembers.Length==3&&owner.LastRigAcquisition.Members.Length==4,"Receipt did not preserve the actual before/after rigs.");
        Check(!ghost.gameObject.activeSelf,"Committed placement retained its preview ghost.");
        yield return new WaitForSecondsRealtime(.15f);
        if(owner.RigPlacementFeedbackVisible)SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(),"Placement-Flight.png",output:Output);
        yield return RigBuildingSceneChecks.WaitAcquisitionFeedback(owner);
        Check(!build.gameObject.activeInHierarchy&&owner.SlotMachine.Active&&owner.CurrentDepth==depth+1,"Placement did not continue exactly once after its short feedback.");
        Force(owner,Card("Cod"));yield return Accept(owner);yield return Land(owner);yield return null;
        build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();data=new SerializedObject(build);presentation=build.GetComponent<RigBuildPresentation>();
        Check(owner.RigUpgradeFeedbackVisible&&owner.Rig.FindSpecies(Card("Cod").UniqueId).Level==2&&owner.Rig.Members.Length==4,"Duplicate did not strengthen its existing position.");
        Check(!((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).gameObject.activeSelf,"Upgrade requires a placement confirmation.");
        var receiptText=(TMP_Text)new SerializedObject(presentation).FindProperty("receiptChange").objectReferenceValue;
        Check(receiptText.text.Contains("ABILITY UPGRADED")&&receiptText.text.Contains("Bonus 8 → 10"),"Upgrade reveal does not identify the strengthened effect.");
        SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(),"Upgrade-Reveal.png",output:Output);
        var upgradedMember=owner.Rig.FindSpecies(Card("Cod").UniqueId);var previous=owner.LastRigAcquisition.PreviousMembers.Single(m=>m.InstanceId==upgradedMember.InstanceId);
        Check(previous.Level==1&&upgradedMember.Level==2,"Upgrade overwrote the previous snapshot.");
        build.enabled=false;yield return null;
        Check(!owner.RigAcquisitionFeedbackVisible&&owner.SlotMachine.Active,"Disabling Build retained the committed-feedback input gate.");
        Check(owner.Rig.Members.Length==4&&owner.Rig.FindSpecies(Card("Cod").UniqueId).Level==2,"Interrupted feedback repeated acquisition.");
        Result="Passed "+checks+" live Build-polish checks";File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");
    }
    /// <summary>Constrains only the disposable encounter selector through the existing real-flow fixture.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",Hidden).Invoke(null,new object[]{owner,card});
    /// <summary>Uses guarded real Discover acceptance after its native animation finishes.</summary>
    private static IEnumerator Accept(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept",Hidden).Invoke(null,new object[]{owner});
    /// <summary>Uses real strike commands through the shared end-to-end fixture.</summary>
    private static IEnumerator Land(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Hidden).Invoke(null,new object[]{owner,true});
    /// <summary>Loads installed content without mutating creature definitions.</summary>
    private static CardDefinition Card(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Seeds only the disposable runtime rig through its transactional owner.</summary>
    private static void Acquire(RigRuntime rig,CardDefinition card)=>Check(rig.TryAcquire(rig.Revision,card,rig.Members.Length,out _,out _),"Fixture acquisition failed.");
    /// <summary>Preserves a precise failure result for asynchronous verification.</summary>
    private static void Check(bool valid,string message){if(!valid){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
