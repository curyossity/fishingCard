using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Owns temporary fixed-resolution Game View entries for actual output-pixel validation.</summary>
public static class JournalOutputChecks
{
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static EditorWindow window;
    private static object group;
    private static int originalIndex, customIndex = -1;
    private static Type viewType, sizeType, sizeModeType;

    /// <summary>Uses the installed Editor's reflected sizing API and restores the user's prior selection afterward.</summary>
    public static void Start()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Open the comparison scene and enter Play Mode.");
        Assembly assembly = typeof(Editor).Assembly; viewType = assembly.GetType("UnityEditor.GameView");
        sizeType = assembly.GetType("UnityEditor.GameViewSize"); sizeModeType = assembly.GetType("UnityEditor.GameViewSizeType");
        Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        object sizes = sizesType.BaseType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        group = sizesType.GetProperty("currentGroup", Flags).GetValue(sizes);
        window = EditorWindow.GetWindow(viewType); originalIndex = (int)viewType.GetProperty("selectedSizeIndex", Flags).GetValue(window);
        JournalPlayerValidation.StartEditor("Docs/UIThemes/MidnightFieldJournal", Resize, Restore);
    }
    /// <summary>Adds one fixed output size without persisting new entries in Editor preferences.</summary>
    private static void Resize(int width, int height)
    {
        RemoveTemporarySize();
        customIndex = (int)group.GetType().GetMethod("GetTotalCount", Flags).Invoke(group, null);
        object size = Activator.CreateInstance(sizeType, Flags, null, new[] { Enum.Parse(sizeModeType, "FixedResolution"), (object)width, height, "Journal output validation" }, null);
        group.GetType().GetMethod("AddCustomSize", Flags).Invoke(group, new[] { size });
        viewType.GetProperty("selectedSizeIndex", Flags).SetValue(window, customIndex); window.Focus(); window.Repaint();
    }
    /// <summary>Removes only the validation-owned size after switching back to a pre-existing selection.</summary>
    private static void RemoveTemporarySize()
    {
        if (customIndex < 0) return;
        viewType.GetProperty("selectedSizeIndex", Flags).SetValue(window, originalIndex);
        group.GetType().GetMethod("RemoveCustomSize", Flags).Invoke(group, new object[] { customIndex }); customIndex = -1;
    }
    /// <summary>Restores the previous size after success or a surfaced validation failure.</summary>
    private static void Restore() { RemoveTemporarySize(); window.Repaint(); }
}
