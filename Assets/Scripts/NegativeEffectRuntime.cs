using System;
using System.Collections.Generic;

/// <summary>Identifies one removable active negative effect occurrence across the run's effect owners.</summary>
public sealed class NegativeEffectOption
{
    private readonly Func<bool> remove;
    public string Label { get; }
    public string Description { get; }
    public CardDefinition SourceCard { get; }

    /// <summary>Captures presentation and removal for one exact runtime occurrence.</summary>
    public NegativeEffectOption(string label, string description, CardDefinition sourceCard, Func<bool> remove)
    {
        Label = label;
        Description = description;
        SourceCard = sourceCard;
        this.remove = remove;
    }

    /// <summary>Asks the owning runtime to remove this occurrence with its normal recalculation rules.</summary>
    public bool TryRemove()
    {
        return remove();
    }
}

/// <summary>Collects active negative effects, including lasting stat penalties while excluding completed actions and ordinary base statistics.</summary>
public static class NegativeEffectRuntime
{
    /// <summary>Builds exact per-source options for attached, Hooked, and queued negative effects.</summary>
    public static NegativeEffectOption[] GetOptions(
        CatchChainRuntime chain,
        EncounterRuntime encounters,
        TechniqueEffectRuntime techniques,
        EffectResolver resolver)
    {
        List<NegativeEffectOption> options = new List<NegativeEffectOption>();
        foreach (ActiveCatchEffectRecord record in chain.ActiveEffectRecords)
        {
            if (record?.Effect?.EffectTone != CardEffectTone.Negative
                || record.ActiveTrigger == CardEffectTrigger.WhenCaught)
            {
                continue;
            }

            ActiveCatchEffectRecord selected = record;
            options.Add(new NegativeEffectOption(
                Label(record.SourceCard, record.Effect), Describe(record.Effect), record.SourceCard,
                () => chain.TryRemoveNegativeEffect(selected, resolver)));
        }

        foreach (CardInstance target in chain.Catches)
        {
            foreach (AppliedCatchModifier modifier in target.AppliedModifiers)
            {
                if (modifier.Effect?.EffectTone != CardEffectTone.Negative)
                {
                    continue;
                }

                CardInstance selectedTarget = target;
                AppliedCatchModifier selectedModifier = modifier;
                string source = modifier.SourceCard == null ? string.Empty : " from " + modifier.SourceCard.DisplayName;
                options.Add(new NegativeEffectOption(target.Definition.DisplayName + source,
                    "Lasting effect: " + Describe(modifier.Effect), target.Definition,
                    () => chain.TryRemoveAppliedNegativeModifier(selectedTarget, selectedModifier, resolver)));
            }
        }

        foreach (PendingCreatureCatchEffect pending in chain.PendingCreatureCatchEffects)
        {
            if (pending.Effect?.EffectTone != CardEffectTone.Negative) continue;
            PendingCreatureCatchEffect selected = pending;
            options.Add(new NegativeEffectOption(Label(pending.SourceCard, pending.Effect),
                "Pending: " + Describe(pending.Effect), pending.SourceCard,
                () => chain.TryRemovePendingCreatureCatchEffect(selected)));
        }

        HookedEffectRecord[] hooked = encounters.HookedEffectRecords;
        for (int i = 0; i < hooked.Length; i++)
        {
            if (hooked[i]?.Effect?.EffectTone != CardEffectTone.Negative)
            {
                continue;
            }

            int selected = i;
            options.Add(new NegativeEffectOption(Label(hooked[i].SourceCard, hooked[i].Effect),
                Describe(hooked[i].Effect), hooked[i].SourceCard,
                () => encounters.TryRemoveNegativeEffect(selected)));
        }

        AddQueued(options, techniques.PendingDescendEffects, techniques, false);
        AddQueued(options, techniques.PendingEncounterEffects, techniques, true);
        return options.ToArray();
    }

    /// <summary>Collects queued occurrences independently even when they share a definition.</summary>
    private static void AddQueued(List<NegativeEffectOption> options, CardEffectDefinition[] effects,
        TechniqueEffectRuntime techniques, bool encounterEffect)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i]?.EffectTone != CardEffectTone.Negative)
            {
                continue;
            }

            int selected = i;
            options.Add(new NegativeEffectOption(encounterEffect ? "Next encounter" : "Next Descend",
                Describe(effects[i]), null, () => techniques.TryRemoveNegativeEffect(encounterEffect, selected)));
        }
    }

    /// <summary>Identifies an effect by its source creature and optional authored ability name.</summary>
    private static string Label(CardDefinition source, CardEffectDefinition effect)
    {
        return source.DisplayName + (string.IsNullOrWhiteSpace(effect.DisplayName) ? string.Empty : " — " + effect.DisplayName);
    }

    /// <summary>Uses player-facing authored reminder text rather than internal effect identifiers.</summary>
    private static string Describe(CardEffectDefinition effect)
    {
        return string.IsNullOrWhiteSpace(effect.ReminderText) ? "Negative effect" : effect.ReminderText;
    }
}
