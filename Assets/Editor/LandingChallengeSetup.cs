using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Installs the approved Landing artwork and preserves the configured gameplay profile and Harbor route.</summary>
public static class LandingChallengeSetup
{
    public const string Root = "Assets/LandingChallenge/";
    public const string ProfilePath = Root + "LandingChallenge.asset";
    public const string PrefabPath = Root + "LandingControls.prefab";

    /// <summary>Installs the authored Landing presentation in the comparison scene outside Play Mode.</summary>
    [MenuItem("Fishing Cards/Landing Challenge/Integrate Approved Landing Art")]
    public static void InstallCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != JournalComparisonSceneBuilder.ScenePath)
            throw new InvalidOperationException("Open the comparison scene outside Play Mode.");
        TackleLandingArtSetup.BuildAssets();
        Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Landing artwork integration.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the approved prefab and existing tuning when scene/Harbor authoring is repeated.</summary>
    public static void Install(Scene scene)
    {
        if (!AssetDatabase.IsValidFolder(Root.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets", "LandingChallenge");
        if (AssetDatabase.LoadAssetAtPath<LandingChallengeProfile>(ProfilePath) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LandingChallengeProfile>(), ProfilePath);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null || prefab.GetComponent<TackleLandingPresentation>() == null) TackleLandingArtSetup.BuildAssets();
        var controller = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
        var view = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var harbor = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<JournalHarborFlow>(true)).Single();
        var controls = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LandingChallengeView>(true)).SingleOrDefault();
        if (controls == null) controls = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<LandingChallengeView>();
        var data = new SerializedObject(controller);
        if (data.FindProperty("landingChallengeProfile").objectReferenceValue == null)
            data.FindProperty("landingChallengeProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LandingChallengeProfile>(ProfilePath);
        data.ApplyModifiedPropertiesWithoutUndo();
        var viewData = new SerializedObject(view); viewData.FindProperty("landingChallengeView").objectReferenceValue = controls; viewData.ApplyModifiedPropertiesWithoutUndo();
        for (int i = controls.ReturnedToHarbor.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(controls.ReturnedToHarbor, i);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(controls.ReturnedToHarbor, harbor.ReturnToHarbor);
        EditorUtility.SetDirty(controls); PrefabUtility.RecordPrefabInstancePropertyModifications(controls);
        controls.gameObject.SetActive(false);
    }
}
