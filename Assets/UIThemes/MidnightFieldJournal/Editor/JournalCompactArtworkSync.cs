using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Binds explicit compact-card coverage without changing central frames, gameplay definitions or approved compact overrides.</summary>
public static class JournalCompactArtworkSync
{
    [Serializable] private sealed class Catalog { public Portrait[] compactPortraits; }
    [Serializable] private sealed class Portrait
    {
        public string cardUniqueId, path;
        public float x = 64, y = 100, width = 552, height = 221;
    }

    /// <summary>Restores the compact identity/framing registry during ordinary theme regeneration.</summary>
    public static void Bind(MidnightFieldJournalTheme theme)
    {
        var catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText(MidnightFieldJournalAssetSetup.Root + "Profiles/SourceAssetCatalog.json"));
        if (catalog.compactPortraits == null) return;
        var data = new SerializedObject(theme); var entries = data.FindProperty("compactPortraits"); entries.arraySize = catalog.compactPortraits.Length;
        for (int i = 0; i < entries.arraySize; i++)
        {
            var source = catalog.compactPortraits[i]; var entry = entries.GetArrayElementAtIndex(i);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(source.path);
            if (sprite == null) throw new InvalidOperationException("Missing exact compact portrait: " + source.cardUniqueId);
            entry.FindPropertyRelative("cardUniqueId").stringValue = source.cardUniqueId;
            entry.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            entry.FindPropertyRelative("portraitBounds").rectValue = MidnightFieldJournalTheme.CompactBounds(source.x, source.y, source.width, source.height);
        }
        data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(theme);
    }
}
