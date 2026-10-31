using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks validated once-only event dispatch and completion after actual creature choices.</summary>
public static class EventResolutionChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;

    /// <summary>Fixture handler exercising the production dispatcher contract and existing runtime owners.</summary>
    private sealed class Handler : IEventEffectHandler
    {
        public CardEffectType EffectType { get; }
        public int Calls { get; private set; }
        public bool Accept = true;
        public bool ThrowDuringValidation;
        public Func<EventResolutionContext, CardEffectDefinition, EventEffectOutcome> Action;

        /// <summary>Configures a fixture handler without registering any production card behavior.</summary>
        public Handler(CardEffectType type) { EffectType = type; }
        /// <summary>Validates fixture effects using their explicitly authored immediate trigger.</summary>
        public bool CanResolve(CardEffectDefinition effect, out string reason)
        {
            if (ThrowDuringValidation) throw new InvalidOperationException("Fixture validation failure");
            reason = Accept && effect.Trigger == CardEffectTrigger.Manual ? string.Empty : "Fixture supports immediate Manual effects only";
            return string.IsNullOrEmpty(reason);
        }
        /// <summary>Counts dispatch calls and forwards mutations to actual runtime owners.</summary>
        public EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect)
        {
            Calls++; return Action == null ? EventEffectOutcome.NoEligibleTarget : Action(context, effect);
        }
    }
#if UNITY_EDITOR
    /// <summary>Runs the consequence and creature-reaction boundary checks in Unity.</summary>
    [MenuItem("Fishing Cards/Validation/Event Consequence Resolution")]
    public static void Run() { Debug.Log("Event consequence resolution passed: " + ValidateRules() + " assertions."); }
