using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Imports reusable item/passive definitions into GUID-stable shop and provenance assets.</summary>
public static class JournalEquipmentCatalogAuthoring
{
    public const string SourcePath = JournalTackleShopSetup.Root + "EquipmentDefinitions.json";
    [Serializable] private sealed class Source { public int version; public Row[] items; }
    [Serializable] private sealed class Row { public string id, name, slot, description; public int price = -1; public Effect[] effects; }
    [Serializable] private sealed class Effect { public string kind; public int amount, threshold; }

    /// <summary>Validates the complete source before updating authored assets; stable identities preserve purchases and equip saves.</summary>
    public static void Rebuild()
    {
        var source = JsonUtility.FromJson<Source>(File.ReadAllText(SourcePath));
        if (source == null || source.version != 1 || source.items == null || source.items.Length == 0) throw new InvalidOperationException("Invalid equipment source.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in source.items)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.id) || !ids.Add(row.id) || string.IsNullOrWhiteSpace(row.name)
                || !Enum.TryParse(row.slot, out EquipmentSlot slot) || !Enum.IsDefined(typeof(EquipmentSlot), slot)
                || row.price < 0 || row.effects == null || row.effects.Length == 0 || string.IsNullOrWhiteSpace(row.description))
                throw new InvalidOperationException("Malformed equipment item source.");
            foreach (var effect in row.effects)
                if (effect == null || !Enum.TryParse(effect.kind, out EquipmentPassiveKind kind) || !Enum.IsDefined(typeof(EquipmentPassiveKind), kind)
                    || effect.amount < 1 || effect.threshold < 1) throw new InvalidOperationException("Malformed passive: " + row.id);
            if (AssetDatabase.LoadAssetAtPath<Sprite>(ArtworkPath(slot)) == null) throw new InvalidOperationException("Missing category illustration: " + slot);
        }
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath);
        if (catalog == null) { catalog = ScriptableObject.CreateInstance<TackleShopCatalog>(); AssetDatabase.CreateAsset(catalog, JournalTackleShopSetup.CatalogPath); }
        var data = new SerializedObject(catalog); var items = data.FindProperty("items"); items.arraySize = source.items.Length;
        for (int i = 0; i < source.items.Length; i++)
        {
            var row = source.items[i]; var slot = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), row.slot);
            var origin = Provenance(row, slot); var item = items.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("id").stringValue = row.id; item.FindPropertyRelative("displayName").stringValue = row.name;
            item.FindPropertyRelative("category").stringValue = row.slot; item.FindPropertyRelative("description").stringValue = row.description;
            item.FindPropertyRelative("slot").enumValueIndex = (int)slot; item.FindPropertyRelative("price").intValue = row.price;
            item.FindPropertyRelative("sourceCard").objectReferenceValue = origin;
            var effects = item.FindPropertyRelative("passiveEffects"); effects.arraySize = row.effects.Length;
            for (int e = 0; e < row.effects.Length; e++)
            {
                var effect = effects.GetArrayElementAtIndex(e); effect.FindPropertyRelative("kind").enumValueIndex = (int)Enum.Parse(typeof(EquipmentPassiveKind), row.effects[e].kind);
                effect.FindPropertyRelative("amount").intValue = row.effects[e].amount; effect.FindPropertyRelative("threshold").intValue = row.effects[e].threshold;
            }
        }
        data.ApplyModifiedPropertiesWithoutUndo(); catalog.Validate(); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
    }

    /// <summary>Preserves the source identity used by existing modifiers and inspection tooltips.</summary>
    private static CardDefinition Provenance(Row row, EquipmentSlot slot)
    {
        string path = JournalTackleShopSetup.Root + row.id + ".asset"; var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
        if (card == null) { card = ScriptableObject.CreateInstance<CardDefinition>(); AssetDatabase.CreateAsset(card, path); }
        var data = new SerializedObject(card); data.FindProperty("uniqueId").stringValue = "equipment_" + row.id;
        data.FindProperty("displayName").stringValue = row.name; data.FindProperty("cardType").enumValueIndex = (int)CardType.Equipment;
        data.FindProperty("rulesText").stringValue = row.description; data.FindProperty("artwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ArtworkPath(slot));
        var effects = data.FindProperty("effects"); effects.arraySize = row.effects.Length;
        for (int i = 0; i < row.effects.Length; i++)
        {
            var authored = row.effects[i]; var kind = (EquipmentPassiveKind)Enum.Parse(typeof(EquipmentPassiveKind), authored.kind);
            var effect = effects.GetArrayElementAtIndex(i); effect.FindPropertyRelative("effectId").stringValue = "equipment_" + row.id + "_" + i;
            effect.FindPropertyRelative("displayName").stringValue = row.name;
            bool weight = kind == EquipmentPassiveKind.ReduceFirstAttachmentWeights || kind == EquipmentPassiveKind.ReduceHeaviestWeight || kind == EquipmentPassiveKind.ReduceBaitWeightsWithMinimumCount;
            effect.FindPropertyRelative("effectType").enumValueIndex = (int)(weight ? CardEffectType.RemoveLineLoadModifier : CardEffectType.ModifyCatchValue);
            effect.FindPropertyRelative("trigger").enumValueIndex = (int)CardEffectTrigger.WhileAttached;
            effect.FindPropertyRelative("target").enumValueIndex = (int)CardEffectTarget.Self;
            effect.FindPropertyRelative("effectTone").enumValueIndex = (int)CardEffectTone.Positive;
            effect.FindPropertyRelative("amount").intValue = authored.amount;
        }
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(card); AssetDatabase.SaveAssetIfDirty(card); return card;
    }

    /// <summary>Reuses existing approved category artwork until dedicated new-item illustrations are supplied.</summary>
    private static string ArtworkPath(EquipmentSlot slot) { return JournalHarborBuilder.Root + "Art/" + slot.ToString().ToLowerInvariant() + ".png"; }
}
