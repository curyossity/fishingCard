using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the reconciled central-only sources and keeps theme regeneration aligned with the canonical catalog.</summary>
public static class JournalDesignSync
{
    [Serializable] private sealed class Catalog { public Source[] centralCards; }
    [Serializable] private sealed class BoundsData
    {
        public float x,y,width,height;
        public Rect Value => new Rect(x,y,width,height);
    }
    [Serializable] private sealed class Source
    {
        public string cardUniqueId,path,sha256;
        public BoundsData portraitBounds,catchNameBounds,catchTextBounds,baitNameBounds,baitTextBounds;
        public float catchNameSize,catchTextSize,baitNameSize,baitTextSize;
    }

    /// <summary>Imports verified images and rebuilds only candidate presentation assets, never gameplay definitions.</summary>
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Import outside Play Mode.");
        var catalog=Load();
        foreach(var c in catalog.centralCards)
        {
            using(var stream=File.OpenRead(c.path))using(var hash=SHA256.Create())
                if(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant()!=c.sha256)throw new InvalidOperationException("Central source hash mismatch: "+c.path);
            Import(c.path,true);
        }
        var source=MidnightFieldJournalAssetSetup.LoadCatalog();
        foreach(var c in source.sprites)if(c.id=="CreatureTemplate")Import(c.path,false);
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        BindCentral(theme);
        var so=new SerializedObject(theme);var sprites=so.FindProperty("sprites");
        foreach(var c in source.sprites)
            for(int i=0;i<sprites.arraySize;i++)
            {
                var e=sprites.GetArrayElementAtIndex(i);if(e.FindPropertyRelative("sourceId").stringValue!=c.id)continue;
                e.FindPropertyRelative("sourceHash").stringValue=c.sha256;
                e.FindPropertyRelative("designCanvas").vector2Value=new Vector2(c.designWidth,c.designHeight);
                if(c.id=="CreatureTemplate")e.FindPropertyRelative("visibleBoundsTopLeft").rectValue=new Rect(0,0,1,1);
            }
        so.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(theme);
        MidnightFieldJournalPrefabBuilder.Build();
        Debug.Log("Journal Sync001 imported the frame, 18 central portraits and all pending presentation revisions.");
    }

    /// <summary>Rebinds the catalog's central entries after normal theme regeneration without affecting compact maps.</summary>
    public static void BindCentral(MidnightFieldJournalTheme theme)
    {
        var catalog=Load();if(catalog.centralCards==null)return;
        var so=new SerializedObject(theme);var entries=so.FindProperty("centralCards");entries.arraySize=catalog.centralCards.Length;
        for(int i=0;i<entries.arraySize;i++)
        {
            var c=catalog.centralCards[i];var e=entries.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("cardUniqueId").stringValue=c.cardUniqueId;e.FindPropertyRelative("sprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(c.path);
            e.FindPropertyRelative("portraitBounds").rectValue=c.portraitBounds.Value;
            e.FindPropertyRelative("catchNameBounds").rectValue=c.catchNameBounds.Value;e.FindPropertyRelative("catchTextBounds").rectValue=c.catchTextBounds.Value;
            e.FindPropertyRelative("baitNameBounds").rectValue=c.baitNameBounds.Value;e.FindPropertyRelative("baitTextBounds").rectValue=c.baitTextBounds.Value;
            e.FindPropertyRelative("catchNameSize").floatValue=c.catchNameSize;e.FindPropertyRelative("catchTextSize").floatValue=c.catchTextSize;
            e.FindPropertyRelative("baitNameSize").floatValue=c.baitNameSize;e.FindPropertyRelative("baitTextSize").floatValue=c.baitTextSize;
        }
        var missing=MidnightFieldJournalAssetSetup.LoadCatalog().unillustratedCardIds;
        var absent=so.FindProperty("unillustratedCardIds");absent.arraySize=missing.Length;
        for(int i=0;i<missing.Length;i++)absent.GetArrayElementAtIndex(i).stringValue=missing[i];
        so.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(theme);
    }

    /// <summary>Loads the checked-in source inventory as the authoritative regeneration input.</summary>
    private static Catalog Load() { return JsonUtility.FromJson<Catalog>(File.ReadAllText(MidnightFieldJournalAssetSetup.Root+"Profiles/SourceAssetCatalog.json")); }

    /// <summary>Keeps full alpha bounds, native source pixels and non-readable uncompressed sprites.</summary>
    private static void Import(string path,bool mipmaps)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.isReadable=false;
        importer.alphaIsTransparency=true;importer.sRGBTexture=true;importer.mipmapEnabled=mipmaps;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=mipmaps?FilterMode.Trilinear:FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
    }
}
