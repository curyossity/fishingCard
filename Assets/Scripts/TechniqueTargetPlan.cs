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
    [SerializeField] private long handVersion = -1;
    [SerializeField] private int playedHandIndex = -1;
    public const int EncounterTargetId = -1;
    public bool CurrentIsHandTarget => CurrentEffectIndex >= 0 && IsHandEffect(card.Effects[CurrentEffectIndex]);
    public bool CurrentIsEncounterTarget => CurrentEffectIndex >= 0 && RequiresEncounterSelection(card.Effects[CurrentEffectIndex]);
    [SerializeField] private List<Selection> selections = new List<Selection>();
    public CardDefinition Card => card;

    /// <summary>Requires a hand-filter plan to commit the exact original played slot, even with identical definitions.</summary>
    public bool MatchesPlayedHandIndex(int index)
    { return !Array.Exists(card.Effects, IsHandEffect) || playedHandIndex == index; }
    public int CurrentEffectIndex
    {
        get { Selection next = selections.Find(s => s.TargetIds.Count < s.Required); return next == null ? -1 : next.EffectIndex; }
    }
    public bool IsComplete => CurrentEffectIndex < 0;
    public int CurrentRequired => selections.Find(s => s.EffectIndex == CurrentEffectIndex)?.Required ?? 0;
    public int CurrentSelected => selections.Find(s => s.EffectIndex == CurrentEffectIndex)?.TargetIds.Count ?? 0;
    public bool CanFinishCurrentSelection => CurrentEffectIndex >= 0 && CurrentSelected > 0
        && (card.Effects[CurrentEffectIndex].EffectType == CardEffectType.ReleaseCatch || CurrentIsHandTarget);

    /// <summary>Accepts fewer than the maximum recipients for an up-to-count Release or Recast effect.</summary>
    public bool TryFinishCurrentSelection()
    {
        if (!CanFinishCurrentSelection) return false;
        Selection selection = selections.Find(s => s.EffectIndex == CurrentEffectIndex);
        selection.Required = selection.TargetIds.Count;
        return true;
    }

    /// <summary>Creates a fresh selection request without choosing any targets automatically.</summary>
    public TechniqueTargetPlan(CardDefinition card, CatchChainRuntime chain, EncounterRuntime encounters = null,
        TechniqueDeckRuntime deck = null, int handIndex = -1)
    {
        this.card = card;
        encounterTarget = encounters?.CurrentEncounter; encounterRevision = encounters?.EncounterRevision ?? -1;
        handVersion = deck?.HandVersion ?? -1; playedHandIndex = handIndex;
        for (int i = 0; i < (card?.Effects?.Length ?? 0); i++)
            if (NeedsPlayerTargets(card.Effects[i]))
                selections.Add(new Selection { EffectIndex = i, Required = IsHandEffect(card.Effects[i])
                    ? Math.Min(2, Math.Max(0, (deck?.Hand.Length ?? 0) - 1))
                    : RequiresEncounterSelection(card.Effects[i]) ? 1 : RequiredCount(card.Effects[i], chain) });
    }

    /// <summary>Accepts explicitly supplied per-effect target IDs for programmatic play and validation.</summary>
    public TechniqueTargetPlan(CardDefinition card, int[][] targets, EncounterRuntime encounters = null,
        TechniqueDeckRuntime deck = null, int handIndex = -1)
    {
        this.card = card;
        encounterTarget = encounters?.CurrentEncounter; encounterRevision = encounters?.EncounterRevision ?? -1;
        handVersion = deck?.HandVersion ?? -1; playedHandIndex = handIndex;
        for (int i = 0; i < (targets?.Length ?? 0); i++)
            if (targets[i] != null && targets[i].Length > 0)
                selections.Add(new Selection { EffectIndex = i, Required = targets[i].Length, TargetIds = new List<int>(targets[i]) });
    }

    /// <summary>Recognizes individual attached-card targets, excluding targetless distributed effects.</summary>
    public static bool RequiresSelection(CardEffectDefinition effect)
    {
        return effect != null && (effect.Target == CardEffectTarget.CatchChain || effect.Target == CardEffectTarget.SpecificCaughtCard)
            && (effect.EffectType == CardEffectType.ReleaseCatch || effect.EffectType == CardEffectType.ModifyCatchValue
                || effect.EffectType == CardEffectType.ReleaseAttachedCreature || effect.EffectType == CardEffectType.SwitchCreatureMode
                || effect.EffectType == CardEffectType.AddLineLoadModifier || effect.EffectType == CardEffectType.RemoveLineLoadModifier);
    }

    /// <summary>Recognizes the single current encounter as an explicitly selected target.</summary>
    public static bool RequiresEncounterSelection(CardEffectDefinition effect)
    {
        return effect != null && (effect.Target == CardEffectTarget.HookedEncounter || effect.Target == CardEffectTarget.CurrentEncounter);
    }

    /// <summary>Distinguishes player-targeted effects from targetless distributed or pending effects.</summary>
    public static bool NeedsPlayerTargets(CardEffectDefinition effect)
    { return RequiresSelection(effect) || RequiresEncounterSelection(effect) || IsHandEffect(effect); }

    /// <summary>Recognizes Recast's other-hand-card targeting, independent of attached creatures.</summary>
    public static bool IsHandEffect(CardEffectDefinition effect)
    { return effect?.EffectType == CardEffectType.FilterTechniqueHand; }

    /// <summary>Encodes a precommit physical hand slot separately from encounter and attachment target IDs.</summary>
    public static int HandTargetId(int slot) { return -1000 - slot; }

    /// <summary>Lists unselected hand slots excluding the Technique being played.</summary>
    public int[] HandCandidates(TechniqueDeckRuntime deck)
    {
        if (!CurrentIsHandTarget || deck == null || deck.HandVersion != handVersion) return Array.Empty<int>();
        List<int> slots = new List<int>();
        for (int i = 0; i < deck.Hand.Length; i++)
            if (i != playedHandIndex && Array.IndexOf(TargetsFor(CurrentEffectIndex), HandTargetId(i)) < 0) slots.Add(i);
        return slots.ToArray();
    }

    /// <summary>Maps precommit hand selections to current slots after the played card has left hand.</summary>
    public int[] ResolvingHandSlots(int effectIndex)
    { return Array.ConvertAll(TargetsFor(effectIndex), id => { int slot = -1000 - id; return slot > playedHandIndex ? slot - 1 : slot; }); }

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
        if (index < 0 || CurrentIsEncounterTarget || CurrentIsHandTarget) return Array.Empty<CardInstance>();
        int[] selected = TargetsFor(index);
        return Array.FindAll(chain.GetTechniqueTargets(card.Effects[index]), c => Array.IndexOf(selected, c.InstanceId) < 0);
    }

    /// <summary>Adds one legal player selection to the current effect.</summary>
    public bool TrySelect(int instanceId, CatchChainRuntime chain, EncounterRuntime encounters = null, TechniqueDeckRuntime deck = null)
    {
        if (CurrentIsHandTarget)
        {
            if (!Array.Exists(HandCandidates(deck), slot => HandTargetId(slot) == instanceId)) return false;
            selections.Find(s => s.EffectIndex == CurrentEffectIndex).TargetIds.Add(instanceId); return true;
        }
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
    public bool Validate(CardDefinition expected, CatchChainRuntime chain, out string reason, EncounterRuntime encounters = null,
        TechniqueDeckRuntime deck = null)
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
            if (IsHandEffect(card.Effects[i]))
            {
                bool resolving = deck?.ResolvingCard == card;
                if (deck == null || handVersion < 0 || deck.HandVersion != handVersion + (resolving ? 1 : 0)
                    || resolving && deck.ResolvingOriginalHandIndex != playedHandIndex
                    || playedHandIndex < 0 || !resolving && (playedHandIndex >= deck.Hand.Length || deck.Hand[playedHandIndex] != card)
                    || ids.Length < 1 || ids.Length > 2) return false;
                HashSet<int> slots = new HashSet<int>();
                foreach (int id in ids)
                {
                    int slot = -1000 - id;
                    if (!slots.Add(slot) || slot < 0 || slot == playedHandIndex
                        || slot >= deck.Hand.Length + (resolving ? 1 : 0)) return false;
                }
                continue;
            }
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
