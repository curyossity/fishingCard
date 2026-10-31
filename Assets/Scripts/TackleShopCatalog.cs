using System;
using UnityEngine;

public enum EquipmentBenefit { LineCapacity, OpeningTechniques, FirstCatchValue }

/// <summary>Authored prices and equipment benefits; ownership and expedition counters live elsewhere.</summary>
public sealed class TackleShopCatalog : ScriptableObject
{
    [Serializable] public sealed class Item
    {
        [SerializeField] private string id, displayName, category, description;
        [SerializeField] private int price, amount, catchLimit;
        [SerializeField] private EquipmentBenefit benefit;
        [SerializeField] private CardDefinition sourceCard;
        public string Id => id;
        public string DisplayName => displayName;
        public string Category => category;
        public string Description => description;
        public int Price => price;
        public int Amount => amount;
        public int CatchLimit => catchLimit;
        public EquipmentBenefit Benefit => benefit;
        public CardDefinition SourceCard => sourceCard;
    }
    [SerializeField] private Item[] items = Array.Empty<Item>();
    public Item[] Items => (Item[])items.Clone();

    /// <summary>Looks up an authored stable identity, never trusting UI prices or effect amounts.</summary>
    public Item Find(string id) { return Array.Find(items, item => item.Id == id); }

    /// <summary>Rejects malformed equipment before it can enter a purchase or expedition.</summary>
    public void Validate()
    {
        var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id) || item.Price < 0 || item.Amount < 1
                || !Enum.IsDefined(typeof(EquipmentBenefit), item.Benefit) || item.SourceCard?.CardType != CardType.Equipment
                || item.Benefit == EquipmentBenefit.FirstCatchValue && (item.CatchLimit < 1 || item.SourceCard.Effects.Length != 1 || item.SourceCard.Effects[0] == null))
                throw new InvalidOperationException("Invalid Tackle Shop equipment catalog.");
    }
}
