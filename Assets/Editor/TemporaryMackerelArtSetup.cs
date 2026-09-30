using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>Configures the newly supplied mackerel overlay and connects it to its card definition.</summary>
[InitializeOnLoad]
internal static class TemporaryMackerelArtSetup
{
    private const string ArtworkPath =
        "Assets/FishingUIAssets/Cards/Creature/mackerel-overlay-v2-1101x1429.png";
    private const string CardPath = "Assets/Cards/Golden Mackerel.asset";
    private const string CompletedKey = "FishingCards.MackerelArtSetup.Completed";

    static TemporaryMackerelArtSetup()
    {
        EditorApplication.delayCall += ConfigureWhenReady;
    }

    /// <summary>Creates full and cropped sprites, then assigns them to Golden Mackerel.</summary>
    private static void ConfigureWhenReady()
    {
        if (SessionState.GetBool(CompletedKey, false))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ConfigureWhenReady;
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(ArtworkPath) as TextureImporter;
        CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(CardPath);
        if (importer == null || card == null)
        {
            Debug.LogError("Mackerel artwork setup could not find its imported texture or card definition.");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.maxTextureSize = 2048;

        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        dataProvider.SetSpriteRects(new[]
        {
            CreateSpriteRect("mackerel-overlay-full", new Rect(0f, 0f, 1101f, 1429f)),
            CreateSpriteRect("mackerel-overlay-cropped", new Rect(156f, 769f, 790f, 335f))
        });
        dataProvider.Apply();
        importer.SaveAndReimport();

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(ArtworkPath).OfType<Sprite>().ToArray();
        Sprite full = Array.Find(sprites, sprite => sprite.name == "mackerel-overlay-full");
        Sprite cropped = Array.Find(sprites, sprite => sprite.name == "mackerel-overlay-cropped");
        if (full == null || cropped == null)
        {
            Debug.LogError("Mackerel artwork sprites were not created correctly.");
            return;
        }

        SerializedObject cardObject = new SerializedObject(card);
        cardObject.FindProperty("artwork").objectReferenceValue = cropped;
        cardObject.FindProperty("encounterArtwork").objectReferenceValue = full;
        cardObject.FindProperty("artworkLayout").enumValueIndex = (int)CardArtworkLayout.FullCardOverlay;
        cardObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(card);
        AssetDatabase.SaveAssets();
        SessionState.SetBool(CompletedKey, true);
        Debug.Log("Golden Mackerel artwork configured for the encounter card and Catch Chain.");
    }

    /// <summary>Creates one centered sprite definition for the imported overlay.</summary>
    private static SpriteRect CreateSpriteRect(string name, Rect rect)
    {
        return new SpriteRect
        {
            name = name,
            rect = rect,
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f),
            border = Vector4.zero,
            spriteID = GUID.Generate()
        };
    }
}
