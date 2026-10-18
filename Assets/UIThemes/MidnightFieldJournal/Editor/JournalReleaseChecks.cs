using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Builds the isolated comparison and records explicit asset dependencies without changing default build scenes.</summary>
public static class JournalReleaseChecks
{
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/";
    public const string PlayerPath = "Builds/MidnightFieldJournal/FishingCardsJournal.exe";
    public static string BuildStatus { get; private set; } = "Not started";
    [Serializable] private sealed class Dependencies
    {
        public string scope = "Explicit AssetDatabase dependencies only; Resources, preload and string references require separate review. Nothing is authorized for deletion.";
        public string[] candidateScene, candidatePrefab, originalScene, sharedBetweenScenes, legacyVisualsStillReferenced, originalOnly, retainedBackups, implicitResources;
    }
    [Serializable] private sealed class BuildResult
    {
        public bool passed, buildSettingsUnchanged;
        public string result, player, failure;
        public int errors, warnings;
        public ulong bytes;
        public double seconds;
        public string[] scenes, packedAssets;
    }
    /// <summary>Classifies old-only dependencies separately from shared gameplay references and backup content.</summary>
    public static void WriteDependencies()
    {
        string[] candidate = AssetDatabase.GetDependencies(JournalComparisonSceneBuilder.ScenePath, true);
        string[] original = AssetDatabase.GetDependencies("Assets/Scenes/SampleScene.unity", true);
        string[] prefab = AssetDatabase.GetDependencies(MidnightFieldJournalAssetSetup.Root + "Prefabs/MidnightFieldJournalCanvas.prefab", true);
        var report = new Dependencies
        {
            candidateScene = candidate.OrderBy(p => p).ToArray(), candidatePrefab = prefab.OrderBy(p => p).ToArray(), originalScene = original.OrderBy(p => p).ToArray(),
            sharedBetweenScenes = candidate.Intersect(original).OrderBy(p => p).ToArray(),
            legacyVisualsStillReferenced = candidate.Where(p => p.StartsWith("Assets/FishingUIAssets/") || p.StartsWith("Assets/Prefabs/UI/")).OrderBy(p => p).ToArray(),
            originalOnly = original.Except(candidate).OrderBy(p => p).ToArray(),
            retainedBackups = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/Cards/Backup/") && !AssetDatabase.IsValidFolder(p)).OrderBy(p => p).ToArray(),
            implicitResources = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && (p.Contains("/Resources/") || p.StartsWith("Assets/StreamingAssets/")) && !AssetDatabase.IsValidFolder(p)).OrderBy(p => p).ToArray()
        };
        File.WriteAllText(Output + "UI08RetirementDependencies.json", JsonUtility.ToJson(report, true));
    }
    /// <summary>Uses an explicit per-build scene list, retaining the original as a smoke-test alternate.</summary>
    public static void BuildPlayer()
    {
        if (Application.isPlaying || BuildStatus == "Building") throw new InvalidOperationException("Build outside Play Mode and only once.");
        byte[] settings = File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset");
        var result = new BuildResult { player = PlayerPath, scenes = new[] { JournalComparisonSceneBuilder.ScenePath, "Assets/Scenes/SampleScene.unity" } };
        try
        {
            BuildStatus = "Building"; Directory.CreateDirectory(Path.GetDirectoryName(PlayerPath));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = result.scenes, target = BuildTarget.StandaloneWindows64, locationPathName = PlayerPath,
                options = BuildOptions.Development | BuildOptions.DetailedBuildReport
            });
            result.result = report.summary.result.ToString(); result.errors = report.summary.totalErrors; result.warnings = report.summary.totalWarnings;
            result.seconds = report.summary.totalTime.TotalSeconds; result.bytes = report.summary.totalSize;
            result.packedAssets = report.packedAssets.SelectMany(p => p.contents).Select(p => p.sourceAssetPath).Where(p => !string.IsNullOrEmpty(p)).Distinct().OrderBy(p => p).ToArray();
            result.passed = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        }
        catch (Exception failure) { result.failure = failure.ToString(); }
        result.buildSettingsUnchanged = settings.SequenceEqual(File.ReadAllBytes("ProjectSettings/EditorBuildSettings.asset"));
        result.passed &= result.buildSettingsUnchanged;
        File.WriteAllText(Output + "UI08BuildValidation.json", JsonUtility.ToJson(result, true));
        BuildStatus = result.passed ? "Passed" : "Failed";
    }
}
