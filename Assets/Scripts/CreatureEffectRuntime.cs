using System;
using System.Collections.Generic;
using UnityEngine;

public enum CreatureEffectChoiceKind
{
    None,
    RemoveNegativeEffect,
    SwapCatch,
    ReleaseAttachedCard,
    RecoverTechnique,
    SelectWeightTargets,
    RedrawTechnique,
    OptionalPredatorRelease,
    ChooseNextEncounter,
    TransferValue
}

/// <summary>Owns ordered creature choices, deferred Release reactions, and a queued encounter without owning presentation.</summary>
[Serializable]
public sealed class CreatureEffectRuntime
{
    [Serializable]
    private sealed class Request
    {
        public int SourceInstanceId;
        public int EffectIndex;
        public string AbilityName;
        public CreatureEffectChoiceKind Kind;
        public int[] EligibleCatchIds = Array.Empty<int>();
        public CardDefinition[] Offers = Array.Empty<CardDefinition>();
        public bool OffersGenerated;
        public int AttachedInstanceId;
        public int[] EligibleSourceIds = Array.Empty<int>();
        public List<int> SelectedTargetIds = new List<int>();
        public int CapturedValue;
        public CardEffectDefinition TransferEffect;
        public CardDefinition TransferSource;
    }

    [SerializeField] private List<Request> requests = new List<Request>();
    [SerializeField] private int biomeVisit;
    [SerializeField] private bool encounterRecoveryConsumed;
    [SerializeField] private Request pendingRecovery;
    [SerializeField] private List<Request> pendingTransfers = new List<Request>();
    [SerializeField] private CardDefinition queuedEncounter;

    public CardDefinition QueuedEncounter => queuedEncounter;
    public int TransferValue => ChoiceKind == CreatureEffectChoiceKind.TransferValue ? requests[0].CapturedValue : 0;
    public int BiomeVisit => biomeVisit;
    public CreatureEffectChoiceKind ChoiceKind => requests.Count == 0
        ? CreatureEffectChoiceKind.None : requests[0].Kind;
    public string ChoiceName => requests.Count == 0 ? string.Empty : requests[0].AbilityName;

    /// <summary>Resolves the queued source by instance ID instead of duplicating runtime card ownership.</summary>
    public CardInstance GetChoiceSource(CardInstance[] catches)
    {
        return requests.Count == 0 ? null
            : Array.Find(catches ?? Array.Empty<CardInstance>(), c => c?.InstanceId == requests[0].SourceInstanceId);
    }
    public CardDefinition[] OfferedCreatures => requests.Count == 0
        ? Array.Empty<CardDefinition>() : requests[0].Offers;

    /// <summary>Clears choices and resets the run's biome-visit ordinal.</summary>
    public void Reset()
    {
        requests.Clear();
        biomeVisit = 0;
        encounterRecoveryConsumed = false;
        pendingRecovery = null;
        pendingTransfers.Clear();
        queuedEncounter = null;
    }

