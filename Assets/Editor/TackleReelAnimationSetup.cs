using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Persists selectable drum-motion alternatives independently of housing and gameplay configuration.</summary>
public static class TackleReelAnimationSetup
{
    private const string SelectionPath = TackleDiscoverArtSetup.Root + "ReelAnimationSelection.json";
    [Serializable] private sealed class Selection { public int style; }

    /// <summary>Defaults older projects to the preserved continuous-wear animation.</summary>
    public static ReelEdgeAnimationStyle SelectedStyle => File.Exists(SelectionPath)
        ? (ReelEdgeAnimationStyle)JsonUtility.FromJson<Selection>(File.ReadAllText(SelectionPath)).style : ReelEdgeAnimationStyle.ContinuousWear;

    /// <summary>Selects the preceding continuous material-wear animation without changing the housing or reels.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Reel Motion/Use Continuous Wear")]
    public static void UseContinuousWear() { Install(ReelEdgeAnimationStyle.ContinuousWear); }

    /// <summary>Selects visibly articulated plates and rivets around each drum's circumference.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Reel Motion/Use Segmented Bands")]
    public static void UseSegmentedBands() { Install(ReelEdgeAnimationStyle.SegmentedBands); }

    /// <summary>Saves the chosen presentation variant in the project and updates the existing GUID-stable prefab.</summary>
    private static void Install(ReelEdgeAnimationStyle style)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Choose reel motion outside Play Mode.");
        File.WriteAllText(SelectionPath,JsonUtility.ToJson(new Selection { style=(int)style },true)+"\n");
        AssetDatabase.ImportAsset(SelectionPath);
        var root = PrefabUtility.LoadPrefabContents(TackleDiscoverArtSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root,TackleDiscoverArtSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the selected animation during full Discover, housing and material regeneration.</summary>
    public static void Configure(GameObject root)
    {
        var style = SelectedStyle;
        if (!Enum.IsDefined(typeof(ReelEdgeAnimationStyle),style)) throw new InvalidOperationException("Unknown reel motion style.");
        foreach (var surface in root.GetComponentsInChildren<TackleReelSurface>(true))
        {
            var data = new SerializedObject(surface); data.FindProperty("edgeAnimation").enumValueIndex=(int)style;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        TackleSlotAudioSetup.Configure(root);
    }
}
