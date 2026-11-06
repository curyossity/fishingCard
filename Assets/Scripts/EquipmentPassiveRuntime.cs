using System;
using System.Collections.Generic;

/// <summary>Owns expedition-only passive counters and applies authored behaviors independently of item identity.</summary>
public sealed class EquipmentPassiveRuntime
{
    private sealed class Passive
    {
        public TackleShopCatalog.Item Item;
        public EquipmentPassiveEffect Definition;
        public CardEffectDefinition Origin;
        public bool EncounterDrawUsed;
    }
    private readonly List<Passive> passives = new List<Passive>();
    private readonly HashSet<int> attachedIds = new HashSet<int>();
    private readonly HashSet<int> baitReactionIds = new HashSet<int>();
    private Func<int> capacity = () => 0;
    private int attachmentCount, catchCount;
    public int CreatureAttachmentCount => attachmentCount;
    public int CatchAttachmentCount => catchCount;

    /// <summary>Binds capacity without putting controller references into authored definitions.</summary>
    public void BindCapacity(Func<int> getCapacity) { capacity = getCapacity ?? throw new ArgumentNullException(nameof(getCapacity)); }

    /// <summary>Starts a single-item fixture through the category-loadout path.</summary>
    public void BeginRun(TackleShopCatalog.Item item) { BeginLoadout(item == null ? Array.Empty<TackleShopCatalog.Item>() : new[] { item }); }

