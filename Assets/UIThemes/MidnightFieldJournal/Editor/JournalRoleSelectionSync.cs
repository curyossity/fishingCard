using System;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the selected-role treatment against the detailed master's rounded panel edges.</summary>
public static class JournalRoleSelectionSync
{
    private const string Root = "Assets/UIThemes/MidnightFieldJournal/";
    private const float RoleBorderInset = 6;

    /// <summary>Shares the asymmetric contour between all role-feedback layers.</summary>
    private static Vector4 RoleCorners(bool catchRole) { return catchRole ? new Vector4(24,24,66,24) : new Vector4(24,24,24,66); }

    /// <summary>Places nonblocking fade overlays above the complete panel, including its separately parented heading.</summary>
    public static void ConfigureFading(JournalCreatureCardVisuals visuals, CreatureCardVisualProfile profile)
    {
        var data = new SerializedObject(visuals);
        foreach (string role in new[] { "Catch", "Bait" })
        {
            var overlay = visuals.transform.Find(role + " Unselected Fade");
            if (overlay == null)
            {
                var obj = new GameObject(role + " Unselected Fade", typeof(RectTransform), typeof(JournalRoleFade));
                obj.transform.SetParent(visuals.transform, false); obj.layer = visuals.gameObject.layer; overlay = obj.transform;
            }
            var rect = (RectTransform)overlay;
            Rect bounds = role == "Catch" ? profile.catchRules : profile.baitRules;
            rect.anchorMin = bounds.min; rect.anchorMax = bounds.max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            overlay.SetAsLastSibling();
            var fade = overlay.GetComponent<JournalRoleFade>(); fade.color = new Color(0, 0, 0, .58f); fade.raycastTarget = false;
            var shape = new SerializedObject(fade);
            shape.FindProperty("borderInset").floatValue = RoleBorderInset;
            shape.FindProperty("cornerRadii").vector4Value = RoleCorners(role == "Catch");
            shape.FindProperty("softInnerGlow").boolValue = false;
            shape.ApplyModifiedPropertiesWithoutUndo(); fade.SetVerticesDirty();
            data.FindProperty(role == "Catch" ? "catchFade" : "baitFade").objectReferenceValue = fade;
            overlay.gameObject.SetActive(false);
            var glow = visuals.transform.Find(role + " Selected Inner Glow");
            if (glow == null)
            {
                var obj = new GameObject(role + " Selected Inner Glow", typeof(RectTransform), typeof(JournalRoleFade));
                obj.transform.SetParent(visuals.transform, false); obj.layer = visuals.gameObject.layer; glow = obj.transform;
            }
            var glowRect = (RectTransform)glow;
            glowRect.anchorMin = bounds.min; glowRect.anchorMax = bounds.max;
            glowRect.offsetMin = glowRect.offsetMax = Vector2.zero; glow.SetAsLastSibling();
            var illumination = glow.GetComponent<JournalRoleFade>();
            illumination.color = new Color32(242, 191, 111, 50); illumination.raycastTarget = false;
            var glowShape = new SerializedObject(illumination);
            glowShape.FindProperty("borderInset").floatValue = RoleBorderInset;
            glowShape.FindProperty("cornerRadii").vector4Value = RoleCorners(role == "Catch");
            glowShape.FindProperty("softInnerGlow").boolValue = true;
            glowShape.FindProperty("glowWidth").floatValue = 24;
            glowShape.ApplyModifiedPropertiesWithoutUndo(); illumination.SetVerticesDirty();
            data.FindProperty(role == "Catch" ? "catchGlow" : "baitGlow").objectReferenceValue = illumination;
            glow.gameObject.SetActive(false);
            visuals.transform.Find(role + " Effect Target/Selected " + role + " Border").gameObject.SetActive(false);
        }
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Uses the panel's complete boundary rather than the smaller rules-content rectangle.</summary>
    public static void ConfigureProfile(CreatureCardVisualProfile profile)
    {
        profile.catchRules = JournalCreatureCardVisuals.Pixels(82, 875, 545, 438);
        profile.baitRules = JournalCreatureCardVisuals.Pixels(634, 875, 545, 438);
    }

    /// <summary>Draws a subtle gold edge on the fade's contour without the earlier thick border, glow or check.</summary>
    public static void ConfigureBorder(JournalCardOutline border, bool catchRole)
    {
        Stroke(border, new Color32(246, 207, 147, 170), RoleBorderInset, 3, 24);
        var shape = new SerializedObject(border);
        shape.FindProperty("useCornerRadii").boolValue = true;
        shape.FindProperty("cornerRadii").vector4Value = RoleCorners(catchRole);
        shape.ApplyModifiedPropertiesWithoutUndo(); border.SetVerticesDirty();
        foreach (Transform decoration in border.transform) decoration.gameObject.SetActive(false);
    }

    /// <summary>Applies identical style values to existing assets and future builder output.</summary>
    private static void Stroke(JournalCardOutline edge, Color color, float inset, float thickness, float radius)
    {
        edge.color = color; edge.raycastTarget = false;
        var data = new SerializedObject(edge);
        data.FindProperty("inset").floatValue = inset;
        data.FindProperty("thickness").floatValue = thickness;
        data.FindProperty("radius").floatValue = radius;
        data.ApplyModifiedPropertiesWithoutUndo();
        edge.SetVerticesDirty();
    }

    /// <summary>Updates only the candidate creature prefab and profile, preserving source art and other templates.</summary>
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Apply role styling outside Play Mode.");
        var profile = AssetDatabase.LoadAssetAtPath<CreatureCardVisualProfile>(Root + "Profiles/CreatureVisualProfile.asset");
        ConfigureProfile(profile); EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile);
        string path = Root + "Prefabs/CreatureCard.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (string role in new[] { "Catch", "Bait" })
            {
                var region = (RectTransform)root.transform.Find(role + " Effect Target");
                Rect bounds = role == "Catch" ? profile.catchRules : profile.baitRules;
                region.anchorMin = bounds.min; region.anchorMax = bounds.max;
                region.offsetMin = region.offsetMax = Vector2.zero;
                ConfigureBorder(region.Find("Selected " + role + " Border").GetComponent<JournalCardOutline>(), role == "Catch");
            }
            ConfigureFading(root.GetComponent<JournalCreatureCardVisuals>(), profile);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
