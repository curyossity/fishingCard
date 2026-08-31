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
        }

        for (int i = 0; i < safeEffects.Length; i++)
        {
            ResolvePersistentStatEffect(safeCatches, safeEffects[i]);
        }
    }

    /// <summary>
    /// Calculates an encounter's weighted-selection tickets after attached attraction effects.
    /// </summary>
    public int GetEncounterSelectionWeight(CardDefinition encounter, ActiveCatchEffectRecord[] activeEffects)
    {
        int weight = 1;
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();

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
        if (!IsPersistent(record))
        {
            return;
        }

        CardEffectDefinition effect = record.Effect;

        if (effect.EffectType != CardEffectType.AddLineLoadModifier
            && effect.EffectType != CardEffectType.RemoveLineLoadModifier
            && effect.EffectType != CardEffectType.ModifyCatchValue)
        {
            return;
        }

        ApplyToTargets(catches, record.SourceCatchIndex, effect);
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
                if (RequiredTagsMatch(effect.RequiredTags, catches[i]?.Definition))
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
            if (i != sourceIndex && RequiredTagsMatch(requiredTags, GetCatch(catches, i)?.Definition))
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
            && record.ActiveTrigger == CardEffectTrigger.WhileAttached;
    }

    /// <summary>
    /// Checks whether every required tag or card-type name is present on a target definition.
    /// </summary>
    private static bool RequiredTagsMatch(string[] requiredTags, CardDefinition target)
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
