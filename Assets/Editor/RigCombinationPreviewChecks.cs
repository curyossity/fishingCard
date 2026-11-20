using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Verifies preview attribution, real tray inspection, cleanup and unchanged placement transactions.</summary>
public static class RigCombinationPreviewChecks
{
    public const string Output = "Docs/Validation/RigCombinationPreview/";
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
    private static int checks;
    public static string Result { get; private set; } = "Not run";

    /// <summary>Uses installed species to verify contiguous boundaries, pure previews, value targets and source-independent abilities.</summary>
    public static int Validate()
    {
        checks = 0;
        var rig = new RigRuntime();
        foreach (string name in new[] { "Sardine", "Mackerel", "Glass Minnow" }) Acquire(rig, Card(name));
        int sardine = rig.Members[0].InstanceId, mackerel = rig.Members[1].InstanceId, minnow = rig.Members[2].InstanceId;
        var links = RigRuntime.InspectAbility(rig.Members, sardine);
        Check(links.SupportingIds.OrderBy(i => i).SequenceEqual(new[] { mackerel, minnow }), "Contiguous support lost a member.");
        long revision = rig.Revision; int haul = rig.HaulValue;
        Check(rig.TryPreview(Card("Cod"), 1, out var split, out _, out _), "Could not preview a school break.");
        Check(RigRuntime.InspectAbility(split, sardine).SupportingIds.Count == 0, "A different tag failed to break the school.");
        Check(RigRuntime.InspectAbility(rig.Members, sardine).SupportingIds.Count == 2, "Preview changed committed relationships.");
        Check(rig.Revision == revision && rig.HaulValue == haul && rig.Members.Length == 3, "Inspection mutated the rig.");
        Check(rig.TryPreview(Card("Cod"), 2, out split, out _, out _), "Could not preview a partial break.");
        Check(RigRuntime.InspectAbility(split, sardine).SupportingIds.SequenceEqual(new[] { mackerel }), "School support crossed the inserted blocker.");
        Check(RigRuntime.InspectAbility(split, int.MaxValue).SupportingIds.Count == 0, "Unknown identity gained links.");
        Check(RigRuntime.InspectAbility(null, sardine).SupportingIds.Count == 0, "Empty snapshot failed.");
        var haven = new RigRuntime();
        foreach (string name in new[] { "Sea Sponge", "Sardine", "Sea Anemone", "Glass Minnow" }) Acquire(haven, Card(name));
        var valueLinks = RigRuntime.InspectAbility(haven.Members, haven.Members[0].InstanceId);
        Check(valueLinks.RecipientIds.SequenceEqual(new[] { haven.Members[1].InstanceId }), "Value recipients crossed the Anchored boundary.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigBuildingSetup.PrefabPath);
        var view = prefab.GetComponent<RigCombinationPreview>(); Check(view != null, "Missing Build combination view.");
        var data = new SerializedObject(view);
        foreach (string field in new[] { "tooltip", "overlay", "glowTemplate" }) Check(data.FindProperty(field).objectReferenceValue != null, "Missing combination binding: " + field);
        var marker = (RigRelationshipGlow)data.FindProperty("glowTemplate").objectReferenceValue;
        Check(!marker.raycastTarget && !marker.gameObject.activeSelf && marker.GetComponent<UnityEngine.UI.Image>() == null, "Glow template intercepts input or retains the old frame.");
        Check(!marker.GetComponentInParent<CanvasGroup>(true).blocksRaycasts, "Relationship layer intercepts input.");
        return checks;
    }

    /// <summary>Starts a disposable expedition with persistence disabled, using real Landing to reach insertion.</summary>
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result = "Running"; checks = 0; Directory.CreateDirectory(Output); Application.runInBackground = true;
        var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        owner.SetDebugSaveResetPending(true); owner.StartCoroutine(Run());
    }
    /// <summary>Exercises actual pooled sources, selected gaps, pointer/native focus, scroll dismissal and phase cleanup.</summary>
    private static IEnumerator Run()
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var owner = harbor.Controller;
        owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke(); yield return null;
        foreach (string name in new[] { "Sardine", "Mackerel", "Glass Minnow" }) Acquire(owner.Rig, Card(name));
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, Card("Cod") });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept", Hidden).Invoke(null, new object[] { owner });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land", Hidden).Invoke(null, new object[] { owner, true }); yield return null;
        var build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>(); var view = build.GetComponent<RigCombinationPreview>();
        var host = build.GetComponent<TackleCardTooltip>(); var data = new SerializedObject(build);
        var rail = (UnityEngine.UI.ScrollRect)data.FindProperty("railScroll").objectReferenceValue;
        var source = rail.content.GetComponentsInChildren<TackleCardTooltipSource>().Single(c => c.Member.Definition.DisplayName == "Sardine");
        int minnow = owner.Rig.Members[2].InstanceId; long revision = owner.Rig.Revision; int haul = owner.Rig.HaulValue;
        var pointer = new PointerEventData(EventSystem.current);
        source.OnPointerEnter(pointer); yield return new WaitForSecondsRealtime(.24f); yield return new WaitForEndOfFrame();
        Check(view.InspectedId == source.Member.InstanceId && view.IsSupporting(minnow), "Hover failed to show the original school.");
        Capture(build, "Connected-1920x1080.png");
        var gaps = rail.content.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b => b.gameObject.activeSelf).ToArray();
        gaps[1].onClick.Invoke(); source.OnPointerEnter(pointer); yield return new WaitForSecondsRealtime(.24f); yield return new WaitForEndOfFrame();
        Check(view.IsDisconnected(minnow) && !view.IsSupporting(minnow), "Insertion did not reveal disconnected support.");
        var context = (TMP_Text)new SerializedObject(host).FindProperty("context").objectReferenceValue;
        Check(context.text.Contains("Disconnected by insertion") && context.text.Contains("Glass Minnow"), "Missing named break explanation.");
        context.ForceMeshUpdate(); Check(context.richText && !context.GetParsedText().Contains("<color="), "Relationship formatting tags are visible.");
        foreach (var size in new[] { new Vector2Int(1920,1080), new Vector2Int(1280,800), new Vector2Int(2560,1080) })
            SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(), "Disconnected-" + size.x + "x" + size.y + ".png", size.x, size.y, Output);
        Check(owner.Rig.Revision == revision && owner.Rig.HaulValue == haul && owner.Rig.Members.Length == 3, "Hover or gap preview mutated acquisition.");
        host.Hide(); yield return new WaitForEndOfFrame(); Check(view.InspectedId == -1, "Dismissal left a stale highlight.");
        source.OnSelect(new BaseEventData(EventSystem.current)); yield return new WaitForEndOfFrame();
        Check(view.InspectedId == source.Member.InstanceId && view.IsDisconnected(minnow), "Native focus lost preview relationships.");
        source.OnScroll(pointer); yield return new WaitForEndOfFrame(); Check(view.InspectedId == -1, "Scrolling retained highlights.");
        source.OnPointerEnter(pointer); yield return new WaitForEndOfFrame(); Check(view.InspectedId > 0, "Same-source reentry failed.");
        Check(!view.GetComponentsInChildren<RigRelationshipGlow>(true).Any(i => i.raycastTarget), "Glow intercepts controls.");
        ((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).onClick.Invoke(); yield return null;
        Check(!build.gameObject.activeInHierarchy && view.InspectedId == -1 && owner.Rig.Members.Length == 4, "Placement did not clean up and continue.");
        Result = "Passed " + checks + " live combination-preview checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}\n");
    }
    /// <summary>Captures native UI without changing production cameras or authoring.</summary>
    private static void Capture(RigBuildingView build, string file) => SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(), file, output: Output);
    /// <summary>Finds current installed content by its authored identity.</summary>
    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    /// <summary>Seeds only this disposable runtime rig through the real transactional owner.</summary>
    private static void Acquire(RigRuntime rig, CardDefinition card) => Check(rig.TryAcquire(rig.Revision, card, rig.Members.Length, out _, out _), "Fixture acquisition failed: " + card.DisplayName);
    /// <summary>Preserves a useful failure result for asynchronous editor checks.</summary>
    private static void Check(bool condition, string message) { if (!condition) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
