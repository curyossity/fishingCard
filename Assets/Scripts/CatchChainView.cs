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
    private static readonly Color SelectedCardColor = new Color(0.12f, 0.29f, 0.30f, 1f);
    private static readonly Color AccentColor = new Color(0.20f, 0.70f, 0.72f, 1f);
    private static readonly Color RigColor = new Color(0.76f, 0.88f, 0.84f, 1f);
    private static readonly Color ApproachingColor = new Color(0.95f, 0.65f, 0.24f, 1f);
    private static readonly Color NegativeColor = new Color(0.94f, 0.34f, 0.28f, 1f);
    private static readonly Color MutedTextColor = new Color(0.66f, 0.72f, 0.73f, 1f);

    private readonly List<GameObject> entryObjects = new List<GameObject>();

    private RectTransform panelRoot;
    private RectTransform contentRoot;
    private RectTransform lineLoadFill;
    private Text lineLoadText;
    private Text lineLoadStatusText;
    private Text emptyStateText;
    private Font uiFont;
    private Action<int> selectCatchAction;

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
    public void Refresh(
        CardInstance[] catches,
        ActiveCatchEffectRecord[] activeEffects,
        int currentLineLoad,
        int lineCapacity,
        int selectedCatchIndex,
        Action<int> selectCatchAction)
    {
        EnsureLayout();
        this.selectCatchAction = selectCatchAction;
        ClearEntries();
        RefreshLineLoad(currentLineLoad, lineCapacity);

        CardInstance[] safeCatches = catches ?? Array.Empty<CardInstance>();
        ActiveCatchEffectRecord[] safeEffects = activeEffects ?? Array.Empty<ActiveCatchEffectRecord>();
        emptyStateText.gameObject.SetActive(safeCatches.Length == 0);

        for (int i = 0; i < safeCatches.Length; i++)
        {
            CreateCatchEntry(safeCatches[i], safeEffects, i, i == selectedCatchIndex);
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
        panelRoot.anchorMax = new Vector2(0.98f, 0.78f);
        panelRoot.offsetMin = Vector2.zero;
        panelRoot.offsetMax = Vector2.zero;
        AddImage(panelObject, PanelColor);

        // Keep the same line visible through the panel chrome so the boat and catches read as one rig.
        GameObject panelRigObject = CreateUiObject("Panel Rig Continuation", panelRoot);
        RectTransform panelRigRect = panelRigObject.GetComponent<RectTransform>();
        SetAnchoredRect(panelRigRect, Vector2.zero, new Vector2(0f, 1f), 32f, 0f, 36f, 0f);
        AddImage(panelRigObject, RigColor);

        Text titleText = CreateText("Title", panelRoot, 20, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(titleText.rectTransform, new Vector2(0f, 1f), Vector2.one, 48f, -40f, -16f, -8f);
        titleText.text = "CATCH CHAIN";

        lineLoadText = CreateText(
            "Line Load",
            panelRoot,
            16,
            FontStyle.Bold,
            TextAnchor.MiddleLeft,
            Color.white);
        SetAnchoredRect(lineLoadText.rectTransform, new Vector2(0f, 1f), new Vector2(0.67f, 1f), 48f, -70f, 0f, -42f);

        lineLoadStatusText = CreateText(
            "Line Load Status",
            panelRoot,
            12,
            FontStyle.Bold,
            TextAnchor.MiddleRight,
            AccentColor);
        SetAnchoredRect(lineLoadStatusText.rectTransform, new Vector2(0.5f, 1f), Vector2.one, 0f, -70f, -16f, -42f);

        GameObject loadBarObject = CreateUiObject("Line Load Bar", panelRoot);
        RectTransform loadBarRect = loadBarObject.GetComponent<RectTransform>();
        SetAnchoredRect(loadBarRect, new Vector2(0f, 1f), Vector2.one, 48f, -94f, -16f, -80f);
        AddImage(loadBarObject, new Color(0.16f, 0.20f, 0.21f, 1f));

        GameObject loadFillObject = CreateUiObject("Fill", loadBarRect);
        lineLoadFill = loadFillObject.GetComponent<RectTransform>();
        lineLoadFill.anchorMin = Vector2.zero;
        lineLoadFill.anchorMax = new Vector2(0f, 1f);
        lineLoadFill.offsetMin = Vector2.zero;
        lineLoadFill.offsetMax = Vector2.zero;
        AddImage(loadFillObject, AccentColor);

        GameObject viewportObject = CreateUiObject("Viewport", panelRoot);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        SetAnchoredRect(viewport, Vector2.zero, Vector2.one, 12f, 12f, -12f, -108f);
        Image viewportImage = AddImage(viewportObject, new Color(0f, 0f, 0f, 0.01f));
        viewportImage.raycastTarget = true;
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject rigObject = CreateUiObject("Central Fishing Rig", viewport);
        RectTransform rigRect = rigObject.GetComponent<RectTransform>();
        SetAnchoredRect(rigRect, new Vector2(0f, 0f), new Vector2(0f, 1f), 20f, 0f, 24f, 0f);
        AddImage(rigObject, RigColor);

        GameObject contentObject = CreateUiObject("Content", viewport);
        contentRoot = contentObject.GetComponent<RectTransform>();
        contentRoot.anchorMin = new Vector2(0f, 1f);
        contentRoot.anchorMax = new Vector2(1f, 1f);
        contentRoot.pivot = new Vector2(0.5f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;
        contentRoot.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 0f;
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
        SetAnchoredRect(emptyStateText.rectTransform, Vector2.zero, Vector2.one, 48f, 18f, -18f, -18f);
        emptyStateText.text = "No catches attached";
    }

    /// <summary>
    /// Updates Load/Capacity text, fill amount, and safe, approaching, or overloaded treatment.
    /// </summary>
    private void RefreshLineLoad(int currentLoad, int capacity)
    {
        int safeLoad = Mathf.Max(0, currentLoad);
        int safeCapacity = Mathf.Max(0, capacity);
        float loadRatio = safeCapacity > 0 ? (float)safeLoad / safeCapacity : (safeLoad > 0 ? 1f : 0f);
        bool isOverloaded = safeLoad > safeCapacity;
        bool isApproaching = !isOverloaded && safeCapacity > 0 && loadRatio >= 0.75f;

        Color stateColor = AccentColor;
        string status = "STABLE";

        if (isOverloaded)
        {
            stateColor = NegativeColor;
            status = $"OVERLOADED +{safeLoad - safeCapacity}";
        }
        else if (isApproaching)
        {
            stateColor = ApproachingColor;
            status = "APPROACHING LIMIT";
        }

        lineLoadText.text = $"LINE LOAD  {safeLoad} / {safeCapacity}";
        lineLoadStatusText.text = status;
        lineLoadStatusText.color = stateColor;
        lineLoadFill.anchorMax = new Vector2(Mathf.Clamp01(loadRatio), 1f);
        lineLoadFill.GetComponent<Image>().color = stateColor;
    }

    /// <summary>
    /// Creates one independent catch card attached to the vertical rig by a visible branch connector.
    /// </summary>
    private void CreateCatchEntry(
        CardInstance caughtInstance,
        ActiveCatchEffectRecord[] activeEffects,
        int catchIndex,
        bool isSelected)
    {
        CardDefinition card = caughtInstance?.Definition;
        bool hasNegativeEffect = HasNegativeEffect(activeEffects, catchIndex);
        GameObject rowObject = CreateUiObject($"Catch Rig Row {catchIndex + 1}", contentRoot);
        entryObjects.Add(rowObject);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 124f;
        layoutElement.preferredHeight = 124f;
        layoutElement.flexibleHeight = 0f;

        GameObject rigSegmentObject = CreateUiObject("Rig Segment", rowObject.transform);
        RectTransform rigSegmentRect = rigSegmentObject.GetComponent<RectTransform>();
        SetAnchoredRect(rigSegmentRect, new Vector2(0f, 0f), new Vector2(0f, 1f), 20f, 0f, 24f, 0f);
        AddImage(rigSegmentObject, RigColor);

        GameObject connectorObject = CreateUiObject("Attachment Line", rowObject.transform);
        RectTransform connectorRect = connectorObject.GetComponent<RectTransform>();
        SetAnchoredRect(connectorRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), 22f, -2f, 54f, 2f);
        AddImage(connectorObject, RigColor);

        GameObject knotObject = CreateUiObject("Attachment Knot", rowObject.transform);
        RectTransform knotRect = knotObject.GetComponent<RectTransform>();
        SetAnchoredRect(knotRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), 16f, -6f, 28f, 6f);
        AddImage(knotObject, isSelected ? Color.white : hasNegativeEffect ? NegativeColor : AccentColor);

        GameObject cardObject = CreateUiObject($"Catch Card {catchIndex + 1}", rowObject.transform);
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        SetAnchoredRect(cardRect, Vector2.zero, Vector2.one, 52f, 4f, -4f, -4f);

        Image background = AddImage(
            cardObject,
            isSelected ? SelectedCardColor : hasNegativeEffect ? NegativeCardColor : CardColor);
        background.raycastTarget = true;
        Button selectButton = cardObject.AddComponent<Button>();
        selectButton.targetGraphic = background;
        int capturedCatchIndex = catchIndex;
        selectButton.onClick.AddListener(() => SelectCatch(capturedCatchIndex));

        GameObject accentObject = CreateUiObject("Card Accent", cardObject.transform);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        SetAnchoredRect(accentRect, Vector2.zero, new Vector2(0f, 1f), 0f, 0f, 5f, 0f);
        AddImage(accentObject, isSelected ? Color.white : hasNegativeEffect ? NegativeColor : AccentColor);

        Text orderText = CreateText(
            "Order",
            cardObject.transform,
            15,
            FontStyle.Bold,
            TextAnchor.UpperCenter,
            MutedTextColor);
        SetAnchoredRect(orderText.rectTransform, Vector2.zero, new Vector2(0f, 1f), 10f, 8f, 42f, -8f);
        orderText.text = (catchIndex + 1).ToString("00");

        Text nameText = CreateText("Name", cardObject.transform, 17, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        SetAnchoredRect(nameText.rectTransform, Vector2.zero, Vector2.one, 50f, 82f, -12f, -8f);
        nameText.text = card == null ? "Unknown Catch" : card.DisplayName;

        Text statsText = CreateText("Stats", cardObject.transform, 14, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        SetAnchoredRect(statsText.rectTransform, Vector2.zero, Vector2.one, 50f, 56f, -12f, -34f);
        statsText.supportRichText = true;
        statsText.text = caughtInstance == null
            ? "WEIGHT --     VALUE --"
            : $"<color=#F2A65A>WEIGHT {BuildModifiedStat(caughtInstance.CurrentWeight, caughtInstance.WeightModifier)}</color>     "
                + $"<color=#72D39B>VALUE {BuildModifiedStat(caughtInstance.CurrentValue, caughtInstance.ValueModifier)}</color>";

        Text effectsText = CreateText(
            "Effects",
            cardObject.transform,
            12,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            MutedTextColor);
        SetAnchoredRect(effectsText.rectTransform, Vector2.zero, Vector2.one, 50f, 8f, -12f, -58f);
        effectsText.supportRichText = true;
        effectsText.text = BuildEffectsText(activeEffects, catchIndex);
    }

    /// <summary>
    /// Forwards a catch-card click to the gameplay controller for Release selection.
    /// </summary>
    private void SelectCatch(int catchIndex)
    {
        selectCatchAction?.Invoke(catchIndex);
    }

    /// <summary>
    /// Formats a resolved stat and makes any interaction modifier explicit.
    /// </summary>
    private static string BuildModifiedStat(int currentValue, int modifier)
    {
        if (modifier == 0)
        {
            return currentValue.ToString();
        }

        string sign = modifier > 0 ? "+" : string.Empty;
        return $"{currentValue} ({sign}{modifier})";
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
