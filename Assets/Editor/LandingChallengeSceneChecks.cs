using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Exercises the installed plain Landing loop with temporary authored scoring fixtures and saving disabled.</summary>
public static class LandingChallengeSceneChecks
{
    public static string Result { get; private set; } = "Not run";
    private static int checks;
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    /// <summary>Runs one success or failure branch in a fresh Harbor session, without writing player progression.</summary>
    public static void Begin(bool succeed)
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play Mode session.");
        var controller = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        controller.SetDebugSaveResetPending(true);
        checks = 0; Result = "Running";
        controller.StartCoroutine(Validate(succeed));
    }

    /// <summary>Checks real timing/commands, source-attributed scoring, the Build boundary and exactly-once full-value failure settlement.</summary>
    private static IEnumerator Validate(bool succeed)
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var controller = harbor.Controller;
        Check(!controller.RunActive && harbor.HubVisible, "Expected fresh Harbor.");
        Check(controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Schooling", "Small" }), "Could not prepare tags.");
        harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        var chain = (CatchChainRuntime)typeof(FishingRunController).GetField("catchChainRuntime", Private).GetValue(controller);
        var resolver = new EffectResolver();
        var catchSource = Fixture("Landing test Catch", 17, false);
        var baitSource = Fixture("Landing test Bait", 999, true);
        try
        {
            chain.Add(catchSource, resolver, CatchAttachmentRole.Catch);
            chain.Add(baitSource, resolver, CatchAttachmentRole.Bait);
            int expectedHaul = chain.Catches[0].HaulValueContribution;
            int initialGold = controller.TotalGold, completions = 0, landed = 0;
            controller.RunCompleted += (id, result) => completions++;
            controller.CreatureLanded += (card, revision) => landed++;
            var slot = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
            SlotReelAnimationChecks.SettleForControlFixture(slot, controller);
            Check(controller.TryAcceptSlotMachine(controller.SlotMachine.Revision), "Slot acceptance failed.");
            var model = controller.Landing;
            var view = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>();
            Check(view != null && !slot.gameObject.activeInHierarchy, "Discover and Landing were not separated.");
            Check(model.State == LandingChallengeState.Active && model.Rig.Count == 2 && model.Points == 0 && model.AttemptsRemaining == 4, "Landing did not initialize a committed-rig snapshot.");
            Check(controller.AwaitingLandingChallenge && !controller.AwaitingRigPlacement && controller.CatchChain.Length == 2, "Creature acquired before landing.");
            Check(!controller.TryChooseEncounterRole(EncounterEffectChoice.Catch) && !controller.TryDescend() && !controller.TrySurface(), "Legacy action bypassed Land.");
            var viewData = new SerializedObject(view);
            var action = viewData.FindProperty("strikeInput");
            Check(action != null, "Space input was not authored.");
            long occurrence = model.Occurrence;
            // Alter only the disposable source after Begin; the score must retain its captured +5 and x2.
            var changed = new SerializedObject(catchSource); changed.FindProperty("landingCatchEffects").GetArrayElementAtIndex(0).FindPropertyRelative("bonus").floatValue = 500; changed.ApplyModifiedPropertiesWithoutUndo();
            Capture(view, succeed ? "Success-Initial.png" : "Failure-Initial.png");
            int expectedStrikes = succeed ? 1 : 4;
            for (int strike = 0; strike < expectedStrikes; strike++)
            {
                double deadline = Time.realtimeSinceStartupAsDouble + 60;
                bool aligned = false;
                while (Time.realtimeSinceStartupAsDouble < deadline)
                {
                    model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
                    double distance = Math.Abs(model.FishPosition - model.ZonePosition);
                    aligned = model.CanStrike && (succeed ? distance < model.PerfectWidth * .2
                        : distance > (model.GoodWidth + model.FishWidth) * .5 + .04);
                    if (aligned) break;
                    yield return null;
                }
                Check(aligned, "No safe alignment reached for the fixture.");
                yield return null;
                model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
                double currentDistance = Math.Abs(model.FishPosition - model.ZonePosition);
                if (succeed ? currentDistance >= (model.PerfectWidth + model.FishWidth) * .5
                    : currentDistance <= (model.GoodWidth + model.FishWidth) * .5) { strike--; continue; }
                long revision = model.Revision;
                var button = (UnityEngine.UI.Button)viewData.FindProperty("strike").objectReferenceValue;
                button.onClick.Invoke();
                Check(model.Revision != revision, "UI strike did not reach controller/runtime.");
                Check(!controller.TryStrikeLanding(occurrence, revision), "Duplicate UI strike consumed another attempt.");
                if (!succeed && strike < 3) Check(model.Points == 0 && model.AttemptsRemaining == 3 - strike, "Miss gained regular bonuses or consumed the wrong attempts.");
                yield return null;
            }
            if (succeed)
            {
                Check(controller.RunActive && controller.AwaitingRigPlacement && !controller.AwaitingLandingChallenge, "Success did not expose Build handoff.");
                Check(model.State == LandingChallengeState.Succeeded && model.Points == 50 && model.AttemptsRemaining == 3, "Snapshot/role-based amplification was incorrect.");
                Check(model.LastScore.Contributions.Count == 2 && controller.CatchChain.Length == 2 && landed == 1 && completions == 0, "Success attached a creature or dispatched duplicate outcomes.");
                Check(!controller.TryChooseEncounterRole(EncounterEffectChoice.Bait) && !controller.TryDescend(), "Legacy commitment bypassed placement.");
                Check(controller.TotalGold == initialGold, "Landing success paid sale value before expedition end.");
                Capture(view, "Landing-Succeeded.png");
                chain.Reset();
            }
            else
            {
                Check(!controller.RunActive && controller.LandingFailureVisible && controller.LastHaulValue == expectedHaul, "Failure lost accumulated Catch value.");
                Check(controller.LastGoldAwarded == expectedHaul && controller.TotalGold == initialGold + expectedHaul, "Full haul was not paid exactly once.");
                Check(controller.CatchChain.Length == 0 && controller.LastHaul.Length == 2 && !controller.LastRunEndedByLineBreak, "Failure retained live attachments or used line-break loss.");
                Check(completions == 1 && landed == 0 && controller.LastHaul[1].HaulValueContribution == 0, "Failed creature acquired or Bait paid sale value.");
                Check(!controller.TryStrikeLanding(occurrence, model.Revision) && controller.TotalGold == initialGold + expectedHaul, "Duplicate terminal command awarded again.");
                Capture(view, "Landing-Failed.png");
                ((UnityEngine.UI.Button)viewData.FindProperty("returnToHarbor").objectReferenceValue).onClick.Invoke();
                Check(harbor.HubVisible && !view.gameObject.activeInHierarchy, "Failure could not return to Harbor.");
                harbor.TryStartNewRun();
                ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
                Check(controller.RunActive && controller.CatchChain.Length == 0 && controller.Landing.Points == 0
                    && controller.TotalGold == initialGold + expectedHaul, "New expedition retained creatures/points or erased permanent value.");
            }
            Result = "Passed " + checks + (succeed ? " live success checks" : " live failure/restart checks");
            Debug.Log(Result);
        }
        finally { UnityEngine.Object.DestroyImmediate(catchSource); UnityEngine.Object.DestroyImmediate(baitSource); }
    }

    /// <summary>Creates disposable role-specific abilities with heavy legacy weight to verify that capacity cannot penalize failure.</summary>
    private static CardDefinition Fixture(string name, int value, bool bait)
    {
        var definition = ScriptableObject.CreateInstance<CardDefinition>();
        var data = new SerializedObject(definition); data.FindProperty("displayName").stringValue = name;
        data.FindProperty("uniqueId").stringValue = "landing-test-" + name; data.FindProperty("cardType").enumValueIndex = (int)CardType.Creature;
        data.FindProperty("value").intValue = value; data.FindProperty("weight").intValue = 999;
        var effects = data.FindProperty(bait ? "landingBaitEffects" : "landingCatchEffects"); effects.arraySize = 1;
        effects.GetArrayElementAtIndex(0).FindPropertyRelative("label").stringValue = bait ? "Fixture multiplier" : "Fixture bonus";
        effects.GetArrayElementAtIndex(0).FindPropertyRelative("bonus").floatValue = bait ? 0 : 5;
        effects.GetArrayElementAtIndex(0).FindPropertyRelative("multiplier").floatValue = bait ? 2 : 1;
        data.ApplyModifiedPropertiesWithoutUndo(); return definition;
    }
    /// <summary>Captures the real native Landing view through the active render pipeline.</summary>
    private static void Capture(LandingChallengeView view, string file)
    { SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), file, output: "Docs/Validation/LandingChallenge/"); }
    /// <summary>Stops at the first failed lifecycle, scoring or payout invariant.</summary>
    private static void Check(bool value, string message)
    { if (!value) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
