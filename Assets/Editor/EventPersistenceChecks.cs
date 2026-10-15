using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks event lifetime expiry, exact cleansing, mode persistence, pending transfer, and run cleanup.</summary>
public static class EventPersistenceChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;

    /// <summary>Fixture handler demonstrating lifetime ownership through the actual consequence dispatcher.</summary>
    private sealed class ModifierHandler : IEventEffectHandler
    {
        private readonly int targetId;
        public CardEffectType EffectType => CardEffectType.ModifyCatchValue;
        /// <summary>Targets the already attached fixture creature.</summary>
        public ModifierHandler(int id) { targetId = id; }
        /// <summary>Accepts the explicitly authored fixture effect data.</summary>
        public bool CanResolve(CardEffectDefinition effect, out string reason) { reason = string.Empty; return true; }
        /// <summary>Applies recipient provenance in the bound persistence owner rather than on the event card.</summary>
        public EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect)
        {
            return context.Persistence.TryAddModifier(context.OccurrenceId, context.Source, effect, targetId,
                0, 3, EventEffectLifetime.UntilRunEnds(), out _) ? EventEffectOutcome.Applied : EventEffectOutcome.NoEligibleTarget;
        }
    }
#if UNITY_EDITOR
    /// <summary>Runs lifetime and cleanup integration checks in Unity.</summary>
    [MenuItem("Fishing Cards/Validation/Event Persistence and Lifetimes")]
    public static void Run() { Debug.Log("Event persistence passed: " + ValidateRules() + " assertions."); }
