using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>Editor-only save reset workspace; never creates gameplay UI or ships in a player.</summary>
public sealed class GameSavesDebugWindow : EditorWindow
{
    private Label status;

    /// <summary>Opens the save tool alongside the existing Fishing Cards editor workspaces.</summary>
    [MenuItem("Fishing Cards/Debug/Game Saves")]
    [MenuItem("Window/Fishing Cards/Game Saves")]
    public static void Open()
    {
        var window = GetWindow<GameSavesDebugWindow>("Game Saves");
        window.minSize = new Vector2(400, 210); window.Show();
    }

    /// <summary>Builds a plain editor panel with a reset action and clear scope.</summary>
    public void CreateGUI()
    {
        var root = rootVisualElement; root.Clear();
        root.style.paddingLeft = root.style.paddingRight = 12;
        root.style.paddingTop = root.style.paddingBottom = 12;
        root.Add(new Label("Game Saves — Debug"));
        var description = new Label("Reset Gold, equipment, capacity upgrades and Journal progress. Settings are kept. In Play Mode, this abandons the expedition and reloads the Harbor.");
        description.style.whiteSpace = WhiteSpace.Normal; description.style.marginTop = 10; root.Add(description);
        var button = new Button(ConfirmReset) { text = "Reset Game Saves", name = "resetGameSaves" };
        button.style.marginTop = 14; button.style.height = 32; root.Add(button);
        status = new Label(); status.style.whiteSpace = WhiteSpace.Normal; status.style.marginTop = 10; root.Add(status);
    }

    /// <summary>Confirms the destructive editor command without touching saves when cancelled.</summary>
    private void ConfirmReset()
    {
        if (!EditorUtility.DisplayDialog("Reset Game Saves?", "Permanently clear this game's Gold, equipment, capacity upgrades and Journal progress? Any active expedition will be abandoned.", "Reset Saves", "Cancel")) return;
        try
        {
            ResetSaves();
            status.text = EditorApplication.isPlaying ? "Progress reset. Harbor reloaded." : "Saves reset. Press Play to start fresh.";
        }
        catch (Exception error)
        {
            status.text = "Reset failed: " + error.Message;
            Debug.LogError(status.text);
        }
    }

    /// <summary>Deletes the active scene's configured profile, suppressing stale teardown writes before a Play Mode reload.</summary>
    internal static void ResetSaves()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode != EditorApplication.isPlaying)
            throw new InvalidOperationException("Wait until compilation or the Play Mode transition finishes.");
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying && string.IsNullOrEmpty(scene.path))
            throw new InvalidOperationException("Save the game scene before resetting during Play Mode.");
        var controller = UnityEngine.Object.FindObjectsByType<FishingRunController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.gameObject.scene == scene);
        var journal = UnityEngine.Object.FindObjectsByType<DiscoveryJournalTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c.gameObject.scene == scene);
        string progressionKey = controller != null ? controller.EditorProgressionSaveKey : FishingRunController.DefaultProgressionSaveKey;
        string journalKey = journal != null ? journal.EditorJournalSaveKey : DiscoveryJournalTracker.DefaultSaveKey;
        bool hadProgression = PlayerPrefs.HasKey(progressionKey), hadJournal = PlayerPrefs.HasKey(journalKey);
        string previousProgression = hadProgression ? PlayerPrefs.GetString(progressionKey) : null;
        string previousJournal = hadJournal ? PlayerPrefs.GetString(journalKey) : null;
        try
        {
            if (controller != null) controller.SetDebugSaveResetPending(true);
            if (journal != null) journal.SetDebugSaveResetPending(true);
            PlayerPrefs.DeleteKey(progressionKey); PlayerPrefs.DeleteKey(journalKey); PlayerPrefs.Save();
            if (EditorApplication.isPlaying)
                EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
            else
            {
                if (controller != null) controller.SetDebugSaveResetPending(false);
                if (journal != null) journal.SetDebugSaveResetPending(false);
            }
        }
        catch
        {
            if (hadProgression) PlayerPrefs.SetString(progressionKey, previousProgression);
            if (hadJournal) PlayerPrefs.SetString(journalKey, previousJournal);
            if (controller != null) controller.SetDebugSaveResetPending(false);
            if (journal != null) journal.SetDebugSaveResetPending(false);
            try { PlayerPrefs.Save(); } catch (Exception) { /* Retain restored in-memory entries for retry. */ }
            throw;
        }
    }
}
