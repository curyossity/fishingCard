using System;

public sealed class EffectResolver
{
    /// <summary>
    /// Recalculates every catch from base stats, then applies all persistent stat interactions.
    /// </summary>
    public void ResolveCatchChain(CardInstance[] catches, ActiveCatchEffectRecord[] activeEffects)
    {
        CardInstance[] safeCatches = catches ?? Array.Empty<CardInstance>();
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();

        // Rebuilding from definitions makes removing an effect source undo its changes without bookkeeping drift.
        for (int i = 0; i < safeCatches.Length; i++)
        {
            safeCatches[i]?.ResetCurrentStats();
            safeCatches[i]?.ResetEffectiveTags();
        }

        // Tags are available before all tag-dependent stat and attachment-trigger calculations.
        foreach (ActiveCatchEffectRecord record in safeEffects)
        {
            if (!IsPersistent(record) || record.Effect.EffectType != CardEffectType.GrantTagToOtherCreatures) continue;
            foreach (CardInstance target in safeCatches)
            {
                if (target != null && target != record.SourceInstance && target.Definition?.CardType == CardType.Creature
                    && RequiredTagsMatchInstance(record.Effect.RequiredTags, target))
                {
                    target.GrantTag(record.Effect.GrantedTag);
                }
            }
        }

        for (int i = 0; i < safeEffects.Length; i++)
        {
            if (safeEffects[i]?.Effect?.EffectType != CardEffectType.SelectAttachedWeightTargets
                && safeEffects[i]?.Effect?.EffectType != CardEffectType.SetSelfWeightZeroWithOtherCreatureCatches
                && safeEffects[i]?.Effect?.EffectType != CardEffectType.ReduceSelfWeightWithMatchingCreatureBait)
                ResolvePersistentStatEffect(safeCatches, safeEffects[i]);
        }
        foreach (ActiveCatchEffectRecord record in safeEffects)
        {
            if (!IsPersistent(record) || record.Effect.EffectType != CardEffectType.ReduceSelfWeightWithMatchingCreatureBait
                || record.SourceInstance == null || record.SourceInstance.IsBait) continue;
            if (Array.Exists(safeCatches, c => c != null && c != record.SourceInstance && c.IsBait
                && c.Definition?.CardType == CardType.Creature && RequiredTagsMatchInstance(record.Effect.RequiredTags, c)))
            {
                record.SourceInstance.ModifyWeight(-Math.Abs(record.Effect.Amount));
            }
        }

        // Selected reductions stack against the final pre-reduction weight, then clamp at zero.
        foreach (ActiveCatchEffectRecord record in safeEffects)
        {
            if (record?.Effect?.EffectType == CardEffectType.SelectAttachedWeightTargets)
                ResolvePersistentStatEffect(safeCatches, record);
        }
        // An eligible Packed Shoal sets final effective Weight to zero, regardless of other weight modifiers.
        foreach (ActiveCatchEffectRecord record in safeEffects)
        {
            if (!IsPersistent(record) || record.Effect.EffectType != CardEffectType.SetSelfWeightZeroWithOtherCreatureCatches
                || record.SourceInstance == null || record.SourceInstance.IsBait) continue;
            int count = Array.FindAll(safeCatches, c => c != null && c != record.SourceInstance && !c.IsBait
                && c.Definition?.CardType == CardType.Creature && RequiredTagsMatchInstance(record.Effect.RequiredTags, c)).Length;
            if (count >= Math.Max(1, record.Effect.Amount)) record.SourceInstance.ModifyWeight(-record.SourceInstance.CurrentWeight);
        }
    }

