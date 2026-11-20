using System;
using UnityEditor;
using UnityEngine;

/// <summary>Installs native, non-intercepting relationship glows without rebuilding Build or repainting its art.</summary>
public static class RigCombinationPreviewSetup
{
    /// <summary>Updates the existing Build prefab; scene instances inherit the serialized bindings.</summary>
    [MenuItem("Fishing Cards/Rig Building/Install Combination Preview")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play mode.");
        var root = PrefabUtility.LoadPrefabContents(RigBuildingSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, RigBuildingSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }
    /// <summary>Retains the feature during normal Build and tooltip regeneration.</summary>
    public static void Configure(GameObject root)
    {
        var build = root.GetComponent<RigBuildingView>(); var tooltip = root.GetComponent<TackleCardTooltip>();
        if (build == null || tooltip == null) return;
        var view = root.GetComponent<RigCombinationPreview>();
        if (view == null) view = root.AddComponent<RigCombinationPreview>();
        var tooltipData = new SerializedObject(tooltip);
        var tooltipLayer = (RectTransform)tooltipData.FindProperty("overlay").objectReferenceValue;
        var layer = (tooltipLayer.Find("Combination glows") ?? tooltipLayer.Find("Combination outlines")) as RectTransform;
        if (layer == null)
        {
            layer = (RectTransform)new GameObject("Combination glows", typeof(RectTransform)).transform;
            layer.SetParent(tooltipLayer, false); layer.SetAsFirstSibling();
            layer.anchorMin = Vector2.zero; layer.anchorMax = Vector2.one; layer.offsetMin = layer.offsetMax = Vector2.zero;
            layer.pivot = new Vector2(0, 1);
            var group = layer.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
        }
        layer.name = "Combination glows";
        var template = (layer.Find("Relationship glow template") ?? layer.Find("Relationship outline template")) as RectTransform;
        if (template == null)
        {
            template = (RectTransform)new GameObject("Relationship glow template", typeof(RectTransform)).transform;
            template.SetParent(layer, false); template.anchorMin = template.anchorMax = template.pivot = new Vector2(0, 1);
            template.gameObject.SetActive(false);
        }
        template.name = "Relationship glow template";
        // The old Image binding has been retired; Unity permits only one Graphic on this object.
        var oldFrame = template.GetComponent<UnityEngine.UI.Image>();
        if (oldFrame != null) UnityEngine.Object.DestroyImmediate(oldFrame);
        var glow = template.GetComponent<RigRelationshipGlow>();
        if (glow == null) glow = template.gameObject.AddComponent<RigRelationshipGlow>();
        glow.raycastTarget = false;
        var data = new SerializedObject(view);
        data.FindProperty("tooltip").objectReferenceValue = tooltip; data.FindProperty("overlay").objectReferenceValue = layer;
        data.FindProperty("glowTemplate").objectReferenceValue = glow; data.ApplyModifiedPropertiesWithoutUndo();
        var buildData = new SerializedObject(build); buildData.FindProperty("combinations").objectReferenceValue = view; buildData.ApplyModifiedPropertiesWithoutUndo();
    }
}
