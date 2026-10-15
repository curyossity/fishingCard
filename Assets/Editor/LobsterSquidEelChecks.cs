using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks cancelled-Hazard rewards, encounter concealment/skip lifetimes, and post-action release draws.</summary>
public static class LobsterSquidEelChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs durable gameplay checks and verifies approved production asset identities.</summary>
    [MenuItem("Fishing Cards/Validation/Lobster Squid and Eel")]
    public static void Run()
    {
        int count = ValidateRules();
        CardDefinition lobster = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Spiny Lobster.asset");
        CardDefinition squid = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/squid.asset");
        CardDefinition eel = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Tidepool Eel.asset");
        Require(lobster.Weight == 3 && lobster.Value == 24 && lobster.MinimumDepth == 8
            && lobster.Rarity == CardRarity.Rare && lobster.Effects[0].EffectType == CardEffectType.GainValueOnHazardCancelled
            && lobster.BaitEffects[0].EffectType == CardEffectType.ReduceOtherMatchingCreatureWeight, "Lobster asset mismatch.");
        Require(squid.Weight == 2 && squid.Value == 20 && squid.MinimumDepth == 8
            && squid.Rarity == CardRarity.Rare && squid.Effects[0].EffectType == CardEffectType.QueueCreatureEncounterConcealment
            && squid.BaitEffects[0].EffectType == CardEffectType.GrantNextCreatureSkip, "Squid asset mismatch.");
        Require(eel.Weight == 2 && eel.Value == 14 && eel.MinimumDepth == 0 && eel.MaximumDepth == 14
            && Array.IndexOf(eel.Tags, "Predator") >= 0 && eel.Effects[0].EffectType == CardEffectType.ReleaseMatchingBaitOnAttach
            && eel.BaitEffects[0].EffectType == CardEffectType.DrawTechniqueAfterCreatureRelease, "Eel asset mismatch.");
        Debug.Log("Lobster/Squid/Eel passed: " + count + " runtime assertions; production assets passed.");
    }
