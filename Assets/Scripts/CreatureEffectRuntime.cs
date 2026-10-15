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
    TransferValue,
    GainAttachmentValue,
    FeedingFrenzy,
    DrawTechniques,
    ExchangeTechniques,
    DiscardTechnique,
    SelectReleaseTargets,
    SwapCreatureModes,
    ActivateChangedRole,
    RecoverPlayedTechnique,
    ReleaseMatchingBait,
    EncounterSkip,
    ResolveReleaseDraws,
    EventSnagTarget,
    EventCreatureRelease
}

/// <summary>Owns ordered creature choices, deferred Release reactions, and a queued encounter without owning presentation.</summary>
[Serializable]
public sealed class CreatureEffectRuntime
{
    [Serializable]
    private sealed class ReleaseDrawEligibility
    {
        public int[] SourceIds = Array.Empty<int>();
    }

    [Serializable]
    private sealed class Request
    {
        public int SourceInstanceId;
        public CardDefinition EventSource;
        public CardEffectDefinition EventEffect;
        public long EventOccurrenceId;
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
        public int DrawCount;
        public int SelectedHandIndex = -1;
        public int SelectedCatchId = -1;
        public CatchAttachmentRole ExpectedRole;
        public bool RequireSourceRole;
        public int MaxDiscards = -1;
        public int DiscardCount;
        public CardDefinition PlayedTechnique;
        public int PlayedDiscardOccurrenceId;
        public int EncounterRevision;
        public CardDefinition Encounter;
        public List<ReleaseDrawEligibility> ReleaseDrawSourceGroups = new List<ReleaseDrawEligibility>();
    }

    [SerializeField] private List<Request> requests = new List<Request>();
    [SerializeField] private int biomeVisit;
    [SerializeField] private bool encounterRecoveryConsumed;
    [SerializeField] private Request pendingRecovery;
    [SerializeField] private List<Request> pendingTransfers = new List<Request>();
    [SerializeField] private CardDefinition queuedEncounter;
    [SerializeField] private List<CardEffectDefinition> pendingSelectedTechniqueRecoveries = new List<CardEffectDefinition>();
    [SerializeField] private CardDefinition resolvingTechnique;
    [SerializeField] private int resolvingDiscardOccurrenceId;
    [SerializeField] private int[] eligibleAttachedIdsAtTechniquePlay = Array.Empty<int>();
    [SerializeField] private Request pendingPlayedTechniqueRecovery;
    public EncounterRuntime EncounterEffects { get; set; }
    public EventPersistenceRuntime EventPersistence { get; set; }

    /// <summary>Queues a mandatory heaviest-target tie decision independently of attachment sources.</summary>
    public void QueueEventSnag(long origin, CardDefinition source, CardEffectDefinition effect)
    {
        requests.Add(new Request { Kind = CreatureEffectChoiceKind.EventSnagTarget, EventSource = source,
            EventEffect = effect, EventOccurrenceId = origin, AbilityName = source.DisplayName });
    }

    /// <summary>Queues an optional event Release without an attached source.</summary>
    public void QueueEventRelease(string name)
    { requests.Add(new Request { Kind = CreatureEffectChoiceKind.EventCreatureRelease, AbilityName = name }); }

