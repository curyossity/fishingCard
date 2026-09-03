using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TechniqueEffectRuntime
{
    [SerializeField] private CardEffectDefinition[] pendingDescendEffects = Array.Empty<CardEffectDefinition>();
    [SerializeField] private CardEffectDefinition[] pendingEncounterEffects = Array.Empty<CardEffectDefinition>();
    [SerializeField] private bool revealsCurrentEncounter;

    public CardEffectDefinition[] PendingDescendEffects => pendingDescendEffects;
    public CardEffectDefinition[] PendingEncounterEffects => pendingEncounterEffects;
    public bool RevealsCurrentEncounter => revealsCurrentEncounter;

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
        out string restrictionReason)
    {
        restrictionReason = string.Empty;

        if (techniqueCard == null || techniqueCard.Effects == null || techniqueCard.Effects.Length == 0)
        {
            restrictionReason = "No usable effect";
            return false;
        }

        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
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
        out string resultSummary)
    {
        List<string> results = new List<string>();

        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = techniqueCard.Effects[i];

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

            switch (effect.EffectType)
            {
                case CardEffectType.ModifyNextDescend:
                    weightChange += effect.Amount;
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
                    break;
                case CardEffectType.RewardOverloadedDescend:
                    overloadReward += effect.Amount;
                    break;
            }
        }

        if (committedCatch != null && (weightChange != 0 || valueChange != 0))
        {
            committedCatch.AddPermanentModifiers(weightChange, valueChange);
            catchChainRuntime.Recalculate(effectResolver);
        }

        int effectiveCapacity = Math.Max(0, baseLineCapacity + capacityBonus);
        bool overloaded = catchChainRuntime.CurrentLineLoad > effectiveCapacity;

        if (committedCatch != null && overloaded && overloadReward != 0)
        {
            committedCatch.AddPermanentModifiers(0, overloadReward);
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

        if (effect == null || hookedEncounter == null)
        {
            return false;
        }

        if (effect.EffectType == CardEffectType.AvoidEncounter)
        {
            return EffectResolver.RequiredTagsMatch(effect.RequiredTags, hookedEncounter);
        }

        if (effect.EffectType == CardEffectType.ReplaceEncounter)
        {
            int selectionDepth = Math.Max(0, depth + GetNextEncounterDepthOffset());
            return EffectResolver.RequiredTagsMatch(effect.RequiredTags, hookedEncounter)
                && encounterRuntime.HasAlternativeEncounter(encounterPool, biomeId, selectionDepth, hookedEncounter);
        }

        if (effect.EffectType == CardEffectType.RevealEncounterInformation)
        {
            return encounterInformationHidden
                && EffectResolver.RequiredTagsMatch(effect.RequiredTags, hookedEncounter);
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
            CardDefinition replacedEncounter = encounterRuntime.HookedEncounter;
            int selectionDepth = Math.Max(0, depth + GetNextEncounterDepthOffset());
            encounterRuntime.CancelActiveEncounterChain();
            bool replaced = encounterRuntime.Reveal(
                encounterPool,
                biomeId,
                selectionDepth,
                random,
                catchChainRuntime.ActiveEffectRecords,
                effectResolver,
                pendingEncounterEffects,
                replacedEncounter,
                encounterChains);

            if (replaced)
            {
                results.Add($"Replaced {replacedEncounter.DisplayName} with {encounterRuntime.CurrentEncounter.DisplayName}");
                CompleteEncounterReveal();
            }

            return replaced;
        }

        if (effect.EffectType == CardEffectType.RevealEncounterInformation)
        {
            revealsCurrentEncounter = true;
            results.Add($"Revealed {encounterRuntime.HookedEncounter.DisplayName}");
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
