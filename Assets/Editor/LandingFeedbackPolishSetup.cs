using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the approved larger source cue, readable history and local effect responses within the existing machine.</summary>
public static class LandingFeedbackPolishSetup
{
    private const string Root = TackleDiscoverArtSetup.Root;

    /// <summary>Updates existing fields and adds only missing native layers, retaining prefab identities and supplied art.</summary>
    public static void Configure(LandingScoreRush host, Transform stage)
    {
        var data = new SerializedObject(host);
        var popup = ((CanvasGroup)data.FindProperty("readout").objectReferenceValue).transform;
        var portrait = (UnityEngine.UI.Image)data.FindProperty("sourcePortrait").objectReferenceValue;
        var source = (TMP_Text)data.FindProperty("sourceName").objectReferenceValue;
        var plate = (RectTransform)source.transform.parent;
        Place(plate, 0, 104, 458, 26);
        // Preserve the original source center below the score, with room for the larger animated illustration.
        portrait.transform.SetParent(plate, false); Place(portrait.rectTransform, 149.5f, -19, 72, 64, true);
        var frame = popup.Find("Source medallion");
        if (frame != null) UnityEngine.Object.DestroyImmediate(frame.gameObject);
        Place(source.rectTransform, 230, 0, 218, 26); source.fontSizeMax = 24; source.fontSizeMin = 18;
        var operation = (TMP_Text)data.FindProperty("operation").objectReferenceValue;
        Place(operation.rectTransform, 8, 104, 132, 26); operation.enableAutoSizing = true; operation.fontSizeMin = 24; operation.fontSizeMax = 32;
        var value = (TMP_Text)data.FindProperty("value").objectReferenceValue;
        Place(value.rectTransform, 8, 28, 442, 72, true); value.fontSizeMax = 62;
        var lamps = data.FindProperty("marquee"); lamps.arraySize = 7;
        for (int i = 0; i < 7; i++)
        {
            var lamp = Image(stage, "Score surge lamp " + i, "lamp-on", new Rect(1170 + i * 62, 331, 14, 14));
            lamp.enabled = false; lamps.GetArrayElementAtIndex(i).objectReferenceValue = lamp;
        }
        var presentation = new SerializedObject(host.GetComponent<TackleLandingPresentation>());
        var progress = ((RectTransform)presentation.FindProperty("progressFill").objectReferenceValue).GetComponent<UnityEngine.UI.Image>();
        Bind(data, "progressLight", progress); data.ApplyModifiedPropertiesWithoutUndo();
        ConfigureHistory(host.GetComponent<LandingStrikeFeedback>());
        ConfigureEffects(host, presentation, stage);
    }

    /// <summary>Gives Landing source history full-size portraits while preserving Build's compact version and inspection behavior.</summary>
    private static void ConfigureHistory(LandingStrikeFeedback feedback)
    {
        var data = new SerializedObject(feedback); var scroll = (UnityEngine.UI.ScrollRect)data.FindProperty("chipScroll").objectReferenceValue;
        Place((RectTransform)scroll.transform, 652, 548, 965, 52); Place(scroll.viewport, 0, 0, 965, 52);
        var caption = scroll.transform.parent.Find("Last Hit Sources Caption"); if (caption != null) caption.gameObject.SetActive(false);
        string path = LandingStrikeFeedbackSetup.ChipPath;
        var chip = PrefabUtility.LoadPrefabContents(path);
        try { var surface = chip.GetComponent<UnityEngine.UI.Image>(); surface.sprite = Sprite("status-plaque"); surface.color = Color.white; PrefabUtility.SaveAsPrefabAsset(chip, path); }
        finally { PrefabUtility.UnloadPrefabContents(chip); }
    }

