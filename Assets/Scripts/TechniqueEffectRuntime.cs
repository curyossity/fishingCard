using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TechniqueEffectRuntime
{
    [SerializeField] private CardEffectDefinition[] pendingDescendEffects = Array.Empty<CardEffectDefinition>();
    [SerializeField] private CardEffectDefinition[] pendingEncounterEffects = Array.Empty<CardEffectDefinition>();
    [SerializeField] private bool revealsCurrentEncounter;
    [SerializeField] private ApprovedTechniqueRuntime approved = new ApprovedTechniqueRuntime();
    public ApprovedTechniqueRuntime Approved => approved;

    public CardEffectDefinition[] PendingDescendEffects => pendingDescendEffects;
    public CardEffectDefinition[] PendingEncounterEffects => pendingEncounterEffects;
    public bool RevealsCurrentEncounter => revealsCurrentEncounter;

    /// <summary>Removes one queued negative effect occurrence without changing shared authored card data.</summary>
    public bool TryRemoveNegativeEffect(bool encounterEffect, int index)
    {
        CardEffectDefinition[] effects = encounterEffect ? pendingEncounterEffects : pendingDescendEffects;
        if (index < 0 || index >= effects.Length || effects[index]?.EffectTone != CardEffectTone.Negative)
        {
            return false;
        }

        List<CardEffectDefinition> remaining = new List<CardEffectDefinition>(effects);
        remaining.RemoveAt(index);
        if (encounterEffect)
        {
            pendingEncounterEffects = remaining.ToArray();
        }
        else
        {
            pendingDescendEffects = remaining.ToArray();
        }

        return true;
    }

    /// <summary>
    /// Reports whether at least one effect on a Technique card has a valid current or pending target.
    /// </summary>
    public bool CanUseCard(
        CardDefinition techniqueCard,
        EncounterRuntime encounterRuntime,
        CatchChainRuntime catchChainRuntime,
        bool encounterInformationHidden,
        CardDefinition[] encounterPool,
        string biomeId,
        int depth,
        out string restrictionReason,
        TechniqueDeckRuntime deck = null)
    {
        restrictionReason = string.Empty;

        if (encounterRuntime == null || !encounterRuntime.HasTechniqueReactionWindow)
        {
            restrictionReason = "No open Technique window";
            return false;
        }

        if (techniqueCard == null || techniqueCard.IsBackupTechnique || techniqueCard.Effects == null || techniqueCard.Effects.Length == 0)
        {
            restrictionReason = "No usable effect";
            return false;
        }

        foreach (CardEffectDefinition effect in techniqueCard.Effects)
        {
            if (TechniqueTargetPlan.RequiresSelection(effect) && catchChainRuntime.GetTechniqueTargets(effect).Length == 0)
            { restrictionReason = "No valid required attachment target"; return false; }
        }
        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
            if (ApprovedTechniqueRuntime.Handles(techniqueCard.Effects[i]))
            {
                if (approved.CanApply(techniqueCard.Effects[i], encounterRuntime, catchChainRuntime, deck, encounterInformationHidden)) return true;
                continue;
            }
            if (CanApplyEffect(
                techniqueCard.Effects[i],
                encounterRuntime,
                catchChainRuntime,
                encounterInformationHidden,
                encounterPool,
                biomeId,
                depth))
            {
                return true;
            }
        }

        restrictionReason = "No valid target";
        return false;
    }

    /// <summary>
    /// Executes immediate effects and stores delayed effects for the next relevant action.
    /// </summary>
    public bool ApplyCard(
        CardDefinition techniqueCard,
        EncounterRuntime encounterRuntime,
        CatchChainRuntime catchChainRuntime,
        EffectResolver effectResolver,
        CardDefinition[] encounterPool,
        EncounterChainDefinition[] encounterChains,
        string biomeId,
        int depth,
        System.Random random,
        bool encounterInformationHidden,
        out string resultSummary,
        TechniqueTargetPlan targets = null,
        TechniqueDeckRuntime deck = null,
        CreatureEffectRuntime creatures = null)
    {
        resultSummary = string.Empty;
        if (techniqueCard?.Effects == null || techniqueCard.IsBackupTechnique || encounterRuntime == null
            || !encounterRuntime.HasTechniqueReactionWindow) return false;
        if (Array.Exists(techniqueCard.Effects, TechniqueTargetPlan.NeedsPlayerTargets)
            && (targets == null || !targets.Validate(techniqueCard, catchChainRuntime, out resultSummary, encounterRuntime, deck))) return false;
        if (targets != null && !targets.Validate(techniqueCard, catchChainRuntime, out resultSummary, encounterRuntime, deck)) return false;
        List<string> results = new List<string>();

        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = techniqueCard.Effects[i];

            if (ApprovedTechniqueRuntime.Handles(effect))
            {
                if (approved.Apply(effect, i, targets, encounterRuntime, catchChainRuntime, effectResolver,
                    creatures, deck, random, encounterInformationHidden, out bool revealCurrent))
                {
                    if (revealCurrent) revealsCurrentEncounter = true;
                    results.Add(effect.ReminderText ?? effect.EffectType.ToString());
                }
                continue;
            }

            if (!CanApplyEffect(
                effect,
                encounterRuntime,
                catchChainRuntime,
                encounterInformationHidden,
                encounterPool,
                biomeId,
                depth))
            {
                continue;
            }

            if (ApplyImmediateEncounterEffect(
                effect,
                encounterRuntime,
                catchChainRuntime,
                effectResolver,
                encounterPool,
                encounterChains,
                biomeId,
                depth,
                random,
                results))
            {
                continue;
            }

            if (TechniqueTargetPlan.RequiresSelection(effect))
            {
                if (catchChainRuntime.TryApplyTechniqueEffectToTargets(effect, targets.TargetsFor(i), effectResolver, out string chosenResult))
                    results.Add(chosenResult);
                continue;
            }

            if (catchChainRuntime.TryApplyTechniqueEffect(effect, effectResolver, out string catchResult))
            {
                results.Add(catchResult);
                continue;
            }

            QueueDelayedEffect(effect);
            results.Add(string.IsNullOrWhiteSpace(effect.ReminderText) ? effect.EffectType.ToString() : effect.ReminderText);
        }

        resultSummary = string.Join("; ", results);
        return results.Count > 0;
    }

    /// <summary>
    /// Applies and consumes all effects waiting for the next Descend action.
    /// </summary>
    public TechniqueDescendResolution ResolveNextDescend(
        CardInstance committedCatch,
        CatchChainRuntime catchChainRuntime,
        EffectResolver effectResolver,
        int baseLineCapacity)
    {
        int weightChange = 0;
        int valueChange = 0;
        int additionalDepth = 0;
        int capacityBonus = 0;
        int overloadReward = 0;

        for (int i = 0; i < pendingDescendEffects.Length; i++)
        {
            CardEffectDefinition effect = pendingDescendEffects[i];

            if (effect == null)
            {
                continue;
            }

            int effectWeightChange = 0;
            int effectValueChange = 0;
            switch (effect.EffectType)
            {
                case CardEffectType.ModifyNextDescend:
                    weightChange += effect.Amount;
                    effectWeightChange = effect.Amount;
                    break;
                case CardEffectType.ModifyDescendDistance:
                    additionalDepth += effect.Amount;
                    break;
                case CardEffectType.ModifyTemporaryCapacity:
                    capacityBonus += effect.Amount;
                    break;
                case CardEffectType.RiskForReward:
                    weightChange += Math.Abs(effect.Amount);
                    valueChange += Math.Abs(effect.Amount) * 2;
                    effectWeightChange = Math.Abs(effect.Amount);
                    effectValueChange = Math.Abs(effect.Amount) * 2;
                    break;
                case CardEffectType.RewardOverloadedDescend:
                    overloadReward += effect.Amount;
                    break;
            }

            if (committedCatch != null && (effectWeightChange != 0 || effectValueChange != 0))
            {
                committedCatch.AddPermanentModifiers(effectWeightChange, effectValueChange, effect, null);
            }
        }

        if (committedCatch != null && (weightChange != 0 || valueChange != 0))
        {
            catchChainRuntime.Recalculate(effectResolver);
        }

        int effectiveCapacity = Math.Max(0, baseLineCapacity + capacityBonus);
        bool overloaded = catchChainRuntime.CurrentLineLoad > effectiveCapacity;

        if (committedCatch != null && overloaded)
        {
            foreach (CardEffectDefinition effect in pendingDescendEffects)
            {
                if (effect?.EffectType == CardEffectType.RewardOverloadedDescend && effect.Amount != 0)
                {
                    committedCatch.AddPermanentModifiers(0, effect.Amount, effect, null);
                }
            }

            catchChainRuntime.Recalculate(effectResolver);
        }

        pendingDescendEffects = Array.Empty<CardEffectDefinition>();
        return new TechniqueDescendResolution(additionalDepth, capacityBonus, weightChange, valueChange, overloaded ? overloadReward : 0);
    }

    /// <summary>
    /// Returns the depth offset used only while choosing the next encounter pool.
    /// </summary>
    public int GetNextEncounterDepthOffset()
    {
        int depthOffset = 0;

        for (int i = 0; i < pendingEncounterEffects.Length; i++)
        {
            CardEffectDefinition effect = pendingEncounterEffects[i];

            if (effect != null && effect.EffectType == CardEffectType.ModifyNextEncounterDepth)
            {
                depthOffset += effect.Amount;
            }
        }

        return depthOffset;
    }

    /// <summary>
    /// Clears one-use encounter modifiers after an encounter selection attempt.
    /// </summary>
    public void CompleteEncounterReveal()
    {
        pendingEncounterEffects = Array.Empty<CardEffectDefinition>();
        revealsCurrentEncounter = false;
    }

    /// <summary>
    /// Clears all delayed Technique effects and presentation overrides.
    /// </summary>
    public void Reset()
    {
        approved.Reset();
        pendingDescendEffects = Array.Empty<CardEffectDefinition>();
        pendingEncounterEffects = Array.Empty<CardEffectDefinition>();
        revealsCurrentEncounter = false;
    }

    /// <summary>
    /// Checks whether one effect has a valid target in the current reaction window.
    /// </summary>
    private bool CanApplyEffect(
        CardEffectDefinition effect,
        EncounterRuntime encounterRuntime,
        CatchChainRuntime catchChainRuntime,
        bool encounterInformationHidden,
        CardDefinition[] encounterPool,
        string biomeId,
        int depth)
    {
        CardDefinition hookedEncounter = encounterRuntime.HookedEncounter;

        if (effect == null || !encounterRuntime.HasTechniqueReactionWindow) return false;
        CardDefinition visibleTarget = encounterRuntime.CurrentEventOccurrence?.State == EventOccurrenceState.Unresolved
            ? encounterRuntime.CurrentEncounter : hookedEncounter;
        if (effect.Target == CardEffectTarget.HookedEncounter && (hookedEncounter == null
            || !EffectResolver.RequiredTagsMatch(effect.RequiredTags, hookedEncounter))) return false;
        if (effect.Target == CardEffectTarget.CurrentEncounter && (visibleTarget == null
            || !EffectResolver.RequiredTagsMatch(effect.RequiredTags, visibleTarget))) return false;
        if (effect.EffectType == CardEffectType.ReplaceEncounter)
        {
            CardDefinition target = effect.Target == CardEffectTarget.CurrentEncounter ? visibleTarget : hookedEncounter;
            int selectionDepth = Math.Max(0, depth + GetNextEncounterDepthOffset());
            return target != null && EffectResolver.RequiredTagsMatch(effect.RequiredTags, target)
                && encounterRuntime.HasAlternativeEncounter(encounterPool, biomeId, selectionDepth, target);
        }

        if (effect.EffectType == CardEffectType.AvoidEncounter)
        {
            return hookedEncounter != null && EffectResolver.RequiredTagsMatch(effect.RequiredTags, hookedEncounter);
        }

        if (effect.EffectType == CardEffectType.RevealEncounterInformation)
        {
            return visibleTarget != null && encounterInformationHidden
                && EffectResolver.RequiredTagsMatch(effect.RequiredTags, visibleTarget);
        }

        if (catchChainRuntime.CanApplyTechniqueEffect(effect))
        {
            return true;
        }

        return IsDelayedEffect(effect);
    }

    /// <summary>
    /// Executes encounter effects that resolve as soon as the Technique card is played.
    /// </summary>
    private bool ApplyImmediateEncounterEffect(
        CardEffectDefinition effect,
        EncounterRuntime encounterRuntime,
        CatchChainRuntime catchChainRuntime,
        EffectResolver effectResolver,
        CardDefinition[] encounterPool,
        EncounterChainDefinition[] encounterChains,
        string biomeId,
        int depth,
        System.Random random,
        List<string> results)
    {
        if (effect.EffectType == CardEffectType.AvoidEncounter)
        {
            string avoidedName = encounterRuntime.HookedEncounter.DisplayName;
            encounterRuntime.SetCurrentEncounter(null);
            encounterRuntime.CancelActiveEncounterChain();
            results.Add($"Avoided {avoidedName}");
            return true;
        }

        if (effect.EffectType == CardEffectType.ReplaceEncounter)
        {
            CardDefinition replacedEncounter = encounterRuntime.HookedEncounter ?? encounterRuntime.CurrentEncounter;
            int selectionDepth = Math.Max(0, depth + GetNextEncounterDepthOffset());
            bool replaced = encounterRuntime.TryReplaceEncounter(
                encounterRuntime.EncounterRevision,
                replacedEncounter,
                encounterPool,
                biomeId,
                selectionDepth,
                random,
                catchChainRuntime.ActiveEffectRecords,
                effectResolver,
                pendingEncounterEffects,
                encounterChains);

            if (replaced)
            {
                results.Add($"Replaced {replacedEncounter.DisplayName} with {encounterRuntime.CurrentEncounter?.DisplayName ?? "a cancelled Hazard"}");
                CompleteEncounterReveal();
            }

            return replaced;
        }

        if (effect.EffectType == CardEffectType.RevealEncounterInformation)
        {
            revealsCurrentEncounter = true;
            results.Add($"Revealed {encounterRuntime.CurrentEncounter.DisplayName}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stores an effect until its next Descend or encounter-selection window.
    /// </summary>
    private void QueueDelayedEffect(CardEffectDefinition effect)
    {
        if (effect.EffectType == CardEffectType.ModifyFutureEncounterProperties
            || effect.EffectType == CardEffectType.ModifyNextEncounterDepth)
        {
            pendingEncounterEffects = AppendEffect(pendingEncounterEffects, effect);
            return;
        }

        pendingDescendEffects = AppendEffect(pendingDescendEffects, effect);
    }

    /// <summary>
    /// Reports whether an effect waits for Descend or the next encounter selection.
    /// </summary>
    private static bool IsDelayedEffect(CardEffectDefinition effect)
    {
        return effect.EffectType == CardEffectType.ModifyFutureEncounterProperties
            || effect.EffectType == CardEffectType.ModifyNextDescend
            || effect.EffectType == CardEffectType.ModifyDescendDistance
            || effect.EffectType == CardEffectType.ModifyNextEncounterDepth
            || effect.EffectType == CardEffectType.ModifyTemporaryCapacity
            || effect.EffectType == CardEffectType.RiskForReward
            || effect.EffectType == CardEffectType.RewardOverloadedDescend;
    }

    /// <summary>
    /// Returns a new effect array with one delayed effect appended.
    /// </summary>
    private static CardEffectDefinition[] AppendEffect(CardEffectDefinition[] source, CardEffectDefinition effect)
    {
        CardEffectDefinition[] safeSource = source ?? Array.Empty<CardEffectDefinition>();
        CardEffectDefinition[] result = new CardEffectDefinition[safeSource.Length + 1];

        for (int i = 0; i < safeSource.Length; i++)
        {
            result[i] = safeSource[i];
        }

        result[result.Length - 1] = effect;
        return result;
    }
}

public sealed class TechniqueDescendResolution
{
    public int AdditionalDepth { get; }
    public int CapacityBonus { get; }
    public int CommittedWeightChange { get; }
    public int CommittedValueChange { get; }
    public int OverloadValueReward { get; }

    /// <summary>
    /// Captures the one-use Technique modifiers applied during a Descend action.
    /// </summary>
    public TechniqueDescendResolution(
        int additionalDepth,
        int capacityBonus,
        int committedWeightChange,
        int committedValueChange,
        int overloadValueReward)
    {
        AdditionalDepth = additionalDepth;
        CapacityBonus = capacityBonus;
        CommittedWeightChange = committedWeightChange;
        CommittedValueChange = committedValueChange;
        OverloadValueReward = overloadValueReward;
    }
}
