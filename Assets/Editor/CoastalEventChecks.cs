using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Validates the five approved coastal events through production dispatch and action owners.</summary>
public static class CoastalEventChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs focused coastal event rules without changing production assets.</summary>
    [MenuItem("Fishing Cards/Validation/Coastal Events")]
    public static void Run() { int rules = ValidateRules(); Debug.Log("Coastal events: " + rules + " rule and " + ValidateAuthoredAssets() + " authored-asset assertions passed."); }
    /// <summary>Checks authored card identity, approved effects, and actual coastal membership through Unity serialization.</summary>
    public static int ValidateAuthoredAssets()
    {
        assertions = 0;
        string[] names = { "Reef Snag", "Silt Burst", "Gentle Upwelling", "Loose Knot", "Fresh Shell Fragments" };
        string[] ids = { "reef_snag", "silt_burst", "gentle_upwelling", "loose_knot", "fresh_shell_fragments" };
        int[] minimum = { 3, 4, 1, 3, 5 }, maximum = { 18, 17, 15, 18, 13 };
        CardType[] types = { CardType.Hazard, CardType.Hazard, CardType.Environment, CardType.Opportunity, CardType.Opportunity };
        CardRarity[] rarity = { CardRarity.Common, CardRarity.Common, CardRarity.Uncommon, CardRarity.Uncommon, CardRarity.Rare };
        CardEffectType[] effects = { CardEffectType.SnagHeaviestUnarmoredCreature, CardEffectType.QueueCreatureEncounterConcealment,
            CardEffectType.QueueBaitWeightReduction, CardEffectType.OptionalEventCreatureRelease, CardEffectType.ScheduleCreatureTrail };
        BiomeDefinition biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/Biomes/Coastal Waters.asset");
        Require(biome != null, "Coastal biome asset is missing.");
        for (int i = 0; i < names.Length; i++)
        {
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/" + names[i] + ".asset");
            Require(card != null && card.UniqueId == ids[i] && card.DisplayName == names[i]
                && card.CardType == types[i] && card.Rarity == rarity[i], "Authored coastal event identity differs: " + names[i]);
            Require(card.MinimumDepth == minimum[i] && card.MaximumDepth == maximum[i] && card.Weight == 0 && card.Value == 0,
                "Authored event has incorrect depth or catch stats: " + names[i]);
            Require(card.Effects.Length == 1 && card.Effects[0].EffectType == effects[i]
                && card.Effects[0].Trigger == CardEffectTrigger.OnEventResolution, "Authored event effect differs: " + names[i]);
            foreach (BiomeDepthTierDefinition tier in biome.DepthTiers)
            {
                bool overlaps = card.MinimumDepth <= tier.MaximumDepth && card.MaximumDepth >= tier.MinimumDepth;
                Require(Array.Exists(tier.EncounterPool, c => c == card) == overlaps, "Event pool placement differs: " + names[i]);
            }
        }
        CardDefinition trail = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Fresh Shell Fragments.asset");
        Require(trail.Effects[0].ReplacementCard == AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Spiny Lobster.asset"),
            "Trail does not reference the existing Spiny Lobster definition.");
        return assertions;
    }
