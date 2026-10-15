using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
using TMPro;
#endif

/// <summary>Validates resolving ownership, precommit targets, capacity, and exact recovery ordering.</summary>
public static class TechniqueRuleChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs approved shared Technique rules without modifying production card assets.</summary>
    [MenuItem("Fishing Cards/Validation/Technique Rules")]
    public static void Run() { int rules = ValidateRules(); Debug.Log("Technique rules: " + rules + " runtime and " + ValidateEditorPresentation() + " Editor assertions passed."); }
    /// <summary>Checks target modal wiring, concealment, and surrounding action controls on the production prefab.</summary>
    public static int ValidateEditorPresentation()
    {
        assertions = 0;
        try
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab");
            Require(prefab != null, "Production run prefab is missing.");
            GameObject obj = UnityEngine.Object.Instantiate(prefab); created.Add(obj);
            FishingRunView view = obj.GetComponent<FishingRunView>(); FishingRunController c = Controller();
            Set(c, "fishingRunView", view);
            CardInstance target = Chain(c).Add(Card(), Resolver(c));
            CardDefinition card = Technique(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard);
            Set(Deck(c), "hand", new[] { card });
            Require(c.TryUseTechniqueCard(0) && c.HasPendingTechniqueSelection && Deck(c).Hand.Length == 1
                && !Get<Button>(view, "surfaceButton").interactable && !Get<Button>(view, "descendButton").interactable,
                "Target modal did not retain the card or disable core actions.");
            Require(c.TryChooseTechniqueTarget(target.InstanceId) && !c.HasPendingTechniqueSelection
                && Get<Button>(view, "surfaceButton").interactable, "Target completion did not restore action availability.");
            CardDefinition hidden = Card(); Set(hidden, "displayName", "SECRET SPECIES");
            CardEffectDefinition conceal = Effect(CardEffectType.QueueCreatureEncounterConcealment, CardEffectTarget.FutureEncounters, 1);
            Set(conceal, "effectTone", CardEffectTone.Negative);
            Encounters(c).GrantCreatureConcealment(conceal, hidden); Encounters(c).SetCurrentEncounter(hidden);
            CardDefinition avoid = Technique(CardEffectType.AvoidEncounter, CardEffectTarget.HookedEncounter);
            Set(Deck(c), "hand", new[] { avoid }); c.TryUseTechniqueCard(0);
            CreatureEffectChoiceView modal = Get<CreatureEffectChoiceView>(view, "creatureEffectChoiceView");
            TMP_Text[] labels = modal.GetComponentsInChildren<TMP_Text>(false);
            Require(Array.Exists(labels, text => text.text == "Unknown creature")
                && !Array.Exists(labels, text => text.text.Contains("SECRET SPECIES")),
                "Encounter target selection leaked concealed identity.");
            c.TryCancelTechniqueSelection();
            return assertions;
        }
        finally { foreach (UnityEngine.Object fixture in created) UnityEngine.Object.DestroyImmediate(fixture); created.Clear(); }
    }
