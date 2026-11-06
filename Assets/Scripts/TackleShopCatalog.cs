using System;
using UnityEngine;

public enum EquipmentSlot { Rod, Reel, Hook }

/// <summary>Authored prices and equipment benefits; ownership and expedition counters live elsewhere.</summary>
public sealed class TackleShopCatalog : ScriptableObject
{
    [Serializable] public sealed class Item
    {
        [SerializeField] private string id, displayName, category, description;
        [SerializeField] private int price;
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private CardDefinition sourceCard;
        [SerializeField] private EquipmentPassiveEffect[] passiveEffects = Array.Empty<EquipmentPassiveEffect>();
        public string Id => id;
        public string DisplayName => displayName;
        public string Category => category;
        public string Description => description;
        public int Price => price;
        public EquipmentSlot Slot => slot;
        public CardDefinition SourceCard => sourceCard;
        public EquipmentPassiveEffect[] PassiveEffects => passiveEffects == null ? Array.Empty<EquipmentPassiveEffect>() : (EquipmentPassiveEffect[])passiveEffects.Clone();
    }
    [SerializeField] private Item[] items = Array.Empty<Item>();
    public Item[] Items => (Item[])items.Clone();

    /// <summary>Looks up an authored stable identity, never trusting UI prices or effect amounts.</summary>
    public Item Find(string id) { return Array.Find(items, item => item.Id == id); }

    /// <summary>Rejects malformed equipment before it can enter a purchase or expedition.</summary>
    public void Validate()
    {
        if (items == null || items.Length == 0) throw new InvalidOperationException("Equipment catalog must contain an item.");
        var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id) || item.Price < 0
                || !Enum.IsDefined(typeof(EquipmentSlot), item.Slot) || item.SourceCard?.CardType != CardType.Equipment
                || item.PassiveEffects.Length == 0 || item.SourceCard.Effects.Length != item.PassiveEffects.Length)
                throw new InvalidOperationException("Invalid Tackle Shop equipment catalog.");
            var effects = item.PassiveEffects;
            for (int i = 0; i < effects.Length; i++)
                if (effects[i] == null || !Enum.IsDefined(typeof(EquipmentPassiveKind), effects[i].Kind)
                    || effects[i].Amount < 1 || effects[i].Threshold < 1 || item.SourceCard.Effects[i] == null)
                    throw new InvalidOperationException("Invalid equipment passive: " + item.Id);
        }
    }
}
