using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Installs the approved Penpot Build/Upgrade composition with separate artwork, live data and native controls.</summary>
public static class RigBuildingSetup
{
    public const string PrefabPath = TackleDiscoverArtSetup.Root + "Prefabs/BuildCanvas.prefab";
    public const string CabinetCardPath = TackleDiscoverArtSetup.Root + "Prefabs/BuildRigCard.prefab";
    public const string RailCardPath = TackleDiscoverArtSetup.Root + "Prefabs/BuildRailCard.prefab";
    public const string ArtPath = TackleDiscoverArtSetup.Root + "Art/RigBuilding/";
    public const string Source = "Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/Refinement/RuntimeAssets/";
    private const string Root = TackleDiscoverArtSetup.Root;
    private static readonly Color Cream = new Color32(222,211,165,255), Gold = new Color32(233,188,99,255), Ink = new Color32(55,59,44,255);
    [Serializable] private sealed class Manifest { public Entry[] assets; }
    [Serializable] private sealed class Entry { public string name; public int width, height; }

    /// <summary>Updates only Build in the saved comparison scene, retaining Harbor, reels, Landing and gameplay tuning.</summary>
    [MenuItem("Fishing Cards/Rig Building/Install Build Phase")]
    public static void InstallCurrentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install Build outside Play Mode.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath) throw new InvalidOperationException("Open the comparison scene.");
        BuildAssets(); Install(scene); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Build bindings.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the phase binding when Harbor or comparison authoring regenerates the scene.</summary>
    public static void Install(Scene scene)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) BuildAssets();
        var runView = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var build = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RigBuildingView>(true)).SingleOrDefault();
        if (build == null) build = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),scene)).GetComponent<RigBuildingView>();
        var data = new SerializedObject(runView); Bind(data,"rigBuildingView",build); data.ApplyModifiedPropertiesWithoutUndo(); build.gameObject.SetActive(false);
    }

    /// <summary>Imports approved decorative exports without resampling and authors reusable live-data prefabs.</summary>
    public static void BuildAssets()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath) == null) TackleDiscoverArtSetup.BuildAssets();
        Directory.CreateDirectory(ArtPath);
        foreach (var entry in JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"Manifest.json")).assets)
        {
            string path=ArtPath+entry.name+".png"; byte[] bytes=File.ReadAllBytes(Source+entry.name+".png");
            if (!File.Exists(path)||!File.ReadAllBytes(path).SequenceEqual(bytes)) File.WriteAllBytes(path,bytes);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spritePixelsPerUnit=100;
            importer.filterMode=FilterMode.Point; importer.wrapMode=TextureWrapMode.Clamp; importer.mipmapEnabled=false;
            importer.sRGBTexture=true; importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.isReadable=false; importer.alphaIsTransparency=true; importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.spriteBorder=Vector4.zero;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite.texture.width!=entry.width||sprite.texture.height!=entry.height)throw new InvalidOperationException("Build artwork was resized: "+entry.name);
        }
        BuildCard(false); BuildCard(true);
        var root=new GameObject("Tackle Build Canvas",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(RigBuildingView));
        try
        {
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay; root.GetComponent<Canvas>().sortingOrder=85;
            root.AddComponent<UnityEngine.UI.Image>().color=new Color32(14,17,15,255);
            var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080); scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var stage=Rect(root.transform,"1920x1080 Stage",new Rect(0,0,1920,1080));
            stage.anchorMin=stage.anchorMax=stage.pivot=new Vector2(.5f,.5f); stage.anchoredPosition=Vector2.zero;
            foreach(var item in new[]{
                ("build-desk",new Rect(0,0,1920,1080)),("build-cabinet",new Rect(24,44,380,995)),
                ("build-title",new Rect(570,28,780,116)),("build-haul",new Rect(1430,29,345,116)),
                ("build-machine",new Rect(490,163,1310,657)),("build-console",new Rect(492,897,1310,165)),
                ("build-message",new Rect(525,836,1140,54)),("build-portrait",new Rect(630,295,455,225)),
                ("build-effect",new Rect(1100,285,565,235)),("build-tray",new Rect(550,562,1135,269)),
                ("build-rail-heading",new Rect(680,521,950,47)),("build-readout",new Rect(545,920,505,116))}) Art(stage,item.Item1,item.Item2);
            var data=new SerializedObject(root.GetComponent<RigBuildingView>());
            Bind(data,"tray",stage.GetComponentsInChildren<UnityEngine.UI.Image>().Single(i=>i.name=="build-tray"));
            Bind(data,"trayDefault",Sprite("build-tray")); Bind(data,"trayThree",Sprite("build-tray-three"));
            Bind(data,"displayFont",Font(false));
            Bind(data,"heading",Label(stage,"BiomePhase","",new Rect(590,72,740,45),30));
            Bind(data,"haul",Label(stage,"Haul","",new Rect(1450,76,300,40),27,false,Gold));
            Label(stage,"RigHeading","YOUR RIG",new Rect(70,114,290,36),24);
            Bind(data,"creatureName",Label(stage,"LandedCreature","",new Rect(690,223,960,42),28));
            Bind(data,"tags",Label(stage,"CreatureTags","",new Rect(666,474,383,30),25,true));
            Bind(data,"levelLabel",Label(stage,"CreatureLevel","",new Rect(1160,322,230,36),24,false,Gold,true));
            Bind(data,"levelDelta",Label(stage,"LevelIncrease","",new Rect(1354,325,48,32),26,true,Gold));
            Label(stage,"SaleCaption","SALE",new Rect(1443,326,77,30),25,true);
            Bind(data,"saleLabel",Label(stage,"SpeciesSaleValue","",new Rect(1526,319,95,42),32,false,Gold));
            var rulesScroll=Scroll(stage,"AbilityScroll",new Rect(1160,383,455,115),true,out var rulesContent);
            var description=Label(rulesContent,"PrimaryAbility","",new Rect(0,0,455,115),33,true,null,true);
            description.richText=true; description.lineSpacing=20; description.enableAutoSizing=false; description.textWrappingMode=TextWrappingModes.Normal;
            Bind(data,"detailsScroll",rulesScroll);
            Bind(data,"details",description);
            var portrait=Rect(stage,"CreaturePortrait",new Rect(655,326,405,116)).gameObject.AddComponent<UnityEngine.UI.Image>();
            CreatureSpriteSetup.CenterPortrait(portrait); portrait.raycastTarget=false; Bind(data,"portrait",portrait);
            Bind(data,"railHeading",Label(stage,"InsertionHeading","",new Rect(695,535,920,30),20));
            Bind(data,"previewCaption",Label(stage,"HaulPreviewCaption","",new Rect(565,939,465,32),20));
            Bind(data,"previewLabel",Label(stage,"HaulPreviewValue","",new Rect(565,978,465,42),31));
            Bind(data,"message",Label(stage,"PlacementMessage","",new Rect(550,854,1085,32),25,true));
            var rig=Scroll(stage,"CommittedRig",new Rect(62,189,315,825),true,out var rigContent);
            var rail=Scroll(stage,"InsertionPositions",new Rect(588,574,1053,223),false,out var railContent);
            Bind(data,"rigScroll",rig); Bind(data,"rigContent",rigContent); Bind(data,"railScroll",rail); Bind(data,"railContent",railContent);
            Bind(data,"emptyRig",Label(stage,"EmptyRig","EMPTY RIG",new Rect(80,500,280,38),25,true));
            Bind(data,"cardPrefab",AssetDatabase.LoadAssetAtPath<GameObject>(CabinetCardPath).GetComponent<TackleRigCardView>());
            Bind(data,"railCardPrefab",AssetDatabase.LoadAssetAtPath<GameObject>(RailCardPath).GetComponent<TackleRigCardView>());
            var rope=Art(railContent,"rope-horizontal",new Rect(0,0,304,48)); rope.gameObject.SetActive(false); Bind(data,"ropeTemplate",rope);
            var gap=Rect(railContent,"InsertionTemplate",new Rect(0,0,80,94));
            var socket=gap.gameObject.AddComponent<UnityEngine.UI.Image>(); socket.sprite=Sprite("build-gap-idle");
            var gapButton=gap.gameObject.AddComponent<UnityEngine.UI.Button>(); gapButton.targetGraphic=socket;
            gap.gameObject.SetActive(false); Bind(data,"gapTemplate",gapButton);
            Bind(data,"socketIdle",Sprite("build-gap-idle")); Bind(data,"socketSelected",Sprite("build-gap-selected")); Bind(data,"socketBlocked",Sprite("insertion-blocked"));
            Bind(data,"selectionCaret",Art(railContent,"build-selection-caret",new Rect(0,0,80,35)).rectTransform);
            var button=Rect(stage,"CommitPlacement",new Rect(1120,920,600,116));
            var hit=button.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color=Color.clear;
            var commit=button.gameObject.AddComponent<UnityEngine.UI.Button>(); commit.targetGraphic=hit; commit.transition=UnityEngine.UI.Selectable.Transition.None;
            var visual=Rect(button,"Visual",new Rect(0,0,600,116)); visual.gameObject.AddComponent<CanvasGroup>();
            Art(visual,"build-place",new Rect(0,0,600,116)); Label(visual,"CommitLabel","PLACE & CONTINUE",new Rect(20,42,560,38),27,false,Ink);
            var focus=Art(button,"focus-corners",new Rect(0,0,600,116)).gameObject; focus.SetActive(false);
            var feedback=new SerializedObject(button.gameObject.AddComponent<TackleButtonFeedback>()); Bind(feedback,"visual",visual); Bind(feedback,"focusCorners",focus); feedback.ApplyModifiedPropertiesWithoutUndo();
            Bind(data,"commit",commit);
            var upgrade=Rect(stage,"UpgradeStatus",new Rect(1120,920,600,116)); Art(upgrade,"build-upgraded",new Rect(0,0,600,116));
            Label(upgrade,"UpgradeLabel","UPGRADED",new Rect(20,25,560,36),25,false,Gold);
            Label(upgrade,"AutomaticContinuation","Next encounter...",new Rect(20,62,560,30),24,true); upgrade.gameObject.SetActive(false); Bind(data,"upgradeStatus",upgrade.gameObject);
            data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Builds native-label masters for the cabinet and the original hanging-card insertion tray.</summary>
    private static void BuildCard(bool rail)
    {
        var root=Rect(null,rail?"Build Rail Card":"Build Rig Card",new Rect(0,0,rail?221:315,rail?190:275));
        try
        {
            var data=new SerializedObject(root.gameObject.AddComponent<TackleRigCardView>());
            Bind(data,"face",Art(root,rail?"build-rail-card":"build-rig-card",new Rect(0,0,rail?221:315,rail?190:244)));
            Bind(data,"catchFace",Sprite(rail?"build-rail-card":"build-rig-card")); Bind(data,"baitFace",Sprite(rail?"build-rail-card":"build-rig-card"));
            var portrait=Rect(root,"Portrait",rail?new Rect(24,57,176,77):new Rect(30,70,255,105)).gameObject.AddComponent<UnityEngine.UI.Image>();
            portrait.raycastTarget=false; CreatureSpriteSetup.CenterPortrait(portrait); Bind(data,"portrait",portrait);
            var clip=Art(root,"card-clip",rail?new Rect(83,-23,55,34):new Rect(130,-23,55,34));
            if(rail)Bind(data,"railClip",clip.gameObject);
            if(!rail) Art(root,"rope-vertical",new Rect(147,244,20,35));
            Bind(data,"nameLabel",Label(root,"Name","",rail?new Rect(37,25,149,27):new Rect(51,27,213,32),rail?21:21,rail));
            Bind(data,"roleLabel",Label(root,"Level","",rail?new Rect(42,151,88,27):new Rect(44,202,140,28),rail?22:18,rail,null,true));
            Bind(data,"valueLabel",Label(root,"SaleValue","",rail?new Rect(141,150,38,30):new Rect(211,201,58,32),rail?24:22,rail));
            data.FindProperty("alignInkTop").boolValue=true;
            if(rail)
            {
                var highlight=Art(root,"focus-corners",new Rect(-6,-4,235,198)); highlight.type=UnityEngine.UI.Image.Type.Sliced; highlight.pixelsPerUnitMultiplier=1.8f;
                highlight.gameObject.SetActive(false); Bind(data,"acquiredHighlight",highlight.gameObject);
            }
            data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root.gameObject,rail?RailCardPath:CabinetCardPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    /// <summary>Creates a clipped dynamic content area using the existing pool-driven sizing policy.</summary>
    private static UnityEngine.UI.ScrollRect Scroll(Transform parent,string name,Rect bounds,bool vertical,out RectTransform content)
    {
        var root=Rect(parent,name,bounds); var scroll=root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        var viewport=Rect(root,"Viewport",new Rect(0,0,bounds.width,bounds.height)); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color=Color.clear;
        content=Rect(viewport,"Content",new Rect(0,0,bounds.width,bounds.height)); scroll.viewport=viewport; scroll.content=content;
        scroll.vertical=vertical; scroll.horizontal=!vertical; scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped; return scroll;
    }
    /// <summary>Loads only approved theme sprites, keeping new Build imports separate from other phases.</summary>
    private static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath+name+".png")??AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/Placement/"+name+".png")??AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/Common/"+name+".png");
    /// <summary>Places a complete original ornament at its exact reference dimensions without slicing it again.</summary>
    private static UnityEngine.UI.Image Art(Transform parent,string name,Rect bounds)
    {
        var image=Rect(parent,name,bounds).gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite=Sprite(name); image.raycastTarget=false;
        if(image.sprite==null)throw new InvalidOperationException("Missing approved Build resource: "+name); return image;
    }
    /// <summary>Authors reference coordinates inside a centered, aspect-preserving stage.</summary>
    private static RectTransform Rect(Transform parent,string name,Rect bounds)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform; if(parent!=null)rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1); rect.anchoredPosition=new Vector2(bounds.x,-bounds.y); rect.sizeDelta=bounds.size; return rect;
    }
    /// <summary>Loads the existing licensed TMP fonts without changing their materials in other screens.</summary>
    private static TMP_FontAsset Font(bool body)=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+"Fonts/"+(body?"VT323":"PressStart2P")+".asset");
    /// <summary>Separates the fixed layout rectangle from the native glyphs so ink-top alignment remains stable.</summary>
    private static TMP_Text Label(Transform parent,string name,string value,Rect bounds,float size,bool body=false,Color? color=null,bool left=false)
    {
        var holder=Rect(parent,name,bounds); var child=Rect(holder,"Text",new Rect(0,0,bounds.width,bounds.height));
        var text=child.gameObject.AddComponent<TextMeshProUGUI>(); text.font=Font(body); text.fontSize=size; text.fontSizeMin=12; text.fontSizeMax=size;
        text.enableAutoSizing=!body; text.color=color??Cream; text.alignment=left?TextAlignmentOptions.TopLeft:TextAlignmentOptions.Top;
        text.textWrappingMode=TextWrappingModes.NoWrap; text.overflowMode=TextOverflowModes.Overflow; text.raycastTarget=false; text.richText=false;
        TackleBuildTypography.SetText(text,value); return text;
    }
    /// <summary>Serializes explicit references so no runtime hierarchy searches are needed.</summary>
    private static void Bind(SerializedObject data,string field,UnityEngine.Object value)=>data.FindProperty(field).objectReferenceValue=value;
}
