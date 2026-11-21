using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Imports the reproducible selected sound family and retains bindings through native prefab authoring.</summary>
public static class DeepwaterAudioSetup
{
    public const string Root = "Assets/Audio/Deepwater/";
    public const string LibraryPath = Root + "DeepwaterSoundLibrary.asset";
    [Serializable] private sealed class Manifest { public int version; public Cue[] cues; }
    [Serializable] private sealed class Cue { public string cue, file, category; public float gainDb, cooldown, pitchGrowth, intensityGainDb; public int priority; }

    /// <summary>Updates only audio assets/components in the four existing phase prefabs; source artwork and scene routes stay intact.</summary>
    [MenuItem("Fishing Cards/Audio/Install Deepwater Sound Direction")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install audio outside Play mode.");
        ImportLibrary(); EnsureBuses();
        Edit(TackleDiscoverArtSetup.PrefabPath, ConfigureDiscover);
        Edit(LandingChallengeSetup.PrefabPath, ConfigureLanding);
        Edit(RigBuildingSetup.PrefabPath, ConfigureBuild);
        Edit(SlotMachineSetup.PreparationPath, ConfigurePreparation);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Builds editable per-cue gain, bus, variation and priority definitions from the offline render manifest.</summary>
    public static void ImportLibrary()
    {
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Root + "manifest.json"));
        foreach (var cue in manifest.cues)
        {
            AssetDatabase.ImportAsset(cue.file, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(cue.file);
            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.PCM; settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = true; settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings; importer.forceToMono = true; importer.loadInBackground = false;
            importer.SaveAndReimport();
        }
        var library = AssetDatabase.LoadAssetAtPath<DeepwaterSoundLibrary>(LibraryPath);
        if (library == null) { library = ScriptableObject.CreateInstance<DeepwaterSoundLibrary>(); AssetDatabase.CreateAsset(library, LibraryPath); }
        var data = new SerializedObject(library); var entries = data.FindProperty("cues");
        var groups = manifest.cues.GroupBy(c => c.cue).ToArray(); entries.arraySize = groups.Length;
        for (int i = 0; i < groups.Length; i++)
        {
            var row = entries.GetArrayElementAtIndex(i); var first = groups[i].First();
            row.FindPropertyRelative("Cue").enumValueIndex = (int)Enum.Parse(typeof(DeepwaterSoundCue), first.cue);
            row.FindPropertyRelative("Bus").enumValueIndex = (int)Enum.Parse(typeof(DeepwaterSoundBus), first.category);
            row.FindPropertyRelative("GainDb").floatValue = first.gainDb;
            row.FindPropertyRelative("Priority").intValue = first.priority;
            row.FindPropertyRelative("Cooldown").floatValue = first.cooldown;
            row.FindPropertyRelative("PitchGrowth").floatValue = manifest.version >= 3 ? first.pitchGrowth : 1;
            row.FindPropertyRelative("IntensityGainDb").floatValue = manifest.version >= 3 ? first.intensityGainDb : 1.5f;
            var takes = row.FindPropertyRelative("Takes"); takes.arraySize = groups[i].Count(); int index = 0;
            foreach (var take in groups[i]) takes.GetArrayElementAtIndex(index++).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(take.file);
        }
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(library);
    }

