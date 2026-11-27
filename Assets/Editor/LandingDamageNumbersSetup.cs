using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Authors the floating damage numbers and the final damage slam on the creature's portrait on the Landing prefab. Idempotent; retained
/// by score-rush regeneration after the combat cards are placed, so it measures their final positions.
/// </summary>
public static class LandingDamageNumbersSetup
{
    public const string LayerName = "Damage numbers", TemplateName = "Floating damage template", CounterName = "Damage counter", SlamName = "Damage slam";
    public const string OutlinePath = LandingCombatCardsSetup.Folder + "damage-number-outline.mat";

    /// <summary>The pixel font's material with a thick dark outline, shared by every floating number.</summary>
    static Material OutlineMaterial(TMP_FontAsset font)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(OutlinePath);
        if (material == null) { material = new Material(font.material); AssetDatabase.CreateAsset(material, OutlinePath); }
        else material.shader = font.material.shader;
        material.mainTexture = font.material.mainTexture;
        material.EnableKeyword("OUTLINE_ON");
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, .32f);
        material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(16, 9, 5, 255));
        material.SetFloat(ShaderUtilities.ID_FaceDilate, .18f);
        EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        return material;
    }

    [MenuItem("Fishing Cards/Landing Feedback/Install Damage Numbers")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    public static void Configure(GameObject root)
    {
        var collision = root.GetComponent<LandingCardCollision>(); var rush = root.GetComponent<LandingScoreRush>();
        var view = root.GetComponent<TackleLandingPresentation>();
        if (collision == null || rush == null || view == null) throw new InvalidOperationException("Damage numbers require the installed Landing collision, score rush and presentation.");
        var stage = (RectTransform)new SerializedObject(collision).FindProperty("stage").objectReferenceValue;
        var font = ((TMP_Text)new SerializedObject(view).FindProperty("points").objectReferenceValue).font;
        var numbers = root.GetComponent<LandingDamageNumbers>(); if (numbers == null) numbers = root.AddComponent<LandingDamageNumbers>();

        // A full-stage, non-interacting layer above both cards; children use center anchors so anchored = local positions.
        var layer = stage.Find(LayerName) as RectTransform;
        if (layer == null) layer = (RectTransform)new GameObject(LayerName, typeof(RectTransform), typeof(CanvasGroup)).transform;
        layer.SetParent(stage, false); layer.SetAsLastSibling();
        layer.anchorMin = Vector2.zero; layer.anchorMax = Vector2.one; layer.pivot = new Vector2(.5f, .5f); layer.offsetMin = layer.offsetMax = Vector2.zero;
        var layerGroup = layer.GetComponent<CanvasGroup>(); layerGroup.blocksRaycasts = false; layerGroup.interactable = false;

        var outline = OutlineMaterial(font);
        var template = layer.Find(TemplateName) as RectTransform;
        if (template == null) template = (RectTransform)new GameObject(TemplateName, typeof(RectTransform)).transform;
        Center(template, layer, Vector2.zero, new Vector2(260, 72));
        // A solid dark outline (not an offset shadow) keeps the numbers crisp over parchment, metal and the dark recess alike.
        var oldShadow = template.Find("Shadow"); if (oldShadow != null) UnityEngine.Object.DestroyImmediate(oldShadow.gameObject);
        var faceText = Label(template, "Face", font, 40, LandingDamageNumbers.BaseColor, Vector2.zero, new Vector2(260, 72));
        faceText.fontSharedMaterial = outline;
        template.gameObject.SetActive(false);

        // The old running-total counter is replaced by the final slam.
        var oldCounter = layer.Find(CounterName); if (oldCounter != null) UnityEngine.Object.DestroyImmediate(oldCounter.gameObject);
        // Final slam: the strike's total, large and outlined, landing on the creature's portrait as the bar starts to drain.
        var slam = layer.Find(SlamName) as RectTransform;
        if (slam == null) slam = (RectTransform)new GameObject(SlamName, typeof(RectTransform), typeof(CanvasGroup)).transform;
        Center(slam, layer, Vector2.zero, new Vector2(360, 100));
        var slamGroup = slam.GetComponent<CanvasGroup>(); slamGroup.alpha = 0; slamGroup.blocksRaycasts = false; slamGroup.interactable = false;
        var value = Label(slam, "Value", font, 64, LandingDamageNumbers.SlamColor, Vector2.zero, new Vector2(360, 100));
        value.fontSharedMaterial = outline;
        slam.SetAsLastSibling();

        var data = new SerializedObject(numbers);
        data.FindProperty("layer").objectReferenceValue = layer;
        data.FindProperty("popupTemplate").objectReferenceValue = template;
        data.FindProperty("slam").objectReferenceValue = slamGroup;
        data.FindProperty("slamValue").objectReferenceValue = value;
        data.FindProperty("collision").objectReferenceValue = collision;
        data.FindProperty("slamTarget").objectReferenceValue = ((Component)new SerializedObject(view).FindProperty("portrait").objectReferenceValue).transform;
        data.ApplyModifiedPropertiesWithoutUndo();
        var rushData = new SerializedObject(rush); rushData.FindProperty("numbers").objectReferenceValue = numbers; rushData.ApplyModifiedPropertiesWithoutUndo();
        // The water impact's digit punch targets the slam number; the slam renders after the water layer and owns its scale.
        var water = root.GetComponent<LandingWaterImpact>();
        if (water != null) { var w = new SerializedObject(water); w.FindProperty("number").objectReferenceValue = value.rectTransform; w.ApplyModifiedPropertiesWithoutUndo(); }
    }

    static void Center(RectTransform rect, RectTransform parent, Vector2 position, Vector2 size)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    static TMP_Text Label(RectTransform parent, string name, TMP_FontAsset font, float size, Color color, Vector2 position, Vector2 box)
    {
        var label = parent.Find(name)?.GetComponent<TMP_Text>();
        if (label == null) label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        Center(label.rectTransform, parent, position, box);
        label.font = font; label.enableAutoSizing = false; label.fontSize = size; label.color = color;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.enableWordWrapping = false; label.overflowMode = TextOverflowModes.Overflow;
        return label;
    }
}
