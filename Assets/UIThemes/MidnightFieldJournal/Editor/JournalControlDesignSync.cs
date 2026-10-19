using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Imports recorded control and Technique sources without rebuilding fonts or touching gameplay data.</summary>
public static class JournalControlDesignSync
{
    private const string Evidence="Docs/UIThemes/MidnightFieldJournal/Sync004/";
    [Serializable] private sealed class Plan { public Input[] sources; }
    [Serializable] private sealed class Input { public string id,path,cardUniqueId,sha256; public float designWidth,designHeight; public bool mipmaps; }
    [Serializable] private sealed class Metadata { public List<MidnightFieldJournalAssetSetup.Source> sprites=new List<MidnightFieldJournalAssetSetup.Source>(); }

    /// <summary>Verifies approved bytes, preserves full alpha and records exact source bounds for the catalog.</summary>
    public static void ImportSources(string evidence = Evidence)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Import outside Play Mode.");
        var plan=JsonUtility.FromJson<Plan>(File.ReadAllText(evidence+"ImportPlan.json"));var report=new Metadata();
        foreach(var s in plan.sources)
        {
            byte[] bytes=File.ReadAllBytes(s.path);
            using(var sha=SHA256.Create())if(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()!=s.sha256)throw new InvalidOperationException("Source hash mismatch: "+s.path);
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!ImageConversion.LoadImage(texture,bytes))throw new InvalidOperationException("Unreadable source: "+s.path);
                int min=255,max=0,left=texture.width,top=texture.height,right=0,bottom=0,hl=left,ht=top,hr=0,hb=0;
                var pixels=texture.GetPixels32();
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                {
                    int a=pixels[y*texture.width+x].a,ty=texture.height-1-y;min=Math.Min(min,a);max=Math.Max(max,a);
                    if(a>0){left=Math.Min(left,x);top=Math.Min(top,ty);right=Math.Max(right,x+1);bottom=Math.Max(bottom,ty+1);}
                    if(a>=128){hl=Math.Min(hl,x);ht=Math.Min(ht,ty);hr=Math.Max(hr,x+1);hb=Math.Max(hb,ty+1);}
                }
                report.sprites.Add(new MidnightFieldJournalAssetSetup.Source{id=s.id,path=s.path,cardUniqueId=s.cardUniqueId,sha256=s.sha256,width=texture.width,height=texture.height,alphaMin=min,alphaMax=max,visibleBoundsPixels=new[]{left,top,right,bottom},halfAlphaBoundsPixels=new[]{hl,ht,hr,hb},designWidth=s.designWidth,designHeight=s.designHeight});
            }
            finally{UnityEngine.Object.DestroyImmediate(texture);}
            AssetDatabase.ImportAsset(s.path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(s.path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=4096;importer.isReadable=false;importer.alphaIsTransparency=true;importer.sRGBTexture=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=s.mipmaps;importer.filterMode=s.mipmaps?FilterMode.Trilinear:FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        File.WriteAllText(evidence+"ImportedSourceMetadata.json",JsonUtility.ToJson(report,true));
    }

    /// <summary>Restores the canonical sprite mapping while preserving existing portrait and Technique framing.</summary>
    public static void Bind(MidnightFieldJournalTheme theme)
    {
        var catalog=MidnightFieldJournalAssetSetup.LoadCatalog();var data=new SerializedObject(theme);var entries=data.FindProperty("sprites");entries.arraySize=catalog.sprites.Length;
        for(int i=0;i<entries.arraySize;i++)
        {
            var s=catalog.sprites[i];var e=entries.GetArrayElementAtIndex(i);var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(s.path);
            if(sprite==null)throw new InvalidOperationException("Missing catalog sprite: "+s.path);
            e.FindPropertyRelative("sourceId").stringValue=s.id;e.FindPropertyRelative("cardUniqueId").stringValue=s.cardUniqueId;e.FindPropertyRelative("sourceHash").stringValue=s.sha256;
            e.FindPropertyRelative("sprite").objectReferenceValue=sprite;e.FindPropertyRelative("designCanvas").vector2Value=new Vector2(s.designWidth,s.designHeight);
            var b=s.visibleBoundsPixels;e.FindPropertyRelative("visibleBoundsTopLeft").rectValue=new Rect(b[0]/(float)s.width,b[1]/(float)s.height,(b[2]-b[0])/(float)s.width,(b[3]-b[1])/(float)s.height);
        }
        var absent=data.FindProperty("unillustratedCardIds");absent.arraySize=catalog.unillustratedCardIds.Length;for(int i=0;i<absent.arraySize;i++)absent.GetArrayElementAtIndex(i).stringValue=catalog.unillustratedCardIds[i];
        data.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(theme);
    }
}
