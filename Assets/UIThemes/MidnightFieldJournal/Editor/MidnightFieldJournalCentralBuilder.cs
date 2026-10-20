using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Authors the approved detailed central card from independent art, emblems and dynamic fields.</summary>
public static partial class MidnightFieldJournalPrefabBuilder
{
    /// <summary>Builds a reusable card whose gameplay content and role choices remain owned by the shared view.</summary>
    private static void Creature(CreatureCardVisualProfile profile)
    {
        GameObject root = Root("CreatureCard",1260,1400);
        var view = root.AddComponent<CreatureCardView>();
        var visuals = root.AddComponent<JournalCreatureCardVisuals>();
        Image background = Picture("Blank Creature Master",root.transform,"CreatureTemplate",new Rect(0,0,1,1));
        Image portrait = Portrait(root.transform,CentralBounds(78,102,1104,621.33f));
        Image overflow = Picture("Runtime Full Card Overlay",root.transform,null,new Rect(0,0,1,1));
        overflow.enabled = false;
        TMP_Text type = Label("Card Type",root.transform,CentralBounds(140,65,980,40),28,Ink,true);
        TMP_Text name = Label("Card Name",root.transform,CentralBounds(140,114,980,82),66,Ink,true);
        name.enableAutoSizing = true;name.fontSizeMin = 30;name.textWrappingMode = TextWrappingModes.NoWrap;
        TMP_Text weight = CentralStat(root.transform,"Weight","WEIGHT",130,307,175,174,133);
        TMP_Text value = CentralStat(root.transform,"Value","VALUE",506,243,134,128,115);
        RectTransform catchRegion = Region("Catch Effect Target",root.transform,profile.catchRules);
        RectTransform baitRegion = Region("Bait Effect Target",root.transform,profile.baitRules);
        TMP_Text catchText = Label("Catch Effect",catchRegion,profile.catchDescription,39,Paper);
        TMP_Text baitText = Label("Bait Effect",baitRegion,profile.baitDescription,33,new Color32(209,208,197,255));
        catchText.alignment = baitText.alignment = TextAlignmentOptions.TopLeft;
        TMP_Text catchName = Label("Catch Ability Name",catchRegion,new Rect(.07f,.5f,.85f,.17f),48,new Color32(240,208,170,255));
        TMP_Text baitName = Label("Bait Ability Name",baitRegion,new Rect(.07f,.5f,.85f,.17f),46,new Color32(209,208,197,255));
        // Keep the optional readability preset local to effect fields, without changing the font's shared default.
        Material effectShadow = AssetDatabase.LoadAssetAtPath<Material>(RootPath+"Profiles/EffectTextShadow.mat");
        if(effectShadow!=null)
            foreach(var text in new[]{catchText,baitText,catchName,baitName})
            {
                text.fontSharedMaterial=effectShadow;
                text.UpdateMeshPadding();
            }
        foreach(var t in new[]{catchName,baitName}) { t.enableAutoSizing=true;t.fontSizeMin=26;t.textWrappingMode=TextWrappingModes.NoWrap; }
        RectTransform roles = Region("Role Decoration",root.transform,new Rect(0,0,1,1));
        TMP_Text catchRole = Label("Catch Role",roles,CentralBounds(181,916,342,55),36,new Color32(230,189,128,255),true);catchRole.text="C A T C H";
        Label("Bait Role",roles,CentralBounds(780,916,300,55),36,theme.MutedInk,true).text="B A I T";
        Emblem("Catch Hook",roles,CentralBounds(109,906,45,65),JournalCardEmblem.Emblem.Hook,new Color32(214,164,105,255));
        Emblem("Bait Twin Spark",roles,CentralBounds(676,915,65,52),JournalCardEmblem.Emblem.TwinSpark,new Color32(165,169,165,255));
        RectTransform eventDecoration = Region("Event Decoration",root.transform,new Rect(0,0,1,1));
        TMP_Text eventType = Label("Event Category",eventDecoration,CentralBounds(260,801,740,46),32,new Color32(119,71,46,255),true);
        TMP_Text eventHeading = Label("Event Effect Heading",eventDecoration,CentralBounds(250,970,760,60),35,new Color32(214,183,132,255),true);
        eventHeading.text="E F F E C T";
        var eventEmblem=Region("Event Emblem",eventDecoration,CentralBounds(610,892,40,40)).gameObject.AddComponent<JournalEventEmblem>();
        eventEmblem.color=new Color32(119,71,46,255);eventEmblem.raycastTarget=false;
        eventDecoration.gameObject.SetActive(false);
        TMP_Text tags = Label("Creature Tags",root.transform,CentralBounds(250,752,760,34),22,new Color32(214,205,184,255),true);
        tags.enableAutoSizing=true;tags.fontSizeMin=18;tags.textWrappingMode=TextWrappingModes.NoWrap;
        RectTransform rarity = Region("Rarity Footer",root.transform,CentralBounds(803,805,348,44));
        var diamond=Emblem("Rarity Diamond",rarity,new Rect(0,11f/44,31f/348,31f/44),JournalCardEmblem.Emblem.Diamond,Ink);
        TMP_Text rarityName=Label("Rarity Name",rarity,new Rect(48f/348,2f/44,300f/348,42f/44),28,Ink,true);
        rarityName.alignment=TextAlignmentOptions.TopLeft;
        var data=new SerializedObject(view);
        Bind(data,"cardBackground",background);Bind(data,"creatureArtwork",portrait);Bind(data,"artworkOverflowLayer",overflow);
        Bind(data,"catchRoleLabel",roles.gameObject);Bind(data,"cardTypeText",type);Bind(data,"cardNameText",name);
        Bind(data,"weightText",weight);Bind(data,"valueText",value);Bind(data,"rulesRegion",catchRegion);Bind(data,"baitRulesRegion",baitRegion);
        Bind(data,"effectText",catchText);Bind(data,"baitEffectText",baitText);Bind(data,"fallbackCardFace",Sprite("CreatureTemplate"));Bind(data,"eventCardFace",Sprite("EventTemplate"));
        Bind(data,"visualProfile",profile);Bind(data,"catchAbilityText",catchName);Bind(data,"baitAbilityText",baitName);Bind(data,"visualExtension",visuals);data.ApplyModifiedPropertiesWithoutUndo();
        data=new SerializedObject(visuals);
        Bind(data,"theme",theme);Bind(data,"tagsText",tags);Bind(data,"rarityRoot",rarity.gameObject);Bind(data,"rarityText",rarityName);Bind(data,"rarityDiamond",diamond);
        Bind(data,"typeText",type);Bind(data,"nameText",name);Bind(data,"catchName",catchName);Bind(data,"catchText",catchText);Bind(data,"baitName",baitName);Bind(data,"baitText",baitText);
        Bind(data,"portraitRegion",portrait.transform.parent);Bind(data,"portrait",portrait.rectTransform);Bind(data,"roleDecoration",roles.gameObject);
        Bind(data,"eventDecoration",eventDecoration.gameObject);Bind(data,"eventTypeText",eventType);Bind(data,"eventEmblem",eventEmblem);
        Bind(data,"catchSelection",RoleGlow(catchRegion,"Selected Catch Border",true));Bind(data,"baitSelection",RoleGlow(baitRegion,"Selected Bait Border",false));
        data.FindProperty("showRarity").boolValue=true;data.ApplyModifiedPropertiesWithoutUndo();
        JournalRoleSelectionSync.ConfigureFading(visuals,profile);
        JournalStatFeedbackSync.Bind(view);
        Save(root,"CreatureCard");
    }

