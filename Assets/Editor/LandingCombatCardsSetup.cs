using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Authors the Landing combat layout: the left mover becomes the player's Strike gauge and the right receiver becomes the encounter
/// creature's specimen plate with a draining Resistance gauge. Both use pixel-art frames from Tools/Art/make_combat_cards.py drawn
/// at true 2x scale. Collision roles are unchanged (left rams right); only what each card shows moves. Idempotent and chained into
/// Landing regeneration right after the collision groups are authored, so every later layer measures the new cards.
/// </summary>
public static class LandingCombatCardsSetup
{
    public const string Folder = "Assets/UIThemes/TackleApparatus/Art/Landing/Combat/";
    public const string StrikeArt = Folder + "strike-panel.png", CreatureArt = Folder + "creature-card.png", HookArt = Folder + "hook-icon.png", BarArt = Folder + "bar-fill.png";
    public const string StrikeAsset = Folder + "strike-panel.asset", CreatureAsset = Folder + "creature-card.asset";
    /// <summary>Card sizes in stage pixels (native art x2) and centers in the shared recess (stage top-left origin).</summary>
    public static readonly Vector2 StrikeSize = new Vector2(452, 220), CreatureSize = new Vector2(428, 232);
    public static readonly Vector2 StrikeCenter = new Vector2(888, 422), CreatureCenter = new Vector2(1408, 422);
    // Live regions in card pixels from the card's top-left (generator layout x2).
    static readonly Rect Display = Rect(26, 38, 426, 156), Tab = Rect(146, 4, 306, 24), Slot = Rect(44, 176, 408, 194);
    static readonly Rect Portrait = Rect(28, 32, 400, 168), TagSlot = Rect(146, 4, 282, 24), Trough = Rect(38, 192, 390, 212);
    static readonly Color Cream = new Color32(242, 230, 200, 255), Tan = new Color32(235, 217, 168, 255), Dim = new Color32(143, 131, 110, 255);

    static Rect Rect(float x0, float y0, float x1, float y1) => new Rect(x0, y0, x1 - x0, y1 - y0);

