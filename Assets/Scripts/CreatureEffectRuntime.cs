using System;
using System.Collections.Generic;
using UnityEngine;

public enum CreatureEffectChoiceKind
{
    None,
    RemoveNegativeEffect,
    SwapCatch,
    ReleaseAttachedCard,
    RecoverTechnique
}

/// <summary>Owns ordered creature attachment and later-biome choices without owning their presentation.</summary>
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
    }

    [SerializeField] private List<Request> requests = new List<Request>();
    [SerializeField] private int biomeVisit;
    [SerializeField] private bool encounterRecoveryConsumed;
    [SerializeField] private Request pendingRecovery;

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
    }

    /// <summary>Stamps a new attachment and queues its selected role's immediate choice effects.</summary>
    public void RecordAttachment(CardInstance instance, bool beforePendingChoices = false)
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
                && (effect.EffectType == CardEffectType.RemoveNegativeEffect
                    || effect.EffectType == CardEffectType.ReleaseCatch))
            {
                immediate.Add(new Request
                {
                    SourceInstanceId = instance.InstanceId,
                    EffectIndex = effectIndex,
                    AbilityName = effect.DisplayName,
                    Kind = effect.EffectType == CardEffectType.ReleaseCatch
                        ? CreatureEffectChoiceKind.ReleaseAttachedCard
                        : CreatureEffectChoiceKind.RemoveNegativeEffect
                });
            }
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
        TechniqueDeckRuntime deck = null)
    {
        while (requests.Count > 0)
        {
            Request request = requests[0];
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

            CardInstance source = GetChoiceSource(chain.Catches);
            CardEffectDefinition[] active = source?.ActiveEffects;
            if (source == null || request.EffectIndex < 0 || request.EffectIndex >= active.Length
                || active[request.EffectIndex] == null)
            {
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

    /// <summary>Starts a new encounter window; replacing the Hooked card does not call this.</summary>
    public void BeginEncounter()
    {
        encounterRecoveryConsumed = false;
    }

    /// <summary>Records the first qualifying deliberate Release while an eligible Bait source is attached.</summary>
    public void RecordDeliberateRelease(CardInstance released, CatchRemovalReason reason, CardInstance[] survivors)
    {
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
                    || !EffectResolver.RequiredTagsMatch(effect.RequiredTags, released.Definition))
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

    /// <summary>Exposes recovery only after the entire releasing technique or creature ability finishes.</summary>
    public void FinishReleaseResolution()
    {
        if (pendingRecovery != null)
        {
            requests.Insert(0, pendingRecovery);
            pendingRecovery = null;
        }
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
        RecordAttachment(replacement, true);
        return true;
    }
}
