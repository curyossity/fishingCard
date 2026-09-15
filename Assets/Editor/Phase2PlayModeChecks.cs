using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Phase2PlayModeChecks
{
    private const string PendingKey = "FishingCards.Phase2.PlayModeCheck.Pending";
    private const string FailedKey = "FishingCards.Phase2.PlayModeCheck.Failed";
    private const string ValidationDirectory = "Docs/Validation/Phase2";
    private static int updateCount;

    static Phase2PlayModeChecks()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    /// <summary>
    /// Opens the gameplay scene and validates the migrated runtime UI after its Awake cycle.
    /// </summary>
    public static void Run()
    {
        Directory.CreateDirectory(ValidationDirectory);
        SessionState.SetBool(PendingKey, true);
        SessionState.SetBool(FailedKey, false);
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }

    /// <summary>
    /// Coordinates validation across the editor domain reloads surrounding Play Mode.
    /// </summary>
    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(PendingKey, false))
        {
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            updateCount = 0;
            EditorApplication.update -= ValidateAfterStartup;
            EditorApplication.update += ValidateAfterStartup;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            bool failed = SessionState.GetBool(FailedKey, false);
            SessionState.EraseBool(PendingKey);
            SessionState.EraseBool(FailedKey);

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(failed ? 1 : 0);
            }
        }
    }

    /// <summary>
    /// Waits for startup refreshes, then checks scene bindings and generated child views.
    /// </summary>
    private static void ValidateAfterStartup()
    {
        updateCount++;
        if (updateCount < 8)
        {
            return;
        }

        EditorApplication.update -= ValidateAfterStartup;

        try
        {
            FishingRunController controller = UnityEngine.Object.FindAnyObjectByType<FishingRunController>();
            FishingRunView runView = UnityEngine.Object.FindAnyObjectByType<FishingRunView>();
            Require(controller != null, "FishingRunController is missing in Play Mode.");
            Require(runView != null, "FishingRunView is missing in Play Mode.");
            Require(controller.RunActive, "The configured run did not start.");

            Transform root = runView.transform;
            Require(root.Find("TopNavigationBar/Location Header") != null, "Top navigation runtime content was not created.");
            Require(root.Find("MainContent/CatchRigPanel/Catch Chain Panel") != null, "Catch Rig runtime content was not created.");
            Require(root.Find("MainContent/EncounterPanel/Current Encounter Region") != null, "Encounter runtime content was not created.");
            Require(root.Find("MainContent/RunControlsPanel/Core Actions") != null, "Run controls runtime content was not created.");
            Require(root.Find("TechniqueHand/Technique Hand Panel") != null, "Technique hand runtime content was not created.");
            Require(runView.TooltipLayer.sortingOrder < runView.TransitionLayer.sortingOrder, "Tooltip and transition order is invalid.");
            Require(runView.TransitionLayer.sortingOrder < runView.ModalLayer.sortingOrder, "Transition and modal order is invalid.");

            string result = "{\"unity\":\"" + Application.unityVersion
                + "\",\"scene\":\"" + SceneManager.GetActiveScene().name
                + "\",\"runActive\":true,\"runtimeRegions\":5,\"status\":\"passed\"}";
            File.WriteAllText(Path.Combine(ValidationDirectory, "playmode-result.json"), result);
            Debug.Log("Phase 2 Play Mode validation passed.");
        }
        catch (Exception exception)
        {
            SessionState.SetBool(FailedKey, true);
            File.WriteAllText(Path.Combine(ValidationDirectory, "playmode-failure.txt"), exception.ToString());
            Debug.LogException(exception);
        }
        finally
        {
            EditorApplication.ExitPlaymode();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
