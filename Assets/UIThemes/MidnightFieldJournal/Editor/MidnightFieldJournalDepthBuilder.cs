using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the independent engraved timeline and its editable pooled tier prototype.</summary>
public static partial class MidnightFieldJournalPrefabBuilder
{
    /// <summary>Builds the recorded guide geometry without baking authored biome labels into artwork.</summary>
    private static JournalDepthGuide BuildDepthGuide(Transform parent)
    {
        RectTransform root=Region("Biome Tiers",parent,ScreenBounds(2840,1130,760,400));
        var guide=root.gameObject.AddComponent<JournalDepthGuide>();
        var line=Region("Depth Connecting Line",root,new Rect(49f/760,95f/550,2f/760,350f/550));
        var lineImage=line.gameObject.AddComponent<UnityEngine.UI.Image>();lineImage.color=new Color32(132,150,154,191);lineImage.raycastTarget=false;
        RectTransform row=Region("Illustrated Tier Template",root,new Rect(0,0,1,175f/550));
        var tier=row.gameObject.AddComponent<JournalDepthTierView>();
        const string materialPath=RootPath+"Profiles/DepthGuideInactive.mat";
        var shader=AssetDatabase.LoadAssetAtPath<Shader>(RootPath+"Shaders/DepthGuideGrayscale.shader");
        if(shader==null)throw new System.InvalidOperationException("Depth-guide grayscale shader is required.");
        var inactiveMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(inactiveMaterial==null){inactiveMaterial=new Material(shader);AssetDatabase.CreateAsset(inactiveMaterial,materialPath);}
        var nodeRect=Region("Native Tier Node",row,new Rect(0,0,0,0));nodeRect.anchorMin=nodeRect.anchorMax=new Vector2(0,.5f);nodeRect.pivot=new Vector2(.5f,.5f);nodeRect.anchoredPosition=new Vector2(50,0);nodeRect.sizeDelta=Vector2.one*82;
        var node=nodeRect.gameObject.AddComponent<JournalDepthNode>();node.raycastTarget=false;
        var artRect=Region("Independent Coral Illustration",row,new Rect(0,0,0,0));artRect.anchorMin=artRect.anchorMax=new Vector2(0,.5f);artRect.pivot=new Vector2(0,.5f);artRect.anchoredPosition=new Vector2(98,0);artRect.sizeDelta=new Vector2(205,153.75f);
        var art=artRect.gameObject.AddComponent<UnityEngine.UI.Image>();art.raycastTarget=false;art.preserveAspect=true;
        TMP_Text label=Label("Authored Tier Name",row,new Rect(340f/760,1-106.5f/175,410f/760,49f/175),32,Paper,true);
        TMP_Text range=Label("Authored Tier Range",row,new Rect(340f/760,1-143.5f/175,410f/760,33f/175),24,Paper);
        foreach(var text in new[]{label,range}){text.alignment=TextAlignmentOptions.TopLeft;text.enableAutoSizing=true;text.textWrappingMode=TextWrappingModes.NoWrap;text.raycastTarget=false;}
        var data=new SerializedObject(tier);Bind(data,"label",label);Bind(data,"range",range);Bind(data,"engraving",art);Bind(data,"node",node);Bind(data,"inactiveIllustrationMaterial",inactiveMaterial);data.ApplyModifiedPropertiesWithoutUndo();row.gameObject.SetActive(false);
        data=new SerializedObject(guide);Bind(data,"prototype",tier);Bind(data,"line",line);Bind(data,"shallows",Sprite("Depth_shallows"));Bind(data,"reef",Sprite("Depth_reef"));Bind(data,"deepWater",Sprite("Depth_deep_water"));data.ApplyModifiedPropertiesWithoutUndo();return guide;
    }
}
