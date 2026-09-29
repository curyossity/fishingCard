using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Keeps the project's stable TMP font assets connected to the approved source font files.
/// Existing regular-font asset GUIDs remain unchanged so authored prefab references stay valid.
/// </summary>
[InitializeOnLoad]
public static class TypographyAssetSynchronizer
{
    public const string DisplayFontAssetPath = "Assets/FishingUIAssets/Fonts/TMP/Marcellus SC SDF.asset";
    public const string BodyFontAssetPath = "Assets/FishingUIAssets/Fonts/TMP/Source Serif 4 SDF.asset";

    private const string DisplaySourcePath =
        "Assets/FishingUIAssets/Fonts/MarcellusSC/MarcellusSC-Regular.ttf";
    private const string BodyRegularSourcePath =
        "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Regular.ttf";
    private const string BodySemiboldSourcePath =
        "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Semibold.ttf";
    private const string BodyBoldSourcePath =
        "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Bold.ttf";
    private const string BodySemiboldAssetPath =
        "Assets/FishingUIAssets/Fonts/TMP/Source Serif 4 Semibold SDF.asset";
    private const string BodyBoldAssetPath =
        "Assets/FishingUIAssets/Fonts/TMP/Source Serif 4 Bold SDF.asset";

    static TypographyAssetSynchronizer()
    {
        EditorApplication.delayCall += SynchronizeIfReady;
    }

    [MenuItem("Fishing Cards/UI/Synchronize Typography Fonts")]
    public static void Synchronize()
    {
        TMP_FontAsset display = SynchronizeStableAsset(
            DisplaySourcePath,
            DisplayFontAssetPath,
            "Marcellus SC SDF");
        TMP_FontAsset bodyRegular = SynchronizeStableAsset(
            BodyRegularSourcePath,
            BodyFontAssetPath,
            "Source Serif 4 SDF");
        TMP_FontAsset bodySemibold = EnsureFontAsset(
            BodySemiboldSourcePath,
            BodySemiboldAssetPath,
            "Source Serif 4 Semibold SDF");
        TMP_FontAsset bodyBold = EnsureFontAsset(
            BodyBoldSourcePath,
            BodyBoldAssetPath,
            "Source Serif 4 Bold SDF");

        LinkWeight(bodyRegular, FontWeight.SemiBold, bodySemibold);
        LinkWeight(bodyRegular, FontWeight.Bold, bodyBold);
        ConfigureDisplayMaterial(display);

        EditorUtility.SetDirty(display);
        EditorUtility.SetDirty(bodyRegular);
        AssetDatabase.SaveAssets();
        Debug.Log("Fishing Cards typography fonts synchronized.");
    }

    private static void ConfigureDisplayMaterial(TMP_FontAsset display)
    {
        Material material = display.material;
        if (material == null)
        {
            return;
        }

        material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(9, 44, 43, 150));
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.06f);
        EditorUtility.SetDirty(material);
    }

    private static void SynchronizeIfReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += SynchronizeIfReady;
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Font>(DisplaySourcePath) != null
            && AssetDatabase.LoadAssetAtPath<Font>(BodyRegularSourcePath) != null)
        {
            Synchronize();
        }
    }

    private static TMP_FontAsset SynchronizeStableAsset(
        string sourcePath,
        string assetPath,
        string assetName)
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (source == null)
        {
            throw new FileNotFoundException("Typography source font is missing.", sourcePath);
        }

        TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (asset == null)
        {
            return EnsureFontAsset(sourcePath, assetPath, assetName);
        }

        string expectedGuid = AssetDatabase.AssetPathToGUID(sourcePath);
        SerializedObject serializedAsset = new SerializedObject(asset);
        SerializedProperty sourceReference = serializedAsset.FindProperty("m_SourceFontFile");
        SerializedProperty sourceGuid = serializedAsset.FindProperty("m_SourceFontFileGUID");
        bool sourceChanged = sourceReference.objectReferenceValue != source
            || sourceGuid.stringValue != expectedGuid;

        if (sourceChanged)
        {
            asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            PropertyInfo editorSourceProperty = typeof(TMP_FontAsset).GetProperty(
                "SourceFont_EditorRef",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (editorSourceProperty == null)
            {
                throw new MissingMemberException(
                    typeof(TMP_FontAsset).FullName,
                    "SourceFont_EditorRef");
            }

            editorSourceProperty.SetValue(asset, source);
            serializedAsset.Update();
            SerializedProperty creationSettings = serializedAsset.FindProperty("m_CreationSettings");
            creationSettings.FindPropertyRelative("sourceFontFileName").stringValue = source.name;
            creationSettings.FindPropertyRelative("sourceFontFileGUID").stringValue = expectedGuid;
            serializedAsset.ApplyModifiedPropertiesWithoutUndo();
            asset.ClearFontAssetData(true);
        }

        asset.name = assetName;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static TMP_FontAsset EnsureFontAsset(
        string sourcePath,
        string assetPath,
        string assetName)
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (existing != null)
        {
            return existing;
        }

        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (source == null)
        {
            throw new FileNotFoundException("Typography source font is missing.", sourcePath);
        }

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(source);
        if (asset == null)
        {
            throw new UnityException("Could not create TMP font asset at " + assetPath);
        }

        asset.name = assetName;
        asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        AssetDatabase.CreateAsset(asset, assetPath);
        AddSubAssetIfNeeded(asset.material, asset, assetName + " Material");
        AddSubAssetIfNeeded(asset.atlasTexture, asset, assetName + " Atlas");
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void AddSubAssetIfNeeded(UnityEngine.Object subAsset, TMP_FontAsset owner, string name)
    {
        if (subAsset == null || AssetDatabase.Contains(subAsset))
        {
            return;
        }

        subAsset.name = name;
        AssetDatabase.AddObjectToAsset(subAsset, owner);
    }

    private static void LinkWeight(
        TMP_FontAsset regular,
        FontWeight weight,
        TMP_FontAsset alternate)
    {
        SerializedObject serializedAsset = new SerializedObject(regular);
        SerializedProperty table = serializedAsset.FindProperty("m_FontWeightTable");
        int index = ((int)weight / 100) - 1;
        if (index < 0 || index >= table.arraySize)
        {
            throw new UnityException("Unsupported TMP font weight: " + weight);
        }

        SerializedProperty pair = table.GetArrayElementAtIndex(index);
        pair.FindPropertyRelative("regularTypeface").objectReferenceValue = alternate;
        serializedAsset.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(regular);
    }
}
