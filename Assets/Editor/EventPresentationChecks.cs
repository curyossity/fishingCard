using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

/// <summary>Checks runtime input availability and the actual event view/collector contracts.</summary>
public static class EventPresentationChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs runtime gates and real Unity view/serialized-authoring checks.</summary>
    [MenuItem("Fishing Cards/Validation/Event Presentation and Authoring")]
    public static void Run()
    {
        int gates = ValidateRules();
        int editor = ValidateEditorContract();
        Debug.Log("Event presentation passed: " + gates + " runtime and " + editor + " Unity Editor assertions.");
    }
#endif
    /// <summary>Tests controller-owned availability across open, cancelled, reveal, suspended, and failed states.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            CardDefinition source = Card(CardType.Hazard);
            FishingRunController controller = Controller(source);
            CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
            EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
            chain.Add(Card(CardType.Creature), resolver);
            controller.SelectCatchForRelease(0);
            Require(controller.CanDescend && controller.CanSurface && !controller.CanReleaseSelectedCatch,
                "Open event did not expose core actions.");
            CardDefinition technique = Card(CardType.Technique);
            CardEffectDefinition delayed = new CardEffectDefinition();
            Set(delayed, "effectType", CardEffectType.ModifyDescendDistance);
            Set(delayed, "target", CardEffectTarget.NextDescend);
            Set(delayed, "amount", 1); Set(technique, "effects", new[] { delayed });
            Set(Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"), "hand", new[] { technique });
            Require(controller.CanUseTechniqueCard(0, out _), "Legal event Technique did not enable.");
            EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
            Require(encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id), "Cancellation fixture failed.");
            Require(controller.CurrentEncounter == null && controller.CanDescend && controller.CanSurface
                && !controller.CanReleaseSelectedCatch && controller.CanUseTechniqueCard(0, out _),
                "Discarded cancelled event disabled its surrounding input window.");
            string[] locks = { "techniqueRefillPending", "descendCompletionPending", "biomeEntryRevealPending",
                "eventActionConfirmationInProgress", "eventActionAdvanceInProgress" };
            foreach (string field in locks)
            {
                Set(controller, field, true);
                Require(!controller.CanDescend && !controller.CanSurface && !controller.CanReleaseSelectedCatch
                    && !controller.CanUseTechniqueCard(0, out _), field + " left a control enabled.");
                controller.SelectCatchForRelease(0);
                Require(Get<int>(controller, "selectedCatchIndex") == 0, "Locked input changed catch selection.");
                Set(controller, field, false);
            }
            Set(controller, "confirmedEventAction", ConfirmedEventAction.Surface);
            Set(controller, "eventActionFailure", "Fixture dispatch failure");
            Require(!controller.CanDescend && !controller.CanSurface && !controller.CanReleaseSelectedCatch
                && !controller.CanUseTechniqueCard(0, out _), "Faulted confirmation reenabled input.");
            Set(controller, "confirmedEventAction", ConfirmedEventAction.None); Set(controller, "eventActionFailure", string.Empty);
            long reaction = 0;
            encounters.EncounterRevealEffects += card => encounters.TryDeferEventRevealReaction(encounters.CurrentEventOccurrence.Id, out reaction);
            encounters.SetCurrentEncounter(source);
            Require(!controller.CanDescend && !controller.CanSurface && !controller.CanReleaseSelectedCatch,
                "Deferred reveal enabled core controls.");
            encounters.TryCompleteEventRevealReaction(encounters.CurrentEventOccurrence.Id, reaction);
            Require(controller.CanDescend && controller.CanSurface && !controller.CanReleaseSelectedCatch,
                "Completed reveal did not restore controls.");
            CardDefinition locked = Card(CardType.Creature); CardEffectDefinition grip = new CardEffectDefinition();
            Set(grip, "effectType", CardEffectType.PreventRelease); Set(grip, "target", CardEffectTarget.Self);
            Set(grip, "trigger", CardEffectTrigger.WhileAttached); Set(locked, "effects", new[] { grip });
            chain.Add(locked, resolver); controller.SelectCatchForRelease(1);
            Require(!controller.CanReleaseSelectedCatch && controller.CanSurface && controller.CanDescend,
                "Release restriction disabled unrelated controls or enabled Release.");
            Require(!controller.CanReleaseCatch(-1) && !controller.CanReleaseCatch(99), "Invalid chain selection enabled Release.");
            Set(controller, "runActive", false);
            Require(!controller.CanDescend && !controller.CanSurface && !controller.CanReleaseCatch(0),
                "Ended run left controls enabled.");
            return assertions;
        }
        finally { DisposeFixtures(); }
    }
