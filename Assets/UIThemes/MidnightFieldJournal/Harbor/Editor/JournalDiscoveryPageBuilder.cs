using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

/// <summary>Reconstructs the approved ornate Journal from separate 4K frames and native fields.</summary>
public static class JournalDiscoveryPageBuilder
{
    public const string Root=JournalHarborBuilder.Root+"Journal/";
    [Serializable] private sealed class Layout { public Resource[] resources; public Node[] nodes; }
    [Serializable] private sealed class Resource { public string id,path; }
    [Serializable] private sealed class Node
    {
        public string kind,name,resource,text,family,color,align,role;
        public int questIndex=-1;
        public float x,y,width,height,fontSize,opacity=1;
    }

    /// <summary>Builds only the Journal page while preserving shared navigation and tracking ownership.</summary>
    public static RectTransform Build(Transform parent,DiscoveryJournalTracker tracker,TextAsset catalogSource,MidnightFieldJournalTheme theme,JournalHarborFlow flow)
    {
        var catalog=DiscoveryQuestCatalog.Load(catalogSource);
        var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Root+"JournalLayout.json"));
        var sprites=layout.resources.ToDictionary(r=>r.id,r=>AssetDatabase.LoadAssetAtPath<Sprite>(r.path));
        if(sprites.Any(r=>r.Value==null)) throw new InvalidOperationException("Import the approved Journal assets before building.");
        var page=JournalModalLayout.Rect("journal",parent,new Rect(0,0,1,1));
        var labels=new TMP_Text[catalog.quests.Length]; var marks=new UnityEngine.UI.Graphic[catalog.quests.Length];
        TMP_Text gold=null,save=null;
        foreach(var node in layout.nodes)
        {
            if(node.kind=="emblem"&&node.resource=="Check") continue;
            var rect=Place(node.name,page,new Rect(node.x,node.y,node.width,node.height));
            ColorUtility.TryParseHtmlString(node.color??"#FFFFFF",out Color color); color.a=node.opacity;
            if(node.kind=="image"||node.kind=="rect")
            {
                var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.raycastTarget=false;
                image.color=node.kind=="image"?Color.white:color;
                if(node.kind=="image") { image.sprite=sprites[node.resource]; image.useSpriteMesh=false; }
            }
            else if(node.kind=="emblem")
            {
                var emblem=rect.gameObject.AddComponent<JournalHarborEmblem>(); emblem.raycastTarget=false; emblem.color=color;
                emblem.Configure((JournalHarborEmblem.Kind)Enum.Parse(typeof(JournalHarborEmblem.Kind),node.resource));
            }
            else
            {
                var text=rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font=node.family=="Cinzel"?theme.DisplayFont:theme.BodyFont;
                text.fontSize=node.fontSize; text.enableAutoSizing=true; text.fontSizeMax=node.fontSize; text.fontSizeMin=node.fontSize*.8f;
                text.color=color; text.text=node.text; text.richText=false; text.raycastTarget=false; text.margin=Vector4.zero;
                text.textWrappingMode=TextWrappingModes.Normal; text.alignment=node.align=="center"?TextAlignmentOptions.Top:TextAlignmentOptions.TopLeft;
                if(node.role=="status") { labels[node.questIndex]=text; text.rectTransform.anchoredPosition=new Vector2(1480,-node.y); text.color=new Color32(224,188,128,255); text.text=JournalDiscoveryPage.FormatProgress(catalog.quests[node.questIndex],0); }
                if(node.role=="gold") gold=text;
                if(node.role=="save") save=text;
            }
        }
        for(int i=0;i<catalog.quests.Length;i++)
        {
            if(catalog.quests[i].metric==DiscoveryQuestMetric.OverloadedSuccessfulExpeditions) continue;
            var mark=Place("Completion Mark",page,new Rect(1460,693+i*540,50,50)).gameObject.AddComponent<JournalHarborEmblem>();
            mark.Configure(JournalHarborEmblem.Kind.Check); mark.color=new Color32(196,217,196,255); mark.raycastTarget=false; mark.enabled=false; marks[i]=mark;
        }
        var risk=catalog.quests.Single(q=>q.metric==DiscoveryQuestMetric.OverloadedSuccessfulExpeditions);
        int riskIndex=Array.IndexOf(catalog.quests,risk); var segments=new UnityEngine.UI.Image[risk.target];
        float width=780f/risk.target;
        for(int i=0;i<segments.Length;i++)
        {
            var image=Place("Expedition Segment",page,new Rect(1480+i*width,755+riskIndex*540,width-20,6)).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color=new Color(114/255f,128/255f,128/255f,.4f); image.raycastTarget=false; segments[i]=image;
        }
        var view=page.gameObject.AddComponent<JournalDiscoveryPage>(); var data=new SerializedObject(view);
        data.FindProperty("tracker").objectReferenceValue=tracker; data.FindProperty("saveStatus").objectReferenceValue=save; data.FindProperty("goldLabel").objectReferenceValue=gold;
        var rows=data.FindProperty("rows"); rows.arraySize=labels.Length;
        for(int i=0;i<labels.Length;i++)
        {
            var row=rows.GetArrayElementAtIndex(i); row.FindPropertyRelative("status").objectReferenceValue=labels[i]; row.FindPropertyRelative("completionMark").objectReferenceValue=marks[i];
            var spans=row.FindPropertyRelative("segments"); spans.arraySize=i==riskIndex?segments.Length:0;
            for(int j=0;j<spans.arraySize;j++) spans.GetArrayElementAtIndex(j).objectReferenceValue=segments[j];
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        string[] names={"Harbor","Tackle Shop","Technique Collection","Discovery Journal"};
        for(int i=0;i<names.Length;i++) { var button=Button(page,names[i],new Rect(1332+i*360,90,345,100)); UnityEventTools.AddIntPersistentListener(button.onClick,flow.Navigate,i); }
        var back=Button(page,"Back to Harbor",new Rect(2670,1920,1010,188)); UnityEventTools.AddIntPersistentListener(back.onClick,flow.Navigate,0);
        return page;
    }

    /// <summary>Imports the approved resources and replaces only the Journal subtree of the existing hub prefab.</summary>
    public static void Install()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!=JournalComparisonSceneBuilder.ScenePath||scene.isDirty) throw new InvalidOperationException("Open the clean journal comparison scene.");
        var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Root+"JournalLayout.json"));
        foreach(var resource in layout.resources) Import(resource.path);
        var root=PrefabUtility.LoadPrefabContents(JournalHarborBuilder.PrefabPath);
        try
        {
            var flow=root.GetComponent<JournalHarborFlow>(); var data=new SerializedObject(flow);
            var pages=data.FindProperty("pages"); var old=(GameObject)pages.GetArrayElementAtIndex(3).objectReferenceValue;
            var tracker=root.GetComponent<DiscoveryJournalTracker>(); var trackerData=new SerializedObject(tracker);
            var theme=AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
            var page=Build(old.transform.parent,tracker,(TextAsset)trackerData.FindProperty("catalogSource").objectReferenceValue,theme,flow); page.gameObject.SetActive(false);
            pages.GetArrayElementAtIndex(3).objectReferenceValue=page.gameObject;
            var gold=data.FindProperty("goldLabels");
            var remaining=Enumerable.Range(0,gold.arraySize).Select(i=>(TMP_Text)gold.GetArrayElementAtIndex(i).objectReferenceValue).Where(t=>t!=null&&!t.transform.IsChildOf(old.transform)).ToList();
            remaining.Add(page.GetComponent<JournalDiscoveryPage>().GoldLabel); gold.arraySize=remaining.Count;
            for(int i=0;i<remaining.Count;i++) gold.GetArrayElementAtIndex(i).objectReferenceValue=remaining[i];
            data.ApplyModifiedPropertiesWithoutUndo(); UnityEngine.Object.DestroyImmediate(old);
            PrefabUtility.SaveAsPrefabAsset(root,JournalHarborBuilder.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>Preserves native frame pixels and alpha at the 4K import boundary.</summary>
    private static void Import(string path)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.maxTextureSize=4096; importer.npotScale=TextureImporterNPOTScale.None; importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.filterMode=FilterMode.Bilinear; importer.wrapMode=TextureWrapMode.Clamp; importer.SaveAndReimport();
    }

    /// <summary>Places a field in the fitted stage using its actual Penpot top-left coordinates.</summary>
    private static RectTransform Place(string name,Transform parent,Rect bounds)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1); rect.anchoredPosition=new Vector2(bounds.x,-bounds.y); rect.sizeDelta=bounds.size; return rect;
    }

    /// <summary>Creates a route hit area with the existing restrained wash feedback.</summary>
    private static UnityEngine.UI.Button Button(Transform parent,string name,Rect bounds)
    {
        var rect=Place(name,parent,bounds); var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color=Color.white;
        var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=image; var colors=button.colors; colors.normalColor=Color.clear; colors.highlightedColor=new Color(1,.94f,.78f,.07f); colors.selectedColor=new Color(1,.94f,.78f,.09f); colors.pressedColor=new Color(1,.94f,.78f,.14f); button.colors=colors; return button;
    }
}
