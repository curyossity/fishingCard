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

/// <summary>Verifies real hover/focus behavior, preview purity, input pass-through and rendering with installed creature content.</summary>
public static class TackleCardTooltipChecks
{
    public const string Output = "Docs/Validation/CardTooltips/";
    public static string Result { get; private set; } = "Not run";
    private static int checks;
    private const BindingFlags Private = BindingFlags.Static | BindingFlags.NonPublic;

    /// <summary>Checks serialized bindings across every installed phase and card master.</summary>
    public static int ValidateAssets()
    {
        checks = 0;
        foreach (string path in new[] { TackleDiscoverArtSetup.PrefabPath, LandingChallengeSetup.PrefabPath, RigBuildingSetup.PrefabPath })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var host = root.GetComponent<TackleCardTooltip>(); Check(host != null, "Missing phase host: " + path);
            var data = new SerializedObject(host);
            foreach (string name in new[] { "overlay", "panel", "outline", "title", "rarity", "metadata", "abilityName", "rules", "context" })
                Check(data.FindProperty(name).objectReferenceValue != null, "Missing tooltip binding: " + name);
            var overlay = (RectTransform)data.FindProperty("overlay").objectReferenceValue;
            Check(!overlay.GetComponent<CanvasGroup>().blocksRaycasts, "Tooltip blocks native input.");
            Check(!overlay.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g => g.raycastTarget), "Tooltip graphic intercepts input.");
            Check(!overlay.GetComponentsInParent<UnityEngine.UI.RectMask2D>(true).Any(), "Tooltip is inside a clipping mask.");
        }
        foreach (string path in new[] { TackleDiscoverArtSetup.RigPrefabPath, RigBuildingSetup.CabinetCardPath, RigBuildingSetup.RailCardPath })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var source = root.GetComponent<TackleCardTooltipSource>();
            Check(source != null && source.targetGraphic.raycastTarget, "Uninspectable card: " + path);
            Check(source.Above == (path == RigBuildingSetup.RailCardPath), "Wrong placement preference.");
        }
        return checks;
    }

    /// <summary>Starts a fresh runtime fixture with player persistence disabled.</summary>
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result = "Running"; checks = 0;
        var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        owner.SetDebugSaveResetPending(true); Application.runInBackground = true; owner.StartCoroutine(Run());
    }

    /// <summary>Reaches Build through real Landing, then exercises the actual pooled cards and UI event handlers.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var owner = harbor.Controller;
        Check(!owner.RunActive, "Expected a fresh Harbor.");
        owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke(); yield return null;
        var sardine = Card("Sardine");
        foreach (string name in new[] { "Cleaner Shrimp", "Sardine", "Glass Minnow", "Cod" }) Acquire(owner.Rig, Card(name));
        Acquire(owner.Rig, sardine); Acquire(owner.Rig, sardine);
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Private).Invoke(null, new object[] { owner, Card("Mackerel") });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept", Private).Invoke(null, new object[] { owner });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land", Private).Invoke(null, new object[] { owner, true });
        yield return null;
        var view = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>(); var host = view.GetComponent<TackleCardTooltip>();
        var sources = view.GetComponentsInChildren<TackleCardTooltipSource>().Where(s => s.Member != null).ToArray();
        var cabinet = sources.First(s => !s.Above && s.Member.Definition.DisplayName == "Sardine");
        var rail = sources.First(s => s.Above && s.Member.Definition.DisplayName == "Glass Minnow");
        long revision = owner.Rig.Revision; int haul = owner.Rig.HaulValue; long occurrence = owner.BuildOccurrence;
        var pointer = new PointerEventData(EventSystem.current);
        cabinet.OnPointerEnter(pointer); Check(!host.IsVisible, "Hover appeared without its delay.");
        cabinet.OnPointerExit(pointer); host.Hide();
        EventSystem.current.SetSelectedGameObject(cabinet.gameObject);
        Check(host.IsVisible && host.Source == cabinet, "Native focus did not show details.");
        Check(cabinet.Member.Level == 3, "Upgrade fixture lost its current level.");
        var hostData = new SerializedObject(host);
        var rules = (TMP_Text)hostData.FindProperty("rules").objectReferenceValue;
        Check(rules.text == RigAbilityText.Describe(cabinet.Member.PrimaryAbility, 3), "Tooltip did not use the frozen current-level ability.");
        foreach (var size in new[] { new Vector2Int(1280,800), new Vector2Int(1920,1080), new Vector2Int(2560,1080) })
            Capture(view, "LeftRig-" + size.x + "x" + size.y + ".png", size.x, size.y);
        EventSystem.current.SetSelectedGameObject(rail.gameObject);
        Check(host.Source == rail && rail.AfterPlacement, "Rail did not bind the selected placement preview.");
        Check(((TMP_Text)hostData.FindProperty("context").objectReferenceValue).text.StartsWith("AFTER PLACEMENT"), "Preview was not labeled.");
        Capture(view, "InsertionPreview-1920x1080.png", 1920,1080);
        var data = new SerializedObject(view);
        var content = (RectTransform)data.FindProperty("railContent").objectReferenceValue;
        var railScroll = (UnityEngine.UI.ScrollRect)data.FindProperty("railScroll").objectReferenceValue;
        int visibleGaps = 0;
        foreach (var gap in content.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b => b.interactable))
        {
            var point = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)gap.transform).TransformPoint(((RectTransform)gap.transform).rect.center));
            if (!RectTransformUtility.RectangleContainsScreenPoint(railScroll.viewport, point)) continue;
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject) == gap.gameObject, "Tooltip intercepted a visible insertion button.");
            visibleGaps++;
        }
        Check(visibleGaps > 0, "No visible insertion gaps were checked.");
        Check(owner.Rig.Revision == revision && owner.Rig.HaulValue == haul && owner.BuildOccurrence == occurrence, "Inspection mutated acquisition or values.");
        // Rebinding the same pooled view must clear the old species before another delayed hover.
        rail.GetComponent<TackleRigCardView>().Bind(owner.Rig.Members[0]); Check(!host.IsVisible || host.Source.Member == owner.Rig.Members[0], "Recycled card retained stale information.");
        host.Dismiss(cabinet); Check(host.Source == rail, "A stale source dismissed the current tooltip.");
        host.Hide(); Check(!host.IsVisible && host.Source == null, "Dismissal retained pending state.");
        EventSystem.current.SetSelectedGameObject(null);
        rail.OnPointerEnter(pointer); yield return new WaitForSecondsRealtime(.22f);
        Check(host.IsVisible, "Hover did not appear after its delay.");
        rail.OnPointerExit(pointer); yield return new WaitForSecondsRealtime(.10f);
        Check(!host.IsVisible, "Pointer exit did not dismiss.");
        cabinet.OnSelect(new BaseEventData(EventSystem.current));
        cabinet.OnScroll(pointer); Check(!host.IsVisible, "Scrolling retained inspection.");
        cabinet.OnSelect(new BaseEventData(EventSystem.current)); cabinet.gameObject.SetActive(false);
        Check(!host.IsVisible, "Source removal retained inspection."); cabinet.gameObject.SetActive(true);
        cabinet.OnSelect(new BaseEventData(EventSystem.current));
        view.gameObject.SetActive(false); Check(!host.IsVisible, "Phase exit retained inspection.");
        Result = "Passed " + checks + " live tooltip checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}"); Debug.Log(Result);
    }

    /// <summary>Captures the actual canvas with its existing camera-safe UI renderer.</summary>
    private static void Capture(RigBuildingView view, string file, int width, int height)
    {
        Canvas.ForceUpdateCanvases();
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), file, width, height, Output);
        var host = view.GetComponent<TackleCardTooltip>(); var data = new SerializedObject(host);
        var panel = (RectTransform)data.FindProperty("panel").objectReferenceValue;
        Check(panel.rect.width == 460 && panel.rect.height > 100, "Tooltip geometry collapsed.");
        foreach (var text in panel.GetComponentsInChildren<TMP_Text>())
        {
            text.ForceMeshUpdate();
            Check(!text.isTextOverflowing && text.textBounds.size.x <= 414, "Tooltip text overflow: " + text.name);
        }
    }
    /// <summary>Loads installed species without editing their shared authored data.</summary>
    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    /// <summary>Seeds only the disposable runtime haul through the real rig owner.</summary>
    private static void Acquire(RigRuntime rig, CardDefinition card) { Check(rig.TryAcquire(rig.Revision, card, rig.Members.Length, out _, out _), "Could not seed tooltip fixture."); }
    /// <summary>Reports a precise coroutine failure for tool-driven verification.</summary>
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