#endif
    /// <summary>Runs actual gameplay source checks without duplicating runtime algorithms.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateLobster(); ValidateInk(); ValidateSkip(); ValidateEel(); ValidateController(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks fully cancelled Hazard dispatch and live other-creature support.</summary>
    private static void ValidateLobster()
    {
        EffectResolver resolver = new EffectResolver(); CatchChainRuntime chain = new CatchChainRuntime();
        CardDefinition data = Lobster(); CardInstance first = chain.Add(data, resolver);
        CardInstance second = chain.Add(data, resolver); CardInstance bait = chain.Add(data, resolver, CatchAttachmentRole.Bait);
        Require(first.CurrentWeight == 2 && second.CurrentWeight == 2 && bait.CurrentWeight == 3, "Support included its own source or missed Catches.");
        CardInstance anotherBait = chain.Add(data, resolver, CatchAttachmentRole.Bait);
        Require(first.CurrentWeight == 1 && bait.CurrentWeight == 2 && anotherBait.CurrentWeight == 2, "Multiple supports failed to stack or supported themselves.");
        CardInstance small = chain.Add(Card("Armor", new[] { "Armored" }), resolver, CatchAttachmentRole.Bait);
        Require(small.CurrentWeight == 0, "Support missed Bait or failed minimum weight.");
        small.SetRuntimeBaseTags(new[] { "Small" }); chain.Recalculate(resolver);
        Require(small.CurrentWeight == 1, "Losing Armored kept support.");
        small.SetRuntimeBaseTags(new[] { "Armored" }); chain.Recalculate(resolver);
        Require(small.CurrentWeight == 0, "Regaining Armored failed to restore support.");
        CardDefinition treasure = Card("Armored treasure", new[] { "Armored" }); Set(treasure, "cardType", CardType.Treasure);
        Require(chain.Add(treasure, resolver).CurrentWeight == 1, "Support applied to a non-creature.");
        bait.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(first.CurrentWeight == 2 && bait.CurrentWeight == 2, "Leaving Bait did not end support.");
        Release(chain, anotherBait, resolver);
        Require(first.CurrentWeight == 3 && small.CurrentWeight == 1, "Removing support left stale weight.");
        EncounterRuntime encounters = new EncounterRuntime(); int cancelled = 0;
        encounters.HazardCancelled += hazard => { cancelled++; chain.RecordHazardCancelled(hazard, resolver); };
        CardDefinition hazardCard = Card("Hazard", Array.Empty<string>()); Set(hazardCard, "cardType", CardType.Hazard);
        encounters.SetCurrentEncounter(hazardCard);
        Require(first.CurrentValue == 24 && cancelled == 0, "Normal reveal granted cancelled-Hazard reward.");
        encounters.SetCurrentEncounter(null);
        Require(first.CurrentValue == 24, "Ordinary removal was treated as cancellation.");
        encounters.HazardCancellation = hazard => true;
        encounters.SetCurrentEncounter(hazardCard);
        Require(cancelled == 1 && first.CurrentValue == 28 && second.CurrentValue == 28 && bait.CurrentValue == 28
            && encounters.CurrentEncounter == null, "Full cancellation failed to reward each current Lobster Catch once.");
        first.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        encounters.SetCurrentEncounter(hazardCard);
        Require(first.CurrentValue == 28 && second.CurrentValue == 32 && cancelled == 2, "Bait earned a reward or prior bonuses disappeared.");
        first.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(first.CurrentValue == 28 && data.Value == 24, "Role changes lost reward or changed immutable asset data.");
        chain.RecordHazardCancelled(treasure, resolver);
        Require(first.CurrentValue == 28, "Non-Hazard cancellation earned value.");
    }

    /// <summary>Checks charge assignment, full commitment concealment, source independence, replacement, and exact cleansing.</summary>
    private static void ValidateInk()
    {
        EffectResolver resolver = new EffectResolver(); CatchChainRuntime chain = new CatchChainRuntime();
        EncounterRuntime encounters = new EncounterRuntime(); CreatureEffectRuntime runtime = new CreatureEffectRuntime { EncounterEffects = encounters };
        CardInstance squid = chain.Add(Squid(), resolver); runtime.RecordAttachment(squid);
        Require(encounters.PendingConcealments.Length == 1 && encounters.PendingConcealments[0].Remaining == 2, "Catch did not grant two independent concealments.");
        Release(chain, squid, resolver);
        foreach (CardType type in new[] { CardType.Hazard, CardType.Treasure, CardType.Environment, CardType.ApexEncounter })
        {
            CardDefinition nonordinary = Card(type.ToString(), Array.Empty<string>()); Set(nonordinary, "cardType", type);
            encounters.SetCurrentEncounter(nonordinary);
            Require(!encounters.IsCreatureConcealed && encounters.PendingConcealments[0].Remaining == 2, "Ineligible reveal consumed Ink.");
        }
        CardDefinition creature = Card("Hidden creature", new[] { "Fish" }); encounters.SetCurrentEncounter(creature);
        Require(encounters.IsCreatureConcealed && encounters.PendingConcealments[0].Remaining == 1, "Eligible reveal failed to reserve exactly one concealment.");
        Require(encounters.TakeHookedEncounter() == creature && !encounters.IsCreatureConcealed,
            "Committed identity did not reveal before attachment resolution.");
        encounters.SetCurrentEncounter(creature);
        Require(encounters.IsCreatureConcealed && encounters.PendingConcealments[0].Remaining == 0, "Second creature was not concealed.");
        encounters.SetCurrentEncounter(null);
        Require(!encounters.IsCreatureConcealed && encounters.PendingConcealments.Length == 0, "Removing hidden creature did not consume charge.");
        encounters.SetCurrentEncounter(creature); Require(!encounters.IsCreatureConcealed, "Expired Ink concealed a third encounter.");
        squid = chain.Add(Squid(), resolver); runtime.RecordAttachment(squid); runtime.RecordAttachment(squid);
        Require(encounters.PendingConcealments.Length == 2, "Consecutive Catch activations did not create independent grants.");
        for (int i = 0; i < 4; i++) { encounters.SetCurrentEncounter(creature); Require(encounters.IsCreatureConcealed, "Stacked Ink failed a consecutive creature."); }
        encounters.SetCurrentEncounter(creature); Require(!encounters.IsCreatureConcealed && encounters.PendingConcealments.Length == 0, "Stacked Ink double-concealed or exceeded its four charges.");
        runtime.RecordAttachment(squid); encounters.SetCurrentEncounter(creature); encounters.Reset(true);
        Require(!encounters.IsCreatureConcealed && encounters.PendingConcealments[0].Remaining == 1, "Biome reset lost unused charge or preserved removed encounter assignment.");
        encounters.SetCurrentEncounter(creature); Require(encounters.IsCreatureConcealed, "Pending Ink did not reach new biome.");
        encounters.Reset(); Require(encounters.PendingConcealments.Length == 0, "Full run reset retained Ink.");
        runtime.RecordAttachment(squid); runtime.RecordAttachment(squid); encounters.SetCurrentEncounter(creature);
        NegativeEffectOption[] options = NegativeEffectRuntime.GetOptions(chain, encounters, new TechniqueEffectRuntime(), resolver);
        Require(options.Length == 2, "Pending/assigned Ink was not available to Quick Clean.");
        Require(options[0].TryRemove() && !encounters.IsCreatureConcealed && encounters.PendingConcealments.Length == 1
            && encounters.PendingConcealments[0].Remaining == 2, "Cleansing an exact assigned occurrence removed another grant or kept current identity hidden.");
        Require(!options[0].TryRemove(), "Cleansing the same expired occurrence succeeded twice.");
        encounters.SetCurrentEncounter(creature); Require(encounters.IsCreatureConcealed, "Other Ink grant failed after cleansing.");
    }

    /// <summary>Checks reveal-time FIFO skip offers, spent activation, hidden decline, and removal during reveal.</summary>
    private static void ValidateSkip()
    {
        EffectResolver resolver = new EffectResolver(); CatchChainRuntime chain = new CatchChainRuntime();
        EncounterRuntime encounters = new EncounterRuntime(); CreatureEffectRuntime runtime = new CreatureEffectRuntime { EncounterEffects = encounters };
        encounters.CreatureRevealed += runtime.RecordCreatureReveal; TechniqueDeckRuntime deck = Deck(0, 0);
        CardInstance squid = chain.Add(Squid(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(squid);
        Require(runtime.PendingCreatureSkipCount == 1 && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Bait offered skip against its own committed encounter.");
        runtime.RecordAttachment(squid); squid.SetAttachmentRole(CatchAttachmentRole.Catch); squid.SetAttachmentRole(CatchAttachmentRole.Bait);
        runtime.RecordAttachment(squid); Release(chain, squid, resolver);
        Require(runtime.PendingCreatureSkipCount == 1, "Mode change refreshed grant or source removal cancelled it.");
        CardDefinition apex = Card("Apex", Array.Empty<string>()); Set(apex, "cardType", CardType.ApexEncounter);
        encounters.SetCurrentEncounter(apex); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.PendingCreatureSkipCount == 1 && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Apex consumed skip.");
        CardDefinition creature = Card("Ordinary", Array.Empty<string>());
        encounters.GrantCreatureConcealment(Squid().Effects[0], Squid()); encounters.SetCurrentEncounter(creature);
        Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.EncounterSkip && runtime.PendingCreatureSkipCount == 0
            && encounters.IsCreatureConcealed, "Hidden eligible creature did not offer immediate skip.");
        Require(runtime.TryChooseEncounterSkip(false, encounters) && encounters.HookedEncounter == creature
            && encounters.IsCreatureConcealed && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declining committed/revealed the creature or retained choice.");
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        int released = chain.ReleasedCatches.Length;
        Require(runtime.TryChooseEncounterSkip(true, encounters) && encounters.HookedEncounter == null
            && !encounters.IsCreatureConcealed && chain.ReleasedCatches.Length == released, "Skip attached/released creature or retained concealment.");
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Spent skip applied to another creature.");
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.PendingCreatureSkipCount == 1 && runtime.TryChooseEncounterSkip(false, encounters), "First FIFO grant consumed multiple offers.");
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.PendingCreatureSkipCount == 0 && runtime.TryChooseEncounterSkip(false, encounters), "Second FIFO grant missed consecutive encounter.");
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        Action<CardDefinition> removeOnReveal = card => { if (card == creature) encounters.SetCurrentEncounter(null); };
        encounters.EncounterRevealEffects += removeOnReveal;
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.PendingCreatureSkipCount == 0 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Creature removed by reveal effects offered stale skip or saved grant.");
        encounters.EncounterRevealEffects -= removeOnReveal;
        encounters.SetCurrentEncounter(creature); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Removed reveal moved its skip to the next creature.");
    }

    /// <summary>Checks Catch release legality and shared draws using pre-removal tags after the whole release action.</summary>
    private static void ValidateEel()
    {
        EffectResolver resolver = new EffectResolver(); CatchChainRuntime chain = new CatchChainRuntime();
        EncounterRuntime encounters = new EncounterRuntime(); CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        chain.DeliberatelyReleased += (card, reason) => runtime.RecordDeliberateRelease(card, reason, chain.Catches);
        CardInstance bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        CardInstance other = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        CardInstance eligible = chain.Add(Card("Elusive Bait", new[] { "Elusive" }), resolver, CatchAttachmentRole.Bait);
        CardInstance caught = chain.Add(Card("Elusive Catch", new[] { "Elusive" }), resolver);
        CardInstance locked = chain.Add(Card("Locked", new[] { "Elusive" }, Effect(CardEffectType.PreventRelease,
            CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0)), resolver, CatchAttachmentRole.Bait);
        Set(locked.Definition, "baitEffects", locked.Definition.Effects); chain.Recalculate(resolver);
        CardInstance eel = chain.Add(Eel(), resolver); runtime.RecordAttachment(eel);
        TechniqueDeckRuntime deck = Deck(4, 8); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.ReleaseMatchingBait
            && runtime.GetMatchingBaitReleaseTargets(chain.Catches).Length == 3, "Catch release included Catch/source/locked target or missed Eel Baits.");
        Require(!runtime.TryChooseMatchingBaitRelease(caught.InstanceId, chain, resolver)
            && !runtime.TryChooseMatchingBaitRelease(locked.InstanceId, chain, resolver), "Catch choice bypassed role or release restriction.");
        Require(runtime.TryChooseMatchingBaitRelease(eligible.InstanceId, chain, resolver), "Legal Eel Catch release failed.");
        Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DrawTechniques && deck.DrawPile.Length == 8, "Release draw ran early or no discard choice offered.");
        Require(runtime.TryChooseFeedingDraw(0, deck, new System.Random(1)), "Eel draw failed optional room discard.");
        Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == 7 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Multiple Eel copies drew twice for one released creature.");
        runtime.RecordAttachment(eel); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.TryChooseMatchingBaitRelease(-1, chain, resolver), "Optional Catch release could not decline.");
        Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declined release saved its opportunity.");
        CardInstance one = chain.Add(Card("One", new[] { "Elusive" }), resolver);
        CardInstance two = chain.Add(Card("Two", new[] { "Elusive" }), resolver, CatchAttachmentRole.Bait);
        int before = deck.DrawPile.Length;
        Require(chain.TryReleaseTogether(new[] { one.InstanceId, two.InstanceId }, resolver), "Group fixture could not release.");
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && deck.DrawPile.Length == before, "Draw became available before release action finished.");
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        runtime.TryChooseFeedingDraw(0, deck, new System.Random(1));
        Require(deck.Hand.Length == 3 && deck.DrawPile.Length == before, "First optional discard drew before all room choices.");
        runtime.TryChooseFeedingDraw(0, deck, new System.Random(1)); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == before - 2, "Group release did not draw one per eligible creature.");
        one = chain.Add(Card("Forced", new[] { "Elusive" }), resolver);
        chain.TryRelease(Array.IndexOf(chain.Catches, one), resolver, out _, out _, out _, CatchRemovalReason.LineStrain);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Forced removal granted release draw.");
        runtime.Reset(); chain.Reset(); deck = Deck(0, 8);
        bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait); one = chain.Add(Card("Elusive", new[] { "Elusive" }), resolver);
        Release(chain, one, resolver); Release(chain, bait, resolver); runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 0 && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Eel removed later in action still granted draw.");
        bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait); other = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        one = chain.Add(Card("Elusive", new[] { "Elusive" }), resolver);
        Release(chain, one, resolver); Release(chain, bait, resolver); runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 2 && deck.DrawPile.Length == 6, "Surviving Eel failed fallback or failed to observe another Eel released.");
        runtime.Reset(); chain.Reset(); deck = Deck(0, 8);
        bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        CardEffectDefinition grant = Effect(CardEffectType.GrantTagToOtherCreatures, CardEffectTrigger.WhileAttached,
            CardEffectTarget.CatchChain, 0, new[] { "Fish" }); Set(grant, "grantedTag", "Elusive");
        CardDefinition garfish = Card("Tag source", new[] { "Fish" }); Set(garfish, "baitEffects", new[] { grant });
        CardInstance tagSource = chain.Add(garfish, resolver, CatchAttachmentRole.Bait);
        one = chain.Add(Card("Fish", new[] { "Fish" }), resolver);
        Require(one.HasTag("Elusive"), "Live-tag release fixture did not grant Elusive.");
        chain.TryReleaseTogether(new[] { one.InstanceId, tagSource.InstanceId }, resolver);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 1, "Release draw missed pre-removal granted tag or counted its non-Elusive source.");
        runtime.Reset(); chain.Reset(); deck = Deck(0, 8);
        bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        one = chain.Add(Card("Elusive", new[] { "Elusive" }), resolver); Release(chain, one, resolver);
        bait.SetAttachmentRole(CatchAttachmentRole.Catch); runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 0 && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Eel leaving Bait before action completion still drew.");
        runtime.Reset(); chain.Reset(); deck = Deck(0, 8);
        eel = chain.Add(Eel(), resolver); runtime.RecordAttachment(eel); Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No-target Catch release did not expire immediately.");
        one = chain.Add(Card("Later", new[] { "Elusive" }), resolver, CatchAttachmentRole.Bait);
        Prepare(runtime, chain, encounters, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Unavailable Catch choice was saved for a later attachment.");
        runtime.Reset(); chain.Reset(); deck = Deck(0, 8); bait = chain.Add(Eel(), resolver, CatchAttachmentRole.Bait);
        int[] ids = new int[6]; for (int i = 0; i < ids.Length; i++) ids[i] = chain.Add(Card("Elusive " + i, new[] { "Elusive" }), resolver).InstanceId;
        chain.TryReleaseTogether(ids, resolver); runtime.FinishReleaseResolution(); Prepare(runtime, chain, encounters, resolver, deck);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == 4 && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Large release group overflowed or stalled the four-card hand.");
    }

    /// <summary>Checks controller binding, commitment before on-attach, immediate skip blocking, and depth progression.</summary>
    private static void ValidateController()
    {
        CardDefinition bass = Card("Hidden Bass", new[] { "Fish" }, Effect(CardEffectType.DiscardTechniqueOnAttach,
            CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1));
        GameObject obj = new GameObject("Ink coordinator checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", Biome(new[] { bass }));
        Set(controller, "startingDepth", 1); Set(controller, "startingLineCapacity", 100); controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        CreatureEffectRuntime runtime = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        runtime.RecordAttachment(chain.Add(Squid(), resolver)); encounters.SetCurrentEncounter(bass); Invoke(controller, "RefreshViews");
        Set(deck, "hand", Deck(2, 0).Hand);
        Require(controller.CurrentEncounterIdentityHidden && controller.CurrentEncounterInformationHidden, "Controller did not conceal full encounter.");
        CardDefinition reveal = Technique("Reveal", Effect(CardEffectType.RevealEncounterInformation,
            CardEffectTrigger.Manual, CardEffectTarget.CurrentEncounter, 0));
        Set(deck, "hand", new[] { reveal, Technique("Discard") });
        Require(!controller.CanUseTechniqueCard(0, out _) && encounters.IsCreatureConcealed,
            "Ordinary reveal Technique bypassed or spent a card against commit-only Ink.");
        Require(controller.TryDescend() && !controller.CurrentEncounterIdentityHidden
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique && controller.CurrentDepth == 1,
            "Identity stayed hidden during on-attach or Descend bypassed mandatory effect.");
        Require(controller.TryChooseRequiredTechniqueDiscard(0) && controller.CurrentDepth == 2
            && controller.CurrentEncounterIdentityHidden, "Resuming Descend did not conceal next eligible encounter.");
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        encounters.SetCurrentEncounter(bass); Invoke(controller, "PrepareCreatureChoice");
        int depth = controller.CurrentDepth;
        Require(controller.PendingCreatureChoice == CreatureEffectChoiceKind.EncounterSkip
            && !controller.TryDescend() && !controller.TrySurface() && !TechniqueRuleChecks.Play(controller, 0), "A new action bypassed immediate skip offer.");
        Require(controller.TryChooseEncounterSkip(true) && controller.CurrentDepth == depth && controller.CurrentEncounter == null,
            "Skip advanced depth or retained encounter.");
        Require(controller.TryDescend() && controller.CurrentDepth == depth + 1 && controller.CurrentEncounter == bass,
            "Descend after skip failed normal advance/reveal.");
        CardInstance lobster = chain.Add(Lobster(), resolver);
        CardDefinition mussel = Card("Protected Mussel", new[] { "Armored" }); Set(mussel, "baitEffects", new[] {
            Effect(CardEffectType.CancelNextHazard, CardEffectTrigger.WhenCaught, CardEffectTarget.Self, 1) });
        runtime.RecordAttachment(chain.Add(mussel, resolver, CatchAttachmentRole.Bait));
        CardDefinition hazard = Card("Hazard", Array.Empty<string>()); Set(hazard, "cardType", CardType.Hazard);
        encounters.SetCurrentEncounter(hazard);
        Require(lobster.CurrentValue == 28 && encounters.CurrentEncounter == null, "Controller did not bind full Hazard cancellation reward.");
        runtime.RecordAttachment(chain.Add(Squid(), resolver));
        runtime.RecordAttachment(chain.Add(Squid(), resolver, CatchAttachmentRole.Bait));
        BiomeDefinition next = Biome(new[] { bass }); Set(controller.CurrentBiome, "nextBiome", next);
        BiomeApexRuntime apexRuntime = Get<BiomeApexRuntime>(controller, "biomeApexRuntime");
        Set(apexRuntime, "state", BiomeApexState.Caught); apexRuntime.RecordNextWatersPresented();
        Require(controller.TryEnterNextBiome() && controller.CurrentEncounterIdentityHidden
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.EncounterSkip,
            "Biome entry lost pending Ink or source-independent skip.");
        Require(controller.TryChooseEncounterSkip(false) && controller.CurrentEncounterIdentityHidden,
            "New-biome skip decline revealed identity.");
        controller.StartRun();
        Require(!controller.CurrentEncounterIdentityHidden && runtime.PendingCreatureSkipCount == 0,
            "Starting another run retained old Ink/skip grants.");
    }

    /// <summary>Creates approved Lobster role effects.</summary>
    private static CardDefinition Lobster()
    {
        CardDefinition card = Card("Lobster", new[] { "Armored", "Heavy" }, Effect(CardEffectType.GainValueOnHazardCancelled,
            CardEffectTrigger.AfterHazardCancelled, CardEffectTarget.Self, 4)); Set(card, "weight", 3); Set(card, "value", 24);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.ReduceOtherMatchingCreatureWeight, CardEffectTrigger.WhileAttached,
            CardEffectTarget.CatchChain, 1, new[] { "Armored" }) }); return card;
    }

    /// <summary>Creates independent negative Ink grants and once-per-card positive skips.</summary>
    private static CardDefinition Squid()
    {
        CardEffectDefinition ink = Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 2);
        Set(ink, "effectTone", CardEffectTone.Negative);
        CardDefinition card = Card("Squid", new[] { "Predator", "Elusive" }, ink);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.GrantNextCreatureSkip, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1) }); return card;
    }

    /// <summary>Creates legal Catch release and shared post-action Eel draw effects.</summary>
    private static CardDefinition Eel()
    {
        CardDefinition card = Card("Eel", new[] { "Fish", "Predator", "Elusive" }, Effect(CardEffectType.ReleaseMatchingBaitOnAttach,
            CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1, new[] { "Elusive" }));
        Set(card, "baitEffects", new[] { Effect(CardEffectType.DrawTechniqueAfterCreatureRelease,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None, 1, new[] { "Elusive" }) }); return card;
    }

    /// <summary>Prepares choices against the actual encounter runtime holding reveal reservations.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EncounterRuntime encounters,
        EffectResolver resolver, TechniqueDeckRuntime deck)
    {
        runtime.PrepareChoice(chain, encounters, null, new TechniqueEffectRuntime(), resolver, new System.Random(1), () => 0, deck);
    }

    /// <summary>Creates immutable fixture design data.</summary>
    private static CardDefinition Card(string name, string[] tags, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "tags", tags); Set(card, "effects", effects);
        Set(card, "hasExplicitBaitData", true); Set(card, "maximumDepth", 20); return card;
    }

    /// <summary>Creates a Technique fixture with controlled targeted effects.</summary>
    private static CardDefinition Technique(string name, params CardEffectDefinition[] effects)
    {
        CardDefinition card = Card(name, Array.Empty<string>(), effects); Set(card, "cardType", CardType.Technique); return card;
    }

    /// <summary>Creates exact hand and draw occurrences for deterministic tests.</summary>
    private static TechniqueDeckRuntime Deck(int handCount, int drawCount)
    {
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        CardDefinition[] hand = new CardDefinition[handCount]; CardDefinition[] draw = new CardDefinition[drawCount];
        for (int i = 0; i < hand.Length; i++) hand[i] = Technique("Hand " + i);
        for (int i = 0; i < draw.Length; i++) draw[i] = Technique("Draw " + i);
        Set(deck, "hand", hand); Set(deck, "drawPile", draw); return deck;
    }

    /// <summary>Creates a coastal biome fixture.</summary>
    private static BiomeDefinition Biome(CardDefinition[] pool)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); Set(biome, "displayName", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", pool);
        Set(biome, "depthTiers", new[] { tier }); return biome;
    }

    /// <summary>Creates serialized effect fixture data.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target,
        int amount, string[] tags = null)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "trigger", trigger);
        Set(effect, "target", target); Set(effect, "amount", amount); Set(effect, "requiredTags", tags ?? Array.Empty<string>());
        Set(effect, "displayName", type.ToString()); return effect;
    }

    /// <summary>Executes the actual deliberate release path.</summary>
    private static void Release(CatchChainRuntime chain, CardInstance card, EffectResolver resolver)
    {
        if (!chain.TryRelease(Array.IndexOf(chain.Catches, card), resolver, out _, out _, out string message))
            throw new InvalidOperationException(message);
    }

    /// <summary>Sets fixture data through the existing Unity serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads coordinator-owned runtime state for integrated action checks.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Invokes a coordinator boundary without copying its behavior.</summary>
    private static object Invoke(object owner, string name)
    {
        return owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);
    }

    /// <summary>Counts a meaningful rule assertion and reports its first failure.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++; if (!condition) throw new InvalidOperationException(message);
    }
}
