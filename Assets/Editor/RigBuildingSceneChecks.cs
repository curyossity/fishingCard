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

/// <summary>Verifies the real Discover/Land/Build/Upgrade loop, native placement UI and complete failure payout with persistence disabled.</summary>
public static class RigBuildingSceneChecks
{
    public const string Output = "Docs/Validation/RigBuilding/";
    public static string Result { get; private set; } = "Not run";
    private static int checks;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    /// <summary>Starts the coroutine in a fresh comparison Play session; it never writes player progression.</summary>
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play session.");
        var controller = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        controller.SetDebugSaveResetPending(true); Application.runInBackground = true;
        checks = 0; Result = "Running"; controller.StartCoroutine(Run());
    }

    /// <summary>Exercises actual strike and button commands, including duplicate upgrades with no extra confirmation.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var controller = harbor.Controller;
        Check(!controller.RunActive, "Expected fresh Harbor.");
        controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Schooling", "Small" }); harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        yield return null;
        yield return Accept(controller);
        var first = controller.CurrentEncounter;
        Check(controller.CatchChain.Length == 0, "Run did not begin with an empty rig.");
        yield return Land(controller, true);
        var build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        Check(build != null && build.gameObject.activeInHierarchy && controller.AwaitingRigPlacement, "Successful Landing did not show Build.");
        Check(!UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>(FindObjectsInactive.Include).gameObject.activeInHierarchy
            && !UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(FindObjectsInactive.Include).gameObject.activeInHierarchy, "Phases overlap.");
        Check(controller.CatchChain.Length == 0, "New species attached before placement.");
        Capture(build, "First-Species.png");
        var data = new SerializedObject(build); var commit = (UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue;
        Check(commit.interactable, "First insertion is not selectable.");
        var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null,
            ((RectTransform)commit.transform).TransformPoint(((RectTransform)commit.transform).rect.center)) };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject) == commit.gameObject, "Placement button is blocked.");
        long occurrence = controller.BuildOccurrence, revision = controller.Rig.Revision;
        commit.onClick.Invoke(); yield return null;
        Check(controller.CatchChain.Length == 1 && controller.CatchChain[0].Definition == first && controller.CatchChain[0].Level == 1, "Placement failed.");
        Check(controller.SlotMachine.Active && !controller.AwaitingRigPlacement && !build.gameObject.activeInHierarchy, "Placement did not automatically continue to Discover.");
        Check(!controller.TryCommitRigPlacement(occurrence, revision, 0) && controller.Rig.HaulValue == first.Value, "Stale placement acquired twice.");

        ForceNextCreature(controller, first); yield return Accept(controller); yield return Land(controller, true); yield return null;
        Check(controller.CatchChain.Length == 1 && controller.CatchChain[0].Level == 2 && controller.CatchChain[0].CopiesCollected == 2, "Duplicate did not automatically upgrade.");
        Check(controller.SlotMachine.Active && !controller.AwaitingRigPlacement && controller.Rig.HaulValue == first.Value * 2, "Duplicate required confirmation or lost sale value.");
        Check(!controller.TryChooseEncounterRole(EncounterEffectChoice.Bait) && !controller.TryDescend() && !controller.TrySurface(), "Removed role/Descend/Surface commands remain active.");
        var pool = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
            .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => c.CardType == CardType.Creature && c != first).ToArray();
        var next = pool.First(c => c.IsAvailableInBiome(controller.CurrentBiome.BiomeId));
        ForceNextCreature(controller, next); yield return Accept(controller); yield return Land(controller, true);
        Check(controller.AwaitingRigPlacement && controller.CatchChain.Length == 1, "New species bypassed placement.");
        build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        var originalIds = controller.CatchChain.Select(m => m.InstanceId).ToArray(); int value = controller.Rig.HaulValue;
        var gapContent = (RectTransform)new SerializedObject(build).FindProperty("railContent").objectReferenceValue;
        var gapButtons = gapContent.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b => b.gameObject.activeSelf).ToArray();
        Check(gapButtons.Length == 2, "One-member rig does not provide two gaps.");
        foreach (var gap in gapButtons) gap.onClick.Invoke();
        Check(controller.CatchChain.Select(m => m.InstanceId).SequenceEqual(originalIds) && controller.Rig.HaulValue == value, "Position preview changed live order/value.");
        Capture(build, "Placement-Preview.png");
        var longRigSpecies = pool.Where(c => c != next).Take(7).ToArray();
        foreach (var creature in longRigSpecies)
            Check(controller.Rig.TryAcquire(controller.Rig.Revision, creature, controller.Rig.Members.Length, out _, out _), "Long-rig fixture failed.");
        typeof(FishingRunController).GetMethod("RefreshViews", Private).Invoke(controller, null);
        data = new SerializedObject(build);
        var rail = (UnityEngine.UI.ScrollRect)data.FindProperty("railScroll").objectReferenceValue;
        var rig = (UnityEngine.UI.ScrollRect)data.FindProperty("rigScroll").objectReferenceValue;
        Check(rail.content.rect.width > rail.viewport.rect.width && rig.content.rect.height > rig.viewport.rect.height, "Long rigs are clipped without scrolling.");
        foreach (var text in build.GetComponentsInChildren<TMP_Text>()) Check(!text.text.Equals("CATCH") && !text.text.Equals("BAIT"), "Catch/Bait remains in Build UI.");
        foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 800), new Vector2Int(2560, 1080) })
            SlotMachineSceneChecks.Capture(build.GetComponent<Canvas>(), "Build-" + size.x + "x" + size.y + ".png", size.x, size.y, Output);
        ((UnityEngine.UI.Button)data.FindProperty("commit").objectReferenceValue).onClick.Invoke(); yield return null;
        Check(controller.SlotMachine.Active && controller.CatchChain.Length == 9, "Long-rig placement failed to continue.");
        int haul = controller.Rig.HaulValue, gold = controller.TotalGold, completions = 0;
        controller.RunCompleted += (id, result) => completions++;
        yield return Accept(controller); yield return Land(controller, false); yield return null;
        Check(!controller.RunActive && controller.LastHaulValue == haul && controller.TotalGold == gold + haul, "Failure did not pay the complete accumulated upgraded haul.");
        Check(controller.CatchChain.Length == 0 && controller.Rig.HaulValue == 0 && completions == 1, "Failure did not clear temporary rig state exactly once.");
        Check(controller.LastHaul.Any(c => c.Definition == first && c.Level == 2 && c.CopiesCollected == 2), "Result lost duplicate upgrades.");
        var landingView = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>();
        ((UnityEngine.UI.Button)new SerializedObject(landingView).FindProperty("returnToHarbor").objectReferenceValue).onClick.Invoke();
        harbor.TryStartNewRun(); ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        Check(controller.RunActive && controller.CatchChain.Length == 0 && controller.Rig.HaulValue == 0 && controller.TotalGold == gold + haul, "Next expedition retained upgrades or lost permanent value.");
        Result = "Passed " + checks + " live Build/Upgrade checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + ",\"duplicateUpgradeAutomatic\":true,\"fullFailurePayout\":true,\"nextRunEmpty\":true}\n");
        Debug.Log(Result);
    }

    /// <summary>Uses the actual strike command after observing robust Perfect/Miss alignment, with no simulated landing outcome.</summary>
    private static IEnumerator Land(FishingRunController controller, bool succeed)
    {
        var model = controller.Landing; double deadline = Time.realtimeSinceStartupAsDouble + 90;
        while (model.State == LandingChallengeState.Active && Time.realtimeSinceStartupAsDouble < deadline)
        {
            model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance = Math.Abs(model.FishPosition - model.ZonePosition);
            bool aligned = succeed ? distance < model.PerfectWidth * .15 : distance > (model.GoodWidth + model.FishWidth) * .5 + .05;
            if (model.CanStrike && aligned) controller.TryStrikeLanding(model.Occurrence, model.Revision);
            yield return null;
        }
        Check(succeed ? controller.AwaitingRigPlacement || controller.SlotMachine.Active : controller.LandingFailureVisible, "Landing fixture did not reach its expected outcome.");
    }
    /// <summary>Accepts through the real Discover command after completing only its cosmetic reel animation.</summary>
    private static IEnumerator Accept(FishingRunController controller)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 15;
        var ready = typeof(FishingRunController).GetProperty("CoreActionInputReady", Private);
        while (!(bool)ready.GetValue(controller) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        SlotReelAnimationChecks.SettleForControlFixture(UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(), controller);
        yield return null;
        Check(controller.TryAcceptSlotMachine(controller.SlotMachine.Revision), "Could not accept Discover.");
    }
    /// <summary>Constrains only this runtime selector for deterministic duplicate/new-species coverage; no asset or eligibility rule is edited.</summary>
    private static void ForceNextCreature(FishingRunController controller, CardDefinition creature)
    {
        var encounter = (EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime", Private).GetValue(controller);
        typeof(EncounterRuntime).GetField("slotCandidates", Private).SetValue(encounter, new[] { creature });
        IReadOnlyList<TagSlotMachineRuntime.Symbol> symbols = new[] { new TagSlotMachineRuntime.Symbol(creature.Tags[0], 1) };
        controller.SlotMachine.Begin(new[] { symbols, symbols, symbols }, 1); controller.SlotMachine.TrySpin(controller.SlotMachine.Revision, new System.Random(1));
        typeof(FishingRunController).GetMethod("RefreshViews", Private).Invoke(controller, null);
    }
    /// <summary>Captures the actual Build canvas through the project render pipeline.</summary>
    private static void Capture(RigBuildingView view, string file) => SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), file, output: Output);
    /// <summary>Retains a readable failure status if a coroutine invariant fails.</summary>
    private static void Check(bool valid, string message)
    {
        if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++;
    }
}
