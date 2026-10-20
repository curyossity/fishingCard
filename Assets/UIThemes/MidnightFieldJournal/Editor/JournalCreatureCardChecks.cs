using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Checks central-card content, identity boundaries and input without changing authored gameplay data.</summary>
public static class JournalCreatureCardChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/";
    private static int assertions;
    private static GameObject smokeRoot;
    private static CreatureCardView smokeView;
    private static bool oldBackground;
    private static int clicks;
    private static string smokeScenePath;
    private static byte[] smokeSceneBytes;
    [Serializable] private sealed class Report { public bool passed, sceneUnchanged; public int assertions, cards; }

    /// <summary>Renders real encounter definitions and checks transitions between catch, event, hidden and blank faces.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Central Cards")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run capture outside Play Mode.");
        assertions = 0; Scene active = SceneManager.GetActiveScene(); bool dirty = active.isDirty;
        Scene preview = EditorSceneManager.NewPreviewScene(); int count = 0;
        try
        {
            GameObject root = new GameObject("Central card checks", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(root, preview);
            root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            CreatureCardView view = Create(root.transform, -1280);
            var theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
            CardDefinition[] cards = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
                .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => Catchable(c) || c.CardType == CardType.Hazard || c.CardType == CardType.Environment || c.CardType == CardType.Opportunity || c.CardType == CardType.Encounter || c.CardType == CardType.Location).ToArray();
            foreach (var card in cards)
            {
                count++; view.SetCard(card, 0, 9999, false); Canvas.ForceUpdateCanvases();
                Require(Text(view, "cardNameText").text == card.DisplayName.ToUpperInvariant(), "Wrong title: " + card.DisplayName);
                Glyphs(Text(view, "cardNameText")); Glyphs(Text(view, "cardTypeText"));
                bool catchable = Catchable(card);
                Require(Text(view, "weightText").transform.parent.gameObject.activeSelf == catchable, "Weight visibility: " + card.DisplayName);
                Require(Text(view, "valueText").transform.parent.gameObject.activeSelf == catchable, "Value visibility: " + card.DisplayName);
                Require(Field<RectTransform>(view, "baitRulesRegion").gameObject.activeSelf == catchable, "Event has Bait panel.");
                Require(view.transform.Find("Rarity Footer").gameObject.activeSelf == catchable, "Rarity visibility incorrect.");
                theme.TryGetArtwork(card.UniqueId, out Sprite expected);
                var central = theme.GetCentralCard(card.UniqueId); if (central != null) expected = central.sprite;
                var centralEvent = theme.GetCentralEvent(card.UniqueId); if (centralEvent != null) expected = centralEvent.sprite;
                var art = Field<UnityEngine.UI.Image>(view, "creatureArtwork");
                Require(art.sprite == expected && art.enabled == (expected != null), "Artwork substituted: " + card.DisplayName);
                Require(!Field<UnityEngine.UI.Image>(view, "artworkOverflowLayer").enabled, "Old full-card overlay leaked.");
                if (catchable)
                {
                    Glyphs(Text(view, "weightText")); Glyphs(Text(view, "valueText"));
                    Require(Text(view, "weightText").text == "0", "Zero weight missing.");
                    Require(Text(view, "valueText").text == (card.HidesOwnValueDuringRunForRole(CatchAttachmentRole.Catch) ? "?" : "9999"), "Value masking mismatch.");
                    var rarityLabel = view.transform.Find("Rarity Footer/Rarity Name").GetComponent<TMP_Text>();
                    Require(rarityLabel.text == card.Rarity.ToString().ToUpperInvariant(), "Wrong rarity name.");
                    var tags = view.transform.Find("Creature Tags").GetComponent<TMP_Text>();
                    if (tags.text.Length > 0) Glyphs(tags);
                    RulesFit(view, "effectText"); RulesFit(view, "baitEffectText");
                    if (Text(view, "catchAbilityText").text.Length > 0) Glyphs(Text(view, "catchAbilityText"));
                    if (Text(view, "baitAbilityText").text.Length > 0) Glyphs(Text(view, "baitAbilityText"));
                    view.SetCard(card, 3, 25, true, true);
                    Require(Text(view, "cardNameText").text == "UNKNOWN CREATURE" && !art.enabled, "Identity leaked.");
                    Require(Text(view, "weightText").text == "?" && Text(view, "valueText").text == "?", "Hidden stats leaked.");
                    Require(Text(view, "effectText").text == "" && Text(view, "baitEffectText").text == "" && Text(view, "catchAbilityText").text == "" && Text(view, "baitAbilityText").text == "", "Hidden ability leaked.");
                    Require(view.transform.Find("Creature Tags").GetComponent<TMP_Text>().text == "" && !view.transform.Find("Rarity Footer").gameObject.activeSelf, "Hidden metadata leaked.");
                    view.SelectEffectChoice(EncounterEffectChoice.Bait, false);
                    Require(view.SelectedEffectChoice == EncounterEffectChoice.Bait, "Hidden role not selectable.");
                    view.SetCard(card, 0, 0, true, false);
                    Require(Text(view, "cardNameText").text == card.DisplayName.ToUpperInvariant() && art.enabled == (expected != null), "Information-only masking hid identity.");
                    Require(Text(view, "effectText").text == "" && Text(view, "weightText").text == "?", "Information masking failed.");
                }
                else
                {
                    Require(!Field<GameObject>(view, "catchRoleLabel").activeSelf && Text(view, "catchAbilityText").text == "", "Event shows Catch heading.");
                    Require(!Field<RectTransform>(view, "rulesRegion").GetComponent<UnityEngine.UI.Button>().enabled, "Event allows role input.");
                    Require(string.Join(" ",Text(view,"effectText").text.Split(new[]{' ','\n','\r','\t'},StringSplitOptions.RemoveEmptyEntries)) == string.Join(" ",card.RulesText.Split(new[]{' ','\n','\r','\t'},StringSplitOptions.RemoveEmptyEntries)), "Event rules changed."); RulesFit(view, "effectText");
                }
            }
            CardDefinition octopus = Card("Small Octopus"); view.SetCard(octopus, 1, 9, false);
            Require(Text(view, "catchAbilityText").text == "Clever Catch" && Text(view, "baitAbilityText").text == "Helping Arms", "Separate ability headings lost.");
            Require(!Text(view, "effectText").text.StartsWith("Clever Catch:"), "Ability heading duplicated in body.");
            view.SetCard(Card("Glass Minnow"), 1, 5, false);
            Require(Text(view, "catchAbilityText").text == "" && Text(view, "effectText").text == "", "Effectless Catch invented content.");
            var clone = UnityEngine.Object.Instantiate(octopus);
            try
            {
                var serialized = new SerializedObject(clone);
                serialized.FindProperty("encounterArtworkScale").floatValue = 1.2f;
                serialized.FindProperty("encounterArtworkRotation").floatValue = 12;
                serialized.FindProperty("encounterArtworkOffset").vector2Value = new Vector2(8, 4);
                var highlights = serialized.FindProperty("rulesTextHighlights"); highlights.arraySize = 2;
                highlights.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue = "Clever Catch";
                highlights.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue = "draw";
                foreach (int i in new[] { 0, 1 })
                { highlights.GetArrayElementAtIndex(i).FindPropertyRelative("color").colorValue = Color.cyan; highlights.GetArrayElementAtIndex(i).FindPropertyRelative("sizePercent").intValue = 110; }
                serialized.ApplyModifiedPropertiesWithoutUndo(); view.SetCard(clone, 1, 9, false);
                var rect = Field<UnityEngine.UI.Image>(view, "creatureArtwork").rectTransform;
                Require(rect.localScale == Vector3.one && Mathf.Abs(Mathf.DeltaAngle(rect.localEulerAngles.z, 0)) < .01f && rect.anchoredPosition == Vector2.zero, "Legacy framing leaked into central-only portrait.");
                Require(Text(view, "catchAbilityText").text.Contains("<color=") && Text(view, "effectText").text.Contains("<color="), "Authored highlighting lost in heading or description.");
                Require(Text(view, "catchAbilityText").text.Contains("110%") && Text(view, "effectText").text.Contains("110%"), "Authored highlight size lost.");
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
            view.SetBlank(); Require(Text(view, "cardNameText").text == "" && !view.transform.Find("Rarity Footer").gameObject.activeSelf, "Blank retains identity.");
            view.SetCard(octopus, 1, 9, false); view.SetPreview("PREVIEW", "CATCH CARD", null, 0, 0, "", CardRarity.Common);
            Require(Text(view, "catchAbilityText").text == "" && view.transform.Find("Creature Tags").GetComponent<TMP_Text>().text == "", "Preview retains prior metadata.");
            view.SetCard(null, 0, 0, false); Require(!view.gameObject.activeSelf, "Null card still visible.");
            string[] deps = AssetDatabase.GetDependencies(Root + "Prefabs/CreatureCard.prefab", true);
            Require(!deps.Any(p => p.StartsWith("Assets/FishingUIAssets/") || p.StartsWith("Assets/Prefabs/UI/")), "Candidate depends on old visual assets.");
            view.SetCard(octopus, 1, 9, false);
            CreatureCardView eventView = Create(root.transform, 0); eventView.SetCard(Card("Reef Snag"), 0, 0, false);
            CreatureCardView hidden = Create(root.transform, 1280); hidden.SetCard(octopus, 1, 9, true, true);
            Capture(root, preview, 3840, 2160, "UI05CentralCards4K.png");
            Capture(root, preview, 1920, 1080, "UI05CentralCards1080p.png");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
        bool unchanged = SceneManager.GetActiveScene() == active && active.isDirty == dirty;
        Require(unchanged, "Validation changed the open scene.");
        File.WriteAllText(Output + "UI05Validation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = unchanged, assertions = assertions, cards = count }, true));
        Debug.Log("Journal central card checks passed: " + assertions + " across " + count + " designs.");
    }

    /// <summary>Creates a transient overlay for rendered-frame role hit testing.</summary>
    public static void StartPlayModeSmoke()
    {
        if (!Application.isPlaying || smokeRoot != null) throw new InvalidOperationException("Start once in Play Mode.");
        assertions = clicks = 0; oldBackground = Application.runInBackground; Application.runInBackground = true;
        smokeScenePath = SceneManager.GetActiveScene().path; smokeSceneBytes = File.ReadAllBytes(smokeScenePath);
        smokeRoot = new GameObject("Isolated Journal Central Smoke", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(CanvasGroup));
        smokeRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; smokeRoot.GetComponent<Canvas>().sortingOrder = 30000;
        var scaler = smokeRoot.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(3840, 2160);
        smokeView = Create(smokeRoot.transform, 0); smokeView.SetCard(Card("Small Octopus"), 1, 9, false);
        smokeView.EffectChoiceChanged += (v, role) => clicks++;
    }

    /// <summary>Exercises real role raycasts, submission, repeated refreshes and parent input locks.</summary>
    public static void FinishPlayModeSmoke()
    {
        if (!Application.isPlaying || smokeRoot == null) throw new InvalidOperationException("Start smoke first.");
        try
        {
            EventSystem events = EventSystem.current; Require(events != null, "Missing EventSystem.");
            var bait = Field<RectTransform>(smokeView, "baitRulesRegion"); var catchRegion = Field<RectTransform>(smokeView, "rulesRegion");
            var pointer = new PointerEventData(events) { position = RectTransformUtility.WorldToScreenPoint(null, bait.TransformPoint(bait.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new System.Collections.Generic.List<RaycastResult>(); smokeRoot.GetComponent<UnityEngine.UI.GraphicRaycaster>().Raycast(pointer, hits);
            Require(hits.Count > 0 && hits[0].gameObject == bait.gameObject, "Bait hit target occluded.");
            ExecuteEvents.Execute(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Require(clicks == 1 && smokeView.SelectedEffectChoice == EncounterEffectChoice.Bait, "Bait click did not select exactly once.");
            for (int i = 0; i < 10; i++) smokeView.SetCard(Card("Small Octopus"), 1, 0, false);
            ExecuteEvents.Execute(catchRegion.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
            Require(clicks == 2 && smokeView.SelectedEffectChoice == EncounterEffectChoice.Catch, "Refresh duplicated role listeners.");
            smokeRoot.GetComponent<CanvasGroup>().interactable = false;
            Require(!bait.GetComponent<UnityEngine.UI.Button>().IsInteractable(), "Role bypasses parent input lock.");
            ExecuteEvents.Execute(bait.gameObject, pointer, ExecuteEvents.pointerClickHandler); Require(clicks == 2, "Locked role dispatched input.");
            smokeRoot.GetComponent<CanvasGroup>().interactable = true;
            smokeView.SetCard(Card("Small Octopus"), 1, 9, true, true);
            ExecuteEvents.Execute(bait.gameObject, pointer, ExecuteEvents.pointerClickHandler); Require(clicks == 3, "Hidden creature role input unavailable.");
            smokeView.SetCard(Card("Reef Snag"), 0, 0, false);
            ExecuteEvents.Execute(catchRegion.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(clicks == 3, "Event accepts role input.");
            smokeView.SetCard(Card("Glass Minnow"), 1, 5, false); Require(smokeView.SelectedEffectChoice == EncounterEffectChoice.None, "New encounter did not clear role.");
            smokeView.SetBlank(); ExecuteEvents.Execute(catchRegion.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(clicks == 3, "Blank accepts role input.");
            bool unchanged = smokeSceneBytes.SequenceEqual(File.ReadAllBytes(smokeScenePath)); Require(unchanged, "Smoke changed the saved scene.");
            File.WriteAllText(Output + "UI05PlayModeValidation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = unchanged, assertions = assertions }, true));
        }
        finally { Application.runInBackground = oldBackground; UnityEngine.Object.Destroy(smokeRoot); smokeRoot = null; smokeView = null; }
    }

    /// <summary>Instantiates the authored central template at the actual design size.</summary>
    private static CreatureCardView Create(Transform parent, float x)
    {
        var obj = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/CreatureCard.prefab"), parent);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = new Vector2(x, 0); rect.sizeDelta = new Vector2(1260, 1400);
        return obj.GetComponent<CreatureCardView>();
    }
    /// <summary>Exports a disposable camera render, never a runtime flattened card asset.</summary>
    private static void Capture(GameObject root, Scene scene, int width, int height, string file)
    {
        var obj = new GameObject("Central check camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(obj, scene);
        Camera camera = obj.GetComponent<Camera>(); camera.scene = scene; camera.orthographic = true; camera.orthographicSize = 1080; camera.transform.position = new Vector3(0, 0, -100); camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(17, 30, 37, 255);
        root.GetComponent<Canvas>().worldCamera = camera;
        var target = new RenderTexture(width, height, 24); RenderTexture old = RenderTexture.active; Texture2D image = null;
        try { camera.targetTexture = target; Canvas.ForceUpdateCanvases(); foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(true, true); camera.Render(); RenderTexture.active = target; image = new Texture2D(width, height, TextureFormat.RGBA32, false); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(Output + file, image.EncodeToPNG()); }
        finally { RenderTexture.active = old; camera.targetTexture = null; target.Release(); UnityEngine.Object.DestroyImmediate(target); if (image != null) UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(obj); }
    }
    /// <summary>Checks all text fits without silently truncating longer definitions.</summary>
    private static void RulesFit(CreatureCardView view, string field)
    { TMP_Text text = Text(view, field); text.ForceMeshUpdate(true, true); Require(!text.isTextOverflowing, "Rules overflow: " + Text(view, "cardNameText").text + " / " + field + " size=" + text.fontSize + " rect=" + text.rectTransform.rect + " preferred=" + text.preferredHeight + " lines=" + text.textInfo.lineCount); }
    /// <summary>Checks fitted stat glyph meshes, including zero and large values.</summary>
    private static void Glyphs(TMP_Text text)
    { text.ForceMeshUpdate(true, true); Require(text.textInfo.characterCount > 0 && text.textInfo.characterInfo.Any(c => c.isVisible) && !text.isTextOverflowing, "Glyph missing/overflow: " + text.name + " / " + text.text + " rect=" + text.rectTransform.rect + " font=" + text.fontSize); }
    /// <summary>Reads serialized bindings without modifying the component.</summary>
    private static T Field<T>(CreatureCardView view, string field) where T : UnityEngine.Object
    { return new SerializedObject(view).FindProperty(field).objectReferenceValue as T; }
    /// <summary>Reads a bound dynamic field.</summary>
    private static TMP_Text Text(CreatureCardView view, string field) { return Field<TMP_Text>(view, field); }
    /// <summary>Finds approved production data without depending on asset filenames.</summary>
    private static CardDefinition Card(string name)
    { return AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.DisplayName == name); }
    /// <summary>Uses the existing encounter category boundary.</summary>
    private static bool Catchable(CardDefinition card) { return card.CardType == CardType.Creature || card.CardType == CardType.ApexEncounter || card.CardType == CardType.Treasure; }
    /// <summary>Stops on the first violated visual or input contract.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
