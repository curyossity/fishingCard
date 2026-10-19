using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Keeps the approved independent tooltip master bound during candidate-theme regeneration.</summary>
public static class JournalInspectionDesignSync
{
    public const string FramePath = MidnightFieldJournalAssetSetup.Root + "Art/Tooltips/EngravedTooltipFrame.png";
    public const string FrameHash = "6c79df49babc1fe030be462fde762e8fd43bfb2d99ebce5ca33c97c137c4bd66";

    /// <summary>Checks provenance, preserves native alpha and binds the frame without importing preview composites.</summary>
    public static void Bind(MidnightFieldJournalTheme theme)
    {
        using (var stream = File.OpenRead(FramePath)) using (var sha = SHA256.Create())
            if (BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant() != FrameHash) throw new InvalidOperationException("Tooltip source hash mismatch.");
        AssetDatabase.ImportAsset(FramePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(FramePath);
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
        }
        var so = new SerializedObject(theme); so.FindProperty("catchTooltipFrame").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
        so.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(theme);
    }
}
