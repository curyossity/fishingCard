using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the pointer-free detailed tooltip with native fields above its independent engraved artwork.</summary>
public static partial class MidnightFieldJournalPrefabBuilder
{
    /// <summary>Converts top-left Penpot tooltip geometry to normalized Unity anchors.</summary>
    private static Rect TooltipBounds(float x, float y, float w, float h) { return new Rect(x / 900, 1 - (y + h) / 640, w / 900, h / 640); }

    /// <summary>Builds the exact current tooltip frame, role icons and editable role-aware fields.</summary>
    private static void CatchTooltip()
    {
        var root = Root("CatchTooltip", 900, 640);
        var view = root.AddComponent<JournalCatchTooltip>();
        var group = root.AddComponent<CanvasGroup>(); group.interactable = false; group.blocksRaycasts = false;
        var frame = Region("Engraved Master", root.transform, TooltipBounds(28, 16, 856, 608)).gameObject.AddComponent<UnityEngine.UI.Image>();
        frame.sprite = theme.CatchTooltipFrame; frame.raycastTarget = false;
        TMP_Text name = Label("Creature Name", root.transform, TooltipBounds(140, 85, 640, 80), 46, Ink, true);
        name.font = title; name.enableAutoSizing = true; name.fontSizeMin = 28; name.fontSizeMax = 46;
        TMP_Text ability = Label("Active Effect Name", root.transform, TooltipBounds(190, 355, 580, 66), 42, Paper);
        ability.alignment = TextAlignmentOptions.TopLeft; ability.enableAutoSizing = true; ability.fontSizeMin = 26; ability.fontSizeMax = 42;
        TMP_Text rules = Label("Active Effect Rules", root.transform, TooltipBounds(182, 432, 548, 142), 33, Paper);
        rules.alignment = TextAlignmentOptions.Top;
        rules.enableAutoSizing = true; rules.fontSizeMin = 24; rules.fontSizeMax = 33;
        var tags = Region("Effective Tags", root.transform, TooltipBounds(110, 197, 680, 46));
        var pill = Region("Tag Prototype", tags, new Rect(.5f, .5f, 0, 0)); pill.sizeDelta = new Vector2(170, 46);
        var face = pill.gameObject.AddComponent<UnityEngine.UI.Image>(); face.color = new Color32(221, 195, 157, 90); face.raycastTarget = false;
        Edge(pill, "Etched Tag Border", new Color32(166, 125, 77, 150), 0, 1.2f, 6);
        var tag = Label("Tag Text", pill, new Rect(0, 0, 1, 1), 26, Ink); tag.margin = new Vector4(12, 0, 12, 0); pill.gameObject.SetActive(false);
        var hook = TooltipIcon(root.transform, "Catch Hook", TooltipBounds(116, 355, 34, 50), JournalCardEmblem.Emblem.Hook);
        var spark = TooltipIcon(root.transform, "Bait Twin Spark", TooltipBounds(116, 355, 55, 43), JournalCardEmblem.Emblem.TwinSpark);
        var data = new SerializedObject(view); Bind(data, "creatureName", name); Bind(data, "abilityName", ability); Bind(data, "description", rules);
        Bind(data, "tagsRoot", tags); Bind(data, "tagPrototype", tag); Bind(data, "catchIcon", hook); Bind(data, "baitIcon", spark); data.ApplyModifiedPropertiesWithoutUndo();
        Save(root, "CatchTooltip");
    }

    /// <summary>Reuses the approved native hook/twin-spark meshes, never a role heading or pointer.</summary>
    private static GameObject TooltipIcon(Transform parent, string name, Rect bounds, JournalCardEmblem.Emblem type)
    {
        var icon = Region(name, parent, bounds).gameObject.AddComponent<JournalCardEmblem>(); icon.color = theme.Copper; icon.raycastTarget = false;
        var data = new SerializedObject(icon); data.FindProperty("emblem").enumValueIndex = (int)type; data.ApplyModifiedPropertiesWithoutUndo(); return icon.gameObject;
    }
}
