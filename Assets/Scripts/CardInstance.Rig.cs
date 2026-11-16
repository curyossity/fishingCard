using System;
using UnityEngine;

public sealed partial class CardInstance
{
    [SerializeField] private bool isRigCreature;
    [SerializeField] private int rigLevel = 1, rigCopies = 1, rigSaleValue, rigTriggeredValue;
    private RigAbilitySpec rigAbility;
    public bool IsRigCreature => isRigCreature;
    public int Level => isRigCreature ? rigLevel : 1;
    public int CopiesCollected => isRigCreature ? rigCopies : 1;
    public RigAbilitySpec PrimaryAbility => rigAbility;
    public int AccumulatedBaseValue => isRigCreature ? checked(baseValue * rigCopies) : baseValue;
    internal int TriggeredRigValue => rigTriggeredValue;

    /// <summary>Creates a role-free rig member using fixed base sale value, without legacy on-attach or randomized-value effects.</summary>
    internal static CardInstance CreateRigCreature(int id, CardDefinition card)
    {
        if (card == null || string.IsNullOrWhiteSpace(card.UniqueId)
            || card.CardType != CardType.Creature && card.CardType != CardType.ApexEncounter) throw new ArgumentException("A rig slot requires an identified creature species.");
        var member = new CardInstance { instanceId = id, definition = card, isRigCreature = true, baseValue = card.Value,
            rigLevel = 1, rigCopies = 1, rigSaleValue = card.Value, currentValue = card.Value, currentWeight = 0,
            rigAbility = card.PrimaryAbility.Snapshot() };
        member.ResetEffectiveTags(); return member;
    }
    /// <summary>Applies one additional landed copy to this species, preserving its stable identity and original base price.</summary>
    internal void UpgradeRigCreature() { rigLevel = checked(rigLevel + 1); rigCopies = checked(rigCopies + 1); }
    /// <summary>Stores only explicit acquisition/upgrade grants, never continuous derived bonuses.</summary>
    internal void GrantRigValue(int value) { rigTriggeredValue = checked(rigTriggeredValue + value); }
    /// <summary>Replaces the derived sale value after a complete positional recalculation.</summary>
    internal void SetRigValue(int value) { rigSaleValue = value; currentValue = value; }
    /// <summary>Adopts a validated candidate without replacing the stable member object used by pooled views.</summary>
    internal void ApplyRigCandidate(CardInstance candidate)
    {
        if (!isRigCreature || !candidate.isRigCreature || instanceId != candidate.instanceId) throw new ArgumentException("Rig candidate identity changed.");
        candidate.CopyRigStateTo(this); currentValue = candidate.currentValue;
    }
    /// <summary>Copies all expedition-only upgrade data into a preview or result without modifying the shared definition.</summary>
    private void CopyRigStateTo(CardInstance target)
    {
        target.isRigCreature = isRigCreature; target.rigLevel = rigLevel; target.rigCopies = rigCopies;
        target.rigSaleValue = rigSaleValue; target.rigTriggeredValue = rigTriggeredValue; target.rigAbility = rigAbility;
    }
}
