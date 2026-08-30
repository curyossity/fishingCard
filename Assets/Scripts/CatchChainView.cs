using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public sealed class CatchChainView : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.035f, 0.055f, 0.065f, 0.96f);
    private static readonly Color CardColor = new Color(0.09f, 0.12f, 0.13f, 1f);
    private static readonly Color NegativeCardColor = new Color(0.19f, 0.075f, 0.065f, 1f);
    private static readonly Color AccentColor = new Color(0.20f, 0.70f, 0.72f, 1f);
    private static readonly Color NegativeColor = new Color(0.94f, 0.34f, 0.28f, 1f);
    private static readonly Color MutedTextColor = new Color(0.66f, 0.72f, 0.73f, 1f);

    private readonly List<GameObject> entryObjects = new List<GameObject>();

    private RectTransform panelRoot;
    private RectTransform contentRoot;
    private Text emptyStateText;
    private Font uiFont;

    /// <summary>
    /// Creates the runtime layout before the first Catch Chain refresh.
    /// </summary>
    private void Awake()
    {
        EnsureLayout();
    }

    /// <summary>
    /// Rebuilds the visible Catch Chain in acquisition order from current runtime state.
    /// </summary>
    public void Refresh(CardDefinition[] catches, ActiveCatchEffectRecord[] activeEffects)
    {
        EnsureLayout();
        ClearEntries();

        CardDefinition[] safeCatches = catches ?? Array.Empty<CardDefinition>();
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();
        emptyStateText.gameObject.SetActive(safeCatches.Length == 0);

        for (int i = 0; i < safeCatches.Length; i++)
        {
            CreateCatchEntry(safeCatches[i], safeEffects, i);
        }
    }

    /// <summary>
    /// Creates the panel, scrolling content, and empty state when they have not been built yet.
    /// </summary>
    private void EnsureLayout()
    {
        if (panelRoot != null && contentRoot != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject panelObject = CreateUiObject("Catch Chain Panel", transform);
        panelRoot = panelObject.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0.62f, 0.06f);
        panelRoot.anchorMax = new Vector2(0.98f, 0.94f);
        panelRoot.offsetMin = Vector2.zero;
        panelRoot.offsetMax = Vector2.zero;
        AddImage(panelObject, PanelColor);

        Text titleText = CreateText("Title", panelRoot, 20, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(titleText.rectTransform, new Vector2(0f, 1f), Vector2.one, 16f, -46f, -16f, -8f);
        titleText.text = "CATCH CHAIN";

        GameObject viewportObject = CreateUiObject("Viewport", panelRoot);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        SetAnchoredRect(viewport, Vector2.zero, Vector2.one, 12f, 12f, -12f, -52f);
        Image viewportImage = AddImage(viewportObject, new Color(0f, 0f, 0f, 0.01f));
        viewportImage.raycastTarget = true;
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = CreateUiObject("Content", viewport);
        contentRoot = contentObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(1f, 1f);
        contentRoot.pivot = new Vector2(0.5f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;
        contentRoot.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = panelObject.AddComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = contentRoot;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        emptyStateText = CreateText(
            "Empty State",
            viewport,
            15,
            FontStyle.Italic,
            TextAnchor.MiddleCenter,
            MutedTextColor);
        SetAnchoredRect(emptyStateText.rectTransform, Vector2.zero, Vector2.one, 18f, 18f, -18f, -18f);
        emptyStateText.text = "No catches attached";
    }

    /// <summary>
    /// Creates one compact catch card with order, stats, and active effect information.
    /// </summary>
    private void CreateCatchEntry(
        CardDefinition card,
        ActiveCatchEffectRecord[] activeEffects,
        int catchIndex)
    {
        bool hasNegativeEffect = HasNegativeEffect(activeEffects, catchIndex);
        GameObject entryObject = CreateUiObject($"Catch {catchIndex + 1}", contentRoot);
        entryObjects.Add(entryObject);

        AddImage(entryObject, hasNegativeEffect ? NegativeCardColor : CardColor);
        LayoutElement layoutElement = entryObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 116f;
        layoutElement.preferredHeight = 116f;
        layoutElement.flexibleHeight = 0f;

        GameObject accentObject = CreateUiObject("Line", entryObject.transform);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        SetAnchoredRect(accentRect, Vector2.zero, new Vector2(0f, 1f), 0f, 0f, 5f, 0f);
        AddImage(accentObject, hasNegativeEffect ? NegativeColor : AccentColor);

        Text orderText = CreateText(
            "Order",
            entryObject.transform,
            15,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            MutedTextColor);
        SetAnchoredRect(orderText.rectTransform, Vector2.zero, new Vector2(0f, 1f), 10f, 8f, 42f, -8f);
        orderText.text = (catchIndex + 1).ToString("00");

        Text nameText = CreateText("Name", entryObject.transform, 17, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        SetAnchoredRect(nameText.rectTransform, Vector2.zero, Vector2.one, 50f, 82f, -12f, -8f);
        nameText.text = card == null ? "Unknown Catch" : card.DisplayName;

        Text statsText = CreateText("Stats", entryObject.transform, 14, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        SetAnchoredRect(statsText.rectTransform, Vector2.zero, Vector2.one, 50f, 56f, -12f, -34f);
        statsText.supportRichText = true;
        statsText.text = card == null
            ? "WEIGHT --     VALUE --"
            : $"<color=#F2A65A>WEIGHT {card.Weight}</color>     <color=#72D39B>VALUE {card.Value}</color>";

        Text effectsText = CreateText(
            "Effects",
            entryObject.transform,
            12,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            MutedTextColor);
        SetAnchoredRect(effectsText.rectTransform, Vector2.zero, Vector2.one, 50f, 8f, -12f, -58f);
        effectsText.supportRichText = true;
        effectsText.text = BuildEffectsText(activeEffects, catchIndex);
    }

    /// <summary>
    /// Removes all generated catch entries before rebuilding the chain.
    /// </summary>
    private void ClearEntries()
    {
        for (int i = 0; i < entryObjects.Count; i++)
        {
            if (entryObjects[i] != null)
            {
                Destroy(entryObjects[i]);
            }
        }

        entryObjects.Clear();
    }

    /// <summary>
    /// Builds readable effect lines for one Catch Chain position.
    /// </summary>
    private static string BuildEffectsText(ActiveCatchEffectRecord[] activeEffects, int catchIndex)
    {
        StringBuilder summary = new StringBuilder();

        for (int i = 0; i < activeEffects.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffects[i];

            if (record == null || record.SourceCatchIndex != catchIndex || record.Effect == null)
            {
                continue;
            }

            if (summary.Length > 0)
            {
                summary.AppendLine();
            }

            bool isNegative = record.Effect.EffectTone == CardEffectTone.Negative;
            string label = isNegative ? "DOWNSIDE" : BuildEffectLabel(record.ActiveTrigger);
            string color = isNegative ? "#EF675B" : "#58C5C7";
            string description = string.IsNullOrWhiteSpace(record.Effect.ReminderText)
                ? record.Effect.EffectType.ToString()
                : record.Effect.ReminderText;

            summary.Append($"<color={color}><b>{label}</b></color>  {description}");
        }

        return summary.Length == 0 ? "No active effect" : summary.ToString();
    }

    /// <summary>
    /// Returns the display label for an active effect trigger.
    /// </summary>
    private static string BuildEffectLabel(CardEffectTrigger trigger)
    {
        return trigger == CardEffectTrigger.WhenCaught ? "ON CATCH" : "ACTIVE";
    }

    /// <summary>
    /// Checks whether one catch has an explicitly negative tracked effect.
    /// </summary>
    private static bool HasNegativeEffect(ActiveCatchEffectRecord[] activeEffects, int catchIndex)
    {
        for (int i = 0; i < activeEffects.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffects[i];

            if (record != null
                && record.SourceCatchIndex == catchIndex
                && record.Effect != null
                && record.Effect.EffectTone == CardEffectTone.Negative)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a layer-five UI object with a RectTransform under the requested parent.
    /// </summary>
    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    /// <summary>
    /// Adds a non-interactive Image with the requested color.
    /// </summary>
    private static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Creates a configured legacy UI Text element.
    /// </summary>
    private Text CreateText(
        string objectName,
        Transform parent,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    /// <summary>
    /// Assigns anchors and edge offsets to a RectTransform.
    /// </summary>
    private static void SetAnchoredRect(
        RectTransform rectTransform,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float left,
        float bottom,
        float right,
        float top)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(right, top);
    }
}