#endif
    /// <summary>Runs actual dispatcher/queue tests and disposes fixture assets.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidatePreflight(); ValidateCompletion(); ValidateReactions(); ValidateFaultsAndOwners(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks the entire plan is validated before any occurrence claim, including malformed or unsupported data.</summary>
    private static void ValidatePreflight()
    {
        EventResolutionContext context = Context(Card(Effect(CardEffectType.ModifyCatchValue), Effect(CardEffectType.AddLineLoadModifier)));
        EventEffectRuntime runtime = new EventEffectRuntime(); Handler handler = new Handler(CardEffectType.ModifyCatchValue);
        runtime.RegisterHandler(handler);
        Require(!runtime.TryBeginResolution(context, out string reason) && reason.Contains("Unsupported") && handler.Calls == 0
            && context.Encounters.HasTechniqueReactionWindow && context.Encounters.CurrentEventOccurrence.State == EventOccurrenceState.Unresolved,
            "Unsupported later effect allowed partial execution or closed the event window.");
        Handler later = new Handler(CardEffectType.AddLineLoadModifier) { Accept = false }; runtime.RegisterHandler(later);
        Require(!runtime.TryBeginResolution(context, out reason) && handler.Calls == 0 && !runtime.IsResolving,
            "Handler validation failure claimed consequences.");
        later.Accept = true; later.ThrowDuringValidation = true;
        Require(!runtime.TryBeginResolution(context, out reason) && reason.Contains("validation failed")
            && context.Encounters.HasTechniqueReactionWindow, "Validation exception claimed the event or escaped without a reason.");
        later.ThrowDuringValidation = false;
        Require(runtime.TryBeginResolution(context, out _) && runtime.Progress == EventResolutionProgress.Ready
            && handler.Calls == 0 && !context.Encounters.HasTechniqueReactionWindow, "Validated begin dispatched too early or left Technique input open.");
        Require(!runtime.TryBeginResolution(context, out _), "Repeated begin claimed the event twice.");
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Completed && handler.Calls == 1 && later.Calls == 1,
            "Validated effect plan did not dispatch in full.");
        bool duplicate = false; try { runtime.RegisterHandler(handler); } catch (ArgumentException) { duplicate = true; }
        Require(duplicate, "Handler registration silently replaced an approved implementation.");
        context = Context(Card((CardEffectDefinition)null)); runtime = new EventEffectRuntime();
        Require(!runtime.TryBeginResolution(context, out reason) && reason.Contains("missing definition"), "Null effect data was silently dropped.");
        context = Context(Card()); context.Encounters.Reset(); runtime = new EventEffectRuntime();
        Require(!runtime.TryBeginResolution(context, out _), "Stale context claimed a reset event.");
    }

    /// <summary>Checks empty/no-target events resolve without cancellation, repeated advances, and stale/cancelled/replaced contexts.</summary>
    private static void ValidateCompletion()
    {
        EventResolutionContext context = Context(Card()); EventEffectRuntime runtime = new EventEffectRuntime();
        int cancellations = 0; context.Encounters.HazardCancelled += card => cancellations++;
        Require(runtime.TryBeginResolution(context, out _) && runtime.AdvanceResolution(context) == EventResolutionProgress.Completed,
            "Effectless event did not complete.");
        Require(context.Encounters.CurrentEventOccurrence.State == EventOccurrenceState.Resolved && context.Encounters.CurrentEncounter == null
            && context.Chain.Catches.Length == 0 && cancellations == 0, "Event attached or was mistaken for cancellation.");
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Completed && !runtime.TryBeginResolution(context, out _),
            "Repeated input reopened completed event consequences.");
        context = Context(Card(Effect(CardEffectType.ModifyCatchValue))); runtime = new EventEffectRuntime();
        Handler handler = new Handler(CardEffectType.ModifyCatchValue); runtime.RegisterHandler(handler);
        context.Encounters.HazardCancelled += card => cancellations++;
        Require(runtime.TryBeginResolution(context, out _) && runtime.AdvanceResolution(context) == EventResolutionProgress.Completed
            && runtime.NoTargetEffectCount == 1 && runtime.AppliedEffectCount == 0 && handler.Calls == 1
            && cancellations == 0 && context.Chain.ReleasedCatches.Length == 0, "No-target effect failed or emitted cancellation/Release.");
        CardDefinition hazard = Card(Effect(CardEffectType.ModifyCatchValue)); Set(hazard, "cardType", CardType.Hazard);
        context = Context(hazard); context.Encounters.TryCancelRevealedHazard(context.OccurrenceId); runtime = new EventEffectRuntime(); runtime.RegisterHandler(handler);
        Require(!runtime.TryBeginResolution(context, out _) && context.Encounters.HasEventReactionWindow && handler.Calls == 1,
            "Cancelled event dispatched consequences or destroyed its pre-action window.");
        context = Context(Card(Effect(CardEffectType.ModifyCatchValue))); runtime = new EventEffectRuntime(); runtime.RegisterHandler(handler);
        EventEncounterOccurrence old = context.Encounters.CurrentEventOccurrence; context.Encounters.SetCurrentEncounter(Card());
        Require(old.State == EventOccurrenceState.Replaced && !runtime.TryBeginResolution(context, out _) && handler.Calls == 1,
            "Replaced occurrence remained dispatchable.");
        EventResolutionContext fresh = Context(Card(Effect(CardEffectType.ModifyCatchValue)));
        runtime.Reset();
        Require(runtime.TryBeginResolution(fresh, out _) && runtime.AdvanceResolution(fresh) == EventResolutionProgress.Completed
            && handler.Calls == 2 && runtime.NoTargetEffectCount == 1, "Reusable dispatcher retained a previous occurrence's cursor/counters.");
        fresh = Context(Card(Effect(CardEffectType.ModifyCatchValue))); runtime.Reset();
        Require(runtime.TryBeginResolution(fresh, out _), "Fixture snapshot claim failed.");
        Set(fresh.Source, "effects", new[] { Effect(CardEffectType.ReleaseCatch) });
        Require(runtime.AdvanceResolution(fresh) == EventResolutionProgress.Completed && handler.Calls == 3,
            "Replacing the definition array changed the already-validated plan.");
        runtime.Reset(); Require(runtime.Progress == EventResolutionProgress.Inactive && runtime.Source == null && !runtime.IsResolving,
            "Run cleanup retained consequence ownership.");
    }

    /// <summary>Checks consequence order and once-only dispatch while the actual creature queue resolves a full-hand draw choice.</summary>
    private static void ValidateReactions()
    {
        EventResolutionContext context = Context(Card(Effect(CardEffectType.ModifyCatchValue), Effect(CardEffectType.AddLineLoadModifier)));
        CardDefinition creature = Card(); Set(creature, "cardType", CardType.Creature);
        Set(creature, "effects", new[] { Effect(CardEffectType.DrawTechniqueOnAttach, CardEffectTrigger.WhenCaught) });
        CardInstance target = context.Chain.Add(creature, context.Resolver);
        Set(context.Deck, "hand", new[] { Technique(), Technique(), Technique(), Technique() });
        Set(context.Deck, "drawPile", new[] { Technique(), Technique() });
        EventEffectRuntime runtime = new EventEffectRuntime();
        Handler first = new Handler(CardEffectType.ModifyCatchValue) { Action = (ctx, effect) =>
        {
            target.AddPermanentModifiers(0, 2, effect, ctx.Source); ctx.Chain.Recalculate(ctx.Resolver);
            ctx.Creatures.RecordAttachment(target, false, ctx.Chain.Catches);
            Require(runtime.AdvanceResolution(ctx) != EventResolutionProgress.Completed, "Effect callback reentered completion.");
            return EventEffectOutcome.Applied;
        } };
        Handler second = new Handler(CardEffectType.AddLineLoadModifier) { Action = (ctx, effect) =>
        {
            Require(ctx.Creatures.ChoiceKind == CreatureEffectChoiceKind.None, "Later consequence ran before creature reactions finished.");
            target.AddPermanentModifiers(1, 0, effect, ctx.Source); ctx.Chain.Recalculate(ctx.Resolver); return EventEffectOutcome.Applied;
        } };
        runtime.RegisterHandler(first); runtime.RegisterHandler(second);
        Require(runtime.TryBeginResolution(context, out _) && runtime.AdvanceResolution(context) == EventResolutionProgress.WaitingForCreatureReactions
            && first.Calls == 1 && second.Calls == 0 && runtime.DispatchedEffectCount == 1, "Queued creature choice did not suspend dispatch.");
        Require(target.CurrentValue == 10 && target.CurrentWeight == 1 && context.Chain.Catches.Length == 1
            && context.Encounters.CurrentEventOccurrence.State == EventOccurrenceState.Resolving, "Effect mutation/attachment boundary was incorrect.");
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.WaitingForCreatureReactions && first.Calls == 1,
            "Repeated advance reexecuted the paused effect.");
        Prepare(context);
        Require(context.Creatures.ChoiceKind == CreatureEffectChoiceKind.None && context.Deck.Hand.Length == 5
            && context.Deck.DrawPile.Length == 1 && !context.Encounters.HasTechniqueReactionWindow,
            "Automatic creature draw was capped or reopened Technique input.");
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Completed && second.Calls == 1 && first.Calls == 1
            && runtime.AppliedEffectCount == 2 && target.CurrentWeight == 2 && target.CurrentValue == 10,
            "Resume missed the later consequence or repeated the earlier mutation.");
        Require(context.Encounters.CurrentEncounter == null && !context.Encounters.HasTechniqueReactionWindow
            && context.Chain.Catches[0] == target && context.Chain.ReleasedCatches.Length == 0, "Completed event attached/released a card or reopened input.");
        runtime.AdvanceResolution(context); Require(first.Calls == 1 && second.Calls == 1, "Post-completion refresh replayed effects.");

        EventResolutionContext pending = Context(Card());
        CardInstance priorAttachment = pending.Chain.Add(creature, pending.Resolver);
        pending.Creatures.RecordAttachment(priorAttachment, false, pending.Chain.Catches);
        EventEffectRuntime blocked = new EventEffectRuntime();
        Require(!blocked.TryBeginResolution(pending, out _) && pending.Encounters.CurrentEventOccurrence.State == EventOccurrenceState.Unresolved,
            "New event resolution overtook existing creature queue work.");
        Prepare(pending);
        Require(blocked.TryBeginResolution(pending, out _) && blocked.AdvanceResolution(pending) == EventResolutionProgress.Completed,
            "Prepared prior creature work failed to release the event boundary.");
        context = Context(Card(Effect(CardEffectType.ModifyCatchValue))); runtime = new EventEffectRuntime();
        first = new Handler(CardEffectType.ModifyCatchValue) { Action = (ctx, effect) =>
        {
            CardInstance attached = ctx.Chain.Add(creature, ctx.Resolver); ctx.Creatures.RecordAttachment(attached, false, ctx.Chain.Catches);
            return EventEffectOutcome.Applied;
        } }; runtime.RegisterHandler(first); Set(context.Deck, "drawPile", new[] { Technique() });
        Require(runtime.TryBeginResolution(context, out _) && runtime.AdvanceResolution(context) == EventResolutionProgress.WaitingForCreatureReactions,
            "Automatic creature reaction queue was ignored.");
        Prepare(context);
        Require(context.Deck.Hand.Length == 1 && runtime.AdvanceResolution(context) == EventResolutionProgress.Completed && first.Calls == 1,
            "Automatic reaction completion did not resume exactly once.");
    }

    /// <summary>Checks failed partial handlers cannot retry, foreign owners cannot complete events, and restored handlers can be rebound.</summary>
    private static void ValidateFaultsAndOwners()
    {
        CardDefinition source = Card(Effect(CardEffectType.ModifyCatchValue)); EventResolutionContext context = Context(source);
        EventEffectRuntime runtime = new EventEffectRuntime(); Handler handler = new Handler(CardEffectType.ModifyCatchValue)
        { Action = (ctx, effect) => throw new InvalidOperationException("Fixture dispatch failure") }; runtime.RegisterHandler(handler);
        Require(runtime.TryBeginResolution(context, out _) && runtime.AdvanceResolution(context) == EventResolutionProgress.Failed
            && runtime.FailureReason.Contains("Fixture dispatch failure") && handler.Calls == 1 && !context.Encounters.HasTechniqueReactionWindow,
            "Handler failure completed the event or reopened its window.");
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Failed && handler.Calls == 1 && !runtime.TryBeginResolution(context, out _),
            "Failed consequence was retried.");
        runtime.Reset(); context.Encounters.Reset(); Require(!runtime.IsResolving && context.Encounters.CurrentEventOccurrence == null,
            "Coordinated run reset retained unfinished event effects.");
        context = Context(source); runtime = new EventEffectRuntime(); handler = new Handler(CardEffectType.ModifyCatchValue); runtime.RegisterHandler(handler);
        runtime.TryBeginResolution(context, out _); EventResolutionContext foreign = Context(source);
        foreign.Encounters.TryBeginEventResolution(foreign.OccurrenceId, out _);
        Require(foreign.OccurrenceId == context.OccurrenceId && runtime.AdvanceResolution(foreign) == EventResolutionProgress.Failed
            && handler.Calls == 0 && foreign.Encounters.CurrentEventOccurrence.State == EventOccurrenceState.Resolving,
            "Coincident IDs in another run allowed dispatch through foreign owners.");
        context = Context(source); runtime = new EventEffectRuntime(); handler = new Handler(CardEffectType.ModifyCatchValue); runtime.RegisterHandler(handler);
        runtime.TryBeginResolution(context, out _);
        Get<Dictionary<CardEffectType, IEventEffectHandler>>(runtime, "handlers").Clear(); Set(runtime, "boundContext", null);
        runtime.RegisterHandler(handler);
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Completed && handler.Calls == 1,
            "Restored runtime setup could not rebind handlers/context before dispatch.");
        context = Context(source); runtime = new EventEffectRuntime(); runtime.RegisterHandler(new Handler(CardEffectType.ModifyCatchValue));
        runtime.TryBeginResolution(context, out _); context.Encounters.Reset();
        Require(runtime.AdvanceResolution(context) == EventResolutionProgress.Failed, "Reset occurrence accepted stale dispatch completion.");
    }

    /// <summary>Prepares actual creature work using runtime owners from the event context.</summary>
    private static void Prepare(EventResolutionContext context)
    {
        context.Creatures.PrepareChoice(context.Chain, context.Encounters, null, new TechniqueEffectRuntime(), context.Resolver,
            context.Random, () => 0, context.Deck);
    }
    /// <summary>Creates independent runtime ownership around an authored fixture event.</summary>
    private static EventResolutionContext Context(CardDefinition card)
    {
        EncounterRuntime encounters = new EncounterRuntime(); encounters.SetCurrentEncounter(card);
        return new EventResolutionContext(encounters, new CatchChainRuntime(), new CreatureEffectRuntime(), new TechniqueDeckRuntime(),
            new EffectResolver(), new System.Random(1), new EventPersistenceRuntime());
    }
    /// <summary>Creates event data without inventing or updating production card designs.</summary>
    private static CardDefinition Card(params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "cardType", CardType.Environment); Set(card, "effects", effects); Set(card, "weight", 1); Set(card, "value", 8);
        Set(card, "hasExplicitBaitData", true); return card;
    }
    /// <summary>Creates an immediate fixture effect with an explicitly supported handler.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger = CardEffectTrigger.Manual)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "trigger", trigger); Set(effect, "amount", 1); return effect;
    }
    /// <summary>Creates a Technique pile fixture for actual deck operations.</summary>
    private static CardDefinition Technique()
    {
        CardDefinition card = Card(); Set(card, "cardType", CardType.Technique); return card;
    }
    /// <summary>Configures fixtures through the existing serialized-field contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }
    /// <summary>Reads the transient registry for a minimal editor-reload rebinding simulation.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }
    /// <summary>Counts invariant assertions and reports the first failure.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++; if (!condition) throw new InvalidOperationException(message);
    }
}
