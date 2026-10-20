using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>Checks the approved eight-Technique set, starter deck, backup exclusion, and action boundaries.</summary>
public static class ApprovedTechniqueChecks
{
    private static int assertions;
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
#if UNITY_EDITOR
    /// <summary>Validates authored content and real runtime rules without mutating assets or the open scene.</summary>
    [MenuItem("Fishing Cards/Validation/Approved Techniques")]
    public static void Run() { Debug.Log("Approved Techniques: " + ValidateRules() + " runtime, " + ValidateAuthoredAssets() + " authored assertions passed."); }

    /// <summary>Checks all active definitions, preserved backups, scene starter counts, and removed legacy unlock.</summary>
    public static int ValidateAuthoredAssets()
    {
        assertions = 0;
        string[] names = { "Cut Loose", "Slip Past", "Clear Water", "Hold Steady", "Re-rig", "Steady Line", "Recast", "Prize Hook" };
        int[] copies = { 3, 2, 1, 1, 1, 2, 1, 1 };
        string scene = File.ReadAllText("Assets/Scenes/SampleScene.unity");
        string deck = scene.Split(new[] { "  startingTechniqueDeck:" }, StringSplitOptions.None)[1].Split(new[] { "  currentEncounterView:" }, StringSplitOptions.None)[0];
        for (int i = 0; i < names.Length; i++)
        {
            string path = "Assets/Cards/" + names[i] + ".asset";
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            Require(card != null && card.CardType == CardType.Technique && !card.IsBackupTechnique
                && card.Effects.Length == 1 && ApprovedTechniqueRuntime.Handles(card.Effects[0])
                && !string.IsNullOrEmpty(card.TechniqueRole), "Invalid active Technique: " + names[i]);
            string guid = AssetDatabase.AssetPathToGUID(path);
            Require(deck.Split(new[] { guid }, StringSplitOptions.None).Length - 1 == copies[i], "Wrong starter copies: " + names[i]);
        }
        int active = 0, backups = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (card.CardType != CardType.Technique) continue;
            if (card.IsBackupTechnique) { backups++; Require(path.StartsWith("Assets/Cards/Backup/Techniques/"), "Backup not archived."); }
            else active++;
        }
        Require(active == 8 && backups == 15 && deck.Split(new[] { "guid:" }, StringSplitOptions.None).Length - 1 == 12,
            "Active/backup/deck totals differ from the approved set.");
        Require(scene.Contains("startingHandSize: 4") && scene.Contains("unlockableTechniqueCard: {fileID: 0}"), "Legacy unlock or wrong starting hand remains.");
        return assertions;
    }
