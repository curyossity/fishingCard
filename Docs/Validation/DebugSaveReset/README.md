# Editor Game Saves window

Open **Fishing Cards → Debug → Game Saves**, or **Window → Fishing Cards → Game Saves**. Click **Reset Game Saves**, then confirm. The tool is entirely editor-only. There is no in-game reset button or development-player overlay.

The reset clears Gold, equipment ownership/equipped item, capacity purchases and Journal progress. Other PlayerPrefs, including settings, are kept. It uses the active scene's configured keys, or the standard game keys if gameplay owners are absent. With Play Mode stopped, the next Play starts fresh. During Play Mode it abandons the expedition and reloads the Harbor. Editor-only controller/Journal hooks suppress stale teardown saves; failed saves/reloads restore previous entries and re-enable persistence.

No prefab, scene, artwork or Penpot resource changed. Remove `Assets/Editor/GameSavesDebugWindow.cs` and its meta to remove the window; its editor-only controller/Journal hooks can also be removed.

Validation in Unity 6000.5.9f1, 31 October 2026: compilation passed, window opened with four native UI elements, and Edit Mode reset removed isolated progression/Journal entries while retaining unrelated settings. Play Mode reset reloaded the Harbor with no active expedition; test keys stayed deleted despite controller teardown and pending Journal evidence. The runtime overlay type was absent and zero reset buttons existed in the game. No console errors. All fixtures used disposable keys, were cleaned up, and real player progress was not erased. Play Mode was stopped. This supersedes the earlier overlay and its connection-limited validation.

This is a Unity-only debug addition; no pending Penpot import is created. Prior design integration statuses remain unchanged.