#endif
    /// <summary>Exercises target choices, persistence, concealment, Release reactions, and promised encounters.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { Snag(); Upwelling(); Silt(); LooseKnot(); Trails(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks post-Technique selection, mandatory ties, stacking, immunity, cleansing, and biome/Surface timing.</summary>
    private static void Snag()
    {
        CardDefinition source = Event(CardEffectType.SnagHeaviestUnarmoredCreature);
        FishingRunController c = Controller(source); CatchChainRuntime chain = Chain(c); EffectResolver r = Resolver(c);
        CardInstance first = chain.Add(Card(2), r), second = chain.Add(Card(2), r, CatchAttachmentRole.Bait);
        CardDefinition armored = Card(5); Set(armored, "tags", new[] { "Armored" });
        CardInstance shell = chain.Add(armored, r);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.PendingCreatureChoice == CreatureEffectChoiceKind.EventSnagTarget
            && c.CurrentDepth == 0 && c.HasConfirmedEventAction, "Snag failed to pause at a heaviest tie.");
        Require(!c.TryChooseEventTarget(shell.InstanceId) && !c.TryChooseEventTarget(-1)
            && !c.TrySurface() && !c.CanDescend, "Snag tie allowed an invalid target or action.");
        Require(c.TryChooseEventTarget(second.InstanceId) && second.CurrentWeight == 3 && first.CurrentWeight == 2
            && shell.CurrentWeight == 5 && c.CurrentDepth == 1, "Snag did not resolve its selected Bait before Descend.");
        chain.TrySetCreatureBaseTags(second.InstanceId, new[] { "Armored" }, r);
        Require(second.CurrentWeight == 3, "Later Armored incorrectly removed a Snag.");
        Require(Store(c).GetNegativeOptions().Length == 1 && Store(c).GetNegativeOptions()[0].TryRemove()
            && second.CurrentWeight == 2, "Quick Clean failed exact Snag removal.");
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && first.CurrentWeight == 3, "Snag targeted heavier Armored instead of eligible creature.");
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && first.CurrentWeight == 4, "Repeated Snags failed to stack.");
        Invoke(c, "EndBiomeEventEffects");
        Require(first.CurrentWeight == 2 && Store(c).Effects.Length == 0, "Biome ending failed to expire Snags.");
        c.StartRun(); chain = Chain(c); first = chain.Add(Card(3), Resolver(c));
        Require(c.TrySurface() && Get<FishingRunResult>(c, "lastSurfaceResult").SurfaceLineLoad == 4
            && first.CurrentWeight == 3, "Surface load checked after Snag expiry.");
        c = Controller(source); chain = Chain(c); chain.Add(armored, Resolver(c));
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && Store(c).Effects.Length == 0 && !c.HasPendingCreatureChoice,
            "All-Armored chain did not resolve Snag without effect.");
        c = Controller(source); chain = Chain(c); first = chain.Add(Card(1), Resolver(c)); second = chain.Add(Card(2), Resolver(c));
        first.AddPermanentModifiers(3, 0); chain.Recalculate(Resolver(c));
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && first.CurrentWeight == 5 && second.CurrentWeight == 2,
            "Snag selected from base or reveal-time weight instead of current effective weight.");
        c = Controller(source); chain = Chain(c); r = Resolver(c);
        CardDefinition school = Card(0); Set(school, "tags", new[] { "Schooling", "Armored" });
        chain.Add(school, r); chain.Add(school, r);
        CardDefinition mackerel = Card(1); Set(mackerel, "tags", new[] { "Schooling" });
        CardEffectDefinition packed = Effect(CardEffectType.SetSelfWeightZeroWithOtherCreatureCatches, CardEffectTrigger.WhileAttached, 2);
        Set(packed, "requiredTags", new[] { "Schooling" }); Set(mackerel, "effects", new[] { packed });
        first = chain.Add(mackerel, r);
        Require(first.CurrentWeight == 0 && FishingRunCheckActions.TryDescendAsCatch(c) && first.CurrentWeight == 1,
            "Snag did not add to the target's current effective zero weight.");
    }

    /// <summary>Checks shared pending grants, pre-on-attach application, floor, mode-entry qualification, and removal.</summary>
    private static void Upwelling()
    {
        FishingRunController c = Controller(Event(CardEffectType.QueueBaitWeightReduction));
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && FishingRunCheckActions.TryDescendAsCatch(c) && Store(c).Effects.Length == 2, "Upwellings failed to queue independently.");
        CatchChainRuntime chain = Chain(c); EffectResolver r = Resolver(c);
        CardDefinition predator = Card(2); Set(predator, "tags", new[] { "Predator" });
        CardInstance caught = chain.Add(predator, r);
        Require(Array.TrueForAll(Store(c).Effects, e => e.IsPending), "Catch consumed a Bait grant.");
        bool observed = false;
        chain.BeforeAttachmentEffects += card => { if (card.IsBait) observed = card.CurrentWeight == 0; };
        CardDefinition urchin = Card(1); Set(urchin, "hasExplicitBaitData", true);
        CardEffectDefinition spines = Effect(CardEffectType.IncreaseSelfWeightWithMatchingAttachedCreature, CardEffectTrigger.WhileAttached, 1);
        Set(spines, "requiredTags", new[] { "Predator" }); Set(urchin, "effects", new[] { spines });
        CardInstance preview = chain.CreateResolvedCatchPreview(urchin, r, CatchAttachmentRole.Bait);
        Store(c).PreviewPendingBaitWeightReductions(preview);
        Require(preview.CurrentWeight == 0 && Array.TrueForAll(Store(c).Effects, e => e.IsPending), "Bait preview consumed grants or missed reductions.");
        CardInstance bait = chain.Add(urchin, r, CatchAttachmentRole.Bait);
        Require(observed && bait.CurrentWeight == 0 && Array.TrueForAll(Store(c).Effects, e => e.TargetInstanceId == bait.InstanceId),
            "Stacked Upwellings missed Bait entry or consumed grants on different targets.");
        Require(chain.TrySwapCreatureModes(caught.InstanceId, bait.InstanceId, r), "Fixture mode swap failed.");
        chain.ActivateRoleOnAttach(caught, r); chain.ActivateRoleOnAttach(bait, r);
        Require(bait.CurrentWeight == 0 && !bait.IsBait, "Upwelling modifier disappeared on entering Catch.");
        Require(FishingRunCheckActions.TryDescendAsCatch(c), "Next Upwelling did not resolve.");
        Invoke(c, "EndBiomeEventEffects");
        Require(Array.Exists(Store(c).Effects, e => e.IsPending), "Biome ending expired pending Upwelling.");
        Require(chain.TrySwapCreatureModes(bait.InstanceId, caught.InstanceId, r), "Second fixture swap failed.");
        Require(!Array.Exists(Store(c).Effects, e => e.IsPending), "Bait mode entry delayed its grant until a later on-attach callback.");
        chain.ActivateRoleOnAttach(bait, r); chain.ActivateRoleOnAttach(caught, r);
        Require(bait.CurrentWeight == 0 && !Array.Exists(Store(c).Effects, e => e.IsPending), "Mode entry failed to consume all pending grants.");
        Require(chain.TryRelease(Array.IndexOf(chain.Catches, bait), r, out _, out _, out _)
            && Store(c).Effects.Length == 0, "Removed recipient retained tracked reductions.");
    }

    /// <summary>Checks shared FIFO concealment, exclusions, removal, independent expiry, and Hazard protection.</summary>
    private static void Silt()
    {
        CardDefinition source = Event(CardEffectType.QueueCreatureEncounterConcealment);
        FishingRunController c = Controller(source); EncounterRuntime e = Encounters(c);
        CardEffectDefinition ink = Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTrigger.WhenCaught, 2);
        e.GrantCreatureConcealment(ink, Card(2));
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && e.PendingConcealments.Length == 2, "Silt did not append after existing Ink.");
        CardDefinition apex = Card(3); Set(apex, "cardType", CardType.ApexEncounter);
        e.SetCurrentEncounter(apex);
        Require(!e.IsCreatureConcealed && e.PendingConcealments[0].Remaining == 2, "Apex consumed Silt or Squid concealment.");
        e.SetCurrentEncounter(Card(1));
        Require(e.IsCreatureConcealed && e.PendingConcealments[0].Remaining == 1
            && e.PendingConcealments[1].Remaining == 1, "One reveal consumed multiple concealment sources.");
        e.SetCurrentEncounter(null); e.Reset(true); e.SetCurrentEncounter(Card(1));
        Require(e.IsCreatureConcealed && e.PendingConcealments[1].Remaining == 1, "Biome reset lost queued Silt.");
        e.TakeHookedEncounter(); e.SetCurrentEncounter(Card(1));
        Require(e.IsCreatureConcealed && e.PendingConcealments.Length == 1, "Silt did not follow the exhausted Squid grant.");
        e.SetCurrentEncounter(null);
        Require(e.PendingConcealments.Length == 0, "Removal without attachment did not consume Silt.");
        c = Controller(source); CatchChainRuntime chain = Chain(c);
        CardDefinition mussel = Card(1); Set(mussel, "hasExplicitBaitData", true);
        Set(mussel, "baitEffects", new[] { Effect(CardEffectType.CancelNextHazard, CardEffectTrigger.WhenCaught, 1) });
        CardInstance protection = chain.Add(mussel, Resolver(c), CatchAttachmentRole.Bait);
        FishingRunCheckActions.TryDescendAsCatch(c);
        Require(Encounters(c).CurrentEventOccurrence.State == EventOccurrenceState.Cancelled
            && Encounters(c).PendingConcealments.Length == 1, "Mussel did not cancel the next Silt reveal.");
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && Encounters(c).PendingConcealments.Length == 1, "Cancelled Silt still added concealment.");
        CardDefinition squid = Card(2); Set(squid, "hasExplicitBaitData", true);
        Set(squid, "baitEffects", new[] { Effect(CardEffectType.GrantNextCreatureSkip, CardEffectTrigger.WhenCaught, 1) });
        CreatureEffectRuntime creatures = Get<CreatureEffectRuntime>(c, "creatureEffectRuntime");
        creatures.RecordAttachment(chain.Add(squid, Resolver(c), CatchAttachmentRole.Bait));
        Encounters(c).SetCurrentEncounter(Card(1)); Invoke(c, "PrepareCreatureChoice");
        Require(c.CurrentEncounterIdentityHidden && c.PendingCreatureChoice == CreatureEffectChoiceKind.EncounterSkip,
            "Squid skip exposed the Silt-concealed creature.");
        Require(c.TryChooseEncounterSkip(true) && creatures.PendingCreatureSkipCount == 0
            && Encounters(c).PendingConcealments.Length == 0 && c.CurrentEncounter == null,
            "Squid skip failed to consume both its grant and assigned Silt.");
    }

    /// <summary>Checks event Release legality, optional decline, cod/Ray/Eel/Lionfish reactions, and Technique separation.</summary>
    private static void LooseKnot()
    {
        CardDefinition source = Event(CardEffectType.OptionalEventCreatureRelease);
        FishingRunController c = Controller(source); CatchChainRuntime chain = Chain(c); EffectResolver r = Resolver(c);
        CardDefinition lockCard = Card(2); Set(lockCard, "effects", new[] { Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, 0) });
        CardInstance crab = chain.Add(lockCard, r);
        CardDefinition anemone = Card(1); Set(anemone, "effects", new[] { Effect(CardEffectType.PreventTechniqueTargeting, CardEffectTrigger.WhileAttached, 0) });
        CardInstance target = chain.Add(anemone, r);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.PendingCreatureChoice == CreatureEffectChoiceKind.EventCreatureRelease
            && !c.TryChooseEventTarget(crab.InstanceId), "Loose Knot bypassed Iron Grip.");
        Require(c.TryChooseEventTarget(target.InstanceId) && chain.ReleasedCatches.Length == 1 && c.CurrentDepth == 1,
            "Loose Knot incorrectly applied Technique targeting restrictions.");
        chain.Add(Card(1), r);
        Require(c.TrySurface() && c.TryChooseEventTarget(-1) && !c.RunActive, "Loose Knot decline did not continue Surface.");
        c = Controller(source); chain = Chain(c); r = Resolver(c);
        CardDefinition lion = Card(2); Set(lion, "value", 30);
        Set(lion, "effects", new[] { Effect(CardEffectType.LoseValueOnCreatureRelease, CardEffectTrigger.WhenAnotherCatchReleased, 5) });
        CardInstance lionCatch = chain.Add(lion, r);
        CardDefinition prey = Card(1); Set(prey, "tags", new[] { "Armored", "Elusive" }); target = chain.Add(prey, r);
        AddBait(chain, r, CardEffectType.RecoverDiscardedTechnique, new[] { "Armored" });
        AddBait(chain, r, CardEffectType.TransferReleasedCatchValue, new[] { "Elusive" });
        AddBait(chain, r, CardEffectType.DrawTechniqueAfterCreatureRelease, new[] { "Elusive" });
        CreatureEffectRuntime creatures = Get<CreatureEffectRuntime>(c, "creatureEffectRuntime");
        CardDefinition technique = Card(0); Set(technique, "cardType", CardType.Technique);
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(c, "techniqueDeckRuntime");
        Set(deck, "discardPile", new[] { technique }); Set(deck, "hand", new[] { technique, technique, technique });
        Set(deck, "drawPile", new[] { technique });
        CardDefinition urchin = Card(1); Set(urchin, "hasExplicitBaitData", true);
        Set(urchin, "baitEffects", new[] { Effect(CardEffectType.GrantSelectedTechniqueRecovery, CardEffectTrigger.WhenCaught, 1) });
        creatures.RecordAttachment(chain.Add(urchin, r, CatchAttachmentRole.Bait));
        Require(c.TrySurface() && c.TryChooseEventTarget(target.InstanceId)
            && c.HasConfirmedEventAction && c.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique
            && lionCatch.CurrentValue == 25, "Event Release missed Lionfish penalty or Cod recovery ordering.");
        Require(c.TryChooseRecoveredTechnique(0) && c.PendingCreatureChoice == CreatureEffectChoiceKind.TransferValue,
            "Cod recovery did not finish before Ray transfer.");
        Require(c.TryChooseCreatureValueTransfer(lionCatch.InstanceId) && !c.RunActive
            && creatures.PendingSelectedTechniqueRecoveryCount == 0
            && Get<FishingRunResult>(c, "lastSurfaceResult").HaulValue == 33, "Ray transfer/Eel draw did not finish Surface.");
        c = Controller(source);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && !c.HasPendingCreatureChoice, "No-target Loose Knot did not complete.");
    }

    /// <summary>Checks legal slots, duplicate guards, queued priority, Technique replacement, skipped depths, and cleanup.</summary>
    private static void Trails()
    {
        CardDefinition lobster = Card(3); Set(lobster, "displayName", "Spiny Lobster"); Set(lobster, "minimumDepth", 8); Set(lobster, "maximumDepth", 20);
        CardDefinition source = Event(CardEffectType.ScheduleCreatureTrail); Set(source.Effects[0], "replacementCard", lobster);
        CardDefinition fish = Card(1); Set(fish, "tags", new[] { "Schooling" });
        FishingRunController c = Controller(source, 5, lobster, fish);
        Encounters(c).SetCurrentEncounter(source);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && Encounters(c).Trail.HasPending && Encounters(c).Trail.DestinationDepth == 8,
            "Fresh Shell Fragments did not schedule depth +3.");
        Require(Encounters(c).Trail.Preview.Contains("Spiny Lobster") && Encounters(c).Trail.Preview.Contains("Weight 3"),
            "Trail preview omitted learned identity/stats.");
        Encounters(c).SetCurrentEncounter(source);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && Encounters(c).Trail.DestinationDepth == 8, "Forced duplicate trail rescheduled a promise.");
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.CurrentDepth == 8 && c.CurrentEncounter == lobster && !Encounters(c).Trail.HasPending,
            "Destination did not consume the normal draw exactly once.");
        Require(Encounters(c).TryReplaceEncounter(Encounters(c).EncounterRevision, lobster, new[] { fish }, "Coastal", 8,
            new System.Random(1), Chain(c).ActiveEffectRecords, Resolver(c)) && Encounters(c).Trail.Preview.Contains("replaced"),
            "Technique replacement failed to mark the consumed promise.");
        Require(c.TrySurface() && !Encounters(c).Trail.HasPending && Encounters(c).Trail.Preview == string.Empty,
            "Surface retained trail presentation or scheduling.");
        c = Controller(source, 5, lobster, fish); Encounters(c).SetCurrentEncounter(source); FishingRunCheckActions.TryDescendAsCatch(c); FishingRunCheckActions.TryDescendAsCatch(c);
        Set(Get<CreatureEffectRuntime>(c, "creatureEffectRuntime"), "queuedEncounter", fish);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.CurrentEncounter == fish && Encounters(c).Trail.Preview.Contains("replaced")
            && !Encounters(c).Trail.HasPending, "Queued School Call failed to consume and replace the trail slot.");
        c = Controller(source, 5, lobster, fish); Encounters(c).SetCurrentEncounter(source); Set(c, "depthStepPerDescend", 4);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.CurrentDepth == 9 && c.CurrentEncounter == lobster
            && Encounters(c).Trail.Preview.Contains("depth 9"), "Passed destination was not retargeted to the first subsequent slot.");
        c = Controller(source, 5, lobster, fish); Encounters(c).SetCurrentEncounter(source); FishingRunCheckActions.TryDescendAsCatch(c); FishingRunCheckActions.TryDescendAsCatch(c);
        CardDefinition silt = Event(CardEffectType.QueueCreatureEncounterConcealment);
        Encounters(c).GrantCreatureConcealment(silt.Effects[0], silt);
        Require(FishingRunCheckActions.TryDescendAsCatch(c) && c.CurrentEncounterIdentityHidden && Encounters(c).Trail.Preview.Contains("Spiny Lobster")
            && Encounters(c).Trail.Preview.Contains("Weight 3"), "Silt erased knowledge already learned from the trail preview.");
        CardDefinition avoid = Card(0); Set(avoid, "cardType", CardType.Technique);
        CardEffectDefinition skip = Effect(CardEffectType.AvoidEncounter, CardEffectTrigger.Manual, 1);
        Set(skip, "target", CardEffectTarget.HookedEncounter); Set(avoid, "effects", new[] { skip });
        Set(Get<TechniqueDeckRuntime>(c, "techniqueDeckRuntime"), "hand", new[] { avoid });
        Require(TechniqueRuleChecks.Play(c, 0) && c.CurrentEncounter == null && Encounters(c).Trail.Preview.Contains("replaced")
            && !Encounters(c).Trail.HasPending && Encounters(c).PendingConcealments.Length == 0,
            "Avoiding the promised concealed Lobster failed to consume trail and concealment.");
        EventEncounterTrailRuntime trail = new EventEncounterTrailRuntime(); BiomeDefinition biome = c.CurrentBiome;
        Require(!trail.TrySchedule(biome, 18, lobster, 3) && !trail.HasPending, "Trail scheduled over the Apex boundary.");
        Require(trail.TrySchedule(biome, 17, lobster, 3) && trail.TakeForSlot(biome, 21, 21, false) == null
            && !trail.HasPending, "No legal remaining slot retained an unusable trail.");
        trail.TrySchedule(biome, 5, lobster, 3);
        Require(!trail.TrySchedule(biome, 5, lobster, 3), "Duplicate pending trail was accepted.");
        Require(trail.TakeForSlot(null, 8, 8, false) == null && !trail.HasPending, "Trail carried into another biome.");
        BiomeDepthTierDefinition unlimited = new BiomeDepthTierDefinition(); Set(unlimited, "encounterPool", new[] { lobster });
        Set(biome, "depthTiers", new[] { unlimited });
        Require(trail.TrySchedule(biome, 17, lobster, 3) && trail.TakeForSlot(biome, 22, 22, false) == null
            && !trail.HasPending, "Unbounded biome retained a trail after the species' last eligible depth.");
    }

    /// <summary>Adds a surviving Bait reaction source.</summary>
    private static void AddBait(CatchChainRuntime chain, EffectResolver resolver, CardEffectType type, string[] tags)
    {
        CardDefinition card = Card(1); Set(card, "hasExplicitBaitData", true);
        CardEffectDefinition effect = Effect(type, CardEffectTrigger.WhenAnotherCatchReleased, 1); Set(effect, "requiredTags", tags);
        Set(card, "baitEffects", new[] { effect }); chain.Add(card, resolver, CatchAttachmentRole.Bait);
    }
    /// <summary>Builds one explicit approved event fixture.</summary>
    private static CardDefinition Event(CardEffectType type)
    {
        CardDefinition card = Card(0); Set(card, "cardType", type == CardEffectType.SnagHeaviestUnarmoredCreature
            || type == CardEffectType.QueueCreatureEncounterConcealment ? CardType.Hazard : CardType.Opportunity);
        CardEffectDefinition effect = Effect(type, CardEffectTrigger.OnEventResolution, type == CardEffectType.ScheduleCreatureTrail ? 3 : 1);
        Set(effect, "target", type == CardEffectType.SnagHeaviestUnarmoredCreature ? CardEffectTarget.SpecificCaughtCard
            : type == CardEffectType.ScheduleCreatureTrail || type == CardEffectType.QueueCreatureEncounterConcealment
                ? CardEffectTarget.FutureEncounters : CardEffectTarget.CatchChain);
        if (type == CardEffectType.SnagHeaviestUnarmoredCreature || type == CardEffectType.QueueCreatureEncounterConcealment)
            Set(effect, "effectTone", CardEffectTone.Negative);
        if (type == CardEffectType.SnagHeaviestUnarmoredCreature || type == CardEffectType.QueueBaitWeightReduction)
        { Set(effect, "hasEventLifetime", true); Set(effect, "eventLifetime", type == CardEffectType.SnagHeaviestUnarmoredCreature
            ? EventEffectLifetime.AtBoundary(EventEffectLifetime.BiomeEnd, 1) : EventEffectLifetime.UntilRunEnds()); }
        Set(card, "effects", new[] { effect }); return card;
    }
    /// <summary>Creates a creature without production-asset mutations.</summary>
    private static CardDefinition Card(int weight)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "cardType", CardType.Creature); Set(card, "weight", weight); Set(card, "value", 8);
        Set(card, "displayName", "Fixture"); return card;
    }
    /// <summary>Creates a simple authored effect fixture.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, int amount)
    { CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "trigger", trigger);
        Set(effect, "target", CardEffectTarget.Self); Set(effect, "amount", amount); return effect; }
    /// <summary>Starts a real controller with a deterministic event pool and ordinary-slot biome.</summary>
    private static FishingRunController Controller(CardDefinition source, int depth = 0, params CardDefinition[] alternatives)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome); Set(biome, "biomeId", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition(); Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20);
        List<CardDefinition> pool = new List<CardDefinition> { source }; pool.AddRange(alternatives); Set(tier, "encounterPool", pool.ToArray());
        Set(biome, "depthTiers", new[] { tier }); GameObject obj = new GameObject("Coastal event checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>(); Set(controller, "startRunOnAwake", false);
        Set(controller, "startingBiome", biome); Set(controller, "startingDepth", depth); Set(controller, "startingLineCapacity", 100);
        Set(controller, "useRandomSeed", false); Set(controller, "randomSeed", 1); controller.StartRun(); return controller;
    }
    /// <summary>Reads the actual attached-card owner.</summary>
    private static CatchChainRuntime Chain(FishingRunController c) { return Get<CatchChainRuntime>(c, "catchChainRuntime"); }
    /// <summary>Reads the actual event persistence owner.</summary>
    private static EventPersistenceRuntime Store(FishingRunController c) { return Get<EventPersistenceRuntime>(c, "eventPersistenceRuntime"); }
    /// <summary>Reads the actual encounter owner.</summary>
    private static EncounterRuntime Encounters(FishingRunController c) { return Get<EncounterRuntime>(c, "encounterRuntime"); }
    /// <summary>Reads the actual resolver.</summary>
    private static EffectResolver Resolver(FishingRunController c) { return Get<EffectResolver>(c, "effectResolver"); }
    /// <summary>Configures private serialized fixture fields.</summary>
    private static void Set(object owner, string field, object value) { owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads owned state without copying its behavior.</summary>
    private static T Get<T>(object owner, string field) { return (T)owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Invokes lifecycle boundaries directly for focused lifetime checks.</summary>
    private static void Invoke(object owner, string method) { owner.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null); }
    /// <summary>Counts a rule assertion and reports its first failure.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
