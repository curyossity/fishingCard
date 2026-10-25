using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the independent R045 resources and binds candidate numeric fields durably.</summary>
public static class JournalStatTooltipDesignSync
{
    private const string Root=MidnightFieldJournalAssetSetup.Root;
    public const string StylePath=Root+"Profiles/StatTooltipStyle.asset";

    /// <summary>Prepares the sliced frame and common category glyphs without changing other theme resources.</summary>
    public static JournalStatTooltipStyle Prepare()
    {
        var style=AssetDatabase.LoadAssetAtPath<JournalStatTooltipStyle>(StylePath);
        if(style==null){style=ScriptableObject.CreateInstance<JournalStatTooltipStyle>();AssetDatabase.CreateAsset(style,StylePath);}
        style.theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        style.frame=Import(Root+"Art/Tooltips/ModifierTooltipFrameV2.png",new Vector4(64,64,64,64));
        style.techniqueIcon=Import(Root+"Art/Tooltips/StatIcons/technique.png",Vector4.zero);
        style.eventIcon=Import(Root+"Art/Tooltips/StatIcons/event.png",Vector4.zero);
        style.creatureIcons=new JournalStatTooltipStyle.Icon[4];string[] names={"lionfish","moonlit_ray","jellyfish","spiny_lobster"};
        for(int i=0;i<names.Length;i++)style.creatureIcons[i]=new JournalStatTooltipStyle.Icon {creatureId="creature_"+names[i],sprite=Import(Root+"Art/Tooltips/StatIcons/"+names[i]+".png",Vector4.zero)};
        const string path=Root+"Profiles/StatSilhouette.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Fishing Cards/UI/Stat Silhouette"));AssetDatabase.CreateAsset(material,path);}
        style.silhouetteMaterial=material;EditorUtility.SetDirty(style);AssetDatabase.SaveAssetIfDirty(style);return style;
    }

    /// <summary>Configures uncompressed UI sprites with explicit borders and no unwanted texture rescaling.</summary>
    private static Sprite Import(string path,Vector4 border)
    {
        if(!File.Exists(path))throw new FileNotFoundException("Export the approved Penpot source first.",path);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;
        i.spritePixelsPerUnit=100;i.spriteBorder=border;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=4096;
        i.alphaIsTransparency=true;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;
        i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    /// <summary>Attaches inspection to existing TMP numbers while preserving parent card clicks and animation.</summary>
    public static void Bind(GameObject root)
    {
        var style=AssetDatabase.LoadAssetAtPath<JournalStatTooltipStyle>(StylePath);if(style==null)throw new InvalidOperationException("Prepare tooltip sources first.");
        UnityEngine.Object view=root.GetComponent<CreatureCardView>();if(view==null)view=root.GetComponent<CompactCatchCardView>();if(view==null)return;
        var data=new SerializedObject(view);
        foreach(string field in new[]{"weightText","valueText"})
        {
            var text=(TMP_Text)data.FindProperty(field).objectReferenceValue;if(text==null)continue;text.raycastTarget=true;
            var trigger=text.GetComponent<JournalStatInspectionTrigger>();if(trigger==null)trigger=text.gameObject.AddComponent<JournalStatInspectionTrigger>();
            var t=new SerializedObject(trigger);t.FindProperty("style").objectReferenceValue=style;
            t.FindProperty("stat").enumValueIndex=field=="weightText"?(int)CreatureStat.Weight:(int)CreatureStat.Value;t.ApplyModifiedPropertiesWithoutUndo();
            var focus=text.GetComponent<UnityEngine.UI.Selectable>();if(focus==null)focus=text.gameObject.AddComponent<UnityEngine.UI.Selectable>();
            focus.targetGraphic=text;focus.transition=UnityEngine.UI.Selectable.Transition.None;
        }
    }

    /// <summary>Updates only the two isolated candidate prefabs and preserves their existing GUIDs and links.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Import Modifier Tooltip R045")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Import outside Play Mode.");Prepare();
        foreach(string name in new[]{"CreatureCard","CompactCatchCard"})
        {
            string path=Root+"Prefabs/"+name+".prefab";GameObject root=PrefabUtility.LoadPrefabContents(path);
            try{Bind(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
    }
}
