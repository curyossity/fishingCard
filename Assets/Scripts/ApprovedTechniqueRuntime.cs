using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns the approved Technique set's pending Prize Hooks and action-scoped Line Load relief.</summary>
[Serializable]
public sealed class ApprovedTechniqueRuntime
{
    [SerializeField] private int pendingLoadRelief;
    [SerializeField] private bool reliefActive;
    [SerializeField] private List<CardEffectDefinition> pendingPrizes = new List<CardEffectDefinition>();
    [SerializeField] private List<CardDefinition> pendingPrizeSources = new List<CardDefinition>();
    public bool HasPendingRelief => pendingLoadRelief > 0 || reliefActive;
    public int PendingPrizeCount => pendingPrizes.Count;

    /// <summary>Identifies the explicitly approved effect family without changing legacy enum values.</summary>
    public static bool Handles(CardEffectDefinition effect)
    { return effect != null && effect.EffectType >= CardEffectType.ReleaseAttachedCreature
        && effect.EffectType <= CardEffectType.QueuePrizeCatchBonus; }

    /// <summary>Checks effect-specific play requirements without consuming cards, concealment, or pending grants.</summary>
    public bool CanApply(CardEffectDefinition effect, EncounterRuntime encounters, CatchChainRuntime chain,
        TechniqueDeckRuntime deck, bool hidden)
    {
        if (!Handles(effect) || encounters == null || !encounters.HasTechniqueReactionWindow) return false;
        switch (effect.EffectType)
        {
            case CardEffectType.ReleaseAttachedCreature:
            case CardEffectType.SwitchCreatureMode: return chain.GetTechniqueTargets(effect).Length > 0;
            case CardEffectType.SkipHookedCreature: return encounters.HookedEncounter?.CardType == CardType.Creature;
            case CardEffectType.ClearCreatureConcealment:
                return encounters.HookedEncounter != null && (encounters.IsCreatureConcealed || hidden)
                    || encounters.HasFutureConcealment;
            case CardEffectType.CancelCurrentHazard:
                return encounters.CurrentEncounter?.CardType == CardType.Hazard
                    && encounters.CurrentEventOccurrence?.State == EventOccurrenceState.Unresolved;
            case CardEffectType.GrantActionLoadRelief: return !HasPendingRelief;
            case CardEffectType.FilterTechniqueHand:
                return deck != null && deck.Hand.Length >= (deck.ResolvingCard == null ? 2 : 1);
            case CardEffectType.QueuePrizeCatchBonus: return true;
            default: return false;
        }
    }

    /// <summary>Resolves immediate approved effects; the caller retains resolving-card ownership throughout.</summary>
    public bool Apply(CardEffectDefinition effect, int effectIndex, TechniqueTargetPlan targets,
        EncounterRuntime encounters, CatchChainRuntime chain, EffectResolver resolver,
        CreatureEffectRuntime creatures, TechniqueDeckRuntime deck, System.Random random, bool hidden,
        out bool revealCurrent, CardDefinition source = null)
    {
        revealCurrent = false;
        if (!CanApply(effect, encounters, chain, deck, hidden)) return false;
        if (TechniqueTargetPlan.NeedsPlayerTargets(effect) && targets == null
            || effect.EffectType == CardEffectType.SwitchCreatureMode && creatures == null) return false;
        switch (effect.EffectType)
        {
            case CardEffectType.ReleaseAttachedCreature:
                return chain.TryApplyTechniqueEffectToTargets(effect, targets.TargetsFor(effectIndex), resolver, out _);
            case CardEffectType.SkipHookedCreature:
                encounters.SetCurrentEncounter(null); encounters.CancelActiveEncounterChain(); return true;
            case CardEffectType.ClearCreatureConcealment:
                if (encounters.HookedEncounter != null && (encounters.IsCreatureConcealed || hidden))
                {
                    if (encounters.IsCreatureConcealed) encounters.TryClearOneCreatureConcealment();
                    revealCurrent = true; return true;
                }
                return encounters.TryClearOneCreatureConcealment();
            case CardEffectType.CancelCurrentHazard:
                return encounters.TryCancelRevealedHazard(encounters.CurrentEventOccurrence.Id);
            case CardEffectType.SwitchCreatureMode:
                int[] ids = targets.TargetsFor(effectIndex);
                if (ids.Length != 1 || !chain.TryChangeCreatureMode(ids[0], resolver)) return false;
                creatures.QueueChangedRole(Array.Find(chain.Catches, c => c.InstanceId == ids[0])); return true;
            case CardEffectType.GrantActionLoadRelief: pendingLoadRelief = Math.Max(0, effect.Amount); return true;
            case CardEffectType.FilterTechniqueHand:
                int[] slots = targets.ResolvingHandSlots(effectIndex);
                Array.Sort(slots); Array.Reverse(slots);
                foreach (int slot in slots) if (!deck.TryDiscardTechnique(slot)) throw new InvalidOperationException("Recast target changed after commitment.");
                deck.DrawTechniques(slots.Length, TechniqueDeckRuntime.HandLimit, random); return true;
            case CardEffectType.QueuePrizeCatchBonus: pendingPrizes.Add(effect); pendingPrizeSources.Add(source ?? deck?.ResolvingCard); return true;
            default: return false;
        }
    }

    /// <summary>Activates relief once before event consequences or creature commitments for a confirmed action.</summary>
    public void BeginAction(CatchChainRuntime chain)
    {
        if (reliefActive || pendingLoadRelief == 0) return;
        reliefActive = true; chain.SetActionLoadReduction(pendingLoadRelief); pendingLoadRelief = 0;
    }

    /// <summary>Expires relief after action checks and before any subsequent encounter reveal.</summary>
    public void EndAction(CatchChainRuntime chain)
    { chain.SetActionLoadReduction(0); reliefActive = false; }

    /// <summary>Awards every pending Prize Hook on ordinary or Apex Catch entry before on-attach effects.</summary>
    public void ApplyPendingPrizes(CardInstance card)
    {
        if (card == null || card.IsBait || (card.Definition.CardType != CardType.Creature
            && card.Definition.CardType != CardType.ApexEncounter)) return;
        PreviewPendingPrizes(card);
        pendingPrizes.Clear();
        pendingPrizeSources.Clear();
    }

    /// <summary>Projects pending Catch modifiers onto a detached preview without consuming the grants.</summary>
    public void PreviewPendingPrizes(CardInstance card)
    {
        if (card == null || card.IsBait || (card.Definition.CardType != CardType.Creature
            && card.Definition.CardType != CardType.ApexEncounter)) return;
        for (int i = 0; i < pendingPrizes.Count; i++)
            card.AddPermanentModifiers(1, pendingPrizes[i].Amount, pendingPrizes[i], i < pendingPrizeSources.Count ? pendingPrizeSources[i] : null);
    }

    /// <summary>Clears all unfinished grants at run replacement or shutdown.</summary>
    public void Reset() { pendingLoadRelief = 0; reliefActive = false; pendingPrizes.Clear(); pendingPrizeSources.Clear(); }
}
