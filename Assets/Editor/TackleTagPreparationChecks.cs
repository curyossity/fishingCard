using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Verifies preparation artwork, draft isolation, native navigation and the real Harbor-to-Discover handoff.</summary>
public static class TackleTagPreparationChecks
{
    public const string Output = "Docs/Validation/TagPreparation/";
    private static int checks;
    public static string Result { get; private set; } = "Not run";

    /// <summary>Validates the independent canvas, five canonical symbols, reusable artwork and saved scene ownership.</summary>
    public static int ValidateAssets()
    {
        checks = 0;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotMachineSetup.PreparationPath);
        Check(prefab != null && prefab.GetComponent<Canvas>().sortingOrder == 500, "Preparation canvas is missing or incorrectly layered.");
        var scaler = prefab.GetComponent<UnityEngine.UI.CanvasScaler>();
        Check(scaler.referenceResolution == new Vector2(1920,1080) && scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand, "Preparation inherits old Harbor scaling.");
        Check(prefab.GetComponent<UnityEngine.UI.GraphicRaycaster>() != null && prefab.GetComponent<UnityEngine.UI.Image>().raycastTarget, "Preparation cannot block pointer input behind it.");
        var data = new SerializedObject(prefab.GetComponent<SlotTagPreparationView>());
        foreach (string field in new[] { "confirm", "cancel", "status", "artwork" }) Check(data.FindProperty(field).objectReferenceValue != null, "Missing preparation field: " + field);
        Check(data.FindProperty("tagButtons").arraySize == 5 && data.FindProperty("tagLabels").arraySize == 5, "Wrong number of tag controls.");
        var art = new SerializedObject(prefab.GetComponent<TackleTagPreparationPresentation>());
        Check(art.FindProperty("plates").arraySize == 5 && art.FindProperty("readyLamps").arraySize == 4, "Wrong selection plate/lamp count.");
        for (int i=0;i<5;i++)
        {
            var plate=art.FindProperty("plates").GetArrayElementAtIndex(i);
            Check(plate.FindPropertyRelative("tag").stringValue==CreatureTagVocabulary.All[i],"Wrong tag identity.");
            foreach(string field in new[]{"face","symbol","lamp","state","feedback"}) Check(plate.FindPropertyRelative(field).objectReferenceValue!=null,"Missing tag plate binding: "+field);
            var button=(UnityEngine.UI.Button)data.FindProperty("tagButtons").GetArrayElementAtIndex(i).objectReferenceValue;
            Check(button.name==CreatureTagVocabulary.All[i]&&button.navigation.mode!=UnityEngine.UI.Navigation.Mode.None,"Tag is not natively navigable.");
        }
        foreach(var text in prefab.GetComponentsInChildren<TMP_Text>(true))
            Check(AssetDatabase.GetAssetPath(text.font).StartsWith(TackleDiscoverArtSetup.Root+"Fonts/",StringComparison.Ordinal),"Old Harbor/default font in preparation.");
        foreach(var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if(image.sprite!=null) Check(AssetDatabase.GetAssetPath(image.sprite).StartsWith(TackleDiscoverArtSetup.Root+"Art/",StringComparison.Ordinal),"Preparation mixes theme imagery.");
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(FindObjectsInactive.Include);
        Check(harbor!=null&&harbor.SlotPreparation!=null,"Saved Harbor has no preparation binding.");
        Check(harbor.SlotPreparation.transform.parent==null&&harbor.SlotPreparation.GetComponent<Canvas>().isRootCanvas,"Preparation is nested under the old Harbor canvas.");
        Check(!harbor.SlotPreparation.gameObject.activeSelf,"Preparation starts visible in the Harbor.");
        Check(UnityEngine.Object.FindObjectsByType<SlotTagPreparationView>(FindObjectsInactive.Include).Length==1,"Duplicate preparation screens remain.");
        Check(new SerializedObject(harbor.SlotPreparation).FindProperty("blockedHarborContent").objectReferenceValue!=null,"Harbor input blocker is unbound.");
        return checks;
    }

