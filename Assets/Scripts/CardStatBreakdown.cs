using System;
using System.Collections.Generic;

/// <summary>Identifies the independently inspectable numerical fields of a creature.</summary>
public enum CreatureStat { Weight, Value }

/// <summary>One effective contribution recorded by the authoritative stat resolver, not inferred by a view.</summary>
public readonly struct CardStatContribution
{
    public readonly CardDefinition Source;
    public readonly CardEffectDefinition Effect;
    public readonly int Weight, Value;
    /// <summary>Retains the actual source and both numerical contributions through previews and snapshots.</summary>
    public CardStatContribution(CardDefinition source, CardEffectDefinition effect, int weight, int value)
    { Source = source; Effect = effect; Weight = weight; Value = value; }
}

/// <summary>One named source row, with a signed effective change rather than an ability explanation.</summary>
public readonly struct CardStatReason
{
    public readonly CardDefinition Source;
    public readonly string Name;
    public readonly int Amount;
    /// <summary>Captures the source identity without depending on its current attachment lifetime.</summary>
    public CardStatReason(CardDefinition source, string name, int amount) { Source = source; Name = name; Amount = amount; }
}

/// <summary>Read-only stat inspection data; constructing it cannot consume pending effects or mutate gameplay.</summary>
public sealed class CardStatBreakdown
{
    public readonly CreatureStat Stat;
    public readonly int Baseline, Current, OccurrenceId;
    public readonly CardDefinition Creature;
    public readonly CardStatReason[] Reasons;
    /// <summary>Groups repeated contributions by their source card while retaining deterministic resolution order.</summary>
    public CardStatBreakdown(CardInstance instance, CreatureStat stat, int occurrenceId)
    {
        Stat = stat; Creature = instance.Definition; OccurrenceId = occurrenceId;
        Baseline = stat == CreatureStat.Weight ? Creature.Weight : instance.BaseValue;
        Current = stat == CreatureStat.Weight ? instance.CurrentWeight : instance.CurrentValue;
        var rows = new List<CardStatReason>();
        foreach (CardStatContribution c in instance.ResolvedStatContributions)
        {
            int amount = stat == CreatureStat.Weight ? c.Weight : c.Value;
            if (amount == 0) continue;
            string name = c.Source != null ? c.Source.DisplayName : !string.IsNullOrWhiteSpace(c.Effect?.DisplayName)
                ? c.Effect.DisplayName : "Unattributed modifier";
            int index = rows.FindIndex(r => r.Source == c.Source && r.Name == name);
            if (index < 0) rows.Add(new CardStatReason(c.Source, name, amount));
            else rows[index] = new CardStatReason(c.Source, name, rows[index].Amount + amount);
        }
        rows.RemoveAll(r => r.Amount == 0); Reasons = rows.ToArray();
    }
}

/// <summary>Lets theme-specific inspection read current visible data without reaching into gameplay controllers.</summary>
public interface ICreatureStatInspectionSource
{
    /// <summary>Returns a visible modified numerical field and its resolver-owned sources, or rejects inspection.</summary>
    bool TryGetStatBreakdown(CreatureStat stat, out CardStatBreakdown breakdown);
}
