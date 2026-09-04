using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CatchChainRuntime
{
    [SerializeField] private CardInstance[] catches = Array.Empty<CardInstance>();
    [SerializeField] private ActiveCatchEffectRecord[] activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
    [SerializeField] private CardInstance[] releasedCatches = Array.Empty<CardInstance>();
    [SerializeField] private CardInstance[] lostCatches = Array.Empty<CardInstance>();
    [SerializeField] private int nextInstanceId = 1;

    public CardInstance[] Catches => catches;
    public ActiveCatchEffectRecord[] ActiveEffectRecords => activeEffectRecords;
    public CardInstance[] ReleasedCatches => releasedCatches;
    public CardInstance[] LostCatches => lostCatches;

    /// <summary>
    /// Calculates the Line Load contributed by all catches still attached to the line.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < catches.Length; i++)
            {
                if (catches[i] != null)
                {
                    load += catches[i].CurrentWeight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Adds a committed encounter and tracks its catch-related effects.
    /// </summary>
    public CardInstance Add(CardDefinition caughtCard, EffectResolver effectResolver)
    {
        if (caughtCard == null)
        {
            return null;
        }

        CardInstance caughtInstance = new CardInstance(nextInstanceId, caughtCard);
        nextInstanceId++;
        catches = AppendCatch(catches, caughtInstance);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return caughtInstance;
    }

    /// <summary>
    /// Reports whether an immediate Technique effect has at least one valid Catch Chain target.
    /// </summary>
    public bool CanApplyTechniqueEffect(CardEffectDefinition effect)
    {
        if (effect == null)
        {
            return false;
        }

        if (effect.Target != CardEffectTarget.CatchChain
            && effect.Target != CardEffectTarget.SpecificCaughtCard)
        {
            return false;
        }

        if (effect.EffectType == CardEffectType.ModifyCaughtCard)
        {
            return CountMatchingCatches(effect.RequiredTags) >= 2;
        }

        if (effect.EffectType != CardEffectType.AddLineLoadModifier
            && effect.EffectType != CardEffectType.RemoveLineLoadModifier
            && effect.EffectType != CardEffectType.ModifyCatchValue
            && effect.EffectType != CardEffectType.ReleaseCatch)
        {
            return false;
        }

        return FindTechniqueTargetIndex(effect) >= 0;
    }

    /// <summary>
    /// Applies an immediate Technique effect to automatically selected Catch Chain targets.
    /// </summary>
    public bool TryApplyTechniqueEffect(
        CardEffectDefinition effect,
        EffectResolver effectResolver,
        out string resultSummary)
    {
        resultSummary = string.Empty;

        if (!CanApplyTechniqueEffect(effect))
        {
            return false;
        }

        if (effect.EffectType == CardEffectType.ReleaseCatch)
        {
            return ReleaseTechniqueTargets(effect, effectResolver, out resultSummary);
        }

        if (effect.EffectType == CardEffectType.ModifyCaughtCard)
        {
            int matchingCount = CountMatchingCatches(effect.RequiredTags);
            int valueChange = effect.Amount * (matchingCount - 1);

            for (int i = 0; i < catches.Length; i++)
            {
                if (EffectResolver.RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition))
                {
                    catches[i].AddPermanentModifiers(0, valueChange);
                }
            }

            effectResolver.ResolveCatchChain(catches, activeEffectRecords);
            resultSummary = $"{matchingCount} interacting catches gained {valueChange} Value each";
            return true;
        }

        int targetIndex = FindTechniqueTargetIndex(effect);
        CardInstance target = catches[targetIndex];
        int weightChange = 0;
        int valueChangeForTarget = 0;

        if (effect.EffectType == CardEffectType.ModifyCatchValue)
        {
            valueChangeForTarget = effect.Amount;
        }
        else if (effect.EffectType == CardEffectType.RemoveLineLoadModifier)
        {
            weightChange = -Math.Abs(effect.Amount);
        }
        else
        {
            weightChange = effect.Amount;
        }

        target.AddPermanentModifiers(weightChange, valueChangeForTarget);
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        resultSummary = $"{target.Definition.DisplayName} changed by Weight {weightChange}, Value {valueChangeForTarget}";
        return true;
    }

    /// <summary>
    /// Recalculates catches after lasting Technique modifiers change instance base stats.
    /// </summary>
    public void Recalculate(EffectResolver effectResolver)
    {
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
    }

    /// <summary>
    /// Removes one catch, records why it left the line, and reports its previous Line Load.
    /// </summary>
    public bool TryRelease(
        int catchIndex,
        EffectResolver effectResolver,
        out CardInstance releasedCatch,
        out int previousLineLoad,
        out string validationMessage,
        CatchRemovalReason removalReason = CatchRemovalReason.PlayerChoice)
    {
        releasedCatch = null;
        previousLineLoad = CurrentLineLoad;
        validationMessage = string.Empty;

        if (catchIndex < 0 || catchIndex >= catches.Length)
        {
            validationMessage = $"Catch Chain index is out of range: {catchIndex}.";
            return false;
        }

        releasedCatch = catches[catchIndex];

        if (releasedCatch == null)
        {
            validationMessage = $"Catch Chain slot {catchIndex} is empty.";
            return false;
        }

        RecordRemoval(releasedCatch, removalReason);
        catches = RemoveCatchAt(catches, catchIndex);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>
    /// Returns a separate snapshot of all catches currently attached to the line.
    /// </summary>
    public CardInstance[] CreateSnapshot()
    {
        CardInstance[] result = new CardInstance[catches.Length];

        for (int i = 0; i < catches.Length; i++)
        {
            result[i] = catches[i]?.CreateSnapshot();
        }

        return result;
    }

    /// <summary>
    /// Clears all Catch Chain cards and active effect records.
    /// </summary>
    public void Reset()
    {
        catches = Array.Empty<CardInstance>();
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
        releasedCatches = Array.Empty<CardInstance>();
        lostCatches = Array.Empty<CardInstance>();
        nextInstanceId = 1;
    }

    /// <summary>
    /// Tracks effects that become relevant when a card enters the Catch Chain.
    /// </summary>
    private void AddCatchEffects(CardInstance caughtInstance, int catchIndex)
    {
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttached, catchIndex);
    }

    /// <summary>
    /// Rebuilds active effect records from catches that remain attached.
    /// </summary>
    private void RebuildActiveEffectRecords()
    {
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();

        // Rebuilding also preserves the correct number of records for repeated card definitions.
        for (int i = 0; i < catches.Length; i++)
        {
            AddCatchEffects(catches[i], i);
        }
    }

    /// <summary>
    /// Adds catch effects matching one active trigger.
    /// </summary>
    private void AddActiveEffects(CardInstance sourceInstance, CardEffectTrigger trigger, int catchIndex)
    {
        CardDefinition sourceCard = sourceInstance?.Definition;

        if (sourceCard == null || sourceCard.Effects == null)
        {
            return;
        }

        List<ActiveCatchEffectRecord> records = new List<ActiveCatchEffectRecord>(activeEffectRecords);

        for (int i = 0; i < sourceCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = sourceCard.Effects[i];

            if (effect == null || effect.Trigger != trigger)
            {
                continue;
            }

            records.Add(new ActiveCatchEffectRecord(sourceInstance, effect, trigger, catchIndex));
        }

        activeEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Removes the requested number of automatically selected catches for a Technique effect.
    /// </summary>
    private bool ReleaseTechniqueTargets(
        CardEffectDefinition effect,
        EffectResolver effectResolver,
        out string resultSummary)
    {
        List<string> releasedNames = new List<string>();
        int releaseCount = Math.Max(1, Math.Abs(effect.Amount));

        for (int i = 0; i < releaseCount; i++)
        {
            int targetIndex = FindTechniqueTargetIndex(effect);

            if (targetIndex < 0)
            {
                break;
            }

            CardInstance releasedCatch = catches[targetIndex];
            releasedNames.Add(releasedCatch.Definition.DisplayName);
            RecordRemoval(releasedCatch, CatchRemovalReason.Technique);
            catches = RemoveCatchAt(catches, targetIndex);
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        resultSummary = $"Released {string.Join(", ", releasedNames)}";
        return releasedNames.Count > 0;
    }

    /// <summary>
    /// Adds a removed catch snapshot to either the released or involuntarily lost history.
    /// </summary>
    private void RecordRemoval(CardInstance removedCatch, CatchRemovalReason removalReason)
    {
        if (removedCatch == null)
        {
            return;
        }

        CardInstance snapshot = removedCatch.CreateSnapshot();

        if (removalReason == CatchRemovalReason.LineStrain)
        {
            lostCatches = AppendCatch(lostCatches, snapshot);
            return;
        }

        releasedCatches = AppendCatch(releasedCatches, snapshot);
    }

    /// <summary>
    /// Selects one Catch Chain target using the effect's automatic target direction.
    /// </summary>
    private int FindTechniqueTargetIndex(CardEffectDefinition effect)
    {
        bool searchFromStart = effect.CaughtCardTargetMode == CaughtCardTargetMode.FirstMatching
            || effect.CaughtCardTargetMode == CaughtCardTargetMode.NextMatching;

        if (searchFromStart)
        {
            for (int i = 0; i < catches.Length; i++)
            {
                if (EffectResolver.RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition))
                {
                    return i;
                }
            }

            return -1;
        }

        for (int i = catches.Length - 1; i >= 0; i--)
        {
            if (EffectResolver.RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Counts attached catches matching an effect's required tags.
    /// </summary>
    private int CountMatchingCatches(string[] requiredTags)
    {
        int matchingCount = 0;

        for (int i = 0; i < catches.Length; i++)
        {
            if (EffectResolver.RequiredTagsMatch(requiredTags, catches[i]?.Definition))
            {
                matchingCount++;
            }
        }

        return matchingCount;
    }

    /// <summary>
    /// Returns a new catch-instance array with one catch appended.
    /// </summary>
    private static CardInstance[] AppendCatch(CardInstance[] source, CardInstance caughtInstance)
    {
        CardInstance[] result = new CardInstance[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = caughtInstance;
        return result;
    }

    /// <summary>
    /// Returns a new catch-instance array without the catch at the requested index.
    /// </summary>
    private static CardInstance[] RemoveCatchAt(CardInstance[] source, int removeIndex)
    {
        CardInstance[] result = new CardInstance[source.Length - 1];
        int resultIndex = 0;

        for (int i = 0; i < source.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            result[resultIndex] = source[i];
            resultIndex++;
        }

        return result;
    }
}

public enum CatchRemovalReason
{
    PlayerChoice,
    Technique,
    LineStrain
}