    /// <summary>Uses actual native button commands to test every excluded tag and starts one expedition without writing progression.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";
        UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller.StartCoroutine(Run());
    }

    /// <summary>Allows the newly opened independent canvas to complete its native first render before pointer checks.</summary>
    private static IEnumerator Run()
    {
        checks=0;Directory.CreateDirectory(Output);
        var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.SetDebugSaveResetPending(true); Check(!owner.RunActive&&harbor.HubVisible,"Expected a fresh Harbor.");
        var preparation=harbor.SlotPreparation;var data=new SerializedObject(preparation);
        var buttons=Enumerable.Range(0,5).Select(i=>(UnityEngine.UI.Button)data.FindProperty("tagButtons").GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        var confirm=(UnityEngine.UI.Button)data.FindProperty("confirm").objectReferenceValue;var back=(UnityEngine.UI.Button)data.FindProperty("cancel").objectReferenceValue;
        var blocker=(CanvasGroup)data.FindProperty("blockedHarborContent").objectReferenceValue;
        var previous=owner.PreparedSlotTags.ToArray();
        Check(!harbor.TryStartNewRun()&&preparation.gameObject.activeInHierarchy,"Harbor Set Sail did not open preparation.");
        yield return null;
        Check(!blocker.interactable&&!blocker.blocksRaycasts,"Old Harbor remains interactive during preparation.");
        Check(EventSystem.current.currentSelectedGameObject!=null&&EventSystem.current.currentSelectedGameObject.transform.IsChildOf(preparation.transform),"Native focus did not enter preparation.");
        foreach(var button in buttons) if(preparation.Draft.Contains(button.name))button.onClick.Invoke();
        Check(preparation.Draft.Count==0&&!confirm.interactable,"An empty selection can start an expedition.");
        confirm.onClick.Invoke();Check(!owner.RunActive&&owner.PreparedSlotTags.SequenceEqual(previous),"Invalid confirmation changed preparation or started a run.");
        // An unfocused Editor may not render the Game view, so submit a real native render before testing graphic depths.
        Capture(preparation,"Empty-1920x1080.png",1920,1080);CheckRaycast(buttons[0]);
        foreach(var text in preparation.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Preparation text overflows: "+text.name);}
        for(int excluded=0;excluded<5;excluded++)
        {
            foreach(var button in buttons) if(preparation.Draft.Contains(button.name))button.onClick.Invoke();
            for(int i=0;i<5;i++)if(i!=excluded)buttons[i].onClick.Invoke();
            Check(preparation.Draft.Count==4&&confirm.interactable&&!buttons[excluded].interactable,"Invalid four-of-five state.");
            var expected=buttons.Where((b,i)=>i!=excluded).Select(b=>b.name).ToArray();
            Check(preparation.Draft.OrderBy(t=>t).SequenceEqual(expected.OrderBy(t=>t)),"A canonical tag was omitted or duplicated.");
            buttons[excluded].onClick.Invoke();Check(preparation.Draft.Count==4&&!preparation.Draft.Contains(buttons[excluded].name),"A fifth tag was added through a stale/direct command.");
            Check(owner.PreparedSlotTags.SequenceEqual(previous),"Draft editing committed an expedition loadout.");
            CheckRaycast(confirm);
        }
        var savedDraft=preparation.Draft.ToArray();harbor.TryStartNewRun();Check(preparation.Draft.SequenceEqual(savedDraft),"Repeated Harbor input discarded the open draft.");
        ExecuteEvents.ExecuteHierarchy(buttons[0].gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.cancelHandler);
        Check(!preparation.gameObject.activeInHierarchy&&!owner.RunActive&&blocker.interactable&&blocker.blocksRaycasts,"Native cancel did not restore Harbor input.");
        Check(owner.PreparedSlotTags.SequenceEqual(previous),"Cancel committed the draft.");
        owner.TryPrepareSlotTags(new[]{"Schooling","Predator","Anchored","Armored"});harbor.TryStartNewRun();
        Check(preparation.Draft.Count==4&&preparation.Draft.Contains("Schooling"),"Prepared symbols were not restored on reopening.");
        buttons[0].onClick.Invoke();Check(!confirm.interactable&&preparation.Draft.Count==3&&buttons[4].interactable,"Removing a tag did not enable its replacement.");
        Capture(preparation,"Choose-One-More.png",1920,1080);buttons[4].onClick.Invoke();
        var chosen=preparation.Draft.ToArray();Check(confirm.interactable&&!chosen.Contains("Schooling")&&chosen.Contains("Elusive"),"Tag swap failed.");
        foreach(var size in new[]{new Vector2Int(1280,800),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})Capture(preparation,"Selected-"+size.x+"x"+size.y+".png",size.x,size.y);
        buttons[2].Select();Check(buttons[2].FindSelectableOnRight()!=null&&buttons[2].FindSelectableOnDown()!=null,"Directional navigation has no route through selection and actions.");
        back.onClick.Invoke();Check(!owner.RunActive&&blocker.interactable,"Back did not cancel without starting.");
        preparation.Open(owner,()=>{});confirm.onClick.Invoke();
        var failure=(TMP_Text)new SerializedObject(preparation.GetComponent<TackleTagPreparationPresentation>()).FindProperty("failure").objectReferenceValue;
        Check(failure.gameObject.activeSelf&&!owner.RunActive&&confirm.interactable,"A failed start hid the draft or did not expose an error.");
        failure.ForceMeshUpdate();Check(!failure.isTextOverflowing,"Start error is clipped.");
        Capture(preparation,"Start-Failure.png",1920,1080);back.onClick.Invoke();
        harbor.TryStartNewRun();buttons[0].onClick.Invoke();buttons[4].onClick.Invoke();
        Check(preparation.Draft.OrderBy(t=>t).SequenceEqual(chosen.OrderBy(t=>t)),"Reopened preparation cannot recreate the chosen four tags.");
        confirm.onClick.Invoke();
        Check(owner.RunActive&&!harbor.HubVisible&&!preparation.gameObject.activeInHierarchy,"Confirmation did not enter the expedition.");
        Check(owner.ExpeditionSlotTags.OrderBy(t=>t).SequenceEqual(chosen.OrderBy(t=>t)),"Discover did not freeze the selected symbols.");
        Check(owner.SlotMachine.Active&&owner.CatchChain.Length==0,"Starting preparation acquired a creature or bypassed Discover.");
        int depth=owner.CurrentDepth;long occurrence=owner.SlotMachine.Occurrence;
        confirm.onClick.Invoke();Check(owner.RunActive&&owner.CurrentDepth==depth&&owner.SlotMachine.Occurrence==occurrence,"Stale confirmation restarted the expedition.");
        Check(!owner.TryPrepareSlotTags(new[]{"Schooling","Predator","Anchored","Armored"}),"Active expedition symbols can change.");
        Result="Passed "+checks+" live tag-preparation checks";
        File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");Debug.Log(Result);
    }

    /// <summary>Checks the actual EventSystem raycast destination, including the independent canvas's sort order.</summary>
    private static void CheckRaycast(UnityEngine.UI.Button button)
    {
        Canvas.ForceUpdateCanvases();var rect=(RectTransform)button.transform;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject)==button.gameObject,"Preparation button is blocked: "+button.name+"; hits: "+string.Join(", ",hits.Take(3).Select(h=>h.gameObject.name+" depth "+h.depth)));
    }
    /// <summary>Captures the native UI at representative 16:9, 16:10 and ultrawide resolutions.</summary>
    private static void Capture(SlotTagPreparationView view,string name,int width,int height)
    {Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(),name,width,height,Output);}
    /// <summary>Fails immediately with an actionable UI or state invariant.</summary>
    private static void Check(bool valid,string error){if(!valid){Result="FAILED: "+error;throw new InvalidOperationException(error);}checks++;}
}
