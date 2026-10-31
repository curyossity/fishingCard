using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks event/cancelled windows, authored Technique targets, and coordinator recovery/completion order.</summary>
public static class EventTechniqueChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs Technique window integration checks in Unity.</summary>
    [MenuItem("Fishing Cards/Validation/Event Technique Windows")]
    public static void Run() { Debug.Log("Event Technique windows passed: " + ValidateRules() + " assertions."); }
#endif
    /// <summary>Executes actual validation and controller paths, releasing fixtures afterward.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateTargets(); ValidateRevealGuards(); ValidateController(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks attached and delayed effects, missing encounters, tag restrictions, and all lifecycle phases.</summary>
    private static void ValidateTargets()
    {
        EncounterRuntime encounters = new EncounterRuntime(); CatchChainRuntime chain = new CatchChainRuntime();
        TechniqueEffectRuntime techniques = new TechniqueEffectRuntime(); EffectResolver resolver = new EffectResolver();
        CardDefinition environment = Event(CardType.Environment); CardDefinition alternate = Event(CardType.Opportunity);
        CardDefinition delayed = Technique("Descend", Effect(CardEffectType.ModifyDescendDistance, CardEffectTrigger.Manual, CardEffectTarget.NextDescend, 2));
        CardDefinition attached = Technique("Attached", Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual, CardEffectTarget.SpecificCaughtCard, 3));
        CardDefinition hooked = Technique("Hooked replace", Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual, CardEffectTarget.HookedEncounter, 1));
        CardDefinition replace = Technique("Current replace", Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual, CardEffectTarget.CurrentEncounter, 1));
        CardDefinition reveal = Technique("Reveal", Effect(CardEffectType.RevealEncounterInformation, CardEffectTrigger.Manual, CardEffectTarget.CurrentEncounter, 0));
        Require(!Can(techniques, delayed, encounters, chain, alternate), "Empty encounter state opened a Technique window.");
        encounters.SetCurrentEncounter(environment);
        Require(Can(techniques, delayed, encounters, chain, alternate) && !Can(techniques, attached, encounters, chain, alternate),
            "Event window rejected delayed effects or granted a missing attachment target.");
        CardInstance target = chain.Add(Card("Target", new[] { "Armored" }), resolver);
        Require(Can(techniques, attached, encounters, chain, alternate), "Attached-card effect still required Hooked state.");
        Require(!Can(techniques, hooked, encounters, chain, alternate) && Can(techniques, replace, encounters, chain, alternate),
            "Replacement ignored authored Hooked versus CurrentEncounter target.");
        CardDefinition tagged = Technique("Predator replacement", Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual,
            CardEffectTarget.CurrentEncounter, 1, new[] { "Predator" }));
        Require(!Can(techniques, tagged, encounters, chain, alternate), "Current encounter required tags were ignored.");
        Require(!Can(techniques, reveal, encounters, chain, alternate) && Can(techniques, reveal, encounters, chain, alternate, true),
            "Reveal ignored hidden information relevance.");
        Require(techniques.ApplyCard(reveal, encounters, chain, resolver, new[] { alternate }, null, "Coastal", 0, new System.Random(1), true, out _, TechniqueRuleChecks.FirstTargets(reveal, chain, encounters))
            && techniques.RevealsCurrentEncounter, "Information reveal dereferenced a nonexistent Hooked event.");
        Require(techniques.ApplyCard(attached, encounters, chain, resolver, new[] { alternate }, null, "Coastal", 0, new System.Random(1), false, out _, TechniqueRuleChecks.FirstTargets(attached, chain, encounters))
            && target.CurrentValue == 11, "Event attached modifier failed its actual target.");
        encounters.SetCurrentEncounter(Event(CardType.Hazard)); encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id);
        Require(encounters.CurrentEncounter == null && Can(techniques, delayed, encounters, chain, alternate)
            && Can(techniques, attached, encounters, chain, alternate), "Cancellation blocked attached/delayed Techniques.");
        Require(!Can(techniques, replace, encounters, chain, alternate) && !Can(techniques, hooked, encounters, chain, alternate)
            && !Can(techniques, reveal, encounters, chain, alternate, true), "Discarded event remained targetable.");
        CardDefinition wrongAttached = Technique("Fish only", Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Fish" }));
        Require(!Can(techniques, wrongAttached, encounters, chain, alternate), "Attached target restrictions were widened after cancellation.");
        CardDefinition currentDelayed = Technique("Current delayed", Effect(CardEffectType.ModifyDescendDistance, CardEffectTrigger.Manual,
            CardEffectTarget.CurrentEncounter, 1));
        Require(!Can(techniques, currentDelayed, encounters, chain, alternate), "Delayed effect needing a visible current target accepted discarded Hazard.");
        long cancelled = encounters.CurrentEventOccurrence.Id; encounters.TryCloseCancelledEventWindow(cancelled);
        Require(!Can(techniques, delayed, encounters, chain, alternate), "Closed cancellation window accepted another Technique.");
        encounters.SetCurrentEncounter(environment); long id = encounters.CurrentEventOccurrence.Id;
        encounters.TryBeginEventResolution(id, out _); int before = techniques.PendingDescendEffects.Length;
        Require(!Can(techniques, delayed, encounters, chain, alternate)
            && !techniques.ApplyCard(delayed, encounters, chain, resolver, new[] { alternate }, null, "Coastal", 0, new System.Random(1), false, out _, TechniqueRuleChecks.FirstTargets(delayed, chain, encounters))
            && techniques.PendingDescendEffects.Length == before, "Active event consequences accepted/queued a Technique through backend entry.");
        encounters.TryCompleteEventResolution(id); Require(!Can(techniques, delayed, encounters, chain, alternate), "Resolved event reopened Technique window.");
        encounters.SetCurrentEncounter(Card("Hooked", new[] { "Fish" }));
        Require(Can(techniques, hooked, encounters, chain, alternate) && Can(techniques, delayed, encounters, chain, alternate),
            "Existing Hooked Technique targets stopped working.");
    }

    /// <summary>Checks input remains closed during synchronous and deferred reveal/cancellation reactions.</summary>
    private static void ValidateRevealGuards()
    {
        EncounterRuntime encounters = new EncounterRuntime(); TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        CatchChainRuntime chain = new CatchChainRuntime(); CardDefinition delayed = Technique("Descend", Effect(CardEffectType.ModifyDescendDistance,
            CardEffectTrigger.Manual, CardEffectTarget.NextDescend, 1));
        long reaction = 0, id = 0;
        encounters.EncounterRevealEffects += card =>
        {
            Require(!Can(techniques, delayed, encounters, chain, null), "Technique was playable within a reveal callback.");
            if (EncounterRuntime.IsEventCard(card))
            {
                id = encounters.CurrentEventOccurrence.Id;
                Require(encounters.TryDeferEventRevealReaction(id, out reaction), "Deferred event fixture failed.");
            }
        };
        encounters.SetCurrentEncounter(Card("Creature", Array.Empty<string>()));
        Require(Can(techniques, delayed, encounters, chain, null), "Creature reveal failed to reopen its normal reaction window.");
        encounters.SetCurrentEncounter(Event(CardType.Environment));
        Require(!Can(techniques, delayed, encounters, chain, null), "Deferred reveal reaction permitted Technique input early.");
        encounters.TryCompleteEventRevealReaction(id, reaction);
        Require(Can(techniques, delayed, encounters, chain, null), "Final reveal reaction did not unlock legal Techniques.");
        encounters = new EncounterRuntime(); encounters.HazardCancellation = card => true;
        encounters.HazardCancelled += card => Require(!Can(techniques, delayed, encounters, chain, null), "Cancellation reward callbacks allowed Technique input.");
        encounters.SetCurrentEncounter(Event(CardType.Hazard));
        Require(Can(techniques, delayed, encounters, chain, null), "Finished cancellation did not reopen the pre-action window.");
    }

    /// <summary>Checks real player actions, no-consume rejection, multiple plays, replacement, and deferred recovery order.</summary>
    private static void ValidateController()
    {
        CardDefinition environment = Event(CardType.Environment), alternate = Event(CardType.Opportunity);
        GameObject obj = new GameObject("Event Technique controller checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", Biome(new[] { environment, alternate }));
        Set(controller, "startingLineCapacity", 100); controller.StartRun();
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        CreatureEffectRuntime creatures = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        TechniqueEffectRuntime techniques = Get<TechniqueEffectRuntime>(controller, "techniqueEffectRuntime");
        CardDefinition attached = Technique("Value", Effect(CardEffectType.ModifyCatchValue, CardEffectTrigger.Manual, CardEffectTarget.SpecificCaughtCard, 2));
        CardDefinition delayed = Technique("Descend", Effect(CardEffectType.ModifyDescendDistance, CardEffectTrigger.Manual, CardEffectTarget.NextDescend, 1));
        CardDefinition replace = Technique("Replace", Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual, CardEffectTarget.CurrentEncounter, 1));
        encounters.SetCurrentEncounter(environment); CardInstance target = chain.Add(Card("Target", new[] { "Armored" }), resolver);
        Set(deck, "hand", new[] { attached, delayed, replace }); Set(deck, "drawPile", Deck(0, 10).DrawPile);
        int depth = controller.CurrentDepth;
        Require(controller.CanUseTechniqueCard(0, out _) && TechniqueRuleChecks.Play(controller, 0) && target.CurrentValue == 10
            && controller.CurrentDepth == depth && encounters.CurrentEncounter == environment && deck.Hand.Length == 2 && deck.DrawPile.Length == 10,
            "Player event Technique drew an unearned replacement or changed the action.");
        Require(TechniqueRuleChecks.Play(controller, 0) && techniques.PendingDescendEffects.Length == 1 && controller.CurrentDepth == depth,
            "Second legal Technique could not be played in the same event window.");
        Require(TechniqueRuleChecks.Play(controller, 0) && encounters.CurrentEncounter == alternate
            && controller.CurrentDepth == depth, "Player replacement did not use the unresolved event path.");
        encounters.SetCurrentEncounter(Event(CardType.Hazard)); encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id);
        Set(deck, "hand", new[] { attached, delayed, replace }); Set(deck, "drawPile", Deck(0, 10).DrawPile);
        Require(controller.CanUseTechniqueCard(0, out _) && TechniqueRuleChecks.Play(controller, 0) && target.CurrentValue == 12
            && encounters.CurrentEncounter == null && controller.CurrentDepth == depth, "Player Technique remained blocked after cancellation.");
        Set(deck, "hand", new[] { replace }); int discards = deck.DiscardPile.Length;
        Require(!TechniqueRuleChecks.Play(controller, 0) && deck.Hand.Length == 1 && deck.Hand[0] == replace
            && deck.DiscardPile.Length == discards, "Illegal missing-target play consumed a card.");
        encounters.SetCurrentEncounter(environment); Set(deck, "hand", new[] { delayed });
        foreach (string pending in new[] { "descendCompletionPending", "biomeEntryRevealPending", "techniqueCompletionPending" })
        {
            Set(controller, pending, true); Require(!controller.CanUseTechniqueCard(0, out _), "Pending controller action allowed a new Technique.");
            Set(controller, pending, false);
        }
        long id = encounters.CurrentEventOccurrence.Id; encounters.TryBeginEventResolution(id, out _);
        Require(!TechniqueRuleChecks.Play(controller, 0) && deck.Hand[0] == delayed, "Controller accepted a Technique during event resolution.");
        encounters.TryCompleteEventResolution(id);
        Require(!controller.CanUseTechniqueCard(0, out _), "Controller reopened Techniques after resolution but before confirmed action completion.");

        chain.Reset(); creatures.Reset(); encounters.SetCurrentEncounter(environment);
        CardDefinition cod = Card("Cod", Array.Empty<string>()); Set(cod, "baitEffects", new[] { Effect(CardEffectType.RecoverDiscardedTechnique,
            CardEffectTrigger.WhenAnotherCatchReleased, CardEffectTarget.None, 1, new[] { "Armored" }) });
        chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        CardDefinition urchin = Card("Urchin", new[] { "Armored" }); Set(urchin, "baitEffects", new[] {
            Effect(CardEffectType.GrantSelectedTechniqueRecovery, CardEffectTrigger.WhenCaught, CardEffectTarget.None, 1, new[] { "Armored" }) });
        creatures.RecordAttachment(chain.Add(urchin, resolver, CatchAttachmentRole.Bait));
        chain.Add(Card("Armored", new[] { "Armored" }), resolver);
        CardDefinition release = Technique("Release", Effect(CardEffectType.ReleaseCatch, CardEffectTrigger.Manual,
            CardEffectTarget.SpecificCaughtCard, 1, new[] { "Armored" }));
        Set(deck, "hand", new[] { release, Technique("A"), Technique("B"), Technique("C") });
        Set(deck, "discardPile", new[] { Technique("Prior") }); Set(deck, "drawPile", Deck(0, 10).DrawPile);
        Require(TechniqueRuleChecks.Play(controller, 0) && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique
            && deck.DrawPile.Length == 10 && deck.DiscardPile[1] == release, "Event Release drew an unearned card or changed recovery order.");
        Require(!TechniqueRuleChecks.Play(controller, 0), "Pending creature recovery allowed another Technique.");
        Require(controller.TryChooseRecoveredTechnique(0) && !controller.HasPendingCreatureChoice
            && deck.Hand.Length == 5 && deck.Hand[4] == release && deck.DrawPile.Length == 10
            && controller.CanUseTechniqueCard(4, out _), "Cod/Urchin order or uncapped exact recovery failed.");
    }

    /// <summary>Creates a non-catchable event fixture.</summary>
    private static CardDefinition Event(CardType type)
    {
        CardDefinition card = Card(type.ToString(), Array.Empty<string>()); Set(card, "cardType", type); return card;
    }

    /// <summary>Exercises the actual Technique backend validation with optional hidden-information state.</summary>
    private static bool Can(TechniqueEffectRuntime techniques, CardDefinition card, EncounterRuntime encounters,
        CatchChainRuntime chain, CardDefinition alternate, bool hidden = false)
    {
        return techniques.CanUseCard(card, encounters, chain, hidden, alternate == null ? Array.Empty<CardDefinition>() : new[] { alternate },
            "Coastal", 0, out _);
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
