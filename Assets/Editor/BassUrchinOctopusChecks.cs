using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks simultaneous releases, exact technique recovery, and ordered atomic role exchanges.</summary>
public static class BassUrchinOctopusChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs gameplay checks and validates the production refactored assets.</summary>
    [MenuItem("Fishing Cards/Validation/Sea Bass Urchin and Small Octopus")]
    public static void Run()
    {
        int count = ValidateRules();
        CardDefinition bass = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sea Bass.asset");
        CardDefinition urchin = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sea Urchin.asset");
        CardDefinition octopus = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Small Octopus.asset");
        Require(bass.Weight == 2 && bass.Value == 16 && bass.MinimumDepth == 4
            && bass.Effects[0].EffectType == CardEffectType.DiscardTechniqueOnAttach
            && bass.BaitEffects[0].EffectType == CardEffectType.ReleaseSelectedCreaturesOnAttach, "Bass asset mismatch.");
        Require(urchin.Weight == 1 && urchin.Value == 9 && urchin.MaximumDepth == 16
            && urchin.Effects[0].EffectType == CardEffectType.IncreaseSelfWeightWithMatchingAttachedCreature
            && urchin.BaitEffects[0].EffectType == CardEffectType.GrantSelectedTechniqueRecovery, "Urchin asset mismatch.");
        Require(octopus.Weight == 1 && octopus.Value == 9 && octopus.MinimumDepth == 6
            && octopus.Effects[0].EffectType == CardEffectType.DrawTechniqueOnAttach
            && octopus.BaitEffects[0].EffectType == CardEffectType.SwapCreatureModesOnAttach, "Octopus asset mismatch.");
        Debug.Log("Bass/Urchin/Octopus validation passed: " + count + " runtime assertions; production assets passed.");
    }
