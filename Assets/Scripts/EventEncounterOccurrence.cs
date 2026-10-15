using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Tracks event consequences independently of the visible encounter and catchable-card state.</summary>
public enum EventOccurrenceState
{
    Unresolved,
    Cancelled,
    Replaced,
    Resolving,
    Resolved
}

/// <summary>Read-only public view of one reveal; EncounterRuntime owns all lifecycle transitions.</summary>
[Serializable]
public sealed class EventEncounterOccurrence
{
    [SerializeField] private long id;
    [SerializeField] private CardDefinition definition;
    [SerializeField] private EventOccurrenceState state;
    [SerializeField] private int revealCallbackDepth = 1;
    [SerializeField] private List<long> pendingRevealReactionIds = new List<long>();
    [SerializeField] private bool reactionWindowClosed;

    public long Id => id;
    public CardDefinition Definition => definition;
    public EventOccurrenceState State => state;
    public bool RevealReactionsComplete => revealCallbackDepth == 0 && pendingRevealReactionIds.Count == 0;
    public int PendingRevealReactionCount => pendingRevealReactionIds.Count;
    internal bool CanDeferRevealReaction => revealCallbackDepth > 0;
    public bool IsReactionWindowOpen => RevealReactionsComplete && !reactionWindowClosed
        && (state == EventOccurrenceState.Unresolved || state == EventOccurrenceState.Cancelled);

    /// <summary>Creates independent runtime identity without mutating the authored event.</summary>
    internal EventEncounterOccurrence(long occurrenceId, CardDefinition eventDefinition)
    {
        id = occurrenceId;
        definition = eventDefinition;
        state = EventOccurrenceState.Unresolved;
    }

    /// <summary>Finishes one nested callback phase; deferred reactions can continue to hold the window closed.</summary>
    internal void CompleteRevealReactions()
    {
        if (revealCallbackDepth > 0) revealCallbackDepth--;
    }

    /// <summary>Closes input during cancellation reactions even when cancellation occurs after the initial reveal.</summary>
    internal void BeginRevealReactionPhase()
    {
        revealCallbackDepth++;
    }

    /// <summary>Tracks one deferred reaction independently so duplicate completions cannot release another reaction.</summary>
    internal void DeferRevealReaction(long reactionId)
    {
        pendingRevealReactionIds.Add(reactionId);
    }

    /// <summary>Completes the exact deferred reaction and reports stale or duplicate tokens.</summary>
    internal bool TryCompleteRevealReaction(long reactionId)
    {
        return pendingRevealReactionIds.Remove(reactionId);
    }

    /// <summary>Records complete Hazard cancellation while retaining the pre-action window.</summary>
    internal void Cancel()
    {
        state = EventOccurrenceState.Cancelled;
    }

    /// <summary>Invalidates unresolved consequences and closes the superseded occurrence's window.</summary>
    internal void Supersede()
    {
        if (state == EventOccurrenceState.Unresolved) state = EventOccurrenceState.Replaced;
        reactionWindowClosed = true;
    }

    /// <summary>Claims consequences exactly once and closes Technique eligibility for this occurrence.</summary>
    internal void BeginResolution()
    {
        state = EventOccurrenceState.Resolving;
        reactionWindowClosed = true;
    }

    /// <summary>Records completed consequences after their resulting reactions have finished.</summary>
    internal void CompleteResolution()
    {
        state = EventOccurrenceState.Resolved;
    }

    /// <summary>Ends a cancelled event's window without changing its cancellation outcome.</summary>
    internal void CloseCancelledWindow()
    {
        reactionWindowClosed = true;
    }
}
