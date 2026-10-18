using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Validates saved scene wiring and exercises the real controller through journal views in Play Mode.</summary>
public static class JournalComparisonChecks
{
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/";
    private static int assertions;
    private static FishingRunController controller;
    private static FishingRunView view;
    private static JournalGameplayHud hud;
    private static JournalChoiceModal choices;
    private static JournalResultModal results;
    private static JournalModalHost runner;
    private static bool oldBackground;
    private static readonly List<UnityEngine.Object> fixtures = new List<UnityEngine.Object>();
    private static byte[] originalScene, originalBuild, comparisonScene;
    public static string Status { get; private set; } = "Not started";
    [Serializable] private sealed class Report { public bool passed, sceneFilesUnchanged; public int assertions; public string scope; }

    /// <summary>Checks one set of live bindings, shared starter data and isolated view dependencies on the saved scene.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Comparison Scene")]
    public static void ValidateScene()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Inspect saved scene outside Play Mode.");
        byte[] sourceBytes = File.ReadAllBytes("Assets/Scenes/SampleScene.unity"), buildBytes = File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"), candidateBytes = File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath);
        assertions = 0; Scene scene = EditorSceneManager.OpenPreviewScene(JournalComparisonSceneBuilder.ScenePath);
        Scene source = EditorSceneManager.OpenPreviewScene("Assets/Scenes/SampleScene.unity");
        try
        {
            var roots = scene.GetRootGameObjects(); var c = roots.SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
            var original = source.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
            Require(roots.SelectMany(g => g.GetComponentsInChildren<EventSystem>(true)).Count() == 1, "Scene needs one EventSystem.");
            foreach (string field in new[] { "fishingRunView", "catchChainView", "techniqueHandView", "runResultView" }) Require(Get<UnityEngine.Object>(c, field) != null, "Missing controller binding " + field);
            var data = new SerializedObject(c); var old = new SerializedObject(original);
            Require(Get<BiomeDefinition>(c, "startingBiome") == Get<BiomeDefinition>(original, "startingBiome"), "Biome changed.");
            Require(Get<CardDefinition[]>(c, "startingTechniqueDeck").SequenceEqual(Get<CardDefinition[]>(original, "startingTechniqueDeck")), "Starter deck changed.");
            Require(Get<CardDefinition[]>(c, "startingTechniqueDeck").Length == 12 && Get<CardDefinition[]>(c, "startingTechniqueDeck").Distinct().Count() == 8, "Incorrect active starter deck.");
            foreach (string field in new[] { "startingLineCapacity", "startingDepth", "startingHandSize", "depthStepPerDescend" }) Require(data.FindProperty(field).intValue == old.FindProperty(field).intValue, "Run configuration changed: " + field);
            Require(Get<CardView>(c, "currentEncounterView") == null && Get<CardView[]>(c, "techniqueHandViews").Length == 0, "Retired views still active.");
            var ui = roots.Single(g => g.GetComponent<FishingRunView>() != null);
            Require(ui.GetComponentsInChildren<CatchChainView>(true).Length == 1 && ui.GetComponentsInChildren<TechniqueHandView>(true).Length == 1 && ui.GetComponentsInChildren<RunResultView>(true).Length == 1, "Duplicate view consumers.");
            Require(ui.GetComponentInChildren<JournalChoiceModal>(true) != null && ui.GetComponentInChildren<JournalResultModal>(true) != null && ui.GetComponentInChildren<JournalSettingsModal>(true) != null, "Missing theme modal.");
            Require(roots.SelectMany(g => g.GetComponentsInChildren<Camera>(true)).Any(ca => ca.CompareTag("MainCamera")), "Missing main camera.");
            Require(!AssetDatabase.GetDependencies(JournalComparisonSceneBuilder.ScenePath, true).Any(p => p.StartsWith("Assets/Prefabs/UI/")), "Old UI prefab in candidate scene.");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); EditorSceneManager.ClosePreviewScene(source); }
        bool unchanged = sourceBytes.SequenceEqual(File.ReadAllBytes("Assets/Scenes/SampleScene.unity")) && buildBytes.SequenceEqual(File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset")) && candidateBytes.SequenceEqual(File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath));
        Require(unchanged, "Validation modified scene or build bytes.");
        File.WriteAllText(Output + "UI07SceneValidation.json", JsonUtility.ToJson(new Report { passed = true, sceneFilesUnchanged = unchanged, assertions = assertions, scope = "Saved comparison scene; original setup and isolated view bindings" }, true));
    }

    /// <summary>Starts background-ticking integration checks in the actual saved comparison scene.</summary>
    public static void StartPlayModeSmoke()
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().path != JournalComparisonSceneBuilder.ScenePath || runner != null) throw new InvalidOperationException("Open the comparison scene and enter Play Mode first.");
        assertions = 0; oldBackground = Application.runInBackground; Application.runInBackground = true;
        originalScene = File.ReadAllBytes("Assets/Scenes/SampleScene.unity"); originalBuild = File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"); comparisonScene = File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath);
        var obj = new GameObject("Journal Integration Validation Runner"); runner = obj.AddComponent<JournalModalHost>(); UnityEngine.Object.DontDestroyOnLoad(obj);
        Status = "Running"; runner.StartCoroutine(Guarded());
    }
    /// <summary>Surfaces coroutine failures and always restores test-owned resources.</summary>
    private static IEnumerator Guarded()
    {
        var sequence = Smoke(); Exception failure = null;
        while (true)
        {
            bool more;
            try { more = sequence.MoveNext(); } catch (Exception error) { failure = error; break; }
            if (!more) break; yield return sequence.Current;
        }
        bool unchanged = originalScene.SequenceEqual(File.ReadAllBytes("Assets/Scenes/SampleScene.unity")) && originalBuild.SequenceEqual(File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset")) && comparisonScene.SequenceEqual(File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath));
        if (!unchanged && failure == null) failure = new InvalidOperationException("Saved scene or build data changed.");
        File.WriteAllText(Output + "UI07PlayModeValidation.json", JsonUtility.ToJson(new Report { passed = failure == null, assertions = assertions, sceneFilesUnchanged = unchanged,
            scope = failure == null ? "Actual scene/controller, targeted runtime fixtures, scene reload and real short run through Surface/results/restart" : failure.ToString() }, true));
        foreach (var fixture in fixtures) if (fixture != null) UnityEngine.Object.Destroy(fixture); fixtures.Clear();
        Application.runInBackground = oldBackground; var owned = runner; runner = null; UnityEngine.Object.Destroy(owned.gameObject);
        Status = failure == null ? "Passed: " + assertions + " assertions" : "Failed: " + failure;
        if (failure != null) Debug.LogException(failure); else Debug.Log("Journal comparison integration passed: " + assertions);
    }

    /// <summary>Exercises normal UI commands and narrow runtime setups without writing card or biome assets.</summary>
    private static IEnumerator Smoke()
    {
        Bind(); yield return null;
        Require(controller.RunActive && controller.TechniqueHand.Length == 4 && controller.TechniqueDrawPile.Length == 8, "Saved scene did not start the real deck.");
        Require(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Duplicate EventSystems.");
        Status = "Settings and input ownership";
        Submit(hud.SettingsButton); var settings = view.GetComponentInChildren<JournalSettingsModal>(true);
        Require(settings.IsVisible && !hud.DescendButton.IsInteractable(), "Settings did not acquire input.");
        long handVersion = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime").HandVersion;
        var hand = view.GetComponentInChildren<JournalTechniqueHandPresentation>();
        Submit(hand.Cards[0].GetComponent<UnityEngine.UI.Button>());
        Require(Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime").HandVersion == handVersion, "Settings allowed keyboard play through its modal.");
        Submit(settings.CloseButton); Require(!settings.IsVisible && hud.DescendButton.IsInteractable(), "Settings did not restore input.");

        Fixture(0, "Small Octopus", new[] { "Sardine", "Clinging Crab", "Glass Minnow" }, new[] { "Cut Loose", "Slip Past", "Steady Line", "Prize Hook" });
        Chain().TryChangeCreatureMode(controller.CatchChain[2].InstanceId, Resolver()); Refresh();
        yield return null; Capture("UI07Gameplay4K.png");

        Status = "Role commitment and transition locks";
        Fixture(0, "Glass Minnow", new[] { "Sardine" }, new[] { "Cut Loose", "Slip Past", "Steady Line", "Prize Hook" });
        var face = Get<CreatureCardView>(view, "encounterCardView"); var bait = Get<RectTransform>(face, "baitRulesRegion").GetComponent<UnityEngine.UI.Button>();
        Submit(bait); Require(controller.CatchChain.Length == 1 && face.SelectedEffectChoice == EncounterEffectChoice.Bait, "Role selection committed early.");
        Submit(hud.DescendButton); Submit(hud.DescendButton);
        Require(controller.CurrentDepth == 1 && controller.CatchChain.Last().IsBait, "Descend duplicated or lost Bait role.");
        Require(view.IsEncounterTransitionPlaying && !controller.CanSurface && !controller.CanDescend && !controller.CanUseTechniqueCard(0, out _), "Transition accepted another action.");
        yield return new WaitForSecondsRealtime(1);
        Require(!view.IsEncounterTransitionPlaying && controller.CanDescend, "Transition did not restore command state.");

        Status = "Technique targeting, cancel and stale press";
        Fixture(0, "Glass Minnow", new[] { "Sardine", "Clinging Crab" }, new[] { "Cut Loose", "Steady Line", "Prize Hook", "Recast" });
        Play(0); Require(controller.HasPendingTechniqueSelection && choices.IsVisible && !controller.CanDescend, "Target selection not modal.");
        var stale = choices.Rows[0]; var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(stale.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        Choose("Cancel play"); Require(!controller.HasPendingTechniqueSelection && controller.TechniqueHand[0].DisplayName == "Cut Loose", "Cancel spent Technique.");
        Play(0); ExecuteEvents.Execute(stale.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        Require(controller.HasPendingTechniqueSelection && controller.CatchChain.Length == 2, "Stale modal press selected a new target.");
        Capture("UI07Choice4K.png"); Choose("Sardine");
        Require(controller.CatchChain.Length == 1 && !controller.HasPendingTechniqueSelection && !choices.IsVisible, "Release did not finish through the real choice view.");

        Status = "Recovery and mode changes";
        Fixture(0, "Glass Minnow", new[] { "Cod", "Mussel" }, new[] { "Cut Loose", "Steady Line", "Prize Hook", "Recast" });
        Chain().TryChangeCreatureMode(controller.CatchChain[0].InstanceId, Resolver()); Refresh();
        Play(0); Choose("Mussel"); Require(controller.HasPendingCreatureChoice && choices.IsVisible, "Cod recovery not presented.");
        Choose("Cut Loose"); Require(!controller.HasPendingCreatureChoice && controller.TechniqueHand.Length <= 4 && controller.TechniqueHand.Any(c => c.DisplayName == "Cut Loose"), "Recovery lost the just-used card or overflowed.");
        Fixture(0, "Glass Minnow", new[] { "Sardine" }, new[] { "Re-rig", "Cut Loose", "Steady Line", "Recast" });
        Play(0); Choose("Sardine"); Require(controller.CatchChain[0].IsBait && controller.CatchChain.Length == 1, "Re-rig removed or reordered its card.");

        Status = "Hidden encounter and avoidance";
        Fixture(0, "Glass Minnow", Array.Empty<string>(), new[] { "Clear Water", "Slip Past", "Prize Hook", "Steady Line" });
        var encounter = Encounters(); var squid = Card("Squid"); var conceal = squid.Effects.First(e => e.EffectType == CardEffectType.QueueCreatureEncounterConcealment);
        encounter.GrantCreatureConcealment(conceal, squid); encounter.SetCurrentEncounter(Card("Sardine")); Refresh();
        Require(controller.CurrentEncounterIdentityHidden && Get<TMP_Text>(Get<CreatureCardView>(view, "encounterCardView"), "cardNameText").text == "UNKNOWN CREATURE", "Hidden species leaked.");
        Play(0); Require(!controller.CurrentEncounterIdentityHidden, "Clear Water failed to reveal assigned concealment.");
        encounter.SetCurrentEncounter(Card("Glass Minnow")); Refresh(); Require(controller.CurrentEncounterIdentityHidden, "Second concealment not assigned.");
        Play(Array.FindIndex(controller.TechniqueHand, c => c.DisplayName == "Slip Past"));
        Require(choices.Rows.Any(r => r.gameObject.activeInHierarchy && RowName(r) == "Unknown creature"), "Hidden Technique target leaked species.");
        Choose("Unknown creature"); Require(controller.CurrentDepth == 0 && controller.HookedEncounter == null && controller.CatchChain.Length == 0 && controller.CanSurface, "Skip committed, advanced or removed Surface.");

        Status = "Hazard cancellation, load relief, prizes and results";
        Fixture(4, "Silt Burst", new[] { "Spiny Lobster" }, new[] { "Hold Steady", "Steady Line", "Prize Hook", "Recast" });
        Play(0); Choose("Silt Burst");
        Require(Encounters().CurrentEventOccurrence.State == EventOccurrenceState.Cancelled && controller.CatchChain[0].CurrentValue == 28, "Hazard cancellation/reaction failed.");
        Play(Array.FindIndex(controller.TechniqueHand, c => c.DisplayName == "Steady Line"));
        Play(Array.FindIndex(controller.TechniqueHand, c => c.DisplayName == "Prize Hook"));
        Require(controller.CanSurface, "Cancelled event closed the reaction window.");
        Submit(hud.SurfaceButton); Require(!controller.RunActive && view.IsSurfaceTransitionPlaying && !results.IsVisible, "Results opened before ascent.");
        yield return new WaitForSecondsRealtime(2);
        Require(results.IsVisible && controller.LastHaulValue == 28, "Surface result missing or changed haul."); Capture("UI07Results4K.png");
        var upgrade = results.GetComponentsInChildren<UnityEngine.UI.Button>().First(b => b.name == "Upgrade Line"); Submit(upgrade);
        Require(controller.ProgressionLineCapacityBonus == 2 && controller.TotalGold == 18, "Result purchase did not use existing progression.");
        Submit(upgrade); Require(controller.TotalGold == 18, "Disabled progression purchase duplicated.");
        Submit(results.NextRunButton); Require(controller.RunActive && !results.IsVisible && controller.TechniqueHand.Length == 4, "Next run failed.");

        Status = "Confirmed event action and Recast choices";
        Fixture(4, "Loose Knot", new[] { "Sardine" }, new[] { "Recast", "Steady Line", "Prize Hook", "Cut Loose" });
        Play(0); Choose("Steady Line"); Require(controller.HasPendingTechniqueSelection && choices.IsVisible, "Recast did not retain its multi-selection step.");
        Choose("Recast selected"); Require(!controller.HasPendingTechniqueSelection && controller.TechniqueHand.Length <= 4, "Recast did not finish filtering.");
        Submit(hud.SurfaceButton); Require(controller.HasConfirmedEventAction && choices.IsVisible && !controller.CanUseTechniqueCard(0, out _), "Event resolution reopened Techniques.");
        Choose("Sardine"); yield return new WaitForSecondsRealtime(2);
        Require(!controller.RunActive && results.IsVisible && controller.LastHaul.Length == 0, "Confirmed event did not finish before Surface.");

        Status = "Apex and biome boundary";
        Fixture(20, "Glass Minnow", Array.Empty<string>(), new[] { "Steady Line", "Prize Hook", "Cut Loose", "Slip Past" });
        Submit(hud.DescendButton); yield return new WaitForSecondsRealtime(1);
        Require(controller.CurrentEncounter != null && controller.CurrentEncounter.CardType == CardType.ApexEncounter, "Apex reveal missing.");
        Submit(hud.DescendButton); yield return new WaitForSecondsRealtime(1);
        Require(controller.NextWatersPresented && controller.CurrentEncounter.CardType == CardType.Location && !controller.CanDescend && controller.CanSurface, "Unconfigured biome boundary lost Surface or invented a destination.");
        var currentBiome = UnityEngine.Object.Instantiate(controller.CurrentBiome); fixtures.Add(currentBiome);
        var destination = UnityEngine.Object.Instantiate(controller.CurrentBiome); fixtures.Add(destination); Set(currentBiome, "nextBiome", destination); Set(controller, "currentBiome", currentBiome); Refresh();
        int apexId = controller.CatchChain.Last().InstanceId; Submit(hud.DescendButton); yield return new WaitForSecondsRealtime(1);
        Require(controller.CurrentBiome == destination && controller.CurrentDepth == destination.StartingDepth && controller.CatchChain.Any(c => c.InstanceId == apexId), "Biome entry reset the chain or failed its view binding.");

        Status = "Scene reload and normal run";
        var oldHand = view.GetComponentInChildren<JournalTechniqueHandPresentation>();
        EditorSceneManager.LoadSceneInPlayMode(JournalComparisonSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
        yield return null; yield return null; Bind();
        Require(controller.RunActive && controller.TechniqueHand.Length == 4 && controller.TechniqueDrawPile.Length == 8 && oldHand == null, "Scene reload lost binding or retained hand stage.");
        Require(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length == 2, "Reload left orphaned projection cameras.");
        for (int step = 0; step < 4; step++)
        {
            Require(controller.CanDescend, "Normal run stuck before Descend."); Submit(hud.DescendButton);
            float deadline = Time.realtimeSinceStartup + 6;
            while ((controller.HasPendingCreatureChoice || view.IsEncounterTransitionPlaying) && Time.realtimeSinceStartup < deadline)
            {
                if (choices.IsVisible) ChooseDefault(); yield return null;
            }
            Require(!controller.HasPendingCreatureChoice && !view.IsEncounterTransitionPlaying, "Normal run choice/transition timed out.");
        }
        Submit(hud.SurfaceButton); float end = Time.realtimeSinceStartup + 15;
        while ((controller.RunActive || view.IsSurfaceTransitionPlaying) && Time.realtimeSinceStartup < end)
        { if (choices.IsVisible) ChooseDefault(); yield return null; }
        Require(!controller.RunActive && !view.IsSurfaceTransitionPlaying, "Normal Surface timed out.");
        Require(results.IsVisible && controller.LastSurfaceDepth == 4, "Normal run did not reach a result.");
        Submit(results.NextRunButton); Require(controller.RunActive && controller.CurrentDepth == 0 && controller.TechniqueHand.Length == 4, "Normal result restart failed.");
    }

    /// <summary>Resolves optional choices conservatively and mandatory choices using a legal displayed row.</summary>
    private static void ChooseDefault()
    {
        var active = choices.Rows.Where(r => r.gameObject.activeInHierarchy).ToArray();
        var optional = active.FirstOrDefault(r => RowName(r).IndexOf("decline", StringComparison.OrdinalIgnoreCase) >= 0 || RowName(r).IndexOf("skip", StringComparison.OrdinalIgnoreCase) >= 0 || RowName(r).IndexOf("no ", StringComparison.OrdinalIgnoreCase) == 0);
        Submit(optional ?? active.Last());
    }
    /// <summary>Creates an in-memory repeatable setup using existing definitions and runtime owners only.</summary>
    private static void Fixture(int depth, string encounter, string[] caught, string[] hand)
    {
        Set(view, "lastRunActive", (bool?)false); controller.StartRun();
        var scenario = ScriptableObject.CreateInstance<CatchChainScenarioDefinition>(); fixtures.Add(scenario);
        Set(scenario, "depth", depth); Set(scenario, "lineCapacity", 100); Set(scenario, "startingCatches", caught.Select(Card).ToArray()); Set(scenario, "currentEncounter", Card(encounter));
        Set(view, "hasPresentedEncounter", false);
        Require(controller.LoadDebugScenario(scenario), "Fixture did not load.");
        Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime").Initialize(hand.Select(Card).ToArray(), hand.Length, new System.Random(1), null);
        // Explicit hand ordering is test-only; authored starter data is never changed.
        Set(Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"), "hand", hand.Select(Card).ToArray()); Refresh();
    }
    /// <summary>Binds the actual currently loaded candidate scene after start or reload.</summary>
    private static void Bind()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects(); controller = roots.SelectMany(g => g.GetComponentsInChildren<FishingRunController>()).Single();
        view = roots.SelectMany(g => g.GetComponentsInChildren<FishingRunView>()).Single(); hud = view.GetComponent<JournalGameplayHud>(); choices = view.GetComponentInChildren<JournalChoiceModal>(true); results = view.GetComponentInChildren<JournalResultModal>(true);
    }
    /// <summary>Plays through the real projected hand button.</summary>
    private static void Play(int index) { Require(index >= 0, "Technique not in hand."); Submit(view.GetComponentInChildren<JournalTechniqueHandPresentation>().Cards[index].GetComponent<UnityEngine.UI.Button>()); }
    /// <summary>Selects one actual displayed modal row, including its runtime target closure.</summary>
    private static void Choose(string prefix)
    { var row = choices.Rows.FirstOrDefault(r => r.gameObject.activeInHierarchy && RowName(r).StartsWith(prefix, StringComparison.Ordinal)); Require(row != null, "Missing choice " + prefix); Submit(row); }
    /// <summary>Reads the displayed option identity without using private controller target state.</summary>
    private static string RowName(JournalChoiceButton row) { return row.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Name").text; }
    /// <summary>Uses normal keyboard submission and Button interactability.</summary>
    private static void Submit(UnityEngine.UI.Button button) { ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler); }
    /// <summary>Refreshes production view bindings after controlled fixture changes.</summary>
    private static void Refresh() { typeof(FishingRunController).GetMethod("RefreshViews", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null); }
    /// <summary>Reads fixture-owned runtime state without changing authored assets.</summary>
    private static T Get<T>(object owner, string field) { return (T)owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Configures only temporary test instances.</summary>
    private static void Set(object owner, string field, object value) { owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Returns the real chain owner.</summary>
    private static CatchChainRuntime Chain() { return Get<CatchChainRuntime>(controller, "catchChainRuntime"); }
    /// <summary>Returns the real encounter owner.</summary>
    private static EncounterRuntime Encounters() { return Get<EncounterRuntime>(controller, "encounterRuntime"); }
    /// <summary>Returns the real effect resolver.</summary>
    private static EffectResolver Resolver() { return Get<EffectResolver>(controller, "effectResolver"); }
    /// <summary>Finds approved production definitions by identity.</summary>
    private static CardDefinition Card(string name) { return AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.DisplayName == name); }
    /// <summary>Writes a native 4K render of the actual loaded scene and restores the screen-space presentation immediately.</summary>
    private static void Capture(string file)
    {
        Canvas canvas = view.GetComponent<Canvas>(); Camera camera = Camera.main; var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        RenderMode mode = canvas.renderMode; Camera oldCamera = canvas.worldCamera; float scale = canvas.scaleFactor, distance = canvas.planeDistance; bool enabled = scaler.enabled; var oldTarget = camera.targetTexture;
        var target = new RenderTexture(3840, 2160, 24); RenderTexture previous = RenderTexture.active; Texture2D pixels = null;
        try
        {
            camera.targetTexture = target; scaler.enabled = false; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.scaleFactor = 1;
            Canvas.ForceUpdateCanvases(); view.GetComponentInChildren<JournalTechniqueHandPresentation>(true).RenderNow(); camera.Render(); RenderTexture.active = target;
            pixels = new Texture2D(3840, 2160, TextureFormat.RGBA32, false); pixels.ReadPixels(new Rect(0, 0, 3840, 2160), 0, 0); pixels.Apply(); File.WriteAllBytes(Output + file, pixels.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous; camera.targetTexture = oldTarget; canvas.renderMode = mode; canvas.worldCamera = oldCamera; canvas.planeDistance = distance; canvas.scaleFactor = scale; scaler.enabled = enabled;
            Canvas.ForceUpdateCanvases(); target.Release(); UnityEngine.Object.Destroy(target); if (pixels != null) UnityEngine.Object.Destroy(pixels);
        }
    }
    /// <summary>Stops at the first broken integration boundary.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }

    /// <summary>Checks full/empty hands, all approved techniques and a long overloaded chain using transient production definitions.</summary>
    public static void ValidateReleaseStress()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Stress the loaded comparison in Play Mode.");
        byte[] beforeScene = File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath), beforeSource = File.ReadAllBytes("Assets/Scenes/SampleScene.unity"), beforeBuild = File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset");
        Bind(); assertions = 0;
        Fixture(10, "Small Octopus", new[] { "Sardine", "Clinging Crab", "Glass Minnow" }, new[] { "Cut Loose", "Slip Past", "Steady Line", "Prize Hook" });
        Chain().TryChangeCreatureMode(controller.CatchChain[2].InstanceId, Resolver()); Refresh(); Capture("UI08Assembled4K.png");
        Fixture(10, "Fresh Shell Fragments", Enumerable.Repeat("Sardine", 16).ToArray(), new[] { "Clear Water", "Hold Steady", "Re-rig", "Recast" });
        Set(controller, "lineCapacity", 10); Refresh();
        var hand = view.GetComponentInChildren<JournalTechniqueHandPresentation>(); var chain = view.GetComponentInChildren<JournalCatchChainPresentation>();
        Require(controller.CurrentLineLoad == 16 && hud.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "LINE LOAD  16 / 10"), "Overload display does not follow controller.");
        Require(chain.Cards.Count == 16 && chain.Cards.All(c => c.gameObject.activeSelf), "Long chain truncated.");
        Require(hand.Cards.Take(4).All(c => c.gameObject.activeSelf), "Second active Technique set missing.");
        var scroll = chain.GetComponentInChildren<UnityEngine.UI.ScrollRect>(); scroll.verticalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
        Require(scroll.content.rect.height > scroll.viewport.rect.height, "Long chain not scrollable."); Capture("UI08LongChainEvent4K.png");
        Fixture(0, "Glass Minnow", Array.Empty<string>(), Array.Empty<string>());
        Require(controller.CatchChain.Length == 0 && controller.TechniqueHand.Length == 0, "Empty fixture failed.");
        Require(!hand.Cards.Any(c => c.gameObject.activeSelf) && !chain.Cards.Any(c => c.gameObject.activeSelf), "Empty views retain stale physical slots."); Capture("UI08Empty4K.png");
        foreach (var fixture in fixtures) if (fixture != null) UnityEngine.Object.Destroy(fixture); fixtures.Clear();
        controller.StartRun();
        bool unchanged = beforeScene.SequenceEqual(File.ReadAllBytes(JournalComparisonSceneBuilder.ScenePath)) && beforeSource.SequenceEqual(File.ReadAllBytes("Assets/Scenes/SampleScene.unity")) && beforeBuild.SequenceEqual(File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"));
        Require(unchanged, "Stress modified saved scene/build data.");
        File.WriteAllText(Output + "UI08StressValidation.json", JsonUtility.ToJson(new Report { passed = true, assertions = assertions, sceneFilesUnchanged = unchanged, scope = "In-memory fixtures: all eight Techniques, 16-card scrollable overloaded chain, event, empty hand/chain. Existing per-definition fit/mask checks cover the full authored roster." }, true));
    }
}
