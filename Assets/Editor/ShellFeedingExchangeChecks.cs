using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Validates growth timing, one-use Hazard protection, release-before-draw, and atomic technique exchanges.</summary>
public static class ShellFeedingExchangeChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs gameplay regression checks and verifies the production dual-effect assets.</summary>
    [MenuItem("Fishing Cards/Validation/Mussel Sardine and Sea Anemone")]
    public static void Run()
    {
        int count = ValidateRules();
        CardDefinition mussel = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Mussel.asset");
        CardDefinition sardine = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sardine.asset");
        CardDefinition anemone = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sea Anemone.asset");
        Require(mussel.Value == 6 && mussel.MaximumDepth == 12
            && mussel.Effects[0].EffectType == CardEffectType.GainSelfValueAfterCatch
            && mussel.BaitEffects[0].EffectType == CardEffectType.CancelNextHazard, "Mussel asset mismatch.");
        Require(sardine.Value == 7 && sardine.MaximumDepth == 14 && sardine.Effects.Length == 0
            && sardine.BaitEffects[0].EffectType == CardEffectType.ReleaseSelfToDrawTechniques, "Sardine asset mismatch.");
        Require(anemone.Value == 8 && anemone.MaximumDepth == 16
            && anemone.Effects[0].EffectType == CardEffectType.PreventTechniqueTargeting
            && anemone.BaitEffects[0].EffectType == CardEffectType.ExchangeTechniqueAfterCatch, "Anemone asset mismatch.");
        Debug.Log("Shell/Feeding/Exchange runtime validation passed: " + count + " assertions; production assets passed.");
    }
