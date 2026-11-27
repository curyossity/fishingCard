using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Authors the creature hurt reaction and Resistance damage chip on the Landing prefab, reusing the collision panel and progress bar.</summary>
public static class LandingDamageFeedbackSetup
{
    public const string AreaName = "Damage chip area", ChipName = "Damage chip";

    /// <summary>Updates the existing Landing prefab in place; scene instances keep their profile and Harbor route.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install Damage Feedback")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Idempotent: reuses the named chip layer and rebinds, so full Landing regeneration retains the damage feedback.</summary>
    public static void Configure(GameObject root)
    {
        var collision = root.GetComponent<LandingCardCollision>(); var rush = root.GetComponent<LandingScoreRush>();
        var view = root.GetComponent<TackleLandingPresentation>();
        if (collision == null || rush == null || view == null) throw new InvalidOperationException("Damage feedback requires the installed Landing collision, score rush and presentation.");
        var damage = root.GetComponent<LandingDamageFeedback>(); if (damage == null) damage = root.AddComponent<LandingDamageFeedback>();
        var source = new SerializedObject(collision); var viewData = new SerializedObject(view);
        var fill = (RectTransform)viewData.FindProperty("progressFill").objectReferenceValue;
        var clip = (RectTransform)fill.parent;
        // The chip sits beside the clipped bar, not inside it, so its swell can rise past the thin bar's edges.
        var area = clip.parent.Find(AreaName) as RectTransform;
        if (area == null) area = (RectTransform)new GameObject(AreaName, typeof(RectTransform)).transform;
        area.SetParent(clip.parent, false); area.SetSiblingIndex(clip.GetSiblingIndex() + 1);
        area.anchorMin = clip.anchorMin; area.anchorMax = clip.anchorMax; area.pivot = clip.pivot;
        area.anchoredPosition = clip.anchoredPosition; area.sizeDelta = clip.sizeDelta; area.localScale = clip.localScale; area.localRotation = clip.localRotation;
        var chip = area.Find(ChipName)?.GetComponent<Image>();
        if (chip == null) chip = new GameObject(ChipName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        chip.rectTransform.SetParent(area, false);
        chip.rectTransform.anchorMin = Vector2.zero; chip.rectTransform.anchorMax = new Vector2(0, 1); chip.rectTransform.pivot = new Vector2(.5f, .5f);
        chip.rectTransform.offsetMin = chip.rectTransform.offsetMax = Vector2.zero; chip.rectTransform.localScale = Vector3.one;
        chip.raycastTarget = false; chip.color = LandingDamageFeedback.ChipColor; chip.gameObject.SetActive(false);
        // The remaining-Resistance number and its plate stay readable above the white trail; the panel flash stays on top of all.
        var resistanceText = ((Component)viewData.FindProperty("resistance").objectReferenceValue).transform;
        foreach (var layer in new[] { area.parent.Find("Resistance plate"), resistanceText.parent == area.parent ? resistanceText : null })
            if (layer != null) layer.SetAsLastSibling();
        var flash = area.parent.GetComponentInChildren<LandingPanelFlash>(true); if (flash != null) flash.transform.SetAsLastSibling();
        var data = new SerializedObject(damage);
        data.FindProperty("collision").objectReferenceValue = collision;
        data.FindProperty("creatureCard").objectReferenceValue = LandingCombatCardsSetup.CreatureCard(root);
        data.FindProperty("creatureFlash").objectReferenceValue = LandingCombatCardsSetup.CreatureFlash(root);
        data.FindProperty("fish").objectReferenceValue = viewData.FindProperty("fish").objectReferenceValue;
        data.FindProperty("chip").objectReferenceValue = chip;
        data.ApplyModifiedPropertiesWithoutUndo();
        var rushData = new SerializedObject(rush); rushData.FindProperty("damage").objectReferenceValue = damage; rushData.ApplyModifiedPropertiesWithoutUndo();
    }
}
