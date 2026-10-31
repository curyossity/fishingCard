using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks confirmed event actions, reaction suspension, exact completion, failures, and run cleanup.</summary>
public static class EventActionChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
    private sealed class Handler : IEventEffectHandler
    {
        public CardEffectType EffectType => CardEffectType.ModifyCatchValue;
        public int Calls;
        public Func<EventResolutionContext, EventEffectOutcome> Action;
        public Action Validate;
        /// <summary>Validates fixture data while checking that confirmation cannot be reentered.</summary>
        public bool CanResolve(CardEffectDefinition effect, out string reason) { Validate?.Invoke(); reason = string.Empty; return true; }
        /// <summary>Applies fixture consequences through actual runtime owners.</summary>
        public EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect) { Calls++; return Action(context); }
    }
#if UNITY_EDITOR
    /// <summary>Runs event-to-core-action integration checks in Unity.</summary>
    [MenuItem("Fishing Cards/Validation/Confirmed Event Actions")]
    public static void Run() { Debug.Log("Confirmed event actions passed: " + ValidateRules() + " assertions."); }
#endif
    /// <summary>Executes coordinator integration checks and disposes all fixture objects.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateImmediate(); ValidatePaused(false); ValidatePaused(true); ValidateCancellationAndReplacement(); ValidateFailuresAndCleanup(); ValidatePresentationIsolation(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks empty/no-target events, normal depth advance, no attachments, and Surface consequences before haul recording.</summary>
    private static void ValidateImmediate()
    {
        CardDefinition source = Event(); FishingRunController controller = Controller(source);
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EventEncounterOccurrence old = encounters.CurrentEventOccurrence; int depth = controller.CurrentDepth;
        Require(controller.TryDescend() && controller.CurrentDepth == depth + 1 && chain.Catches.Length == 0
            && old.State == EventOccurrenceState.Resolved && !controller.HasConfirmedEventAction,
            "Effectless event attached or advanced more than once.");
        Require(encounters.CurrentEventOccurrence.Id != old.Id && encounters.RecentEncounterHistory.Length == 2,
            "Completed Descend failed normal next reveal or drew an extra encounter.");
        Invoke(controller, "ResumeAfterCreatureChoices");
        Require(controller.CurrentDepth == depth + 1 && chain.Catches.Length == 0, "Repeated resume performed a completed action again.");
        source = Event(Effect()); controller = Controller(source); Handler noTarget = new Handler { Action = ctx => EventEffectOutcome.NoEligibleTarget };
        controller.RegisterEventEffectHandler(noTarget); encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        int cancellations = 0; encounters.HazardCancelled += card => cancellations++;
        old = encounters.CurrentEventOccurrence;
        Require(controller.TryDescend() && noTarget.Calls == 1 && old.State == EventOccurrenceState.Resolved && cancellations == 0,
            "No-target event was cancelled or failed the confirmed action.");
        source = Event(Effect()); controller = Controller(source); chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver"); CardInstance target = chain.Add(Creature(), resolver);
        Handler value = new Handler { Action = ctx =>
        {
            Require(controller.PendingEventAction == ConfirmedEventAction.Surface && !controller.TryDescend()
                && !controller.TrySurface() && !controller.TryReleaseCatch(0), "Surface consequence did not lock every core action.");
            ctx.Persistence.TryAddModifier(ctx.OccurrenceId, ctx.Source, ctx.Source.Effects[0], target.InstanceId,
                0, 3, EventEffectLifetime.UntilRunEnds(), out _); return EventEffectOutcome.Applied;
        } };
        controller.RegisterEventEffectHandler(value); depth = controller.CurrentDepth;
        Require(controller.TrySurface() && value.Calls == 1 && !controller.RunActive && controller.CurrentDepth == depth,
            "Surface did not execute its event first or advanced depth.");
        FishingRunResult result = Get<FishingRunResult>(controller, "lastSurfaceResult");
        Require(result.HaulValue == 11 && result.Haul.Length == 1 && result.Haul[0].CurrentValue == 11 && target.CurrentValue == 8,
            "Surface snapshotted before consequences or live cleanup altered the haul.");
        Require(Get<EventEffectRuntime>(controller, "eventEffectRuntime").Progress == EventResolutionProgress.Inactive
            && Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime").Effects.Length == 0
            && !controller.HasConfirmedEventAction, "Completed run retained dispatcher/persistence/action ownership.");
    }

    /// <summary>Checks both confirmed actions wait for required creature discard choices and reject stale/reentrant input.</summary>
    private static void ValidatePaused(bool surface)
    {
        CardDefinition source = Event(Effect()); FishingRunController controller = Controller(source);
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime"); EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        CardDefinition creature = Creature(); CardEffectDefinition draw = Effect(); Set(draw, "effectType", CardEffectType.DiscardTechniqueOnAttach);
        Set(draw, "trigger", CardEffectTrigger.WhenCaught); Set(creature, "effects", new[] { draw });
        CardInstance target = chain.Add(creature, resolver); TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime");
        CardDefinition delayed = Technique(); Set(deck, "hand", new[] { delayed, delayed, delayed, delayed });
        Set(deck, "drawPile", new[] { delayed, delayed }); int depth = controller.CurrentDepth;
        Handler handler = new Handler { Validate = () => Require(!controller.TryDescend() && !controller.TrySurface()
            && !controller.CanUseTechniqueCard(0, out _), "Preflight allowed a nested confirmation or Technique."), Action = ctx =>
        {
            ctx.Persistence.TryAddModifier(ctx.OccurrenceId, ctx.Source, ctx.Source.Effects[0], target.InstanceId,
                0, 3, EventEffectLifetime.UntilRunEnds(), out _);
            ctx.Creatures.RecordAttachment(target, false, ctx.Chain.Catches);
            Invoke(controller, "RefreshViews");
            Require(!controller.TryChooseRequiredTechniqueDiscard(-1) && deck.Hand.Length == 4 && deck.DrawPile.Length == 2,
                "Partial consequence processed a creature choice before its handler returned.");
            return EventEffectOutcome.Applied;
        } }; controller.RegisterEventEffectHandler(handler);
        Require(surface ? controller.TrySurface() : controller.TryDescend(), "Event action was not accepted.");
        Require(controller.RunActive && controller.CurrentDepth == depth && handler.Calls == 1
            && controller.PendingEventAction == (surface ? ConfirmedEventAction.Surface : ConfirmedEventAction.Descend)
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique,
            "Confirmed action executed before its creature reactions finished.");
        Require(!controller.TryDescend() && !controller.TrySurface() && !controller.TryReleaseCatch(0)
            && !TechniqueRuleChecks.Play(controller, 0) && !controller.TryEnterNextBiome(), "A pending event action was replaced/interrupted.");
        Invoke(controller, "ResumeAfterCreatureChoices"); Invoke(controller, "RefreshViews");
        Require(handler.Calls == 1 && controller.CurrentDepth == depth && deck.DrawPile.Length == 2, "Repeated callbacks replayed an effect or action.");
        Require(controller.TryChooseRequiredTechniqueDiscard(0), "Creature reaction could not make room for its draw.");
        Require(handler.Calls == 1 && !controller.HasConfirmedEventAction && !controller.HasPendingCreatureChoice,
            "Finished creature reaction did not finish the confirmed action exactly once.");
        if (surface)
        {
            Require(!controller.RunActive && controller.CurrentDepth == depth
                && Get<FishingRunResult>(controller, "lastSurfaceResult").HaulValue == 11,
                "Paused Surface lost event effects or performed Descend.");
        }
        else
        {
            Require(controller.RunActive && controller.CurrentDepth == depth + 1 && chain.Catches.Length == 1
                && deck.Hand.Length == 3 && deck.DrawPile.Length == 2 && controller.CanUseTechniqueCard(0, out _),
                "Paused Descend added an event catch, changed draw order, or failed the next window.");
        }
        depth = controller.CurrentDepth; Invoke(controller, "ResumeAfterCreatureChoices");
        Require(!controller.TryChooseRequiredTechniqueDiscard(-1) && controller.CurrentDepth == depth && handler.Calls == 1,
            "Old choice completion caused a second action.");
    }

    /// <summary>Checks cancelled windows, reveal holds, replacement, and independent cancellation rewards remain separate from consequences.</summary>
    private static void ValidateCancellationAndReplacement()
    {
        CardDefinition source = Event(Effect()); Set(source, "cardType", CardType.Hazard);
        FishingRunController controller = Controller(source); EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime"); EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        Handler handler = new Handler { Action = ctx => EventEffectOutcome.Applied }; controller.RegisterEventEffectHandler(handler);
        encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id);
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"); Set(deck, "hand", new[] { Technique() });
        Require(controller.CanUseTechniqueCard(0, out _) && TechniqueRuleChecks.Play(controller, 0), "Cancelled event lost its pre-confirmation Technique window.");
        int depth = controller.CurrentDepth;
        Require(controller.TrySurface() && handler.Calls == 0 && !controller.RunActive && controller.CurrentDepth == depth,
            "Cancelled Hazard executed consequences or advanced on Surface.");
        controller = Controller(source); encounters = Get<EncounterRuntime>(controller, "encounterRuntime"); controller.RegisterEventEffectHandler(handler);
        encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id); depth = controller.CurrentDepth;
        Require(controller.TryDescend() && handler.Calls == 0 && controller.CurrentDepth == depth + 1
            && !controller.HasConfirmedEventAction, "Cancelled Hazard did not perform only its confirmed Descend.");
        controller = Controller(Event()); encounters = Get<EncounterRuntime>(controller, "encounterRuntime"); long token = 0;
        encounters.EncounterRevealEffects += card => { encounters.TryDeferEventRevealReaction(encounters.CurrentEventOccurrence.Id, out token); };
        encounters.SetCurrentEncounter(Event()); depth = controller.CurrentDepth;
        Require(!controller.TryDescend() && !controller.TrySurface() && !controller.HasConfirmedEventAction,
            "Unfinished reveal reactions allowed confirmation.");
        encounters.TryCompleteEventRevealReaction(encounters.CurrentEventOccurrence.Id, token);
        Require(controller.TryDescend() && controller.CurrentDepth == depth + 1, "Completed reveal reactions failed to permit the action.");

        CardDefinition original = Event(Effect()), replacement = Event(); controller = Controller(original, replacement);
        encounters = Get<EncounterRuntime>(controller, "encounterRuntime"); encounters.SetCurrentEncounter(original);
        controller.RegisterEventEffectHandler(handler); EventEncounterOccurrence old = encounters.CurrentEventOccurrence;
        CardDefinition replace = Technique(); Set(replace, "effects", new[] { Effect(CardEffectType.ReplaceEncounter, CardEffectTarget.CurrentEncounter) });
        deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"); Set(deck, "hand", new[] { replace });
        Require(TechniqueRuleChecks.Play(controller, 0) && old.State == EventOccurrenceState.Replaced && encounters.CurrentEncounter == replacement,
            "Event replacement failed before confirmation.");
        depth = controller.CurrentDepth;
        Require(controller.TryDescend() && handler.Calls == 0 && controller.CurrentDepth == depth + 1,
            "Replacing an unresolved event did not suppress its old consequences.");
    }

    /// <summary>Checks unsupported preflight leaves input open, failures cannot retry/react, restart cleanup, and post-effect overload.</summary>
    private static void ValidateFailuresAndCleanup()
    {
        CardDefinition source = Event(Effect()); FishingRunController controller = Controller(source);
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime"); int depth = controller.CurrentDepth;
        Require(!controller.TryDescend() && !controller.TrySurface() && !controller.HasConfirmedEventAction
            && controller.CurrentDepth == depth && encounters.HasEventReactionWindow && controller.EventActionFailure.Contains("Unsupported"),
            "Unsupported event was silently skipped or partially confirmed.");
        controller = Controller(source); Handler fault = new Handler { Action = ctx =>
        {
            CardDefinition creature = Creature(); CardEffectDefinition draw = Effect(); Set(draw, "effectType", CardEffectType.DrawTechniqueOnAttach);
            Set(draw, "trigger", CardEffectTrigger.WhenCaught); Set(creature, "effects", new[] { draw });
            ctx.Creatures.RecordAttachment(ctx.Chain.Add(creature, ctx.Resolver), false, ctx.Chain.Catches);
            ctx.Persistence.QueuePending(ctx.OccurrenceId, ctx.Source, ctx.Source.Effects[0], EventEffectLifetime.UntilRunEnds());
            throw new InvalidOperationException("Fixture action failure");
        } }; controller.RegisterEventEffectHandler(fault);
        TechniqueDeckRuntime deck = Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"); Set(deck, "drawPile", new[] { Technique() });
        Require(controller.TryDescend() && controller.HasConfirmedEventAction && controller.EventActionFailure.Contains("Fixture action failure")
            && fault.Calls == 1 && deck.DrawPile.Length == 1 && controller.PendingCreatureChoice == CreatureEffectChoiceKind.None,
            "Faulting consequence reopened/processed partial reactions.");
        Invoke(controller, "ResumeAfterCreatureChoices");
        Require(!controller.TryChooseRequiredTechniqueDiscard(-1) && !controller.TrySurface() && fault.Calls == 1,
            "Failed confirmed action was retried or bypassed through a choice.");
        controller.StartRun();
        Require(!controller.HasConfirmedEventAction && Get<EventEffectRuntime>(controller, "eventEffectRuntime").Progress == EventResolutionProgress.Inactive
            && Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime").Effects.Length == 0
            && Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime").ChoiceKind == CreatureEffectChoiceKind.None,
            "Restart did not abort pending dispatcher/action/reaction/effect state.");
        controller = Controller(source); CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver"); CardInstance target = chain.Add(Creature(), resolver);
        Handler weight = new Handler { Action = ctx =>
        {
            ctx.Persistence.TryAddModifier(ctx.OccurrenceId, ctx.Source, ctx.Source.Effects[0], target.InstanceId,
                5, -3, EventEffectLifetime.UntilRunEnds(), out _); return EventEffectOutcome.Applied;
        } }; controller.RegisterEventEffectHandler(weight);
        Set(controller, "lineCapacity", 1); LineLoadRiskRuntime risk = Get<LineLoadRiskRuntime>(controller, "lineLoadRiskRuntime");
        Set(risk, "baseBreakChance", 1f); Set(risk, "maximumBreakChance", 1f); depth = controller.CurrentDepth;
        Require(controller.TryDescend() && controller.LastRunEndedByLineBreak && !controller.RunActive && controller.CurrentDepth == depth
            && weight.Calls == 1 && !controller.HasConfirmedEventAction, "Overload was checked before event consequences or advanced after break.");
        FishingRunResult result = Get<FishingRunResult>(controller, "lastSurfaceResult");
        Require(result.SurfaceLineLoad == 6 && result.LostCatches[0].CurrentValue == 5
            && Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime").Effects.Length == 0
            && Get<EncounterRuntime>(controller, "encounterRuntime").ActiveEncounterChain == null,
            "Line-break cleanup lost post-event snapshots or retained event trails.");
    }

    /// <summary>Checks ascent snapshots preserve values and reference snapshots while all gameplay owners are already cleared.</summary>
    private static void ValidatePresentationIsolation()
    {
        FishingRunController controller = Controller(Event()); CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver"); CardDefinition creature = Creature();
        CardEffectDefinition passive = Effect(CardEffectType.ModifyCatchValue, CardEffectTarget.Self);
        Set(passive, "trigger", CardEffectTrigger.WhileAttached); Set(creature, "effects", new[] { passive });
        CardInstance target = chain.Add(creature, resolver);
        EventPersistenceRuntime persistence = Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime");
        persistence.TryAddModifier(1, Event(), Effect(), target.InstanceId, 0, 3, EventEffectLifetime.UntilRunEnds(), out _);
        Invoke(controller, "CaptureEndedRunPresentation");
        CardInstance[] copies = Get<CardInstance[]>(controller, "endedRunPresentationCatches");
        ActiveCatchEffectRecord[] records = Get<ActiveCatchEffectRecord[]>(controller, "endedRunPresentationEffects");
        Require(copies.Length == 1 && copies[0] != target && copies[0].CurrentValue == 12
            && records.Length == 1 && records[0].SourceInstance == copies[0],
            "Ascent retained mutable gameplay instances or original effect-record sources.");
        Set(controller, "runActive", false); Set(controller, "runEndCleanupPending", true);
        Invoke(controller, "ResetEndedRunRuntimeState");
        Require(chain.Catches.Length == 0 && persistence.Effects.Length == 0
            && Get<EncounterRuntime>(controller, "encounterRuntime").CurrentEventOccurrence == null && copies[0].CurrentValue == 12,
            "Immediate gameplay cleanup altered frozen values or retained live event state.");
        CardInstance[] displayed = (CardInstance[])controller.GetType().GetProperty("DisplayedCatches", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
        Require(displayed == copies && target.CurrentValue == 9, "Ascent did not use frozen copies after live modifiers expired.");
        Invoke(controller, "ClearEndedRunState");
        Require(Get<CardInstance[]>(controller, "endedRunPresentationCatches").Length == 0
            && Get<ActiveCatchEffectRecord[]>(controller, "endedRunPresentationEffects").Length == 0,
            "Ascent completion retained presentation copies.");
    }

    /// <summary>Creates a controller with a real coastal encounter pool and deterministic hand/runtime owners.</summary>
    private static FishingRunController Controller(params CardDefinition[] pool)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome); Set(biome, "biomeId", "Coastal");
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition(); Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20);
        Set(tier, "encounterPool", pool); Set(biome, "depthTiers", new[] { tier });
        GameObject obj = new GameObject("Confirmed event action checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>(); Set(controller, "startRunOnAwake", false);
        Set(controller, "startingBiome", biome); Set(controller, "startingLineCapacity", 100); controller.StartRun(); return controller;
    }
    /// <summary>Creates an event fixture with explicit effect data.</summary>
    private static CardDefinition Event(params CardEffectDefinition[] effects)
    { CardDefinition card = Creature(); Set(card, "cardType", CardType.Environment); Set(card, "effects", effects); return card; }
    /// <summary>Creates a simple creature recipient fixture without production assets.</summary>
    private static CardDefinition Creature()
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "maximumDepth", 20); Set(card, "hasExplicitBaitData", true); return card;
    }
    /// <summary>Creates a legal delayed Technique for reaction-window/input tests.</summary>
    private static CardDefinition Technique()
    { CardDefinition card = Creature(); Set(card, "cardType", CardType.Technique); Set(card, "effects", new[] { Effect(CardEffectType.ModifyDescendDistance, CardEffectTarget.NextDescend) }); return card; }
    /// <summary>Creates existing effect-type data with an explicit amount.</summary>
    private static CardEffectDefinition Effect(CardEffectType type = CardEffectType.ModifyCatchValue, CardEffectTarget target = CardEffectTarget.SpecificCaughtCard)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "target", target);
        Set(effect, "amount", 1); return effect;
    }
    /// <summary>Configures fixtures through serialized fields.</summary>
    private static void Set(object owner, string name, object value) { owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads controller-owned runtimes for integrated checks.</summary>
    private static T Get<T>(object owner, string name) { return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Invokes preparation/resumption boundaries without copying their behavior.</summary>
    private static void Invoke(object owner, string name) { owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null); }
    /// <summary>Counts invariants and reports the first failure.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
