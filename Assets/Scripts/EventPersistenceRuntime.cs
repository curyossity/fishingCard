using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns independent event pending effects and recipient modifiers after their event cards are discarded.</summary>
[Serializable]
public sealed class EventPersistenceRuntime
{
    [Serializable]
    private sealed class BoundaryProgress
    {
        public string Id;
        public long LastOccurrenceId;
    }
    [SerializeField] private List<PersistentEventEffect> effects = new List<PersistentEventEffect>();
    [SerializeField] private List<BoundaryProgress> boundaries = new List<BoundaryProgress>();
    [SerializeField] private long nextEffectId = 1;
    [SerializeField] private long epoch = 1;
    private CatchChainRuntime chain;
    private EffectResolver resolver;
    /// <summary>Awards every pending Upwelling to the same next creature entering Bait before own attachment effects.</summary>
    public void ApplyPendingBaitWeightReductions(CardInstance recipient)
    {
        if (recipient?.Definition?.CardType != CardType.Creature || !recipient.IsBait) return;
        foreach (PersistentEventEffect record in Effects)
            if (record.IsPending && record.Effect.EffectType == CardEffectType.QueueBaitWeightReduction)
                TryApplyPendingModifier(record.Id, recipient.InstanceId, -record.Effect.Amount, 0);
    }

    /// <summary>Projects Upwelling on a detached preview without consuming pending grants.</summary>
    public void PreviewPendingBaitWeightReductions(CardInstance preview)
    {
        if (preview?.Definition?.CardType != CardType.Creature || !preview.IsBait) return;
        foreach (PersistentEventEffect record in Effects)
            if (record.IsPending && record.Effect.EffectType == CardEffectType.QueueBaitWeightReduction)
                preview.ModifyWeight(-record.Effect.Amount, record.Source, record.Effect);
    }

    public long Epoch => epoch;
    public PersistentEventEffect[] Effects { get { Refresh(); return effects.ToArray(); } }

    /// <summary>Binds mutation owners at runtime setup/reload, rejecting rebinding active records to a different live chain.</summary>
    public void Bind(CatchChainRuntime chain, EffectResolver resolver)
    {
        if (chain == null || resolver == null) throw new ArgumentNullException("Event persistence owners are required.");
        if (this.chain != null && this.chain != chain && effects.Count > 0)
            throw new InvalidOperationException("Reset event persistence before binding a different chain.");
        this.chain = chain; this.resolver = resolver;
    }

    /// <summary>Queues an explicitly timed occurrence that survives source-event discard without inventing target/trigger semantics.</summary>
    public long QueuePending(long sourceOccurrenceId, CardDefinition source, CardEffectDefinition effect, EventEffectLifetime lifetime)
    {
        RequireOwners();
        if (sourceOccurrenceId <= 0 || !EncounterRuntime.IsEventCard(source) || effect == null || lifetime == null
            || lifetime.HasBoundary && lifetime.BoundaryCount <= 0)
            throw new ArgumentException("An event origin, effect, and explicit lifetime are required.");
        PersistentEventEffect record = new PersistentEventEffect(nextEffectId, sourceOccurrenceId, source, effect, lifetime);
        nextEffectId = checked(nextEffectId + 1); effects.Add(record); return record.Id;
    }

    /// <summary>Applies an event modifier to an existing recipient; missing targets leave no pending or applied record.</summary>
    public bool TryAddModifier(long origin, CardDefinition source, CardEffectDefinition effect, int targetId,
        int weight, int value, EventEffectLifetime lifetime, out long modifierId)
    {
        RequireOwners(); modifierId = 0;
        if (FindTarget(targetId) == null || weight == 0 && value == 0) return false;
        modifierId = QueuePending(origin, source, effect, lifetime);
        return TryApplyPendingModifier(modifierId, targetId, weight, value);
    }

    /// <summary>Transfers a pending occurrence to a target without refreshing its remaining lifetime or source identity.</summary>
    public bool TryApplyPendingModifier(long effectId, int targetId, int weight, int value)
    {
        RequireOwners(); Refresh();
        PersistentEventEffect record = effects.Find(e => e.Id == effectId && e.IsPending);
        CardInstance target = FindTarget(targetId);
        if (record == null || target == null || weight == 0 && value == 0) return false;
        target.AddEventModifier(record.Id, weight, value, record.Effect, record.Source);
        record.AttachTo(targetId); chain.Recalculate(resolver); return true;
    }

    /// <summary>Claims one exact pending occurrence once for a card-specific handler; no target or trigger is assumed here.</summary>
    public bool TryConsumePending(long effectId, out PersistentEventEffect record)
    {
        RequireOwners(); Refresh(); record = effects.Find(e => e.Id == effectId && e.IsPending);
        return record != null && effects.Remove(record);
    }