#endif
    /// <summary>Executes actual persistence/modifier/cleansing operations and releases fixture assets.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateModifiers(); ValidatePending(); ValidateRecipientsAndReset(); ValidateDispatchAndController(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks mode changes, shared descriptors, exact positive/negative expiry, duplicate boundary notifications, and Quick Clean.</summary>
    private static void ValidateModifiers()
    {
        CatchChainRuntime chain = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver(); EventPersistenceRuntime store = Store(chain, resolver);
        CardDefinition source = Card(CardType.Environment); CardEffectDefinition penalty = Effect(CardEffectTone.Negative);
        CardEffectDefinition bonus = Effect(CardEffectTone.Positive); CardInstance target = chain.Add(Card(), resolver);
        target.AddPermanentModifiers(0, 2); chain.Recalculate(resolver);
        Require(penalty.EventLifetime == null, "Legacy effect data acquired an implicit event duration.");
        EventEffectLifetime lifetime = EventEffectLifetime.AtBoundary("fixture_action", 2);
        Set(penalty, "hasEventLifetime", true); Set(penalty, "eventLifetime", lifetime);
        Require(penalty.EventLifetime == lifetime, "Explicit authored lifetime was not exposed to effect handlers.");
        long positive = 0;
        Require(store.TryAddModifier(1, source, penalty, target.InstanceId, 2, -3, lifetime, out long negative)
            && store.TryAddModifier(2, source, bonus, target.InstanceId, 1, 4, EventEffectLifetime.UntilRunEnds(), out positive)
            && target.CurrentWeight == 4 && target.CurrentValue == 11, "Event modifiers missed provenance or base contributions.");
        Require(target.AppliedModifiers.Length == 2 && target.AppliedModifiers[0].EventEffectId == negative
            && store.Effects[0].SourceOccurrenceId == 1 && store.Effects[0].Source == source, "Source occurrence/recipient identity was lost.");
        CardInstance snapshot = target.CreateSnapshot(); long epoch = store.Epoch;
        Require(store.NotifyBoundary(epoch, "fixture_action", 1) && store.Effects[0].RemainingBoundaries == 1
            && lifetime.BoundaryCount == 2, "Boundary mutated shared lifetime data or missed the first count.");
        target.SetAttachmentRole(CatchAttachmentRole.Bait); chain.Recalculate(resolver);
        Require(target.CurrentWeight == 4 && target.CurrentValue == 11 && target.HaulValueContribution == 0
            && store.Effects[0].RemainingBoundaries == 1, "Mode change restarted lifetime or removed modifiers.");
        Require(!store.NotifyBoundary(epoch, "fixture_action", 1) && store.Effects[0].RemainingBoundaries == 1,
            "Duplicate boundary notification consumed duration twice.");
        Require(store.NotifyBoundary(epoch, "other_fixture_boundary", 1) && store.Effects[0].RemainingBoundaries == 1,
            "Unrelated boundary expired an effect.");
        NegativeEffectOption[] options = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(), new TechniqueEffectRuntime(), resolver, store);
        Require(options.Length == 1 && options[0].SourceCard == source && options[0].TryRemove()
            && target.CurrentWeight == 2 && target.CurrentValue == 14 && store.Effects.Length == 1,
            "Quick Clean duplicated provenance or removed the wrong contribution.");
        Require(!options[0].TryRemove() && !store.TryRemoveNegativeEffect(positive), "Duplicate cleanse or cleansing a positive effect succeeded.");
        Require(snapshot.CurrentWeight == 4 && snapshot.CurrentValue == 11 && snapshot.AppliedModifiers.Length == 2,
            "Live expiry/cleansing mutated a historical snapshot.");
        target.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(target.CurrentValue == 14 && target.Definition.Value == 8 && target.Definition.Weight == 1,
            "Returning to Catch lost value or mutated authored stats.");
        Require(store.TryAddModifier(3, source, bonus, target.InstanceId, 0, 5, EventEffectLifetime.AtBoundary("fixture_action", 1), out long timed),
            "Positive timed fixture could not be applied.");
        Set(target, "appliedModifiers", new List<AppliedCatchModifier>(Array.ConvertAll(target.AppliedModifiers,
            m => new AppliedCatchModifier(m.Effect, m.SourceCard, m.WeightAmount, m.ValueAmount, m.EventEffectId))));
        Require(store.NotifyBoundary(epoch, "fixture_action", 2) && target.CurrentValue == 14
            && !store.OwnsModifier(timed, target.InstanceId) && store.OwnsModifier(positive, target.InstanceId),
            "Expiry could not remove a positive modifier or removed another source.");
        store.Reset(); Require(target.CurrentWeight == 1 && target.CurrentValue == 10 && target.AppliedModifiers.Length == 0,
            "Cleanup removed untracked native/permanent contributions or left positive event modifiers.");
    }

    /// <summary>Checks independent pending instances, consumption once, deferred application without clock reset, and pending expiry/cleansing.</summary>
    private static void ValidatePending()
    {
        CatchChainRuntime chain = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver(); EventPersistenceRuntime store = Store(chain, resolver);
        CardDefinition source = Card(CardType.Environment); CardEffectDefinition negative = Effect(CardEffectTone.Negative);
        EventEffectLifetime shared = EventEffectLifetime.AtBoundary("fixture_encounter", 2);
        long first = store.QueuePending(4, source, negative, shared); long second = store.QueuePending(4, source, negative, shared);
        Require(first != second && store.Effects.Length == 2 && store.Effects[0].IsPending, "Duplicate definitions shared pending runtime identity.");
        Require(store.GetNegativeOptions().Length == 2 && store.TryRemoveNegativeEffect(first) && store.Effects.Length == 1
            && store.Effects[0].Id == second && shared.BoundaryCount == 2, "Pending cleanse removed another occurrence.");
        Require(store.NotifyBoundary(store.Epoch, "fixture_encounter", 1) && store.Effects[0].RemainingBoundaries == 1,
            "Pending duration did not advance independently of its discarded source.");
        Require(!store.TryApplyPendingModifier(second, 999, 1, 0) && store.Effects[0].IsPending,
            "Missing recipient consumed a pending application.");
        CardInstance bait = chain.Add(Card(), resolver, CatchAttachmentRole.Bait);
        Require(store.TryApplyPendingModifier(second, bait.InstanceId, 1, -2) && bait.CurrentWeight == 2 && bait.CurrentValue == 6
            && store.Effects[0].RemainingBoundaries == 1 && !store.Effects[0].IsPending,
            "Pending transfer reset its lifetime or rejected Bait.");
        Require(!store.TryApplyPendingModifier(second, bait.InstanceId, 1, -2) && !store.TryConsumePending(second, out _),
            "Already-applied occurrence was applied/consumed twice.");
        bait.SetAttachmentRole(CatchAttachmentRole.Catch); chain.Recalculate(resolver);
        Require(store.NotifyBoundary(store.Epoch, "fixture_encounter", 2) && store.Effects.Length == 0
            && bait.CurrentWeight == 1 && bait.CurrentValue == 8, "Transferred modifier expiry lost original duration across modes.");
        long pending = store.QueuePending(5, source, negative, EventEffectLifetime.UntilRunEnds());
        Require(store.TryConsumePending(pending, out PersistentEventEffect consumed) && consumed.SourceOccurrenceId == 5
            && !store.TryConsumePending(pending, out _) && store.GetNegativeOptions().Length == 0,
            "Pending claim repeated or completed one-time effect remained cleanseable.");
        store.QueuePending(6, source, negative, EventEffectLifetime.AtBoundary("fixture_biome", 1));
        store.NotifyBoundary(store.Epoch, "fixture_encounter", 3);
        Require(store.Effects.Length == 1, "Pending effect expired at a non-authored biome/encounter boundary.");
        store.NotifyBoundary(store.Epoch, "fixture_biome", 1); Require(store.Effects.Length == 0, "Authored pending boundary did not expire.");
        store.NotifyBoundary(store.Epoch, "fixture_encounter", 10);
        pending = store.QueuePending(7, source, negative, shared);
        Require(!store.NotifyBoundary(store.Epoch, "fixture_encounter", 10) && store.Effects[0].RemainingBoundaries == 2,
            "A previously processed boundary expired a newly queued effect.");
        Require(store.NotifyBoundary(store.Epoch, "fixture_encounter", 11) && store.Effects[0].RemainingBoundaries == 1,
            "A new boundary did not advance newly queued duration.");
    }

    /// <summary>Checks target removal/exchange, old callbacks, positive/negative cleanup, fallback cleansing, and owner binding.</summary>
    private static void ValidateRecipientsAndReset()
    {
        CatchChainRuntime chain = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver(); EventPersistenceRuntime store = Store(chain, resolver);
        CardDefinition source = Card(CardType.Environment); CardEffectDefinition penalty = Effect(CardEffectTone.Negative);
        CardInstance target = chain.Add(Card(), resolver);
        Require(!store.TryAddModifier(1, source, penalty, 999, 1, 0, EventEffectLifetime.UntilRunEnds(), out _)
            && store.Effects.Length == 0, "Missing target created a leaked applied/pending record.");
        store.TryAddModifier(1, source, penalty, target.InstanceId, 1, 0, EventEffectLifetime.UntilRunEnds(), out long id);
        chain.TryRelease(0, resolver, out _, out _, out _);
        Require(store.Effects.Length == 0 && store.GetNegativeOptions().Length == 0 && !store.TryRemoveNegativeEffect(id),
            "Detached recipient remained an active event target.");
        target = chain.Add(Card(), resolver); store.TryAddModifier(2, source, penalty, target.InstanceId, 1, 0,
            EventEffectLifetime.UntilRunEnds(), out id);
        CardInstance replacement = chain.ReplaceCreatureCatch(target.InstanceId, Card(), 0, resolver, new System.Random(1));
        Require(replacement != null && replacement.CurrentWeight == 1 && store.Effects.Length == 0,
            "Exchange retained old recipient modifiers or tracked replacement under old identity.");
        store.TryAddModifier(3, source, penalty, replacement.InstanceId, 1, 0, EventEffectLifetime.UntilRunEnds(), out id);
        NegativeEffectOption captured = store.GetNegativeOptions()[0]; long oldEpoch = store.Epoch;
        store.QueuePending(3, source, penalty, EventEffectLifetime.UntilRunEnds()); store.Reset();
        Require(store.Effects.Length == 0 && replacement.CurrentWeight == 1 && store.Epoch > oldEpoch,
            "Reset missed pending effects or target modifiers.");
        long next = store.QueuePending(4, source, penalty, EventEffectLifetime.AtBoundary("fixture_action", 1));
        Require(next > id && !captured.TryRemove() && !store.NotifyBoundary(oldEpoch, "fixture_action", 1)
            && store.Effects.Length == 1, "Old cleanse/boundary callback changed a new run's effect.");
        Require(store.NotifyBoundary(store.Epoch, "fixture_action", 1) && store.Effects.Length == 0,
            "New-run boundary tokens could not restart after reset.");
        replacement.AddEventModifier(999, 1, 0, penalty, source); chain.Recalculate(resolver);
        NegativeEffectOption[] fallback = NegativeEffectRuntime.GetOptions(chain, new EncounterRuntime(), new TechniqueEffectRuntime(), resolver, store);
        Require(fallback.Length == 1 && fallback[0].TryRemove() && replacement.CurrentWeight == 1,
            "Unowned provenance was silently excluded from existing modifier cleansing.");
        store.QueuePending(5, source, penalty, EventEffectLifetime.UntilRunEnds()); bool rejected = false;
        try { store.Bind(new CatchChainRuntime(), resolver); } catch (InvalidOperationException) { rejected = true; }
        Require(rejected && store.Effects.Length == 1, "Active records rebound to foreign recipient IDs.");
        store.Reset(); store.Bind(new CatchChainRuntime(), resolver); Require(store.Effects.Length == 0, "Reset store could not bind another run owner.");
    }

    /// <summary>Checks persistence survives final event discard and is cleared by actual controller run-ending/new-run paths.</summary>
    private static void ValidateDispatchAndController()
    {
        EncounterRuntime encounters = new EncounterRuntime(); CatchChainRuntime chain = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver();
        EventPersistenceRuntime store = Store(chain, resolver); CardDefinition source = Card(CardType.Environment);
        Set(source, "effects", new[] { Effect(CardEffectTone.Positive) }); encounters.SetCurrentEncounter(source);
        CardInstance target = chain.Add(Card(), resolver);
        EventResolutionContext context = new EventResolutionContext(encounters, chain, new CreatureEffectRuntime(), new TechniqueDeckRuntime(),
            resolver, new System.Random(1), store);
        EventEffectRuntime dispatch = new EventEffectRuntime(); dispatch.RegisterHandler(new ModifierHandler(target.InstanceId));
        Require(dispatch.TryBeginResolution(context, out _) && dispatch.AdvanceResolution(context) == EventResolutionProgress.Completed
            && encounters.CurrentEncounter == null && store.Effects.Length == 1 && target.CurrentValue == 11,
            "Final event discard removed its completed lasting consequence.");
        store.Reset(); Require(target.CurrentValue == 8, "Dispatcher-created contribution could not clean up independently.");

        GameObject obj = new GameObject("Event persistence controller checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>(); Set(controller, "startRunOnAwake", false);
        Set(controller, "startingBiome", Biome(Card())); Set(controller, "startingLineCapacity", 100); controller.StartRun();
        chain = Get<CatchChainRuntime>(controller, "catchChainRuntime"); resolver = Get<EffectResolver>(controller, "effectResolver");
        store = Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime"); target = chain.Add(Card(), resolver);
        CardEffectDefinition negative = Effect(CardEffectTone.Negative);
        store.TryAddModifier(1, source, negative, target.InstanceId, 2, -3, EventEffectLifetime.UntilRunEnds(), out _);
        long pending = store.QueuePending(1, source, negative, EventEffectLifetime.UntilRunEnds());
        Require(controller.RemovableNegativeEffects.Length == 2, "Controller Quick Clean did not see pending/applied event occurrences once each.");
        bool surfaced = controller.TrySurface();
        Require(surfaced && !controller.RunActive && store.Effects.Length == 0 && chain.Catches.Length == 0
            && target.CurrentValue == 8, "Actual run-ending cleanup retained live effects or modifier contributions: surfaced=" + surfaced + " active=" + controller.RunActive + " records=" + store.Effects.Length + " catches=" + chain.Catches.Length + " value=" + target.CurrentValue);
        FishingRunResult result = Get<FishingRunResult>(controller, "lastSurfaceResult");
        Require(result.HaulValue == 5 && result.Haul[0].CurrentValue == 5,
            "Live cleanup altered already-recorded haul results/snapshots.");
        controller.StartRun(); target = chain.Add(Card(), resolver);
        long after = store.QueuePending(2, source, negative, EventEffectLifetime.UntilRunEnds());
        store.TryAddModifier(2, source, negative, target.InstanceId, 1, 0, EventEffectLifetime.UntilRunEnds(), out _);
        controller.StartRun();
        Require(after > pending && store.Effects.Length == 0 && target.CurrentWeight == 1 && controller.RemovableNegativeEffects.Length == 0,
            "New-run initialization retained old pending/modifier ownership.");
        store.QueuePending(3, source, negative, EventEffectLifetime.UntilRunEnds());
        CardDefinition shrimp = Card(); CardEffectDefinition clean = Effect(CardEffectTone.Positive);
        Set(clean, "effectType", CardEffectType.RemoveNegativeEffect); Set(clean, "trigger", CardEffectTrigger.WhenCaught);
        Set(shrimp, "effects", new[] { clean });
        CreatureEffectRuntime creatures = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        creatures.RecordAttachment(chain.Add(shrimp, resolver), false, chain.Catches);
        controller.GetType().GetMethod("PrepareCreatureChoice", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null);
        Require(controller.PendingCreatureChoice == CreatureEffectChoiceKind.RemoveNegativeEffect
            && controller.TryChooseNegativeEffect(0) && store.Effects.Length == 0 && !controller.HasPendingCreatureChoice,
            "Actual Quick Clean could not remove a pending event occurrence through the controller.");
        controller.StartRun(); target = chain.Add(Card(), resolver);
        store.TryAddModifier(4, source, negative, target.InstanceId, 2, -3, EventEffectLifetime.UntilRunEnds(), out _);
        store.QueuePending(4, source, negative, EventEffectLifetime.UntilRunEnds());
        LineLoadRiskRuntime risk = Get<LineLoadRiskRuntime>(controller, "lineLoadRiskRuntime");
        Set(risk, "baseBreakChance", 1f); Set(risk, "maximumBreakChance", 1f); Set(controller, "lineCapacity", 0);
        Require(controller.TrySurface() && controller.LastRunEndedByLineBreak && store.Effects.Length == 0 && !controller.RunActive,
            "Line-break ending retained unfinished event effects.");
        result = Get<FishingRunResult>(controller, "lastSurfaceResult");
        Require(result.LostCatches.Length == 1 && result.LostCatches[0].CurrentValue == 5,
            "Line-break cleanup changed the captured loss snapshot.");
    }

    /// <summary>Creates a bound event persistence owner.</summary>
    private static EventPersistenceRuntime Store(CatchChainRuntime chain, EffectResolver resolver)
    { EventPersistenceRuntime store = new EventPersistenceRuntime(); store.Bind(chain, resolver); return store; }
    /// <summary>Creates immutable creature/event fixture data without production assets.</summary>
    private static CardDefinition Card(CardType type = CardType.Creature)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card); Set(card, "cardType", type);
        Set(card, "displayName", type.ToString()); Set(card, "weight", type == CardType.Creature ? 1 : 0);
        Set(card, "value", type == CardType.Creature ? 8 : 0); Set(card, "maximumDepth", 20); Set(card, "hasExplicitBaitData", true); return card;
    }
    /// <summary>Creates a neutral biome fixture for controller setup.</summary>
    private static BiomeDefinition Biome(CardDefinition card)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { card });
        Set(biome, "depthTiers", new[] { tier }); return biome;
    }
    /// <summary>Creates explicit effect tone/provenance fixture data.</summary>
    private static CardEffectDefinition Effect(CardEffectTone tone)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", CardEffectType.ModifyCatchValue);
        Set(effect, "effectTone", tone); return effect;
    }
    /// <summary>Sets fixtures through the existing serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    { owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads controller runtime owners for integrated cleanup checks.</summary>
    private static T Get<T>(object owner, string name)
    { return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Counts invariants and reports the first failure.</summary>
    private static void Require(bool condition, string message)
    { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