    /// <summary>Snapshots passives and resets counters independently of saved progression.</summary>
    public void BeginLoadout(TackleShopCatalog.Item[] items)
    {
        passives.Clear(); attachedIds.Clear(); baitReactionIds.Clear(); attachmentCount = catchCount = 0;
        foreach (var item in items ?? Array.Empty<TackleShopCatalog.Item>())
        {
            var effects = item.PassiveEffects;
            if (effects.Length == 0 || item.SourceCard == null || item.SourceCard.Effects.Length != effects.Length)
                throw new ArgumentException("Passive equipment requires matching authored effects and provenance.");
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == null || effects[i].Amount < 1 || effects[i].Threshold < 1 || item.SourceCard.Effects[i] == null
                    || !Enum.IsDefined(typeof(EquipmentPassiveKind), effects[i].Kind))
                    throw new ArgumentException("Equipment passives require a known behavior, positive amount/threshold and provenance.");
                passives.Add(new Passive { Item = item, Definition = effects[i], Origin = item.SourceCard.Effects[i] });
            }
        }
    }

    /// <summary>Counts new physical attachments once; mode changes never spend counters.</summary>
    public void BeforeAttachment(CardInstance card)
    {
        if (!IsCreature(card) || !attachedIds.Add(card.InstanceId)) return;
        ApplyGrants(card, attachmentCount + 1, catchCount + (card.IsBait ? 0 : 1));
        attachmentCount++; if (!card.IsBait) catchCount++;
    }

    /// <summary>Projects the next attachment without consuming counters.</summary>
    public void PreviewCatchBonus(CardInstance card)
    {
        if (IsCreature(card)) ApplyGrants(card, attachmentCount + 1, catchCount + (card.IsBait ? 0 : 1));
    }

    /// <summary>Stores lasting, source-attributed grants on the qualifying recipient.</summary>
    private void ApplyGrants(CardInstance card, int attachmentOrdinal, int catchOrdinal)
    {
        foreach (var passive in passives)
        {
            var effect = passive.Definition;
            if (effect.Kind == EquipmentPassiveKind.ReduceFirstAttachmentWeights && attachmentOrdinal <= effect.Threshold)
                card.AddPermanentModifiers(-effect.Amount, 0, passive.Origin, passive.Item.SourceCard);
            else if (effect.Kind == EquipmentPassiveKind.ValueEveryNthCatch && !card.IsBait && catchOrdinal % effect.Threshold == 0)
                card.AddPermanentModifiers(0, effect.Amount, passive.Origin, passive.Item.SourceCard);
        }
    }

    /// <summary>Rebuilds live weight conditions before evaluating conditional value bonuses.</summary>
    public void ResolvePassiveStats(CardInstance[] catches)
    {
        var creatures = Array.FindAll(catches, IsCreature);
        int baitCount = Array.FindAll(creatures, card => card.IsBait).Length;
        foreach (var passive in passives)
        {
            var effect = passive.Definition;
            if (effect.Kind == EquipmentPassiveKind.ReduceHeaviestWeight)
            {
                CardInstance target = null;
                // Original weight then chain order prevents a reduction from making its target oscillate.
                foreach (var card in creatures) if (target == null || card.Definition.Weight > target.Definition.Weight) target = card;
                target?.ModifyWeight(-effect.Amount, passive.Item.SourceCard, passive.Origin);
            }
            else if (effect.Kind == EquipmentPassiveKind.ReduceBaitWeightsWithMinimumCount && baitCount >= effect.Threshold)
                foreach (var card in creatures) if (card.IsBait) card.ModifyWeight(-effect.Amount, passive.Item.SourceCard, passive.Origin);
        }
        int load = 0; foreach (var card in catches) if (card != null) load += card.CurrentWeight;
        foreach (var passive in passives)
        {
            var effect = passive.Definition;
            bool enabled = effect.Kind == EquipmentPassiveKind.CatchValueWhileOverloaded && load > capacity()
                || effect.Kind == EquipmentPassiveKind.CatchValueWithMinimumBaitCount && baitCount >= effect.Threshold;
            if (enabled) foreach (var card in creatures) if (!card.IsBait) card.ModifyValue(effect.Amount, passive.Item.SourceCard, passive.Origin);
        }
    }

    /// <summary>Runs equipment draws after the caller's normal biome-start draw.</summary>
    public int BeginBiome(TechniqueDeckRuntime deck, System.Random random)
    {
        int drawn = 0;
        foreach (var passive in passives) if (passive.Definition.Kind == EquipmentPassiveKind.DrawAtBiomeStart)
            drawn += deck.DrawTechniques(passive.Definition.Amount, random);
        return drawn;
    }

    /// <summary>Resets opportunities on advancement, never on Hooked-card replacement.</summary>
    public void BeginEncounter() { foreach (var passive in passives) passive.EncounterDrawUsed = false; }

    /// <summary>Checks the hand after all Technique reactions; an empty-pile attempt spends the opportunity.</summary>
    public int CompleteTechnique(TechniqueDeckRuntime deck, System.Random random)
    {
        int drawn = 0;
        foreach (var passive in passives)
        {
            if (passive.Definition.Kind != EquipmentPassiveKind.DrawAfterEmptyHandTechnique || passive.EncounterDrawUsed || deck.Hand.Length != 0) continue;
            passive.EncounterDrawUsed = true; drawn += deck.DrawTechniques(passive.Definition.Amount, random);
        }
        return drawn;
    }

    /// <summary>Queues filtering after a new Bait's own effect requests, independent of subsequent removal.</summary>
    public void RecordBaitAttachment(CardInstance card, CreatureEffectRuntime reactions)
    {
        if (!IsCreature(card) || !card.IsBait || !baitReactionIds.Add(card.InstanceId)) return;
        foreach (var passive in passives) if (passive.Definition.Kind == EquipmentPassiveKind.FilterAfterBaitAttachment)
            reactions.QueueEquipmentFilter(passive.Item.SourceCard, passive.Definition.Amount);
    }

    /// <summary>Counts authored first-biome extra draws for the loadout preview without executing them.</summary>
    public static int OpeningDrawCount(TackleShopCatalog.Item[] items)
    {
        int count = 0;
        foreach (var item in items ?? Array.Empty<TackleShopCatalog.Item>()) foreach (var effect in item.PassiveEffects)
            if (effect.Kind == EquipmentPassiveKind.DrawAtBiomeStart) count += effect.Amount;
        return count;
    }

    /// <summary>Recognizes ordinary creatures and creature-like Apex, excluding treasures and events.</summary>
    private static bool IsCreature(CardInstance card) { return card?.Definition?.RequiresAttachmentChoice == true; }
    /// <summary>Clears live ownership after the result has captured its modifiers.</summary>
    public void Reset() { BeginLoadout(null); }
}
