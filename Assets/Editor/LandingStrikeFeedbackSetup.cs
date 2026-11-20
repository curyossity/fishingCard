using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors native strike attribution without repainting the approved machine or card artwork.</summary>
public static class LandingStrikeFeedbackSetup
{
    private const string Root = TackleDiscoverArtSetup.Root + "Prefabs/";
    public const string ChipPath = Root + "LandingContributionChip.prefab", RowPath = Root + "LandingContributionRow.prefab", ActivationPath = Root + "LandingActivation.prefab";

    /// <summary>Updates only Landing and Build prefab bindings; existing scene instances inherit the change.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install feedback outside Play Mode.");
        EnsurePrefabs();
        foreach (string path in new[] { LandingChallengeSetup.PrefabPath, RigBuildingSetup.PrefabPath })
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the same feedback composition when either phase's existing authoring path is regenerated.</summary>
    public static void Configure(GameObject root)
    {
        if (root.GetComponent<LandingStrikeFeedback>() != null)
        {
            var existing = new SerializedObject(root.GetComponent<LandingStrikeFeedback>());
            existing.FindProperty("compactSummary").boolValue = root.GetComponent<RigBuildingView>() != null;
            if (root.GetComponent<RigBuildingView>() == null)
            {
                var assist = (RectTransform)existing.FindProperty("assistance").objectReferenceValue;
                assist.GetComponentInParent<UnityEngine.UI.ScrollRect>().GetComponent<RectTransform>().anchoredPosition = new Vector2(1150,-433);
                ((TMP_Text)existing.FindProperty("assistanceTotals").objectReferenceValue).rectTransform.anchoredPosition = new Vector2(1150,-462);
                Bind(existing,"attemptCount",new SerializedObject(root.GetComponent<TackleLandingPresentation>()).FindProperty("attempts").objectReferenceValue);
                var recovery=(TMP_Text)existing.FindProperty("attemptFeedback").objectReferenceValue;
                recovery.rectTransform.anchoredPosition=new Vector2(945,-758);recovery.rectTransform.sizeDelta=new Vector2(300,32);recovery.fontSize=20;recovery.overflowMode=TextOverflowModes.Ellipsis;
            }
            existing.ApplyModifiedPropertiesWithoutUndo(); LandingScoreRushSetup.Configure(root); return;
        }
        EnsurePrefabs();
        bool build = root.GetComponent<RigBuildingView>() != null;
        var phase = new SerializedObject(build ? (UnityEngine.Object)root.GetComponent<RigBuildingView>() : root.GetComponent<TackleLandingPresentation>());
        var stage = root.transform.Find("1920x1080 Stage");
        if (stage == null) throw new InvalidOperationException("Missing authored Tackle stage.");
        var host = root.AddComponent<LandingStrikeFeedback>(); var data = new SerializedObject(host);
        data.FindProperty("compactSummary").boolValue = build;
        Bind(phase, "strikeFeedback", host); phase.ApplyModifiedPropertiesWithoutUndo();
        Bind(data, "rowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(RowPath).GetComponent<LandingContributionView>());
        Bind(data, "chipPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ChipPath).GetComponent<LandingContributionView>());
        Bind(data, "activationPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ActivationPath).GetComponent<LandingActivationView>());
        var layer = Rect(root.transform, "Strike Activation Overlay", new Rect(0,0,0,0));
        layer.anchorMin = Vector2.zero; layer.anchorMax = Vector2.one; layer.pivot = new Vector2(.5f,.5f); layer.offsetMin = layer.offsetMax = Vector2.zero;
        var group = layer.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
        Bind(data, "pulseLayer", layer);
        var chipScroll = Scroll(stage, "Last Strike Sources", build ? new Rect(1010,846,630,34) : new Rect(776,543,828,34), false, out var chips);
        Bind(data, "chips", chips); Bind(data, "chipScroll", chipScroll);
        if (build)
        {
            var message = (TMP_Text)phase.FindProperty("message").objectReferenceValue;
            var messageHolder = message.rectTransform.parent as RectTransform;
            messageHolder.sizeDelta = new Vector2(235,54); message.rectTransform.sizeDelta = new Vector2(235,54);
            Bind(data, "equation", Text(stage,"Last Hit Summary",new Rect(799,849,206,28),22));
            Bind(data, "attemptFeedback", Text(stage,"Last Hit Recovery",new Rect(555,811,1080,22),22));
        }
        else
        {
            Text(stage,"Last Hit Sources Caption",new Rect(665,548,110,24),22).text = "SOURCES";
            Bind(data, "equation", phase.FindProperty("grade").objectReferenceValue);
            var old = (TMP_Text)phase.FindProperty("breakdown").objectReferenceValue;
            old.gameObject.SetActive(false);
            var scroll = (UnityEngine.UI.ScrollRect)phase.FindProperty("scoreScroll").objectReferenceValue;
            var rows = Rect(scroll.viewport,"Contribution Rows",new Rect(0,0,445,64)); scroll.content = rows;
            Bind(data, "rows", rows); Bind(data, "rowScroll", scroll);
            var assistScroll = Scroll(stage,"Ongoing Assistance",new Rect(1150,433,445,28),false,out var assist);
            Bind(data,"assistance",assist);
            Bind(data,"assistanceTotals",Text(stage,"Assistance Totals",new Rect(1150,462,445,21),20));
            var recovery=Text(stage,"Attempt Recovery",new Rect(945,758,300,32),20);recovery.overflowMode=TextOverflowModes.Ellipsis;
            Bind(data,"attemptFeedback",recovery);Bind(data,"attemptCount",phase.FindProperty("attempts").objectReferenceValue);
            scroll.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,60);
            scroll.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,60);
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        LandingScoreRushSetup.Configure(root);
    }

    /// <summary>Creates reusable portrait targets and a non-intercepting activation outline once.</summary>
    private static void EnsurePrefabs()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ChipPath) == null) BuildTarget(false);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RowPath) == null) BuildTarget(true);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ActivationPath) != null) return;
        var root = Rect(null,"Landing Activation",new Rect(0,0,321,250));
        try
        {
            root.anchorMin = root.anchorMax = new Vector2(.5f,.5f);
            var data = new SerializedObject(root.gameObject.AddComponent<LandingActivationView>());
            var border = Rect(root,"Brass activation",new Rect(0,0,321,250));
            border.anchorMin = Vector2.zero; border.anchorMax = Vector2.one; border.offsetMin = border.offsetMax = Vector2.zero;
            var alpha = border.gameObject.AddComponent<CanvasGroup>(); Bind(data,"outline",alpha);
            for (int i=0;i<4;i++)
            {
                var edge = Rect(border,"Edge "+i,new Rect(0,0,0,0));
                edge.anchorMin = i==0?new Vector2(0,1):Vector2.zero; edge.anchorMax = i==1?new Vector2(1,0):Vector2.one;
                if(i==2)edge.anchorMax=new Vector2(0,1); if(i==3)edge.anchorMin=new Vector2(1,0);
                edge.pivot=new Vector2(.5f,.5f); edge.offsetMin=edge.offsetMax=Vector2.zero;
                edge.SetSizeWithCurrentAnchors(i<2?RectTransform.Axis.Vertical:RectTransform.Axis.Horizontal,3);
                edge.anchoredPosition=i<2?new Vector2(0,i==0?-1.5f:1.5f):new Vector2(i==2?1.5f:-1.5f,0);
                var image=edge.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color=new Color32(233,188,99,255);image.raycastTarget=false;
            }
            var background=Rect(root,"Badge background",new Rect(135,-8,180,31)).gameObject.AddComponent<UnityEngine.UI.Image>(); background.raycastTarget=false;
            var label=Text(root,"Strike amount",new Rect(143,-8,164,31),25); label.alignment=TextAlignmentOptions.Center;
            Bind(data,"badge",label); Bind(data,"badgeBackground",background); data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,ActivationPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }
    /// <summary>Builds a native inspectable source target; its Selectable has no submit action.</summary>
    private static void BuildTarget(bool row)
    {
        var root=Rect(null,row?"Contribution row":"Contribution chip",new Rect(0,0,row?445:128,row?32:34));
        try
        {
            var background=root.gameObject.AddComponent<UnityEngine.UI.Image>(); background.color=new Color32(18,28,26,235);
            var view=root.gameObject.AddComponent<LandingContributionView>(); view.targetGraphic=background;
            view.transition=UnityEngine.UI.Selectable.Transition.ColorTint;
            var colors=view.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color32(255,221,162,255);colors.selectedColor=colors.highlightedColor;view.colors=colors;
            var portrait=Rect(root,"Source portrait",new Rect(3,3,28,26)).gameObject.AddComponent<UnityEngine.UI.Image>(); portrait.preserveAspect=true;portrait.raycastTarget=false;
            var text=Text(root,"Contribution",new Rect(36,2,row?400:90,29),row?22:24);
            text.overflowMode=TextOverflowModes.Ellipsis; text.textWrappingMode=TextWrappingModes.NoWrap;
            var data=new SerializedObject(view);Bind(data,"portrait",portrait);Bind(data,"caption",text);data.FindProperty("includeName").boolValue=row;data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,row?RowPath:ChipPath);
        }
        finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
    }
    /// <summary>Provides clipped scrolling for any number of contributors or ongoing assistance sources.</summary>
    private static UnityEngine.UI.ScrollRect Scroll(Transform parent,string name,Rect bounds,bool vertical,out RectTransform content)
    {
        var root=Rect(parent,name,bounds); var scroll=root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        var viewport=Rect(root,"Viewport",new Rect(0,0,bounds.width,bounds.height));viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color=Color.clear;
        content=Rect(viewport,"Content",new Rect(0,0,bounds.width,bounds.height));scroll.viewport=viewport;scroll.content=content;
        scroll.horizontal=!vertical;scroll.vertical=vertical;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;return scroll;
    }
    /// <summary>Creates top-left reference coordinates inside the existing centered stage.</summary>
    private static RectTransform Rect(Transform parent,string name,Rect bounds)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;if(parent!=null)rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(bounds.x,-bounds.y);rect.sizeDelta=bounds.size;return rect;
    }
    /// <summary>Uses the supplied body font for native amounts and source labels.</summary>
    private static TMP_Text Text(Transform parent,string name,Rect bounds,float size)
    {
        var text=Rect(parent,name,bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root+"Fonts/VT323.asset");text.fontSize=size;text.color=new Color32(233,220,177,255);
        text.raycastTarget=false;text.richText=false;text.textWrappingMode=TextWrappingModes.NoWrap;return text;
    }
    /// <summary>Persists explicit references instead of requiring runtime hierarchy searches.</summary>
    private static void Bind(SerializedObject data,string name,UnityEngine.Object value)=>data.FindProperty(name).objectReferenceValue=value;
}
