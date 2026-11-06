using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Authors the approved equipment data and binds the existing ornate Tackle Shop without replacing its artwork.</summary>
public static class JournalTackleShopSetup
{
    public const string Root = JournalHarborBuilder.Root + "TackleShop/";
    public const string CatalogPath = Root + "TackleShopCatalog.asset";

    /// <summary>Creates GUID-stable equipment definitions and installs live controls in the candidate hub.</summary>
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath || scene.isDirty) throw new InvalidOperationException("Open the clean candidate scene.");
        var root = PrefabUtility.LoadPrefabContents(JournalHarborBuilder.PrefabPath);
        try { Bind(root); PrefabUtility.SaveAsPrefabAsset(root, JournalHarborBuilder.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        var controller = UnityEngine.Object.FindAnyObjectByType<FishingRunController>(); var data = new SerializedObject(controller);
        data.FindProperty("tackleShopCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(CatalogPath);
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Restores this binding during normal hub prefab regeneration.</summary>
    public static void Bind(GameObject root)
    {
        JournalEquipmentCatalogAuthoring.Rebuild();
        if (!JournalTackleShopDesignSync.IsAvailable) throw new InvalidOperationException("Import the approved shop artwork before building.");
        JournalTackleShopDesignSync.Bind(root);
    }
}