    /// <summary>Stamps a new attachment and queues its selected role's immediate choice effects.</summary>
    public void RecordAttachment(CardInstance instance, bool beforePendingChoices = false, CardInstance[] catches = null)
    {
        if (instance == null)
        {
            return;
        }

        instance.SetAttachmentBiomeVisit(biomeVisit);
        List<Request> immediate = new List<Request>();
        CardEffectDefinition[] effects = instance.ActiveEffects;
        for (int effectIndex = 0; effectIndex < effects.Length; effectIndex++)
        {
            CardEffectDefinition effect = effects[effectIndex];
            if (effect?.Trigger == CardEffectTrigger.WhenCaught
                && effect.EffectType == CardEffectType.ChooseNextSchoolingEncounter && instance.IsBait
                && instance.TryConsumeAttachmentActivation(effectIndex))
            {
                immediate.Add(new Request
                {
                    SourceInstanceId = instance.InstanceId, EffectIndex = effectIndex,
                    AbilityName = effect.DisplayName, Kind = CreatureEffectChoiceKind.ChooseNextEncounter
                });
            }
            if (effect?.Trigger == CardEffectTrigger.WhenCaught
                && (effect.EffectType == CardEffectType.RemoveNegativeEffect
                    || effect.EffectType == CardEffectType.ReleaseCatch
                    || effect.EffectType == CardEffectType.SelectAttachedWeightTargets))
            {
                immediate.Add(new Request
                {
                    SourceInstanceId = instance.InstanceId,
                    EffectIndex = effectIndex,
                    AbilityName = effect.DisplayName,
                    Kind = effect.EffectType == CardEffectType.SelectAttachedWeightTargets
                        ? CreatureEffectChoiceKind.SelectWeightTargets
                        : effect.EffectType == CardEffectType.ReleaseCatch
                        ? CreatureEffectChoiceKind.ReleaseAttachedCard
                        : CreatureEffectChoiceKind.RemoveNegativeEffect
                });
            }
        }

        // Group each shared reaction once, ordered by the first source's position in the chain.
        foreach (CardEffectType type in new[] { CardEffectType.RedrawTechniqueAfterCatch, CardEffectType.ReleaseCreatureAfterCatch })
        {
            List<int> sources = new List<int>();
            foreach (CardInstance source in catches ?? Array.Empty<CardInstance>())
            {
                if (source == null || !source.IsBait || source.InstanceId == instance.InstanceId) continue;
                if (Array.Exists(source.ActiveEffects, effect => effect?.EffectType == type
                    && effect.Trigger == CardEffectTrigger.AfterAnotherCatchAttached)) sources.Add(source.InstanceId);
            }
            bool eligibleType = type == CardEffectType.ReleaseCreatureAfterCatch || instance.Definition?.CardType == CardType.Creature;
            if (!instance.IsBait && eligibleType && sources.Count > 0)
            {
                immediate.Add(new Request
                {
                    Kind = type == CardEffectType.RedrawTechniqueAfterCatch
                        ? CreatureEffectChoiceKind.RedrawTechnique : CreatureEffectChoiceKind.OptionalPredatorRelease,
                    AttachedInstanceId = instance.InstanceId, EligibleSourceIds = sources.ToArray()
                });
            }
        }
        // Immediate attachment choices finish before all shared after-attachment reactions.
        int reactionStart = immediate.FindIndex(r => r.Kind == CreatureEffectChoiceKind.RedrawTechnique
            || r.Kind == CreatureEffectChoiceKind.OptionalPredatorRelease);
        if (reactionStart >= 0)
        {
            List<Request> reactions = immediate.GetRange(reactionStart, immediate.Count - reactionStart);
            reactions.Sort((a, b) => Array.FindIndex(catches, c => c?.InstanceId == a.EligibleSourceIds[0])
                .CompareTo(Array.FindIndex(catches, c => c?.InstanceId == b.EligibleSourceIds[0])));
            immediate.RemoveRange(reactionStart, immediate.Count - reactionStart);
            immediate.AddRange(reactions);
        }

        if (beforePendingChoices)
        {
            requests.InsertRange(0, immediate);
        }
        else
        {
            requests.AddRange(immediate);
        }
    }

    /// <summary>Snapshots pre-entry creature Catches and queues surviving Bait effects in chain order.</summary>
    public void BeginBiome(CardInstance[] catches)
    {
        biomeVisit++;
        List<int> eligible = new List<int>();
        foreach (CardInstance card in catches ?? Array.Empty<CardInstance>())
        {
            if (card != null && !card.IsBait && card.Definition?.CardType == CardType.Creature)
            {
                eligible.Add(card.InstanceId);
            }
        }

        foreach (CardInstance source in catches ?? Array.Empty<CardInstance>())
        {
            if (source == null || !source.IsBait || source.AttachmentBiomeVisit >= biomeVisit)
            {
                continue;
            }

            CardEffectDefinition[] effects = source.ActiveEffects;
            for (int effectIndex = 0; effectIndex < effects.Length; effectIndex++)
            {
                CardEffectDefinition effect = effects[effectIndex];
                if (effect?.Trigger == CardEffectTrigger.OnLaterBiomeStart
                    && effect.EffectType == CardEffectType.SwapCatchAtBiomeStart)
                {
                    requests.Add(new Request
                    {
                        SourceInstanceId = source.InstanceId,
                        EffectIndex = effectIndex,
                        AbilityName = effect.DisplayName,
                        Kind = CreatureEffectChoiceKind.SwapCatch,
                        EligibleCatchIds = eligible.ToArray()
                    });
                }
            }
        }
    }

