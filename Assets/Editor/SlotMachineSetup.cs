using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Authors plain reusable controls for Discover and Harbor preparation without importing or changing artwork.</summary>
public static class SlotMachineSetup
{
    public const string Root = "Assets/SlotMachine/";
    public const string ProfilePath = Root + "SlotMachine.asset";
    private const string DiscoverPath = Root + "DiscoverControls.prefab";
    private const string PreparationPath = Root + "TagPreparation.prefab";

    /// <summary>Installs the logical Discover increment into the existing comparison scene.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Plain Controls")]
    public static void InstallCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != JournalComparisonSceneBuilder.ScenePath || scene.isDirty)
            throw new InvalidOperationException("Open the saved comparison scene outside Play Mode before installation.");
        Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save slot integration.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Wires the same reusable controls during normal comparison-scene regeneration.</summary>
    public static void Install(Scene scene)
    {
        BuildAssets();
        var controller = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
        var view = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var harbor = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<JournalHarborFlow>(true)).Single();
        var data = new SerializedObject(controller);
        data.FindProperty("slotMachineProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SlotMachineProfile>(ProfilePath);
        data.FindProperty("tagTimingProfile").objectReferenceValue = null;
        data.FindProperty("rarityTimingProfile").objectReferenceValue = null;
        data.FindProperty("beginWithBoatCard").boolValue = false;
        data.FindProperty("startRunOnAwake").boolValue = false;
        data.ApplyModifiedPropertiesWithoutUndo();
        var viewData = new SerializedObject(view);
        var oldTiming = viewData.FindProperty("encounterTimingView").objectReferenceValue as EncounterTimingView;
        if (oldTiming != null) oldTiming.gameObject.SetActive(false);
        viewData.FindProperty("encounterTimingView").objectReferenceValue = null;
        var slot = view.GetComponentInChildren<SlotMachineView>(true);
        if (slot == null)
        {
            var panel = viewData.FindProperty("encounterPanel").objectReferenceValue as RectTransform;
            slot = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(DiscoverPath), panel.parent))
                .GetComponent<SlotMachineView>();
        }
        viewData.FindProperty("slotMachineView").objectReferenceValue = slot;
        viewData.ApplyModifiedPropertiesWithoutUndo();
        var hud = view.GetComponentInChildren<FishingRunHudPresentation>(true);
        if (hud != null)
        {
            hud.DescendButton.gameObject.SetActive(false);
            hud.SurfaceButton.gameObject.SetActive(false);
            foreach (string name in new[] { "Actions Heading", "Descend Hint", "Surface Hint" })
            {
                Transform legacyLabel = hud.DescendButton.transform.parent.Find(name);
                if (legacyLabel != null) legacyLabel.gameObject.SetActive(false);
            }
        }
        foreach (LineLoadMeterView meter in view.GetComponentsInChildren<LineLoadMeterView>(true)) meter.gameObject.SetActive(false);
        slot.gameObject.SetActive(false);
        var preparation = harbor.GetComponentInChildren<SlotTagPreparationView>(true);
        if (preparation == null)
            preparation = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PreparationPath), harbor.transform))
                .GetComponent<SlotTagPreparationView>();
        var harborData = new SerializedObject(harbor);
        harborData.FindProperty("slotPreparation").objectReferenceValue = preparation;
        var hubContent = harborData.FindProperty("hubContent").objectReferenceValue as GameObject;
        var blocker = hubContent.GetComponent<CanvasGroup>();
        if (blocker == null) blocker = hubContent.AddComponent<CanvasGroup>();
        var preparationData = new SerializedObject(preparation);
        Bind(preparationData, "blockedHarborContent", blocker);
        preparationData.ApplyModifiedPropertiesWithoutUndo();
        harborData.ApplyModifiedPropertiesWithoutUndo();
        preparation.gameObject.SetActive(false);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath) != null) TackleDiscoverArtSetup.Install(scene);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath) != null) LandingChallengeSetup.Install(scene);
    }

    /// <summary>Creates GUID-stable configuration and serialized controls; leaves existing tuning unchanged.</summary>
    private static void BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(Root.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets", "SlotMachine");
        if (AssetDatabase.LoadAssetAtPath<SlotMachineProfile>(ProfilePath) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SlotMachineProfile>(), ProfilePath);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(DiscoverPath) == null) BuildDiscover();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PreparationPath) == null) BuildPreparation();
        var existing = PrefabUtility.LoadPrefabContents(DiscoverPath);
        try
        {
            var data = new SerializedObject(existing.GetComponent<SlotMachineView>());
            if (data.FindProperty("resultDescription").objectReferenceValue == null)
            {
                Bind(data, "resultDescription", existing.transform.Find("Discover Content/Instruction").GetComponent<TMP_Text>());
                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(existing, DiscoverPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(existing); }
    }

    /// <summary>Authors three faces, individual lock commands, accept and the one optional respin.</summary>
    private static void BuildDiscover()
    {
        var root = Panel("Discover Controls", 1100, 580);
        try
        {
            var view = root.gameObject.AddComponent<SlotMachineView>();
            var data = new SerializedObject(view);
            Bind(data, "heading", Label(root, "Heading", "DISCOVER", 0, 220, 1000, 70, 34));
            var content = Rect("Discover Content", root, 0, -20, 1100, 420);
            Bind(data, "discoverContent", content.gameObject);
            var faces = data.FindProperty("reelLabels"); faces.arraySize = 3;
            var locks = data.FindProperty("lockButtons"); locks.arraySize = 3;
            var lockText = data.FindProperty("lockLabels"); lockText.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 335;
                faces.GetArrayElementAtIndex(i).objectReferenceValue = Label(content, "Reel " + (i + 1), "—", x, 100, 310, 100, 42);
                var button = Button(content, "Lock " + (i + 1), "LOCK", x, -10, 250, 75);
                locks.GetArrayElementAtIndex(i).objectReferenceValue = button;
                lockText.GetArrayElementAtIndex(i).objectReferenceValue = button.GetComponentInChildren<TMP_Text>();
            }
            Bind(data, "resultDescription", Label(content, "Instruction", "Match result", 0, -105, 1020, 65, 27));
            Bind(data, "acceptButton", Button(content, "Accept", "ACCEPT", -260, -190, 420, 75));
            Bind(data, "respinButton", Button(content, "Respin", "RESPIN", 260, -190, 420, 75));
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, DiscoverPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    /// <summary>Authors a Harbor modal with the eight explicit tags and a four-tag confirmation gate.</summary>
    private static void BuildPreparation()
    {
        var root = Panel("Active Tag Preparation", 1500, 1100);
        try
        {
            var view = root.gameObject.AddComponent<SlotTagPreparationView>();
            var data = new SerializedObject(view);
            Bind(data, "status", Label(root, "Status", "Choose 4 active tags", 0, 440, 1400, 100, 48));
            Label(root, "Instruction", "These four symbols remain active throughout the expedition.", 0, 330, 1400, 90, 30);
            var buttons = data.FindProperty("tagButtons"); buttons.arraySize = 8;
            var labels = data.FindProperty("tagLabels"); labels.arraySize = 8;
            for (int i = 0; i < 8; i++)
            {
                var button = Button(root, CreatureTagVocabulary.All[i], CreatureTagVocabulary.All[i],
                    i % 2 == 0 ? -350 : 350, 180 - i / 2 * 145, 620, 110);
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = button;
                labels.GetArrayElementAtIndex(i).objectReferenceValue = button.GetComponentInChildren<TMP_Text>();
            }
            Bind(data, "confirm", Button(root, "Confirm", "SET SAIL", 350, -420, 620, 100));
            Bind(data, "cancel", Button(root, "Cancel", "BACK", -350, -420, 620, 100));
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PreparationPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    /// <summary>Creates an opaque plain panel using native UI only; no artwork or new textures.</summary>
    private static RectTransform Panel(string name, float width, float height)
    {
        var root = Rect(name, null, 0, 0, width, height);
        var image = root.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color32(18, 34, 42, 255);
        return root;
    }

    /// <summary>Creates centered native geometry in the existing reference-resolution canvas.</summary>
    private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var root = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(parent, false); root.anchorMin = root.anchorMax = root.pivot = new Vector2(.5f, .5f);
        root.anchoredPosition = new Vector2(x, y); root.sizeDelta = new Vector2(width, height);
        return root;
    }

    /// <summary>Creates a readable runtime-owned text field using the project's existing TMP font.</summary>
    private static TMP_Text Label(Transform parent, string name, string value, float x, float y, float width, float height, float size)
    {
        var label = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value; label.fontSize = size; label.alignment = TextAlignmentOptions.Center;
        label.color = new Color32(244, 233, 211, 255); label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    /// <summary>Creates a standard keyboard/gamepad-navigable Button with independent text.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, string value, float x, float y, float width, float height)
    {
        var root = Rect(name, parent, x, y, width, height);
        var image = root.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color32(57, 76, 82, 255);
        var button = root.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
        Label(root, "Label", value, 0, 0, width - 20, height - 12, 34);
        return button;
    }

    /// <summary>Assigns a serialized reference without reflection or runtime scene searches.</summary>
    private static void Bind(SerializedObject data, string property, UnityEngine.Object value)
    { data.FindProperty(property).objectReferenceValue = value; }
}
