using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks deliberate creature Releases, shared recovery windows, and controller action timing.</summary>
public static class CreatureReleaseChecks
{
    private static int assertions;
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

#if UNITY_EDITOR
    /// <summary>Runs the focused suite from Unity without modifying authored definitions.</summary>
    [MenuItem("Fishing Cards/Validation/Crab and Cod")]
    public static void Run()
    {
        int count = ValidateRules();
        CardDefinition crab = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Clinging Crab.asset");
        CardDefinition cod = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");
        Require(crab.Weight == 2 && crab.Value == 14 && crab.MinimumDepth == 0 && crab.MaximumDepth == 12,
            "Crab authored stats differ from the approved design.");
        Require(cod.Weight == 2 && cod.Value == 12 && cod.MaximumDepth == 20 && cod.Effects.Length == 0,
            "Cod authored stats or empty Catch differ from the approved design.");
        Debug.Log("Crab and Cod validation passed: " + (count + 2) + " assertions.");
    }
#endif

    /// <summary>Runs gameplay checks against real runtime owners; also callable by a standalone harness.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            ValidateRolesAndRelease();
            ValidateRecovery();
            ValidateController();
            ValidateMultipleReleaseTechnique();
            return assertions;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj);
            created.Clear();
        }
    }

    /// <summary>Checks locks in the selected role, forced removal, exchanges, and one-use Claw Cut targets.</summary>
    private static void ValidateRolesAndRelease()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        CardDefinition crab = Crab();
        CardInstance locked = chain.Add(crab, resolver);
        CardInstance bait = chain.Add(crab, resolver, CatchAttachmentRole.Bait);
        Require(locked.PreventsReleaseWhileAttached && !bait.PreventsReleaseWhileAttached, "Role release locks leaked.");
        Require(!chain.TryRelease(0, resolver, out _, out _, out _), "Core Release bypassed Iron Grip.");
        CardEffectDefinition release = Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual, CardEffectTarget.CatchChain);
        Require(chain.TryApplyTechniqueEffect(release, resolver, out _) && chain.Catches.Length == 1,
            "Technique Release selected an Iron Grip Catch.");
        Require(chain.TryRelease(0, resolver, out _, out _, out _, CatchRemovalReason.LineStrain), "Forced removal was blocked.");
        locked = chain.Add(crab, resolver);
        Require(chain.ReplaceCreatureCatch(locked.InstanceId, Card("Replacement"), 1, resolver, new System.Random(2)) != null,
            "Exchange was blocked by Iron Grip.");
        chain.Reset();
        locked = chain.Add(crab, resolver);
        bait = chain.Add(crab, resolver, CatchAttachmentRole.Bait);
        CardInstance otherBait = chain.Add(Card("Other Bait"), resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(bait);
        Require(runtime.GetReleaseTargets(chain.Catches).Length == 1 && runtime.GetReleaseTargets(chain.Catches)[0] == otherBait,
            "Claw Cut target eligibility failed.");
        Require(!runtime.TryRelease(locked.InstanceId, chain, resolver) && !runtime.TryRelease(bait.InstanceId, chain, resolver),
            "Claw Cut released itself or a locked card.");
        Require(runtime.TryRelease(otherBait.InstanceId, chain, resolver) && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Claw Cut did not complete once.");
        runtime.RecordAttachment(chain.Add(crab, resolver, CatchAttachmentRole.Bait));
        chain.TryRelease(1, resolver, out _, out _, out _);
        runtime.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver, new System.Random(3), () => 0);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Unused Claw Cut was saved without a target.");
        CardDefinition empty = Card("Explicit empty Bait", Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self));
        Set(empty, "hasExplicitBaitData", true);
        Set(empty, "rulesText", "Catch only");
        Require(empty.BaitEffects.Length == 0 && string.IsNullOrEmpty(empty.BaitRulesText), "Empty role fell back to Catch.");
    }

    /// <summary>Checks shared limits, empty-discard consumption, exact discard recovery, and completed trigger lifetime.</summary>
    private static void ValidateRecovery()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        CardDefinition technique = Card("Technique"); Set(technique, "cardType", CardType.Technique);
        Set(deck, "discardPile", new[] { technique, technique });
        Set(deck, "hand", new[] { technique, technique, technique, technique });
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        chain.Add(Cod(), resolver, CatchAttachmentRole.Bait);
        chain.Add(Cod(), resolver, CatchAttachmentRole.Bait);
        CardDefinition armored = Card("Armored"); Set(armored, "tags", new[] { "Armored" });
        CardInstance target = chain.Add(armored, resolver, CatchAttachmentRole.Bait);
        chain.TryRelease(2, resolver, out _, out _, out _);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Recovery interrupted release resolution.");
        runtime.FinishReleaseResolution();
        Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RecoverTechnique, "Armored Bait release failed to trigger.");
        Require(!deck.TryRecoverDiscardedTechnique(1) && deck.Hand.Length == 4,
            "Direct recovery bypassed the four-card cap.");
        Require(runtime.TryChooseCodRecovery(1, deck)
            && runtime.ChoiceKind == CreatureEffectChoiceKind.MakeRoomForRecoveredTechnique
            && runtime.TryChooseCodRecoveryRoom(0, deck) && deck.Hand.Length == 4 && deck.DiscardPile.Length == 2,
            "Full-hand Cod recovery did not discard for room before returning the exact occurrence.");
        chain.Add(armored, resolver);
        chain.TryRelease(2, resolver, out _, out _, out _); runtime.FinishReleaseResolution();
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Multiple Cod copies or second release bypassed shared limit.");
        runtime.BeginEncounter();
        Set(deck, "discardPile", Array.Empty<CardDefinition>());
        chain.Add(armored, resolver); chain.TryRelease(2, resolver, out _, out _, out _);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty discard offered recovery.");
        Set(deck, "discardPile", new[] { technique });
        chain.Add(armored, resolver); chain.TryRelease(2, resolver, out _, out _, out _); runtime.FinishReleaseResolution();
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Empty discard failed to consume the window.");
        runtime.BeginEncounter();
        chain.Add(armored, resolver); chain.TryRelease(2, resolver, out _, out _, out _, CatchRemovalReason.LineStrain);
        runtime.FinishReleaseResolution();
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Forced removal triggered recovery.");
        CardDefinition treasure = Card("Armored Treasure"); Set(treasure, "tags", new[] { "Armored" }); Set(treasure, "cardType", CardType.Treasure);
        chain.Add(treasure, resolver); chain.TryRelease(2, resolver, out _, out _, out _); runtime.FinishReleaseResolution();
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Non-creature triggered recovery.");
        chain.Add(armored, resolver);
        CardInstance crab = chain.Add(Crab(), resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(crab);
        Require(runtime.TryRelease(chain.Catches[2].InstanceId, chain, resolver)
            && runtime.ChoiceKind == CreatureEffectChoiceKind.RecoverTechnique, "Creature Release did not trigger recovery.");
        runtime.CompleteChoice(); runtime.BeginEncounter();
        chain.Add(armored, resolver); chain.TryRelease(chain.Catches.Length - 1, resolver, out _, out _, out _);
        chain.TryRelease(0, resolver, out _, out _, out _); chain.TryRelease(0, resolver, out _, out _, out _);
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RecoverTechnique, "Removing Cod after its trigger erased recovery.");
    }

    /// <summary>Checks just-used recovery before reshuffle, encounter replacement, and genuine advancement reset.</summary>
    private static void ValidateController()
    {
        CardDefinition hooked = Card("Hooked");
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { hooked });
        Set(biome, "biomeId", "Test"); Set(biome, "depthTiers", new[] { tier });
        GameObject obj = new GameObject("Release Timing Check"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", biome); Set(controller, "startingLineCapacity", 100);
        controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        chain.Add(Cod(), resolver, CatchAttachmentRole.Bait);
        CardDefinition armored = Card("Armored"); Set(armored, "tags", new[] { "Armored" });
        chain.Add(armored, resolver);
        CardDefinition cut = Card("Cut", Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual, CardEffectTarget.SpecificCaughtCard));
        Set(cut.Effects[0], "requiredTags", new[] { "Armored" }); Set(cut, "cardType", CardType.Technique);
        Set(deck, "hand", new[] { cut });
        Require(TechniqueRuleChecks.Play(controller, 0) && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique,
            "Technique recovery did not wait for its required choice.");
        Require(deck.DiscardPile.Length == 1 && deck.DiscardPile[0] == cut && deck.Hand.Length == 0,
            "Just-used technique reshuffled before recovery.");
        Require(!controller.TryDescend() && !controller.TrySurface(), "Core action bypassed recovery.");
        Require(controller.TryChooseRecoveredTechnique(0) && deck.Hand.Length == 1 && deck.Hand[0] == cut,
            "Just-used technique could not be recovered.");
        // Technique replacement changes the Hooked card inside the same encounter window.
        CardDefinition replacement = Card("Replacement");
        Set(tier, "encounterPool", new[] { hooked, replacement });
        CardEffectDefinition replace = Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual, CardEffectTarget.HookedEncounter);
        Set(replace, "replacementCard", replacement);
        CardDefinition redirect = Card("Redirect", replace); Set(redirect, "cardType", CardType.Technique);
        Set(deck, "hand", new[] { redirect });
        Require(TechniqueRuleChecks.Play(controller, 0), "Replacement fixture was not playable.");
        chain.Add(armored, resolver);
        Require(TechniqueRuleChecks.ReleaseFixture(controller, chain.Catches.Length - 1) && !controller.HasPendingCreatureChoice,
            "Replacement reset the recovery limit.");
        Require(controller.TryDescend(), "Genuine encounter advancement failed.");
        Set(deck, "discardPile", new[] { cut }); chain.Add(armored, resolver);
        Require(TechniqueRuleChecks.ReleaseFixture(controller, chain.Catches.Length - 1)
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique, "New encounter did not reset recovery.");
    }

    /// <summary>Checks one recovery after a multi-effect Technique removes both its target and the Cod source.</summary>
    private static void ValidateMultipleReleaseTechnique()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        EncounterRuntime encounters = new EncounterRuntime();
        chain.DeliberatelyReleased += (released, reason) => runtime.RecordDeliberateRelease(released, reason, chain.Catches);
        chain.Add(Cod(), resolver, CatchAttachmentRole.Bait);
        CardDefinition armored = Card("Armored"); Set(armored, "tags", new[] { "Armored" });
        chain.Add(armored, resolver); chain.Add(armored, resolver);
        CardEffectDefinition first = Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual, CardEffectTarget.CatchChain);
        Set(first, "requiredTags", new[] { "Armored" }); Set(first, "amount", 2);
        CardEffectDefinition second = Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual, CardEffectTarget.CatchChain);
        CardDefinition used = Card("Multiple Releases", first, second); Set(used, "cardType", CardType.Technique);
        encounters.SetCurrentEncounter(Card("Hooked"));
        Set(deck, "hand", new[] { used });
        Require(TechniqueRuleChecks.DiscardFixture(deck, 0, out _, out _), "Deferred discard failed.");
        Require(techniques.ApplyCard(used, encounters, chain, resolver, Array.Empty<CardDefinition>(), Array.Empty<EncounterChainDefinition>(), "Test", 0, new System.Random(1), false, out _, TechniqueRuleChecks.FirstTargets(used, chain, encounters)), "Multi-effect Technique failed.");
        Require(chain.Catches.Length == 0 && runtime.ChoiceKind == CreatureEffectChoiceKind.None,
            "Recovery interrupted the technique or prevented later Release effects.");
        runtime.FinishReleaseResolution(); Prepare(runtime, chain, resolver, deck);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.RecoverTechnique && deck.DiscardPile[0] == used,
            "Completed recovery lost its removed Cod source or just-used card.");
        Require(deck.TryRecoverDiscardedTechnique(0), "Recovery after multiple releases failed.");
        runtime.CompleteChoice();
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Multiple armored releases queued duplicate recovery.");
    }

    /// <summary>Prepares a queued request with a controlled discard pile.</summary>
    private static void Prepare(CreatureEffectRuntime runtime, CatchChainRuntime chain, EffectResolver resolver, TechniqueDeckRuntime deck)
    {
        runtime.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver, new System.Random(1), () => 0, deck);
    }

    /// <summary>Creates the two approved Crab roles.</summary>
    private static CardDefinition Crab()
    {
        CardDefinition card = Card("Crab", Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached, CardEffectTarget.Self));
        Set(card, "hasExplicitBaitData", true);
        Set(card, "baitEffects", new[] { Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.WhenCaught, CardEffectTarget.SpecificCaughtCard) });
        return card;
    }

    /// <summary>Creates an effectless Catch and recovery Bait.</summary>
    private static CardDefinition Cod()
    {
        CardDefinition card = Card("Cod");
        CardEffectDefinition recover = Effect(CardEffectType.RecoverDiscardedTechnique, CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None);
        Set(recover, "requiredTags", new[] { "Armored" });
        Set(card, "hasExplicitBaitData", true); Set(card, "baitEffects", new[] { recover });
        return card;
    }

    /// <summary>Creates an isolated creature definition.</summary>
    private static CardDefinition Card(string name, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "effects", effects);
        return card;
    }

    /// <summary>Creates a controlled authored effect.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target)
    {
        CardEffectDefinition effect = new CardEffectDefinition();
        Set(effect, "effectType", type); Set(effect, "trigger", trigger); Set(effect, "target", target); Set(effect, "amount", 1);
        return effect;
    }

    /// <summary>Sets a private serialized fixture field.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads a controller runtime for integration checks.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Fails with the relevant gameplay rule.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
