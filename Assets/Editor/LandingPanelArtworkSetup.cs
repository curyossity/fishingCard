using System;
using UnityEditor;
using UnityEngine;

/// <summary>Authors independent panel sprites and transparent silhouettes without resampling or repainting the supplied machine.</summary>
public static class LandingPanelArtworkSetup
{
    public const string Folder = "Assets/UIThemes/TackleApparatus/Art/Landing/Separated";
    public const string CreaturePath = Folder + "/creature-panel.asset";
    public const string ScorePath = Folder + "/score-panel.asset";

    /// <summary>Returns the preserved standalone panel asset, creating its source slice and beveled contour only when absent.</summary>
    public static LandingPanelArtwork Get(Sprite master, bool score)
    {
        string path = score ? ScorePath : CreaturePath;
        var existing = AssetDatabase.LoadAssetAtPath<LandingPanelArtwork>(path);
        if (existing != null) return existing;
        if (AssetDatabase.LoadMainAssetAtPath(path) != null) throw new InvalidOperationException("Unexpected panel asset at " + path);
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/UIThemes/TackleApparatus/Art/Landing", "Separated");
        Rect region = score ? LandingCollisionGraphic.ScoreRegion : LandingCollisionGraphic.CreatureRegion;
        Rect shell = TackleMachineGeometry.StageBounds, source = master.rect;
        var pixels = new Rect(source.x + (region.x - shell.x) / shell.width * source.width,
            source.y + (1 - (region.yMax - shell.y) / shell.height) * source.height,
            region.width / shell.width * source.width, region.height / shell.height * source.height);
        // These source-pixel outlines follow the actual bevels, excluding the rectangular cabinet surround and neighboring trim.
        Vector2[] outline = score ? new[] { new Vector2(871,185), new Vector2(1482,185), new Vector2(1510,214), new Vector2(1510,476),
            new Vector2(1483,501), new Vector2(871,501), new Vector2(845,476), new Vector2(845,214) }
            : new[] { new Vector2(244,185), new Vector2(805,185), new Vector2(831,214), new Vector2(831,476),
                new Vector2(805,502), new Vector2(244,502), new Vector2(218,476), new Vector2(218,214) };
        for (int i = 0; i < outline.Length; i++)
            outline[i] = new Vector2((outline[i].x - pixels.x) / pixels.width, (master.texture.height - outline[i].y - pixels.y) / pixels.height);
        var artwork = ScriptableObject.CreateInstance<LandingPanelArtwork>();
        var face = Sprite.Create(master.texture, pixels, new Vector2(.5f,.5f), master.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        face.name = score ? "Score panel source" : "Creature panel source";
        try
        {
            AssetDatabase.CreateAsset(artwork, path); AssetDatabase.AddObjectToAsset(face, artwork);
            var data = new SerializedObject(artwork); data.FindProperty("face").objectReferenceValue = face;
            var points = data.FindProperty("silhouette"); points.arraySize = outline.Length;
            for (int i = 0; i < outline.Length; i++) points.GetArrayElementAtIndex(i).vector2Value = outline[i];
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(artwork); AssetDatabase.SaveAssetIfDirty(artwork);
            return artwork;
        }
        catch
        {
            if (!EditorUtility.IsPersistent(face)) UnityEngine.Object.DestroyImmediate(face);
            if (!EditorUtility.IsPersistent(artwork)) UnityEngine.Object.DestroyImmediate(artwork);
            throw;
        }
    }
}
