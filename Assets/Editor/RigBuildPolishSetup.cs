using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors native Build feedback around the existing supplied machine and reusable card masters.</summary>
public static class RigBuildPolishSetup
{
    /// <summary>Installs into the GUID-stable Build prefab without rebuilding the scene or source artwork.</summary>
    [MenuItem("Fishing Cards/Rig Building/Install Placement Polish")]
    public static void Install()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Install outside Play mode.");
        var root=PrefabUtility.LoadPrefabContents(RigBuildingSetup.PrefabPath);
        try{Configure(root);PrefabUtility.SaveAsPrefabAsset(root,RigBuildingSetup.PrefabPath);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
    }
    /// <summary>Retains serialized view ownership when Build's standard authoring path regenerates its prefab.</summary>
    public static void Configure(GameObject root)
    {
        var view=root.GetComponent<RigBuildingView>();if(view==null)return;
        var host=root.GetComponent<RigBuildPresentation>();if(host==null)host=root.AddComponent<RigBuildPresentation>();
        var viewData=new SerializedObject(view);var data=new SerializedObject(host);var stage=root.transform.Find("1920x1080 Stage");
        var flight=stage.Find("Placement flight layer") as RectTransform;
        if(flight==null){flight=(RectTransform)new GameObject("Placement flight layer",typeof(RectTransform),typeof(CanvasGroup)).transform;flight.SetParent(stage,false);flight.anchorMin=Vector2.zero;flight.anchorMax=Vector2.one;flight.offsetMin=flight.offsetMax=Vector2.zero;flight.pivot=new Vector2(.5f,.5f);flight.GetComponent<CanvasGroup>().blocksRaycasts=false;flight.GetComponent<CanvasGroup>().interactable=false;}
        Bind(data,"flightLayer",flight);
        var label=Text(stage,"Placement change template",new Rect(0,-22,221,21));label.enableAutoSizing=true;label.fontSizeMin=17;label.fontSizeMax=23;label.overflowMode=TextOverflowModes.Ellipsis;label.gameObject.SetActive(false);Bind(data,"changeTemplate",label);
        var shadow=label.GetComponent<UnityEngine.UI.Outline>();if(shadow==null)shadow=label.gameObject.AddComponent<UnityEngine.UI.Outline>();shadow.effectColor=new Color(0,0,0,.9f);shadow.effectDistance=new Vector2(1,-1);
        var receipt=Text(stage,"Acquisition effect change",new Rect(550,846,1085,44));receipt.textWrappingMode=TextWrappingModes.Normal;receipt.enableAutoSizing=true;receipt.fontSizeMin=18;receipt.fontSizeMax=22;receipt.gameObject.SetActive(false);Bind(data,"receiptChange",receipt);
        Bind(data,"levelLabel",viewData.FindProperty("levelLabel").objectReferenceValue);
        var feedback=root.GetComponent<LandingStrikeFeedback>();
        if(feedback!=null)
        {
            var feedbackData=new SerializedObject(feedback);var layers=data.FindProperty("secondaryLayers");layers.arraySize=2;
            ((TMP_Text)feedbackData.FindProperty("attemptFeedback").objectReferenceValue).gameObject.SetActive(false);
            var chips=((UnityEngine.UI.ScrollRect)feedbackData.FindProperty("chipScroll").objectReferenceValue).gameObject;
            var equation=((TMP_Text)feedbackData.FindProperty("equation").objectReferenceValue).gameObject;
            foreach(var pair in new[]{(chips,0),(equation,1)}){var group=pair.Item1.GetComponent<CanvasGroup>();if(group==null)group=pair.Item1.AddComponent<CanvasGroup>();layers.GetArrayElementAtIndex(pair.Item2).objectReferenceValue=group;}
        }
        data.ApplyModifiedPropertiesWithoutUndo();Bind(viewData,"buildFeedback",host);
        var status=(GameObject)viewData.FindProperty("upgradeStatus").objectReferenceValue;Bind(viewData,"acquisitionStatus",status.transform.Find("UpgradeLabel").GetComponent<TMP_Text>());viewData.ApplyModifiedPropertiesWithoutUndo();
        DeepwaterAudioSetup.ConfigureBuild(root);
    }
    /// <summary>Creates or reuses a non-intercepting native label in reference-stage coordinates.</summary>
    private static TMP_Text Text(Transform parent,string name,Rect bounds)
    {
        var rect=parent.Find(name) as RectTransform;if(rect==null){rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);}
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(bounds.x,-bounds.y);rect.sizeDelta=bounds.size;
        var text=rect.GetComponent<TMP_Text>();if(text==null)text=rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root+"Fonts/VT323.asset");text.fontSize=23;text.richText=false;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.NoWrap;text.alignment=TextAlignmentOptions.Center;text.color=new Color32(233,188,99,255);return text;
    }
    /// <summary>Stores explicit references so runtime code needs no scene searches.</summary>
    private static void Bind(SerializedObject data,string name,UnityEngine.Object value)=>data.FindProperty(name).objectReferenceValue=value;
}