#endif
    /// <summary>Runs real deck/controller/targeting integration checks and releases fixtures.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { DeckOwnership(); Selection(); EncounterSelection(); MultiEffect(); Recovery(); ReentryAndFailure(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks the four-card cap and exclusion of a resolving physical card from draw, recovery, and discard.</summary>
    private static void DeckOwnership()
    {
        CardDefinition card = Technique(CardEffectType.ModifyDescendDistance, CardEffectTarget.NextDescend);
        TechniqueDeckRuntime deck = new TechniqueDeckRuntime();
        deck.Initialize(new[] { card, card, card, card, card, card }, 20, new System.Random(1), null);
        Require(deck.Hand.Length == 4 && deck.DrawPile.Length == 2, "Initial hand exceeded four or lost undrawn cards.");
        long version = deck.HandVersion;
        Require(deck.TryBeginPlay(0, out CardDefinition played, out _) && deck.Hand.Length == 3
            && deck.ResolvingCard == played && deck.DiscardPile.Length == 0 && deck.HandVersion > version,
            "Played card entered discard before effect completion.");
        int id = deck.ResolvingOccurrenceId;
        Require(!deck.TryBeginPlay(0, out _, out _) && !deck.TryRecoverPlayedTechnique(id, played, 4),
            "Resolving occurrence was replayed or recovered early.");
        Require(deck.DrawTechniques(9, 99, new System.Random(1)) == 1 && deck.Hand.Length == 4,
            "Explicit effect draw ignored the global cap.");
        Require(deck.TryCompletePlay(id) && deck.LastDiscardOccurrenceId == id && deck.ResolvingCard == null
            && !deck.TryCompletePlay(id), "Completion failed exact once-only discard.");
        Require(!deck.TryRecoverDiscardedTechnique(0) && deck.Hand.Length == 4, "Cod recovery created a fifth card.");
        Require(deck.TryDiscardTechnique(0) && deck.TryRecoverPlayedTechnique(id, played, 99)
            && deck.Hand.Length == 4 && !deck.HasDiscardedTechniqueOccurrence(id, played),
            "Exact recovery or capacity enforcement failed.");
        Require(!deck.TryRecoverPlayedTechnique(id, played, 4), "A second recovery duplicated the physical card.");
        deck.Refill(99, new System.Random(1));
        Require(deck.Hand.Length == 4, "Normal refill ignored the hand cap.");
        deck.Reset(); Set(deck, "hand", new[] { card });
        Require(deck.TryBeginPlay(0, out _, out _) && deck.DrawTechniques(4, 4, new System.Random(1)) == 0
            && deck.ResolvingCard == card && deck.DiscardPile.Length == 0,
            "The resolving card was reshuffled into an effect draw.");
        int old = deck.ResolvingOccurrenceId; deck.Reset(); Set(deck, "hand", new[] { card }); deck.TryBeginPlay(0, out _, out _);
        Require(deck.ResolvingOccurrenceId != old && !deck.TryCompletePlay(old), "A previous run's completion discarded a new card.");
        deck.Reset(); Require(deck.ResolvingCard == null && deck.ResolvingOccurrenceId == 0, "Shutdown retained resolving ownership.");
    }

    /// <summary>Checks no-cost cancellation, explicit non-first selection, legal restrictions, and stale callbacks.</summary>
    private static void Selection()
    {
        FishingRunController c = Controller(); CatchChainRuntime chain = Chain(c); EffectResolver resolver = Resolver(c);
        CardInstance first = chain.Add(Card(), resolver), second = chain.Add(Card(), resolver);
        CardDefinition card = Technique(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard, 3);
        TechniqueDeckRuntime deck = Deck(c); Set(deck, "hand", new[] { card, card });
        Require(c.TryUseTechniqueCard(0) && c.HasPendingTechniqueSelection && deck.Hand.Length == 2
            && deck.ResolvingCard == null && deck.DiscardPile.Length == 0, "Target selection spent the card.");
        long token = c.TechniqueSelectionId;
        Require(!c.TrySurface() && !c.TryDescend() && !c.TryReleaseCatch(0) && !c.TryUseTechniqueCard(1),
            "A different action interrupted target selection.");
        Require(c.TryCancelTechniqueSelection(token) && deck.Hand.Length == 2 && first.CurrentValue == 8,
            "Cancelling targets changed gameplay state.");
        Require(!c.TryChooseTechniqueTarget(second.InstanceId, token), "A cancelled callback committed a play.");
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(second.InstanceId)
            && second.CurrentValue == 11 && first.CurrentValue == 8 && deck.Hand.Length == 1 && deck.DiscardPile.Length == 1,
            "The play substituted an automatic target.");
        Require(!c.TryChooseTechniqueTarget(second.InstanceId), "Repeated target completion played the card twice.");
        CardDefinition immune = Card(); Set(immune, "effects", new[] { Effect(CardEffectType.PreventTechniqueTargeting, CardEffectTarget.Self) });
        CardInstance anemone = chain.Add(immune, resolver);
        Require(!c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(card, new[] { new[] { anemone.InstanceId } }))
            && deck.Hand.Length == 1, "Sea Anemone Catch was selected or invalid selection spent a card.");
        CardDefinition release = Technique(CardEffectType.ReleaseCatch, CardEffectTarget.CatchChain, 1);
        CardDefinition locked = Card(); Set(locked, "effects", new[] { Effect(CardEffectType.PreventRelease, CardEffectTarget.Self) });
        CardInstance crab = chain.Add(locked, resolver); Set(deck, "hand", new[] { release });
        Require(!c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(release, new[] { new[] { crab.InstanceId } })),
            "Iron Grip was bypassed.");
        Require(c.TryUseTechniqueCard(0), "Legal Release selection did not open.");
        deck.TryDiscardTechnique(0);
        Require(!c.TryChooseTechniqueTarget(first.InstanceId) && !c.HasPendingTechniqueSelection && chain.Catches.Length == 4,
            "A stale hand selection released a card.");
        Set(deck, "hand", new[] { card }); c.TryUseTechniqueCard(0);
        Encounters(c).SetCurrentEncounter(Card());
        Require(!c.TryChooseTechniqueTarget(first.InstanceId) && deck.Hand.Length == 1, "Changed encounter committed stale targets.");
    }

    /// <summary>Checks explicit current-encounter selection, stale reveal cancellation, and complete required-target validation.</summary>
    private static void EncounterSelection()
    {
        FishingRunController c = Controller();
        CardDefinition avoid = Technique(CardEffectType.AvoidEncounter, CardEffectTarget.HookedEncounter);
        Set(Deck(c), "hand", new[] { avoid });
        Require(c.TryUseTechniqueCard(0) && c.HasPendingTechniqueSelection && Deck(c).Hand.Length == 1
            && c.HookedEncounter != null, "Current encounter target was implicitly committed.");
        Require(c.TryChooseTechniqueTarget(TechniqueTargetPlan.EncounterTargetId) && c.CurrentEncounter == null
            && c.CurrentDepth == 0 && Deck(c).DiscardPile.Length == 1, "Selecting an encounter advanced depth or failed to play.");
        CardDefinition hazard = Card(); Set(hazard, "cardType", CardType.Hazard); Encounters(c).SetCurrentEncounter(hazard);
        CardDefinition replace = Technique(CardEffectType.ReplaceEncounter, CardEffectTarget.CurrentEncounter);
        Set(Deck(c), "hand", new[] { replace });
        Require(c.TryUseTechniqueCard(0) && c.HasPendingTechniqueSelection, "Unresolved event target selection did not open.");
        Encounters(c).TryCancelRevealedHazard(Encounters(c).CurrentEventOccurrence.Id);
        Require(!c.TryChooseTechniqueTarget(TechniqueTargetPlan.EncounterTargetId) && !c.HasPendingTechniqueSelection
            && Deck(c).Hand.Length == 1, "A discarded event committed as a stale target.");
        c = Controller();
        CardDefinition needsTarget = Technique(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard);
        Set(needsTarget, "effects", new[] { needsTarget.Effects[0], Effect(CardEffectType.ModifyDescendDistance, CardEffectTarget.NextDescend, 1, CardEffectTrigger.Manual) });
        Set(Deck(c), "hand", new[] { needsTarget });
        Require(!c.CanUseTechniqueCard(0, out _) && !c.TryUseTechniqueCard(0) && Deck(c).Hand.Length == 1,
            "A usable delayed effect bypassed another effect's required target.");
        CardInstance first = Chain(c).Add(Card(), Resolver(c)); Chain(c).Add(Card(), Resolver(c));
        CardDefinition release = Technique(CardEffectType.ReleaseCatch, CardEffectTarget.CatchChain, 2);
        Set(Deck(c), "hand", new[] { release });
        Require(!c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(release, new[] { new[] { first.InstanceId, first.InstanceId } }))
            && Chain(c).Catches.Length == 2 && Deck(c).Hand.Length == 1, "Duplicate release targets passed commit validation.");
        c.TryUseTechniqueCard(0);
        c.StartRun();
        Require(!c.HasPendingTechniqueSelection && Deck(c).ResolvingCard == null, "Restart retained a precommit plan.");
    }

    /// <summary>Checks multiple exact targets, missing recipients, targetless effects, and backend validation.</summary>
    private static void MultiEffect()
    {
        FishingRunController c = Controller(); CatchChainRuntime chain = Chain(c); EffectResolver resolver = Resolver(c);
        CardInstance first = chain.Add(Card(), resolver), second = chain.Add(Card(), resolver);
        CardDefinition release = Technique(CardEffectType.ReleaseCatch, CardEffectTarget.CatchChain, 2); Set(Deck(c), "hand", new[] { release });
        Require(c.TryUseTechniqueCard(0) && c.TryChooseTechniqueTarget(second.InstanceId) && Deck(c).Hand.Length == 1
            && chain.Catches.Length == 2, "Multi-target play committed before all selections.");
        Require(!c.TryChooseTechniqueTarget(second.InstanceId) && c.TryChooseTechniqueTarget(first.InstanceId)
            && chain.Catches.Length == 0 && chain.ReleasedCatches.Length == 2, "Distinct selected releases were not respected.");
        c = Controller(); chain = Chain(c); resolver = Resolver(c);
        first = chain.Add(Card(), resolver); second = chain.Add(Card(), resolver);
        Set(Deck(c), "hand", new[] { release });
        Require(c.TryUseTechniqueCard(0) && !c.TryFinishTechniqueTargets()
            && c.TryChooseTechniqueTarget(second.InstanceId) && c.TryFinishTechniqueTargets()
            && chain.Catches.Length == 1 && chain.Catches[0] == first && Deck(c).DiscardPile.Length == 1,
            "Up-to-two Release could not commit one selected recipient or committed none.");
        c = Controller(); chain = Chain(c); resolver = Resolver(c);
        first = chain.Add(Card(), resolver); second = chain.Add(Card(), resolver);
        CardDefinition both = Technique(CardEffectType.ReleaseCatch, CardEffectTarget.SpecificCaughtCard, 1);
        Set(both, "effects", new[] { both.Effects[0], Effect(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard, 5) });
        Set(Deck(c), "hand", new[] { both });
        Require(c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(both, new[] { new[] { first.InstanceId }, new[] { first.InstanceId } }))
            && second.CurrentValue == 8 && chain.Catches.Length == 1, "A missing committed recipient was silently retargeted.");
        CardDefinition distribute = Technique(CardEffectType.ModifyCaughtCard, CardEffectTarget.CatchChain, 1);
        chain.Add(Card(), resolver); Set(Deck(c), "hand", new[] { distribute });
        int selections = 0; chain.TechniqueTargetSelected += target => selections++;
        Require(c.TryUseTechniqueCard(0) && !c.HasPendingTechniqueSelection && selections == 0,
            "Targetless distribution emitted a targeting trigger.");
        CardDefinition value = Technique(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard, 1);
        Require(!Get<TechniqueEffectRuntime>(c, "techniqueEffectRuntime").ApplyCard(value, Encounters(c), chain, resolver,
            Array.Empty<CardDefinition>(), null, "Coastal", 0, new System.Random(1), false, out _),
            "Backend guessed a target without a supplied plan.");
    }

    /// <summary>Checks full-hand Cod choices and exact Cod-before-Urchin recovery without duplication.</summary>
    private static void Recovery()
    {
        FishingRunController c = Controller(); CatchChainRuntime chain = Chain(c); EffectResolver resolver = Resolver(c);
        CardDefinition armored = Card(); Set(armored, "tags", new[] { "Armored" }); CardInstance target = chain.Add(armored, resolver);
        AddCod(chain, resolver);
        CardDefinition card = Technique(CardEffectType.ModifyDescendDistance, CardEffectTarget.NextDescend);
        TechniqueDeckRuntime deck = Deck(c); Set(deck, "hand", new[] { card, card, card, card }); Set(deck, "discardPile", new[] { card });
        Require(c.TryReleaseCatch(0) && c.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique,
            "Cod did not offer recovery.");
        int reserved = deck.GetDiscardOccurrenceId(0);
        Require(c.TryChooseRecoveredTechnique(0) && deck.Hand.Length == 4
            && c.PendingCreatureChoice == CreatureEffectChoiceKind.MakeRoomForRecoveredTechnique,
            "Full Cod recovery overflowed or did not offer room.");
        Require(c.TryChooseRecoveryRoom(0) && deck.Hand.Length == 4 && !deck.HasDiscardedTechniqueOccurrence(reserved, card)
            && deck.DiscardPile.Length == 1 && !c.TryChooseRecoveryRoom(0),
            "Cod recovered the wrong identical card or duplicated completion.");
        c = Controller(); chain = Chain(c); resolver = Resolver(c); target = chain.Add(armored, resolver); AddCod(chain, resolver);
        Set(Deck(c), "hand", new[] { card, card, card, card }); Set(Deck(c), "discardPile", new[] { card });
        c.TryReleaseCatch(0); c.TryChooseRecoveredTechnique(0);
        Require(c.TryChooseRecoveryRoom(-1) && Deck(c).Hand.Length == 4 && Deck(c).DiscardPile.Length == 1,
            "Declining Cod recovery changed the hand or spent a hand card.");
        c = Controller(); chain = Chain(c); resolver = Resolver(c); target = chain.Add(armored, resolver); AddCod(chain, resolver);
        CardDefinition urchin = Card(); Set(urchin, "hasExplicitBaitData", true);
        CardEffectDefinition grip = Effect(CardEffectType.GrantSelectedTechniqueRecovery, CardEffectTarget.None, 1, CardEffectTrigger.WhenCaught);
        Set(grip, "requiredTags", new[] { "Armored" }); Set(urchin, "baitEffects", new[] { grip });
        CreatureEffectRuntime creatures = Get<CreatureEffectRuntime>(c, "creatureEffectRuntime");
        creatures.RecordAttachment(chain.Add(urchin, resolver, CatchAttachmentRole.Bait));
        CardDefinition cut = Technique(CardEffectType.ReleaseCatch, CardEffectTarget.SpecificCaughtCard, 1);
        Set(Deck(c), "hand", new[] { cut });
        bool during = false;
        chain.TechniqueTargetSelected += selected => during = Deck(c).ResolvingCard == cut && Deck(c).DiscardPile.Length == 0;
        Require(c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(cut, new[] { new[] { target.InstanceId } }))
            && during && c.PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique && Deck(c).DiscardPile[0] == cut,
            "Cod/Urchin did not wait for the played card to enter discard.");
        Require(c.TryChooseRecoveredTechnique(0) && Deck(c).Hand.Length == 1 && Deck(c).Hand[0] == cut
            && Deck(c).DiscardPile.Length == 0 && creatures.PendingSelectedTechniqueRecoveryCount == 0
            && !c.HasPendingCreatureChoice, "Urchin duplicated Cod's recovered physical card.");
    }

    /// <summary>Checks synchronous callbacks cannot reopen input, resolve queued choices early, or reset a partial play.</summary>
    private static void ReentryAndFailure()
    {
        FishingRunController c = Controller(); CatchChainRuntime chain = Chain(c);
        CardInstance target = chain.Add(Card(), Resolver(c)); CardDefinition card = Technique(CardEffectType.ModifyCatchValue, CardEffectTarget.SpecificCaughtCard);
        Set(Deck(c), "hand", new[] { card });
        chain.TechniqueTargetSelected += selected =>
        {
            Require(!c.TryDescend() && !c.TrySurface() && !c.TryUseTechniqueCard(0)
                && !c.TryChooseRecoveredTechnique(0) && !c.TryChooseEventTarget(selected.InstanceId),
                "A synchronous Technique callback reopened gameplay.");
            c.StartRun();
            Require(chain.Catches.Length == 1 && Deck(c).ResolvingCard == card, "Restart ran inside a resolving effect.");
        };
        Require(c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(card, new[] { new[] { target.InstanceId } }))
            && Deck(c).ResolvingCard == null, "Guarded play did not complete.");
        c = Controller(); chain = Chain(c); target = chain.Add(Card(), Resolver(c)); Set(Deck(c), "hand", new[] { card });
        CreatureEffectRuntime failedChoices = Get<CreatureEffectRuntime>(c, "creatureEffectRuntime");
        chain.TechniqueTargetSelected += selected =>
        {
            failedChoices.QueueEventRelease("Failure fixture");
            throw new InvalidOperationException("Fixture failure");
        };
        Require(!c.TryUseTechniqueCardWithTargets(0, new TechniqueTargetPlan(card, new[] { new[] { target.InstanceId } }))
            && Deck(c).ResolvingCard == card && Deck(c).DiscardPile.Length == 0
            && !c.CanDescend && !c.CanSurface && !c.CanUseTechniqueCard(0, out _)
            && !c.HasPendingCreatureChoice && !c.TryChooseEventTarget(target.InstanceId)
            && chain.Catches.Length == 1,
            "Partial failure exposed discard or reopened input.");
        c.StartRun();
        Require(Deck(c).ResolvingCard == null && string.IsNullOrEmpty(c.TechniquePlayFailure),
            "Explicit restart retained failed resolving ownership.");
    }

    /// <summary>Supplies explicit first-eligible fixture choices for pre-existing rule tests, not gameplay.</summary>
    public static TechniqueTargetPlan FirstTargets(CardDefinition card, CatchChainRuntime chain, EncounterRuntime encounters = null)
    {
        int[][] ids = new int[card.Effects.Length][];
        for (int i = 0; i < ids.Length; i++)
        {
            if (TechniqueTargetPlan.RequiresEncounterSelection(card.Effects[i]))
            { ids[i] = new[] { TechniqueTargetPlan.EncounterTargetId }; continue; }
            if (!TechniqueTargetPlan.RequiresSelection(card.Effects[i])) continue;
            CardInstance[] candidates = chain.GetTechniqueTargets(card.Effects[i]);
            int count = TechniqueTargetPlan.RequiredCount(card.Effects[i], chain); ids[i] = new int[count];
            for (int j = 0; j < count; j++) ids[i][j] = candidates[j].InstanceId;
        }
        return new TechniqueTargetPlan(card, ids, encounters);
    }
    /// <summary>Plays legacy fixture inputs with explicit supplied targets through the production controller.</summary>
    public static bool Play(FishingRunController c, int index)
    {
        if (!c.CanUseTechniqueCard(index, out _)) return false;
        return c.TryUseTechniqueCardWithTargets(index, FirstTargets(c.TechniqueHand[index], Chain(c), Encounters(c)));
    }
    /// <summary>Creates a completed discard occurrence for low-level recovery fixtures.</summary>
    public static bool DiscardFixture(TechniqueDeckRuntime deck, int index, out CardDefinition card, out string reason)
    {
        if (!deck.TryBeginPlay(index, out card, out reason)) return false;
        return deck.TryCompletePlay(deck.ResolvingOccurrenceId);
    }
    /// <summary>Creates the shared Cod release reaction fixture.</summary>
    private static void AddCod(CatchChainRuntime chain, EffectResolver resolver)
    {
        CardDefinition cod = Card(); Set(cod, "hasExplicitBaitData", true);
        CardEffectDefinition recover = Effect(CardEffectType.RecoverDiscardedTechnique, CardEffectTarget.None, 1, CardEffectTrigger.WhenAnotherCatchReleased);
        Set(recover, "requiredTags", new[] { "Armored" }); Set(cod, "baitEffects", new[] { recover });
        chain.Add(cod, resolver, CatchAttachmentRole.Bait);
    }
    /// <summary>Creates a creature fixture.</summary>
    private static CardDefinition Card()
    {
        CardDefinition c = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(c); Set(c, "cardType", CardType.Creature);
        Set(c, "weight", 1); Set(c, "value", 8); return c;
    }
    /// <summary>Creates an explicit Technique fixture.</summary>
    private static CardDefinition Technique(CardEffectType type, CardEffectTarget target, int amount = 1)
    { CardDefinition c = Card(); Set(c, "cardType", CardType.Technique); Set(c, "effects", new[] { Effect(type, target, amount, CardEffectTrigger.Manual) }); return c; }
    /// <summary>Creates authored effect data without changing production assets.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTarget target, int amount = 0, CardEffectTrigger trigger = CardEffectTrigger.WhileAttached)
    { CardEffectDefinition e = new CardEffectDefinition(); Set(e, "effectType", type); Set(e, "target", target); Set(e, "amount", amount); Set(e, "trigger", trigger); return e; }
    /// <summary>Starts a controller with a stable creature reaction window.</summary>
    private static FishingRunController Controller()
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome); Set(biome, "biomeId", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition(); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { Card() });
        Set(biome, "depthTiers", new[] { tier }); GameObject obj = new GameObject("Technique rules"); created.Add(obj);
        FishingRunController c = obj.AddComponent<FishingRunController>(); Set(c, "startRunOnAwake", false);
        Set(c, "startingBiome", biome); Set(c, "startingHandSize", 0); Set(c, "startingLineCapacity", 100); c.StartRun(); return c;
    }
    /// <summary>Reads the actual chain owner.</summary>
    private static CatchChainRuntime Chain(FishingRunController c) { return Get<CatchChainRuntime>(c, "catchChainRuntime"); }
    /// <summary>Reads the actual deck owner.</summary>
    private static TechniqueDeckRuntime Deck(FishingRunController c) { return Get<TechniqueDeckRuntime>(c, "techniqueDeckRuntime"); }
    /// <summary>Reads the actual encounter owner.</summary>
    private static EncounterRuntime Encounters(FishingRunController c) { return Get<EncounterRuntime>(c, "encounterRuntime"); }
    /// <summary>Reads the actual resolver.</summary>
    private static EffectResolver Resolver(FishingRunController c) { return Get<EffectResolver>(c, "effectResolver"); }
    /// <summary>Configures serialized fixture fields.</summary>
    private static void Set(object o, string name, object value) { o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(o, value); }
    /// <summary>Reads runtime ownership without duplicating logic.</summary>
    private static T Get<T>(object o, string name) { return (T)o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(o); }
    /// <summary>Counts an invariant and reports its first mismatch.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