    /// <summary>Finds the current heaviest non-Armored creatures in either attachment role.</summary>
    public CardInstance[] GetEventSnagTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.EventSnagTarget) return Array.Empty<CardInstance>();
        CardInstance[] eligible = Array.FindAll(catches, c => c?.Definition?.CardType == CardType.Creature
            && !c.HasTag(CreatureTagVocabulary.Armored));
        int heaviest = -1;
        foreach (CardInstance card in eligible) heaviest = Math.Max(heaviest, card.CurrentWeight);
        return Array.FindAll(eligible, c => c.CurrentWeight == heaviest);
    }

    /// <summary>Applies a selected Snag after Techniques, preserving its recipient through tag changes.</summary>
    public bool TryChooseEventSnag(int targetId, CatchChainRuntime chain)
    {
        if (!Array.Exists(GetEventSnagTargets(chain.Catches), c => c.InstanceId == targetId)) return false;
        Request request = requests[0];
        if (EventPersistence == null || !EventPersistence.TryAddModifier(request.EventOccurrenceId, request.EventSource,
            request.EventEffect, targetId, 1, 0, request.EventEffect.EventLifetime, out _)) return false;
        CompleteChoice(); return true;
    }

    /// <summary>Finds releasable creatures without applying Technique targeting restrictions.</summary>
    public CardInstance[] GetEventReleaseTargets(CardInstance[] catches)
    {
        return ChoiceKind != CreatureEffectChoiceKind.EventCreatureRelease ? Array.Empty<CardInstance>()
            : Array.FindAll(catches, c => c?.Definition?.CardType == CardType.Creature && !c.PreventsReleaseWhileAttached);
    }

    /// <summary>Declines or performs one event Release and queues its ordinary Release reactions.</summary>
    public bool TryChooseEventRelease(int targetId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.EventCreatureRelease) return false;
        if (targetId >= 0)
        {
            if (!Array.Exists(GetEventReleaseTargets(chain.Catches), c => c.InstanceId == targetId)) return false;
            int index = Array.FindIndex(chain.Catches, c => c?.InstanceId == targetId);
            if (!chain.TryRelease(index, resolver, out _, out _, out _, CatchRemovalReason.Event)) return false;
        }
        CompleteChoice(); FinishReleaseResolution(); return true;
    }
    [SerializeField] private List<string> pendingCreatureSkips = new List<string>();
    [SerializeField] private List<ReleaseDrawEligibility> pendingReleaseDrawSourceGroups = new List<ReleaseDrawEligibility>();
    [SerializeField] private string pendingReleaseDrawName;
    public int PendingCreatureSkipCount => pendingCreatureSkips.Count;
    public int PendingSelectedTechniqueRecoveryCount => pendingSelectedTechniqueRecoveries.Count;
    public int PendingDrawCount => ChoiceKind == CreatureEffectChoiceKind.DrawTechniques ? requests[0].DrawCount : 0;
    public int SelectedReleaseCount => ChoiceKind == CreatureEffectChoiceKind.SelectReleaseTargets ? requests[0].SelectedTargetIds.Count : 0;
    public int SelectedModeSwapCatchId => ChoiceKind == CreatureEffectChoiceKind.SwapCreatureModes ? requests[0].SelectedCatchId : -1;

    public int SelectedExchangeHandIndex => ChoiceKind == CreatureEffectChoiceKind.ExchangeTechniques
        ? requests[0].SelectedHandIndex : -1;
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
        pendingSelectedTechniqueRecoveries.Clear();
        resolvingTechnique = null;
        resolvingDiscardOccurrenceId = 0;
        eligibleAttachedIdsAtTechniquePlay = Array.Empty<int>();
        pendingPlayedTechniqueRecovery = null;
        pendingCreatureSkips.Clear();
        pendingReleaseDrawSourceGroups.Clear();
        pendingReleaseDrawName = null;
    }

    /// <summary>Stamps a new attachment and queues its selected role's immediate choice effects.</summary>
    public void RecordAttachment(CardInstance instance, bool beforePendingChoices = false, CardInstance[] catches = null,
        bool isRoleChange = false)
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
            if (effect?.Trigger == CardEffectTrigger.WhenCaught)
            {
                if (effect.EffectType == CardEffectType.QueueCreatureEncounterConcealment && !instance.IsBait)
                {
                    if (EncounterEffects == null) throw new InvalidOperationException("Encounter runtime must be bound before concealment activation.");
                    EncounterEffects.GrantCreatureConcealment(effect, instance.Definition);
                }
                if (effect.EffectType == CardEffectType.GrantNextCreatureSkip && instance.IsBait
                    && instance.TryConsumeAttachmentActivation(effectIndex))
                    pendingCreatureSkips.Add(effect.DisplayName);
                if (effect.EffectType == CardEffectType.ReleaseMatchingBaitOnAttach && !instance.IsBait)
                    immediate.Add(new Request { SourceInstanceId = instance.InstanceId, EffectIndex = effectIndex,
                        AbilityName = effect.DisplayName, Kind = CreatureEffectChoiceKind.ReleaseMatchingBait });
                if (effect.EffectType == CardEffectType.DiscardTechniqueOnAttach && !instance.IsBait)
                    immediate.Add(new Request { SourceInstanceId = instance.InstanceId, EffectIndex = effectIndex,
                        AbilityName = effect.DisplayName, Kind = CreatureEffectChoiceKind.DiscardTechnique });
                if (effect.EffectType == CardEffectType.DrawTechniqueOnAttach && !instance.IsBait)
                    immediate.Add(new Request { SourceInstanceId = instance.InstanceId, EffectIndex = effectIndex,
                        AbilityName = effect.DisplayName, Kind = CreatureEffectChoiceKind.DrawTechniques,
                        DrawCount = effect.Amount, MaxDiscards = 1 });
                if (instance.IsBait && (effect.EffectType == CardEffectType.ReleaseSelectedCreaturesOnAttach
                    || effect.EffectType == CardEffectType.SwapCreatureModesOnAttach
                    || effect.EffectType == CardEffectType.GrantSelectedTechniqueRecovery)
                    && instance.TryConsumeAttachmentActivation(effectIndex))
                {
                    if (effect.EffectType == CardEffectType.GrantSelectedTechniqueRecovery)
                        pendingSelectedTechniqueRecoveries.Add(effect);
                    else
                        immediate.Add(new Request { SourceInstanceId = instance.InstanceId, EffectIndex = effectIndex,
                            AbilityName = effect.DisplayName, Kind = effect.EffectType == CardEffectType.ReleaseSelectedCreaturesOnAttach
                                ? CreatureEffectChoiceKind.SelectReleaseTargets : CreatureEffectChoiceKind.SwapCreatureModes });
                }
            }
            if (effect?.Trigger == CardEffectTrigger.WhenCaught
                && (effect.EffectType == CardEffectType.RemoveNegativeEffect
                    || effect.EffectType == CardEffectType.ReleaseCatch
                    || effect.EffectType == CardEffectType.SelectAttachedWeightTargets)
                && (!instance.IsBait || effect.EffectType == CardEffectType.RemoveNegativeEffect
                    || instance.TryConsumeAttachmentActivation(effectIndex)))
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

        foreach (Request request in immediate)
        {
            request.ExpectedRole = instance.AttachmentRole;
            request.RequireSourceRole = true;
        }
        // A role exchange activates only the swapped cards' own on-attach abilities, not a new catch.
        if (!isRoleChange)
        {
            // Group each shared reaction once, ordered by the first source's position in the chain.
            foreach (CardEffectType type in new[] { CardEffectType.RedrawTechniqueAfterCatch, CardEffectType.ReleaseCreatureAfterCatch,
                CardEffectType.GainSelfValueAfterCatch, CardEffectType.ReleaseSelfToDrawTechniques, CardEffectType.ExchangeTechniqueAfterCatch })
            {
                List<int> sources = new List<int>();
                foreach (CardInstance source in catches ?? Array.Empty<CardInstance>())
                {
                    bool catchSource = type == CardEffectType.GainSelfValueAfterCatch;
                    if (source == null || source.IsBait == catchSource || source.InstanceId == instance.InstanceId) continue;
                    if (Array.Exists(source.ActiveEffects, effect => effect?.EffectType == type
                        && effect.Trigger == CardEffectTrigger.AfterAnotherCatchAttached)) sources.Add(source.InstanceId);
                }
                bool eligibleType = type == CardEffectType.ReleaseCreatureAfterCatch || type == CardEffectType.ReleaseSelfToDrawTechniques
                    || instance.Definition?.CardType == CardType.Creature;
                if (!instance.IsBait && eligibleType && sources.Count > 0)
                {
                    immediate.Add(new Request
                    {
                        Kind = type == CardEffectType.RedrawTechniqueAfterCatch ? CreatureEffectChoiceKind.RedrawTechnique
                            : type == CardEffectType.GainSelfValueAfterCatch ? CreatureEffectChoiceKind.GainAttachmentValue
                            : type == CardEffectType.ReleaseSelfToDrawTechniques ? CreatureEffectChoiceKind.FeedingFrenzy
                            : type == CardEffectType.ExchangeTechniqueAfterCatch ? CreatureEffectChoiceKind.ExchangeTechniques
                            : CreatureEffectChoiceKind.OptionalPredatorRelease,
                        AttachedInstanceId = instance.InstanceId, EligibleSourceIds = sources.ToArray()
                    });
                }
            }
            // Immediate attachment choices finish before all shared after-attachment reactions.
            int reactionStart = immediate.FindIndex(r => r.Kind == CreatureEffectChoiceKind.RedrawTechnique
                || r.Kind == CreatureEffectChoiceKind.OptionalPredatorRelease
                || r.Kind == CreatureEffectChoiceKind.GainAttachmentValue
                || r.Kind == CreatureEffectChoiceKind.FeedingFrenzy
                || r.Kind == CreatureEffectChoiceKind.ExchangeTechniques);
            if (reactionStart >= 0)
            {
                List<Request> reactions = immediate.GetRange(reactionStart, immediate.Count - reactionStart);
                reactions.Sort((a, b) => Array.FindIndex(catches, c => c?.InstanceId == a.EligibleSourceIds[0])
                    .CompareTo(Array.FindIndex(catches, c => c?.InstanceId == b.EligibleSourceIds[0])));
                immediate.RemoveRange(reactionStart, immediate.Count - reactionStart);
                immediate.AddRange(reactions);
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
                        Kind = CreatureEffectChoiceKind.SwapCatch, ExpectedRole = CatchAttachmentRole.Bait, RequireSourceRole = true,
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
            if (request.Kind == CreatureEffectChoiceKind.EventSnagTarget)
            {
                CardInstance[] targets = GetEventSnagTargets(chain.Catches);
                if (targets.Length > 1) return;
                if (targets.Length == 1)
                {
                    if (!TryChooseEventSnag(targets[0].InstanceId, chain))
                        throw new InvalidOperationException("Bind event Snag persistence before resolution.");
                }
                else CompleteChoice();
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.EventCreatureRelease)
            {
                if (GetEventReleaseTargets(chain.Catches).Length > 0) return;
                CompleteChoice(); continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.EncounterSkip)
            {
                if (encounters.EncounterRevision == request.EncounterRevision
                    && encounters.HookedEncounter == request.Encounter) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.ResolveReleaseDraws)
            {
                request.DrawCount = request.ReleaseDrawSourceGroups.FindAll(group => Array.Exists(chain.Catches,
                    c => c != null && c.IsBait && Array.IndexOf(group.SourceIds, c.InstanceId) >= 0
                        && Array.Exists(c.ActiveEffects, e => e?.EffectType == CardEffectType.DrawTechniqueAfterCreatureRelease))).Count;
                if (request.DrawCount == 0)
                {
                    requests.RemoveAt(0);
                    continue;
                }
                request.Kind = CreatureEffectChoiceKind.DrawTechniques;
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.ActivateChangedRole)
            {
                CardInstance changed = GetChoiceSource(chain.Catches);
                requests.RemoveAt(0);
                if (changed != null && changed.AttachmentRole == request.ExpectedRole)
                {
                    chain.ActivateRoleOnAttach(changed, resolver);
                    RecordAttachment(changed, true, chain.Catches, true);
                }
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.RecoverPlayedTechnique)
            {
                if (deck == null || !deck.HasDiscardedTechniqueOccurrence(request.PlayedDiscardOccurrenceId, request.PlayedTechnique))
                {
                    requests.RemoveAt(0);
                    continue;
                }
                if (deck.Hand.Length < 4)
                    deck.TryRecoverPlayedTechnique(request.PlayedDiscardOccurrenceId, request.PlayedTechnique, 4);
                else if (deck.Hand.Length == 4 && request.DiscardCount == 0) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.GainAttachmentValue)
            {
                ApplyAttachmentValue(request, chain, resolver);
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.FeedingFrenzy)
            {
                if (PrepareAttachmentReaction(request, chain.Catches, CardEffectType.ReleaseSelfToDrawTechniques)
                    && GetFeedingFrenzySources(chain.Catches).Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.ExchangeTechniques)
            {
                if (PrepareAttachmentReaction(request, chain.Catches, CardEffectType.ExchangeTechniqueAfterCatch)
                    && deck != null && Array.Exists(deck.Hand, c => c?.CardType == CardType.Technique)
                    && Array.Exists(deck.DiscardPile, c => c?.CardType == CardType.Technique)) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.DrawTechniques)
            {
                if (deck == null)
                {
                    requests.RemoveAt(0);
                    continue;
                }
                bool canMakeRoom = request.MaxDiscards < 0
                    || deck.Hand.Length - (request.MaxDiscards - request.DiscardCount) <= 4 - Math.Min(4, request.DrawCount);
                if (deck.Hand.Length > 4 - Math.Min(4, request.DrawCount) && canMakeRoom
                    && (request.MaxDiscards < 0 || request.DiscardCount < request.MaxDiscards)) return;
                deck.DrawTechniques(request.DrawCount, 4, random);
                requests.RemoveAt(0);
                continue;
            }
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
            if (source == null || (request.RequireSourceRole && source.AttachmentRole != request.ExpectedRole)
                || request.EffectIndex < 0 || request.EffectIndex >= active.Length
                || active[request.EffectIndex] == null)
            {
                requests.RemoveAt(0);
                continue;
            }

            if (request.Kind == CreatureEffectChoiceKind.ReleaseMatchingBait)
            {
                if (GetMatchingBaitReleaseTargets(chain.Catches).Length > 0) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.DiscardTechnique)
            {
                if (deck != null && Array.Exists(deck.Hand, c => c?.CardType == CardType.Technique)) return;
                requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.SelectReleaseTargets)
            {
                if (GetSelectedReleaseTargets(chain.Catches).Length > 0) return;
                if (!ResolveSelectedReleases(chain, resolver)) requests.RemoveAt(0);
                continue;
            }
            if (request.Kind == CreatureEffectChoiceKind.SwapCreatureModes)
            {
                if (GetModeSwapTargets(chain, false).Length > 0 && GetModeSwapTargets(chain, true).Length > 0) return;
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
            || ((type == CardEffectType.RedrawTechniqueAfterCatch || type == CardEffectType.ExchangeTechniqueAfterCatch)
                && attached.Definition.CardType != CardType.Creature)) return false;
        foreach (CardInstance source in catches)
        {
            if (source == null || !source.IsBait || Array.IndexOf(request.EligibleSourceIds, source.InstanceId) < 0
                || (type == CardEffectType.ReleaseSelfToDrawTechniques && source.PreventsReleaseWhileAttached)) continue;
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

    /// <summary>Reserves the oldest skip at reveal before on-reveal effects can remove or replace the original encounter.</summary>
    public void RecordCreatureReveal(CardDefinition creature, int revision)
    {
        if (creature?.CardType != CardType.Creature || pendingCreatureSkips.Count == 0) return;
        string name = pendingCreatureSkips[0];
        pendingCreatureSkips.RemoveAt(0);
        requests.Insert(0, new Request { Kind = CreatureEffectChoiceKind.EncounterSkip,
            AbilityName = name, Encounter = creature, EncounterRevision = revision });
    }

    /// <summary>Immediately skips or declines the original offered creature without attaching or dispatching Release effects.</summary>
    public bool TryChooseEncounterSkip(bool skip, EncounterRuntime encounters)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.EncounterSkip) return false;
        Request request = requests[0];
        if (encounters.EncounterRevision != request.EncounterRevision || encounters.HookedEncounter != request.Encounter) return false;
        CompleteChoice();
        if (skip)
        {
            encounters.SetCurrentEncounter(null);
            encounters.CancelActiveEncounterChain();
        }
        return true;
    }

    /// <summary>Returns only another legally releasable attached Elusive creature Bait.</summary>
    public CardInstance[] GetMatchingBaitReleaseTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ReleaseMatchingBait) return Array.Empty<CardInstance>();
        CardInstance source = GetChoiceSource(catches);
        CardEffectDefinition effect = source?.ActiveEffects[requests[0].EffectIndex];
        return Array.FindAll(catches, c => c != null && c != source && c.IsBait
            && c.Definition?.CardType == CardType.Creature && !c.PreventsReleaseWhileAttached
            && effect != null && EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, c));
    }

    /// <summary>Resolves the optional Eel Catch Release, then exposes ordinary completed Release reactions.</summary>
    public bool TryChooseMatchingBaitRelease(int targetId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ReleaseMatchingBait) return false;
        if (targetId >= 0)
        {
            CardInstance target = Array.Find(GetMatchingBaitReleaseTargets(chain.Catches), c => c.InstanceId == targetId);
            if (target == null || !chain.TryRelease(Array.IndexOf(chain.Catches, target), resolver,
                out _, out _, out _, CatchRemovalReason.Creature)) return false;
        }
        CompleteChoice();
        FinishReleaseResolution();
        return true;
    }

    /// <summary>Records one draw per released Elusive creature and the original Eel Baits that may survive its whole action.</summary>
    private void RecordReleaseDraw(CardInstance released, CatchRemovalReason reason, CardInstance[] survivors)
    {
        if (reason == CatchRemovalReason.LineStrain || released?.Definition?.CardType != CardType.Creature) return;
        List<int> sources = new List<int>();
        foreach (CardInstance source in survivors)
        {
            if (source == null || !source.IsBait || source.InstanceId == released.InstanceId) continue;
            CardEffectDefinition effect = Array.Find(source.ActiveEffects, e => e?.EffectType == CardEffectType.DrawTechniqueAfterCreatureRelease
                && e.Trigger == CardEffectTrigger.WhenAnotherCatchReleased
                && EffectResolver.RequiredTagsMatchInstance(e.RequiredTags, released));
            if (effect == null) continue;
            sources.Add(source.InstanceId);
            pendingReleaseDrawName = effect.DisplayName;
        }
        if (sources.Count > 0) pendingReleaseDrawSourceGroups.Add(new ReleaseDrawEligibility { SourceIds = sources.ToArray() });
    }

    /// <summary>Discards the chosen hand occurrence for the mandatory Catch downside without playing or drawing it.</summary>
    public bool TryChooseRequiredDiscard(int handIndex, TechniqueDeckRuntime deck)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.DiscardTechnique || deck == null || !deck.TryDiscardTechnique(handIndex)) return false;
        CompleteChoice();
        return true;
    }

    /// <summary>Returns distinct legal Small creature selections without exposing the Hooked encounter.</summary>
    public CardInstance[] GetSelectedReleaseTargets(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SelectReleaseTargets) return Array.Empty<CardInstance>();
        Request request = requests[0];
        CardInstance source = GetChoiceSource(catches);
        CardEffectDefinition effect = source?.ActiveEffects[request.EffectIndex];
        return Array.FindAll(catches, c => c != null && c != source && c.Definition?.CardType == CardType.Creature
            && !c.PreventsReleaseWhileAttached && !request.SelectedTargetIds.Contains(c.InstanceId)
            && effect != null && EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, c));
    }

    /// <summary>Collects all choices before any removal, then completes the one-use shared removal window.</summary>
    public bool TryChooseSelectedRelease(int targetId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SelectReleaseTargets) return false;
        if (targetId >= 0)
        {
            if (!Array.Exists(GetSelectedReleaseTargets(chain.Catches), c => c.InstanceId == targetId)) return false;
            requests[0].SelectedTargetIds.Add(targetId);
        }
        if (targetId < 0 || requests[0].SelectedTargetIds.Count == 2 || GetSelectedReleaseTargets(chain.Catches).Length == 0)
            return ResolveSelectedReleases(chain, resolver);
        return true;
    }

    /// <summary>Removes the completed selection together and prioritizes all resulting Release reactions.</summary>
    private bool ResolveSelectedReleases(CatchChainRuntime chain, EffectResolver resolver)
    {
        int[] ids = requests[0].SelectedTargetIds.ToArray();
        if (ids.Length > 0 && !chain.TryReleaseTogether(ids, resolver)) return false;
        CompleteChoice();
        FinishReleaseResolution();
        return true;
    }

    /// <summary>Returns ordinary attached creatures of the required role, excluding the activating Octopus.</summary>
    public CardInstance[] GetModeSwapTargets(CatchChainRuntime chain, bool bait)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SwapCreatureModes) return Array.Empty<CardInstance>();
        return Array.FindAll(chain.Catches, c => c != null && c.InstanceId != requests[0].SourceInstanceId
            && c.IsBait == bait && chain.CanChangeCreatureMode(c));
    }

    /// <summary>Chooses a Catch without changing any role, or declines the entire activation.</summary>
    public bool TryChooseModeSwapCatch(int catchId, CatchChainRuntime chain)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SwapCreatureModes) return false;
        if (catchId < 0)
        {
            CompleteChoice();
            return true;
        }
        if (!Array.Exists(GetModeSwapTargets(chain, false), c => c.InstanceId == catchId)) return false;
        requests[0].SelectedCatchId = catchId;
        return true;
    }

    /// <summary>Swaps both validated roles together, then activates the new Catch before the surviving new Bait.</summary>
    public bool TryChooseModeSwapBait(int baitId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.SwapCreatureModes
            || !Array.Exists(GetModeSwapTargets(chain, true), c => c.InstanceId == baitId)) return false;
        int catchId = requests[0].SelectedCatchId;
        if (!Array.Exists(GetModeSwapTargets(chain, false), c => c.InstanceId == catchId)
            || !chain.TrySwapCreatureModes(catchId, baitId, resolver)) return false;
        CompleteChoice();
        requests.InsertRange(0, new[]
        {
            new Request { Kind = CreatureEffectChoiceKind.ActivateChangedRole, SourceInstanceId = baitId, ExpectedRole = CatchAttachmentRole.Catch },
            new Request { Kind = CreatureEffectChoiceKind.ActivateChangedRole, SourceInstanceId = catchId, ExpectedRole = CatchAttachmentRole.Bait }
        });
        return true;
    }

    /// <summary>Captures attached Armored eligibility at play time before any Technique targets are changed or released.</summary>
    public void BeginTechniquePlay(CardDefinition technique, int discardOccurrenceId, CardInstance[] catches)
    {
        resolvingTechnique = technique;
        resolvingDiscardOccurrenceId = discardOccurrenceId;
        eligibleAttachedIdsAtTechniquePlay = Array.ConvertAll(Array.FindAll(catches, c => c?.Definition?.CardType == CardType.Creature
            && pendingSelectedTechniqueRecoveries.Count > 0
            && EffectResolver.RequiredTagsMatchInstance(pendingSelectedTechniqueRecoveries[0].RequiredTags, c)), c => c.InstanceId);
        pendingPlayedTechniqueRecovery = null;
    }

    /// <summary>Consumes only the oldest pending recovery when this play selects an initially Armored attached creature.</summary>
    public void RecordSelectedTechniqueTarget(CardInstance target)
    {
        if (resolvingTechnique == null || pendingPlayedTechniqueRecovery != null || target == null
            || pendingSelectedTechniqueRecoveries.Count == 0 || Array.IndexOf(eligibleAttachedIdsAtTechniquePlay, target.InstanceId) < 0) return;
        pendingPlayedTechniqueRecovery = new Request
        {
            Kind = CreatureEffectChoiceKind.RecoverPlayedTechnique, AbilityName = pendingSelectedTechniqueRecoveries[0].DisplayName,
            PlayedTechnique = resolvingTechnique, PlayedDiscardOccurrenceId = resolvingDiscardOccurrenceId, MaxDiscards = 1
        };
        pendingSelectedTechniqueRecoveries.RemoveAt(0);
    }

    /// <summary>Waits until every Technique-triggered choice finishes before attempting the exact played-card recovery.</summary>
    public void FinishTechniquePlay()
    {
        if (pendingPlayedTechniqueRecovery != null) requests.Add(pendingPlayedTechniqueRecovery);
        pendingPlayedTechniqueRecovery = null;
        resolvingTechnique = null;
        eligibleAttachedIdsAtTechniquePlay = Array.Empty<int>();
    }

    /// <summary>Discards at most one hand technique for room, or declines this already-consumed exact recovery.</summary>
    public bool TryChoosePlayedTechniqueRecovery(int handIndex, TechniqueDeckRuntime deck)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.RecoverPlayedTechnique || deck == null) return false;
        if (handIndex < 0)
        {
            CompleteChoice();
            return true;
        }
        Request request = requests[0];
        if (request.DiscardCount != 0 || !deck.TryDiscardTechnique(handIndex)) return false;
        request.DiscardCount++;
        return true;
    }

    /// <summary>Applies one lasting bonus to every original surviving qualifying Catch after on-attach effects finish.</summary>
    private static void ApplyAttachmentValue(Request request, CatchChainRuntime chain, EffectResolver resolver)
    {
        CardInstance attached = Array.Find(chain.Catches, c => c?.InstanceId == request.AttachedInstanceId);
        if (attached == null || attached.IsBait || attached.Definition?.CardType != CardType.Creature) return;
        foreach (CardInstance source in chain.Catches)
        {
            if (source == null || source.IsBait || Array.IndexOf(request.EligibleSourceIds, source.InstanceId) < 0) continue;
            CardEffectDefinition effect = Array.Find(source.ActiveEffects, e => e?.EffectType == CardEffectType.GainSelfValueAfterCatch
                && e.Trigger == CardEffectTrigger.AfterAnotherCatchAttached
                && EffectResolver.RequiredTagsMatchInstance(e.RequiredTags, attached));
            if (effect != null) source.AddPermanentModifiers(0, effect.Amount, effect, source.Definition);
        }
        chain.Recalculate(resolver);
    }

    /// <summary>Returns original surviving Sardine Baits that can legally activate this shared Predator opportunity.</summary>
    public CardInstance[] GetFeedingFrenzySources(CardInstance[] catches)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.FeedingFrenzy) return Array.Empty<CardInstance>();
        Request request = requests[0];
        CardInstance attached = Array.Find(catches, c => c?.InstanceId == request.AttachedInstanceId);
        if (attached == null || attached.IsBait) return Array.Empty<CardInstance>();
        return Array.FindAll(catches, c => c != null && c.IsBait && !c.PreventsReleaseWhileAttached
            && Array.IndexOf(request.EligibleSourceIds, c.InstanceId) >= 0
            && Array.Exists(c.ActiveEffects, e => e?.EffectType == CardEffectType.ReleaseSelfToDrawTechniques
                && e.Trigger == CardEffectTrigger.AfterAnotherCatchAttached
                && EffectResolver.RequiredTagsMatchInstance(e.RequiredTags, attached)));
    }

    /// <summary>Releases one selected Bait, then prioritizes its Release reactions before the surviving draw request.</summary>
    public bool TryActivateFeedingFrenzy(int sourceId, CatchChainRuntime chain, EffectResolver resolver)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.FeedingFrenzy) return false;
        if (sourceId < 0)
        {
            CompleteChoice();
            return true;
        }
        CardInstance source = Array.Find(GetFeedingFrenzySources(chain.Catches), c => c.InstanceId == sourceId);
        if (source == null) return false;
        CardEffectDefinition effect = Array.Find(source.ActiveEffects, e => e?.EffectType == CardEffectType.ReleaseSelfToDrawTechniques);
        Request draw = new Request
        {
            Kind = CreatureEffectChoiceKind.DrawTechniques, SourceInstanceId = source.InstanceId,
            AbilityName = effect.DisplayName, DrawCount = Math.Max(0, effect.Amount)
        };
        if (!chain.TryRelease(Array.IndexOf(chain.Catches, source), resolver, out _, out _, out _, CatchRemovalReason.Creature))
            return false;
        CompleteChoice();
        requests.Insert(0, draw);
        FinishReleaseResolution();
        return true;
    }

    /// <summary>Optionally discards an exact hand occurrence before drawing only the activated reward that fits four slots.</summary>
    public bool TryChooseFeedingDraw(int discardHandIndex, TechniqueDeckRuntime deck, System.Random random)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.DrawTechniques || deck == null) return false;
        if (discardHandIndex >= 0)
        {
            Request request = requests[0];
            if (request.MaxDiscards >= 0 && request.DiscardCount >= request.MaxDiscards
                || !deck.TryDiscardTechnique(discardHandIndex)) return false;
            request.DiscardCount++;
            return true;
        }
        deck.DrawTechniques(requests[0].DrawCount, 4, random);
        CompleteChoice();
        return true;
    }

    /// <summary>Chooses a hand occurrence without moving it, or declines the entire immediate exchange.</summary>
    public bool TryChooseExchangeHand(int handIndex, TechniqueDeckRuntime deck)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ExchangeTechniques || deck == null) return false;
        if (handIndex < 0)
        {
            CompleteChoice();
            return true;
        }
        if (!deck.TryGetHandCard(handIndex, out _, out _)) return false;
        requests[0].SelectedHandIndex = handIndex;
        return true;
    }

    /// <summary>Chooses an already-discarded occurrence and atomically moves both selections without a draw.</summary>
    public bool TryChooseExchangeDiscard(int discardIndex, TechniqueDeckRuntime deck)
    {
        if (ChoiceKind != CreatureEffectChoiceKind.ExchangeTechniques || deck == null
            || !deck.TryExchangeTechniques(requests[0].SelectedHandIndex, discardIndex)) return false;
        CompleteChoice();
        return true;
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
        RecordReleaseDraw(released, reason, survivors ?? Array.Empty<CardInstance>());
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
        if (pendingReleaseDrawSourceGroups.Count > 0)
        {
            completed.Add(new Request { Kind = CreatureEffectChoiceKind.ResolveReleaseDraws, AbilityName = pendingReleaseDrawName,
                ReleaseDrawSourceGroups = new List<ReleaseDrawEligibility>(pendingReleaseDrawSourceGroups) });
            pendingReleaseDrawSourceGroups.Clear();
        }
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