    /// <summary>
    /// Calculates encounter-selection tickets after attached and one-use attraction effects.
    /// </summary>
    public int GetEncounterSelectionWeight(
        CardDefinition encounter,
        ActiveCatchEffectRecord[] activeEffects,
        CardEffectDefinition[] temporaryEffects)
    {
        int weight = 1;
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();
        CardEffectDefinition[] safeTemporaryEffects = temporaryEffects ?? Array.Empty<CardEffectDefinition>();

        for (int i = 0; i < safeEffects.Length; i++)
        {
            ActiveCatchEffectRecord record = safeEffects[i];
            CardEffectDefinition effect = record?.Effect;

            if (!IsPersistent(record)
                || effect.EffectType != CardEffectType.ModifyFutureEncounterProperties
                || effect.Target != CardEffectTarget.FutureEncounters
                || !RequiredTagsMatch(effect.RequiredTags, encounter))
            {
                continue;
            }

            weight += effect.Amount;
        }

        for (int i = 0; i < safeTemporaryEffects.Length; i++)
        {
            CardEffectDefinition effect = safeTemporaryEffects[i];

            if (effect != null
                && effect.EffectType == CardEffectType.ModifyFutureEncounterProperties
                && effect.Target == CardEffectTarget.FutureEncounters
                && RequiredTagsMatch(effect.RequiredTags, encounter))
            {
                weight += effect.Amount;
            }
        }

        return Math.Max(0, weight);
    }

