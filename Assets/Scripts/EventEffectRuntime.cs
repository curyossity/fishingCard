using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Validates and dispatches event consequences once, waiting for the creature queue before completion.</summary>
[Serializable]
public sealed class EventEffectRuntime
{
    [SerializeField] private long activeOccurrenceId;
    [SerializeField] private CardDefinition source;
    [SerializeField] private CardEffectDefinition[] effects = Array.Empty<CardEffectDefinition>();
    [SerializeField] private int nextEffectIndex;
    [SerializeField] private int appliedEffectCount;
    [SerializeField] private int noTargetEffectCount;
    [SerializeField] private EventResolutionProgress progress;
    [SerializeField] private string failureReason;
    private readonly Dictionary<CardEffectType, IEventEffectHandler> handlers = new Dictionary<CardEffectType, IEventEffectHandler>();
    private bool advancing;
    private EventResolutionContext boundContext;

    public long ActiveOccurrenceId => activeOccurrenceId;
    public CardDefinition Source => source;
    public EventResolutionProgress Progress => progress;
    public int DispatchedEffectCount => nextEffectIndex;
    public int AppliedEffectCount => appliedEffectCount;
    public int NoTargetEffectCount => noTargetEffectCount;
    public string FailureReason => failureReason;
    public bool IsDispatching => advancing;
    public bool IsResolving => activeOccurrenceId != 0;

    /// <summary>Reports registrations without replacing an already bound handler.</summary>
    public bool HasHandler(CardEffectType type) { return handlers.ContainsKey(type); }

    /// <summary>Registers one approved effect implementation; runtime setup must rebind handlers after editor reload.</summary>
    public void RegisterHandler(IEventEffectHandler handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (advancing) throw new InvalidOperationException("Cannot register an event handler during dispatch.");
        if (handlers.ContainsKey(handler.EffectType)) throw new ArgumentException("An event handler is already registered for " + handler.EffectType);
        handlers.Add(handler.EffectType, handler);
    }

    /// <summary>Validates the entire plan before claiming the unresolved occurrence; cancelled/replaced events never dispatch.</summary>
    public bool TryBeginResolution(EventResolutionContext context, out string reason)
    {
        reason = string.Empty;
        if (context == null || context.Source == null || IsResolving || advancing
            || context.Encounters.CurrentEventOccurrence?.Id != context.OccurrenceId
            || !context.Encounters.HasEventReactionWindow
            || context.Encounters.CurrentEventOccurrence.State != EventOccurrenceState.Unresolved
            || context.Creatures.ChoiceKind != CreatureEffectChoiceKind.None)
        {
            reason = "No unresolved event ready for consequence resolution";
            return false;
        }
        CardEffectDefinition[] plan = (CardEffectDefinition[])(context.Source.Effects ?? Array.Empty<CardEffectDefinition>()).Clone();
        foreach (CardEffectDefinition effect in plan)
        {
            if (effect == null || !handlers.TryGetValue(effect.EffectType, out IEventEffectHandler handler))
            {
                reason = "Unsupported event effect: " + (effect == null ? "missing definition" : effect.EffectType.ToString());
                return false;
            }
            try
            {
                if (!handler.CanResolve(effect, out reason))
                {
                    if (string.IsNullOrWhiteSpace(reason)) reason = "Invalid event effect: " + effect.EffectType;
                    return false;
                }
            }
            catch (Exception exception)
            {
                reason = "Event effect validation failed: " + exception.Message;
                return false;
            }
        }
        if (!context.Encounters.TryBeginEventResolution(context.OccurrenceId, out CardDefinition claimed))
        {
            reason = "Event changed before consequence resolution";
            return false;
        }
        boundContext = context;
        activeOccurrenceId = context.OccurrenceId;
        source = claimed;
        effects = plan;
        nextEffectIndex = 0;
        appliedEffectCount = 0;
        noTargetEffectCount = 0;
        failureReason = string.Empty;
        progress = EventResolutionProgress.Ready;
        return true;
    }

