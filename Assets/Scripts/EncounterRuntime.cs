using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed partial class EncounterRuntime
{
    [SerializeField] private CardDefinition currentEncounter;
    [SerializeField] private EncounterState currentState;
    [SerializeField] private CardDefinition hookedEncounter;
    [SerializeField] private HookedEffectRecord[] hookedEffectRecords = Array.Empty<HookedEffectRecord>();
    [SerializeField] private int lastSelectedEncounterWeight;
    [SerializeField] private int lastTotalEncounterWeight;
    [SerializeField] private string lastCandidateWeightSummary;
    [SerializeField] private EncounterChainDefinition activeEncounterChain;
    [SerializeField] private int nextEncounterChainIndex;
    [SerializeField] private EncounterVarietyRuntime encounterVarietyRuntime = new EncounterVarietyRuntime();

    [SerializeField] private EventEncounterOccurrence currentEventOccurrence;
    [SerializeField] private long nextEventOccurrenceId = 1;
    [SerializeField] private long nextEventRevealReactionId = 1;
    public EventEncounterOccurrence CurrentEventOccurrence => currentEventOccurrence;
    private int activeRevealCallbackDepth;
    public bool IsRevealReactionInProgress => activeRevealCallbackDepth > 0
        || currentEventOccurrence != null && !currentEventOccurrence.RevealReactionsComplete
            && (currentEventOccurrence.State == EventOccurrenceState.Unresolved || currentEventOccurrence.State == EventOccurrenceState.Cancelled);
    public bool HasTechniqueReactionWindow => activeRevealCallbackDepth == 0
        && (hookedEncounter != null || HasEventReactionWindow);
    public bool HasEventReactionWindow => currentEventOccurrence?.IsReactionWindowOpen == true;

    /// <summary>Recognizes non-catchable events without including progression Locations or player cards.</summary>
    public static bool IsEventCard(CardDefinition card)
    {
        return card != null && (card.CardType == CardType.Hazard || card.CardType == CardType.Environment
            || card.CardType == CardType.Opportunity || card.CardType == CardType.Encounter);
    }

    /// <summary>Claims only the current unresolved event after reveal reactions; stale and cancelled IDs cannot execute consequences.</summary>
    public bool TryBeginEventResolution(long occurrenceId, out CardDefinition eventCard)
    {
        eventCard = null;
        if (currentEventOccurrence?.Id != occurrenceId
            || currentEventOccurrence.State != EventOccurrenceState.Unresolved
            || !currentEventOccurrence.IsReactionWindowOpen) return false;
        currentEventOccurrence.BeginResolution();
        eventCard = currentEventOccurrence.Definition;
        return true;
    }

    /// <summary>Completes only the claimed occurrence and discards its visible card without destroying its lifecycle record.</summary>
    public bool TryCompleteEventResolution(long occurrenceId)
    {
        if (currentEventOccurrence?.Id != occurrenceId
            || currentEventOccurrence.State != EventOccurrenceState.Resolving) return false;
        currentEventOccurrence.CompleteResolution();
        if (currentEncounter == currentEventOccurrence.Definition) SetCurrentEncounter(null);
        return true;
    }

    /// <summary>Closes a cancelled occurrence at action confirmation without making its effects executable.</summary>
    public bool TryCloseCancelledEventWindow(long occurrenceId)
    {
        if (currentEventOccurrence?.Id != occurrenceId
            || currentEventOccurrence.State != EventOccurrenceState.Cancelled
            || !currentEventOccurrence.IsReactionWindowOpen) return false;
        currentEventOccurrence.CloseCancelledWindow();
        return true;
    }

    /// <summary>Defers a reveal reaction during its callback phase, preserving a closed input window until its exact completion.</summary>
    public bool TryDeferEventRevealReaction(long occurrenceId, out long reactionId)
    {
        reactionId = 0;
        if (currentEventOccurrence?.Id != occurrenceId || !currentEventOccurrence.CanDeferRevealReaction
            || (currentEventOccurrence.State != EventOccurrenceState.Unresolved
                && currentEventOccurrence.State != EventOccurrenceState.Cancelled)) return false;
        reactionId = nextEventRevealReactionId;
        nextEventRevealReactionId = checked(nextEventRevealReactionId + 1);
        currentEventOccurrence.DeferRevealReaction(reactionId);
        return true;
    }

    /// <summary>Completes one deferred reaction only for the still-current unresolved or cancelled event.</summary>
    public bool TryCompleteEventRevealReaction(long occurrenceId, long reactionId)
    {
        if (currentEventOccurrence?.Id != occurrenceId
            || (currentEventOccurrence.State != EventOccurrenceState.Unresolved
                && currentEventOccurrence.State != EventOccurrenceState.Cancelled)) return false;
        return currentEventOccurrence.TryCompleteRevealReaction(reactionId);
    }

    /// <summary>Cancels only a current revealed unresolved Hazard, discarding it before notifying rewards exactly once.</summary>
    public bool TryCancelRevealedHazard(long occurrenceId)
    {
        EventEncounterOccurrence occurrence = currentEventOccurrence;
        if (occurrence?.Id != occurrenceId || occurrence.State != EventOccurrenceState.Unresolved
            || occurrence.Definition.CardType != CardType.Hazard) return false;
        occurrence.BeginRevealReactionPhase();
        occurrence.Cancel();
        SetCurrentEncounter(null);
        lastCancelledHazard = occurrence.Definition;
        HazardCancelled?.Invoke(occurrence.Definition);
        if (currentEventOccurrence == occurrence) occurrence.CompleteRevealReactions();
        return true;
    }

    /// <summary>Replaces a legal current encounter only when an alternative exists; failed replacement preserves its state and chain.</summary>
    public bool TryReplaceEncounter(int expectedRevision, CardDefinition expectedEncounter, CardDefinition[] pool, string biomeId, int depth,
        System.Random random, ActiveCatchEffectRecord[] effects, EffectResolver resolver,
        CardEffectDefinition[] temporaryEffects = null, EncounterChainDefinition[] chains = null)
    {
        if (expectedRevision != encounterRevision || expectedEncounter == null || currentEncounter != expectedEncounter
            || (IsEventCard(expectedEncounter) && (currentEventOccurrence?.State != EventOccurrenceState.Unresolved
                || !HasEventReactionWindow))
            || (!IsEventCard(expectedEncounter) && hookedEncounter != expectedEncounter)
            || !HasAlternativeEncounter(pool, biomeId, depth, expectedEncounter)) return false;
        Trail.MarkReplaced(expectedEncounter);
        CancelActiveEncounterChain();
        return Reveal(pool, biomeId, depth, random, effects, resolver, temporaryEffects, expectedEncounter, chains);
    }

    [SerializeField] private EventEncounterTrailRuntime trail = new EventEncounterTrailRuntime();
    public EventEncounterTrailRuntime Trail => trail ?? (trail = new EventEncounterTrailRuntime());
    public Func<CardDefinition, int, bool> EventAvailability { get; set; }

    [SerializeField] private CardDefinition lastCancelledHazard;
    public CardDefinition LastCancelledHazard => lastCancelledHazard;
    public Func<CardDefinition, bool> HazardCancellation { get; set; }

    [Serializable]
    public sealed class PendingConcealment
    {
        public int Id;
        public CardEffectDefinition Effect;
        public CardDefinition Source;
        public int Remaining;
    }

    [SerializeField] private List<PendingConcealment> pendingConcealments = new List<PendingConcealment>();
    [SerializeField] private int assignedConcealmentId = -1;
    [SerializeField] private int nextConcealmentId = 1;
    [SerializeField] private int encounterRevision;
    public int EncounterRevision => encounterRevision;
    public bool IsCreatureConcealed => assignedConcealmentId >= 0;
    public PendingConcealment[] PendingConcealments => pendingConcealments.ToArray();
    public bool HasFutureConcealment => pendingConcealments.Exists(c => c.Remaining > 0);

    /// <summary>Ends only the assigned concealment, or consumes one oldest future charge when none is assigned.</summary>
    public bool TryClearOneCreatureConcealment()
    {
        if (assignedConcealmentId >= 0) { CompleteConcealedEncounter(); return true; }
        PendingConcealment next = pendingConcealments.Find(c => c.Remaining > 0);
        if (next == null) return false;
        next.Remaining--;
        pendingConcealments.RemoveAll(c => c.Remaining == 0 && c.Id != assignedConcealmentId);
        return true;
    }
    public event Action<CardDefinition> HazardCancelled;
    public event Action<CardDefinition, int> CreatureRevealed;
    public event Action<CardDefinition> EncounterRevealEffects;

    /// <summary>Queues independent negative concealment charges without tying them to source lifetime.</summary>
    public void GrantCreatureConcealment(CardEffectDefinition effect, CardDefinition source)
    {
        if (effect == null || effect.Amount <= 0) return;
        pendingConcealments.Add(new PendingConcealment { Id = nextConcealmentId++, Effect = effect, Source = source, Remaining = effect.Amount });
    }

    /// <summary>Removes an exact pending/assigned concealment occurrence, preserving all other sources' charges.</summary>
    public bool TryRemoveCreatureConcealment(PendingConcealment concealment)
    {
        if (concealment?.Effect?.EffectTone != CardEffectTone.Negative || !pendingConcealments.Remove(concealment)) return false;
        if (assignedConcealmentId == concealment.Id) assignedConcealmentId = -1;
        return true;
    }

    /// <summary>Consumes only the concealment reserved for the current committed or removed encounter.</summary>
    private void CompleteConcealedEncounter()
    {
        int completed = assignedConcealmentId;
        assignedConcealmentId = -1;
        pendingConcealments.RemoveAll(c => c.Id == completed && c.Remaining == 0);
    }

    public CardDefinition CurrentEncounter => currentEncounter;
    public EncounterState CurrentState => currentState;
    public CardDefinition HookedEncounter => hookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => hookedEffectRecords;
    public int LastSelectedEncounterWeight => lastSelectedEncounterWeight;
    public int LastTotalEncounterWeight => lastTotalEncounterWeight;
    public string LastCandidateWeightSummary => lastCandidateWeightSummary;
    public EncounterChainDefinition ActiveEncounterChain => activeEncounterChain;
    public int NextEncounterChainIndex => nextEncounterChainIndex;
    public CardDefinition[] RecentEncounterHistory => encounterVarietyRuntime == null
        ? Array.Empty<CardDefinition>()
        : encounterVarietyRuntime.RecentEncounterHistory;
    public string RecentEncounterSequenceSummary => encounterVarietyRuntime == null
        ? string.Empty
        : encounterVarietyRuntime.RecentEncounterSequenceSummary;
    public int ConsecutiveCreatureEncounters => encounterVarietyRuntime == null
        ? 0
        : encounterVarietyRuntime.ConsecutiveCreatureEncounters;
    public string LastRepetitionRuleSummary => encounterVarietyRuntime == null
        ? string.Empty
        : encounterVarietyRuntime.LastRepetitionRuleSummary;

    /// <summary>
    /// Selects and activates a valid encounter for the current biome and depth.
    /// </summary>
    public bool Reveal(
        CardDefinition[] encounterPool,
        string biomeId,
        int depth,
        System.Random random,
        ActiveCatchEffectRecord[] activeCatchEffects,
        EffectResolver effectResolver,
        CardEffectDefinition[] temporaryEncounterEffects = null,
        CardDefinition excludedEncounter = null,
        EncounterChainDefinition[] encounterChains = null)
    {
        EnsureEncounterCanBeRevealed();
        List<CardDefinition> candidates = new List<CardDefinition>();
        List<int> candidateWeights = new List<int>();
        List<string> candidateWeightLabels = new List<string>();
        EnsureEncounterVarietyRuntime();
        lastSelectedEncounterWeight = 0;
        lastTotalEncounterWeight = 0;
        lastCandidateWeightSummary = string.Empty;
        encounterVarietyRuntime.BeginSelection();

        if (TryRevealActiveChain(biomeId, depth))
        {
            return true;
        }

        if (encounterPool != null)
        {
            for (int i = 0; i < encounterPool.Length; i++)
            {
                CardDefinition card = encounterPool[i];

                if (card == null || card == excludedEncounter || !IsEncounterCard(card))
                {
                    continue;
                }

                if (card.IsAvailableInBiome(biomeId) && card.IsAvailableAtDepth(depth)
                    && (EventAvailability == null || EventAvailability(card, depth)))
                {
                    candidates.Add(card);
                }
            }
        }

        // Category pacing remains unchanged; timed species draws use the full eligible pool equally.
        List<CardDefinition> timingCandidates = TimingProfile != null || TagTimingProfile != null ? new List<CardDefinition>(candidates) : null;
        encounterVarietyRuntime.ApplyRepetitionRules(candidates);

        if (candidates.Count == 0)
        {
            SetCurrentEncounter(null);
            return false;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            CardDefinition candidate = candidates[i];
            int selectionWeight = effectResolver.GetEncounterSelectionWeight(
                candidate,
                activeCatchEffects,
                temporaryEncounterEffects);
            candidateWeights.Add(selectionWeight);
            candidateWeightLabels.Add($"{candidate.DisplayName}: {selectionWeight}");
            lastTotalEncounterWeight += selectionWeight;
        }

        int selectedIndex = SelectWeightedIndex(candidateWeights, lastTotalEncounterWeight, random);
        lastCandidateWeightSummary = string.Join(", ", candidateWeightLabels);
        lastSelectedEncounterWeight = candidateWeights[selectedIndex];
        CardDefinition selected = candidates[selectedIndex];
        if (TryDeferCreature(selected, timingCandidates ?? candidates, encounterChains)) return true;
        encounterVarietyRuntime.RecordSelectedEncounter(selected);
        BeginEncounterChain(selected, encounterChains);
        SetCurrentEncounter(selected);
        return true;
    }

    /// <summary>Samples distinct eligible ordinary creatures without advancing encounter chains or repetition history.</summary>
    public CardDefinition[] OfferCreatures(
        CardDefinition[] pool,
        string biomeId,
        int depth,
        int count,
        System.Random random,
        ActiveCatchEffectRecord[] attachedEffects,
        EffectResolver resolver,
        CardEffectDefinition[] temporaryEffects = null,
        string[] requiredTags = null)
    {
        List<CardDefinition> candidates = new List<CardDefinition>();
        List<int> weights = new List<int>();
        HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CardDefinition card in pool ?? Array.Empty<CardDefinition>())
        {
            if (card == null || card.CardType != CardType.Creature
                || !EffectResolver.RequiredTagsMatch(requiredTags, card)
                || !card.IsAvailableInBiome(biomeId) || !card.IsAvailableAtDepth(depth)
                || candidates.Contains(card)
                || (!string.IsNullOrEmpty(card.UniqueId) && !ids.Add(card.UniqueId)))
            {
                continue;
            }

            candidates.Add(card);
            weights.Add(resolver.GetEncounterSelectionWeight(card, attachedEffects, temporaryEffects));
        }

        List<CardDefinition> offers = new List<CardDefinition>();
        while (offers.Count < count && candidates.Count > 0)
        {
            int total = 0;
            foreach (int weight in weights)
            {
                total += weight;
            }

            int selected = SelectWeightedIndex(weights, total, random);
            offers.Add(candidates[selected]);
            candidates.RemoveAt(selected);
            weights.RemoveAt(selected);
        }

        return offers.ToArray();
    }

    /// <summary>Reveals a queued chosen creature at the normal time through the same Hooked and on-reveal state as a draw.</summary>
    public bool RevealSelectedCreature(CardDefinition chosen, string biomeId, int depth,
        ActiveCatchEffectRecord[] effects, EffectResolver resolver,
        CardEffectDefinition[] temporaryEffects = null, EncounterChainDefinition[] chains = null)
    {
        if (chosen?.CardType != CardType.Creature || !chosen.IsAvailableInBiome(biomeId) || !chosen.IsAvailableAtDepth(depth)) return false;
        EnsureEncounterCanBeRevealed();
        EnsureEncounterVarietyRuntime();
        CancelActiveEncounterChain();
        encounterVarietyRuntime.BeginSelection();
        lastSelectedEncounterWeight = resolver.GetEncounterSelectionWeight(chosen, effects, temporaryEffects);
        lastTotalEncounterWeight = lastSelectedEncounterWeight;
        lastCandidateWeightSummary = "Chosen next encounter: " + chosen.DisplayName;
        if (TryDeferCreature(chosen, null, chains)) return true;
        encounterVarietyRuntime.RecordSelectedEncounter(chosen);
        BeginEncounterChain(chosen, chains);
        SetCurrentEncounter(chosen);
        return true;
    }

    /// <summary>
    /// Activates a preselected biome Apex through the normal Hooked encounter state.
    /// </summary>
    public bool RevealApex(CardDefinition apexEncounter)
    {
        if (apexEncounter == null || apexEncounter.CardType != CardType.ApexEncounter)
        {
            return false;
        }

        EnsureEncounterVarietyRuntime();
        CancelActiveEncounterChain();
        lastSelectedEncounterWeight = 1;
        lastTotalEncounterWeight = 1;
        lastCandidateWeightSummary = $"Biome Apex: {apexEncounter.DisplayName}";
        encounterVarietyRuntime.BeginSelection();
        if (TryDeferCreature(apexEncounter, null, null)) return true;
        encounterVarietyRuntime.RecordSelectedEncounter(apexEncounter);
        SetCurrentEncounter(apexEncounter);
        return true;
    }

    /// <summary>
    /// Cancels a queued follow-up when the encounter that started it is avoided or replaced.
    /// </summary>
    public void CancelActiveEncounterChain()
    {
        activeEncounterChain = null;
        nextEncounterChainIndex = 0;
    }

    /// <summary>
    /// Reports whether the pool contains another valid encounter for replacement at a depth.
    /// </summary>
    public bool HasAlternativeEncounter(
        CardDefinition[] encounterPool,
        string biomeId,
        int depth,
        CardDefinition excludedEncounter)
    {
        if (encounterPool == null)
        {
            return false;
        }

        for (int i = 0; i < encounterPool.Length; i++)
        {
            CardDefinition card = encounterPool[i];

            if (card != null
                && card != excludedEncounter
                && IsEncounterCard(card)
                && card.IsAvailableInBiome(biomeId)
                && card.IsAvailableAtDepth(depth)
                && (EventAvailability == null || EventAvailability(card, depth)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes and returns the Hooked encounter so it can enter the Catch Chain.
    /// </summary>
    public CardDefinition TakeHookedEncounter()
    {
        if (hookedEncounter == null)
        {
            return null;
        }

        CompleteConcealedEncounter();
        CardDefinition caughtCard = hookedEncounter;
        hookedEncounter = null;
        hookedEffectRecords = Array.Empty<HookedEffectRecord>();
        currentState = EncounterState.Caught;
        return caughtCard;
    }

    /// <summary>
    /// Adds Technique effects that can influence the current Hooked encounter.
    /// </summary>
    public int AddTechniqueEffects(CardDefinition techniqueCard)
    {
        List<HookedEffectRecord> records = new List<HookedEffectRecord>(hookedEffectRecords);
        int originalCount = records.Count;

        AddRelevantEffectRecords(records, techniqueCard, HookedEffectSource.TechniqueCard);
        hookedEffectRecords = records.ToArray();
        return records.Count - originalCount;
    }

    /// <summary>
    /// Counts effects on a Technique card that can currently target the Hooked encounter.
    /// </summary>
    public int CountApplicableTechniqueEffects(CardDefinition techniqueCard)
    {
        if (techniqueCard == null || techniqueCard.Effects == null || hookedEncounter == null)
        {
            return 0;
        }

        int applicableEffects = 0;

        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
            if (DoesEffectAffectHookedEncounter(techniqueCard.Effects[i], hookedEncounter))
            {
                applicableEffects++;
            }
        }

        return applicableEffects;
    }

    /// <summary>Removes one active negative reaction-window record without modifying its definition.</summary>
    public bool TryRemoveNegativeEffect(int index)
    {
        if (index < 0 || index >= hookedEffectRecords.Length
            || hookedEffectRecords[index]?.Effect?.EffectTone != CardEffectTone.Negative)
        {
            return false;
        }

        List<HookedEffectRecord> remaining = new List<HookedEffectRecord>(hookedEffectRecords);
        remaining.RemoveAt(index);
        hookedEffectRecords = remaining.ToArray();
        return true;
    }

    /// <summary>
    /// Clears the current encounter and its reaction-window state.
    /// </summary>
    public void Reset(bool preservePendingConcealments = false)
    {
        RarityTiming.Reset(); timingChains = null; timingChainFollowUp = false;
        if (preservePendingConcealments) TagTiming.Reset(); else TagTiming.ResetRun();
        Trail.Reset();
        currentEventOccurrence?.Supersede();
        currentEventOccurrence = null;
        // Keep IDs monotonic across resets so callbacks from a previous run cannot claim a new event.
        CompleteConcealedEncounter();
        if (!preservePendingConcealments)
        {
            pendingConcealments.Clear();
            nextConcealmentId = 1;
        }
        encounterRevision++;
        lastCancelledHazard = null;
        currentEncounter = null;
        currentState = EncounterState.None;
        hookedEncounter = null;
        hookedEffectRecords = Array.Empty<HookedEffectRecord>();
        lastSelectedEncounterWeight = 0;
        lastTotalEncounterWeight = 0;
        lastCandidateWeightSummary = string.Empty;
        EnsureEncounterVarietyRuntime();
        encounterVarietyRuntime.Reset();
        CancelActiveEncounterChain();
    }

    /// <summary>
    /// Restores the variety runtime when loading scene data created before it existed.
    /// </summary>
    private void EnsureEncounterVarietyRuntime()
    {
        if (encounterVarietyRuntime == null)
        {
            encounterVarietyRuntime = new EncounterVarietyRuntime();
        }
    }

    /// <summary>
    /// Selects one candidate from integer weights, falling back to uniform selection when all weights are zero.
    /// </summary>
    private static int SelectWeightedIndex(List<int> weights, int totalWeight, System.Random random)
    {
        if (totalWeight <= 0)
        {
            return random.Next(weights.Count);
        }

        int roll = random.Next(totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];

            if (roll < cumulativeWeight)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }

    /// <summary>
    /// Reveals the next valid card in an active short encounter sequence.
    /// </summary>
    private bool TryRevealActiveChain(string biomeId, int depth)
    {
        if (activeEncounterChain == null
            || !activeEncounterChain.TryGetCard(nextEncounterChainIndex, out CardDefinition chainedEncounter))
        {
            CancelActiveEncounterChain();
            return false;
        }

        if (!IsEncounterCard(chainedEncounter)
            || !chainedEncounter.IsAvailableInBiome(biomeId)
            || !chainedEncounter.IsAvailableAtDepth(depth))
        {
            CancelActiveEncounterChain();
            return false;
        }

        nextEncounterChainIndex++;

        if (!activeEncounterChain.TryGetCard(nextEncounterChainIndex, out _))
        {
            EncounterChainDefinition completedChain = activeEncounterChain;
            CancelActiveEncounterChain();
            lastCandidateWeightSummary = $"Chain: {completedChain.DisplayName}";
        }
        else
        {
            lastCandidateWeightSummary = $"Chain: {activeEncounterChain.DisplayName}";
        }

        lastSelectedEncounterWeight = 1;
        lastTotalEncounterWeight = 1;
        if (TryDeferCreature(chainedEncounter, null, null, true)) return true;
        encounterVarietyRuntime.RecordAuthoredChainFollowUp();
        encounterVarietyRuntime.RecordSelectedEncounter(chainedEncounter);
        SetCurrentEncounter(chainedEncounter);
        return true;
    }

    /// <summary>
    /// Queues the second card when a randomly selected encounter starts an authored chain.
    /// </summary>
    private void BeginEncounterChain(CardDefinition selectedEncounter, EncounterChainDefinition[] encounterChains)
    {
        if (encounterChains == null)
        {
            return;
        }

        for (int i = 0; i < encounterChains.Length; i++)
        {
            EncounterChainDefinition chain = encounterChains[i];

            if (chain != null && chain.StartsWith(selectedEncounter))
            {
                activeEncounterChain = chain;
                nextEncounterChainIndex = 1;
                return;
            }
        }
    }

    /// <summary>Rejects new reveals before selection, history, or chain state can change during active consequences.</summary>
    private void EnsureEncounterCanBeRevealed()
    {
        if (Timing.Active) throw new InvalidOperationException("Stop the timing marker before revealing another encounter.");
        if (currentEventOccurrence?.State == EventOccurrenceState.Resolving)
            throw new InvalidOperationException("Cannot reveal a new encounter while event consequences are resolving.");
    }

    /// <summary>
    /// Cancels protected Hazards before activating encounter state or building any effect records.
    /// </summary>
    public void SetCurrentEncounter(CardDefinition encounter)
    {
        activeRevealCallbackDepth++;
        try { ApplyRevealedEncounter(encounter); }
        finally { activeRevealCallbackDepth--; }
    }

    /// <summary>Applies one reveal while all nested reveal callbacks keep Technique input closed.</summary>
    private void ApplyRevealedEncounter(CardDefinition encounter)
    {
        if (encounter != null) EnsureEncounterCanBeRevealed();
        EventEncounterOccurrence revealedEvent = null;
        if (encounter != null)
        {
            currentEventOccurrence?.Supersede();
            currentEventOccurrence = null;
            if (IsEventCard(encounter))
            {
                revealedEvent = new EventEncounterOccurrence(nextEventOccurrenceId, encounter);
                nextEventOccurrenceId = checked(nextEventOccurrenceId + 1);
                currentEventOccurrence = revealedEvent;
            }
        }
        // Clearing presentation alone must not discard an unresolved/cancelled event's action window.
        if (encounter == null && currentState == EncounterState.Hooked) Trail.MarkReplaced(currentEncounter);
        CompleteConcealedEncounter();
        encounterRevision++;
        lastCancelledHazard = null;
        currentEncounter = encounter;
        if (encounter?.CardType == CardType.Hazard && HazardCancellation?.Invoke(encounter) == true)
        {
            TryCancelRevealedHazard(revealedEvent.Id);
            if (revealedEvent == currentEventOccurrence) revealedEvent.CompleteRevealReactions();
            return;
        }
        if (encounter?.CardType == CardType.Creature)
        {
            PendingConcealment concealment = pendingConcealments.Find(c => c.Remaining > 0);
            if (concealment != null)
            {
                concealment.Remaining--;
                assignedConcealmentId = concealment.Id;
            }
        }

        if (currentEncounter == null)
        {
            hookedEncounter = null;
            currentState = EncounterState.None;
            RebuildHookedEffectRecords();
            if (revealedEvent == currentEventOccurrence) revealedEvent?.CompleteRevealReactions();
            return;
        }

        if (IsCatchableEncounter(currentEncounter))
        {
            hookedEncounter = currentEncounter;
            currentState = EncounterState.Hooked;
            RebuildHookedEffectRecords();
            if (encounter.CardType == CardType.Creature) CreatureRevealed?.Invoke(encounter, encounterRevision);
            EncounterRevealEffects?.Invoke(encounter);
            return;
        }

        hookedEncounter = null;
        currentState = EncounterState.Encountered;
        RebuildHookedEffectRecords();
        EncounterRevealEffects?.Invoke(encounter);
        if (revealedEvent == currentEventOccurrence) revealedEvent?.CompleteRevealReactions();
    }

    /// <summary>
    /// Rebuilds effects that can influence the current Hooked reaction window.
    /// </summary>
    private void RebuildHookedEffectRecords()
    {
        if (hookedEncounter == null)
        {
            hookedEffectRecords = Array.Empty<HookedEffectRecord>();
            return;
        }

        List<HookedEffectRecord> records = new List<HookedEffectRecord>();
        AddRelevantEffectRecords(records, hookedEncounter, HookedEffectSource.HookedEncounter);
        hookedEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Adds effects from a source card when they match the current Hooked encounter.
    /// </summary>
    private void AddRelevantEffectRecords(
        List<HookedEffectRecord> records,
        CardDefinition sourceCard,
        HookedEffectSource sourceType)
    {
        if (sourceCard == null || sourceCard.Effects == null)
        {
            return;
        }

        for (int i = 0; i < sourceCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = sourceCard.Effects[i];

            if (DoesEffectAffectHookedEncounter(effect, hookedEncounter))
            {
                records.Add(new HookedEffectRecord(sourceCard, effect, sourceType));
            }
        }
    }

    /// <summary>
    /// Checks whether an effect targets and matches the current Hooked encounter.
    /// </summary>
    private static bool DoesEffectAffectHookedEncounter(CardEffectDefinition effect, CardDefinition targetEncounter)
    {
        if (effect == null || targetEncounter == null)
        {
            return false;
        }

        bool targetsHookedCard = effect.Target == CardEffectTarget.HookedEncounter
            || effect.Target == CardEffectTarget.CurrentEncounter
            || effect.Target == CardEffectTarget.NextDescend;

        return targetsHookedCard && RequiredTagsMatch(effect.RequiredTags, targetEncounter);
    }

    /// <summary>
    /// Checks whether every required effect tag matches the target card.
    /// </summary>
    private static bool RequiredTagsMatch(string[] requiredTags, CardDefinition targetCard)
    {
        if (requiredTags == null || requiredTags.Length == 0)
        {
            return true;
        }

        if (targetCard == null)
        {
            return false;
        }

        for (int i = 0; i < requiredTags.Length; i++)
        {
            if (string.Equals(requiredTags[i], targetCard.CardType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!HasTag(targetCard.Tags, requiredTags[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a tag collection contains a requested tag, ignoring case.
    /// </summary>
    private static bool HasTag(string[] tags, string requiredTag)
    {
        if (tags == null || tags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (string.Equals(requiredTag, tags[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether a card belongs to the encounter-facing side of the game.
    /// </summary>
    private static bool IsEncounterCard(CardDefinition card)
    {
        // Apex uses encounter behavior, while later content systems keep it out of ordinary pools.
        return card.CardType == CardType.Creature
            || card.CardType == CardType.Treasure
            || card.CardType == CardType.Hazard
            || card.CardType == CardType.Environment
            || card.CardType == CardType.Opportunity
            || card.CardType == CardType.ApexEncounter
            || card.CardType == CardType.Encounter;
    }

    /// <summary>
    /// Checks whether a revealed encounter should enter the Hooked reaction state.
    /// </summary>
    private static bool IsCatchableEncounter(CardDefinition card)
    {
        return card != null
            && (card.CardType == CardType.Creature
                || card.CardType == CardType.Treasure
                || card.CardType == CardType.ApexEncounter);
    }

}
