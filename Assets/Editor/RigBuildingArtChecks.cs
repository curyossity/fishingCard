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

/// <summary>Verifies the approved Build artwork, current-level text and real automatic upgrade handoff without writing player progression.</summary>
public static class RigBuildingArtChecks
{
    public const string Output="Docs/Validation/TackleBuildArt/";
    public static string Result { get; private set; }="Not run";
    private static int checks;
    private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;

    /// <summary>Checks independent source pixels, native dynamic fields and role-free prefab/scene bindings.</summary>
    public static int ValidateAssets()
    {
        checks=0; Check(RigBuildingChecks.ValidateAssets()>0,"Build binding checks failed.");
        foreach(var path in Directory.GetFiles(RigBuildingSetup.ArtPath,"*.png"))
        {
            Check(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(RigBuildingSetup.Source+Path.GetFileName(path))),"Build source pixels changed: "+path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Check(importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&!importer.isReadable&&importer.textureCompression==TextureImporterCompression.Uncompressed&&importer.npotScale==TextureImporterNPOTScale.None,"Build sprite settings changed: "+path);
        }
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(RigBuildingSetup.PrefabPath);
        foreach(var name in new[]{"build-gap-idle","build-gap-selected","build-selection-caret","build-rail-card","build-machine"})
            CheckTransparentSilhouette(RigBuildingSetup.ArtPath+name+".png");
        CheckIntegratedHousing(prefab);
        CheckIntegratedFrames();
        Check(!prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true).Any(i=>i.name=="build-effect"||i.name=="build-portrait"),"Assembled frame overlays remain on the integrated housing.");
        foreach(var text in prefab.GetComponentsInChildren<TMP_Text>(true))
            Check(!text.text.Contains("NEW SPECIES")&&!text.text.Contains("DUPLICATE")&&!text.text.Contains("CATCH")&&!text.text.Contains("BAIT"),"Obsolete Build wording: "+text.text);
        var ability=new RigAbilitySpec("Schooling","",new[]{new RigAbilityEffect(RigAbilityKind.LandingMultiplier,RigAbilityTrigger.Continuous,LandingAccuracyFilter.Successful,.2f,.2f,RigPositionCount.ContiguousOther,"Schooling")});
        Check(RigAbilityText.Describe(ability,1,false)=="Successful strikes gain +0.2x per other contiguous Schooling creature.","Level 1 effect description is wrong.");
        Check(RigAbilityText.Describe(ability,3,false)=="Successful strikes gain +0.6x per other contiguous Schooling creature.","Level 3 still displays the base effect.");
        Check(RigAbilityText.Describe(new RigAbilitySpec("","",null),1)=="No active ability.","An unassigned creature gained a fabricated effect.");
        return checks;
    }

    /// <summary>Rejects opaque screen crops without making shipped textures CPU-readable.</summary>
    private static void CheckTransparentSilhouette(string path)
    {
        var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
        try
        {
            Check(texture.LoadImage(File.ReadAllBytes(path)),"Cannot inspect sprite alpha: "+path);
            var pixels=texture.GetPixels32();int width=texture.width,height=texture.height;
            Check(pixels[0].a==0&&pixels[width-1].a==0&&pixels[(height-1)*width].a==0&&pixels[pixels.Length-1].a==0,
                "Sprite corners retain a cropped background: "+path);
            Check(pixels.Count(p=>p.a==0)>pixels.Length/200&&pixels.Any(p=>p.a>0),
                "Sprite must retain both visible artwork and transparent space: "+path);
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
    }

    /// <summary>Rejects tray overlays and nonuniform housing scaling; the complete authored source must reach Unity byte-for-byte.</summary>
    private static void CheckIntegratedHousing(GameObject prefab)
    {
        const string source="Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine/integrated-build-machine-v2.png";
        var images=prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        var housing=images.Single(i=>i.name=="build-machine");
        Check(File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(AssetDatabase.GetAssetPath(housing.sprite))),"Integrated housing was cropped, repainted or resampled during export.");
        Check(housing.rectTransform.sizeDelta==new Vector2(1310,681),"Housing layout no longer matches its authored recess.");
        Check(Math.Abs(housing.rectTransform.rect.width/housing.rectTransform.rect.height-housing.sprite.rect.width/housing.sprite.rect.height)<.001f,"Housing is stretched nonuniformly.");
        Check(!images.Any(i=>i.sprite!=null&&AssetDatabase.GetAssetPath(i.sprite).Contains("build-tray")),"A separate tray background still overlays the machine.");
        Check(!File.Exists(RigBuildingSetup.ArtPath+"build-tray.png")&&!File.Exists(RigBuildingSetup.ArtPath+"build-tray-three.png"),"Retired cropped tray assets remain in runtime art.");
        var view=new SerializedObject(prefab.GetComponent<RigBuildingView>());
        var rail=(UnityEngine.UI.ScrollRect)view.FindProperty("railScroll").objectReferenceValue;
        Check(rail.viewport.rect.width==1020,"Insertion viewport does not fit the built-in wood opening.");
        var heading=images.Single(i=>i.name=="build-rail-heading").rectTransform;
        float frameBottom=TackleMachineGeometry.StageBounds.y+502f/904f*TackleMachineGeometry.StageBounds.height;
        Check(-heading.anchoredPosition.y>=frameBottom,"Insertion heading covers the shared creature/effects frame borders.");
    }

    /// <summary>Checks native Landing frame borders and placement while keeping the old Build rig/casing pixels and removing only the effects-pane trough.</summary>
    private static void CheckIntegratedFrames()
    {
        const string source="Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine/";
        var paths=new[]{source+"integrated-build-machine-source.png",AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Sprite>(RigBuildingSetup.ArtPath+"build-machine.png")),TackleLandingArtSetup.HousingSource,source+"FramesV2/upper-frame-mask.png",source+"FramesV2/effects-interior-mask.png"};
        var textures=paths.Select(_=>new Texture2D(2,2,TextureFormat.RGBA32,false)).ToArray();
        try
        {
            for(int i=0;i<paths.Length;i++)textures[i].LoadImage(File.ReadAllBytes(paths[i]));
            Check(textures.All(t=>t.width==1739&&t.height==904),"The shared frame coordinate grid changed.");
            var old=textures[0].GetPixels32();var result=textures[1].GetPixels32();var landing=textures[2].GetPixels32();var upper=textures[3].GetPixels32();var fill=textures[4].GetPixels32();
            int alphaDifferent=0,outsideDifferent=0,portraitDifferent=0,borderDifferent=0,barDifferent=0;
            var portrait=new RectInt(216,186,614,316);var effects=new RectInt(844,186,670,316);var bar=new RectInt(861,414,634,69);
            for(int i=0;i<old.Length;i++)
            {
                var point=new Vector2Int(i%1739,903-i/1739);
                bool oldDifferent=result[i].r!=old[i].r||result[i].g!=old[i].g||result[i].b!=old[i].b;
                bool landDifferent=result[i].r!=landing[i].r||result[i].g!=landing[i].g||result[i].b!=landing[i].b;
                if(result[i].a!=old[i].a)alphaDifferent++;
                if(upper[i].a==0&&old[i].a>0&&oldDifferent)outsideDifferent++;
                if(portrait.Contains(point)&&landDifferent)portraitDifferent++;
                if(effects.Contains(point)&&fill[i].a==0&&landDifferent)borderDifferent++;
                if(bar.Contains(point)&&landDifferent)barDifferent++;
            }
            Check(alphaDifferent==0&&outsideDifferent==0,"Adding frames changed the Build casing, wooden rig or other protected artwork.");
            Check(portraitDifferent==0,"Creature frame differs from Landing or moved on the native grid.");
            Check(borderDifferent==0,"Effects frame border differs from Landing or changed size/position.");
            Check(barDifferent>0,"The Landing progress trough remains in the Build effects frame.");
            Check(!File.Exists(RigBuildingSetup.ArtPath+"build-portrait.png")&&!File.Exists(RigBuildingSetup.ArtPath+"build-effect.png"),"Superseded runtime frame sprites remain.");
        }
        finally{foreach(var texture in textures)UnityEngine.Object.DestroyImmediate(texture);}
    }

    /// <summary>Starts a fresh runtime-only fixture; source creature definitions and persistent saves remain untouched.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter fresh comparison Play mode.");
        var controller=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        controller.SetDebugSaveResetPending(true); Application.runInBackground=true; checks=0; Result="Running";
        controller.StartCoroutine(Run());
    }

    /// <summary>Captures empty and populated trays after actual Landing success and verifies native controls and guarded automatic continuation.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        Check(!owner.RunActive,"Expected a fresh Harbor.");
        var original=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sardine.asset");
        var sardine=UnityEngine.Object.Instantiate(original);sardine.name="Sardine presentation fixture";
        var authored=new SerializedObject(sardine);var ability=authored.FindProperty("primaryAbility");
        ability.FindPropertyRelative("name").stringValue="Schooling";ability.FindPropertyRelative("description").stringValue="";
        var effects=ability.FindPropertyRelative("effects");effects.arraySize=1;var effect=effects.GetArrayElementAtIndex(0);
        effect.FindPropertyRelative("kind").enumValueIndex=(int)RigAbilityKind.LandingMultiplier;
        effect.FindPropertyRelative("trigger").enumValueIndex=(int)RigAbilityTrigger.Continuous;
        effect.FindPropertyRelative("accuracy").enumValueIndex=(int)LandingAccuracyFilter.Successful;
        effect.FindPropertyRelative("amount").floatValue=.2f;effect.FindPropertyRelative("amountPerLevel").floatValue=.2f;
        effect.FindPropertyRelative("count").enumValueIndex=(int)RigPositionCount.ContiguousOther;effect.FindPropertyRelative("tag").stringValue="Schooling";
        effect.FindPropertyRelative("perPosition").boolValue=true;authored.ApplyModifiedPropertiesWithoutUndo();
        CardDefinition mackerel=Load("Mackerel"),glass=Load("Glass Minnow"),bass=Load("Sea Bass");
        Start(harbor);yield return null;
        Force(owner,sardine);yield return Accept(owner);yield return Land(owner,true);
        var empty=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        Check(owner.Rig.Members.Length==0&&empty!=null,"First landed species did not open an empty insertion tray.");
        Capture(empty,"EmptyRig-1920x1080.png");
        var emptyData=new SerializedObject(empty);
        var emptyRail=(RectTransform)emptyData.FindProperty("railContent").objectReferenceValue;
        var emptyButtons=emptyRail.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.gameObject.activeSelf).ToArray();
        Check(emptyButtons.Length==1,"Empty rig must have one insertion target.");CheckHit(emptyButtons[0]);
        ((UnityEngine.UI.Button)emptyData.FindProperty("commit").objectReferenceValue).onClick.Invoke();yield return null;
        yield return Accept(owner);yield return Land(owner,false);yield return null;
        owner.DismissLandingResult();harbor.ReturnToHarbor();
        Start(harbor);yield return null;
        Acquire(owner,mackerel,0);Acquire(owner,glass,1);Acquire(owner,bass,2);
        Force(owner,sardine);yield return Accept(owner);yield return Land(owner,true);
        var view=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();var data=new SerializedObject(view);
        Check(view!=null&&view.gameObject.activeInHierarchy&&owner.Rig.Members.Length==3,"New-species screen did not reserve placement.");
        Check(Text(data,"levelLabel")=="LV 1"&&Text(data,"saleLabel")=="7"&&Text(data,"details").Contains("+0.2x"),"Level 1 fields do not use live values.");
        Check(Text(data,"previewLabel")=="31 > 38","Preview haul differs from the approved fixture.");
        Capture(view,"Level1-1920x1080.png");
        var rail=(RectTransform)data.FindProperty("railContent").objectReferenceValue;
        var buttons=rail.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.gameObject.activeSelf).ToArray();
        Check(buttons.Length==4,"Three species must offer four insertion gaps.");
        foreach(var button in buttons)CheckHit(button);
        var before=owner.Rig.Members.Select(m=>m.InstanceId).ToArray();int value=owner.Rig.HaulValue;
        buttons[0].onClick.Invoke();Check(owner.Rig.Members.Select(m=>m.InstanceId).SequenceEqual(before)&&owner.Rig.HaulValue==value,"Selecting a gap mutated the rig.");
        buttons[3].onClick.Invoke();
        long occurrence=owner.BuildOccurrence,revision=owner.Rig.Revision;
        ((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).onClick.Invoke();yield return null;
        Check(!owner.TryCommitRigPlacement(occurrence,revision,3)&&owner.Rig.Members.Length==4,"A stale placement acquired twice.");
        yield return Accept(owner);yield return Land(owner,false);yield return null;
        owner.DismissLandingResult();harbor.ReturnToHarbor();
        Start(harbor);yield return null;
        Acquire(owner,sardine,0);Acquire(owner,sardine,-1);Acquire(owner,mackerel,1);Acquire(owner,glass,2);
        Force(owner,sardine);yield return Accept(owner);yield return Land(owner,true);
        view=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();data=new SerializedObject(view);
        yield return null;
        Check(owner.RigUpgradeFeedbackVisible&&view!=null,"Automatic upgrade feedback is missing.");
        Check(owner.Rig.Members.Length==3&&owner.Rig.Members[0].Level==3&&owner.Rig.HaulValue==36,"Upgrade changed order, count or accumulated sale value.");
        Check(Text(data,"levelLabel")=="LV 2 > 3"&&Text(data,"levelDelta")=="+1"&&Text(data,"saleLabel")=="21"&&Text(data,"details").Contains("+0.6x"),"Upgrade fields do not show the resulting level/effect/value.");
        Check(!((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).gameObject.activeSelf,"Upgrade requires an extra confirmation.");
        Check(!rail.gameObject.activeInHierarchy||!view.GetComponentsInChildren<UnityEngine.UI.Button>().Any(b=>b.gameObject.activeInHierarchy),"Insertion controls remain during an upgrade.");
        Check(!UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(FindObjectsInactive.Include).gameObject.activeInHierarchy&&!UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>(FindObjectsInactive.Include).gameObject.activeInHierarchy,"Phases overlap during upgrade feedback.");
        Check(!owner.TryAcceptSlotMachine(owner.SlotMachine.Revision)&&!owner.CompleteRigUpgradeFeedback(-1),"Commands bypassed upgrade feedback identity.");
        Capture(view,"Level3-1920x1080.png");
        foreach(var size in new[]{new Vector2Int(1280,800),new Vector2Int(2560,1080)})
            Capture(view,"Level3-"+size.x+"x"+size.y+".png",size.x,size.y);
        double deadline=Time.realtimeSinceStartupAsDouble+8;
        while(owner.RigUpgradeFeedbackVisible&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        Check(!owner.RigUpgradeFeedbackVisible&&owner.SlotMachine.Active&&!view.gameObject.activeInHierarchy,"Upgrade did not continue automatically.");
        Check(!owner.CompleteRigUpgradeFeedback(owner.LastRigAcquisition.Occurrence),"Completed feedback was processed twice.");
        UnityEngine.Object.Destroy(sardine);
        Result="Passed "+checks+" Build art/live checks";
        File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+",\"level1Coefficient\":0.2,\"level3Coefficient\":0.6,\"level3Value\":21,\"automaticUpgrade\":true}");Debug.Log(Result);
    }
    /// <summary>Uses the real Harbor preparation and confirmation controls.</summary>
    private static void Start(JournalHarborFlow harbor)
    {
        harbor.Controller.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        var prep=new SerializedObject(harbor.SlotPreparation);((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
    }
    /// <summary>Reads the real named species definition without changing authoring.</summary>
    private static CardDefinition Load(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Seeds only a runtime fixture's prior haul and checks each mutation.</summary>
    private static void Acquire(FishingRunController owner,CardDefinition card,int position)=>Check(owner.Rig.TryAcquire(owner.Rig.Revision,card,position,out _,out _),"Could not seed "+card?.DisplayName);
    /// <summary>Uses the existing deterministic selector fixture while preserving ordinary controller commands.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{owner,card});
    /// <summary>Waits for actual phase input readiness before accepting the current resolved slots.</summary>
    private static IEnumerator Accept(FishingRunController owner)
    {
        var ready=typeof(FishingRunController).GetProperty("CoreActionInputReady",Private);double deadline=Time.realtimeSinceStartupAsDouble+20;
        while(!(bool)ready.GetValue(owner)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        SlotReelAnimationChecks.SettleForControlFixture(UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(),owner);yield return null;
        Check(owner.TryAcceptSlotMachine(owner.SlotMachine.Revision),"Could not accept Discover.");
        owner.CompleteLandingReveal(); // Entrance covered by LandingCreatureRevealChecks.
    }
    /// <summary>Performs real perfectly timed or deliberately missed strikes, rather than substituting an outcome.</summary>
    private static IEnumerator Land(FishingRunController owner,bool succeed)
    {
        var model=owner.Landing;double deadline=Time.realtimeSinceStartupAsDouble+90;
        while(model.State==LandingChallengeState.Active&&Time.realtimeSinceStartupAsDouble<deadline)
        {
            model.AdvanceTo(Time.realtimeSinceStartupAsDouble);double distance=Math.Abs(model.FishPosition-model.ZonePosition);
            if(model.CanStrike&&(succeed?distance<model.PerfectWidth*.15:distance>(model.GoodWidth+model.FishWidth)*.5+.05)) owner.TryStrikeLanding(model.Occurrence,model.Revision);
            yield return null;
        }
        owner.CompleteLandingFeedback();owner.CompleteLandingOutcome(); // Score feedback and outcome beat have their own checks.
        Check(succeed?owner.AwaitingRigPlacement||owner.RigUpgradeFeedbackVisible:owner.LandingFailureVisible,"Landing did not reach the expected outcome.");
    }
    /// <summary>Checks the EventSystem's actual top raycast handler for every native insertion target.</summary>
    private static void CheckHit(UnityEngine.UI.Button button)
    {
        Canvas.ForceUpdateCanvases();var rect=(RectTransform)button.transform;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
        TacklePhaseTransitionChecks.Settle(); // At-rest layout check; skip the cosmetic phase transition.
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject)==button.gameObject,"Insertion button is blocked.");
    }
    /// <summary>Returns a live text field through its authored binding.</summary>
    private static string Text(SerializedObject data,string name)=>((TMP_Text)data.FindProperty(name).objectReferenceValue).text;
    /// <summary>Captures a frozen native hierarchy without changing the live canvas's camera or clipping state.</summary>
    internal static void Capture(RigBuildingView view,string file,int width=1920,int height=1080,string output=Output)
    {
        var copy=UnityEngine.Object.Instantiate(view.gameObject);
        try
        {
            copy.name="Build capture fixture";copy.GetComponent<RigBuildingView>().enabled=false;
            foreach(var image in copy.GetComponentsInChildren<UnityEngine.UI.MaskableGraphic>()) { image.RecalculateClipping();image.RecalculateMasking();image.SetAllDirty(); }
            Canvas.ForceUpdateCanvases();
            SlotMachineSceneChecks.Capture(copy.GetComponent<Canvas>(),file,width,height,output);
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }
    /// <summary>Retains a readable failure result for asynchronous fixture runs.</summary>
    private static void Check(bool valid,string message)
    {
        if(!valid){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;
    }
}
