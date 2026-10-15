using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks deliberate-release reactions, conditional weights, and queued School Call encounters.</summary>
public static class PredatorSchoolRayChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs gameplay checks and verifies the production cards retain the approved role effects.</summary>
    [MenuItem("Fishing Cards/Validation/Lionfish Mackerel and Moonlit Ray")]
    public static void Run()
    {
        int count = ValidateRules();
        CardDefinition lion = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Lionfish.asset");
        CardDefinition school = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Golden Mackerel.asset");
        CardDefinition ray = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Moonlit Ray.asset");
        Require(lion.Weight == 2 && lion.Value == 30 && lion.MinimumDepth == 10
            && lion.Effects[0].EffectType == CardEffectType.LoseValueOnCreatureRelease
            && lion.BaitEffects[0].EffectType == CardEffectType.ReleaseCreatureAfterCatch, "Lionfish asset mismatch.");
        Require(school.Weight == 1 && school.Value == 10 && school.MinimumDepth == 0
            && school.Effects[0].EffectType == CardEffectType.SetSelfWeightZeroWithOtherCreatureCatches
            && school.BaitEffects[0].EffectType == CardEffectType.ChooseNextSchoolingEncounter, "Mackerel asset mismatch.");
        Require(ray.Weight == 3 && ray.Value == 24 && ray.MinimumDepth == 9
            && ray.Effects[0].EffectType == CardEffectType.ReduceSelfWeightWithMatchingCreatureBait
            && ray.BaitEffects[0].EffectType == CardEffectType.TransferReleasedCatchValue, "Ray asset mismatch.");
        Debug.Log("Lionfish/Mackerel/Ray runtime validation passed: " + count + " assertions; authored assets passed.");
    }
