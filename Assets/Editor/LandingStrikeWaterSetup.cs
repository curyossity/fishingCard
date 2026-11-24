using UnityEditor;
using UnityEngine;

/// <summary>Authors the timing-glass water below all existing hit-zone and fish graphics.</summary>
public static class LandingStrikeWaterSetup
{
    /// <summary>Reuses the track mask and preserves all marker geometry and native bindings during regeneration.</summary>
    public static void Configure(GameObject root)
    {
        var view = new SerializedObject(root.GetComponent<TackleLandingPresentation>());
        var track = (RectTransform)view.FindProperty("track").objectReferenceValue;
        var rect = track.Find("Strike water level") as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject("Strike water level", typeof(RectTransform)).transform;
        rect.SetParent(track, false); rect.SetAsFirstSibling();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.one * 4; rect.offsetMax = Vector2.one * -4;
        rect.localScale = Vector3.one;
        var water = rect.GetComponent<LandingStrikeWater>(); if (water == null) water = rect.gameObject.AddComponent<LandingStrikeWater>();
        water.raycastTarget = false;
        var rush = new SerializedObject(root.GetComponent<LandingScoreRush>());
        rush.FindProperty("strikeWater").objectReferenceValue = water; rush.ApplyModifiedPropertiesWithoutUndo();
    }
}
