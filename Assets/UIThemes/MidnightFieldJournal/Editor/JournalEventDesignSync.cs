using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Imports exact event sources and restores their central-only mappings during theme regeneration.</summary>
public static class JournalEventDesignSync
{
    [Serializable] private sealed class Catalog { public Source[] centralEvents; }
    [Serializable] private sealed class Source
    {
        public string cardUniqueId,path,sha256;
        public BoundsData portraitBounds;
        public float titleSize;
        public int[] descriptionLineBreaks;
    }
    [Serializable] private sealed class BoundsData
    {
        public float x,y,width,height;
        public Rect Value => new Rect(x,y,width,height);
    }

    /// <summary>Imports catalog-verified event art and regenerates candidate-only prefabs.</summary>
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Import outside Play Mode.");
        foreach(var source in Load().centralEvents)Import(source.path,source.sha256,true);
        foreach(var source in MidnightFieldJournalAssetSetup.LoadCatalog().sprites)
            if(source.id=="EventTemplate")Import(source.path,source.sha256,false);
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        BindEvents(theme);
        var so=new SerializedObject(theme);var sprites=so.FindProperty("sprites");
        foreach(var source in MidnightFieldJournalAssetSetup.LoadCatalog().sprites)
        {
            if(source.id!="EventTemplate")continue;
            for(int i=0;i<sprites.arraySize;i++)
            {
                var entry=sprites.GetArrayElementAtIndex(i);
                if(entry.FindPropertyRelative("sourceId").stringValue!="EventTemplate")continue;
                entry.FindPropertyRelative("sourceHash").stringValue=source.sha256;
                entry.FindPropertyRelative("designCanvas").vector2Value=new Vector2(1260,1400);
                entry.FindPropertyRelative("visibleBoundsTopLeft").rectValue=new Rect(0,0,1,1);
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(theme);
        MidnightFieldJournalPrefabBuilder.Build();
        Debug.Log("Journal event sources and reusable event presentation synchronized.");
    }

    /// <summary>Restores event mappings without replacing creature, compact-chain or Technique entries.</summary>
    public static void BindEvents(MidnightFieldJournalTheme theme)
    {
        var sources=Load().centralEvents;if(sources==null)return;
        var so=new SerializedObject(theme);var entries=so.FindProperty("centralEvents");entries.arraySize=sources.Length;
        for(int i=0;i<sources.Length;i++)
        {
            var source=sources[i];var entry=entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("cardUniqueId").stringValue=source.cardUniqueId;
            entry.FindPropertyRelative("sprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>(source.path);
            entry.FindPropertyRelative("portraitBounds").rectValue=source.portraitBounds.Value;
            entry.FindPropertyRelative("titleSize").floatValue=source.titleSize;
            var breaks=entry.FindPropertyRelative("descriptionLineBreaks");breaks.arraySize=source.descriptionLineBreaks.Length;
            for(int j=0;j<breaks.arraySize;j++)breaks.GetArrayElementAtIndex(j).intValue=source.descriptionLineBreaks[j];
        }
        var missing=MidnightFieldJournalAssetSetup.LoadCatalog().unillustratedCardIds;
        var absent=so.FindProperty("unillustratedCardIds");absent.arraySize=missing.Length;
        for(int i=0;i<missing.Length;i++)absent.GetArrayElementAtIndex(i).stringValue=missing[i];
        so.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(theme);
    }

    /// <summary>Reads the source inventory used by both selective import and ordinary regeneration.</summary>
    private static Catalog Load() { return JsonUtility.FromJson<Catalog>(File.ReadAllText(MidnightFieldJournalAssetSetup.Root+"Profiles/SourceAssetCatalog.json")); }

    /// <summary>Preserves complete alpha bounds and source resolution after verifying the recorded image hash.</summary>
    private static void Import(string path,string expected,bool mipmaps)
    {
        using(var stream=File.OpenRead(path))using(var hash=SHA256.Create())
            if(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant()!=expected)throw new InvalidOperationException("Event source hash mismatch: "+path);
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
