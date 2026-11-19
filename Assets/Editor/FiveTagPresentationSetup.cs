using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Migrates existing native preparation controls and reel mappings without rebuilding approved machine artwork.</summary>
public static class FiveTagPresentationSetup
{
    /// <summary>Installs the five supported symbols in saved prefabs and their currently loaded scene instances.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Five Tag Presentation")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before migrating presentation.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isDirty) throw new InvalidOperationException("Save the current scene before migrating its presentation.");
        UpdatePrefab(SlotMachineSetup.PreparationPath);
        UpdatePrefab(TackleDiscoverArtSetup.PrefabPath);
        foreach (var view in UnityEngine.Object.FindObjectsByType<SlotTagPreparationView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (view.gameObject.scene.IsValid()) UpdatePreparation(view);
        foreach (var view in UnityEngine.Object.FindObjectsByType<TackleDiscoverPresentation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (view.gameObject.scene.IsValid()) UpdateSymbols(view);
        foreach (var view in UnityEngine.Object.FindObjectsByType<SlotTagPreparationView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (view.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
        AssetDatabase.SaveAssets();
        if (scene.isDirty) EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Edits an existing prefab in isolation, preserving its identity and unrelated objects.</summary>
    private static void UpdatePrefab(string path)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var view in root.GetComponentsInChildren<SlotTagPreparationView>(true)) UpdatePreparation(view);
            foreach (var view in root.GetComponentsInChildren<TackleDiscoverPresentation>(true)) UpdateSymbols(view);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>Retains five native buttons, hides obsolete controls and updates their serialized captions.</summary>
    private static void UpdatePreparation(SlotTagPreparationView view)
    {
        var data = new SerializedObject(view);
        var buttons = data.FindProperty("tagButtons"); var labels = data.FindProperty("tagLabels");
        for (int i = 0; i < buttons.arraySize; i++)
        {
            var button = (UnityEngine.UI.Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
            button.gameObject.SetActive(i < CreatureTagVocabulary.All.Length);
            if (i >= CreatureTagVocabulary.All.Length) continue;
            button.name = CreatureTagVocabulary.All[i];
            ((TMPro.TMP_Text)labels.GetArrayElementAtIndex(i).objectReferenceValue).text = CreatureTagVocabulary.All[i];
        }
        buttons.arraySize = labels.arraySize = CreatureTagVocabulary.All.Length;
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(view);
    }

    /// <summary>Reuses the supplied illustrations for the five active gameplay identities.</summary>
    private static void UpdateSymbols(TackleDiscoverPresentation view)
    {
        var data = new SerializedObject(view); TackleReelSymbolSetup.Bind(data);
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(view);
    }
}