    /// <summary>Connects accuracy assistance to the track and real attempt recovery to its lamps and native source cue.</summary>
    private static void ConfigureEffects(LandingScoreRush host, SerializedObject presentation, Transform stage)
    {
        var effects = host.GetComponent<LandingEffectFeedback>();
        if (effects == null) effects = host.gameObject.AddComponent<LandingEffectFeedback>();
        var data = new SerializedObject(effects); Bind(presentation, "effectFeedback", effects); presentation.ApplyModifiedPropertiesWithoutUndo();
        Bind(data, "rush", host);
        foreach (string zone in new[] { "goodZone", "perfectZone" })
        {
            var target = (RectTransform)presentation.FindProperty(zone).objectReferenceValue;
            var edge = Image(target, "Assisted edges", "focus-corners", new Rect(0, 0, 1, 1));
            edge.rectTransform.anchorMin = Vector2.zero; edge.rectTransform.anchorMax = Vector2.one;
            edge.rectTransform.offsetMin = edge.rectTransform.offsetMax = Vector2.zero; edge.color = Color.clear;
            Bind(data, zone == "goodZone" ? "goodEdge" : "perfectEdge", edge);
        }
        Bind(data, "behavior", presentation.FindProperty("movement").objectReferenceValue);
        var feedback = new SerializedObject(host.GetComponent<LandingStrikeFeedback>());
        Bind(data, "persistentRecovery", feedback.FindProperty("attemptFeedback").objectReferenceValue);
        var slowPortrait = Image(stage, "Slow source portrait", null, new Rect(1312, 750, 30, 22)); slowPortrait.preserveAspect = true; slowPortrait.enabled = false;
        Bind(data, "slowPortrait", slowPortrait);
        var slow = Text(stage, "Active slow cue", new Rect(1348, 748, 240, 30), 24); slow.color = new Color32(160, 232, 216, 255);
        slow.gameObject.SetActive(false); Bind(data, "slowCaption", slow);
        var recovery = Rect(stage, "Attempt recovery cue", new Rect(947, 758, 309, 26));
        var group = recovery.GetComponent<CanvasGroup>();
        if (group == null) group = recovery.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false; group.interactable = false;
        Bind(data, "recovery", group);
        var portrait = Image(recovery, "Recovery source portrait", null, new Rect(0, 2, 28, 22)); portrait.preserveAspect = true;
        Bind(data, "recoveryPortrait", portrait);
        var caption = Text(recovery, "Recovery source caption", new Rect(32, 0, 275, 26), 22);
        caption.enableAutoSizing = true; caption.fontSizeMin = 18; caption.fontSizeMax = 22; caption.color = new Color32(160, 232, 216, 255);
        Bind(data, "recoveryCaption", caption); data.ApplyModifiedPropertiesWithoutUndo(); recovery.gameObject.SetActive(false);
    }

    /// <summary>Finds an owned child or creates one without rebuilding neighboring machine layers.</summary>
    private static RectTransform Rect(Transform parent, string name, Rect bounds)
    {
        var existing = parent.Find(name) as RectTransform;
        var rect = existing != null ? existing : (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        if (existing == null) rect.SetParent(parent, false); Place(rect, bounds.x, bounds.y, bounds.width, bounds.height); return rect;
    }
    /// <summary>Places a reference-pixel rectangle with an optional centered animation pivot.</summary>
    private static void Place(RectTransform rect, float x, float y, float width, float height, bool centered = false)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = centered ? new Vector2(.5f, .5f) : new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x + (centered ? width * .5f : 0), -y - (centered ? height * .5f : 0)); rect.sizeDelta = new Vector2(width, height);
    }
    /// <summary>Uses existing Tackle art, leaving sprite sources untouched and feedback transparent to input.</summary>
    private static UnityEngine.UI.Image Image(Transform parent, string name, string sprite, Rect bounds)
    {
        var rect = Rect(parent, name, bounds); var image = rect.GetComponent<UnityEngine.UI.Image>();
        if (image == null) image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite == null ? null : Sprite(sprite); image.raycastTarget = false; return image;
    }
    /// <summary>Loads a preserved common sprite for a reusable native UI layer.</summary>
    private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Common/" + name + ".png");
    /// <summary>Creates native body text that fits the target region without adding printed card information.</summary>
    private static TMP_Text Text(Transform parent, string name, Rect bounds, float size)
    {
        var rect = Rect(parent, name, bounds); var text = rect.GetComponent<TMP_Text>();
        if (text == null) text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/VT323.asset"); text.fontSize = size;
        text.raycastTarget = false; text.richText = false; text.textWrappingMode = TextWrappingModes.NoWrap; text.alignment = TextAlignmentOptions.Center; return text;
    }
    /// <summary>Serializes exact view ownership rather than finding machine elements in runtime updates.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) => data.FindProperty(field).objectReferenceValue = value;
}