#if UNITY_EDITOR
    /// <summary>Instantiates production UI and checks serialized collector edits with Undo/Redo and disk reload.</summary>
    public static int ValidateEditorContract()
    {
        assertions = 0;
        try { ValidateViews(); ValidateAuthoring(); return assertions; }
        finally { DisposeFixtures(); }
    }

    /// <summary>Verifies event faces, role suppression, and runtime-supplied action button states on the production prefab.</summary>
    private static void ValidateViews()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab");
        Require(prefab != null, "Production run prefab is missing.");
        GameObject obj = UnityEngine.Object.Instantiate(prefab); created.Add(obj);
        FishingRunView view = obj.GetComponent<FishingRunView>();
        CardDefinition card = Card(CardType.Hazard); Set(card, "rulesText", "Event description.");
        view.Refresh(true, null, 0, card, 99, 99, false, 10, 1, 4, 0, true, false, true,
            () => true, () => true, () => true);
        Require(Get<Button>(view, "descendButton").interactable && !Get<Button>(view, "releaseButton").interactable
            && Get<Button>(view, "surfaceButton").interactable, "View inferred Release from selected index.");
        CreatureCardView face = Get<CreatureCardView>(view, "encounterCardView");
        foreach (CardType type in new[] { CardType.Hazard, CardType.Environment, CardType.Opportunity, CardType.Encounter })
        {
            Set(card, "cardType", type); face.SetCard(card, 99, 99, false);
            Require(!Get<TMP_Text>(face, "weightText").transform.parent.gameObject.activeSelf
                && !Get<TMP_Text>(face, "valueText").transform.parent.gameObject.activeSelf,
                "Event displayed catch stats: " + type);
            Require(!Get<RectTransform>(face, "baitRulesRegion").gameObject.activeSelf
                && !Get<Button>(face, "catchEffectButton").enabled && !Get<Button>(face, "baitEffectButton").enabled,
                "Event exposed Catch/Bait selection: " + type);
            Require(!Get<RectTransform>(face, "rarityAnchorRoot").gameObject.activeSelf
                && (Get<RectTransform>(face, "tagIconRoot") == null
                    || !Get<RectTransform>(face, "tagIconRoot").gameObject.activeSelf),
                "Event displayed rarity hooks or tag rail: " + type);
            int notifications = 0; Action<CreatureCardView, EncounterEffectChoice> callback = (sender, choice) => notifications++;
            face.EffectChoiceChanged += callback; face.SelectEffectChoice(EncounterEffectChoice.Bait);
            face.EffectChoiceChanged -= callback;
            Require(notifications == 0 && Get<TMP_Text>(face, "effectText").text.Contains("Event description"),
                "Event role selection emitted gameplay input or replaced its description.");
        }
        view.Refresh(true, null, 0, null, 0, 0, false, 10, 1, 4, 0, true, true, true,
            () => true, () => true, () => true);
        Require(!face.gameObject.activeSelf && Get<Button>(view, "descendButton").interactable
            && !Get<Button>(view, "releaseButton").gameObject.activeSelf && Get<Button>(view, "surfaceButton").interactable,
            "Empty cancelled card region disabled supplied action availability.");
        view.Refresh(true, null, 0, null, 0, 0, false, 10, 1, 4, 0, false, false, false,
            () => true, () => true, () => true);
        Require(!Get<Button>(view, "descendButton").interactable && !Get<Button>(view, "releaseButton").interactable
            && !Get<Button>(view, "surfaceButton").interactable, "Confirmed action left a run button enabled.");
    }

    /// <summary>Captures authored event faces at supported resolutions in an isolated preview scene.</summary>
    public static int CaptureReadability(string output)
    {
        Directory.CreateDirectory(output);
        Scene scene = EditorSceneManager.NewPreviewScene();
        RenderTexture previous = RenderTexture.active;
        int captures = 0;
        try
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab");
            GameObject obj = UnityEngine.Object.Instantiate(prefab); SceneManager.MoveGameObjectToScene(obj, scene);
            GameObject cameraObject = new GameObject("Event validation camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.scene = scene; camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.nearClipPlane = -10f; camera.farClipPlane = 10f;
            Canvas canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1f;
            FishingRunView view = obj.GetComponent<FishingRunView>();
            foreach (Vector2Int size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1366, 768), new Vector2Int(1280, 800) })
            {
                RenderTexture target = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
                Texture2D capture = null;
                try
                {
                    camera.targetTexture = target;
                    foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }))
                    {
                        CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                        if (!EncounterRuntime.IsEventCard(card)) continue;
                        view.Refresh(true, null, 0, card, 0, 0, false, 10, 0, 4, -1, true, false, true,
                            () => true, () => false, () => true);
                        Canvas.ForceUpdateCanvases();
                        CreatureCardView face = Get<CreatureCardView>(view, "encounterCardView");
                        foreach (string field in new[] { "cardNameText", "cardTypeText", "effectText" })
                        {
                            TMP_Text text = Get<TMP_Text>(face, field); text.ForceMeshUpdate();
                            if (text.isTextOverflowing) throw new InvalidOperationException(card.DisplayName + " overflows " + field + " at " + size);
                        }
                        camera.Render(); RenderTexture.active = target;
                        capture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                        capture.ReadPixels(new Rect(0f, 0f, size.x, size.y), 0, 0); capture.Apply();
                        File.WriteAllBytes(Path.Combine(output, "event-" + guid + "-" + size.x + "x" + size.y + ".png"), capture.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(capture); capture = null; captures++;
                    }
                }
                finally
                {
                    RenderTexture.active = previous; camera.targetTexture = null;
                    if (capture != null) UnityEngine.Object.DestroyImmediate(capture);
                    target.Release(); UnityEngine.Object.DestroyImmediate(target);
                }
            }
            if (captures == 0) throw new InvalidOperationException("No authored events were available for readability captures.");
            return captures;
        }
        finally { RenderTexture.active = previous; EditorSceneManager.ClosePreviewScene(scene); }
    }

    /// <summary>Exercises the collector's serialization and membership boundary without editing production assets.</summary>
    private static void ValidateAuthoring()
    {
        string folder = "Assets/Editor/EventValidation_" + Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets/Editor", folder.Substring("Assets/Editor/".Length));
        try
        {
            CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>();
            string path = folder + "/Event.asset"; AssetDatabase.CreateAsset(card, path);
            SerializedObject data = new SerializedObject(card);
            data.FindProperty("cardType").intValue = (int)CardType.Environment;
            data.FindProperty("displayName").stringValue = "Round trip event";
            data.FindProperty("rulesText").stringValue = "Approved fixture description";
            data.FindProperty("weight").intValue = 17; data.FindProperty("value").intValue = 42;
            SerializedProperty effects = data.FindProperty("effects"); effects.arraySize = 1;
            SerializedProperty effect = effects.GetArrayElementAtIndex(0);
            effect.FindPropertyRelative("effectType").intValue = (int)CardEffectType.ModifyCatchValue;
            effect.FindPropertyRelative("amount").intValue = 3;
            effect.FindPropertyRelative("hasEventLifetime").boolValue = true;
            effect.FindPropertyRelative("eventLifetime").FindPropertyRelative("boundaryId").stringValue = "FixtureBoundary";
            effect.FindPropertyRelative("eventLifetime").FindPropertyRelative("boundaryCount").intValue = 2;
            data.ApplyModifiedPropertiesWithoutUndo();
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            data.Update(); data.FindProperty("rulesText").stringValue = "Edited fixture description";
            Require(data.ApplyModifiedProperties() && card.RulesText == "Edited fixture description", "Serialized event edit failed.");
            Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); Undo.PerformUndo();
            Require(card.RulesText == "Approved fixture description", "Undo did not restore event description.");
            Undo.PerformRedo(); Require(card.RulesText == "Edited fixture description", "Redo did not restore event edit.");
            EditorUtility.SetDirty(card); AssetDatabase.SaveAssets(); Undo.ClearUndo(card); Resources.UnloadAsset(card);
            CardDefinition loaded = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            Require(loaded != null && loaded.RulesText == "Edited fixture description" && loaded.Weight == 17 && loaded.Value == 42,
                "Save/reload lost event edits or altered preserved shared-schema data.");
            Require(loaded.Effects.Length == 1 && loaded.Effects[0].Amount == 3
                && loaded.Effects[0].EventLifetime?.BoundaryId == "FixtureBoundary"
                && loaded.Effects[0].EventLifetime.BoundaryCount == 2, "Save/reload lost event effect/lifetime data.");
            EventCatalogWindow collector = ScriptableObject.CreateInstance<EventCatalogWindow>(); created.Add(collector);
            MethodInfo scope = typeof(EventCatalogWindow).GetMethod("IsInScope", BindingFlags.Instance | BindingFlags.NonPublic);
            Require((bool)scope.Invoke(collector, new object[] { loaded, 0 }), "Reloaded event left collector scope.");
            Require(!(bool)scope.Invoke(collector, new object[] { Card(CardType.Creature), 0 }), "Creature appeared in event scope.");
            BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
            Set(tier, "encounterPool", new[] { loaded, loaded }); Set(biome, "depthTiers", new[] { tier });
            string biomePath = folder + "/Biome.asset"; AssetDatabase.CreateAsset(biome, biomePath);
            SerializedObject biomeData = new SerializedObject(biome);
            SerializedProperty pool = biomeData.FindProperty("depthTiers").GetArrayElementAtIndex(0).FindPropertyRelative("encounterPool");
            MethodInfo membership = typeof(CardCatalogWindow).GetMethod("SetReferenceMembership", BindingFlags.Static | BindingFlags.NonPublic);
            membership.Invoke(null, new object[] { pool, loaded, false }); biomeData.ApplyModifiedProperties();
            Require(biome.GetEncounterPool(0).Length == 0, "Collector membership removal left duplicate references.");
            biomeData.Update(); pool = biomeData.FindProperty("depthTiers").GetArrayElementAtIndex(0).FindPropertyRelative("encounterPool");
            membership.Invoke(null, new object[] { pool, loaded, true }); biomeData.ApplyModifiedProperties();
            EditorUtility.SetDirty(biome); AssetDatabase.SaveAssets(); Undo.ClearUndo(biome); Resources.UnloadAsset(biome);
            BiomeDefinition loadedBiome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(biomePath);
            Require(loadedBiome.GetEncounterPool(0).Length == 1 && loadedBiome.GetEncounterPool(0)[0] == loaded,
                "Collector membership did not survive save/reload.");
            Undo.ClearUndo(loadedBiome); Undo.ClearUndo(loaded);
        }
        finally { AssetDatabase.DeleteAsset(folder); }
    }
#endif
    /// <summary>Creates a controller with an event-only deterministic pool.</summary>
    private static FishingRunController Controller(CardDefinition card)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { card });
        Set(biome, "depthTiers", new[] { tier }); GameObject obj = new GameObject("Event input checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>(); Set(controller, "startRunOnAwake", false);
        Set(controller, "startingBiome", biome); Set(controller, "startingLineCapacity", 100); controller.StartRun(); return controller;
    }
    /// <summary>Creates authored fixture data without modifying project definitions.</summary>
    private static CardDefinition Card(CardType type)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "cardType", type); Set(card, "weight", 1); Set(card, "value", 8); return card;
    }
    /// <summary>Sets private serialized fixture fields.</summary>
    private static void Set(object owner, string field, object value) { owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads actual runtime or view state rather than duplicating its logic.</summary>
    private static T Get<T>(object owner, string field) { return (T)owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Disposes transient fixtures even when validation fails.</summary>
    private static void DisposeFixtures() { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    /// <summary>Counts an invariant and reports the first mismatch.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
