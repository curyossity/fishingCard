using System;
using UnityEditor;
using UnityEngine;

/// <summary>Opts the candidate creature prefab into animated stat feedback without changing legacy card templates.</summary>
public static class JournalStatFeedbackSync
{
    /// <summary>Binds the existing dynamic numeric fields, preserving their artwork and layout.</summary>
    public static void Bind(CreatureCardView view)
    {
        var animator = view.GetComponent<CreatureCardStatAnimator>();
        if (animator == null) animator = view.gameObject.AddComponent<CreatureCardStatAnimator>();
        animator.ConfigureRapidTiming();
        var data = new SerializedObject(view); var fields = new SerializedObject(animator);
        fields.FindProperty("weightText").objectReferenceValue = data.FindProperty("weightText").objectReferenceValue;
        fields.FindProperty("valueText").objectReferenceValue = data.FindProperty("valueText").objectReferenceValue;
        fields.ApplyModifiedPropertiesWithoutUndo();
        data.FindProperty("statAnimator").objectReferenceValue = animator; data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Updates only the isolated candidate card; normal template builds call the same binding.</summary>
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Bind stat feedback outside Play Mode.");
        const string path = "Assets/UIThemes/MidnightFieldJournal/Prefabs/CreatureCard.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try { Bind(root.GetComponent<CreatureCardView>()); PrefabUtility.SaveAsPrefabAsset(root, path); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