    /// <summary>Builds a subtle native selected-role edge using the unselected fade's contour.</summary>
    private static JournalCardOutline RoleGlow(Transform parent,string name,bool catchRole)
    {
        var border=Edge(parent,name,new Color32(255,209,141,242),0,3,26);
        Edge(border.transform,"Outer Warm Glow",new Color32(255,166,66,24),-8,16,34);
        Edge(border.transform,"Inner Warm Glow",new Color32(255,166,66,45),-3,6,29);
        JournalRoleSelectionSync.ConfigureBorder(border,catchRole);
        return border;
    }

    /// <summary>Creates one independently colored native emblem using the approved source paths.</summary>
    private static JournalCardEmblem Emblem(string name,Transform parent,Rect bounds,JournalCardEmblem.Emblem kind,Color color)
    {
        var emblem=Region(name,parent,bounds).gameObject.AddComponent<JournalCardEmblem>();emblem.color=color;emblem.raycastTarget=false;
        var data=new SerializedObject(emblem);data.FindProperty("emblem").intValue=(int)kind;data.ApplyModifiedPropertiesWithoutUndo();return emblem;
    }

    /// <summary>Converts the current central-card top-left coordinates into normalized anchors.</summary>
    private static Rect CentralBounds(float x,float y,float w,float h) { return JournalCreatureCardVisuals.Pixels(x,y,w,h); }

    /// <summary>Keeps fixed labels and mutable numbers under event-hidden stat parents.</summary>
    private static TMP_Text CentralStat(Transform parent,string name,string caption,float x,float width,float labelWidth,float valueX,float valueWidth)
    {
        var region=Region(name+" Stat",parent,CentralBounds(x,786,width,75));
        Label(name+" Label",region,new Rect(0,17f/75,labelWidth/width,39f/75),28,Ink,true).text=caption;
        return Label(name+" Value",region,new Rect(valueX/width,0,valueWidth/width,1),64,Ink);
    }
}
