using System;
using System.Collections.Generic;

/// <summary>Freezes the equipped benefit for one expedition and owns its finite Catch grants.</summary>
public sealed class RunEquipmentRuntime
{
    private TackleShopCatalog.Item equipped;
    private int rewardedCatches;
    private readonly HashSet<int> attachedIds = new HashSet<int>();
    public int CapacityBonus => equipped?.Benefit == EquipmentBenefit.LineCapacity ? equipped.Amount : 0;
    public int OpeningDrawBonus => equipped?.Benefit == EquipmentBenefit.OpeningTechniques ? equipped.Amount : 0;
    public int RemainingCatchBonuses => equipped?.Benefit == EquipmentBenefit.FirstCatchValue ? Math.Max(0, equipped.CatchLimit - rewardedCatches) : 0;

    /// <summary>Starts a fresh equipment instance; changing a Harbor selection cannot affect an active run.</summary>
    public void BeginRun(TackleShopCatalog.Item item) { equipped = item; rewardedCatches = 0; attachedIds.Clear(); }

    /// <summary>Consumes charges for newly attached Catches, never for Bait or an existing card changing modes.</summary>
    public void BeforeAttachment(CardInstance card)
    {
        if (card == null || !attachedIds.Add(card.InstanceId)) return;
        if (!Eligible(card)) return;
        PreviewCatchBonus(card); rewardedCatches++;
    }

    /// <summary>Projects the next equipment grant without consuming its charge or changing any saved data.</summary>
    public void PreviewCatchBonus(CardInstance card)
    {
        if (!Eligible(card)) return;
        var source = equipped.SourceCard;
        card.AddPermanentModifiers(0, equipped.Amount, source.Effects[0], source);
    }

    /// <summary>Recognizes ordinary creatures and creature-like Apex in Catch mode.</summary>
    private bool Eligible(CardInstance card) { return RemainingCatchBonuses > 0 && card?.Definition?.RequiresAttachmentChoice == true && !card.IsBait; }

    /// <summary>Clears equipment state only after the result has captured all lasting card modifiers.</summary>
    public void Reset() { BeginRun(null); }
}