#endif
    /// <summary>Executes the production controller, target plans, effects, and lifecycle owners against controlled inputs.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ReleaseAndAvoidance(); ConcealmentAndHazards(); ModesAndPrizes(); Relief(); Recast(); Backup(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks granted release legality, deferred recovery, ordinary avoidance, and Apex/event rejection.</summary>
    private static void ReleaseAndAvoidance()
    {
        FishingRunController c = Controller(); CatchChainRuntime chain = Chain(c); EffectResolver resolver = Resolver(c);
        CardInstance victim = chain.Add(Creature(), resolver, CatchAttachmentRole.Bait);
        Require(!c.CanReleaseCatch(0) && !c.TryReleaseCatch(0), "Universal free Release remains enabled.");
        CardDefinition cut = Technique(CardEffectType.ReleaseAttachedCreature, CardEffectTarget.SpecificCaughtCard, 1);
        CardDefinition cod = Creature(); Set(cod, "baitEffects", new[] { Effect(CardEffectType.RecoverDiscardedTechnique, CardEffectTarget.None, 1, CardEffectTrigger.WhenAnotherCatchReleased) });
        Set(victim.Definition, "tags", new[] { "Armored" }); chain.Recalculate(resolver); chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        CardDefinition lionfish = Creature(); Set(lionfish, "value", 30);
        Set(lionfish, "effects", new[] { Effect(CardEffectType.LoseValueOnCreatureRelease, CardEffectTarget.Self, 5, CardEffectTrigger.WhenAnotherCatchReleased) });
        CardInstance lion = chain.Add(lionfish, resolver);
        bool reactionAfterDiscard = false;
        chain.DeliberatelyReleased += (released, reason) => reactionAfterDiscard = Deck(c).ResolvingCard == null
            && Array.IndexOf(Deck(c).DiscardPile, cut) >= 0 && lion.CurrentValue == 25;
        Set(Deck(c), "hand", new[] { cut });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(victim.InstanceId)
            && c.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique && Deck(c).DiscardPile[0] == cut,
            "Cut Loose did not defer Cod until its discard.");
        Require(reactionAfterDiscard, "Cut Loose resolved creature reactions before its discard.");
        Require(c.TryChooseRecoveredTechnique(0) && Deck(c).Hand[0] == cut && c.CurrentDepth == 0, "Cod could not recover Cut Loose.");
        CardDefinition crab = Creature(); Set(crab, "effects", new[] { Effect(CardEffectType.PreventRelease, CardEffectTarget.Self) });
        CardDefinition anemone = Creature(); Set(anemone, "effects", new[] { Effect(CardEffectType.PreventTechniqueTargeting, CardEffectTarget.Self) });
        c = Controller(); chain = Chain(c); chain.Add(crab, Resolver(c)); chain.Add(anemone, Resolver(c));
        Set(Deck(c), "hand", new[] { cut }); Require(!c.CanUseTechniqueCard(0, out _), "Cut Loose bypassed release/target locks.");
        CardDefinition slip = Technique(CardEffectType.SkipHookedCreature, CardEffectTarget.HookedEncounter);
        Set(Deck(c), "hand", new[] { slip });
        Encounters(c).GrantCreatureConcealment(Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTarget.FutureEncounters, 2), Creature());
        Encounters(c).SetCurrentEncounter(Creature());
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(TechniqueTargetPlan.EncounterTargetId)
            && c.HookedEncounter == null && c.CurrentEncounter == null && !Encounters(c).IsCreatureConcealed
            && Encounters(c).HasFutureConcealment && c.CurrentDepth == 0 && c.CanSurface, "Slip Past did not skip a concealed ordinary creature cleanly.");
        CardDefinition apex = Creature(); Set(apex, "cardType", CardType.ApexEncounter); Encounters(c).SetCurrentEncounter(apex);
        Set(Deck(c), "hand", new[] { slip }); Require(!c.CanUseTechniqueCard(0, out _), "Slip Past accepted Apex.");
        CardDefinition hazard = Creature(); Set(hazard, "cardType", CardType.Hazard); Encounters(c).SetCurrentEncounter(hazard);
        Require(!c.CanUseTechniqueCard(0, out _), "Slip Past accepted an event.");
    }

    /// <summary>Checks one-charge concealment removal and explicit Hazard cancellation/reward semantics.</summary>
    private static void ConcealmentAndHazards()
    {
        FishingRunController c = Controller(); CardDefinition clear = Technique(CardEffectType.ClearCreatureConcealment);
        Set(Deck(c), "hand", new[] { clear }); Require(!c.CanUseTechniqueCard(0, out _), "Clear Water accepted no concealment.");
        Encounters(c).GrantCreatureConcealment(Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTarget.FutureEncounters, 3), Creature());
        Encounters(c).SetCurrentEncounter(Creature());
        Require(c.TryUseTechniqueCard(0) && !c.CurrentEncounterIdentityHidden
            && Encounters(c).PendingConcealments[0].Remaining == 2 && c.HookedEncounter != null, "Clear Water consumed future concealment while revealing the current creature.");
        Set(Deck(c), "hand", new[] { clear });
        Require(c.TryUseTechniqueCard(0) && Encounters(c).PendingConcealments[0].Remaining == 1, "Clear Water removed an entire multi-charge source.");
        CardDefinition lobster = Creature(); Set(lobster, "effects", new[] { Effect(CardEffectType.GainValueOnHazardCancelled, CardEffectTarget.Self, 4, CardEffectTrigger.AfterHazardCancelled) });
        CardInstance caught = Chain(c).Add(lobster, Resolver(c));
        CardDefinition hazard = Creature(); Set(hazard, "cardType", CardType.Hazard);
        Set(hazard, "effects", new[] { Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTarget.None, 1, CardEffectTrigger.OnEventResolution) });
        Encounters(c).SetCurrentEncounter(hazard);
        CardDefinition hold = Technique(CardEffectType.CancelCurrentHazard, CardEffectTarget.CurrentEncounter);
        Set(Deck(c), "hand", new[] { hold });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(TechniqueTargetPlan.EncounterTargetId)
            && Encounters(c).CurrentEventOccurrence.State == EventOccurrenceState.Cancelled && caught.CurrentValue == 12
            && c.CurrentDepth == 0 && c.CanDescend && c.CanSurface, "Hold Steady did not cancel/reward without advancing.");
        Set(Deck(c), "hand", new[] { hold }); Require(!c.CanUseTechniqueCard(0, out _), "Hold Steady replayed on cancelled Hazard.");
        c = Controller(); Encounters(c).SetCurrentEncounter(hazard); Set(Deck(c), "hand", new[] { clear });
        Require(!c.CanUseTechniqueCard(0, out _), "Clear Water removed ungranted unresolved event concealment.");
        Set(hazard, "cardType", CardType.Environment); Encounters(c).SetCurrentEncounter(hazard);
        Set(Deck(c), "hand", new[] { hold }); Require(!c.CanUseTechniqueCard(0, out _), "Hold Steady accepted Environment.");
    }

    /// <summary>Checks in-place mode entry, own triggers, preserved modifiers, and stacked Prize Hooks.</summary>
    private static void ModesAndPrizes()
    {
        FishingRunController c = Controller(); CardDefinition prize = Technique(CardEffectType.QueuePrizeCatchBonus, CardEffectTarget.None, 8);
        Set(Deck(c), "hand", new[] { prize, prize }); Require(c.TryUseTechniqueCard(0) && c.TryUseTechniqueCard(0), "Prize Hook could not stack.");
        CardDefinition crab = Creature(); Set(crab, "effects", new[] { Effect(CardEffectType.PreventRelease, CardEffectTarget.Self) });
        CardInstance target = Chain(c).Add(crab, Resolver(c), CatchAttachmentRole.Bait);
        Require(target.CurrentWeight == 1 && Effects(c).Approved.PendingPrizeCount == 2, "Bait consumed Prize Hook.");
        CardInstance preview = Chain(c).CreateResolvedCatchPreview(crab, Resolver(c), CatchAttachmentRole.Catch, Effects(c).Approved.PreviewPendingPrizes);
        Require(preview.CurrentWeight == 3 && preview.CurrentValue == 24 && Effects(c).Approved.PendingPrizeCount == 2,
            "Prize Hook preview missed modifiers or consumed grants.");
        CardDefinition rig = Technique(CardEffectType.SwitchCreatureMode, CardEffectTarget.SpecificCaughtCard);
        Set(Deck(c), "hand", new[] { rig });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(target.InstanceId) && !target.IsBait
            && target.CurrentWeight == 3 && target.CurrentValue == 24 && Effects(c).Approved.PendingPrizeCount == 0,
            "Catch mode entry did not award stacked Prize Hooks before its own effects.");
        Set(Deck(c), "hand", new[] { rig });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(target.InstanceId) && target.IsBait
            && target.CurrentWeight == 3 && target.HaulValueContribution == 0 && !target.PreventsReleaseWhileAttached
            && Chain(c).Catches[0] == target && Chain(c).ReleasedCatches.Length == 0, "Re-rig lost modifiers/position or counted as Release.");
        CardDefinition anemone = Creature(); Set(anemone, "effects", new[] { Effect(CardEffectType.PreventTechniqueTargeting, CardEffectTarget.Self) });
        CardInstance blocked = Chain(c).Add(anemone, Resolver(c));
        Set(Deck(c), "hand", new[] { rig }); Require(c.TryUseTechniqueCard(0) && !c.TryChooseTechniqueTarget(blocked.InstanceId), "Re-rig targeted immune Anemone."); c.TryCancelTechniqueSelection();
        CardDefinition squid = Creature(); Set(squid, "effects", new[] { Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTarget.FutureEncounters, 2, CardEffectTrigger.WhenCaught) });
        CardInstance squidBait = Chain(c).Add(squid, Resolver(c), CatchAttachmentRole.Bait); Set(Deck(c), "hand", new[] { rig });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(squidBait.InstanceId)
            && Encounters(c).PendingConcealments[0].Remaining == 2 && Deck(c).DiscardPile.Length > 0,
            "Re-rig did not activate the new mode's own on-attach effect after discard.");
        Set(Deck(c), "hand", new[] { prize }); c.TryUseTechniqueCard(0);
        CardDefinition apex = Creature(); Set(apex, "cardType", CardType.ApexEncounter);
        CardInstance apexCatch = Chain(c).Add(apex, Resolver(c));
        Require(apexCatch.CurrentWeight == 2 && apexCatch.CurrentValue == 16, "Prize Hook excluded Apex Catch.");
    }

    /// <summary>Checks pending anti-stacking, action-time checks, event-before-Surface timing, and expiry before reveal.</summary>
    private static void Relief()
    {
        FishingRunController c = Controller(); CardDefinition heavy = Creature(); Set(heavy, "weight", 3);
        Chain(c).Add(heavy, Resolver(c)); CardDefinition steady = Technique(CardEffectType.GrantActionLoadRelief, CardEffectTarget.None, 2);
        Set(Deck(c), "hand", new[] { steady, steady });
        Require(c.TryUseTechniqueCard(0) && c.CurrentLineLoad == 3 && !c.CanUseTechniqueCard(0, out _), "Steady Line applied early or stacked.");
        Set(c, "lineCapacity", 3);
        int loadAtReveal = -1; Encounters(c).CreatureRevealed += (card, revision) => loadAtReveal = c.CurrentLineLoad;
        Require(c.TryChooseEncounterRole(EncounterEffectChoice.Catch), "Fixture must explicitly choose Catch before Descend.");
        Require(c.TryDescend() && loadAtReveal == 4 && c.CurrentLineLoad == 4 && !Effects(c).Approved.HasPendingRelief
            && Get<LineLoadRiskRuntime>(c, "lineLoadRiskRuntime").LastOutcome == LineLoadRiskOutcome.NotOverloaded && c.LineCapacity == 3,
            "Steady Line persisted into the next encounter.");
        c = Controller(); Set(c, "lineCapacity", 2); Chain(c).Add(heavy, Resolver(c)); Set(Deck(c), "hand", new[] { steady }); c.TryUseTechniqueCard(0);
        CardDefinition snag = Creature(); Set(snag, "cardType", CardType.Hazard);
        CardEffectDefinition snagEffect = Effect(CardEffectType.SnagHeaviestUnarmoredCreature, CardEffectTarget.SpecificCaughtCard, 1, CardEffectTrigger.OnEventResolution);
        Set(snagEffect, "effectTone", CardEffectTone.Negative); Set(snagEffect, "hasEventLifetime", true);
        Set(snagEffect, "eventLifetime", EventEffectLifetime.AtBoundary(EventEffectLifetime.BiomeEnd, 1));
        Set(snag, "effects", new[] { snagEffect });
        Encounters(c).SetCurrentEncounter(snag);
        Require(c.TrySurface() && c.LastSurfaceLineLoad == 2 && !c.LastSurfaceWasOverloaded && !c.RunActive,
            "Steady Line did not include event consequences in Surface load checks.");
        Require(!Effects(c).Approved.HasPendingRelief && c.CurrentLineLoad == 0, "Run cleanup retained load relief.");
        CatchChainRuntime empty = new CatchChainRuntime(); empty.SetActionLoadReduction(2);
        Require(empty.CurrentLineLoad == 0, "Relief made load negative.");
    }

    /// <summary>Checks one/two other-hand selections, exact duplicate slots, stale input, reshuffle, and resolving exclusion.</summary>
    private static void Recast()
    {
        FishingRunController c = Controller(); CardDefinition recast = Technique(CardEffectType.FilterTechniqueHand, CardEffectTarget.None, 2);
        CardDefinition other = Technique(CardEffectType.QueuePrizeCatchBonus, CardEffectTarget.None, 8);
        Set(Deck(c), "hand", new[] { recast }); Require(!c.CanUseTechniqueCard(0, out _), "Recast accepted no other card.");
        Set(Deck(c), "hand", new[] { recast, recast, other });
        TechniqueTargetPlan wrongCopy = new TechniqueTargetPlan(recast, new[] { new[] { TechniqueTargetPlan.HandTargetId(0) } }, Encounters(c), Deck(c), 1);
        Require(!c.TryUseTechniqueCardWithTargets(0, wrongCopy) && Deck(c).Hand.Length == 3,
            "Recast committed a different identical physical copy from its preselected played slot.");
        Set(Deck(c), "hand", new[] { other, recast, other, other });
        Require(c.TryUseTechniqueCard(1) && !c.TryChooseTechniqueTarget(TechniqueTargetPlan.HandTargetId(1))
            && c.TryChooseTechniqueTarget(TechniqueTargetPlan.HandTargetId(0)) && !c.TryChooseTechniqueTarget(TechniqueTargetPlan.HandTargetId(0))
            && c.TryFinishTechniqueTargets() && !c.HasPendingTechniqueSelection, "Recast could not choose one distinct other card.");
        c = Controller(); Set(Deck(c), "hand", new[] { other, recast, other, other });
        TechniqueTargetPlan plan = new TechniqueTargetPlan(recast, new[] { new[] { TechniqueTargetPlan.HandTargetId(0), TechniqueTargetPlan.HandTargetId(3) } }, Encounters(c), Deck(c), 1);
        Require(plan.Validate(recast, Chain(c), out _, Encounters(c), Deck(c)) && Deck(c).TryBeginPlay(1, out _, out _), "Recast precommit plan failed.");
        Require(Effects(c).ApplyCard(recast, Encounters(c), Chain(c), Resolver(c), Array.Empty<CardDefinition>(), null,
            "Coastal", 0, new System.Random(1), false, out _, plan, Deck(c), Get<CreatureEffectRuntime>(c, "creatureEffectRuntime"))
            && Deck(c).ResolvingCard == recast && Deck(c).Hand.Length == 3 && Deck(c).DiscardPile.Length == 0
            && Array.TrueForAll(Deck(c).Hand, card => card == other), "Recast reshuffled itself early or changed hand count.");
        Require(Deck(c).TryCompletePlay(Deck(c).ResolvingOccurrenceId) && Deck(c).DiscardPile[0] == recast, "Recast did not discard after draws.");
        c = Controller(); Set(Deck(c), "hand", new[] { recast, other }); c.TryUseTechniqueCard(0);
        Deck(c).TryDiscardTechnique(1);
        Require(!c.TryChooseTechniqueTarget(TechniqueTargetPlan.HandTargetId(1)) && !c.HasPendingTechniqueSelection
            && Deck(c).Hand.Length == 1, "Stale Recast input spent the played card.");
    }

    /// <summary>Checks archived cards cannot enter decks, be unlocked, or be played through backend APIs.</summary>
    private static void Backup()
    {
        CardDefinition old = Technique(CardEffectType.ModifyDescendDistance); Set(old, "backupTechnique", true);
        CardDefinition active = Technique(CardEffectType.QueuePrizeCatchBonus, CardEffectTarget.None, 8);
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime(); deck.Initialize(new[] { old, active }, 4, new System.Random(1), null);
        Require(deck.Hand.Length == 1 && deck.Hand[0] == active, "Backup entered deck initialization.");
        RunProgressionRuntime progression = new RunProgressionRuntime(); Set(progression, "savedTechniqueDeck", new[] { old }); Set(progression, "techniqueDeckInitialized", true);
        progression.InitializeTechniqueDeck(new[] { active, active });
        Require(progression.SavedTechniqueDeck.Length == 2 && progression.SavedTechniqueDeck[0] == active, "Existing legacy session deck did not migrate.");
        FishingRunController c = Controller(); Set(Deck(c), "hand", new[] { old });
        Require(!c.CanUseTechniqueCard(0, out _) && !c.TryUseTechniqueCard(0), "Backup was playable.");
    }

    /// <summary>Creates an isolated real controller with deterministic broad-capacity encounter data.</summary>
    private static FishingRunController Controller()
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome); Set(biome, "biomeId", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition(); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { Creature() }); Set(biome, "depthTiers", new[] { tier });
        GameObject obj = new GameObject("Approved Technique checks"); created.Add(obj); FishingRunController c = obj.AddComponent<FishingRunController>();
        Set(c, "startRunOnAwake", false); Set(c, "startingBiome", biome); Set(c, "startingLineCapacity", 100); Set(c, "startingHandSize", 0); c.StartRun(); return c;
    }
    /// <summary>Creates a creature with explicitly empty Bait effects.</summary>
    private static CardDefinition Creature()
    { CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card); Set(card, "cardType", CardType.Creature); Set(card, "weight", 1); Set(card, "value", 8); Set(card, "hasExplicitBaitData", true); return card; }
    /// <summary>Creates one approved authored-effect input.</summary>
    private static CardDefinition Technique(CardEffectType type, CardEffectTarget target = CardEffectTarget.None, int amount = 0)
    { CardDefinition card = Creature(); Set(card, "cardType", CardType.Technique); Set(card, "effects", new[] { Effect(type, target, amount, CardEffectTrigger.Manual) }); return card; }
    /// <summary>Creates an effect fixture using serialized data fields.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTarget target, int amount = 0, CardEffectTrigger trigger = CardEffectTrigger.WhileAttached)
    { CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "target", target); Set(effect, "amount", amount); Set(effect, "trigger", trigger); return effect; }
    /// <summary>Reads the controller's chain owner.</summary>
    private static CatchChainRuntime Chain(FishingRunController c) { return Get<CatchChainRuntime>(c, "catchChainRuntime"); }
    /// <summary>Reads the controller's deck owner.</summary>
    private static TechniqueDeckRuntime Deck(FishingRunController c) { return Get<TechniqueDeckRuntime>(c, "techniqueDeckRuntime"); }
    /// <summary>Reads the controller's encounter owner.</summary>
    private static EncounterRuntime Encounters(FishingRunController c) { return Get<EncounterRuntime>(c, "encounterRuntime"); }
    /// <summary>Reads the controller's Technique effect owner.</summary>
    private static TechniqueEffectRuntime Effects(FishingRunController c) { return Get<TechniqueEffectRuntime>(c, "techniqueEffectRuntime"); }
    /// <summary>Reads the controller's stat resolver.</summary>
    private static EffectResolver Resolver(FishingRunController c) { return Get<EffectResolver>(c, "effectResolver"); }
    /// <summary>Sets a serialized fixture field without changing production definitions.</summary>
    private static void Set(object obj, string field, object value) { obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value); }
    /// <summary>Reads a runtime field for integration assertions.</summary>
    private static T Get<T>(object obj, string field) { return (T)obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj); }
    /// <summary>Counts each meaningful invariant and reports its first failure.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
