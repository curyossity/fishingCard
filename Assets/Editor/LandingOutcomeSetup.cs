using System;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the Landed/Escaped outcome beat on the Landing prefab, reusing the collision panel, flash, stage and banner construction.</summary>
public static class LandingOutcomeSetup
{
    public const string BannerName = "Outcome banner", PayoutName = "Haul payout";

    /// <summary>Updates the existing Landing prefab in place; scene instances keep their profile and Harbor route.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install Landing Outcome")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Idempotent: binds existing native owners and reuses named layers, so full Landing regeneration retains the outcome.</summary>
    public static void Configure(GameObject root)
    {
        var collision = root.GetComponent<LandingCardCollision>();
        var camera = root.GetComponent<LandingImpactCamera>();
        var rush = root.GetComponent<LandingScoreRush>();
        var view = root.GetComponent<TackleLandingPresentation>();
        if (collision == null || camera == null || rush == null || view == null)
            throw new InvalidOperationException("Landing outcome requires the installed Landing collision, camera, score rush and presentation.");
        var outcome = root.GetComponent<LandingOutcome>(); if (outcome == null) outcome = root.AddComponent<LandingOutcome>();
        var source = new SerializedObject(collision); var viewData = new SerializedObject(view);
        var data = new SerializedObject(outcome);
        // The encounter creature lives on the receiving (right) card.
        data.FindProperty("creatureCard").objectReferenceValue = LandingCombatCardsSetup.CreatureCard(root);
        data.FindProperty("creatureFade").objectReferenceValue = LandingCombatCardsSetup.CreatureFade(root);
        data.FindProperty("creatureFlash").objectReferenceValue = LandingCombatCardsSetup.CreatureFlash(root);
        data.FindProperty("sound").objectReferenceValue = viewData.FindProperty("sound").objectReferenceValue;
        data.FindProperty("impactCamera").objectReferenceValue = camera;
        data.FindProperty("preferences").objectReferenceValue = rush;
        var payout = Payout(source, LandingCombatCardsSetup.CreatureCard(root));
        data.FindProperty("payout").objectReferenceValue = payout;
        data.FindProperty("payoutCaption").objectReferenceValue = payout.transform.Find("Caption").GetComponent<TMP_Text>();
        data.FindProperty("payoutValue").objectReferenceValue = payout.transform.Find("Value").GetComponent<TMP_Text>();
        data.FindProperty("banner").objectReferenceValue = LandingCreatureRevealSetup.Banner(root, rush, BannerName);
        // Assistance readouts sit in the recess behind the creature card; an escaped creature must not uncover them.
        var stage = (RectTransform)source.FindProperty("stage").objectReferenceValue;
        var behind = data.FindProperty("behindCard"); behind.arraySize = 0;
        foreach (string name in new[] { "Ongoing Assistance", "Assistance Totals" })
        {
            var layer = stage.Find(name); if (layer == null) continue;
            var group = layer.GetComponent<CanvasGroup>(); if (group == null) group = layer.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1; group.interactable = layer.GetComponentInChildren<UnityEngine.UI.Selectable>(true) != null; group.blocksRaycasts = true;
            behind.arraySize++; behind.GetArrayElementAtIndex(behind.arraySize - 1).objectReferenceValue = group;
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        viewData.FindProperty("outcome").objectReferenceValue = outcome; viewData.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>HAUL PAID readout centered in the creature frame, shown only once an escaped creature has left it.</summary>
    private static CanvasGroup Payout(SerializedObject collision, RectTransform card)
    {
        var stage = (RectTransform)collision.FindProperty("stage").objectReferenceValue;
        var rect = stage.Find(PayoutName) as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject(PayoutName, typeof(RectTransform), typeof(CanvasGroup)).transform;
        rect.SetParent(stage, false); rect.SetAsLastSibling();
        rect.anchorMin = card.anchorMin; rect.anchorMax = card.anchorMax; rect.pivot = card.pivot;
        rect.sizeDelta = new Vector2(card.rect.width * card.localScale.x, card.rect.height * card.localScale.y);
        rect.anchoredPosition = card.anchoredPosition; rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
        var group = rect.GetComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/PressStart2P.asset");
        Label(rect, "Caption", font, "HAUL PAID", 22, new Color32(233, 220, 177, 255), new Vector2(0, 44), new Vector2(360, 34));
        Label(rect, "Value", font, "0", 64, new Color32(255, 214, 102, 255), new Vector2(0, -22), new Vector2(380, 82));
        return group;
    }

    private static void Label(RectTransform parent, string name, TMP_FontAsset font, string text, float size, Color color, Vector2 position, Vector2 box)
    {
        var label = parent.Find(name)?.GetComponent<TMP_Text>();
        if (label == null) label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(parent, false);
        label.font = font; label.enableAutoSizing = true; label.fontSizeMin = size * .5f; label.fontSizeMax = label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.text = text; label.color = color;
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = label.rectTransform.pivot = new Vector2(.5f, .5f);
        label.rectTransform.sizeDelta = box; label.rectTransform.anchoredPosition = position; label.rectTransform.localScale = Vector3.one;
    }
}
