using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Exercises unique species, upgrades, irreversible order, deterministic previews and derived power using disposable definitions.</summary>
public static class RigBuildingChecks
{
    private static int checks;
    private static readonly List<CardDefinition> fixtures = new List<CardDefinition>();

    /// <summary>Checks the saved native Build prefab and its scene binding so successful Land cannot fall through to the old screen.</summary>
    public static int ValidateAssets()
    {
        checks = 0;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigBuildingSetup.PrefabPath);
        Check(prefab != null && prefab.GetComponent<Canvas>().sortingOrder == 85, "Build canvas is missing or incorrectly layered.");
        var data = new SerializedObject(prefab.GetComponent<RigBuildingView>());
        foreach (string field in new[] { "heading", "haul", "details", "message", "previewLabel", "emptyRig", "cardPrefab", "railCardPrefab", "portrait", "creatureName", "tags", "levelLabel", "levelDelta", "saleLabel", "previewCaption", "railHeading", "upgradeStatus", "rigContent", "railContent", "rigScroll", "railScroll", "detailsScroll", "gapTemplate", "commit", "socketIdle", "socketSelected", "socketBlocked", "selectionCaret" })
            Check(data.FindProperty(field).objectReferenceValue != null, "Build field is unbound: " + field);
        var details = (TMPro.TMP_Text)data.FindProperty("details").objectReferenceValue;
        Check(details.color.r > .7f && details.color.g > .7f && details.richText, "Ability text is unreadable on the single dark frame.");
        Check(prefab.GetComponent<UnityEngine.UI.Image>()?.raycastTarget == true, "Clicks can leak through Build into the retired UI.");
        var view = UnityEngine.Object.FindAnyObjectByType<FishingRunView>(FindObjectsInactive.Include);
        var bound = new SerializedObject(view).FindProperty("rigBuildingView").objectReferenceValue as RigBuildingView;
        Check(bound != null && bound.gameObject.scene == view.gameObject.scene, "Saved run view has no Build binding.");
        var scaler = prefab.GetComponent<UnityEngine.UI.CanvasScaler>();
        Check(scaler.referenceResolution == new Vector2(1920, 1080) && scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand, "Build scaling changed.");
        return checks;
    }

    /// <summary>Runs pure rig invariants without modifying current species, scene state or progression.</summary>
    public static int ValidateRules()
    {
        checks = 0;
        try
        {
            var sardine = Fixture("Sardine", 5, "Schooling");
            var a = Fixture("School A", 3, "Schooling"); var b = Fixture("School B", 4, "Schooling");
            var c = Fixture("School C", 6, "Schooling"); var cod = Fixture("Cod", 12, "Armored");
            Effect(sardine, RigAbilityKind.LandingMultiplier, .2f, .1f, RigPositionCount.ContiguousOther, "Schooling");
            var rig = new RigRuntime(); Acquire(rig, sardine, 0); Acquire(rig, a, 1); Acquire(rig, b, 2); Acquire(rig, c, 3);
            var ids = rig.Members.Select(m => m.InstanceId).ToArray();
            var originalMember = rig.Members[0];
            Check(Score(rig) == 32, "Level 1 contiguous multiplier must be x1.6.");
            Acquire(rig, sardine, -1); Check(Score(rig) == 38, "Level 2 multiplier must be x1.9.");
            Check(ReferenceEquals(rig.Members[0], originalMember), "An upgrade replaced the existing member object.");
            Acquire(rig, sardine, -1); Check(Score(rig) == 44, "Level 3 multiplier must be x2.2.");
            Check(rig.Members.Length == 4 && rig.Members[0].Level == 3 && rig.Members[0].AccumulatedBaseValue == 15, "Duplicates created slots or lost their base sale value.");
            Check(rig.Members.Select(m => m.InstanceId).SequenceEqual(ids), "Duplicate upgrade changed order/identity.");
            Acquire(rig, a, -1); Check(Score(rig) == 44, "An upgraded neighbor counted as multiple positions.");
            long revision = rig.Revision; int value = rig.HaulValue;
            for (int position = 0; position <= rig.Members.Length; position++)
            {
                Check(rig.TryPreview(cod, position, out var preview, out _, out _), "Valid gap was not previewable.");
                Check(preview.Where(m => m.Definition != cod).Select(m => m.InstanceId).SequenceEqual(ids), "Preview rearranged existing creatures.");
                Check(preview[position].Definition == cod, "Preview inserted at the wrong position.");
            }
            Check(rig.Revision == revision && rig.HaulValue == value && rig.FindSpecies(cod.UniqueId) == null, "Preview granted value or changed live state.");
            Acquire(rig, cod, 1); Check(Score(rig) == 20, "Insertion did not break contiguous connectivity.");
            Check(!rig.TryAcquire(revision, cod, 0, out _, out _) && rig.FindSpecies(cod.UniqueId).Level == 1, "Stale command acquired twice.");
            Check(rig.TryRemoveByAbility(rig.Revision, rig.FindSpecies(cod.UniqueId).InstanceId, out _), "Authorized removal failed.");
            Check(Score(rig) == 44 && rig.HaulValue == value + cod.Value, "Removal did not restore synergy/preserve collected base value.");
            var snapshot = rig.CreateHaulSnapshot(); var result = new FishingRunResult(); result.RecordLandingFailure(snapshot, 5);
            Check(result.HaulValue == rig.HaulValue && result.Haul[0].Level == 3, "Result lost upgraded value/levels.");
            result.Record(rig.CreateHaulSnapshot(), Array.Empty<CardInstance>(), Array.Empty<CardInstance>(), 5, 0, 0, false);
            Check(result.HaulValue == rig.HaulValue && result.Haul[0].CopiesCollected == 3, "Success settlement lost duplicate value.");
            rig.Reset(); Check(rig.Members.Length == 0 && rig.HaulValue == 0 && snapshot[0].Level == 3, "Reset retained a rig or mutated result snapshots.");
            Acquire(rig, sardine, 0); Check(rig.Members[0].Level == 1 && rig.Members[0].CopiesCollected == 1, "A new expedition retained upgrades.");

            var mackerel = Fixture("Mackerel", 10, "Schooling"); Effect(mackerel, RigAbilityKind.ValueBonus, 5, 0, RigPositionCount.ContiguousOther, "Schooling");
            rig.Reset(); Acquire(rig, a, 0); Acquire(rig, b, 1); Acquire(rig, mackerel, 2); Acquire(rig, c, 3);
            Check(rig.FindSpecies(mackerel.UniqueId).CurrentValue == 25, "Contiguous value bonus is wrong.");
            Acquire(rig, cod, 3); Check(rig.FindSpecies(mackerel.UniqueId).CurrentValue == 20, "A broken group retained old value.");
            Check(rig.TryRemoveByAbility(rig.Revision, rig.FindSpecies(cod.UniqueId).InstanceId, out _), "Removal failed.");
            Check(rig.FindSpecies(mackerel.UniqueId).CurrentValue == 25, "Recalculation accumulated or lost derived value.");

            var anchored = Fixture("Anchor", 7, "Anchored"); Effect(anchored, RigAbilityKind.BlockInsertionBefore, 1);
            rig.Reset(); Acquire(rig, a, 0); Acquire(rig, anchored, 1);
            Check(!rig.CanInsert(cod, 0, out _) && !rig.CanInsert(cod, 1, out _) && rig.CanInsert(cod, 2, out _), "Rod-side insertion restriction is wrong.");
            Check(!rig.TryAcquire(rig.Revision, cod, 0, out _, out _) && rig.Members.Length == 2, "Blocked placement mutated the rig.");
            Acquire(rig, anchored, -1); Check(rig.Members[1].Level == 2, "Restriction incorrectly blocked an automatic duplicate upgrade.");

            var acquired = Fixture("Acquired grant", 5, "Armored"); Effect(acquired, RigAbilityKind.ValueBonus, 3, trigger: RigAbilityTrigger.Acquired);
            var upgraded = Fixture("Upgrade grant", 4, "Armored"); Effect(upgraded, RigAbilityKind.ValueBonus, 7, trigger: RigAbilityTrigger.Upgraded);
            rig.Reset(); Acquire(rig, acquired, 0); Acquire(rig, acquired, -1);
            Check(rig.Members[0].CurrentValue == 13, "Upgrade retriggered acquisition effects.");
            Acquire(rig, upgraded, 1); Check(rig.Members[1].CurrentValue == 4, "Initial acquisition triggered an upgrade effect.");
            Acquire(rig, upgraded, -1); Check(rig.Members[1].CurrentValue == 15, "Upgrade effect did not fire exactly once.");
            value = rig.HaulValue; rig.TryPreview(cod, 1, out _, out _, out _); rig.TryPreview(cod, 1, out _, out _, out _);
            Check(rig.HaulValue == value, "Preview triggered a live value grant.");

            var huge = Fixture("Huge", int.MaxValue, "Armored"); rig.Reset(); Acquire(rig, huge, 0); revision = rig.Revision;
            Check(!rig.TryAcquire(revision, huge, -1, out _, out _) && rig.Revision == revision && rig.Members[0].Level == 1, "Overflow partially applied an upgrade.");
            Check(sardine.Value == 5 && sardine.PrimaryAbility.Snapshot().Effects[0].BaseAmount == .2m, "Runtime changed shared species data.");
            var attempts = Fixture("Extra attempts", 1, "Armored"); Effect(attempts, RigAbilityKind.ExtraAttempts, 1, 1);
            var guard = Fixture("Miss protection", 1, "Armored"); Effect(guard, RigAbilityKind.MissProtection, 1);
            rig.Reset(); Acquire(rig, attempts, 0); Acquire(rig, guard, 1); Acquire(rig, attempts, -1);
            var landingRig = rig.SnapshotForLanding(out int extra, out int protection);
            Check(extra == 2 && protection == 1 && landingRig[0].Level == 2, "Current-level assistance was not captured.");
            var profile = ScriptableObject.CreateInstance<LandingChallengeProfile>();
            try
            {
                var model = new LandingChallengeRuntime(); model.Begin(1, 9999, profile.CreateSettings(), landingRig, new System.Random(7), 0, extra, protection);
                Check(model.InitialAttempts == 6, "Extra attempts did not reach Landing.");
                double clock = 0;
                for (int miss = 0; miss < 2; miss++)
                {
                    bool struck = false;
                    for (int step = 0; step < 10000 && !struck; step++)
                    {
                        clock += .01; model.AdvanceTo(clock);
                        if (model.CanStrike && LandingChallengeRuntime.Grade(model.FishPosition, model.FishWidth, model.ZonePosition, model.GoodWidth, model.PerfectWidth) == LandingAccuracy.Miss)
                            struck = model.TryStrike(model.Occurrence, model.Revision, clock);
                    }
                    Check(struck && model.Points == 0 && model.AttemptsRemaining == (miss == 0 ? 6 : 5), "Miss protection did not expire after exactly one Miss.");
                }
                Acquire(rig, attempts, -1); Check(model.InitialAttempts == 6 && model.Rig[0].Level == 2, "A later upgrade mutated an active challenge.");
                Check(rig.Members.All(m => m.CurrentWeight == 0 && !m.IsBait && m.ActiveEffects.Length == 0), "Rig members retained legacy role/weight effects.");
            }
            finally { UnityEngine.Object.DestroyImmediate(profile); }
            return checks;
        }
        finally { foreach (var fixture in fixtures) UnityEngine.Object.DestroyImmediate(fixture); fixtures.Clear(); }
    }

    /// <summary>Creates an isolated species, leaving the actual creature catalog untouched.</summary>
    public static CardDefinition Fixture(string name, int value, params string[] tags)
    {
        var card = ScriptableObject.CreateInstance<CardDefinition>(); fixtures.Add(card);
        var data = new SerializedObject(card); data.FindProperty("uniqueId").stringValue = "rig-check-" + name;
        data.FindProperty("displayName").stringValue = name; data.FindProperty("cardType").enumValueIndex = (int)CardType.Creature;
        data.FindProperty("value").intValue = value; var tagData = data.FindProperty("tags"); tagData.arraySize = tags.Length;
        data.FindProperty("weight").intValue = 999;
        for (int i = 0; i < tags.Length; i++) tagData.GetArrayElementAtIndex(i).stringValue = tags[i];
        data.ApplyModifiedPropertiesWithoutUndo(); return card;
    }
    /// <summary>Authors one explicit disposable primary ability rather than assigning illustrative effects to real species.</summary>
    private static void Effect(CardDefinition card, RigAbilityKind kind, float amount, float growth = 0,
        RigPositionCount count = RigPositionCount.Once, string tag = "", RigAbilityTrigger trigger = RigAbilityTrigger.Continuous)
    {
        var data = new SerializedObject(card); var ability = data.FindProperty("primaryAbility"); ability.FindPropertyRelative("name").stringValue = "Fixture ability";
        var effects = ability.FindPropertyRelative("effects"); effects.arraySize = 1; var effect = effects.GetArrayElementAtIndex(0);
        effect.FindPropertyRelative("kind").enumValueIndex = (int)kind; effect.FindPropertyRelative("trigger").enumValueIndex = (int)trigger;
        effect.FindPropertyRelative("accuracy").enumValueIndex = (int)LandingAccuracyFilter.Successful;
        effect.FindPropertyRelative("amount").floatValue = amount; effect.FindPropertyRelative("amountPerLevel").floatValue = growth;
        effect.FindPropertyRelative("count").enumValueIndex = (int)count; effect.FindPropertyRelative("tag").stringValue = tag;
        effect.FindPropertyRelative("perPosition").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
    }
    /// <summary>Measures actual resolved Perfect points with current upgrade/position snapshots.</summary>
    private static decimal Score(RigRuntime rig)
    {
        var profile = ScriptableObject.CreateInstance<LandingChallengeProfile>();
        try { return LandingScoring.Resolve(LandingAccuracy.Perfect, profile.CreateSettings(), rig.SnapshotForLanding(out _, out _)).Points; }
        finally { UnityEngine.Object.DestroyImmediate(profile); }
    }
    /// <summary>Fails immediately if a fixture acquisition is rejected.</summary>
    private static void Acquire(RigRuntime rig, CardDefinition card, int position) => Check(rig.TryAcquire(rig.Revision, card, position, out _, out _), "Fixture acquisition failed: " + card.DisplayName);
    /// <summary>Counts an independent model invariant.</summary>
    private static void Check(bool valid, string error) { if (!valid) throw new InvalidOperationException(error); checks++; }
}
