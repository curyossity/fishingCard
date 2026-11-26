using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the rarity-scaled creature entrance on the Landing prefab, reusing the collision panel, flash and stage owners.</summary>
public static class LandingCreatureRevealSetup
{
    /// <summary>Updates the existing Landing prefab in place; scene instances keep their profile and Harbor route.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install Creature Reveal")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Idempotent: binds existing native owners only, so full Landing regeneration retains the reveal.</summary>
    public static void Configure(GameObject root)
    {
        var collision = root.GetComponent<LandingCardCollision>();
        var camera = root.GetComponent<LandingImpactCamera>();
        var rush = root.GetComponent<LandingScoreRush>();
        var view = root.GetComponent<TackleLandingPresentation>();
        if (collision == null || camera == null || rush == null || view == null)
            throw new InvalidOperationException("Creature reveal requires the installed Landing collision, camera, score rush and presentation.");
        var reveal = root.GetComponent<LandingCreatureReveal>(); if (reveal == null) reveal = root.AddComponent<LandingCreatureReveal>();
        var source = new SerializedObject(collision); var viewData = new SerializedObject(view);
        var data = new SerializedObject(reveal);
        data.FindProperty("creatureCard").objectReferenceValue = source.FindProperty("creatureCard").objectReferenceValue;
        data.FindProperty("creatureFade").objectReferenceValue = source.FindProperty("creatureFade").objectReferenceValue;
        data.FindProperty("creatureFlash").objectReferenceValue = source.FindProperty("creatureFlash").objectReferenceValue;
        data.FindProperty("creatureName").objectReferenceValue = viewData.FindProperty("creature").objectReferenceValue;
        data.FindProperty("sound").objectReferenceValue = viewData.FindProperty("sound").objectReferenceValue;
        data.FindProperty("impactCamera").objectReferenceValue = camera;
        data.FindProperty("preferences").objectReferenceValue = rush;
        data.FindProperty("banner").objectReferenceValue = Banner(source, rush);
        data.ApplyModifiedPropertiesWithoutUndo();
        viewData.FindProperty("reveal").objectReferenceValue = reveal; viewData.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Places the Rare/Legendary tag across the lower creature card, above both panels and their flashes, never over the timing track.</summary>
    private static LandingRarityBanner Banner(SerializedObject collision, LandingScoreRush rush)
    {
        var stage = (RectTransform)collision.FindProperty("stage").objectReferenceValue;
        var card = (RectTransform)collision.FindProperty("creatureCard").objectReferenceValue;
        var rect = stage.Find("Rarity banner") as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject("Rarity banner", typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup), typeof(LandingRarityBanner)).transform;
        rect.SetParent(stage, false); rect.SetAsLastSibling();
        rect.anchorMin = card.anchorMin; rect.anchorMax = card.anchorMax; rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(420, 100); rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
        rect.anchoredPosition = card.anchoredPosition + new Vector2(0, -card.rect.height * card.localScale.y * .2f);
        var group = rect.GetComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
        var banner = rect.GetComponent<LandingRarityBanner>(); banner.raycastTarget = false;
        var data = new SerializedObject(banner);
        data.FindProperty("group").objectReferenceValue = group;
        data.FindProperty("preferences").objectReferenceValue = rush;
        foreach (string name in new[] { "Shadow", "Face" })
        {
            var label = rect.Find(name)?.GetComponent<TMP_Text>();
            if (label == null) label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(rect, false);
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/PressStart2P.asset");
            label.enableAutoSizing = true; label.fontSizeMin = 16; label.fontSizeMax = label.fontSize = 28;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.text = "";
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = label.rectTransform.pivot = new Vector2(.5f, .5f);
            label.rectTransform.sizeDelta = new Vector2(290, 34);
            label.rectTransform.anchoredPosition = name == "Shadow" ? new Vector2(2, 9) : new Vector2(0, 11);
            data.FindProperty(name == "Shadow" ? "shadow" : "face").objectReferenceValue = label;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        return banner;
    }
}