    /// <summary>
    /// Reports whether an attached persistent downside hides current encounter details.
    /// </summary>
    public bool HidesEncounterInformation(ActiveCatchEffectRecord[] activeEffects)
    {
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();

        for (int i = 0; i < safeEffects.Length; i++)
        {
            ActiveCatchEffectRecord record = safeEffects[i];
            CardEffectDefinition effect = record?.Effect;

            if (IsPersistent(record)
                && effect.EffectType == CardEffectType.HideEncounterInformation
                && (effect.Target == CardEffectTarget.CurrentEncounter
                    || effect.Target == CardEffectTarget.FutureEncounters))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Applies one supported persistent stat effect to its resolved catch targets.
    /// </summary>
    private static void ResolvePersistentStatEffect(CardInstance[] catches, ActiveCatchEffectRecord record)
    {
        if (record?.Effect?.EffectType == CardEffectType.SelectAttachedWeightTargets)
        {
            foreach (CardInstance target in catches)
            {
                if (target != null && target != record.SourceInstance && target.Definition?.CardType == CardType.Creature
                    && record.SourceInstance.IsBait && record.SourceInstance.IsSelectedEffectTarget(record.Effect, target.InstanceId)
                    && RequiredTagsMatchInstance(record.Effect.RequiredTags, target))
                {
                    target.ModifyWeight(-Math.Abs(record.Effect.Amount));
                }
            }
            return;
        }

        if (!IsPersistent(record))
        {
            return;
        }

        CardEffectDefinition effect = record.Effect;

        if (effect.EffectType == CardEffectType.ModifySelfValuePerOtherMatchingCatch
            || effect.EffectType == CardEffectType.ModifySelfValuePerOtherCreatureCatch)
        {
            ApplySelfValuePerOtherMatchingCatch(catches, record.SourceCatchIndex, effect);
            return;
        }

        if (effect.EffectType != CardEffectType.AddLineLoadModifier
            && effect.EffectType != CardEffectType.RemoveLineLoadModifier
            && effect.EffectType != CardEffectType.ModifyCatchValue)
        {
            return;
        }

        ApplyToTargets(catches, record.SourceCatchIndex, effect);
    }

    /// <summary>Changes the source catch's Value once for every other attached catch matching the authored tags.</summary>
    private static void ApplySelfValuePerOtherMatchingCatch(
        CardInstance[] catches,
        int sourceIndex,
        CardEffectDefinition effect)
    {
        CardInstance source = GetCatch(catches, sourceIndex);
        if (source == null || effect.Target != CardEffectTarget.Self)
        {
            return;
        }

        int matchingOthers = 0;
        for (int i = 0; i < catches.Length; i++)
        {
            if (i != sourceIndex && RequiredTagsMatchInstance(effect.RequiredTags, catches[i])
                && (effect.EffectType != CardEffectType.ModifySelfValuePerOtherCreatureCatch
                    || (!catches[i].IsBait && catches[i].Definition?.CardType == CardType.Creature)))
            {
                matchingOthers++;
            }
        }

        source.ModifyValue(effect.Amount * matchingOthers);
    }

    /// <summary>
    /// Finds the effect targets and applies its numerical stat change to each one.
    /// </summary>
    private static void ApplyToTargets(CardInstance[] catches, int sourceIndex, CardEffectDefinition effect)
    {
        if (effect.Target == CardEffectTarget.Self)
        {
            ApplyStatChange(GetCatch(catches, sourceIndex), effect);
            return;
        }

        if (effect.Target == CardEffectTarget.CatchChain)
        {
            for (int i = 0; i < catches.Length; i++)
            {
                if (RequiredTagsMatchInstance(effect.RequiredTags, catches[i]))
                {
                    ApplyStatChange(catches[i], effect);
                }
            }

            return;
        }

        if (effect.Target == CardEffectTarget.SpecificCaughtCard)
        {
            int targetIndex = FindSpecificTarget(catches, sourceIndex, effect);
            ApplyStatChange(GetCatch(catches, targetIndex), effect);
        }
    }

    /// <summary>
    /// Selects one matching catch according to the target rule authored on the effect.
    /// </summary>
    private static int FindSpecificTarget(CardInstance[] catches, int sourceIndex, CardEffectDefinition effect)
    {
        switch (effect.CaughtCardTargetMode)
        {
            case CaughtCardTargetMode.NextMatching:
                return FindMatching(catches, sourceIndex + 1, catches.Length, 1, sourceIndex, effect.RequiredTags);
            case CaughtCardTargetMode.FirstMatching:
                return FindMatching(catches, 0, catches.Length, 1, sourceIndex, effect.RequiredTags);
            case CaughtCardTargetMode.LastMatching:
                return FindMatching(catches, catches.Length - 1, -1, -1, sourceIndex, effect.RequiredTags);
            default:
                return FindMatching(catches, sourceIndex - 1, -1, -1, sourceIndex, effect.RequiredTags);
        }
    }

    /// <summary>
    /// Searches a directional range for the first matching catch other than the effect source.
    /// </summary>
    private static int FindMatching(
        CardInstance[] catches,
        int start,
        int endExclusive,
        int step,
        int sourceIndex,
        string[] requiredTags)
    {
        for (int i = start; i != endExclusive; i += step)
        {
            if (i != sourceIndex && RequiredTagsMatchInstance(requiredTags, GetCatch(catches, i)))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Applies the numerical portion of a supported stat effect to one catch.
    /// </summary>
    private static void ApplyStatChange(CardInstance target, CardEffectDefinition effect)
    {
        if (target == null)
        {
            return;
        }

        if (effect.EffectType == CardEffectType.ModifyCatchValue)
        {
            target.ModifyValue(effect.Amount);
            return;
        }

        int weightChange = effect.EffectType == CardEffectType.RemoveLineLoadModifier
            ? -Math.Abs(effect.Amount)
            : effect.Amount;
        target.ModifyWeight(weightChange);
    }

    /// <summary>
    /// Checks whether a tracked effect remains active for as long as its source catch is attached.
    /// </summary>
    private static bool IsPersistent(ActiveCatchEffectRecord record)
    {
        return record != null
            && record.Effect != null
            && (record.ActiveTrigger == CardEffectTrigger.WhileAttached
                || record.ActiveTrigger == CardEffectTrigger.UntilNextEncounterResolved
                || record.ActiveTrigger == CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased);
    }

    /// <summary>
    /// Checks whether every required tag or card-type name is present on a target definition.
    /// </summary>
    public static bool RequiredTagsMatch(string[] requiredTags, CardDefinition target)
    {
        if (requiredTags == null || requiredTags.Length == 0)
        {
            return target != null;
        }

        if (target == null)
        {
            return false;
        }

        for (int i = 0; i < requiredTags.Length; i++)
        {
            if (string.Equals(requiredTags[i], target.CardType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!HasTag(target.Tags, requiredTags[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Checks effective attached-card tags while preserving definition-based encounter eligibility.</summary>
    public static bool RequiredTagsMatchInstance(string[] requiredTags, CardInstance target)
    {
        if (target?.Definition == null) return false;
        foreach (string tag in requiredTags ?? Array.Empty<string>())
        {
            if (!string.Equals(tag, target.Definition.CardType.ToString(), StringComparison.OrdinalIgnoreCase)
                && !target.HasTag(tag)) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks a card's authored tags without case sensitivity.
    /// </summary>
    private static bool HasTag(string[] tags, string requiredTag)
    {
        if (tags == null)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (string.Equals(tags[i], requiredTag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Safely returns a catch at an index, or null when the index is outside the chain.
    /// </summary>
    private static CardInstance GetCatch(CardInstance[] catches, int index)
    {
        return catches != null && index >= 0 && index < catches.Length ? catches[index] : null;
    }
}