    /// <summary>Keeps the existing reel loop and revision owner, with all one-shots routed through the shared cue player.</summary>
    public static void ConfigureDiscover(GameObject root)
    {
        var host = root.GetComponent<TackleSlotAudio>(); if (host == null) return;
        var player = ConfigurePlayer(root, 8); var data = new SerializedObject(host); Bind(data, "sound", player);
        string[] fields = { "start", "roll", "tick", "stop", "latch", "unlatch", "pair", "triple" };
        var cues = new[] { DeepwaterSoundCue.SlotStart, DeepwaterSoundCue.SlotRoll, DeepwaterSoundCue.SlotTick, DeepwaterSoundCue.SlotStop1,
            DeepwaterSoundCue.SlotLock, DeepwaterSoundCue.SlotUnlock, DeepwaterSoundCue.Pair, DeepwaterSoundCue.Triple };
        for (int i = 0; i < fields.Length; i++) Bind(data, fields[i], player.Library.Find(cues[i]).Takes[0]);
        var loop = (AudioSource)data.FindProperty("rolling").objectReferenceValue;
        loop.outputAudioMixerGroup = Buses()[(int)DeepwaterSoundBus.Motion]; loop.priority = 240;
        loop.clip = player.Library.Find(DeepwaterSoundCue.SlotRoll).Takes[0]; loop.playOnAwake = false; loop.spatialBlend = 0; loop.loop = true;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Binds score scheduling and encounter/failure acknowledgments to the same authored acoustic palette.</summary>
    public static void ConfigureLanding(GameObject root)
    {
        var host = root.GetComponent<LandingScoreRush>(); if (host == null) return;
        var player = ConfigurePlayer(root, 8); var data = new SerializedObject(host); Bind(data, "sound", player);
        string[] fields = { "hit", "addition", "multiplier", "windup", "payoutSmall", "payoutBig", "miss" };
        var cues = new[] { DeepwaterSoundCue.Good, DeepwaterSoundCue.Addition, DeepwaterSoundCue.Multiplier, DeepwaterSoundCue.Travel,
            DeepwaterSoundCue.PayoutSmall, DeepwaterSoundCue.PayoutBig, DeepwaterSoundCue.Miss };
        for (int i = 0; i < fields.Length; i++) Bind(data, fields[i], player.Library.Find(cues[i]).Takes[0]);
        data.ApplyModifiedPropertiesWithoutUndo();
        var presentation = new SerializedObject(root.GetComponent<TackleLandingPresentation>()); Bind(presentation, "sound", player); presentation.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Uses distinct insertion and upgrade phrases, retaining accepted receipt impact timing.</summary>
    public static void ConfigureBuild(GameObject root)
    {
        var host = root.GetComponent<RigBuildPresentation>(); if (host == null) return;
        var player = ConfigurePlayer(root, 6); var data = new SerializedObject(host); Bind(data, "sound", player);
        Bind(data, "voice", root.GetComponentsInChildren<AudioSource>(true).First(s => !s.loop));
        Bind(data, "clipSnap", player.Library.Find(DeepwaterSoundCue.Attach).Takes[0]);
        Bind(data, "upgradeSound", player.Library.Find(DeepwaterSoundCue.Upgrade).Takes[0]); data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Gives deliberate tag changes a quiet paper acknowledgment; focus and hover stay silent.</summary>
    public static void ConfigurePreparation(GameObject root)
    {
        var host = root.GetComponent<SlotTagPreparationView>(); if (host == null) return;
        var player = ConfigurePlayer(root, 4); var data = new SerializedObject(host); Bind(data, "sound", player); data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Reuses existing 2D voices before adding the few required for bounded look-ahead overlap.</summary>
    private static DeepwaterSoundPlayer ConfigurePlayer(GameObject root, int count)
    {
        var library = AssetDatabase.LoadAssetAtPath<DeepwaterSoundLibrary>(LibraryPath);
        if (library == null) { ImportLibrary(); library = AssetDatabase.LoadAssetAtPath<DeepwaterSoundLibrary>(LibraryPath); }
        var player = root.GetComponent<DeepwaterSoundPlayer>(); if (player == null) player = root.AddComponent<DeepwaterSoundPlayer>();
        var sources = root.GetComponentsInChildren<AudioSource>(true).Where(s => !s.loop).ToList();
        while (sources.Count < count)
        {
            var child = new GameObject("Acoustic feedback voice " + sources.Count); child.transform.SetParent(root.transform, false);
            sources.Add(child.AddComponent<AudioSource>());
        }
        var buses = Buses(); var data = new SerializedObject(player); Bind(data, "library", library);
        var voices = data.FindProperty("voices"); voices.arraySize = count;
        for (int i = 0; i < count; i++)
        {
            sources[i].playOnAwake = false; sources[i].spatialBlend = 0; sources[i].outputAudioMixerGroup = buses[0]; sources[i].loop = false;
            voices.GetArrayElementAtIndex(i).objectReferenceValue = sources[i];
        }
        var routes = data.FindProperty("buses"); routes.arraySize = buses.Length;
        for (int i = 0; i < buses.Length; i++) routes.GetArrayElementAtIndex(i).objectReferenceValue = buses[i];
        data.ApplyModifiedPropertiesWithoutUndo(); return player;
    }

    /// <summary>Adds category buses to the GUID-stable existing mixer, preserving all earlier source references.</summary>
    private static void EnsureBuses()
    {
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(LandingScoreRushSetup.MixerPath);
        if (mixer == null) throw new InvalidOperationException("Install the game's feedback mixer before the sound family.");
        var master = mixer.FindMatchingGroups("Master").First(g => g.name == "Master");
        var masterData = new SerializedObject(master); var children = masterData.FindProperty("m_Children");
        foreach (string name in Enum.GetNames(typeof(DeepwaterSoundBus)))
        {
            if (mixer.FindMatchingGroups(name).Any(g => g.name == name)) continue;
            // Group creation is Editor-only; player code uses public AudioMixer/AudioSource APIs exclusively.
            var factory = mixer.GetType().GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (factory == null) throw new InvalidOperationException("Unity mixer group authoring is unavailable.");
            var group = (AudioMixerGroup)factory.Invoke(mixer, new object[] { name, true });
            children.InsertArrayElementAtIndex(children.arraySize); children.GetArrayElementAtIndex(children.arraySize - 1).objectReferenceValue = group;
        }
        masterData.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(mixer); AssetDatabase.SaveAssets();
    }

    /// <summary>Resolves exactly one category group in stable enum order, creating missing groups during authoring only.</summary>
    private static AudioMixerGroup[] Buses()
    {
        EnsureBuses(); var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(LandingScoreRushSetup.MixerPath);
        return Enum.GetNames(typeof(DeepwaterSoundBus)).Select(name => mixer.FindMatchingGroups(name).Single(g => g.name == name)).ToArray();
    }
    /// <summary>Edits a reusable prefab in isolation and always unloads its temporary authoring scene.</summary>
    private static void Edit(string path, Action<GameObject> configure)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try { configure(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    /// <summary>Changes only an existing serialized reference so missing authoring contracts fail visibly.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) { data.FindProperty(field).objectReferenceValue = value; }
}