#endif
    /// <summary>Executes actual gameplay sources in Unity or the minimal Unity API adapter harness.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            ValidateBass();
            ValidateSpinesAndCleansing();
            ValidateRecovery();
            ValidateOctopus();
            ValidateController();
            return assertions;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj);
            created.Clear();
        }
    }

    /// <summary>Checks mandatory discard, distinct selections, simultaneous pre-removal values, and surviving Release observers.</summary>
    private static void ValidateBass()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = Deck(3, 3);
        CardInstance bass = chain.Add(Bass(), resolver);
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        CardDefinition chosen = deck.Hand[1];
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DiscardTechnique
            && !runtime.TryChooseRequiredDiscard(-1, deck), "Thrashing Catch allowed declining a nonempty hand.");
        Require(runtime.TryChooseRequiredDiscard(1, deck) && deck.Hand.Length == 2
            && deck.DiscardPile.Length == 1 && deck.DiscardPile[0] == chosen && deck.DrawPile.Length == 3,
            "Required discard played, drew, or selected the wrong occurrence.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Mandatory discard repeated.");
        Set(deck, "hand", Array.Empty<CardDefinition>());
        bass = chain.Add(Bass(), resolver); runtime.RecordAttachment(bass, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty hand did not skip the discard.");

        chain.Reset(); runtime.Reset();
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        CardDefinition ray = Card("Ray", new[] { "Elusive" });
        Set(ray, "baitEffects", new[] { Effect(CardEffectType.TransferReleasedCatchValue,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.SpecificCaughtCard, 1, new[] { "Elusive" }) });
        chain.Add(ray, resolver, CatchAttachmentRole.Bait);
        CardDefinition lion = Card("Lion", new[] { "Predator" }, Effect(CardEffectType.LoseValueOnCreatureRelease,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.Self, 5));
        Set(lion, "value", 30); CardInstance recipient = chain.Add(lion, resolver);
        CardDefinition swift = Card("Small Swift", new[] { "Small", "Elusive" },
            Effect(CardEffectType.ModifySelfValuePerOtherCreatureCatch, CardEffectTrigger.WhileAttached,
                CardEffectTarget.Self, 3, new[] { "Elusive" }));
        CardInstance first = chain.Add(swift, resolver);
        CardInstance second = chain.Add(swift, resolver);
        bass = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        CardInstance locked = chain.Add(Card("Locked Small", new[] { "Small" },
            Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0)), resolver);
        CardDefinition smallTreasure = Card("Small treasure", new[] { "Small" }); Set(smallTreasure, "cardType", CardType.Treasure);
        chain.Add(smallTreasure, resolver);
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.GetSelectedReleaseTargets(chain.Catches).Length == 2, "Scatter included locks, source, or non-creatures.");
        Require(!runtime.TryChooseSelectedRelease(locked.InstanceId, chain, resolver), "Scatter bypassed normal release restriction.");
        Require(runtime.TryChooseSelectedRelease(first.InstanceId, chain, resolver)
            && chain.ReleasedCatches.Length == 0 && chain.Catches.Length == 7, "First selection removed a target early.");
        Require(!runtime.TryChooseSelectedRelease(first.InstanceId, chain, resolver), "Scatter allowed duplicate selection.");
        Require(first.CurrentValue == 11 && second.CurrentValue == 11, "Simultaneous value fixture is incorrect.");
        int events = 0;
        chain.DeliberatelyReleased += (released, reason) =>
        {
            events++;
            Require(!Array.Exists(chain.Catches, c => c == first || c == second), "Release reaction saw a half-removed group.");
            Require(released.CurrentValue == 11, "Simultaneous release lost the pre-group value snapshot.");
        };
        Require(runtime.TryChooseSelectedRelease(second.InstanceId, chain, resolver) && events == 2
            && recipient.CurrentValue == 20, "Simultaneous releases missed per-creature Lion penalties.");
        Require(runtime.TransferValue == 11 && runtime.TryChooseValueTransfer(recipient.InstanceId, chain, resolver)
            && runtime.TransferValue == 11 && runtime.TryChooseValueTransfer(recipient.InstanceId, chain, resolver)
            && recipient.CurrentValue == 42, "Ray transfers used sequential post-removal values.");
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Scatter repeated on the same card.");
        bass.SetAttachmentRole(CatchAttachmentRole.Catch); bass.SetAttachmentRole(CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Mode changes restored Scatter.");

        chain.Reset(); runtime.Reset();
        // Resetting collections preserves event subscriptions; use a fresh chain to isolate the group assertions above.
        chain = new CatchChainRuntime();
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        CardDefinition cod = Card("Small Cod", new[] { "Small" });
        Set(cod, "baitEffects", new[] { Effect(CardEffectType.RecoverDiscardedTechnique,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None, 1, new[] { "Armored" }) });
        CardInstance removedObserver = chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        first = chain.Add(Card("Armored Small", new[] { "Small", "Armored" }), resolver);
        bass = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseSelectedRelease(removedObserver.InstanceId, chain, resolver);
        runtime.TryChooseSelectedRelease(first.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "A simultaneously removed Cod reacted after leaving.");
        first = chain.Add(Card("One Small", new[] { "Small" }), resolver);
        bass = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryChooseSelectedRelease(-1, chain, resolver) && Array.Exists(chain.Catches, c => c == first),
            "Declining Scatter removed a creature.");
        runtime.RecordAttachment(bass); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declined Scatter saved its activation.");
        CardInstance another = chain.Add(Card("Other Small", new[] { "Small" }), resolver);
        int count = chain.Catches.Length;
        Require(!chain.TryReleaseTogether(new[] { first.InstanceId, first.InstanceId }, resolver) && chain.Catches.Length == count,
            "Duplicate batch release partially mutated the chain.");
        another = chain.Add(Card("Locked", new[] { "Small" },
            Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0)), resolver);
        count = chain.Catches.Length;
        Require(!chain.TryReleaseTogether(new[] { first.InstanceId, another.InstanceId }, resolver) && chain.Catches.Length == count,
            "Invalid batch release removed its legal half.");
    }

    /// <summary>Checks live conditional non-stacking weight and independent cleansed effect indices across roles.</summary>
    private static void ValidateSpinesAndCleansing()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardDefinition authored = Urchin();
        CardInstance urchin = chain.Add(authored, resolver);
        Require(urchin.CurrentWeight == 1, "Bristling Spines applied without a Predator.");
        CardInstance predator = chain.Add(Card("Predator", new[] { "Predator" }), resolver, CatchAttachmentRole.Bait);
        Require(urchin.CurrentWeight == 2, "Predator Bait did not activate spines.");
        CardInstance extra = chain.Add(Card("Extra Predator", new[] { "Predator" }), resolver);
        Require(urchin.CurrentWeight == 2, "Extra Predators stacked spines.");
        Release(chain, extra, resolver); predator.SetRuntimeBaseTags(new[] { "Fish" }); chain.Recalculate(resolver);
        Require(urchin.CurrentWeight == 1, "Spines ignored Predator tag loss.");
        predator.SetRuntimeBaseTags(new[] { "Predator" }); chain.Recalculate(resolver);
        predator.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(urchin.CurrentWeight == 2, "Spines lost eligibility across Predator mode changes.");
        CardInstance preview = chain.CreateResolvedCatchPreview(authored, resolver);
        Require(preview.CurrentWeight == 2 && urchin.CurrentWeight == 2, "Spines preview missed penalty or mutated attached state.");
        NegativeEffectOption[] options = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(), new TechniqueEffectRuntime(), resolver);
        Require(options.Length == 1 && options[0].TryRemove() && urchin.CurrentWeight == 1,
            "Quick Clean could not remove the active spine penalty.");
        urchin.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(urchin.ActiveEffects[0] == authored.BaitEffects[0], "Catch cleansing removed the Bait effect at the same index.");
        CreatureEffectRuntime runtime = new CreatureEffectRuntime(); runtime.RecordAttachment(urchin);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 1, "Cleansed Urchin failed to grant Bait recovery.");
        CardInstance snapshot = urchin.CreateSnapshot();
        snapshot.SetAttachmentRole(CatchAttachmentRole.Catch);
        Require(snapshot.ActiveEffects[0] == null && urchin.ActiveEffects[0] != null,
            "Snapshot lost or leaked role-specific cleansing.");
        urchin.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(urchin.ActiveEffects[0] == null && urchin.CurrentWeight == 1, "Returning to Catch restored cleansed spines.");
        urchin.SetAttachmentRole(CatchAttachmentRole.Bait); runtime.RecordAttachment(urchin);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 1, "Returning to Bait granted another recovery.");
        chain.Reset(); urchin = chain.Add(Urchin(), resolver);
        CardDefinition treasure = Card("Predator treasure", new[] { "Predator" }); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver);
        Require(urchin.CurrentWeight == 1, "Spines counted a non-creature.");
        urchin.SetRuntimeBaseTags(new[] { "Small", "Armored", "Predator" }); chain.Recalculate(resolver);
        Require(urchin.CurrentWeight == 2, "An attached Urchin with a runtime Predator tag failed to qualify.");
    }

    /// <summary>Checks FIFO grants, selected-target eligibility, exact discard occurrences and uncapped recovery ordering.</summary>
    private static void ValidateRecovery()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        chain.TechniqueTargetSelected += runtime.RecordSelectedTechniqueTarget;
        CardDefinition firstUrchin = Urchin(); Set(firstUrchin.BaitEffects[0], "displayName", "Oldest Grip");
        CardDefinition secondUrchin = Urchin(); Set(secondUrchin.BaitEffects[0], "displayName", "Next Grip");
        CardInstance first = chain.Add(firstUrchin, resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(first);
        CardInstance second = chain.Add(secondUrchin, resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(second);
        first.SetAttachmentRole(CatchAttachmentRole.Catch); Release(chain, first, resolver); Release(chain, second, resolver);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 2, "Source mode/removal erased pending recovery.");
        CardInstance armor = chain.Add(Card("Armor", new[] { "Armored" }), resolver, CatchAttachmentRole.Bait);
        CardDefinition technique = Technique("Modify", Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 2, new[] { "Armored" }));
        TechniqueDeckRuntime deck = Deck(4, 3); Set(deck, "hand", new[] { technique, Technique("A"), Technique("B"), Technique("C"), Technique("D") });
        Play(runtime, deck, chain, resolver, technique);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 1 && runtime.ChoiceKind == CreatureEffectChoiceKind.None
            && deck.Hand.Length == 5 && Array.Exists(deck.Hand, c => c == technique), "Oldest grant did not recover the exact play.");
        Require(armor.CurrentValue == 10, "Targeted technique effect failed.");
        Set(deck, "hand", new[] { technique, Technique("E"), Technique("F"), Technique("G"), Technique("H") });
        Play(runtime, deck, chain, resolver, technique);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 0 && deck.Hand.Length == 5
            && runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Next grant failed uncapped recovery.");
        runtime.Reset(); first = chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(first);
        runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        deck = Deck(0, 2);
        Set(deck, "hand", new[] { technique, Technique("OA"), Technique("OB"), Technique("OC"), Technique("OD"), Technique("OE") });
        Play(runtime, deck, chain, resolver, technique);
        Require(deck.Hand.Length == 6 && runtime.ChoiceKind == CreatureEffectChoiceKind.None
            && runtime.PendingSelectedTechniqueRecoveryCount == 0,
            "Recovery above six failed or offered an unnecessary discard.");
        runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        CardInstance otherArmor = chain.Add(Card("Armor2", new[] { "Armored" }), resolver);
        CardDefinition untargeted = Technique("Untargeted", Effect(CardEffectType.ModifyCaughtCard, CardEffectTrigger.Manual,
            CardEffectTarget.CatchChain, 1, new[] { "Armored" }));
        deck = Deck(1, 1); Set(deck, "hand", new[] { untargeted }); Play(runtime, deck, chain, resolver, untargeted);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 1 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "An untargeted distributed technique consumed recovery.");
        CardDefinition hookedOnly = Technique("Avoid", Effect(CardEffectType.AvoidEncounter, CardEffectTrigger.Manual,
            CardEffectTarget.HookedEncounter, 1));
        Set(deck, "hand", new[] { hookedOnly });
        Play(runtime, deck, chain, resolver, hookedOnly);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 1, "Hooked-only targeting consumed recovery.");
        CardDefinition release = Technique("Release", Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Armored" }));
        Set(deck, "hand", new[] { release }); Play(runtime, deck, chain, resolver, release);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 0 && deck.Hand.Length == 1 && deck.Hand[0] == release
            && !Array.Exists(chain.Catches, c => c == otherArmor), "Releasing the Armored target prevented exact recovery.");

        runtime.Reset(); first = chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(first);
        // Two identical definitions in discard must still be distinguishable occurrences.
        deck = Deck(0, 2); Set(deck, "discardPile", new[] { technique }); Set(deck, "hand", new[] { technique });
        TechniqueRuleChecks.DiscardFixture(deck, 0, out _, out _);
        int playedId = deck.LastDiscardOccurrenceId;
        runtime.BeginTechniquePlay(technique, playedId, chain.Catches);
        chain.TryApplyTechniqueEffect(technique.Effects[0], resolver, out _);
        Require(runtime.PendingSelectedTechniqueRecoveryCount == 0, "Selected Armored Bait did not qualify.");
        deck.TryRecoverDiscardedTechnique(1); // Another completed effect already returned the played occurrence.
        runtime.FinishTechniquePlay(); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 1 && deck.DiscardPile.Length == 1
            && !deck.HasDiscardedTechniqueOccurrence(playedId, technique) && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Missing played occurrence recovered an identical unplayed copy.");

        runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        deck = Deck(0, 1); Set(deck, "discardPile", new[] { technique }); Set(deck, "hand", new[] { technique });
        TechniqueRuleChecks.DiscardFixture(deck, 0, out _, out _); playedId = deck.LastDiscardOccurrenceId;
        runtime.BeginTechniquePlay(technique, playedId, chain.Catches);
        chain.TryApplyTechniqueEffect(technique.Effects[0], resolver, out _);
        deck.TryRecoverDiscardedTechnique(0); // Recovering the older identical copy must preserve the played token.
        runtime.FinishTechniquePlay(); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 2 && deck.DiscardPile.Length == 0,
            "Recovering an older duplicate erased the played occurrence identity.");

        runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        deck = Deck(0, 0); Set(deck, "hand", new[] { technique });
        TechniqueRuleChecks.DiscardFixture(deck, 0, out _, out _); playedId = deck.LastDiscardOccurrenceId;
        runtime.BeginTechniquePlay(technique, playedId, chain.Catches); chain.TryApplyTechniqueEffect(technique.Effects[0], resolver, out _);
        deck.DrawTechniques(1, new System.Random(1)); // The play has been reshuffled/drawn by an earlier effect.
        runtime.FinishTechniquePlay(); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 1 && runtime.PendingSelectedTechniqueRecoveryCount == 0
            && !deck.HasDiscardedTechniqueOccurrence(playedId, technique), "Reshuffled played card produced a second recovery.");

        runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        armor = chain.Add(Card("Eligibility", new[] { "Armored" }), resolver);
        CardDefinition anyTarget = Technique("Any target", Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 1));
        deck = Deck(0, 1); Set(deck, "hand", new[] { anyTarget }); TechniqueRuleChecks.DiscardFixture(deck, 0, out _, out _);
        runtime.BeginTechniquePlay(anyTarget, deck.LastDiscardOccurrenceId, chain.Catches);
        armor.SetRuntimeBaseTags(Array.Empty<string>()); chain.Recalculate(resolver);
        chain.TryApplyTechniqueEffect(anyTarget.Effects[0], resolver, out _);
        runtime.FinishTechniquePlay(); Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 1 && deck.Hand[0] == anyTarget, "Eligibility was checked after play-time tags changed.");
    }

    /// <summary>Checks single immediate draw choices, atomic role selection, ongoing effects, and one-use limits.</summary>
    private static void ValidateOctopus()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = Deck(0, 3);
        CardInstance octopus = chain.Add(Octopus(), resolver); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 1 && deck.DrawPile.Length == 2 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Clever Catch failed to draw exactly one immediately.");
        deck = Deck(4, 3); octopus = chain.Add(Octopus(), resolver); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && deck.Hand.Length == 5 && deck.DrawPile.Length == 2,
            "Clever Catch failed to draw above four without discarding.");
        octopus = chain.Add(Octopus(), resolver); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(deck.Hand.Length == 6 && deck.DrawPile.Length == 1 && deck.DiscardPile.Length == 0,
            "Repeated Clever Catch did not grant exactly one.");

        chain.Reset(); runtime.Reset(); deck = Deck(4, 4);
        CardInstance catchBass = chain.Add(Bass(), resolver);
        CardInstance baitBass = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        // Consume its original Scatter and leave it as an already-used Bait.
        runtime.RecordAttachment(baitBass, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        if (runtime.ChoiceKind == CreatureEffectChoiceKind.SelectReleaseTargets) runtime.TryChooseSelectedRelease(-1, chain, resolver);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.SwapCreatureModes
            && runtime.GetModeSwapTargets(chain, false).Length == 1
            && runtime.GetModeSwapTargets(chain, true).Length == 1, "Helping Arms included itself or missed a pair.");
        int catchPosition = Array.IndexOf(chain.Catches, catchBass); int baitPosition = Array.IndexOf(chain.Catches, baitBass);
        catchBass.AddPermanentModifiers(2, 7); chain.Recalculate(resolver);
        Require(!runtime.TryChooseModeSwapBait(baitBass.InstanceId, chain, resolver), "Swap proceeded without its Catch half.");
        Require(runtime.TryChooseModeSwapCatch(catchBass.InstanceId, chain) && !catchBass.IsBait && baitBass.IsBait,
            "First pair selection changed a role.");
        Require(runtime.TryChooseModeSwapBait(baitBass.InstanceId, chain, resolver)
            && catchBass.IsBait && !baitBass.IsBait && Array.IndexOf(chain.Catches, catchBass) == catchPosition
            && Array.IndexOf(chain.Catches, baitBass) == baitPosition && catchBass.PermanentValueModifier == 7
            && chain.ReleasedCatches.Length == 0, "Pair swap moved cards, released cards, or reset lasting modifiers.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.DiscardTechnique, "New Catch's on-attach effect did not run first.");
        runtime.TryChooseRequiredDiscard(0, deck); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.SelectReleaseTargets
            && runtime.GetSelectedReleaseTargets(chain.Catches).Length == 1,
            "New Bait did not activate after the new Catch finished.");
        runtime.TryChooseSelectedRelease(-1, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Declining the new Bait activation did not complete the swap.");
        Require(!catchBass.TryConsumeAttachmentActivation(0), "New Bait Scatter did not consume one-per-card activation.");
        octopus.SetAttachmentRole(CatchAttachmentRole.Catch); octopus.SetAttachmentRole(CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Returning Octopus to Bait refreshed Helping Arms.");

        chain.Reset(); runtime.Reset(); deck = Deck(1, 4);
        CardDefinition grant = Card("Garfish", new[] { "Fish", "Elusive" });
        CardEffectDefinition tagGrant = Effect(CardEffectType.GrantTagToOtherCreatures, CardEffectTrigger.WhileAttached,
            CardEffectTarget.CatchChain, 1, new[] { "Fish" }); Set(tagGrant, "grantedTag", "Elusive");
        Set(grant, "baitEffects", new[] { tagGrant });
        CardInstance catchFish = chain.Add(Card("Fish Catch", new[] { "Fish" }), resolver);
        CardInstance baitGrant = chain.Add(grant, resolver, CatchAttachmentRole.Bait);
        Require(catchFish.HasTag("Elusive"), "Role-swap tag grant fixture failed.");
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseModeSwapCatch(catchFish.InstanceId, chain);
        runtime.TryChooseModeSwapBait(baitGrant.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(!catchFish.HasTag("Elusive") && baitGrant.HasTag("Elusive"), "Old ongoing grant survived or erased a native tag.");

        chain.Reset(); runtime.Reset();
        CardDefinition mussel = Card("Mussel", new[] { "Small", "Armored" });
        Set(mussel, "baitEffects", new[] { Effect(CardEffectType.CancelNextHazard, CardEffectTrigger.WhenCaught, CardEffectTarget.Self, 1) });
        CardInstance catchUrchin = chain.Add(Urchin(), resolver);
        CardInstance baitMussel = chain.Add(mussel, resolver, CatchAttachmentRole.Bait);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseModeSwapCatch(catchUrchin.InstanceId, chain);
        runtime.TryChooseModeSwapBait(baitMussel.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(!baitMussel.HasUnusedHazardProtection && runtime.PendingSelectedTechniqueRecoveryCount == 1,
            "Mode swap failed protection expiry or first Urchin grant.");
        Require(!catchUrchin.TryConsumeAttachmentActivation(0), "Urchin grant was not recorded as spent.");

        chain.Reset(); runtime.Reset();
        CardInstance onlyCatch = chain.Add(Card("Catch", Array.Empty<string>()), resolver);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && !octopus.TryConsumeAttachmentActivation(0),
            "No eligible pair left Helping Arms available for later.");
        chain.Add(Card("Late Bait", Array.Empty<string>()), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "A late Bait restored the skipped activation.");

        chain.Reset(); runtime.Reset();
        onlyCatch = chain.Add(Card("Catch", Array.Empty<string>()), resolver);
        CardInstance onlyBait = chain.Add(Card("Bait", Array.Empty<string>()), resolver, CatchAttachmentRole.Bait);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck); runtime.TryChooseModeSwapCatch(onlyCatch.InstanceId, chain);
        Require(runtime.TryChooseModeSwapCatch(-1, chain) && !onlyCatch.IsBait && onlyBait.IsBait,
            "Declining a partially chosen pair changed modes.");
        CardDefinition treasure = Card("Treasure", Array.Empty<string>()); Set(treasure, "cardType", CardType.Treasure);
        CardInstance nonCreature = chain.Add(treasure, resolver);
        Require(!chain.CanChangeCreatureMode(nonCreature) && !chain.TrySwapCreatureModes(nonCreature.InstanceId, onlyBait.InstanceId, resolver),
            "Non-creature could change modes.");

        // The approved swap activates only own on-attach effects, not other cards' after-catching reactions.
        chain.Reset(); runtime.Reset(); deck = Deck(0, 4);
        CardDefinition growth = Card("Growth", new[] { "Armored" }, Effect(CardEffectType.GainSelfValueAfterCatch,
            CardEffectTrigger.AfterAnotherCatchAttached, CardEffectTarget.Self, 2, new[] { "Armored" }));
        CardInstance observer = chain.Add(growth, resolver);
        onlyCatch = chain.Add(Card("Plain", Array.Empty<string>()), resolver);
        onlyBait = chain.Add(Card("Armored Bait", new[] { "Armored" }), resolver, CatchAttachmentRole.Bait);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck); runtime.TryChooseModeSwapCatch(onlyCatch.InstanceId, chain);
        runtime.TryChooseModeSwapBait(onlyBait.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(observer.CurrentValue == 8, "Mode swap counted a fresh catch for Shell Growth.");
        CardInstance priorCatch = onlyCatch;
        CardDefinition jelly = Card("Jelly", new[] { "Small" }, Effect(CardEffectType.QueueNextCreatureCatchWeight,
            CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1));
        onlyCatch = chain.Add(jelly, resolver);
        Require(chain.PendingCreatureCatchEffects.Length == 1, "Pending Tangle fixture failed.");
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(octopus, false, chain.Catches);
        Prepare(runtime, chain, resolver, deck); runtime.TryChooseModeSwapCatch(onlyCatch.InstanceId, chain);
        Require(runtime.TryChooseModeSwapBait(priorCatch.InstanceId, chain, resolver), "Valid pending-effect pair failed.");
        Prepare(runtime, chain, resolver, deck);
        Require(chain.PendingCreatureCatchEffects.Length == 1 && priorCatch.CurrentWeight == 1,
            "A role swap consumed another card's pending next-attachment penalty.");
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseModeSwapCatch(priorCatch.InstanceId, chain);
        runtime.TryChooseModeSwapBait(onlyCatch.InstanceId, chain, resolver); Prepare(runtime, chain, resolver, deck);
        Require(chain.PendingCreatureCatchEffects.Length == 2,
            "New Jellyfish Catch failed to run its own on-attach Tangle before the Bait activation.");

        chain.Reset(); runtime.Reset(); deck = Deck(1, 3);
        CardDefinition cutCatch = Card("Catch release", Array.Empty<string>(),
            Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1));
        CardInstance oldCatch = chain.Add(Bass(), resolver);
        CardInstance oldBait = chain.Add(cutCatch, resolver, CatchAttachmentRole.Bait);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseModeSwapCatch(oldCatch.InstanceId, chain); runtime.TryChooseModeSwapBait(oldBait.InstanceId, chain, resolver);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.ReleaseAttachedCard,
            "The new Catch's immediate creature choice failed to resolve first.");
        Require(runtime.TryRelease(oldCatch.InstanceId, chain, resolver), "New Catch failed to remove the prospective new Bait.");
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && chain.ReleasedCatches.Length == 1,
            "Removed new Bait still activated its on-attach ability.");

        chain.Reset(); runtime.Reset();
        CardDefinition crab = Card("Crab", new[] { "Armored" },
            Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 0));
        Set(crab, "baitEffects", new[] { Effect(CardEffectType.ReleaseCatch,
            CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1) });
        CardInstance usedCrab = chain.Add(crab, resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(usedCrab, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        oldCatch = chain.Add(Card("Other creature", Array.Empty<string>()), resolver);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryChooseModeSwapCatch(oldCatch.InstanceId, chain)
            && runtime.TryChooseModeSwapBait(usedCrab.InstanceId, chain, resolver), "Release restriction incorrectly prohibited a mode swap.");
        Prepare(runtime, chain, resolver, deck);
        octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        runtime.TryChooseModeSwapCatch(usedCrab.InstanceId, chain); runtime.TryChooseModeSwapBait(oldCatch.InstanceId, chain, resolver);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && chain.ReleasedCatches.Length == 0,
            "Returning a spent Crab to Bait refreshed Claw Cut.");

    }

    /// <summary>Checks real coordinator choices, exact recovery without extra draws, and mode exchange without encounter/depth changes.</summary>
    private static void ValidateController()
    {
        CardDefinition bass = Bass(); BiomeDefinition biome = Biome(new[] { bass });
        GameObject obj = new GameObject("Bass role coordinator checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", biome);
        Set(controller, "startingDepth", 4); Set(controller, "startingLineCapacity", 100); controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        CreatureEffectRuntime runtime = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        Set(deck, "hand", Deck(4, 0).Hand); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        CardDefinition old = deck.Hand[0];
        Require(controller.TryChooseEncounterRole(EncounterEffectChoice.Catch), "Fixture must explicitly choose Catch before Descend.");
        Require(controller.TryDescend() && controller.CurrentDepth == 4
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique, "Bass Descend failed to pause for discard.");
        Require(!controller.TrySurface() && !controller.TryDescend(), "Core action bypassed mandatory discard.");
        Require(controller.TryChooseRequiredTechniqueDiscard(0) && controller.CurrentDepth == 5
            && !controller.HasPendingCreatureChoice && deck.DiscardPile[0] == old, "Discard failed to resume normal Descend.");
        chain.Reset(); runtime.Reset();
        CardInstance bait = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        CardInstance one = chain.Add(Card("Small", new[] { "Small" }), resolver);
        CardInstance two = chain.Add(Card("Small2", new[] { "Small" }), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bait, false, chain.Catches); Invoke(controller, "PrepareCreatureChoice");
        int depth = controller.CurrentDepth; CardDefinition encounter = controller.CurrentEncounter;
        Require(controller.TryChooseSelectedCreatureRelease(one.InstanceId) && chain.ReleasedCatches.Length == 0,
            "Controller first Scatter choice removed early.");
        Require(controller.TryChooseSelectedCreatureRelease(two.InstanceId) && chain.ReleasedCatches.Length == 2
            && controller.CurrentDepth == depth && controller.CurrentEncounter == encounter, "Controller group release changed encounter/depth.");

        chain.Reset(); runtime.Reset(); runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        chain.Add(Card("Armored", new[] { "Armored" }), resolver);
        CardDefinition technique = Technique("Armored technique", Effect(CardEffectType.ModifyCatchValue,
            CardEffectTrigger.Manual, CardEffectTarget.SpecificCaughtCard, 1, new[] { "Armored" }));
        Set(deck, "hand", new[] { technique, Technique("A"), Technique("B"), Technique("C") });
        Set(deck, "discardPile", Array.Empty<CardDefinition>()); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        int drawCount = deck.DrawPile.Length;
        Require(TechniqueRuleChecks.Play(controller, 0) && deck.Hand.Length == 4 && deck.Hand[3] == technique
            && deck.DrawPile.Length == drawCount && runtime.PendingSelectedTechniqueRecoveryCount == 0,
            "Exact recovery changed the draw pile or left a pending recovery.");
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        chain.Reset(); runtime.Reset();
        runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        encounters.SetCurrentEncounter(Card("Armored Hooked", new[] { "Armored" }));
        CardDefinition hookedOnly = Technique("Hooked avoidance", Effect(CardEffectType.AvoidEncounter,
            CardEffectTrigger.Manual, CardEffectTarget.HookedEncounter, 1, new[] { "Armored" }));
        Set(deck, "hand", new[] { hookedOnly }); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        Require(TechniqueRuleChecks.Play(controller, 0) && runtime.PendingSelectedTechniqueRecoveryCount == 1,
            "Controller Hooked-only targeting consumed Spiny Grip.");
        encounters.SetCurrentEncounter(bass);

        chain.Reset(); runtime.Reset();
        runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        CardDefinition cod = Card("Cod", Array.Empty<string>());
        Set(cod, "baitEffects", new[] { Effect(CardEffectType.RecoverDiscardedTechnique,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None, 1, new[] { "Armored" }) });
        chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        chain.Add(Card("Armored release target", new[] { "Armored" }), resolver);
        CardDefinition releaseTech = Technique("Armored release", Effect(CardEffectType.ReleaseCatch,
            CardEffectTrigger.Manual, CardEffectTarget.SpecificCaughtCard, 1, new[] { "Armored" }));
        CardDefinition priorDiscard = Technique("Prior discard");
        Set(deck, "hand", new[] { releaseTech, Technique("A"), Technique("B"), Technique("C") });
        Set(deck, "discardPile", new[] { priorDiscard }); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        drawCount = deck.DrawPile.Length;
        Require(TechniqueRuleChecks.Play(controller, 0) && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique
            && deck.DrawPile.Length == drawCount, "Spiny Grip or an unearned draw ran before Cod's release reaction.");
        Require(controller.TryChooseRecoveredTechnique(0) && !controller.HasPendingCreatureChoice
            && deck.Hand.Length == 5 && deck.Hand[4] == releaseTech && deck.DrawPile.Length == drawCount,
            "Cod then Urchin recovery failed or drew an unearned card.");

        chain.Reset(); runtime.Reset();
        runtime.RecordAttachment(chain.Add(Urchin(), resolver, CatchAttachmentRole.Bait));
        chain.Add(cod, resolver, CatchAttachmentRole.Bait); chain.Add(Card("Armored target2", new[] { "Armored" }), resolver);
        Set(deck, "hand", new[] { releaseTech, Technique("D"), Technique("E"), Technique("F") });
        Set(deck, "discardPile", new[] { releaseTech }); Set(deck, "drawPile", Deck(0, 4).DrawPile);
        Require(TechniqueRuleChecks.Play(controller, 0) && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique,
            "Duplicate-occurrence controller fixture failed.");
        Require(controller.TryChooseRecoveredTechnique(1) && !controller.HasPendingCreatureChoice
            && deck.DiscardPile.Length == 1 && deck.DiscardPile[0] == releaseTech && deck.Hand.Length == 4,
            "Spiny Grip substituted an older identical occurrence already bypassed by Cod.");


        chain.Reset(); runtime.Reset();
        CardInstance catchUrchin = chain.Add(Urchin(), resolver);
        CardInstance baitBass = chain.Add(Bass(), resolver, CatchAttachmentRole.Bait);
        CardInstance octopus = chain.Add(Octopus(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(octopus, false, chain.Catches); Invoke(controller, "PrepareCreatureChoice");
        depth = controller.CurrentDepth; encounter = controller.CurrentEncounter;
        Require(controller.TryChooseModeSwapCatch(catchUrchin.InstanceId) && !catchUrchin.IsBait,
            "Controller mode selection changed its first half.");
        Require(controller.TryChooseModeSwapBait(baitBass.InstanceId)
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique, "New Catch did not run first through controller.");
        Require(controller.TryChooseRequiredTechniqueDiscard(0) && catchUrchin.IsBait && !baitBass.IsBait
            && runtime.PendingSelectedTechniqueRecoveryCount == 1 && controller.CurrentDepth == depth
            && controller.CurrentEncounter == encounter && !controller.HasPendingCreatureChoice,
            "Controller swap failed new Bait activation or changed encounter/depth.");
    }

    /// <summary>Creates approved Bass role effects.</summary>
    private static CardDefinition Bass()
    {
        CardDefinition card = Card("Bass", new[] { "Fish", "Predator" },
            Effect(CardEffectType.DiscardTechniqueOnAttach, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1));
        Set(card, "weight", 2); Set(card, "value", 16);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.ReleaseSelectedCreaturesOnAttach,
            CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 2, new[] { "Small" }) }); return card;
    }

    /// <summary>Creates approved Urchin conditional penalty and independent recovery grant.</summary>
    private static CardDefinition Urchin()
    {
        CardEffectDefinition spines = Effect(CardEffectType.IncreaseSelfWeightWithMatchingAttachedCreature,
            CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 1, new[] { "Predator" });
        Set(spines, "effectTone", CardEffectTone.Negative);
        CardDefinition card = Card("Urchin", new[] { "Small", "Armored", "Anchored" }, spines);
        Set(card, "value", 9);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.GrantSelectedTechniqueRecovery,
            CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1, new[] { "Armored" }) }); return card;
    }

    /// <summary>Creates approved Octopus immediate draw and one-time pair swap.</summary>
    private static CardDefinition Octopus()
    {
        CardDefinition card = Card("Octopus", new[] { "Small", "Predator", "Elusive" },
            Effect(CardEffectType.DrawTechniqueOnAttach, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1));
        Set(card, "value", 9);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.SwapCreatureModesOnAttach,
            CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1) }); return card;
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

    /// <summary>Prepares actual creature requests deterministically.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EffectResolver resolver, TechniqueDeckRuntime deck)
    {
        runtime.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver, new System.Random(1), () => 0, deck);
    }

    /// <summary>Exercises discard, selected targets, completion ordering, and pending recovery without imitating pile mutations.</summary>
    private static void Play(CreatureEffectRuntime runtime, TechniqueDeckRuntime deck, CatchChainRuntime chain,
        EffectResolver resolver, CardDefinition technique)
    {
        if (!TechniqueRuleChecks.DiscardFixture(deck, Array.IndexOf(deck.Hand, technique), out _, out string message))
            throw new InvalidOperationException(message);
        runtime.BeginTechniquePlay(technique, deck.LastDiscardOccurrenceId, chain.Catches);
        foreach (CardEffectDefinition effect in technique.Effects) chain.TryApplyTechniqueEffect(effect, resolver, out _);
        runtime.FinishReleaseResolution(); runtime.FinishTechniquePlay(); Prepare(runtime, chain, resolver, deck);
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
