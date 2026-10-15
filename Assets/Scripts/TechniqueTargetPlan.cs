using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Stores exact player-selected recipients per effect before a Technique is committed.</summary>
[Serializable]
public sealed class TechniqueTargetPlan
{
    [Serializable]
    private sealed class Selection
    {
        public int EffectIndex;
        public int Required;
        public List<int> TargetIds = new List<int>();
    }
    [SerializeField] private CardDefinition card;
    [SerializeField] private CardDefinition encounterTarget;
    [SerializeField] private int encounterRevision = -1;
    public const int EncounterTargetId = -1;
    public bool CurrentIsEncounterTarget => CurrentEffectIndex >= 0 && RequiresEncounterSelection(card.Effects[CurrentEffectIndex]);
    [SerializeField] private List<Selection> selections = new List<Selection>();
    public CardDefinition Card => card;
    public int CurrentEffectIndex
    {
        get { Selection next = selections.Find(s => s.TargetIds.Count < s.Required); return next == null ? -1 : next.EffectIndex; }
    }
    public bool IsComplete => CurrentEffectIndex < 0;
    public int CurrentRequired => selections.Find(s => s.EffectIndex == CurrentEffectIndex)?.Required ?? 0;
    public int CurrentSelected => selections.Find(s => s.EffectIndex == CurrentEffectIndex)?.TargetIds.Count ?? 0;
    public bool CanFinishCurrentSelection => CurrentEffectIndex >= 0 && CurrentSelected > 0
        && card.Effects[CurrentEffectIndex].EffectType == CardEffectType.ReleaseCatch;

    /// <summary>Accepts fewer than the maximum recipients for an up-to-count Release effect.</summary>
    public bool TryFinishCurrentSelection()
    {
        if (!CanFinishCurrentSelection) return false;
        Selection selection = selections.Find(s => s.EffectIndex == CurrentEffectIndex);
        selection.Required = selection.TargetIds.Count;
        return true;
    }

    /// <summary>Creates a fresh selection request without choosing any targets automatically.</summary>
    public TechniqueTargetPlan(CardDefinition card, CatchChainRuntime chain, EncounterRuntime encounters = null)
    {
        this.card = card;
        encounterTarget = encounters?.CurrentEncounter; encounterRevision = encounters?.EncounterRevision ?? -1;
        for (int i = 0; i < (card?.Effects?.Length ?? 0); i++)
            if (NeedsPlayerTargets(card.Effects[i]))
                selections.Add(new Selection { EffectIndex = i, Required = RequiresEncounterSelection(card.Effects[i]) ? 1 : RequiredCount(card.Effects[i], chain) });
    }

    /// <summary>Accepts explicitly supplied per-effect target IDs for programmatic play and validation.</summary>
    public TechniqueTargetPlan(CardDefinition card, int[][] targets, EncounterRuntime encounters = null)
    {
        this.card = card;
        encounterTarget = encounters?.CurrentEncounter; encounterRevision = encounters?.EncounterRevision ?? -1;
        for (int i = 0; i < (targets?.Length ?? 0); i++)
            if (targets[i] != null && targets[i].Length > 0)
                selections.Add(new Selection { EffectIndex = i, Required = targets[i].Length, TargetIds = new List<int>(targets[i]) });
    }

    /// <summary>Recognizes individual attached-card targets, excluding targetless distributed effects.</summary>
    public static bool RequiresSelection(CardEffectDefinition effect)
    {
        return effect != null && (effect.Target == CardEffectTarget.CatchChain || effect.Target == CardEffectTarget.SpecificCaughtCard)
            && (effect.EffectType == CardEffectType.ReleaseCatch || effect.EffectType == CardEffectType.ModifyCatchValue
                || effect.EffectType == CardEffectType.AddLineLoadModifier || effect.EffectType == CardEffectType.RemoveLineLoadModifier);
    }

    /// <summary>Recognizes the single current encounter as an explicitly selected target.</summary>
    public static bool RequiresEncounterSelection(CardEffectDefinition effect)
    {
        return effect != null && (effect.Target == CardEffectTarget.HookedEncounter || effect.Target == CardEffectTarget.CurrentEncounter);
    }

