using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Imports the approved blank R046 frame and authors separate editable timing components.</summary>
public static class JournalRarityTimingBuilder
{
    private const string Root=MidnightFieldJournalAssetSetup.Root;
    public const string ProfilePath=Root+"Profiles/RarityTiming.asset";
    public const string PrefabPath=Root+"Prefabs/RarityTiming.prefab";
    private static readonly Color Ink=new Color32(48,68,72,255),Paper=new Color32(240,226,194,255);

    /// <summary>Builds only the timing template, preserving GUIDs and existing balance tuning.</summary>
    public static void Build()
    {
        var profile=AssetDatabase.LoadAssetAtPath<RarityTimingProfile>(ProfilePath);
        if(profile==null){profile=ScriptableObject.CreateInstance<RarityTimingProfile>();AssetDatabase.CreateAsset(profile,ProfilePath);}
        string imagePath=Root+"Art/Rarity/RarityBarFrame.png";AssetDatabase.ImportAsset(imagePath,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(imagePath);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spriteBorder=new Vector4(64,64,64,64);importer.spritePixelsPerUnit=100;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
        importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
        var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        var root=new GameObject("Rarity Timing Phase",typeof(RectTransform),typeof(CanvasGroup));var rt=root.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(3840,2160);
        var view=root.AddComponent<JournalRarityTimingView>();var data=new SerializedObject(view);
        Label("Stage Heading",rt,new Rect(1300,470,1160,80),46,theme.DisplayFont,Paper).text="SOMETHING STIRS BELOW";
        Bind(data,"subtitle",Label("Destination",rt,new Rect(1300,565,1160,50),30,theme.BodyFont,Paper));
        var panel=Rect("Parchment Instrument",rt,new Rect(1300,710,1160,340));
        var face=panel.gameObject.AddComponent<UnityEngine.UI.Image>();face.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);face.type=UnityEngine.UI.Image.Type.Sliced;face.raycastTarget=false;
        Bind(data,"heading",Label("Rarity Heading",panel,new Rect(56,28,1000,40),26,theme.DisplayFont,Ink));
        var border=Rect("Track Outline",panel,new Rect(54,106,1052,66)).gameObject.AddComponent<UnityEngine.UI.Image>();border.color=Ink;border.raycastTarget=false;
        var track=Rect("Dynamic Zones",panel,new Rect(56,108,1048,62));track.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var zones=data.FindProperty("zones");zones.arraySize=5;var legends=data.FindProperty("legends");legends.arraySize=5;
        for(int i=0;i<5;i++)
        {
            var zone=Rect("Rarity Zone "+i,track,new Rect(0,0,1048,62));var fill=zone.gameObject.AddComponent<UnityEngine.UI.Image>();fill.raycastTarget=false;
            var pattern=Rect("Independent Zone Pattern",zone,new Rect(0,0,1048,62));pattern.anchorMin=Vector2.zero;pattern.anchorMax=Vector2.one;pattern.offsetMin=pattern.offsetMax=Vector2.zero;
            var glyph=pattern.gameObject.AddComponent<JournalRarityGraphic>();glyph.color=new Color(1,1,1,.25f);glyph.raycastTarget=false;
            zones.GetArrayElementAtIndex(i).objectReferenceValue=zone;
            var legend=Rect("Rarity Legend "+i,panel,new Rect(56,194,349,36));
            var emblem=Rect("Legend Emblem",legend,new Rect(0,5,25,25)).gameObject.AddComponent<JournalRarityGraphic>();emblem.raycastTarget=false;
            Label("Rarity Label",legend,new Rect(35,0,200,36),23,theme.DisplayFont,Ink).alignment=TextAlignmentOptions.MidlineLeft;
            legends.GetArrayElementAtIndex(i).objectReferenceValue=legend;
        }
        // Readable external labels remain independent of tiny optional intervals.
        for(int i=0;i<=20;i++){var tick=Rect("Track Tick "+i,panel,new Rect(56+i*52.4f,108,1,7)).gameObject.AddComponent<UnityEngine.UI.Image>();tick.color=new Color(1,1,1,.6f);tick.raycastTarget=false;}
        var wash=Rect("Stopped Zone Wash",panel,new Rect(56,108,1048,62));var washImage=wash.gameObject.AddComponent<UnityEngine.UI.Image>();washImage.color=new Color(1,1,1,.23f);washImage.raycastTarget=false;Bind(data,"wash",wash);
        var burst=Rect("Stop Burst",panel,new Rect(56,101,70,70));burst.pivot=new Vector2(.5f,.5f);var burstMesh=burst.gameObject.AddComponent<JournalRarityGraphic>();burstMesh.drawing=JournalRarityGraphic.Drawing.Burst;burstMesh.color=new Color32(155,113,63,255);burstMesh.raycastTarget=false;Bind(data,"burst",burst);
        var marker=Rect("Independent Compass Marker",panel,new Rect(56,79,38,117));marker.pivot=new Vector2(.5f,1);var markerMesh=marker.gameObject.AddComponent<JournalRarityGraphic>();markerMesh.drawing=JournalRarityGraphic.Drawing.Marker;markerMesh.color=Paper;markerMesh.raycastTarget=false;Bind(data,"marker",marker);
        var instruction=Label("Instruction",panel,new Rect(56,260,708,44),23,theme.BodyFont,Ink);instruction.alignment=TextAlignmentOptions.MidlineLeft;Bind(data,"instruction",instruction);
        var buttonRoot=Rect("Stop Button",panel,new Rect(808,248,296,66));var buttonFace=buttonRoot.gameObject.AddComponent<UnityEngine.UI.Image>();buttonFace.color=Ink;
        var button=buttonRoot.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=buttonFace;
        var colours=button.colors;colours.highlightedColor=new Color32(170,212,195,255);colours.selectedColor=colours.highlightedColor;colours.disabledColor=new Color32(150,139,111,255);button.colors=colours;
        var stopIcon=Rect("Stop Square",buttonRoot,new Rect(35,25,14,14)).gameObject.AddComponent<UnityEngine.UI.Image>();stopIcon.color=Paper;stopIcon.raycastTarget=false;
        Bind(data,"stopLabel",Label("Stop Label",buttonRoot,new Rect(62,11,220,44),27,theme.DisplayFont,Paper));Bind(data,"stopButton",button);Bind(data,"visibility",root.GetComponent<CanvasGroup>());
        data.ApplyModifiedPropertiesWithoutUndo();root.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);UnityEngine.Object.DestroyImmediate(root);
    }

    /// <summary>Adds one reusable phase to an authored canvas without replacing its existing card, controls or hand.</summary>
    public static void BindCanvas(GameObject canvas)
    {
        var view=canvas.GetComponent<FishingRunView>();var data=new SerializedObject(view);
        if(data.FindProperty("rarityTimingView").objectReferenceValue!=null)return;
        var encounter=(RectTransform)data.FindProperty("encounterPanel").objectReferenceValue;
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),encounter.parent);
        var rect=instance.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        Bind(data,"rarityTimingView",instance.GetComponent<JournalRarityTimingView>());data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Imports only R046 and enables its gameplay profile in the isolated playable scene.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Integrate Rarity Timing R046")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Integrate outside Play Mode.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!=JournalComparisonSceneBuilder.ScenePath||scene.isDirty)throw new InvalidOperationException("Open the clean comparison scene before integrating.");
        Build();string path=Root+"Prefabs/MidnightFieldJournalCanvas.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try{BindCanvas(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        var controller=UnityEngine.Object.FindAnyObjectByType<FishingRunController>();var data=new SerializedObject(controller);
        Bind(data,"rarityTimingProfile",AssetDatabase.LoadAssetAtPath<RarityTimingProfile>(ProfilePath));data.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Creates top-left fixed design geometry inside its existing 4K viewport.</summary>
    private static RectTransform Rect(string name,Transform parent,Rect bounds)
    {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(bounds.x,-bounds.y);r.sizeDelta=bounds.size;return r;}
    /// <summary>Creates a live native TMP field with explicit theme font and contrast.</summary>
    private static TMP_Text Label(string name,Transform parent,Rect bounds,float size,TMP_FontAsset font,Color color)
    {var text=Rect(name,parent,bounds).gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSize=size;text.color=color;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;text.text="";return text;}
    /// <summary>Binds a serialized component reference without relying on runtime searches.</summary>
    private static void Bind(SerializedObject data,string field,UnityEngine.Object value){data.FindProperty(field).objectReferenceValue=value;}
}