    /// <summary>Expires matching lifetimes once per monotonic boundary token; epochs reject callbacks from an ended run.</summary>
    public bool NotifyBoundary(long expectedEpoch, string boundaryId, long occurrenceId)
    {
        RequireOwners();
        if (expectedEpoch != epoch || string.IsNullOrWhiteSpace(boundaryId) || occurrenceId <= 0) return false;
        BoundaryProgress clock = boundaries.Find(b => b.Id == boundaryId);
        if (clock != null && occurrenceId <= clock.LastOccurrenceId) return false;
        if (clock == null) { clock = new BoundaryProgress { Id = boundaryId }; boundaries.Add(clock); }
        clock.LastOccurrenceId = occurrenceId;
        Refresh(); bool changed = false;
        foreach (PersistentEventEffect record in effects.ToArray())
            if (record.ObserveBoundary(boundaryId)) changed |= Remove(record);
        if (changed) chain.Recalculate(resolver);
        return true;
    }

    /// <summary>Removes one still-active negative event occurrence, undoing only its exact recipient contribution.</summary>
    public bool TryRemoveNegativeEffect(long effectId)
    {
        RequireOwners(); Refresh(); PersistentEventEffect record = effects.Find(e => e.Id == effectId);
        if (record?.Effect?.EffectTone != CardEffectTone.Negative) return false;
        bool removed = Remove(record); if (removed) chain.Recalculate(resolver); return removed;
    }

    /// <summary>Identifies recipient provenance represented by this owner's active applied record, leaving orphaned modifiers cleanseable.</summary>
    public bool OwnsModifier(long effectId, int targetId)
    {
        return effects.Exists(e => e.Id == effectId && !e.IsPending && e.TargetInstanceId == targetId);
    }

    /// <summary>Builds Quick Clean options for pending/applied event occurrences without duplicating their recipient provenance.</summary>
    public NegativeEffectOption[] GetNegativeOptions()
    {
        RequireOwners(); Refresh(); List<NegativeEffectOption> options = new List<NegativeEffectOption>();
        foreach (PersistentEventEffect record in effects)
        {
            if (record.Effect.EffectTone != CardEffectTone.Negative) continue;
            long id = record.Id;
            string label = record.Source.DisplayName + (string.IsNullOrWhiteSpace(record.Effect.DisplayName) ? string.Empty : " — " + record.Effect.DisplayName);
            string description = (record.IsPending ? "Pending event effect: " : "Event modifier: ")
                + (string.IsNullOrWhiteSpace(record.Effect.ReminderText) ? "Negative effect" : record.Effect.ReminderText);
            options.Add(new NegativeEffectOption(label, description, record.Source, () => TryRemoveNegativeEffect(id)));
        }
        return options.ToArray();
    }

    /// <summary>Prunes recipients that left/were exchanged and records removed through an existing modifier API.</summary>
    public void Refresh()
    {
        if (chain == null) return;
        effects.RemoveAll(e => !e.IsPending && (FindTarget(e.TargetInstanceId) == null
            || !Array.Exists(FindTarget(e.TargetInstanceId).AppliedModifiers, m => m.EventEffectId == e.Id)));
    }

    /// <summary>Clears unfinished effects, reverses active recipient modifiers, and invalidates old run boundary callbacks.</summary>
    public void Reset()
    {
        RequireOwners(); epoch = checked(epoch + 1);
        foreach (PersistentEventEffect record in effects.ToArray()) Remove(record);
        boundaries.Clear(); chain.Recalculate(resolver);
    }

    /// <summary>Removes a tracked contribution by stable event ID, including positive expiry and serialized/rebuilt records.</summary>
    private bool Remove(PersistentEventEffect record)
    {
        if (!effects.Remove(record)) return false;
        if (!record.IsPending)
        {
            CardInstance target = FindTarget(record.TargetInstanceId);
            AppliedCatchModifier modifier = target == null ? null : Array.Find(target.AppliedModifiers, m => m.EventEffectId == record.Id);
            if (modifier != null) target.TryRemoveTrackedModifier(modifier);
        }
        return true;
    }

    /// <summary>Finds only current attached recipients by instance ID; historical snapshots are not mutation targets.</summary>
    private CardInstance FindTarget(int id) { return Array.Find(chain.Catches, c => c != null && c.InstanceId == id); }

    /// <summary>Requires bound owners before lifecycle mutations or cleanup.</summary>
    private void RequireOwners()
    {
        if (chain == null || resolver == null) throw new InvalidOperationException("Bind event persistence owners before use.");
    }
}