#endif

    /// <summary>Runs actual runtime sources in Unity or the adapter-based gameplay harness.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            ValidateGrowth();
            ValidateProtection();
            ValidateFeeding();
            ValidateTargeting();
            ValidateExchange();
            ValidateController();
            return assertions;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj);
            created.Clear();
        }
    }

    /// <summary>Checks growth after on-attach choices, original sources, both modes, and permanent bonus lifetime.</summary>
    private static void ValidateGrowth()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        CardInstance first = chain.Add(Mussel(), resolver);
        runtime.RecordAttachment(first, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 6, "Shell Growth counted itself.");
        CardInstance second = chain.Add(Mussel(), resolver);
        runtime.RecordAttachment(second, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 8 && second.CurrentValue == 6, "Original Mussel growth or self exclusion failed.");
        CardInstance bait = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bait, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 8 && second.CurrentValue == 6, "Armored Bait triggered Shell Growth.");
        CardEffectDefinition attach = Effect(CardEffectType.RemoveNegativeEffect, CardEffectTrigger.WhenCaught,
            CardEffectTarget.None, 1);
        CardDefinition armored = Card("Armored", new[] { "Armored" }, attach);
        CardInstance caught = chain.Add(armored, resolver); runtime.RecordAttachment(caught, false, chain.Catches);
        Prepare(runtime, chain, resolver, null, 1);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect && first.CurrentValue == 8,
            "Shell Growth ran before on-attach resolution.");
        runtime.CompleteChoice(); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 10 && second.CurrentValue == 8 && bait.CurrentValue == 6,
            "Each Mussel Catch did not gain its own single bonus.");
        Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 10 && second.CurrentValue == 8, "Growth repeated during view refresh.");
        Release(chain, caught, resolver); chain.Recalculate(resolver);
        Require(first.CurrentValue == 10, "Released trigger recipient erased growth.");
        first.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(first.CurrentValue == 10 && first.HaulValueContribution == 0, "Mussel mode change lost stored growth or sold Bait.");
        first.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(first.CurrentValue == 10, "Mussel Catch mode failed to restore stored growth.");
        caught = chain.Add(Card("Runtime Armored", Array.Empty<string>()), resolver);
        runtime.RecordAttachment(caught, false, chain.Catches);
        caught.SetRuntimeBaseTags(new[] { "Armored" }); chain.Recalculate(resolver); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 12 && second.CurrentValue == 10, "Growth missed effective tags after attachment effects.");
        CardDefinition treasure = Card("Armored treasure", new[] { "Armored" }); Set(treasure, "cardType", CardType.Treasure);
        caught = chain.Add(treasure, resolver); runtime.RecordAttachment(caught, false, chain.Catches); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 12, "Growth counted a non-creature.");
        caught = chain.Add(armored, resolver); runtime.RecordAttachment(caught, false, chain.Catches);
        runtime.CompleteChoice();
        first.SetAttachmentRole(CatchAttachmentRole.Bait); Prepare(runtime, chain, resolver);
        Require(first.CurrentValue == 12 && second.CurrentValue == 12, "Growth applied to a source that left Catch mode.");
    }

    /// <summary>Checks oldest-copy consumption, role expiry, snapshots, biomes, classifications, and cancellation before active state.</summary>
    private static void ValidateProtection()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardInstance first = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        CardInstance second = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        CardDefinition hazard = Card("Hazard", Array.Empty<string>()); Set(hazard, "cardType", CardType.Hazard);
        CardDefinition apex = Card("Apex", Array.Empty<string>()); Set(apex, "cardType", CardType.ApexEncounter);
        CardDefinition eventCard = Card("Event", Array.Empty<string>()); Set(eventCard, "cardType", CardType.Environment);
        Require(!chain.TryCancelHazard(apex) && !chain.TryCancelHazard(eventCard)
            && first.HasUnusedHazardProtection && second.HasUnusedHazardProtection, "Protection canceled a non-Hazard.");
        CardInstance copy = first.CreateSnapshot();
        Require(copy.TryConsumeHazardProtection() && first.HasUnusedHazardProtection, "Snapshot consumed real protection.");
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        runtime.BeginBiome(chain.Catches); runtime.BeginBiome(chain.Catches);
        Require(first.HasUnusedHazardProtection, "Biome entry expired protection.");
        EncounterRuntime encounters = new EncounterRuntime { HazardCancellation = chain.TryCancelHazard };
        Require(encounters.Reveal(new[] { hazard }, "Coastal", 1, new System.Random(1), chain.ActiveEffectRecords, resolver),
            "Cancelled Hazard falsely reported an empty encounter pool.");
        Require(encounters.CurrentEncounter == null && encounters.CurrentState == EncounterState.None
            && encounters.HookedEffectRecords.Length == 0 && encounters.LastCancelledHazard == hazard,
            "Cancelled Hazard entered active state.");
        Require(!first.HasUnusedHazardProtection && second.HasUnusedHazardProtection,
            "A Hazard did not consume only the oldest protection.");
        Require(encounters.RecentEncounterHistory.Length == 1, "Cancelled normal reveal failed to count its encounter draw.");
        first.SetAttachmentRole(CatchAttachmentRole.Catch); first.SetAttachmentRole(CatchAttachmentRole.Bait);
        Require(!first.HasUnusedHazardProtection, "Used protection restored on Bait return.");
        encounters.SetCurrentEncounter(hazard);
        Require(!second.HasUnusedHazardProtection && encounters.CurrentEncounter == null,
            "Second protected Mussel failed to cancel the next Hazard.");
        encounters.SetCurrentEncounter(hazard);
        Require(encounters.CurrentEncounter == hazard && encounters.LastCancelledHazard == null, "Spent protections kept canceling Hazards.");
        CardInstance unused = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        unused.SetAttachmentRole(CatchAttachmentRole.Catch); unused.SetAttachmentRole(CatchAttachmentRole.Bait);
        Require(!unused.HasUnusedHazardProtection && !chain.TryCancelHazard(hazard), "Expired protection restored on Bait return.");
        CardInstance catchMussel = chain.Add(Mussel(), resolver);
        Require(!catchMussel.HasUnusedHazardProtection, "Catch Mussel gained Bait protection.");
        catchMussel.SetAttachmentRole(CatchAttachmentRole.Bait);
        Require(catchMussel.HasUnusedHazardProtection, "First transition into Bait failed to initialize protection.");
        Release(chain, catchMussel, resolver);
        Require(!chain.TryCancelHazard(hazard), "Removed Bait canceled a later Hazard.");
        chain.Reset();
        CardInstance slot = chain.Add(Card("Old slot", Array.Empty<string>()), resolver);
        CardInstance olderBait = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        CardInstance replacement = chain.ReplaceCreatureCatch(slot.InstanceId, Mussel(), 0, resolver, new System.Random(1));
        replacement.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(chain.TryCancelHazard(hazard) && !olderBait.HasUnusedHazardProtection
            && replacement.HasUnusedHazardProtection, "Oldest protection followed a replaced slot rather than attachment age.");
        Require(replacement.CreateSnapshot().HasUnusedHazardProtection, "Snapshot lost unused protection.");
        Require(chain.TryCancelHazard(hazard) && !replacement.CreateSnapshot().HasUnusedHazardProtection,
            "Snapshot restored used protection.");

        chain.Reset(); chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        encounters.Reset(); CardDefinition hooked = Card("Hooked", Array.Empty<string>());
        encounters.SetCurrentEncounter(hooked);
        CardDefinition redirect = Technique("Redirect");
        Set(redirect, "effects", new[] { Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual,
            CardEffectTarget.CurrentEncounter, 1) });
        TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        Require(techniques.ApplyCard(redirect, encounters, chain, resolver, new[] { hazard }, null, "Coastal", 1,
            new System.Random(1), false, out _) && encounters.CurrentEncounter == null
            && encounters.LastCancelledHazard == hazard, "Technique replacement bypassed Hazard cancellation.");

        chain.Reset(); chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        encounters.Reset(); encounters.SetCurrentEncounter(hooked);
        EncounterChainDefinition authoredChain = ScriptableObject.CreateInstance<EncounterChainDefinition>();
        created.Add(authoredChain); Set(authoredChain, "sequence", new[] { hooked, hazard });
        encounters.Reveal(new[] { hooked }, "Coastal", 1, new System.Random(1), chain.ActiveEffectRecords, resolver,
            null, null, new[] { authoredChain });
        Require(encounters.Reveal(new[] { hooked }, "Coastal", 2, new System.Random(1), chain.ActiveEffectRecords, resolver)
            && encounters.CurrentEncounter == null && encounters.LastCancelledHazard == hazard,
            "Authored chain Hazard bypassed protection.");

    }

    /// <summary>Checks shared optional activation, legal sacrifice, deferred Release reactions, four-slot draws, and ordinary removals.</summary>
    private static void ValidateFeeding()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = Deck(4, 3);
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        CardInstance first = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        CardInstance second = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        CardInstance predator = chain.Add(Card("Predator", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.FeedingFrenzy
            && runtime.GetFeedingFrenzySources(chain.Catches).Length == 2, "Shared Sardine activation did not offer both eligible sources.");
        Require(!runtime.TryActivateFeedingFrenzy(predator.InstanceId, chain, resolver), "Feeding Frenzy released the Predator.");
        Require(runtime.TryActivateFeedingFrenzy(second.InstanceId, chain, resolver)
            && runtime.ChoiceKind == CreatureEffectChoiceKind.DrawTechniques && deck.Hand.Length == 4,
            "Sacrifice drew before its Release window completed.");
        Require(chain.ReleasedCatches.Length == 1 && Array.Exists(chain.Catches, c => c == first),
            "Shared activation sacrificed multiple copies.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DrawTechniques, "Full hand skipped optional discard choices.");
        CardDefinition original = deck.Hand[0];
        Require(runtime.TryChooseFeedingDraw(0, deck, new System.Random(1))
            && deck.Hand.Length == 3 && deck.DiscardPile[0] == original, "Pre-draw discard played or redrew a technique.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DrawTechniques, "One slot prevented a second optional discard.");
        Require(runtime.TryChooseFeedingDraw(-1, deck, new System.Random(1))
            && deck.Hand.Length == 4 && deck.DrawPile.Length == 2, "Feeding draw exceeded available hand space.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Multiple Sardines offered repeat activations.");
        predator = chain.Add(Card("Predator2", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryActivateFeedingFrenzy(-1, chain, resolver) && Array.Exists(chain.Catches, c => c == first),
            "Declining sacrificed Sardine.");
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declined activation was saved.");

        chain.Reset(); runtime.Reset(); deck = Deck(4, 3);
        CardDefinition lockedSardine = Sardine();
        Set(lockedSardine, "baitEffects", new[] { lockedSardine.BaitEffects[0],
            Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0) });
        first = chain.Add(lockedSardine, resolver, CatchAttachmentRole.Bait);
        second = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        predator = chain.Add(Card("Predator3", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.GetFeedingFrenzySources(chain.Catches).Length == 1
            && !runtime.TryActivateFeedingFrenzy(first.InstanceId, chain, resolver), "Prohibited release gained a draw.");
        runtime.CompleteChoice(); Release(chain, second, resolver, CatchRemovalReason.LineStrain);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && deck.Hand.Length == 4, "Forced Sardine loss activated Feeding Frenzy.");
        second = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait); Release(chain, second, resolver);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && deck.Hand.Length == 4, "External Sardine release activated Feeding Frenzy.");
        predator = chain.Add(Card("Predator4", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No legal Sardine failed to skip activation.");

        chain.Reset(); runtime.Reset(); deck = Deck(2, 3);
        first = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        predator = chain.Add(Card("Predator5", new[] { "Predator" }), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryActivateFeedingFrenzy(first.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == 1 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Two empty slots did not draw exactly two automatically.");
        chain.Reset(); runtime.Reset(); deck = Deck(4, 3);
        first = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        CardEffectDefinition delayedAttach = Effect(CardEffectType.RemoveNegativeEffect, CardEffectTrigger.WhenCaught,
            CardEffectTarget.None, 1);
        predator = chain.Add(Card("Predator on-attach", new[] { "Predator" }, delayedAttach), resolver);
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck, 1);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect,
            "Sardine activation ran before on-attach effects.");
        runtime.CompleteChoice(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryActivateFeedingFrenzy(first.InstanceId, chain, resolver)
            && runtime.TryChooseFeedingDraw(-1, deck, new System.Random(1)) && deck.Hand.Length == 4
            && deck.DrawPile.Length == 3, "Finishing a full hand drew cards without room.");
        deck = Deck(0, 0); CardDefinition recovered = Technique("Only discard");
        Set(deck, "discardPile", new[] { recovered });
        Require(deck.DrawTechniques(2, 4, new System.Random(1)) == 1 && deck.Hand.Length == 1,
            "Limited draw failed normal reshuffle or invented cards.");
        deck = Deck(5, 3);
        Require(deck.DrawTechniques(2, 4, new System.Random(1)) == 0 && deck.Hand.Length == 5,
            "Capacity-limited draw expanded an oversized recovery hand.");

        chain.Reset(); runtime.Reset(); deck = Deck(4, 3);
        CardDefinition cod = Card("Cod", Array.Empty<string>());
        Set(cod, "baitEffects", new[] { Effect(CardEffectType.RecoverDiscardedTechnique,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None, 1, new[] { "Armored" }) });
        chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        first = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        first.SetRuntimeBaseTags(new[] { "Fish", "Small", "Schooling", "Armored" }); chain.Recalculate(resolver);
        predator = chain.Add(Card("Predator6", new[] { "Predator" }), resolver);
        CardDefinition discard = Technique("Recovered"); Set(deck, "discardPile", new[] { discard });
        runtime.RecordAttachment(predator, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryActivateFeedingFrenzy(first.InstanceId, chain, resolver)
            && runtime.ChoiceKind == CreatureEffectChoiceKind.RecoverTechnique, "Sardine draws ran before Cod Release recovery.");
        Require(deck.DrawPile.Length == 3 && deck.Hand.Length == 4, "Draw occurred during Release recovery.");
        deck.TryRecoverDiscardedTechnique(0); runtime.CompleteChoice(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DrawTechniques && deck.Hand.Length == 5,
            "Completed sacrifice draw vanished when source was removed.");
        runtime.TryChooseFeedingDraw(0, deck, new System.Random(1));
        runtime.TryChooseFeedingDraw(0, deck, new System.Random(1));
        runtime.TryChooseFeedingDraw(0, deck, new System.Random(1)); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == 1, "Optional discards after recovery did not make room for two draws.");
    }

    /// <summary>Checks technique-only target immunity while leaving untargeted and creature effects available.</summary>
    private static void ValidateTargeting()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardInstance anemone = chain.Add(Anemone(), resolver);
        CardEffectDefinition targeted = Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 3, new[] { "Small" });
        Require(anemone.PreventsTechniqueTargeting && !chain.CanApplyTechniqueEffect(targeted),
            "Anemone Catch accepted a targeted technique.");
        CardInstance other = chain.Add(Card("Other Small", new[] { "Small" }), resolver);
        Set(targeted, "caughtCardTargetMode", CaughtCardTargetMode.FirstMatching);
        Require(chain.TryApplyTechniqueEffect(targeted, resolver, out _) && other.CurrentValue == 11
            && anemone.CurrentValue == 8, "Automatic targeting selected an immune Anemone.");
        CardEffectDefinition all = Effect(CardEffectType.ModifyCaughtCard, CardEffectTrigger.Manual,
            CardEffectTarget.CatchChain, 2, new[] { "Small" });
        Require(chain.TryApplyTechniqueEffect(all, resolver, out _) && anemone.CurrentValue == 10,
            "Untargeted effect incorrectly skipped Anemone.");
        CardEffectDefinition cut = Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Small" });
        Require(chain.TryApplyTechniqueEffect(cut, resolver, out _) && chain.Catches.Length == 1
            && chain.Catches[0] == anemone, "Technique release removed the immune Catch.");
        Require(!chain.CanApplyTechniqueEffect(cut), "Technique claimed an immune-only target.");
        anemone.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(!anemone.PreventsTechniqueTargeting && chain.TryApplyTechniqueEffect(targeted, resolver, out _),
            "Anemone immunity persisted outside Catch mode.");
        anemone.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(chain.TryRelease(0, resolver, out _, out _, out _, CatchRemovalReason.Creature),
            "Creature Release was blocked by technique immunity.");
        CardDefinition negativeAnemone = Anemone();
        Set(negativeAnemone.Effects[0], "effectTone", CardEffectTone.Negative);
        anemone = chain.Add(negativeAnemone, resolver);
        NegativeEffectOption[] options = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(),
            new TechniqueEffectRuntime(), resolver);
        Require(options.Length == 1 && options[0].TryRemove() && !anemone.PreventsTechniqueTargeting
            && chain.CanApplyTechniqueEffect(targeted), "Quick Clean did not remove the exact targeting restriction.");

    }

    /// <summary>Checks existing-discard selection, atomic movement, duplicate occurrences, shared copies, and decline.</summary>
    private static void ValidateExchange()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = Deck(2, 2);
        CardDefinition discarded = Technique("Existing discard");
        Set(deck, "discardPile", new[] { discarded });
        chain.Add(Anemone(), resolver, CatchAttachmentRole.Bait); chain.Add(Anemone(), resolver, CatchAttachmentRole.Bait);
        CardEffectDefinition attach = Effect(CardEffectType.RemoveNegativeEffect, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1);
        CardInstance small = chain.Add(Card("Small", new[] { "Small" }, attach), resolver);
        runtime.RecordAttachment(small, false, chain.Catches); Prepare(runtime, chain, resolver, deck, 1);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect, "Exchange ran before on-attach effects.");
        runtime.CompleteChoice(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.ExchangeTechniques, "Small Catch failed to offer exchange.");
        CardDefinition hand = deck.Hand[0]; int drawCount = deck.DrawPile.Length;
        Require(runtime.TryChooseExchangeHand(0, deck) && deck.Hand[0] == hand && deck.DiscardPile[0] == discarded,
            "Selecting the hand half moved a card.");
        Require(!runtime.TryChooseExchangeDiscard(1, deck) && deck.Hand[0] == hand,
            "Exchange selected the not-yet-discarded hand card.");
        Require(runtime.TryChooseExchangeDiscard(0, deck) && deck.Hand.Length == 2 && deck.Hand[0] == discarded
            && deck.DiscardPile.Length == 1 && deck.DiscardPile[0] == hand && deck.DrawPile.Length == drawCount,
            "Atomic exchange drew, changed hand size, or moved the wrong occurrence.");
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Multiple Anemones offered multiple swaps.");
        small = chain.Add(Card("Small2", new[] { "Small" }), resolver);
        runtime.RecordAttachment(small, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseExchangeHand(1, deck);
        Require(runtime.TryChooseExchangeHand(-1, deck) && deck.Hand[0] == discarded
            && deck.DiscardPile[0] == hand, "Declining a partially chosen exchange moved cards.");
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declined exchange was saved.");
        Set(deck, "discardPile", Array.Empty<CardDefinition>());
        small = chain.Add(Card("Small3", new[] { "Small" }), resolver);
        runtime.RecordAttachment(small, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty existing discard offered exchange.");
        Set(deck, "discardPile", new[] { discarded }); Set(deck, "hand", Array.Empty<CardDefinition>());
        small = chain.Add(Card("Small4", new[] { "Small" }), resolver);
        runtime.RecordAttachment(small, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty hand offered exchange.");
        deck = Deck(2, 2);
        Set(deck, "hand", new[] { hand, hand }); Set(deck, "discardPile", new[] { hand, discarded });
        Require(deck.TryExchangeTechniques(1, 1) && deck.Hand[0] == hand && deck.Hand[1] == discarded
            && deck.DiscardPile[0] == hand && deck.DiscardPile[1] == hand, "Exchange confused duplicate card occurrences.");
        CardDefinition nonTechnique = Card("Not technique", Array.Empty<string>());
        Set(deck, "discardPile", new[] { nonTechnique });
        Require(!deck.TryExchangeTechniques(0, 0) && deck.Hand[0] == hand, "Invalid discard half mutated the hand.");
        deck = Deck(2, 1); Set(deck, "discardPile", new[] { discarded });
        small = chain.Add(Card("Small Bait", new[] { "Small" }), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(small, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Small Bait triggered exchange.");
    }

    /// <summary>Checks real controller action blocking, sacrifice/discard resumption, exchanges, and Hazard cancellation depth.</summary>
    private static void ValidateController()
    {
        CardDefinition fish = Card("Encounter", new[] { "Predator", "Small" });
        BiomeDefinition biome = Biome(new[] { fish });
        GameObject obj = new GameObject("Shell reaction controller check"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", biome);
        Set(controller, "startingDepth", 1); Set(controller, "startingLineCapacity", 100);
        controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        CreatureEffectRuntime runtime = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        Set(deck, "hand", Deck(4, 0).Hand); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        CardInstance sardine = chain.Add(Sardine(), resolver, CatchAttachmentRole.Bait);
        Require(controller.TryDescend() && controller.CurrentDepth == 1
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.FeedingFrenzy, "Descend did not pause for Sardine.");
        Require(!controller.TryDescend() && !controller.TrySurface(), "Gameplay bypassed Sardine activation.");
        Require(controller.TryActivateFeedingFrenzy(sardine.InstanceId)
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.DrawTechniques, "Controller sacrifice missed discard window.");
        Require(controller.TryChooseFeedingDraw(0) && controller.CurrentDepth == 1, "Discard advanced depth early.");
        Require(controller.TryChooseFeedingDraw(0) && controller.CurrentDepth == 2 && !controller.HasPendingCreatureChoice
            && deck.Hand.Length == 4 && deck.DrawPile.Length == 2, "Two discards did not finish reward before normal Descend.");
        chain.Add(Anemone(), resolver, CatchAttachmentRole.Bait);
        CardDefinition discard = Technique("Exchange target"); Set(deck, "discardPile", new[] { discard });
        int originalDepth = controller.CurrentDepth;
        Require(controller.TryDescend() && controller.PendingCreatureChoice == CreatureEffectChoiceKind.ExchangeTechniques
            && controller.CurrentDepth == originalDepth, "Controller did not pause for exchange.");
        CardDefinition old = deck.Hand[0];
        Require(controller.TryChooseExchangeHand(0) && deck.Hand[0] == old, "Controller first selection moved a card.");
        Require(controller.TryChooseExchangeDiscard(0) && deck.Hand[0] == discard
            && controller.CurrentDepth == originalDepth + 1 && !controller.HasPendingCreatureChoice, "Controller exchange failed to resume.");

        chain.Reset(); runtime.Reset(); encounters.Reset();
        CardInstance mussel = chain.Add(Mussel(), resolver, CatchAttachmentRole.Bait);
        CardDefinition hazard = Card("Protected Hazard", Array.Empty<string>()); Set(hazard, "cardType", CardType.Hazard);
        BiomeDefinition hazardBiome = Biome(new[] { hazard }); Set(controller, "currentBiome", hazardBiome);
        originalDepth = controller.CurrentDepth;
        Invoke(controller, "RevealEncounterAtCurrentDepth");
        Require(controller.CurrentEncounter == null && encounters.LastCancelledHazard == hazard
            && controller.CurrentDepth == originalDepth && !mussel.HasUnusedHazardProtection,
            "Controller Hazard cancellation advanced depth or left Hazard active.");
        Require(controller.TryDescend() && controller.CurrentDepth == originalDepth + 1 && controller.CurrentEncounter == hazard,
            "Next Descend after cancellation did not advance normally to the next Hazard.");
    }

    /// <summary>Creates an approved Mussel fixture.</summary>
    private static CardDefinition Mussel()
    {
        CardDefinition card = Card("Mussel", new[] { "Small", "Armored", "Anchored" },
            Effect(CardEffectType.GainSelfValueAfterCatch, CardEffectTrigger.AfterAnotherCatchAttached, CardEffectTarget.Self, 2, new[] { "Armored" }));
        Set(card, "value", 6);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.CancelNextHazard, CardEffectTrigger.WhenCaught, CardEffectTarget.Self, 1) });
        return card;
    }

    /// <summary>Creates an approved Sardine fixture.</summary>
    private static CardDefinition Sardine()
    {
        CardDefinition card = Card("Sardine", new[] { "Fish", "Schooling", "Small" });
        Set(card, "value", 7);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.ReleaseSelfToDrawTechniques,
            CardEffectTrigger.AfterAnotherCatchAttached, CardEffectTarget.Self, 2, new[] { "Predator" }) });
        return card;
    }

    /// <summary>Creates an approved Anemone fixture.</summary>
    private static CardDefinition Anemone()
    {
        CardDefinition card = Card("Anemone", new[] { "Small", "Predator", "Anchored" },
            Effect(CardEffectType.PreventTechniqueTargeting, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0));
        Set(card, "baitEffects", new[] { Effect(CardEffectType.ExchangeTechniqueAfterCatch,
            CardEffectTrigger.AfterAnotherCatchAttached, CardEffectTarget.None, 1, new[] { "Small" }) });
        return card;
    }

    /// <summary>Creates controlled immutable card design data.</summary>
    private static CardDefinition Card(string name, string[] tags, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "tags", tags); Set(card, "effects", effects);
        Set(card, "hasExplicitBaitData", true); Set(card, "maximumDepth", 20);
        return card;
    }

    /// <summary>Creates a valid technique occurrence fixture.</summary>
    private static CardDefinition Technique(string name)
    {
        CardDefinition card = Card(name, Array.Empty<string>()); Set(card, "cardType", CardType.Technique); return card;
    }

    /// <summary>Creates separate hand/draw occurrences without depending on random initialization.</summary>
    private static TechniqueDeckRuntime Deck(int handCount, int drawCount)
    {
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        CardDefinition[] hand = new CardDefinition[handCount]; CardDefinition[] draw = new CardDefinition[drawCount];
        for (int i = 0; i < hand.Length; i++) hand[i] = Technique("Hand " + i);
        for (int i = 0; i < draw.Length; i++) draw[i] = Technique("Draw " + i);
        Set(deck, "hand", hand); Set(deck, "drawPile", draw); return deck;
    }

    /// <summary>Creates a single-tier coastal biome fixture.</summary>
    private static BiomeDefinition Biome(CardDefinition[] pool)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); Set(biome, "displayName", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", pool);
        Set(biome, "depthTiers", new[] { tier }); return biome;
    }

    /// <summary>Creates a serialized authored-effect fixture.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target,
        int amount, string[] tags = null)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type);
        Set(effect, "trigger", trigger); Set(effect, "target", target); Set(effect, "amount", amount);
        Set(effect, "requiredTags", tags ?? Array.Empty<string>()); return effect;
    }

    /// <summary>Prepares the actual choice queue in a deterministic resolution window.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EffectResolver resolver,
        TechniqueDeckRuntime deck = null, int negatives = 0)
    {
        runtime.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver,
            new System.Random(1), () => negatives, deck);
    }

    /// <summary>Exercises actual release behavior and event dispatch.</summary>
    private static void Release(CatchChainRuntime chain, CardInstance card, EffectResolver resolver,
        CatchRemovalReason reason = CatchRemovalReason.PlayerChoice)
    {
        if (!chain.TryRelease(Array.IndexOf(chain.Catches, card), resolver, out _, out _, out string message, reason))
            throw new InvalidOperationException(message);
    }

    /// <summary>Sets authored/runtime fixture fields using the Unity serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads a coordinator-owned runtime for action-level assertions.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Invokes a coordinator boundary without replicating its gameplay logic.</summary>
    private static object Invoke(object owner, string name)
    {
        return owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);
    }

    /// <summary>Counts and enforces a meaningful gameplay assertion.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
