using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Creates a separate playable comparison scene using existing authored game setup and isolated journal UI.</summary>
public static class JournalComparisonSceneBuilder
{
    public const string ScenePath = MidnightFieldJournalAssetSetup.Root + "Scenes/MidnightFieldJournalComparison.unity";
    private const string SourcePath = "Assets/Scenes/SampleScene.unity";

    /// <summary>Regenerates the candidate scene without altering the original scene or build selection.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Build Playable Comparison Scene")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play Mode.");
        if (SceneManager.GetSceneByPath(ScenePath).isLoaded && SceneManager.GetSceneByPath(ScenePath).isDirty) throw new InvalidOperationException("Save or close the edited comparison scene before regenerating it.");
        byte[] sourceBytes = File.ReadAllBytes(SourcePath), buildBytes = File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset");
        Scene active = SceneManager.GetActiveScene(); Scene source = EditorSceneManager.OpenPreviewScene(SourcePath);
        Scene target = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            MidnightFieldJournalPrefabBuilder.Build();
            MidnightFieldJournalAssetSetup.EnsureFolder(MidnightFieldJournalAssetSetup.Root + "Scenes/");
            var sourceController = source.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
            var ui = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MidnightFieldJournalAssetSetup.Root + "Prefabs/MidnightFieldJournalCanvas.prefab"), target);
            GameObject owner = new GameObject("RunController"); SceneManager.MoveGameObjectToScene(owner, target);
            var controller = owner.AddComponent<FishingRunController>(); EditorUtility.CopySerialized(sourceController, controller);
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("beginWithBoatCard").boolValue = true;
            serialized.FindProperty("currentEncounterView").objectReferenceValue = null; serialized.FindProperty("techniqueHandViews").arraySize = 0;
            serialized.FindProperty("fishingRunView").objectReferenceValue = ui.GetComponent<FishingRunView>();
            serialized.FindProperty("catchChainView").objectReferenceValue = ui.GetComponentInChildren<CatchChainView>(true);
            serialized.FindProperty("techniqueHandView").objectReferenceValue = ui.GetComponentInChildren<TechniqueHandView>(true);
            serialized.FindProperty("runResultView").objectReferenceValue = ui.GetComponentInChildren<RunResultView>(true);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var events = source.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<EventSystem>(true)).Single();
            Clone(events.gameObject, target);
            var camera = source.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true)).First(c => c.CompareTag("MainCamera"));
            Clone(camera.gameObject, target);
            foreach (var light in source.GetRootGameObjects().Where(g => g.GetComponents<Component>().Any(c => c != null && c.GetType().Name == "Light2D"))) Clone(light, target);
            if (!EditorSceneManager.SaveScene(target, ScenePath, false)) throw new InvalidOperationException("Could not save comparison scene.");
        }
        finally { EditorSceneManager.CloseScene(target, true); EditorSceneManager.ClosePreviewScene(source); SceneManager.SetActiveScene(active); }
        if (!sourceBytes.SequenceEqual(File.ReadAllBytes(SourcePath)) || !buildBytes.SequenceEqual(File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"))) throw new InvalidOperationException("Protected scene/build bytes changed.");
        AssetDatabase.ImportAsset(ScenePath);
        Debug.Log("Playable journal scene created at " + ScenePath + "; original scene and build selection unchanged.");
    }
    /// <summary>Copies only the named scene service, preserving its existing Unity/package configuration.</summary>
    private static void Clone(GameObject source, Scene target)
    { GameObject clone = UnityEngine.Object.Instantiate(source); clone.name = source.name; SceneManager.MoveGameObjectToScene(clone, target); }
}
