using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the shared native tooltip and preserves it through all Tackle phase regeneration paths.</summary>
public static class TackleCardTooltipSetup
{
    public const string OverlayPath = TackleDiscoverArtSetup.Root + "Prefabs/CardTooltipOverlay.prefab";

    /// <summary>Installs inspection in existing prefabs without regenerating their approved artwork.</summary>
    [MenuItem("Fishing Cards/Card Tooltips/Install")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install tooltips outside Play Mode.");
        BuildOverlay();
        foreach (string path in new[] { TackleDiscoverArtSetup.RigPrefabPath, RigBuildingSetup.CabinetCardPath, RigBuildingSetup.RailCardPath })
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { ConfigureCard(root, path == RigBuildingSetup.RailCardPath); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        foreach (string path in new[] { TackleDiscoverArtSetup.PrefabPath, LandingChallengeSetup.PrefabPath, RigBuildingSetup.PrefabPath })
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try { ConfigurePhase(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Creature tooltips installed in Discover, Landing and Build, including both rig card masters.");
    }

    /// <summary>Serializes the face as its inspection target; no artwork or gameplay button is replaced.</summary>
    public static void ConfigureCard(GameObject root, bool above = false)
    {
        var view = root.GetComponent<TackleRigCardView>();
        var data = new SerializedObject(view);
        var face = (UnityEngine.UI.Image)data.FindProperty("face").objectReferenceValue;
        var source = root.GetComponent<TackleCardTooltipSource>() ?? root.AddComponent<TackleCardTooltipSource>();
        source.transition = UnityEngine.UI.Selectable.Transition.None;
        source.targetGraphic = face; face.raycastTarget = true;
        source.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Automatic };
        var sourceData = new SerializedObject(source);
        Bind(sourceData, "faceBounds", face.rectTransform);
        sourceData.FindProperty("above").boolValue = above; sourceData.ApplyModifiedPropertiesWithoutUndo();
        Bind(data, "tooltipSource", source); data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Adds one overlay beyond clipping masks, leaving the phase's existing controls and layout intact.</summary>
    public static void ConfigurePhase(GameObject root)
    {
        if (root.GetComponent<TackleCardTooltip>() != null) { RigCombinationPreviewSetup.Configure(root); return; }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPath) == null) BuildOverlay();
        var overlay = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPath));
        overlay.transform.SetParent(root.transform, false);
        var canvas = overlay.GetComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = root.GetComponent<Canvas>().sortingOrder + 1;
        var host = root.AddComponent<TackleCardTooltip>();
        var data = new SerializedObject(host);
        Bind(data, "overlay", overlay.transform);
        Bind(data, "panel", overlay.transform.Find("Panel"));
        Bind(data, "outline", overlay.transform.Find("SourceOutline"));
        foreach (string name in new[] { "title", "rarity", "metadata", "abilityName", "rules", "context" })
            Bind(data, name, overlay.transform.Find("Panel/" + name).GetComponent<TMP_Text>());
        data.ApplyModifiedPropertiesWithoutUndo();
        RigCombinationPreviewSetup.Configure(root);
    }

    /// <summary>Builds reusable editable geometry and native text from the reviewed 460-pixel SVG design.</summary>
    private static void BuildOverlay()
    {
        var root = Rect(null, "Creature Tooltip Overlay", Vector2.zero);
        try
        {
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.pivot = new Vector2(.5f, .5f); root.offsetMin = root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<Canvas>();
            var group = root.gameObject.AddComponent<CanvasGroup>(); group.interactable = false; group.blocksRaycasts = false;
            var panel = Rect(root, "Panel", new Vector2(460, 340));
            Fill(panel, "#141D1C"); Border(panel, "Brass", "#A67D42", 2);
            var inner = Rect(panel, "Inset", new Vector2(448, 328)); inner.anchoredPosition = new Vector2(6, -6);
            inner.anchorMin = Vector2.zero; inner.anchorMax = Vector2.one; inner.offsetMin = new Vector2(6,6); inner.offsetMax = new Vector2(-6,-6);
            Border(inner, "InnerRule", "#5F543B", 1);
            foreach (string name in new[] { "title", "rarity", "metadata", "abilityName", "rules", "context" })
            {
                var text = Rect(panel, name, new Vector2(412, 40)).gameObject.AddComponent<TextMeshProUGUI>();
                bool heading = name == "title";
                text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/" + (heading ? "PressStart2P" : "VT323") + ".asset");
                if (text.font == null) throw new InvalidOperationException("Missing supplied tooltip font.");
                text.fontSize = heading ? 16 : name == "rarity" || name == "context" ? 25 : 28;
                text.enableAutoSizing = false; text.textWrappingMode = TextWrappingModes.Normal;
                text.overflowMode = TextOverflowModes.Overflow; text.raycastTarget = false;
                text.color = ColorOf(name == "abilityName" ? "#E9BC63" : name == "rarity" || name == "context" ? "#BEBAA8" : "#F2E5C4");
                text.richText = name == "rules" || name == "metadata"; text.margin = Vector4.zero;
            }
            var outline = Rect(root, "SourceOutline", new Vector2(323,252)); Border(outline, "Focus", "#E9BC63", 2);
            panel.gameObject.SetActive(false); outline.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, OverlayPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    /// <summary>Creates geometry in parent-local top-left coordinates; overlay children use centered canvas coordinates.</summary>
    private static RectTransform Rect(Transform parent, string name, Vector2 size)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        if (parent != null) rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = parent != null && parent.name == "Creature Tooltip Overlay" ? new Vector2(.5f,.5f) : new Vector2(0,1);
        rect.pivot = new Vector2(0,1); rect.sizeDelta = size; return rect;
    }
    /// <summary>Draws an editable flat fill that cannot intercept UI input.</summary>
    private static void Fill(RectTransform rect, string color)
    { var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = ColorOf(color); image.raycastTarget = false; }
    /// <summary>Uses four native strips so borders stay crisp as content height grows.</summary>
    private static void Border(RectTransform parent, string name, string color, float thickness)
    {
        for (int i = 0; i < 4; i++)
        {
            var edge = Rect(parent, name + i, Vector2.zero); Fill(edge, color);
            edge.pivot = new Vector2(.5f,.5f);
            edge.anchorMin = i == 0 ? new Vector2(0,1) : Vector2.zero;
            edge.anchorMax = i == 1 ? new Vector2(1,0) : Vector2.one;
            if (i == 2) edge.anchorMax = new Vector2(0,1);
            if (i == 3) edge.anchorMin = new Vector2(1,0);
            edge.offsetMin = Vector2.zero; edge.offsetMax = Vector2.zero;
            if (i < 2) { edge.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, thickness); edge.anchoredPosition = new Vector2(0, i == 0 ? -thickness/2 : thickness/2); }
            else { edge.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, thickness); edge.anchoredPosition = new Vector2(i == 2 ? thickness/2 : -thickness/2, 0); }
        }
    }
    /// <summary>Parses the reviewed SVG palette without implicit color substitutions.</summary>
    private static Color ColorOf(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }
    /// <summary>Persists explicit view references into phase/card prefabs.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) { data.FindProperty(field).objectReferenceValue = value; }
}
