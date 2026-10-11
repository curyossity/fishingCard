using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Validates effective tags, independent Tangle penalties, frozen Buoyant Bell targets, and deferred redraws.</summary>
public static class CoastalSupportChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;

#if UNITY_EDITOR
    /// <summary>Runs runtime checks and validates the authored refactored cards in the Unity editor.</summary>
    [MenuItem("Fishing Cards/Validation/Garfish Minnow and Jellyfish")]
    public static void Run()
    {
        ValidateRules();
        CardDefinition garfish = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Garfish.asset");
        CardDefinition minnow = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Glass Minnow.asset");
        CardDefinition jelly = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Jellyfish.asset");
        Require(garfish.Weight == 1 && garfish.Value == 9 && garfish.Effects[0].Amount == 3, "Garfish asset mismatch.");
        Require(minnow.Weight == 1 && minnow.Value == 5 && minnow.Effects.Length == 0, "Minnow asset mismatch.");
        Require(jelly.Weight == 1 && jelly.Value == 8 && jelly.Effects[0].Amount == 1, "Jellyfish asset mismatch.");
        Debug.Log("Coastal support validation passed: " + assertions + " assertions.");
    }
#endif

    /// <summary>Runs the same gameplay suite in Unity or a harness with minimal Unity API adapters.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            ValidateTagsAndValue();
            ValidateTangle();
            ValidateSelectedReductions();
            ValidateRedraw();
            ValidateController();
            return assertions;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj);
            created.Clear();
        }
    }

    /// <summary>Checks native tags, overlapping grants, value eligibility, attachment trigger ordering, and independent snapshots.</summary>
    private static void ValidateTagsAndValue()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardDefinition garfish = Garfish();
        CardInstance swift = chain.Add(garfish, resolver);
        CardDefinition fish = Card("Fish", new[] { "Fish" });
        CardInstance catchFish = chain.Add(fish, resolver);
        CardInstance baitFish = chain.Add(fish, resolver, CatchAttachmentRole.Bait);
        CardInstance otherGarfish = chain.Add(garfish, resolver, CatchAttachmentRole.Bait);
        Require(catchFish.HasTag("Elusive") && baitFish.HasTag("Elusive"), "Slipstream missed a role.");
        Require(swift.CurrentValue == 12, "Swift counted Bait or missed a granted tag.");
        Require(!Array.Exists(fish.Tags, t => t == "Elusive"), "Tag grant mutated the shared definition.");
        CardInstance native = chain.Add(Card("Native", new[] { "Fish", "Elusive" }), resolver);
        CardInstance nonFish = chain.Add(Card("Non-fish", new[] { "Small" }), resolver);
        Require(!nonFish.HasTag("Elusive"), "Slipstream tagged a non-Fish.");
        CardDefinition treasure = Card("Treasure", new[] { "Elusive", "Fish" }); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver);
        Require(swift.CurrentValue == 15, "Swift counted non-creatures or itself.");
        CardInstance secondSource = chain.Add(garfish, resolver, CatchAttachmentRole.Bait);
        Require(Array.FindAll(catchFish.Tags, t => t == "Elusive").Length == 1, "Duplicate grants created duplicate tags.");
        chain.TryRelease(Array.FindIndex(chain.Catches, c => c == otherGarfish), resolver, out _, out _, out _);
        Require(catchFish.HasTag("Elusive"), "Removing one source removed an overlapping grant.");
        CardInstance snapshot = catchFish.CreateSnapshot();
        snapshot.SetRuntimeBaseTags(new[] { "Armored" });
        Require(catchFish.HasTag("Fish"), "Snapshot tag mutations leaked.");
        chain.TryRelease(Array.FindIndex(chain.Catches, c => c == secondSource), resolver, out _, out _, out _);
        Require(!catchFish.HasTag("Elusive") && native.HasTag("Elusive"), "Ending Slipstream removed native tags or left grants.");
        Require(swift.CurrentValue == 12, "Swift failed to recalculate after grant removal.");
        catchFish.SetRuntimeBaseTags(new[] { "Fish", "Elusive" }); chain.Recalculate(resolver);
        Require(swift.CurrentValue == 15, "Runtime tag changes did not update Swift.");

        CardEffectDefinition next = Effect(CardEffectType.AddLineLoadModifier, CardEffectTrigger.WhenNextMatchingCatchCaught,
            CardEffectTarget.SpecificCaughtCard, 2, new[] { "Elusive" });
        CatchChainRuntime triggerChain = new CatchChainRuntime();
        triggerChain.Add(garfish, resolver, CatchAttachmentRole.Bait);
        triggerChain.Add(Card("Next-trigger", Array.Empty<string>(), next), resolver);
        Require(triggerChain.Add(fish, resolver).CurrentWeight == 3, "Attachment triggers ran before Slipstream grants.");
        Require(!Array.Exists(fish.Tags, tag => tag == "Elusive"), "Trigger matching changed native tags.");
        CardEffectDefinition elusiveTechnique = Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 2, new[] { "Elusive" });
        Require(triggerChain.TryApplyTechniqueEffect(elusiveTechnique, resolver, out _)
            && triggerChain.Catches[2].CurrentValue == 10, "Technique targets ignored runtime tags.");
        Require(chain.CreateResolvedCatchPreview(fish, resolver).CurrentWeight == fish.Weight
            && !catchFish.CreateSnapshot().HasTag("Armored"), "Preview mutated attached tag state.");
    }

    /// <summary>Checks independent pending effects, non-Catch exclusions, stacking, previews, cleansing, and source removal.</summary>
    private static void ValidateTangle()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CardDefinition jelly = Jellyfish();
        CardInstance source = chain.Add(jelly, resolver);
        Require(source.CurrentWeight == 1 && chain.PendingCreatureCatchEffects.Length == 1, "Tangle consumed itself or did not queue.");
        chain.TryRelease(0, resolver, out _, out _, out _);
        chain.Add(Card("Bait", Array.Empty<string>()), resolver, CatchAttachmentRole.Bait);
        CardDefinition treasure = Card("Treasure", Array.Empty<string>()); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver);
        Require(chain.PendingCreatureCatchEffects.Length == 1, "Bait or non-creature consumed Tangle.");
        CardDefinition fish = Card("Recipient", new[] { "Fish" });
        CardInstance preview = chain.CreateResolvedCatchPreview(fish, resolver);
        Require(preview.CurrentWeight == 2 && chain.PendingCreatureCatchEffects.Length == 1, "Preview consumed or omitted Tangle.");
        CardInstance recipient = chain.Add(fish, resolver);
        Require(recipient.CurrentWeight == 2 && chain.PendingCreatureCatchEffects.Length == 0, "Removed source lost its pending penalty.");
        Set(recipient, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(recipient.CurrentWeight == 2, "Recipient role change erased applied Tangle.");
        Require(recipient.CreateSnapshot().PermanentWeightModifier == 1, "Snapshot lost the applied penalty.");
        NegativeEffectOption[] negatives = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(), new TechniqueEffectRuntime(), resolver);
        Require(negatives.Length == 1 && negatives[0].TryRemove() && recipient.CurrentWeight == 1, "Quick Clean could not remove applied Tangle.");
        chain.Reset();
        CardDefinition doubled = Jellyfish(); Set(doubled, "effects", new[] { jelly.Effects[0], jelly.Effects[0] });
        chain.Add(doubled, resolver); chain.TryRelease(0, resolver, out _, out _, out _);
        Require(chain.PendingCreatureCatchEffects.Length == 2, "Pending effects did not stack independently.");
        negatives = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(), new TechniqueEffectRuntime(), resolver);
        Require(negatives.Length == 2 && negatives[0].TryRemove() && chain.PendingCreatureCatchEffects.Length == 1,
            "Pending negative cleansing was not exact.");
        Require(chain.Add(fish, resolver).CurrentWeight == 2, "Cleansed pending Tangle returned.");
        chain.Reset(); chain.Add(doubled, resolver);
        Require(chain.Add(fish, resolver).CurrentWeight == 3, "Two pending Tangles did not add together.");
        chain.Reset(); chain.Add(jelly, resolver); chain.TryRelease(0, resolver, out _, out _, out _);
        CardInstance old = chain.Add(Card("Old", Array.Empty<string>()), resolver, CatchAttachmentRole.Bait);
        Set(old, "attachmentRole", CatchAttachmentRole.Catch);
        Require(chain.ReplaceCreatureCatch(old.InstanceId, fish, 1, resolver, new System.Random(1)).CurrentWeight == 2,
            "Replacement Catch did not consume Tangle.");
    }

    /// <summary>Checks distinct optional selections, tag suspension/restoration, source removal, clamping, and no retargeting.</summary>
    private static void ValidateSelectedReductions()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        CardDefinition armored = Card("Armored", new[] { "Armored" }); Set(armored, "weight", 3);
        CardInstance first = chain.Add(armored, resolver);
        CardInstance second = chain.Add(armored, resolver, CatchAttachmentRole.Bait);
        CardDefinition treasure = Card("Armored treasure", new[] { "Armored" }); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver);
        CardInstance jelly = chain.Add(Jellyfish(), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(jelly);
        Require(runtime.GetWeightTargets(chain.Catches).Length == 2, "Buoyant Bell included a non-creature or wrong targets.");
        Require(runtime.TryChooseWeightTarget(first.InstanceId, chain, resolver)
            && runtime.ChoiceKind == CreatureEffectChoiceKind.SelectWeightTargets, "First optional selection failed.");
        Require(!runtime.TryChooseWeightTarget(first.InstanceId, chain, resolver), "Duplicate target was selectable.");
        Require(runtime.TryChooseWeightTarget(second.InstanceId, chain, resolver) && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Second distinct selection did not complete.");
        Require(first.CurrentWeight == 2 && second.CurrentWeight == 2, "Selected reductions missed Catch or Bait.");
        Require(chain.TrySetCreatureBaseTags(first.InstanceId, Array.Empty<string>(), resolver), "Attached tag mutation failed.");
        Require(first.CurrentWeight == 3 && second.CurrentWeight == 2, "Losing Armored did not suspend only that reduction.");
        chain.TrySetCreatureBaseTags(first.InstanceId, new[] { "Armored" }, resolver);
        Require(first.CurrentWeight == 2, "Regaining Armored failed to restore reduction.");
        Set(first, "attachmentRole", CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(first.CurrentWeight == 2, "Changing target role ended reduction.");
        CardInstance anotherSource = chain.Add(Jellyfish(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(anotherSource);
        runtime.TryChooseWeightTarget(first.InstanceId, chain, resolver); runtime.TryChooseWeightTarget(-1, chain, resolver);
        Require(first.CurrentWeight == 1 && second.CurrentWeight == 2, "Overlapping reductions or finishing early failed.");
        Set(armored, "weight", 1); chain.Recalculate(resolver);
        Require(first.CurrentWeight == 0, "Stacked reductions went below zero.");
        // A later weight-increasing aura must apply before the selected reductions clamp the result.
        CardEffectDefinition increase = Effect(CardEffectType.AddLineLoadModifier, CardEffectTrigger.WhileAttached, CardEffectTarget.CatchChain, 1, new[] { "Armored" });
        chain.Add(Card("Increase", Array.Empty<string>(), increase), resolver);
        Require(first.CurrentWeight == 0, "Reduction clamping depended on source order.");
        CardInstance untouched = chain.Add(armored, resolver);
        Require(untouched.CurrentWeight == 2, "Unused target selection was applied to a later attachment.");
        chain.TryRelease(Array.FindIndex(chain.Catches, c => c == first), resolver, out _, out _, out _);
        CardInstance replacement = chain.Add(armored, resolver);
        Require(replacement.CurrentWeight == 2, "Removed target was replaced by a new instance.");
        chain.TryRelease(Array.FindIndex(chain.Catches, c => c == jelly), resolver, out _, out _, out _);
        Require(second.CurrentWeight == 2, "Ending a source failed to restore selected weight.");
        CardInstance declineSource = chain.Add(Jellyfish(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(declineSource);
        Require(runtime.TryChooseWeightTarget(-1, chain, resolver) && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Declining all targets failed.");
        CardInstance copiedSource = anotherSource.CreateSnapshot();
        Require(copiedSource.IsSelectedEffectTarget(anotherSource.ActiveEffects[0], first.InstanceId), "Snapshot lost frozen targets.");
    }

    /// <summary>Checks one redraw per Catch attachment, decline/empty-hand handling, attachment ordering, and exact replacement draw.</summary>
    private static void ValidateRedraw()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        CardDefinition tech = Technique("Old"); CardDefinition replacement = Technique("Replacement");
        Set(deck, "hand", new[] { tech, tech, tech, tech, tech }); Set(deck, "drawPile", new[] { replacement });
        chain.Add(Minnow(), resolver, CatchAttachmentRole.Bait); chain.Add(Minnow(), resolver, CatchAttachmentRole.Bait);
        CardDefinition schooling = Card("Schooling", new[] { "Schooling" });
        runtime.RecordAttachment(chain.Add(schooling, resolver), false, chain.Catches);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RedrawTechnique, "Schooling Catch did not offer redraw.");
        Require(!runtime.TryChooseRedraw(99, deck, new System.Random(1)), "Invalid hand index consumed the redraw.");
        Require(runtime.TryChooseRedraw(3, deck, new System.Random(1)) && deck.Hand.Length == 5 && deck.Hand[4] == replacement,
            "Oversized hand failed to draw exactly one replacement.");
        Require(deck.DiscardPile.Length == 1 && deck.DiscardPile[0] == tech && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Redraw played the discarded card or duplicated Minnow opportunities.");
        runtime.RecordAttachment(chain.Add(schooling, resolver), false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.TryChooseRedraw(-1, deck, new System.Random(2)) && deck.Hand.Length == 5 && deck.DiscardPile.Length == 1,
            "Declining changed the hand or saved the redraw.");
        runtime.RecordAttachment(chain.Add(schooling, resolver, CatchAttachmentRole.Bait), false, chain.Catches);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Bait attachment offered a redraw.");
        runtime.RecordAttachment(chain.Add(Card("Non-schooling", Array.Empty<string>()), resolver), false, chain.Catches);
        Prepare(runtime, chain, resolver, deck); Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Non-Schooling attachment offered redraw.");
        Set(deck, "hand", Array.Empty<CardDefinition>());
        runtime.RecordAttachment(chain.Add(schooling, resolver), false, chain.Catches); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty hand did not skip redraw.");
        Set(deck, "hand", new[] { tech }); Set(deck, "drawPile", Array.Empty<CardDefinition>()); Set(deck, "discardPile", Array.Empty<CardDefinition>());
        Require(deck.TryRedrawTechnique(0, new System.Random(2)) && deck.Hand.Length == 1 && deck.Hand[0] == tech,
            "Empty draw pile did not reshuffle the just-discarded technique.");
        CardEffectDefinition clean = Effect(CardEffectType.RemoveNegativeEffect, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1);
        CardDefinition withChoice = Card("Schooling Clean", new[] { "Schooling" }, clean);
        runtime.RecordAttachment(chain.Add(withChoice, resolver), false, chain.Catches);
        Prepare(runtime, chain, resolver, deck, 1);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect, "Redraw preceded on-attach choice.");
        runtime.CompleteChoice(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RedrawTechnique, "Redraw did not follow the completed on-attach choice.");
        runtime.TryChooseRedraw(-1, deck, new System.Random(1));
        CardInstance target = chain.Add(withChoice, resolver); runtime.RecordAttachment(target, false, chain.Catches);
        runtime.CompleteChoice();
        chain.TryRelease(Array.FindIndex(chain.Catches, c => c == target), resolver, out _, out _, out _);
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Removed recipient still offered redraw.");
        chain.Reset(); runtime.Reset();
        CardInstance bait = chain.Add(Minnow(), resolver, CatchAttachmentRole.Bait);
        target = chain.Add(withChoice, resolver); runtime.RecordAttachment(target, false, chain.Catches);
        runtime.CompleteChoice(); chain.TryRelease(0, resolver, out _, out _, out _); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Removed Minnow still offered redraw.");
    }

    /// <summary>Checks redraw suspension and resume through the actual Descend coordinator.</summary>
    private static void ValidateController()
    {
        CardDefinition schooling = Card("Schooling", new[] { "Schooling" });
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { schooling });
        Set(biome, "biomeId", "Test"); Set(biome, "depthTiers", new[] { tier });
        GameObject obj = new GameObject("Support Checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", biome); Set(controller, "startingLineCapacity", 100);
        controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        chain.Add(Minnow(), resolver, CatchAttachmentRole.Bait); Set(deck, "hand", new[] { Technique("Old") });
        Require(controller.TryDescend() && controller.CurrentDepth == 0
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RedrawTechnique, "Descend did not pause for Shoal Signal.");
        Require(!controller.TryDescend() && !controller.TrySurface() && !controller.TryReleaseCatch(0), "Core action bypassed redraw.");
        Require(controller.TryChooseCreatureRedraw(-1) && controller.CurrentDepth == 1 && !controller.HasPendingCreatureChoice,
            "Declining redraw did not resume Descend.");
        Require(!controller.TryChooseCreatureRedraw(0), "A completed redraw was saved for later.");
    }

    /// <summary>Creates the approved Garfish role effects.</summary>
    private static CardDefinition Garfish()
    {
        CardDefinition card = Card("Garfish", new[] { "Fish", "Small", "Elusive" },
            Effect(CardEffectType.ModifySelfValuePerOtherCreatureCatch, CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 3, new[] { "Elusive" }));
        Set(card, "value", 9);
        CardEffectDefinition grant = Effect(CardEffectType.GrantTagToOtherCreatures, CardEffectTrigger.WhileAttached, CardEffectTarget.CatchChain, 0, new[] { "Fish" });
        Set(grant, "grantedTag", "Elusive"); Set(card, "baitEffects", new[] { grant }); return card;
    }

    /// <summary>Creates effectless Minnow Catch and delayed redraw Bait.</summary>
    private static CardDefinition Minnow()
    {
        CardDefinition card = Card("Minnow", new[] { "Fish", "Schooling", "Small" });
        Set(card, "baitEffects", new[] { Effect(CardEffectType.RedrawTechniqueAfterCatch, CardEffectTrigger.AfterAnotherCatchAttached,
            CardEffectTarget.None, 1, new[] { "Schooling" }) }); return card;
    }

    /// <summary>Creates independent Tangle and selected-target Buoyant Bell.</summary>
    private static CardDefinition Jellyfish()
    {
        CardEffectDefinition tangle = Effect(CardEffectType.QueueNextCreatureCatchWeight, CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard, 1);
        Set(tangle, "effectTone", CardEffectTone.Negative);
        CardDefinition card = Card("Jellyfish", new[] { "Small" }, tangle);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.SelectAttachedWeightTargets, CardEffectTrigger.WhenCaught,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Armored" }) }); return card;
    }

    /// <summary>Creates a Technique fixture for pile operations.</summary>
    private static CardDefinition Technique(string name)
    {
        CardDefinition card = Card(name, Array.Empty<string>()); Set(card, "cardType", CardType.Technique); return card;
    }

    /// <summary>Creates an isolated ordinary creature definition with explicit role data.</summary>
    private static CardDefinition Card(string name, string[] tags, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "tags", tags); Set(card, "effects", effects);
        Set(card, "hasExplicitBaitData", true); return card;
    }

    /// <summary>Creates controlled serialized effect data.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target, int amount, string[] tags = null)
    {
        CardEffectDefinition effect = new CardEffectDefinition();
        Set(effect, "effectType", type); Set(effect, "trigger", trigger); Set(effect, "target", target); Set(effect, "amount", amount);
        Set(effect, "requiredTags", tags ?? Array.Empty<string>()); return effect;
    }

    /// <summary>Prepares the next deferred choice without real encounter draws.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EffectResolver resolver, TechniqueDeckRuntime deck, int negatives = 0)
    {
        runtime.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver, new System.Random(1), () => negatives, deck);
    }

    /// <summary>Sets a private fixture field through its serialized contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads a runtime owner for action integration checks.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Fails with the rule that regressed.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