#endif

    /// <summary>Runs actual gameplay sources in Unity or the minimal Unity API adapter harness.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            ValidateLion();
            ValidateWeights();
            ValidateTransfers();
            ValidateSchool();
            ValidateController();
            return assertions;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj);
            created.Clear();
        }
    }

    /// <summary>Checks Catch-only penalties and deferred optional Bait releases with shared-copy limits.</summary>
    private static void ValidateLion()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        CardInstance lion = chain.Add(Lion(), resolver);
        CardDefinition small = Card("Small", new[] { "Small" });
        CardInstance other = chain.Add(small, resolver, CatchAttachmentRole.Bait);
        Release(chain, other, resolver);
        Require(lion.CurrentValue == 25, "Lionfish ignored deliberate Bait release.");
        other = chain.Add(small, resolver); Release(chain, other, resolver);
        Require(lion.CurrentValue == 20, "Lionfish ignored deliberate Catch release.");
        CardDefinition treasure = Card("Treasure", new[] { "Small" }); Set(treasure, "cardType", CardType.Treasure);
        Release(chain, chain.Add(treasure, resolver), resolver);
        Require(lion.CurrentValue == 20, "Lionfish penalized a non-creature.");
        Release(chain, chain.Add(small, resolver), resolver, CatchRemovalReason.LineStrain);
        Require(lion.CurrentValue == 20, "Forced loss penalized Lionfish.");
        other = chain.Add(small, resolver);
        chain.ReplaceCreatureCatch(other.InstanceId, small, 0, resolver, new System.Random(1));
        Require(lion.CurrentValue == 20, "Exchange penalized Lionfish.");
        for (int i = 0; i < 5; i++) Release(chain, chain.Add(small, resolver), resolver);
        Require(lion.CurrentValue == 0 && lion.PermanentValueModifier == -30, "Lionfish floor accumulated hidden debt.");
        lion.AddPermanentModifiers(0, 2); chain.Recalculate(resolver);
        Require(lion.CurrentValue == 2, "A floored penalty swallowed a later bonus.");
        Set(lion, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Release(chain, chain.Add(small, resolver), resolver);
        Set(lion, "attachmentRole", CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(lion.CurrentValue == 2, "Mode change reset loss or applied a Catch penalty in Bait mode.");

        chain.Reset(); runtime.Reset();
        CardInstance bait = chain.Add(Lion(), resolver, CatchAttachmentRole.Bait);
        chain.Add(Lion(), resolver, CatchAttachmentRole.Bait);
        CardInstance catchLion = chain.Add(Lion(), resolver);
        CardInstance legal = chain.Add(small, resolver, CatchAttachmentRole.Bait);
        CardEffectDefinition lockEffect = Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached,
            CardEffectTarget.Self, 0, Array.Empty<string>());
        CardInstance locked = chain.Add(Card("Locked", new[] { "Small" }, lockEffect), resolver);
        CardEffectDefinition onAttach = Effect(CardEffectType.RemoveNegativeEffect, CardEffectTrigger.WhenCaught,
            CardEffectTarget.None, 1, Array.Empty<string>());
        CardInstance predator = chain.Add(Card("Predator", new[] { "Predator", "Small" }, onAttach), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches);
        Prepare(runtime, chain, resolver, null, null, 1);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect, "Lionfish acted before on-attach choices.");
        runtime.CompleteChoice(); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.OptionalPredatorRelease, "Predator did not offer release.");
        CardInstance[] targets = runtime.GetPredatorReleaseTargets(chain.Catches);
        Require(targets.Length == 1 && targets[0] == legal, "Lionfish included locked/current Predator/non-Small targets.");
        Require(!runtime.TryChoosePredatorRelease(locked.InstanceId, chain, resolver), "Lionfish bypassed Iron Grip.");
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        Require(runtime.TryChoosePredatorRelease(legal.InstanceId, chain, resolver), "Lionfish could not release legal Bait.");
        Require(catchLion.CurrentValue == 25 && chain.ReleasedCatches.Length == 1, "Creature release missed normal release effects.");
        Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Two Lionfish offered two releases per Predator.");
        legal = chain.Add(small, resolver);
        predator = chain.Add(Card("Predator2", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(runtime.TryChoosePredatorRelease(-1, chain, resolver) && Array.Exists(chain.Catches, c => c == legal),
            "Declining removed a creature.");
        Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declined release remained available.");
        CardDefinition apex = Card("Apex predator", new[] { "Predator" }); Set(apex, "cardType", CardType.ApexEncounter);
        predator = chain.Add(apex, resolver); runtime.RecordAttachment(predator, false, chain.Catches);
        Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.OptionalPredatorRelease, "Predator Catch reaction excluded Apex.");
        runtime.CompleteChoice();
        Release(chain, legal, resolver);
        Release(chain, Array.Find(chain.Catches, c => c.Definition.DisplayName == "Predator"), resolver);
        predator = chain.Add(Card("No targets", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No-target Lionfish choice failed to expire.");
        legal = chain.Add(small, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Expired release returned when a target attached.");
        Require(bait.HaulValueContribution == 0, "Lionfish Bait contributed sale value.");
    }

    /// <summary>Checks live Schooling/Elusive tags, role eligibility, absolute zero, and non-stacking glide.</summary>
    private static void ValidateWeights()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardInstance school = chain.Add(Mackerel(), resolver);
        CardDefinition schooling = Card("School", new[] { "Schooling" });
        CardInstance first = chain.Add(schooling, resolver);
        CardInstance second = chain.Add(schooling, resolver, CatchAttachmentRole.Bait);
        Require(school.CurrentWeight == 1, "Packed Shoal counted Bait or itself.");
        Set(second, "attachmentRole", CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(school.CurrentWeight == 0, "Packed Shoal missed two other Schooling Catches.");
        school.AddPermanentModifiers(4, 0); chain.Recalculate(resolver);
        Require(school.CurrentWeight == 0, "Packed Shoal failed to set absolute zero after modifiers.");
        first.SetRuntimeBaseTags(new[] { "Fish" }); chain.Recalculate(resolver);
        Require(school.CurrentWeight == 5, "Packed Shoal failed to restore weight after tag loss.");
        first.SetRuntimeBaseTags(new[] { "Schooling" }); chain.Recalculate(resolver);
        Require(school.CurrentWeight == 0, "Packed Shoal ignored regained Schooling.");
        Set(school, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(school.CurrentWeight == 5, "Packed Shoal Catch effect ran in Bait mode.");
        chain.Reset(); school = chain.Add(Mackerel(), resolver); chain.Add(schooling, resolver);
        CardDefinition treasure = Card("School treasure", new[] { "Schooling" }); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver);
        Require(school.CurrentWeight == 1, "Packed Shoal counted non-creatures.");
        chain.Reset();
        CardInstance ray = chain.Add(Ray(), resolver);
        CardDefinition elusive = Card("Elusive", new[] { "Elusive" });
        chain.Add(elusive, resolver);
        Require(ray.CurrentWeight == 3, "Graceful Glide counted Elusive Catch.");
        CardInstance bait = chain.Add(elusive, resolver, CatchAttachmentRole.Bait);
        Require(ray.CurrentWeight == 2, "Graceful Glide missed Elusive Bait.");
        CardInstance extra = chain.Add(elusive, resolver, CatchAttachmentRole.Bait);
        Require(ray.CurrentWeight == 2, "Graceful Glide stacked per Elusive Bait.");
        bait.SetRuntimeBaseTags(Array.Empty<string>()); chain.Recalculate(resolver);
        Require(ray.CurrentWeight == 2, "Removing one eligible tag removed the other source's reduction.");
        Set(extra, "attachmentRole", CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(ray.CurrentWeight == 3, "Graceful Glide ignored mode change.");
        bait.SetRuntimeBaseTags(new[] { "Elusive" }); chain.Recalculate(resolver);
        Require(ray.CurrentWeight == 2, "Graceful Glide ignored regained Elusive.");
        Set(ray, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(ray.CurrentWeight == 3, "Ray Catch effect ran in Bait mode.");
        chain.Reset(); ray = chain.Add(Ray(), resolver);
        Set(treasure, "tags", new[] { "Elusive" }); chain.Add(treasure, resolver, CatchAttachmentRole.Bait);
        Require(ray.CurrentWeight == 3, "Graceful Glide counted non-creature Bait.");
    }

    /// <summary>Checks value capture across whole multi-release actions, shared copies, and lasting recipient bonuses.</summary>
    private static void ValidateTransfers()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        CardInstance ray = chain.Add(Ray(), resolver, CatchAttachmentRole.Bait);
        CardInstance rayTwo = chain.Add(Ray(), resolver, CatchAttachmentRole.Bait);
        CardInstance recipient = chain.Add(Card("Recipient", Array.Empty<string>()), resolver);
        CardEffectDefinition swift = Effect(CardEffectType.ModifySelfValuePerOtherCreatureCatch, CardEffectTrigger.WhileAttached,
            CardEffectTarget.Self, 3, new[] { "Elusive" });
        CardDefinition elusive = Card("Swift", new[] { "Elusive" }, swift);
        CardInstance one = chain.Add(elusive, resolver);
        CardInstance two = chain.Add(elusive, resolver);
        one.AddPermanentModifiers(0, 7); chain.Recalculate(resolver);
        Require(one.CurrentValue == 18 && two.CurrentValue == 11, "Transfer fixture live modifiers are wrong.");
        Release(chain, one, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Transfer interrupted the release action.");
        Require(two.CurrentValue == 8, "Release did not recalculate the remaining Catch.");
        Release(chain, two, resolver); runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.TransferValue == 18, "First transfer lost pre-removal effective value.");
        Require(runtime.GetTransferTargets(chain.Catches).Length == 1, "Transfer target included Bait or removed Catches.");
        Require(!runtime.TryChooseValueTransfer(ray.InstanceId, chain, resolver), "Transfer allowed a Bait recipient.");
        Require(runtime.TryChooseValueTransfer(recipient.InstanceId, chain, resolver) && recipient.CurrentValue == 26,
            "First lasting transfer failed.");
        Require(runtime.TransferValue == 8, "Second transfer used stale value from before the earlier release.");
        Require(runtime.TryChooseValueTransfer(recipient.InstanceId, chain, resolver) && recipient.CurrentValue == 34
            && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Shared Ray copies duplicated transfers.");
        Release(chain, ray, resolver); Release(chain, rayTwo, resolver); chain.Recalculate(resolver);
        Require(recipient.CurrentValue == 34, "Removing Ray erased a completed transfer.");
        Set(recipient, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(recipient.HaulValueContribution == 0 && recipient.CurrentValue == 34, "Recipient mode change lost stored value or sold Bait.");
        Set(recipient, "attachmentRole", CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(recipient.CurrentValue == 34, "Catch mode failed to restore preserved transfer value.");

        chain.Reset(); runtime.Reset();
        ray = chain.Add(Ray(), resolver, CatchAttachmentRole.Bait);
        recipient = chain.Add(Card("Recipient2", Array.Empty<string>()), resolver);
        one = chain.Add(elusive, resolver, CatchAttachmentRole.Bait);
        Release(chain, one, resolver); runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Releasing Elusive Bait caused transfer.");
        one = chain.Add(elusive, resolver); Release(chain, one, resolver, CatchRemovalReason.LineStrain);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Forced removal caused transfer.");
        one = chain.Add(elusive, resolver); chain.ReplaceCreatureCatch(one.InstanceId, elusive, 0, resolver, new System.Random(2));
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Exchange caused transfer.");
        one = chain.Catches[2]; int recorded = one.CurrentValue;
        Release(chain, one, resolver); Release(chain, ray, resolver);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.TransferValue == recorded && runtime.TryChooseValueTransfer(recipient.InstanceId, chain, resolver),
            "Later source removal in one action canceled an already triggered transfer.");
        chain.Reset(); runtime.Reset(); chain.Add(Ray(), resolver, CatchAttachmentRole.Bait);
        one = chain.Add(elusive, resolver); Release(chain, one, resolver);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No-Catch transfer remained pending.");
        chain.Add(Card("Too late", Array.Empty<string>()), resolver);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Skipped transfer returned later.");
    }

    /// <summary>Checks distinct eligible offers, once-per-card activation, queue replacement, and reveal-time validation.</summary>
    private static void ValidateSchool()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        EncounterRuntime encounters = new EncounterRuntime();
        CardDefinition one = Card("One", new[] { "Schooling" });
        CardDefinition two = Card("Two", new[] { "Schooling" });
        CardDefinition deep = Card("Too deep", new[] { "Schooling" }); Set(deep, "minimumDepth", 9);
        CardDefinition foreign = Card("Foreign", new[] { "Schooling" }); Set(foreign, "biomeIds", new[] { "Elsewhere" });
        CardDefinition apex = Card("Schooling Apex", new[] { "Schooling" }); Set(apex, "cardType", CardType.ApexEncounter);
        CardDefinition ordinary = Card("Ordinary", new[] { "Fish" });
        BiomeDefinition biome = Biome(new[] { one, one, two, deep, foreign, apex, ordinary });
        NextEncounterDestination destination = new NextEncounterDestination(biome, 3);
        encounters.SetCurrentEncounter(ordinary);
        CardInstance source = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(source);
        Prepare(runtime, chain, resolver, biome, destination, 0, encounters);
        Require(runtime.OfferedCreatures.Length == 2 && runtime.OfferedCreatures[0] != runtime.OfferedCreatures[1],
            "School Call offers were not distinct.");
        Require(Array.Exists(runtime.OfferedCreatures, c => c == one) && Array.Exists(runtime.OfferedCreatures, c => c == two),
            "School Call included wrong tags, depth, biome, or Apex.");
        Require(encounters.CurrentEncounter == ordinary && encounters.RecentEncounterHistory.Length == 0,
            "Offers consumed or revealed a normal encounter.");
        Require(!runtime.TryChooseNextEncounter(2), "School Call accepted invalid offer.");
        CardDefinition chosen = runtime.OfferedCreatures[0];
        Require(runtime.TryChooseNextEncounter(0) && runtime.QueuedEncounter == chosen, "School Call did not queue selection.");
        runtime.RecordAttachment(source); Prepare(runtime, chain, resolver, biome, destination);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "School Call repeated on the same card.");
        Require(!source.CreateSnapshot().TryConsumeAttachmentActivation(0), "Snapshot reset School Call activation.");
        Set(source, "attachmentRole", CatchAttachmentRole.Catch); chain.Recalculate(resolver); runtime.RecordAttachment(source);
        Set(source, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver); runtime.RecordAttachment(source);
        runtime.BeginBiome(chain.Catches); Prepare(runtime, chain, resolver, biome, destination);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Mode/biome change refreshed School Call.");
        Release(chain, source, resolver);
        Require(runtime.TakeQueuedEncounter(destination) == chosen && runtime.QueuedEncounter == null,
            "Removing Mackerel canceled its queued encounter or queue repeated.");
        Require(runtime.TakeQueuedEncounter(destination) == null, "Queue survived consumption.");

        source = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(source);
        BiomeDefinition single = Biome(new[] { two }); NextEncounterDestination singleDestination = new NextEncounterDestination(single, 3);
        Prepare(runtime, chain, resolver, single, singleDestination);
        Require(runtime.OfferedCreatures.Length == 1 && runtime.TryChooseNextEncounter(0), "Single eligible species did not offer one.");
        CardInstance second = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        Prepare(runtime, chain, resolver, biome, destination);
        Require(runtime.TryChooseNextEncounter(-1) && runtime.QueuedEncounter == two, "Decline erased an earlier queued selection.");
        second = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        single = Biome(new[] { one }); singleDestination = new NextEncounterDestination(single, 3);
        Prepare(runtime, chain, resolver, single, singleDestination); runtime.TryChooseNextEncounter(0);
        Require(runtime.QueuedEncounter == one, "New selection failed to replace an earlier queue.");
        Require(runtime.TakeQueuedEncounter(new NextEncounterDestination(Biome(new[] { ordinary }), 3)) == null
            && runtime.QueuedEncounter == null, "Changed destination retained an ineligible selection.");
        second = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        Prepare(runtime, chain, resolver, biome, new NextEncounterDestination(biome, 3, false));
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && !second.TryConsumeAttachmentActivation(0),
            "Scheduled Apex was replaceable or activation remained unspent.");
        second = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        Prepare(runtime, chain, resolver, biome, destination); runtime.TryChooseNextEncounter(0);
        Require(runtime.TakeQueuedEncounter(new NextEncounterDestination(biome, 3, false)) == null,
            "Queued selection replaced a newly scheduled Apex.");
        second = chain.Add(Mackerel(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        Prepare(runtime, chain, resolver, biome, new NextEncounterDestination(Biome(new[] { ordinary }), 3));
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No eligible Schooling species failed to skip.");
        CardDefinition heavyTicket = Card("Weighted School", new[] { "Schooling", "Heavy" });
        CardEffectDefinition attraction = Effect(CardEffectType.ModifyFutureEncounterProperties, CardEffectTrigger.Manual,
            CardEffectTarget.FutureEncounters, 20, new[] { "Heavy" });
        CardDefinition[] weightedOffers = encounters.OfferCreatures(new[] { one, heavyTicket }, "Coastal", 3, 1,
            new System.Random(42), chain.ActiveEffectRecords, resolver, new[] { attraction }, new[] { "Schooling" });
        Require(weightedOffers.Length == 1 && weightedOffers[0] == heavyTicket,
            "School Call ignored normal effect-modified encounter weights.");
        CardDefinition sameSpecies = Card("One", new[] { "Schooling" });
        weightedOffers = encounters.OfferCreatures(new[] { one, sameSpecies }, "Coastal", 3, 2,
            new System.Random(42), chain.ActiveEffectRecords, resolver, null, new[] { "Schooling" });
        Require(weightedOffers.Length == 1, "Distinct species filtering relied only on asset reference.");
        CardEffectDefinition reveal = Effect(CardEffectType.HideEncounterInformation, CardEffectTrigger.WhileAttached,
            CardEffectTarget.CurrentEncounter, 0, Array.Empty<string>());
        CardDefinition revealCard = Card("Chosen reveal", new[] { "Schooling" }, reveal);
        Require(encounters.RevealSelectedCreature(revealCard, "Coastal", 3, chain.ActiveEffectRecords, resolver),
            "Chosen encounter failed normal reveal.");
        Require(encounters.CurrentEncounter == revealCard && encounters.HookedEffectRecords.Length == 1
            && encounters.RecentEncounterHistory.Length == 1, "Chosen reveal missed Hooked effects or encounter history.");
    }

    /// <summary>Checks destination previews and coordinator suspension/resumption at the normal next reveal.</summary>
    private static void ValidateController()
    {
        CardDefinition school = Mackerel();
        CardDefinition shallow = Card("Shallow", new[] { "Schooling" }); Set(shallow, "maximumDepth", 2);
        CardDefinition deep = Card("Next depth", new[] { "Schooling" }); Set(deep, "minimumDepth", 3);
        BiomeDefinition biome = Biome(new[] { school, shallow, deep });
        GameObject obj = new GameObject("School controller validation"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", biome);
        Set(controller, "startingDepth", 2); Set(controller, "startingLineCapacity", 100);
        controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        CreatureEffectRuntime runtime = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        TechniqueEffectRuntime techniques = Get<TechniqueEffectRuntime>(controller, "techniqueEffectRuntime");
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        CardInstance source = chain.Add(school, resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(source);
        Set(controller, "descendCompletionPending", true);
        Set(controller, "pendingCommittedCatchId", source.InstanceId);
        encounters.TakeHookedEncounter();
        Invoke(controller, "PrepareCreatureChoice");
        Require(controller.CurrentDepth == 2 && runtime.OfferedCreatures.Length == 2
            && Array.IndexOf(runtime.OfferedCreatures, shallow) < 0, "Offers used current depth or advanced it early.");
        int offer = Array.IndexOf(runtime.OfferedCreatures, deep);
        Require(offer >= 0 && controller.TryChooseNextCreatureEncounter(offer)
            && controller.CurrentDepth == 3 && controller.CurrentEncounter == deep && !controller.HasPendingCreatureChoice,
            "Choosing School Call failed to resume Descend and normal reveal.");
        Require(runtime.QueuedEncounter == null, "Controller did not consume its chosen encounter.");
        CardEffectDefinition distance = Effect(CardEffectType.ModifyDescendDistance, CardEffectTrigger.Manual,
            CardEffectTarget.None, 2, Array.Empty<string>());
        Set(techniques, "pendingDescendEffects", new[] { distance });
        Set(controller, "descendCompletionPending", true);
        NextEncounterDestination preview = (NextEncounterDestination)Invoke(controller, "GetNextEncounterDestination");
        Require(preview.Depth == 6 && controller.CurrentDepth == 3 && techniques.PendingDescendEffects.Length == 1,
            "Destination preview omitted or consumed next-Descend distance.");
        Set(controller, "descendCompletionPending", false);
        Set(controller, "currentDepth", biome.GetApexBoundaryDepth());
        preview = (NextEncounterDestination)Invoke(controller, "GetNextEncounterDestination");
        Require(!preview.CanReplaceNormalEncounter, "Destination preview ignored Apex priority.");
        Set(controller, "currentDepth", 3);
        Set(techniques, "pendingDescendEffects", Array.Empty<CardEffectDefinition>());
        CardEffectDefinition offset = Effect(CardEffectType.ModifyNextEncounterDepth, CardEffectTrigger.Manual,
            CardEffectTarget.FutureEncounters, 2, Array.Empty<string>());
        Set(techniques, "pendingEncounterEffects", new[] { offset });
        preview = (NextEncounterDestination)Invoke(controller, "GetNextEncounterDestination");
        Require(preview.Depth == 5 && techniques.PendingEncounterEffects.Length == 1,
            "Destination preview omitted or consumed next-encounter depth offset.");
        Set(techniques, "pendingEncounterEffects", Array.Empty<CardEffectDefinition>());

        CardInstance baitRay = chain.Add(Ray(), resolver, CatchAttachmentRole.Bait);
        CardInstance victim = chain.Add(Card("Elusive release", new[] { "Small", "Elusive" }), resolver);
        CardInstance recipient = chain.Add(Card("Transfer recipient", Array.Empty<string>()), resolver);
        Require(TechniqueRuleChecks.ReleaseFixture(controller, Array.IndexOf(chain.Catches, victim))
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.TransferValue,
            "Controller did not expose a completed release transfer.");
        Require(!controller.TryDescend() && !controller.TrySurface(), "Gameplay bypassed the transfer choice.");
        Require(controller.TryChooseCreatureValueTransfer(recipient.InstanceId)
            && recipient.CurrentValue == 16 && !controller.HasPendingCreatureChoice,
            "Controller failed to apply and finish the transfer.");

        CardInstance lionBait = chain.Add(Lion(), resolver, CatchAttachmentRole.Bait);
        victim = chain.Add(Card("Small release", new[] { "Small", "Elusive" }), resolver);
        CardInstance predator = chain.Add(Card("Predator catch", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Invoke(controller, "PrepareCreatureChoice");
        Require(controller.PendingCreatureChoice == CreatureEffectChoiceKind.OptionalPredatorRelease,
            "Controller did not prepare Lionfish's optional choice.");
        Require(controller.TryChoosePredatorRelease(victim.InstanceId)
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.TransferValue,
            "Lionfish controller release missed Ray's normal release reaction.");
        Require(controller.TryChooseCreatureValueTransfer(recipient.InstanceId)
            && recipient.CurrentValue == 24 && !controller.HasPendingCreatureChoice,
            "Nested Lionfish/Ray reaction did not finish.");

    }

    /// <summary>Builds a Lionfish fixture with separate trigger and release-target tags.</summary>
    private static CardDefinition Lion()
    {
        CardDefinition card = Card("Lionfish", new[] { "Fish", "Predator", "Armored" },
            Effect(CardEffectType.LoseValueOnCreatureRelease, CardEffectTrigger.WhenAnotherCatchReleased,
                CardEffectTarget.Self, 5, Array.Empty<string>()));
        Set(card, "weight", 2); Set(card, "value", 30);
        CardEffectDefinition bait = Effect(CardEffectType.ReleaseCreatureAfterCatch, CardEffectTrigger.AfterAnotherCatchAttached,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Predator" });
        Set(bait, "targetTags", new[] { "Small" }); Set(card, "baitEffects", new[] { bait });
        return card;
    }

    /// <summary>Builds an approved Mackerel fixture.</summary>
    private static CardDefinition Mackerel()
    {
        CardDefinition card = Card("Mackerel", new[] { "Fish", "Schooling" },
            Effect(CardEffectType.SetSelfWeightZeroWithOtherCreatureCatches, CardEffectTrigger.WhileAttached,
                CardEffectTarget.Self, 2, new[] { "Schooling" }));
        Set(card, "value", 10); Set(card, "baitEffects", new[] {
            Effect(CardEffectType.ChooseNextSchoolingEncounter, CardEffectTrigger.WhenCaught,
                CardEffectTarget.FutureEncounters, 2, new[] { "Schooling" }) });
        return card;
    }

    /// <summary>Builds an approved Ray fixture with a flat single-point Catch reduction.</summary>
    private static CardDefinition Ray()
    {
        CardDefinition card = Card("Moonlit Ray", new[] { "Fish", "Predator", "Elusive" },
            Effect(CardEffectType.ReduceSelfWeightWithMatchingCreatureBait, CardEffectTrigger.WhileAttached,
                CardEffectTarget.Self, 1, new[] { "Elusive" }));
        Set(card, "weight", 3); Set(card, "value", 24); Set(card, "baitEffects", new[] {
            Effect(CardEffectType.TransferReleasedCatchValue, CardEffectTrigger.WhenAnotherCatchReleased,
                CardEffectTarget.SpecificCaughtCard, 1, new[] { "Elusive" }) });
        return card;
    }

    /// <summary>Creates an immutable authoring fixture.</summary>
    private static CardDefinition Card(string name, string[] tags, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "tags", tags); Set(card, "effects", effects);
        Set(card, "hasExplicitBaitData", true); Set(card, "minimumDepth", 0); Set(card, "maximumDepth", 20);
        return card;
    }

    /// <summary>Creates a coastal biome with a complete normal depth tier.</summary>
    private static BiomeDefinition Biome(CardDefinition[] pool)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); Set(biome, "displayName", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", pool);
        Set(biome, "depthTiers", new[] { tier }); return biome;
    }

    /// <summary>Creates a controlled serialized effect fixture.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger,
        CardEffectTarget target, int amount, string[] tags)
    {
        CardEffectDefinition effect = new CardEffectDefinition();
        Set(effect, "effectType", type); Set(effect, "trigger", trigger); Set(effect, "target", target);
        Set(effect, "amount", amount); Set(effect, "requiredTags", tags); return effect;
    }

    /// <summary>Prepares a request at a deterministic resolution window.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EffectResolver resolver,
        BiomeDefinition biome = null, NextEncounterDestination destination = null, int negativeCount = 0,
        EncounterRuntime encounters = null)
    {
        runtime.PrepareChoice(chain, encounters ?? new EncounterRuntime(), biome,
            new TechniqueEffectRuntime(), resolver, new System.Random(42), () => negativeCount, null, destination);
    }

    /// <summary>Exercises the actual chain removal path rather than imitating release state.</summary>
    private static void Release(CatchChainRuntime chain, CardInstance card, EffectResolver resolver,
        CatchRemovalReason reason = CatchRemovalReason.PlayerChoice)
    {
        if (!chain.TryRelease(Array.IndexOf(chain.Catches, card), resolver, out _, out _, out string message, reason))
            throw new InvalidOperationException(message);
    }

    /// <summary>Sets fixture fields through their existing Unity serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads a runtime owner for action-level regression checks.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Invokes coordinator boundaries without duplicating their gameplay behavior in fixtures.</summary>
    private static object Invoke(object owner, string name)
    {
        return owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);
    }

    /// <summary>Counts a meaningful assertion and stops at the first failed rule.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