    /// <summary>Returns only original pre-entry Catches still attached and not already replaced.</summary>
    public CardInstance[] GetSwapTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SwapCatch)
        {
            return Array.Empty<CardInstance>();
        }

        List<CardInstance> targets = new List<CardInstance>();
        foreach (CardInstance card in catches ?? Array.Empty<CardInstance>())
        {
            if (card != null && !card.IsBait && card.Definition?.CardType == CardType.Creature
                && Array.IndexOf(requests[0].EligibleCatchIds, card.InstanceId) >= 0)
            {
                targets.Add(card);
            }
        }

        return targets.ToArray();
    }

    /// <summary>Discards unavailable sources and creates each swap offer once at its resolution window.</summary>
    public void PrepareChoice(
        CatchChainRuntime chain,
        EncounterRuntime encounters,
        BiomeDefinition biome,
        TechniqueEffectRuntime techniques,
        EffectResolver resolver,
        System.Random random,
        Func<int> negativeEffectCount,
        TechniqueDeckRuntime deck = null,
        NextEncounterDestination nextDestination = null)
    {
        while (requests.Count > 0)
        {
            Request request = requests[0];
            if (request.Kind == CreatureEffectChoiceKind.TransferValue)
            {
                if (GetTransferTargets(chain.Catches).Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.OptionalPredatorRelease)
            {
                if (PrepareAttachmentReaction(request, chain.Catches, CardEffectType.ReleaseCreatureAfterCatch)
                    && GetPredatorReleaseTargets(chain.Catches).Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }

            // A completed Release trigger survives later removal of its Bait source.
            if (request.Kind == CreatureEffectChoiceKind.RecoverTechnique)
            {
                if (deck != null && Array.Exists(deck.DiscardPile, c => c?.CardType == CardType.Technique))
                {
                    return;
                }
                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.RedrawTechnique)
            {
                if (PrepareAttachmentReaction(request, chain.Catches, CardEffectType.RedrawTechniqueAfterCatch) && deck != null && deck.Hand.Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }

            CardInstance source = GetChoiceSource(chain.Catches);
            CardEffectDefinition[] active = source?.ActiveEffects;
            if (source == null || request.EffectIndex < 0 || request.EffectIndex >= active.Length
                || active[request.EffectIndex] == null)
            {
                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.ChooseNextEncounter)
            {
                if (nextDestination == null || !nextDestination.CanReplaceNormalEncounter)
                {
                    requests.RemoveAt(0);
                    continue;
                }
                if (!request.OffersGenerated)
                {
                    request.Offers = encounters.OfferCreatures(nextDestination.Biome.GetEncounterPool(nextDestination.Depth),
                        nextDestination.Biome.BiomeId, nextDestination.Depth, 2, random, chain.ActiveEffectRecords,
                        resolver, techniques.PendingEncounterEffects, active[request.EffectIndex].RequiredTags);
                    request.OffersGenerated = true;
                }
                if (request.Offers.Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.RemoveNegativeEffect)
            {
                if (negativeEffectCount() > 0)
                {
                    return;
                }

                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.SelectWeightTargets)
            {
                if (GetWeightTargets(chain.Catches).Length > 0) return;
                source.SetEffectTargets(request.EffectIndex, request.SelectedTargetIds.ToArray());
                chain.Recalculate(resolver);
                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.ReleaseAttachedCard)
            {
                if (GetReleaseTargets(chain.Catches).Length > 0)
                {
                    return;
                }
                requests.RemoveAt(0);
                continue;
            }

            if (biome == null || GetSwapTargets(chain.Catches).Length == 0)
            {
                requests.RemoveAt(0);
                continue;
            }

            if (!request.OffersGenerated)
            {
                request.Offers = encounters.OfferCreatures(
                    biome.GetEncounterPool(biome.StartingDepth), biome.BiomeId, biome.StartingDepth,
                    2, random, chain.ActiveEffectRecords, resolver, techniques.PendingEncounterEffects);
                request.OffersGenerated = true;
            }

            if (request.Offers.Length == 2)
            {
                return;
            }

            requests.RemoveAt(0);
        }
    }

    /// <summary>Evaluates a shared reaction after attachment effects, using only surviving original Bait sources.</summary>
    private static bool PrepareAttachmentReaction(Request request, CardInstance[] catches, CardEffectType type)
    {
        CardInstance attached = Array.Find(catches, c => c?.InstanceId == request.AttachedInstanceId);
        if (attached == null || attached.IsBait || attached.Definition == null
            || (type == CardEffectType.RedrawTechniqueAfterCatch && attached.Definition.CardType != CardType.Creature)) return false;
        foreach (CardInstance source in catches)
        {
            if (source == null || !source.IsBait || Array.IndexOf(request.EligibleSourceIds, source.InstanceId) < 0) continue;
            CardEffectDefinition[] effects = source.ActiveEffects;
            for (int i = 0; i < effects.Length; i++)
            {
                CardEffectDefinition effect = effects[i];
                if (effect?.EffectType == type
                    && effect.Trigger == CardEffectTrigger.AfterAnotherCatchAttached
                    && EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, attached))
                {
                    request.SourceInstanceId = source.InstanceId;
                    request.EffectIndex = i;
                    request.AbilityName = effect.DisplayName;
                    return true;
                }
            }
        }
        return false;
    }

    public int SelectedWeightTargetCount => ChoiceKind == CreatureEffectChoiceKind.SelectWeightTargets
        ? requests[0].SelectedTargetIds.Count : 0;

    /// <summary>Returns eligible distinct recipients for the remaining one-time target selections.</summary>
    public CardInstance[] GetWeightTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SelectWeightTargets) return Array.Empty<CardInstance>();
        Request request = requests[0];
        CardInstance source = GetChoiceSource(catches);
        CardEffectDefinition[] active = source?.ActiveEffects;
        CardEffectDefinition effect = active != null && request.EffectIndex >= 0 && request.EffectIndex < active.Length
            ? active[request.EffectIndex] : null;
        return Array.FindAll(catches, target => target != null && target != source
            && target.Definition?.CardType == CardType.Creature
            && !request.SelectedTargetIds.Contains(target.InstanceId)
            && effect != null && EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, target));
    }

    /// <summary>Selects up to two distinct recipients, or finishes early, without allowing later retargeting.</summary>
    public bool TryChooseWeightTarget(int targetInstanceId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SelectWeightTargets) return false;
        Request request = requests[0];
        if (targetInstanceId >= 0)
        {
            if (!Array.Exists(GetWeightTargets(chain.Catches), c => c.InstanceId == targetInstanceId)) return false;
            request.SelectedTargetIds.Add(targetInstanceId);
        }
        if (targetInstanceId < 0 || request.SelectedTargetIds.Count == 2 || GetWeightTargets(chain.Catches).Length == 0)
        {
            CardInstance source = GetChoiceSource(chain.Catches);
            if (source == null) return false;
            source.SetEffectTargets(request.EffectIndex, request.SelectedTargetIds.ToArray());
            CompleteChoice();
            chain.Recalculate(resolver);
        }
        return true;
    }

    /// <summary>Resolves an optional redraw immediately; declining never discards or draws.</summary>
    public bool TryChooseRedraw(int handIndex, TechniqueDeckRuntime deck, System.Random random)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.RedrawTechnique
            || (handIndex >= 0 && !deck.TryRedrawTechnique(handIndex, random))) return false;
        CompleteChoice();
        return true;
    }

    /// <summary>Starts a new encounter window; replacing the Hooked card does not call this.</summary>
    public void BeginEncounter()
    {
        encounterRecoveryConsumed = false;
    }

    /// <summary>Records the first qualifying deliberate Release while an eligible Bait source is attached.</summary>
    public void RecordDeliberateRelease(CardInstance released, CatchRemovalReason reason, CardInstance[] survivors)
    {
        RecordValueTransfer(released, reason, survivors);
        if (encounterRecoveryConsumed || reason == CatchRemovalReason.LineStrain
            || released?.Definition?.CardType != CardType.Creature)
        {
            return;
        }

        foreach (CardInstance source in survivors ?? Array.Empty<CardInstance>())
        {
            if (source == null || !source.IsBait || source.InstanceId == released.InstanceId)
            {
                continue;
            }
            CardEffectDefinition[] effects = source.ActiveEffects;
            for (int index = 0; index < effects.Length; index++)
            {
                CardEffectDefinition effect = effects[index];
                if (effect?.EffectType != CardEffectType.RecoverDiscardedTechnique
                    || effect.Trigger != CardEffectTrigger.WhenAnotherCatchReleased
                    || !EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, released))
                {
                    continue;
                }

                // Consume before checking the discard pile: an empty pile still spends this window.
                encounterRecoveryConsumed = true;
                pendingRecovery = new Request
                {
                    SourceInstanceId = source.InstanceId,
                    EffectIndex = index,
                    AbilityName = effect.DisplayName,
                    Kind = CreatureEffectChoiceKind.RecoverTechnique
                };
                return;
            }
        }
    }

    /// <summary>Exposes recovery and captured Value transfers after the entire releasing ability finishes.</summary>
    public void FinishReleaseResolution()
    {
        List<Request> completed = new List<Request>();
        if (pendingRecovery != null)
        {
            completed.Add(pendingRecovery);
            pendingRecovery = null;
        }
        completed.AddRange(pendingTransfers);
        pendingTransfers.Clear();
        requests.InsertRange(0, completed);
    }

    /// <summary>Records one pre-removal Value per released Elusive Catch, shared across all matching Bait copies.</summary>
    private void RecordValueTransfer(CardInstance released, CatchRemovalReason reason, CardInstance[] survivors)
    {
        if (reason == CatchRemovalReason.LineStrain || released == null || released.IsBait) return;
        foreach (CardInstance source in survivors ?? Array.Empty<CardInstance>())
        {
            if (source == null || !source.IsBait) continue;
            CardEffectDefinition effect = Array.Find(source.ActiveEffects, e => e?.EffectType == CardEffectType.TransferReleasedCatchValue
                && e.Trigger == CardEffectTrigger.WhenAnotherCatchReleased
                && EffectResolver.RequiredTagsMatchInstance(e.RequiredTags, released));
            if (effect == null) continue;
            pendingTransfers.Add(new Request
            {
                SourceInstanceId = source.InstanceId, AbilityName = effect.DisplayName,
                Kind = CreatureEffectChoiceKind.TransferValue, CapturedValue = released.CurrentValue,
                TransferEffect = effect, TransferSource = source.Definition
            });
            return;
        }
    }

    /// <summary>Returns remaining Catch-role attachments as recipients of one completed transfer.</summary>
    public CardInstance[] GetTransferTargets(CardInstance[] catches)
    {
        return ChoiceKind == CreatureEffectChoiceKind.TransferValue
            ? Array.FindAll(catches, c => c != null && !c.IsBait) : Array.Empty<CardInstance>();
    }

    /// <summary>Applies one lasting captured Value bonus independently of its original Bait source.</summary>
    public bool TryChooseValueTransfer(int targetId, CatchChainRuntime chain, EffectResolver resolver)
    {
        CardInstance target = Array.Find(GetTransferTargets(chain.Catches), c => c.InstanceId == targetId);
        if (target == null) return false;
        Request request = requests[0];
        target.AddPermanentModifiers(0, request.CapturedValue, request.TransferEffect, request.TransferSource);
        CompleteChoice();
        chain.Recalculate(resolver);
        return true;
    }

    /// <summary>Returns legal Small creature targets other than the Predator just attached.</summary>
    public CardInstance[] GetPredatorReleaseTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.OptionalPredatorRelease) return Array.Empty<CardInstance>();
        Request request = requests[0];
        CardInstance source = GetChoiceSource(catches);
        CardEffectDefinition[] active = source?.ActiveEffects;
        CardEffectDefinition effect = active != null && request.EffectIndex >= 0 && request.EffectIndex < active.Length
            ? active[request.EffectIndex] : null;
        return Array.FindAll(catches, c => c != null && c.InstanceId != request.AttachedInstanceId
            && c.Definition?.CardType == CardType.Creature && !c.PreventsReleaseWhileAttached
            && effect != null && EffectResolver.RequiredTagsMatchInstance(effect.TargetTags, c));
    }

    /// <summary>Resolves or declines the shared immediate release, then exposes any resulting Release reactions.</summary>
    public bool TryChoosePredatorRelease(int targetId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.OptionalPredatorRelease) return false;
        if (targetId >= 0)
        {
            if (!Array.Exists(GetPredatorReleaseTargets(chain.Catches), c => c.InstanceId == targetId)) return false;
            int index = Array.FindIndex(chain.Catches, c => c?.InstanceId == targetId);
            if (!chain.TryRelease(index, resolver, out _, out _, out _, CatchRemovalReason.Creature)) return false;
        }
        CompleteChoice();
        FinishReleaseResolution();
        return true;
    }

    /// <summary>Queues a selected next encounter without revealing it or tying it to its source's attached lifetime.</summary>
    public bool TryChooseNextEncounter(int offerIndex)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ChooseNextEncounter
            || offerIndex >= OfferedCreatures.Length) return false;
        if (offerIndex >= 0) queuedEncounter = OfferedCreatures[offerIndex];
        CompleteChoice();
        return true;
    }

    /// <summary>Consumes the next-encounter selection once, falling back if destination eligibility or scheduling changed.</summary>
    public CardDefinition TakeQueuedEncounter(NextEncounterDestination destination)
    {
        CardDefinition selected = queuedEncounter;
        queuedEncounter = null;
        return destination != null && destination.IsEligible(selected) ? selected : null;
    }

    /// <summary>Returns legal deliberate Release targets other than the ability source, in either role.</summary>
    public CardInstance[] GetReleaseTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ReleaseAttachedCard)
        {
            return Array.Empty<CardInstance>();
        }
        return Array.FindAll(catches ?? Array.Empty<CardInstance>(), c => c != null
            && c.InstanceId != requests[0].SourceInstanceId && !c.PreventsReleaseWhileAttached);
    }

    /// <summary>Resolves the one-use attachment Release and then exposes any resulting recovery.</summary>
    public bool TryRelease(int targetInstanceId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (!Array.Exists(GetReleaseTargets(chain.Catches), c => c.InstanceId == targetInstanceId))
        {
            return false;
        }
        int index = Array.FindIndex(chain.Catches, c => c?.InstanceId == targetInstanceId);
        if (!chain.TryRelease(index, resolver, out _, out _, out _, CatchRemovalReason.Creature))
        {
            return false;
        }
        CompleteChoice();
        FinishReleaseResolution();
        return true;
    }

    /// <summary>Completes one resolved choice, leaving subsequent sources queued.</summary>
    public void CompleteChoice()
    {
        if (requests.Count > 0)
        {
            requests.RemoveAt(0);
        }
    }

    /// <summary>Validates both selections, swaps the source Catch, and prioritizes new on-attach effects.</summary>
    public bool TrySwap(
        int targetInstanceId,
        int offerIndex,
        CatchChainRuntime chain,
        EffectResolver resolver,
        System.Random random)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SwapCatch
            || offerIndex < 0 || offerIndex >= OfferedCreatures.Length
            || !Array.Exists(GetSwapTargets(chain.Catches), c => c.InstanceId == targetInstanceId))
        {
            return false;
        }

        CardInstance replacement = chain.ReplaceCreatureCatch(
            targetInstanceId, OfferedCreatures[offerIndex], biomeVisit, resolver, random);
        if (replacement == null)
        {
            return false;
        }

        CompleteChoice();
        RecordAttachment(replacement, true, chain.Catches);
        return true;
    }
}

/// <summary>Immutable preview of the next normal reveal destination, including Apex/Location scheduling eligibility.</summary>
public sealed class NextEncounterDestination
{
    public BiomeDefinition Biome { get; }
    public int Depth { get; }
    public bool CanReplaceNormalEncounter { get; }

    /// <summary>Captures the destination without advancing run depth or consuming next-Descend modifiers.</summary>
    public NextEncounterDestination(BiomeDefinition biome, int depth, bool canReplaceNormalEncounter = true)
    {
        Biome = biome;
        Depth = depth;
        CanReplaceNormalEncounter = canReplaceNormalEncounter && biome != null;
    }

    /// <summary>Validates a queued Schooling species against the actual reveal's pool, biome, depth, and Apex priority.</summary>
    public bool IsEligible(CardDefinition card)
    {
        return CanReplaceNormalEncounter && card?.CardType == CardType.Creature
            && EffectResolver.RequiredTagsMatch(new[] { "Schooling" }, card)
            && card.IsAvailableInBiome(Biome.BiomeId) && card.IsAvailableAtDepth(Depth)
            && Array.Exists(Biome.GetEncounterPool(Depth), candidate => candidate == card);
    }
}
