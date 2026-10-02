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
    [SerializeField] private string[] consumedNextCatchEffectKeys = Array.Empty<string>();
    [SerializeField] private string[] consumedNextEncounterEffectKeys = Array.Empty<string>();
    [SerializeField] private string[] invalidatedNoReleaseEffectKeys = Array.Empty<string>();
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
    public CardInstance Add(
        CardDefinition caughtCard,
        EffectResolver effectResolver,
        System.Random random = null)
    {
        if (caughtCard == null)
        {
            return null;
        }

        CardInstance caughtInstance = new CardInstance(nextInstanceId, caughtCard, random);
        nextInstanceId++;
        ApplyPendingNextCatchEffects(caughtInstance, true);
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

        if (removalReason != CatchRemovalReason.LineStrain
            && releasedCatch.Definition != null
            && releasedCatch.Definition.PreventsReleaseWhileCaught)
        {
            validationMessage = $"{releasedCatch.Definition.DisplayName} cannot be released once caught.";
            releasedCatch = null;
            return false;
        }

        ApplyAnotherCatchReleasedEffects(releasedCatch, removalReason);
        RecordRemoval(releasedCatch, removalReason);
        catches = RemoveCatchAt(catches, catchIndex);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>
    /// Randomly loses between one catch and half of the attached chain after an overload line break.
    /// </summary>
    public CardInstance[] LoseRandomCatchesAfterLineBreak(
        EffectResolver effectResolver,
        System.Random random)
    {
        if (catches.Length == 0 || random == null)
        {
            return Array.Empty<CardInstance>();
        }

        int maximumLossCount = Mathf.Max(1, catches.Length / 2);
        int lossCount = random.Next(1, maximumLossCount + 1);
        CardInstance[] losses = new CardInstance[lossCount];

        for (int i = 0; i < lossCount; i++)
        {
            int lossIndex = random.Next(catches.Length);
            CardInstance lostCatch = catches[lossIndex];
            losses[i] = lostCatch;
            RecordRemoval(lostCatch, CatchRemovalReason.LineStrain);
            catches = RemoveCatchAt(catches, lossIndex);
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return losses;
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
    /// Resolves a temporary catch copy against the current chain without mutating the active run.
    /// </summary>
    public CardInstance CreateResolvedCatchPreview(CardDefinition card, EffectResolver effectResolver)
    {
        if (card == null || effectResolver == null)
        {
            return null;
        }

        CardInstance[] previewCatches = CreateSnapshot();
        CardInstance previewCatch = new CardInstance(0, card);
        ApplyPendingNextCatchEffects(previewCatch, false);
        previewCatches = AppendCatch(previewCatches, previewCatch);

        List<ActiveCatchEffectRecord> previewEffects = new List<ActiveCatchEffectRecord>(activeEffectRecords);
        AppendPreviewEffects(previewEffects, previewCatch, previewCatches.Length - 1);
        effectResolver.ResolveCatchChain(previewCatches, previewEffects.ToArray());
        return previewCatch;
    }

    /// <summary>Consumes attached effects that last through exactly one encounter resolution.</summary>
    public void CompleteEncounter(EffectResolver effectResolver)
    {
        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            if (record?.Effect == null
                || record.SourceInstance == null
                || record.ActiveTrigger != CardEffectTrigger.UntilNextEncounterResolved)
            {
                continue;
            }

            int effectIndex = Array.IndexOf(record.SourceInstance.Definition.Effects, record.Effect);
            if (effectIndex >= 0)
            {
                consumedNextEncounterEffectKeys = AppendEffectKey(
                    consumedNextEncounterEffectKeys,
                    BuildEffectKey(record.SourceInstance, effectIndex));
            }
        }

        RebuildActiveEffectRecords();
        effectResolver?.ResolveCatchChain(catches, activeEffectRecords);
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
        consumedNextCatchEffectKeys = Array.Empty<string>();
        consumedNextEncounterEffectKeys = Array.Empty<string>();
        invalidatedNoReleaseEffectKeys = Array.Empty<string>();
        nextInstanceId = 1;
    }

    /// <summary>
    /// Tracks effects that become relevant when a card enters the Catch Chain.
    /// </summary>
    private void AddCatchEffects(CardInstance caughtInstance, int catchIndex)
    {
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttached, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenAnotherCatchReleased, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenNextMatchingCatchCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.UntilNextEncounterResolved, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased, catchIndex);
    }

    /// <summary>
    /// Adds the temporary catch's own catch-time and attached effects to a preview effect list.
    /// </summary>
    private static void AppendPreviewEffects(
        List<ActiveCatchEffectRecord> previewEffects,
        CardInstance previewCatch,
        int catchIndex)
    {
        CardDefinition card = previewCatch?.Definition;

        if (card == null || card.Effects == null)
        {
            return;
        }

        for (int i = 0; i < card.Effects.Length; i++)
        {
            CardEffectDefinition effect = card.Effects[i];

            if (effect == null
                || (effect.Trigger != CardEffectTrigger.WhenCaught
                    && effect.Trigger != CardEffectTrigger.WhileAttached
                    && effect.Trigger != CardEffectTrigger.WhenAnotherCatchReleased
                    && effect.Trigger != CardEffectTrigger.UntilNextEncounterResolved
                    && effect.Trigger != CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased))
            {
                continue;
            }

            previewEffects.Add(new ActiveCatchEffectRecord(previewCatch, effect, effect.Trigger, catchIndex));
        }
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

            if (trigger == CardEffectTrigger.WhenNextMatchingCatchCaught
                && IsNextCatchEffectConsumed(sourceInstance, i))
            {
                continue;
            }

            if (trigger == CardEffectTrigger.UntilNextEncounterResolved
                && IsEffectKeyTracked(consumedNextEncounterEffectKeys, sourceInstance, i))
            {
                continue;
            }

            if (trigger == CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased
                && IsEffectKeyTracked(invalidatedNoReleaseEffectKeys, sourceInstance, i))
            {
                continue;
            }

            records.Add(new ActiveCatchEffectRecord(sourceInstance, effect, trigger, catchIndex));
        }

        activeEffectRecords = records.ToArray();
    }

    /// <summary>Applies each attached source's unused one-shot bonus to the next matching catch.</summary>
    private void ApplyPendingNextCatchEffects(CardInstance newCatch, bool consumeEffects)
    {
        if (newCatch?.Definition == null)
        {
            return;
        }

        for (int catchIndex = 0; catchIndex < catches.Length; catchIndex++)
        {
            CardInstance source = catches[catchIndex];
            CardEffectDefinition[] effects = source?.Definition?.Effects;
            if (effects == null)
            {
                continue;
            }

            for (int effectIndex = 0; effectIndex < effects.Length; effectIndex++)
            {
                CardEffectDefinition effect = effects[effectIndex];
                if (effect == null
                    || effect.Trigger != CardEffectTrigger.WhenNextMatchingCatchCaught
                    || effect.Target != CardEffectTarget.SpecificCaughtCard
                    || IsNextCatchEffectConsumed(source, effectIndex)
                    || !EffectResolver.RequiredTagsMatch(effect.RequiredTags, newCatch.Definition))
                {
                    continue;
                }

                int weightChange = 0;
                int valueChange = 0;
                if (effect.EffectType == CardEffectType.ModifyCatchValue)
                {
                    valueChange = effect.Amount;
                }
                else if (effect.EffectType == CardEffectType.AddLineLoadModifier)
                {
                    weightChange = effect.Amount;
                }
                else if (effect.EffectType == CardEffectType.RemoveLineLoadModifier)
                {
                    weightChange = -Math.Abs(effect.Amount);
                }
                else
                {
                    continue;
                }

                newCatch.AddPermanentModifiers(weightChange, valueChange);
                if (consumeEffects)
                {
                    consumedNextCatchEffectKeys = AppendEffectKey(
                        consumedNextCatchEffectKeys,
                        BuildEffectKey(source, effectIndex));
                }
            }
        }
    }

    /// <summary>Reports whether one source instance already spent its future-catch effect.</summary>
    private bool IsNextCatchEffectConsumed(CardInstance source, int effectIndex)
    {
        string key = BuildEffectKey(source, effectIndex);
        return Array.IndexOf(consumedNextCatchEffectKeys, key) >= 0;
    }

    /// <summary>Reports whether a per-instance effect key exists in one lifecycle-state collection.</summary>
    private static bool IsEffectKeyTracked(string[] keys, CardInstance source, int effectIndex)
    {
        return Array.IndexOf(keys ?? Array.Empty<string>(), BuildEffectKey(source, effectIndex)) >= 0;
    }

    /// <summary>Builds a stable per-run key for one effect on one caught card instance.</summary>
    private static string BuildEffectKey(CardInstance source, int effectIndex)
    {
        return $"{source?.InstanceId ?? 0}:{effectIndex}";
    }

    private static string[] AppendEffectKey(string[] source, string key)
    {
        string[] safeSource = source ?? Array.Empty<string>();
        string[] result = new string[safeSource.Length + 1];
        Array.Copy(safeSource, result, safeSource.Length);
        result[result.Length - 1] = key;
        return result;
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
            ApplyAnotherCatchReleasedEffects(releasedCatch, CatchRemovalReason.Technique);
            RecordRemoval(releasedCatch, CatchRemovalReason.Technique);
            catches = RemoveCatchAt(catches, targetIndex);
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        resultSummary = $"Released {string.Join(", ", releasedNames)}";
        return releasedNames.Count > 0;
    }

    /// <summary>Applies lasting self-value reactions when a different catch is deliberately released.</summary>
    private void ApplyAnotherCatchReleasedEffects(
        CardInstance releasedCatch,
        CatchRemovalReason removalReason)
    {
        if (removalReason == CatchRemovalReason.LineStrain)
        {
            return;
        }

        InvalidateNoReleaseEffects(releasedCatch);

        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            CardEffectDefinition effect = record?.Effect;
            CardInstance source = record?.SourceInstance;

            if (record == null
                || source == null
                || ReferenceEquals(source, releasedCatch)
                || !IsCatchAttached(source)
                || effect == null
                || record.ActiveTrigger != CardEffectTrigger.WhenAnotherCatchReleased
                || effect.EffectType != CardEffectType.ModifyCatchValue
                || effect.Target != CardEffectTarget.Self)
            {
                continue;
            }

            source.AddPermanentModifiers(0, effect.Amount);
        }
    }

    /// <summary>Forfeits conditional Value effects when another catch is deliberately released later.</summary>
    private void InvalidateNoReleaseEffects(CardInstance releasedCatch)
    {
        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            CardInstance source = record?.SourceInstance;
            if (record?.Effect == null
                || source == null
                || ReferenceEquals(source, releasedCatch)
                || !IsCatchAttached(source)
                || record.ActiveTrigger != CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased)
            {
                continue;
            }

            int effectIndex = Array.IndexOf(source.Definition.Effects, record.Effect);
            if (effectIndex >= 0)
            {
                invalidatedNoReleaseEffectKeys = AppendEffectKey(
                    invalidatedNoReleaseEffectKeys,
                    BuildEffectKey(source, effectIndex));
            }
        }
    }

    /// <summary>Checks whether an effect source still belongs to the current Catch Chain.</summary>
    private bool IsCatchAttached(CardInstance candidate)
    {
        for (int i = 0; i < catches.Length; i++)
        {
            if (ReferenceEquals(catches[i], candidate))
            {
                return true;
            }
        }

        return false;
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
                if (EffectResolver.RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition)
                    && IsValidTechniqueTarget(effect, catches[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        for (int i = catches.Length - 1; i >= 0; i--)
        {
            if (EffectResolver.RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition)
                && IsValidTechniqueTarget(effect, catches[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Excludes release-locked catches only when a Technique is trying to release its target.</summary>
    private static bool IsValidTechniqueTarget(CardEffectDefinition effect, CardInstance candidate)
    {
        return effect.EffectType != CardEffectType.ReleaseCatch
            || candidate?.Definition == null
            || !candidate.Definition.PreventsReleaseWhileCaught;
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