    /// <summary>Runs remaining effects in order and waits for all queued creature work before the next effect or final discard.</summary>
    public EventResolutionProgress AdvanceResolution(EventResolutionContext context)
    {
        if (!IsResolving) return progress;
        if (advancing || progress == EventResolutionProgress.Failed) return progress;
        if (context == null || context.Source != source || !UsesBoundRuntimeOwners(context)
            || context.OccurrenceId != activeOccurrenceId
            || context.Encounters.CurrentEventOccurrence?.Id != activeOccurrenceId
            || context.Encounters.CurrentEventOccurrence.State != EventOccurrenceState.Resolving)
            return Fail("Current event no longer matches this resolution");
        if (boundContext == null) boundContext = context;
        if (context.Creatures.ChoiceKind != CreatureEffectChoiceKind.None)
            return progress = EventResolutionProgress.WaitingForCreatureReactions;
        advancing = true;
        try
        {
            while (nextEffectIndex < effects.Length)
            {
                CardEffectDefinition effect = effects[nextEffectIndex];
                if (!handlers.TryGetValue(effect.EffectType, out IEventEffectHandler handler))
                    return Fail("Event handler must be rebound before dispatch: " + effect.EffectType);
                // Claim the step before calling effect code so callbacks cannot execute it twice.
                nextEffectIndex++;
                EventEffectOutcome outcome = handler.Resolve(context, effect);
                if (outcome == EventEffectOutcome.Applied) appliedEffectCount++;
                else if (outcome == EventEffectOutcome.NoEligibleTarget) noTargetEffectCount++;
                else return Fail("Unknown event effect outcome");
                if (context.Encounters.CurrentEventOccurrence?.Id != activeOccurrenceId
                    || context.Encounters.CurrentEventOccurrence.State != EventOccurrenceState.Resolving)
                    return Fail("Event ownership changed while applying a consequence");
                if (context.Creatures.ChoiceKind != CreatureEffectChoiceKind.None)
                    return progress = EventResolutionProgress.WaitingForCreatureReactions;
            }
            if (!context.Encounters.TryCompleteEventResolution(activeOccurrenceId))
                return Fail("Event completion was rejected");
            activeOccurrenceId = 0;
            effects = Array.Empty<CardEffectDefinition>();
            return progress = EventResolutionProgress.Completed;
        }
        catch (Exception exception)
        {
            // Applied mutations cannot be rolled back generically; keep the window closed and reject retries.
            return Fail(exception.GetType().Name + ": " + exception.Message);
        }
        finally { advancing = false; }
    }

    /// <summary>Clears unfinished dispatch state at run shutdown; EncounterRuntime reset owns aborting its occurrence.</summary>
    public void Reset()
    {
        if (advancing) throw new InvalidOperationException("Cannot reset event dispatch from inside an effect handler.");
        activeOccurrenceId = 0;
        boundContext = null;
        source = null;
        effects = Array.Empty<CardEffectDefinition>();
        nextEffectIndex = 0;
        appliedEffectCount = 0;
        noTargetEffectCount = 0;
        progress = EventResolutionProgress.Inactive;
        failureReason = string.Empty;
    }

    /// <summary>Accepts rebuilt contexts for the same owners and allows setup to rebind a restored serialized resolution.</summary>
    private bool UsesBoundRuntimeOwners(EventResolutionContext context)
    {
        return boundContext == null || (context.Encounters == boundContext.Encounters && context.Chain == boundContext.Chain
            && context.Creatures == boundContext.Creatures && context.Deck == boundContext.Deck
            && context.Resolver == boundContext.Resolver && context.Random == boundContext.Random
            && context.Persistence == boundContext.Persistence);
    }

    /// <summary>Stops dispatch after a fault without reopening input or retrying partially applied consequences.</summary>
    private EventResolutionProgress Fail(string reason)
    {
        failureReason = reason;
        return progress = EventResolutionProgress.Failed;
    }
}