    /// <summary>Distinguishes player-targeted effects from targetless distributed or pending effects.</summary>
    public static bool NeedsPlayerTargets(CardEffectDefinition effect)
    { return RequiresSelection(effect) || RequiresEncounterSelection(effect); }

    /// <summary>Uses the existing effect's target count, capped by the available legal attachments.</summary>
    public static int RequiredCount(CardEffectDefinition effect, CatchChainRuntime chain)
    {
        int eligible = chain.GetTechniqueTargets(effect).Length;
        return effect.EffectType == CardEffectType.ReleaseCatch
            ? (int)Math.Min(eligible, Math.Max(1L, Math.Abs((long)effect.Amount))) : Math.Min(1, eligible);
    }

    /// <summary>Returns a copy of the selected physical attachment IDs for one effect.</summary>
    public int[] TargetsFor(int effectIndex)
    { return selections.Find(s => s.EffectIndex == effectIndex)?.TargetIds.ToArray() ?? Array.Empty<int>(); }

    /// <summary>Returns legal remaining choices without spending the card or changing the chain.</summary>
    public CardInstance[] Candidates(CatchChainRuntime chain)
    {
        int index = CurrentEffectIndex;
        if (index < 0 || CurrentIsEncounterTarget) return Array.Empty<CardInstance>();
        int[] selected = TargetsFor(index);
        return Array.FindAll(chain.GetTechniqueTargets(card.Effects[index]), c => Array.IndexOf(selected, c.InstanceId) < 0);
    }

    /// <summary>Adds one legal player selection to the current effect.</summary>
    public bool TrySelect(int instanceId, CatchChainRuntime chain, EncounterRuntime encounters = null)
    {
        if (CurrentIsEncounterTarget)
        {
            if (instanceId != EncounterTargetId || !MatchesEncounter(card.Effects[CurrentEffectIndex], encounters)) return false;
            selections.Find(s => s.EffectIndex == CurrentEffectIndex).TargetIds.Add(instanceId); return true;
        }
        if (!Array.Exists(Candidates(chain), c => c.InstanceId == instanceId)) return false;
        selections.Find(s => s.EffectIndex == CurrentEffectIndex).TargetIds.Add(instanceId);
        return true;
    }

    /// <summary>Revalidates all required selections before commitment, rejecting duplicates and targetless selections.</summary>
    public bool Validate(CardDefinition expected, CatchChainRuntime chain, out string reason, EncounterRuntime encounters = null)
    {
        reason = "Required Technique targets changed or are invalid";
        if (card == null || card != expected || !IsComplete) return false;
        foreach (Selection selection in selections)
        {
            if (selection.EffectIndex < 0 || selection.EffectIndex >= card.Effects.Length
                || !NeedsPlayerTargets(card.Effects[selection.EffectIndex])) return false;
        }
        for (int i = 0; i < card.Effects.Length; i++)
        {
            if (!NeedsPlayerTargets(card.Effects[i])) continue;
            int[] ids = TargetsFor(i);
            if (RequiresEncounterSelection(card.Effects[i]))
            {
                if (ids.Length != 1 || ids[0] != EncounterTargetId || !MatchesEncounter(card.Effects[i], encounters)) return false;
                continue;
            }
            int maximum = RequiredCount(card.Effects[i], chain);
            if (maximum == 0 || ids.Length == 0 || ids.Length > maximum
                || card.Effects[i].EffectType != CardEffectType.ReleaseCatch && ids.Length != maximum) return false;
            HashSet<int> unique = new HashSet<int>();
            CardInstance[] legal = chain.GetTechniqueTargets(card.Effects[i]);
            foreach (int id in ids)
                if (!unique.Add(id) || !Array.Exists(legal, c => c.InstanceId == id)) return false;
        }
        reason = string.Empty; return true;
    }

    /// <summary>Rejects discarded, replaced, or stale encounter identities at commitment.</summary>
    private bool MatchesEncounter(CardEffectDefinition effect, EncounterRuntime encounters)
    {
        CardDefinition current = effect.Target == CardEffectTarget.HookedEncounter ? encounters?.HookedEncounter : encounters?.CurrentEncounter;
        return current != null && current == encounterTarget && encounters.EncounterRevision == encounterRevision
            && EffectResolver.RequiredTagsMatch(effect.RequiredTags, current);
    }
}