    /// <summary>Updates the existing Landing prefab in place.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install Combat Cards")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try
        {
            Configure(root);
            LandingCreatureRevealSetup.Configure(root); LandingOutcomeSetup.Configure(root); LandingDamageFeedbackSetup.Configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>The encounter creature's card: the collision's receiving (right) card.</summary>
    public static RectTransform CreatureCard(GameObject root) => (RectTransform)new SerializedObject(root.GetComponent<LandingCardCollision>()).FindProperty("scoreCard").objectReferenceValue;
    /// <summary>The creature card's own flash silhouette.</summary>
    public static LandingPanelFlash CreatureFlash(GameObject root) => (LandingPanelFlash)new SerializedObject(root.GetComponent<LandingCardCollision>()).FindProperty("scoreFlash").objectReferenceValue;
    /// <summary>Whole-card fade used by the entrance and outcome beats.</summary>
    public static CanvasGroup CreatureFade(GameObject root)
    {
        var card = CreatureCard(root); var group = card.GetComponent<CanvasGroup>();
        if (group == null) group = card.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1; group.interactable = false; group.blocksRaycasts = false; return group;
    }

    public static void Configure(GameObject root)
    {
        var collision = new SerializedObject(root.GetComponent<LandingCardCollision>());
        var view = new SerializedObject(root.GetComponent<TackleLandingPresentation>());
        var rush = new SerializedObject(root.GetComponent<LandingScoreRush>());
        var left = (RectTransform)collision.FindProperty("creatureCard").objectReferenceValue;
        var right = (RectTransform)collision.FindProperty("scoreCard").objectReferenceValue;
        var strikeArt = Artwork(StrikeAsset, StrikeArt, 226, 110, new[] { (0f, 12f), (8f, 4f), (67f, 0f), (159f, 0f), (218f, 4f), (226f, 12f), (226f, 102f), (218f, 110f), (8f, 110f), (0f, 102f) });
        var creatureArt = Artwork(CreatureAsset, CreatureArt, 214, 116, new[] { (0f, 12f), (8f, 4f), (67f, 0f), (147f, 0f), (206f, 4f), (214f, 12f), (214f, 108f), (206f, 116f), (8f, 116f), (0f, 108f) });
        Card(left, strikeArt, StrikeSize, StrikeCenter); Card(right, creatureArt, CreatureSize, CreatureCenter);
        CreatureFade(root);

        // Right: the encounter creature, its tag and a draining Resistance gauge.
        var font = ((TMP_Text)view.FindProperty("tags").objectReferenceValue).font;
        var bigFont = ((TMP_Text)view.FindProperty("points").objectReferenceValue).font;
        var portrait = (Image)view.FindProperty("portrait").objectReferenceValue;
        Place(portrait.rectTransform, right, Portrait); portrait.preserveAspect = true;
        var tags = (TMP_Text)view.FindProperty("tags").objectReferenceValue;
        Place(tags.rectTransform, right, TagSlot); Style(tags, bigFont, 9, Tan, 9);
        foreach (string name in new[] { "PointsCaption", "ResistanceCaption", "Separator" }) { var t = right.Find(name); if (t != null) t.gameObject.SetActive(false); }
        ((Component)view.FindProperty("points").objectReferenceValue).gameObject.SetActive(false);
        var fill = (RectTransform)view.FindProperty("progressFill").objectReferenceValue;
        var clip = (RectTransform)fill.parent;
        Place(clip, right, Trough);
        var fillImage = fill.GetComponent<Image>();
        fillImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BarArt); fillImage.type = Image.Type.Sliced; fillImage.color = Color.white; fillImage.pixelsPerUnitMultiplier = 1;
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.pivot = new Vector2(0, .5f); fill.offsetMin = fill.offsetMax = Vector2.zero;
        var plate = Child<Image>(right, "Resistance plate");
        Place(plate.rectTransform, right, new Rect(Trough.center.x - 56, Trough.center.y - 10, 112, 20));
        plate.color = new Color32(18, 12, 8, 230); plate.raycastTarget = false; plate.sprite = null;
        var resistance = (TMP_Text)view.FindProperty("resistance").objectReferenceValue;
        Place(resistance.rectTransform, right, new Rect(Trough.center.x - 56, Trough.center.y - 10, 112, 20));
        Style(resistance, bigFont, 12, Cream, 8); resistance.characterSpacing = 0;
        Order(right, clip, plate.transform, resistance.transform);

        // Left: the player's Strike gauge.
        var readout = (CanvasGroup)rush.FindProperty("readout").objectReferenceValue;
        Place((RectTransform)readout.transform, left, Display);
        var readoutImage = readout.GetComponent<Image>(); if (readoutImage != null) readoutImage.enabled = false;
        var glow = (Image)rush.FindProperty("glow").objectReferenceValue; Stretch(glow.rectTransform);
        var caption = (TMP_Text)rush.FindProperty("caption").objectReferenceValue;
        PlaceIn(caption.rectTransform, Display, Tab); Style(caption, bigFont, 9, Tan, 9);
        var value = (TMP_Text)rush.FindProperty("value").objectReferenceValue;
        Center(value.rectTransform, new Vector2(360, 70)); Style(value, bigFont, 46, new Color32(243, 210, 126, 255), 24);
        var operation = (TMP_Text)rush.FindProperty("operation").objectReferenceValue;
        Center(operation.rectTransform, new Vector2(200, 28)); Style(operation, bigFont, 18, new Color32(240, 205, 133, 255), 10);
        var source = readout.transform.Find("Active source"); if (source != null) source.gameObject.SetActive(false);

        var idle = Child<RectTransform>(left, "Strike idle"); Place(idle, left, new Rect(0, 0, StrikeSize.x, StrikeSize.y));
        var idleCaption = Text(idle, "Idle caption", Tab, bigFont, 9, Tan, 9, "YOUR STRIKE");
        var hook = Child<Image>(idle, "Hook"); hook.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HookArt); hook.raycastTarget = false;
        Place(hook.rectTransform, idle, new Rect(Display.x + 92, Display.y + 22, 32, 52));
        var ready = Text(idle, "Ready", new Rect(Display.x + 136, Display.y + 26, 200, 46), bigFont, 32, Cream, 18, "READY");
        var legend = Text(idle, "Legend", new Rect(Display.x, Display.yMax - 34, Display.width, 22), bigFont, 9, Dim, 9, "GOOD · PERFECT");

