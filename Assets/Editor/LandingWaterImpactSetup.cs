using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the non-intercepting water layer using actual PixelFX frame, pivot and atlas metadata.</summary>
public static class LandingWaterImpactSetup
{
    private const string Folder = "Assets/FishingUIAssets/Effects/WaterImpact/";
    [Serializable] private sealed class Export { public Metadata meta; }
    [Serializable] private sealed class Metadata { public Size size; public PixelFX pixelfx; }
    [Serializable] private sealed class Size { public int w, h; }
    [Serializable] private sealed class PixelFX { public int fps, frames, columns; public int[] frameSize, pivot; }

    /// <summary>Preserves references and recreates the same effect layer through either Landing authoring path.</summary>
    public static void Configure(GameObject root)
    {
        var stage = root.transform.Find("1920x1080 Stage");
        var rect = stage.Find("Water collision spray") as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject("Water collision spray", typeof(RectTransform)).transform;
        rect.SetParent(stage, false); rect.SetAsLastSibling();
        var metadata = JsonUtility.FromJson<Export>(File.ReadAllText("Tools/PixelFX/Small.json")).meta;
        var layout = metadata.pixelfx;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(layout.pivot[0] / (float)layout.frameSize[0], 1 - layout.pivot[1] / (float)layout.frameSize[1]);
        rect.sizeDelta = new Vector2(layout.frameSize[0], layout.frameSize[1]) * 2;
        rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one;
        var image = rect.GetComponent<UnityEngine.UI.RawImage>(); if (image == null) image = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
        image.raycastTarget = false; image.enabled = false; image.color = Color.white;
        var owner = root.GetComponent<LandingWaterImpact>(); if (owner == null) owner = root.AddComponent<LandingWaterImpact>();
        var rush = new SerializedObject(root.GetComponent<LandingScoreRush>()); var data = new SerializedObject(owner);
        data.FindProperty("spray").objectReferenceValue = image;
        data.FindProperty("number").objectReferenceValue = ((TMPro.TMP_Text)rush.FindProperty("value").objectReferenceValue).rectTransform;
        data.FindProperty("collision").objectReferenceValue = root.GetComponent<LandingCardCollision>();
        data.FindProperty("frameCount").intValue = layout.frames; data.FindProperty("columns").intValue = layout.columns;
        data.FindProperty("rows").intValue = metadata.size.h / layout.frameSize[1]; data.FindProperty("fps").floatValue = layout.fps;
        var sheets = data.FindProperty("sheets"); sheets.arraySize = 3;
        string[] tiers = { "Small", "Medium", "Large" };
        for (int i = 0; i < tiers.Length; i++)
        {
            string path = Folder + tiers[i] + ".png";
            var other = JsonUtility.FromJson<Export>(File.ReadAllText("Tools/PixelFX/" + tiers[i] + ".json")).meta;
            if (other.pixelfx.frames != layout.frames || other.pixelfx.columns != layout.columns || other.size.w != metadata.size.w || other.size.h != metadata.size.h)
                throw new InvalidOperationException("Water impact sheet layouts disagree: " + tiers[i]);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default; importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            sheets.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        data.ApplyModifiedPropertiesWithoutUndo(); rush.FindProperty("waterImpact").objectReferenceValue = owner; rush.ApplyModifiedPropertiesWithoutUndo();
    }
}
