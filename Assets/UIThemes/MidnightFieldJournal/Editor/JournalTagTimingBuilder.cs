using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Authors the approved engraved coastal instrument as independent reusable uGUI elements.</summary>
public static class JournalTagTimingBuilder
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    public const string ProfilePath = Root+"Profiles/TagTiming.asset";
    public const string PrefabPath = Root+"Prefabs/TagTiming.prefab";
    private static readonly Color Ink = new Color32(41,61,64,255), Paper = new Color32(238,224,195,255);

    /// <summary>Builds only tag assets and preserves existing authored configuration and GUIDs.</summary>
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<TagTimingProfile>(ProfilePath)==null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TagTimingProfile>(),ProfilePath);
        string art=Root+"Art/TagTiming/EngravedCoastal/";
        foreach(string name in new[]{"EngravedTagBarFrame","EngravedStopButton"})
        {
            string path=art+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null)throw new InvalidOperationException("Missing independent Penpot export: "+path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spriteBorder=Vector4.zero;importer.spritePixelsPerUnit=100;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=4096;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
        }
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        var root=new GameObject("Tag Timing Phase",typeof(RectTransform),typeof(CanvasGroup));
        var rt=root.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(3840,2160);
        try
        {
            var view=root.AddComponent<JournalTagTimingView>();var data=new SerializedObject(view);
            Label("Stage Heading",rt,new Rect(1120,470,1600,80),46,theme.DisplayFont,Paper).text="SOMETHING STIRS BELOW";
            Bind(data,"subtitle",Label("Destination",rt,new Rect(1120,565,1600,50),30,theme.BodyFont,Paper));
            var panel=Rect("Lantern Focus Instrument",rt,new Rect(1056,690,2160,720));
            // Keep the native Penpot geometry while fitting between the chain and action column.
            panel.localScale=Vector3.one*.8f;
            Picture("Independent Engraved Frame",panel,new Rect(0,0,2160,720),art+"EngravedTagBarFrame.png");
            data.FindProperty("precisionHeight").floatValue=90;
            data.FindProperty("markerTop").floatValue=224;
            data.FindProperty("burstTop").floatValue=500;
            data.FindProperty("stopFaceColor").colorValue=Color.white;
            Bind(data,"heading",Label("Heading",panel,new Rect(340,166,1130,52),36,theme.DisplayFont,Ink,TextAlignmentOptions.MidlineLeft));
            Bind(data,"phase",Label("Phase",panel,new Rect(1470,172,340,45),25,theme.DisplayFont,new Color32(149,115,77,255),TextAlignmentOptions.MidlineRight));
            var channel=Rect("Shared Parchment Channel",panel,new Rect(250,250,1660,200));
            foreach(float y in new[]{0f,200f})
            {var rule=Rect("Engraved Track Hairline",channel,new Rect(0,y,1660,1.5f)).gameObject.AddComponent<UnityEngine.UI.Image>();rule.color=new Color32(143,112,77,166);rule.raycastTarget=false;}
            var track=Rect("Dynamic Tag Zones",panel,new Rect(250,250,1660,200));
            var zones=data.FindProperty("zones");zones.arraySize=8;
            var areas=data.FindProperty("precisionAreas");areas.arraySize=8;
            var labels=data.FindProperty("labels");labels.arraySize=8;
            for(int i=0;i<8;i++)
            {
                var zone=Rect("Tag / "+CreatureTagVocabulary.All[i],track,new Rect(i*207.5f,0,207.5f,200));
                zone.anchorMin=new Vector2(i/8f,0);zone.anchorMax=new Vector2((i+1)/8f,1);zone.offsetMin=zone.offsetMax=Vector2.zero;
                var light=Graphic("Proportional Precision Light",zone,new Rect(0,110,37.35f,90),JournalTagGraphic.Drawing.Precision,new Color32(184,135,67,255));
                light.rectTransform.anchorMin=light.rectTransform.anchorMax=new Vector2(.5f,1);light.rectTransform.pivot=new Vector2(.5f,1);light.rectTransform.anchoredPosition=new Vector2(0,-110);
                var glyph=Graphic("Tag Glyph",zone,new Rect(0,28,50,50),(JournalTagGraphic.Drawing)i,Ink);
                glyph.rectTransform.anchorMin=glyph.rectTransform.anchorMax=new Vector2(.5f,1);glyph.rectTransform.pivot=new Vector2(.5f,1);glyph.rectTransform.anchoredPosition=new Vector2(0,-28);
                var label=Label("Tag Name",zone,new Rect(0,105,207.5f,44),29,theme.DisplayFont,Ink);label.text=CreatureTagVocabulary.All[i];
                label.rectTransform.anchorMin=new Vector2(0,1);label.rectTransform.anchorMax=new Vector2(1,1);label.rectTransform.sizeDelta=new Vector2(-8,44);
                label.rectTransform.anchoredPosition=new Vector2(4,-105);label.enableAutoSizing=true;label.fontSizeMin=19;label.fontSizeMax=29;
                if(i>0){var rule=Rect("Quiet Divider",zone,new Rect(0,20,1,160)).gameObject.AddComponent<UnityEngine.UI.Image>();rule.color=new Color32(41,61,64,46);rule.raycastTarget=false;}
                zones.GetArrayElementAtIndex(i).objectReferenceValue=zone;areas.GetArrayElementAtIndex(i).objectReferenceValue=light.rectTransform;labels.GetArrayElementAtIndex(i).objectReferenceValue=label;
            }
            var burst=Rect("Stop Burst",panel,new Rect(250,500,70,70));burst.pivot=new Vector2(.5f,.5f);
            var rays=burst.gameObject.AddComponent<JournalRarityGraphic>();rays.drawing=JournalRarityGraphic.Drawing.Burst;rays.color=new Color32(149,115,77,200);rays.raycastTarget=false;Bind(data,"burst",burst);
            var marker=Graphic("Independent Anchor Cursor",panel,new Rect(250,224,70,286),JournalTagGraphic.Drawing.Marker,Ink);
            marker.rectTransform.pivot=new Vector2(.5f,1);Bind(data,"marker",marker.rectTransform);
            var instruction=Label("Instruction",panel,new Rect(430,552,930,48),31,theme.BodyFont,Paper,TextAlignmentOptions.MidlineLeft);
            instruction.enableAutoSizing=true;instruction.fontSizeMin=22;instruction.fontSizeMax=31;Bind(data,"instruction",instruction);
            var buttonRect=Rect("Stop Button",panel,new Rect(1420,510,400,400f/3));var face=buttonRect.gameObject.AddComponent<UnityEngine.UI.Image>();
            face.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(art+"EngravedStopButton.png");face.color=Color.white;
            var button=buttonRect.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=face;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color32(255,239,208,255);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color32(210,189,153,255);colors.disabledColor=new Color32(180,174,160,255);button.colors=colors;

            var emblem=Rect("Stop Emblem",buttonRect,new Rect(93,63,12,12)).gameObject.AddComponent<UnityEngine.UI.Image>();emblem.color=Ink;emblem.raycastTarget=false;
            var stopLabel=Label("Stop Label",buttonRect,new Rect(124,40,180,51),38,theme.DisplayFont,Ink);
            stopLabel.enableAutoSizing=true;stopLabel.fontSizeMin=28;stopLabel.fontSizeMax=38;
            Bind(data,"stopLabel",stopLabel);Bind(data,"stopButton",button);Bind(data,"visibility",root.GetComponent<CanvasGroup>());
            data.ApplyModifiedPropertiesWithoutUndo();root.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Replaces only the old timing instance, retaining every other authored canvas element.</summary>
    public static void BindCanvas(GameObject canvas)
    {
        var data=new SerializedObject(canvas.GetComponent<FishingRunView>());
        var old=data.FindProperty("encounterTimingView").objectReferenceValue as EncounterTimingView;
        if(old is JournalTagTimingView)return;
        if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var encounter=(RectTransform)data.FindProperty("encounterPanel").objectReferenceValue;
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),encounter.parent);
        var rt=instance.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
        Bind(data,"encounterTimingView",instance.GetComponent<JournalTagTimingView>());data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Enables tag timing only in the isolated journal candidate scene and canvas.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Integrate Engraved Tag Timing R059-R060")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Integrate outside Play Mode.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!=JournalComparisonSceneBuilder.ScenePath||scene.isDirty)throw new InvalidOperationException("Open the clean comparison scene before integration.");
        Build();string path=Root+"Prefabs/MidnightFieldJournalCanvas.prefab";var canvas=PrefabUtility.LoadPrefabContents(path);
        try{BindCanvas(canvas);PrefabUtility.SaveAsPrefabAsset(canvas,path);}finally{PrefabUtility.UnloadPrefabContents(canvas);}
        var data=new SerializedObject(UnityEngine.Object.FindAnyObjectByType<FishingRunController>());
        Bind(data,"tagTimingProfile",AssetDatabase.LoadAssetAtPath<TagTimingProfile>(ProfilePath));Bind(data,"rarityTimingProfile",null);data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }

    /// <summary>Creates top-left native geometry in the fixed 4K composition.</summary>
    private static RectTransform Rect(string name,Transform parent,Rect b)
    {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(b.x,-b.y);r.sizeDelta=b.size;return r;}
    /// <summary>Creates an explicitly themed live text field.</summary>
    private static TMP_Text Label(string name,Transform parent,Rect b,float size,TMP_FontAsset font,Color color,TextAlignmentOptions align=TextAlignmentOptions.Center)
    {var t=Rect(name,parent,b).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.text="";return t;}
    /// <summary>Creates a native independent ornament.</summary>
    private static JournalTagGraphic Graphic(string name,Transform parent,Rect b,JournalTagGraphic.Drawing drawing,Color color)
    {var g=Rect(name,parent,b).gameObject.AddComponent<JournalTagGraphic>();g.drawing=drawing;g.color=color;g.raycastTarget=false;return g;}
    /// <summary>Uses independent static source art without baking labels, glyphs or zones into it.</summary>
    private static void Picture(string name,Transform parent,Rect b,string path)
    {var g=Rect(name,parent,b).gameObject.AddComponent<UnityEngine.UI.Image>();g.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);g.type=UnityEngine.UI.Image.Type.Simple;g.raycastTarget=false;}
    /// <summary>Persists an explicit reference using Unity serialization.</summary>
    private static void Bind(SerializedObject data,string field,UnityEngine.Object value){data.FindProperty(field).objectReferenceValue=value;}
}