        var contributor = Child<Image>(left, "Contributor portrait");
        Place(contributor.rectTransform, left, new Rect(Display.x + 14, Display.y + 10, 170, 92));
        contributor.preserveAspect = true; contributor.raycastTarget = false; contributor.enabled = false; contributor.sprite = null;
        var detail = Text((RectTransform)readout.transform, "Contributor detail", new Rect(14, Display.height - 26, Display.width - 28, 20), bigFont, 8, Dim, 8, "");
        var label = Text(left, "Strike label", Slot, bigFont, 10, Tan, 10, LandingStrikePanel.IdleLabel);
        Order(left, (RectTransform)readout.transform, contributor.transform, idle, label.transform);

        var portraitOwner = root.GetComponentInChildren<LandingScoringPortrait>(true);
        var scoring = new SerializedObject(portraitOwner);
        scoring.FindProperty("portrait").objectReferenceValue = contributor;
        scoring.FindProperty("creatureName").objectReferenceValue = label;
        scoring.FindProperty("tags").objectReferenceValue = detail;
        scoring.ApplyModifiedPropertiesWithoutUndo();

        var panel = root.GetComponent<LandingStrikePanel>(); if (panel == null) panel = root.AddComponent<LandingStrikePanel>();
        var panelData = new SerializedObject(panel);
        panelData.FindProperty("idle").objectReferenceValue = idle.gameObject;
        panelData.FindProperty("legend").objectReferenceValue = legend;
        panelData.FindProperty("label").objectReferenceValue = label;
        panelData.FindProperty("ready").objectReferenceValue = ready;
        panelData.FindProperty("hook").objectReferenceValue = hook;
        panelData.FindProperty("readout").objectReferenceValue = readout;
        panelData.FindProperty("value").objectReferenceValue = value.rectTransform;
        panelData.FindProperty("operation").objectReferenceValue = operation.rectTransform;
        panelData.FindProperty("contributors").objectReferenceValue = portraitOwner;
        panelData.ApplyModifiedPropertiesWithoutUndo();
        view.FindProperty("strikePanel").objectReferenceValue = panel; view.ApplyModifiedPropertiesWithoutUndo();
        // The old score panel's captions no longer exist, so nothing is hidden behind the readout.
        rush.FindProperty("coveredLabels").arraySize = 0; rush.ApplyModifiedPropertiesWithoutUndo();
        Flash(left, strikeArt); Flash(right, creatureArt);
    }

    /// <summary>Imports the pixel-art frame crisply and pairs it with its convex silhouette (flash outline and face clip).</summary>
    static LandingPanelArtwork Artwork(string assetPath, string spritePath, float w, float h, (float x, float y)[] outline)
    {
        Import(spritePath, spritePath == BarArt);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) throw new InvalidOperationException("Missing combat card art " + spritePath + "; run Tools/Art/make_combat_cards.py.");
        var artwork = AssetDatabase.LoadAssetAtPath<LandingPanelArtwork>(assetPath);
        if (artwork == null) { artwork = ScriptableObject.CreateInstance<LandingPanelArtwork>(); AssetDatabase.CreateAsset(artwork, assetPath); }
        var data = new SerializedObject(artwork);
        data.FindProperty("face").objectReferenceValue = sprite;
        var points = data.FindProperty("silhouette"); points.arraySize = outline.Length;
        // Points are normalized with y up, matching the face sprite's UV space.
        for (int i = 0; i < outline.Length; i++) points.GetArrayElementAtIndex(i).vector2Value = new Vector2(outline[i].x / w, 1 - outline[i].y / h);
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(artwork); AssetDatabase.SaveAssetIfDirty(artwork);
        return artwork;
    }

    /// <summary>Pixel art: point filtering, no mipmaps, no compression; the bar fill nine-slices around its 2-pixel edge.</summary>
    public static void Import(string path, bool sliced)
    {
        foreach (var file in new[] { path, HookArt, BarArt })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file);
            if (importer == null) { AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport); importer = (TextureImporter)AssetImporter.GetAtPath(file); }
            bool changed = importer.textureType != TextureImporterType.Sprite || importer.filterMode != FilterMode.Point || importer.mipmapEnabled
                || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.spriteImportMode != SpriteImportMode.Single;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
            if (file == BarArt && importer.spriteBorder != new Vector4(2, 2, 2, 2)) { importer.spriteBorder = new Vector4(2, 2, 2, 2); changed = true; }
            if (changed) importer.SaveAndReimport();
        }
    }

    /// <summary>Sizes a card at true 2x pixel scale around its recess center and binds its frame art.</summary>
    static void Card(RectTransform card, LandingPanelArtwork art, Vector2 size, Vector2 center)
    {
        card.anchorMin = card.anchorMax = new Vector2(0, 1); card.pivot = new Vector2(.5f, .5f);
        card.sizeDelta = size; card.localScale = Vector3.one; card.localRotation = Quaternion.identity;
        card.anchoredPosition = new Vector2(center.x, -center.y);
        var graphic = new SerializedObject(card.GetComponent<LandingCollisionGraphic>());
        graphic.FindProperty("panel").objectReferenceValue = art; graphic.FindProperty("source").objectReferenceValue = null;
        graphic.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Flash(RectTransform card, LandingPanelArtwork art)
    {
        var flash = card.GetComponentInChildren<LandingPanelFlash>(true); if (flash == null) return;
        var data = new SerializedObject(flash); data.FindProperty("panel").objectReferenceValue = art; data.ApplyModifiedPropertiesWithoutUndo();
        flash.transform.SetAsLastSibling();
    }

    /// <summary>Places a field in card pixels measured from the card's top-left corner.</summary>
    static void Place(RectTransform rect, RectTransform card, Rect area)
    {
        rect.SetParent(card, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = area.size; rect.anchoredPosition = new Vector2(area.center.x, -area.center.y);
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }

    /// <summary>Places a child of a region in card pixels, relative to that region's own top-left.</summary>
    static void PlaceIn(RectTransform rect, Rect parent, Rect area)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = area.size; rect.anchoredPosition = new Vector2(area.center.x - parent.x, -(area.center.y - parent.y));
        rect.localScale = Vector3.one;
    }

    static void Center(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = size; rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }

    static T Child<T>(RectTransform parent, string name) where T : Component
    {
        var existing = parent.Find(name);
        var go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false); go.SetActive(true);
        if (typeof(T) == typeof(RectTransform)) return (T)(Component)go.transform;
        var component = go.GetComponent<T>(); if (component == null) component = go.AddComponent<T>();
        return component;
    }

    static TMP_Text Text(RectTransform parent, string name, Rect area, TMP_FontAsset font, float size, Color color, float min, string text)
    {
        var label = Child<TextMeshProUGUI>(parent, name);
        Place(label.rectTransform, parent, area);
        Style(label, font, size, color, min); label.text = text; return label;
    }

    static void Style(TMP_Text text, TMP_FontAsset font, float size, Color color, float min)
    {
        if (font != null) text.font = font;
        text.enableAutoSizing = true; text.fontSizeMax = text.fontSize = size; text.fontSizeMin = min;
        text.color = color; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Truncate;
    }

    /// <summary>Draw order inside a card: listed layers in sequence, after the frame and before the flash.</summary>
    static void Order(RectTransform card, params Transform[] layers) { foreach (var layer in layers) layer.SetAsLastSibling(); }
}
