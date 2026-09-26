using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CatchChainView : MonoBehaviour
{
    [SerializeField] private bool fillParentRegion;
    [SerializeField] private Sprite headingSprite;
    [SerializeField] private TMP_FontAsset headingFont;
    [SerializeField] private string headingLabel = "CATCH RIG";
    [SerializeField] private Sprite rodSprite;
    [SerializeField] private Sprite ringSprite;
    [SerializeField] private Sprite clipSprite;
    [SerializeField] private Sprite medallionSprite;
    [SerializeField] private CompactCatchCardView compactCatchCardPrefab;
    [SerializeField] private Sprite passiveEffectSprite;
    [SerializeField] private Sprite lineLoadPanelSprite;
    [SerializeField] private Sprite lineLoadEmptySprite;
    [SerializeField] private Sprite lineLoadFilledSprite;
    [SerializeField] private Sprite lineLoadDangerSprite;

    private static readonly Color PanelColor = new Color(0.035f, 0.055f, 0.065f, 0.96f);
    private static readonly Color MutedTextColor = new Color(0.66f, 0.72f, 0.73f, 1f);
    private static readonly Color LineLoadTextColor = new Color(0.19f, 0.22f, 0.20f, 1f);

    private readonly List<GameObject> entryObjects = new List<GameObject>();
    private readonly List<Image> lineLoadPips = new List<Image>();

    private RectTransform panelRoot;
    private RectTransform contentRoot;
    private RectTransform lineLoadPipRoot;
    private TMP_Text headingText;
    private TMP_Text lineLoadText;
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
        panelRoot.anchorMin = fillParentRegion ? Vector2.zero : new Vector2(0.62f, 0.06f);
        panelRoot.anchorMax = fillParentRegion ? Vector2.one : new Vector2(0.98f, 0.78f);
        panelRoot.offsetMin = Vector2.zero;
        panelRoot.offsetMax = Vector2.zero;
        AddImage(panelObject, PanelColor);

        CreateHeading();
        CreateLineLoadPanel();

        GameObject viewportObject = CreateUiObject("Viewport", panelRoot);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        SetAnchoredRect(viewport, Vector2.zero, Vector2.one, 12f, 112f, -12f, -70f);
        Image viewportImage = AddImage(viewportObject, new Color(0f, 0f, 0f, 0.01f));
        viewportImage.raycastTarget = true;
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject rigObject = CreateUiObject("Central Fishing Rig", viewport);
        RectTransform rigRect = rigObject.GetComponent<RectTransform>();
        SetAnchoredRect(rigRect, new Vector2(0f, 0f), new Vector2(0f, 1f), 21f, 0f, 43f, 0f);
        Image rigImage = AddImage(rigObject, Color.white);
        rigImage.sprite = rodSprite;
        rigImage.type = Image.Type.Tiled;

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
        SetAnchoredRect(emptyStateText.rectTransform, Vector2.zero, Vector2.one, 112f, 18f, -18f, -18f);
        emptyStateText.text = "No catches attached";
    }

    /// <summary>
    /// Creates the supplied blank Line Load panel with runtime-owned text and pip content.
    /// </summary>
    private void CreateLineLoadPanel()
    {
        GameObject panelObject = CreateUiObject("Line Load Panel", panelRoot);
        RectTransform loadPanelRect = panelObject.GetComponent<RectTransform>();
        loadPanelRect.anchorMin = new Vector2(0.5f, 0f);
        loadPanelRect.anchorMax = new Vector2(0.5f, 0f);
        loadPanelRect.pivot = new Vector2(0.5f, 0f);
        loadPanelRect.anchoredPosition = new Vector2(0f, 4f);
        loadPanelRect.sizeDelta = new Vector2(390f, 100f);

        Image panelImage = AddImage(panelObject, Color.white);
        panelImage.sprite = lineLoadPanelSprite;
        panelImage.type = Image.Type.Simple;

        GameObject labelObject = CreateUiObject("Label", loadPanelRect);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        lineLoadText = label;
        label.font = headingFont != null ? headingFont : TMP_Settings.defaultFontAsset;
        label.fontSize = 23f;
        label.fontStyle = FontStyles.Normal;
        label.alignment = TextAlignmentOptions.Center;
        label.color = LineLoadTextColor;
        label.enableAutoSizing = true;
        label.fontSizeMin = 16f;
        label.fontSizeMax = 23f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        SetAnchoredRect(label.rectTransform, Vector2.zero, Vector2.one, 40f, 46f, -40f, -12f);

        GameObject pipRootObject = CreateUiObject("Capacity Pips", loadPanelRect);
        lineLoadPipRoot = pipRootObject.GetComponent<RectTransform>();
        SetAnchoredRect(lineLoadPipRoot, Vector2.zero, Vector2.one, 26f, 13f, -26f, -54f);
    }

    /// <summary>
    /// Creates the supplied Catch Rig heading plate with independently localizable text.
    /// </summary>
    private void CreateHeading()
    {
        GameObject headingObject = CreateUiObject("Catch Rig Heading", panelRoot);
        RectTransform headingRect = headingObject.GetComponent<RectTransform>();
        headingRect.anchorMin = new Vector2(0.5f, 1f);
        headingRect.anchorMax = new Vector2(0.5f, 1f);
        headingRect.pivot = new Vector2(0.5f, 1f);
        headingRect.anchoredPosition = new Vector2(0f, -4f);
        headingRect.sizeDelta = new Vector2(360f, 60f);

        Image headingImage = AddImage(headingObject, Color.white);
        headingImage.sprite = headingSprite;
        headingImage.type = Image.Type.Simple;

        GameObject labelObject = CreateUiObject("Label", headingRect);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        headingText = label;
        label.font = headingFont != null ? headingFont : TMP_Settings.defaultFontAsset;
        label.fontSize = 28f;
        label.fontStyle = FontStyles.Normal;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.93f, 0.84f, 0.61f, 1f);
        label.enableAutoSizing = true;
        label.fontSizeMin = 20f;
        label.fontSizeMax = 28f;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        SetAnchoredRect(label.rectTransform, Vector2.zero, Vector2.one, 54f, 10f, -54f, -10f);
        label.text = string.IsNullOrWhiteSpace(headingLabel) ? "CATCH RIG" : headingLabel;
    }

    /// <summary>
    /// Updates the heading independently of its supplied frame for localization.
    /// </summary>
    public void SetHeading(string localizedHeading)
    {
        headingLabel = string.IsNullOrWhiteSpace(localizedHeading) ? "CATCH RIG" : localizedHeading;

        if (headingText != null)
        {
            headingText.text = headingLabel;
        }
    }

    /// <summary>
    /// Updates Load/Capacity text and the authored empty, filled, or danger sprite for every capacity slot.
    /// </summary>
    private void RefreshLineLoad(int currentLoad, int capacity)
    {
        int safeLoad = Mathf.Max(0, currentLoad);
        int safeCapacity = Mathf.Max(0, capacity);
        bool isOverloaded = safeLoad > safeCapacity;
        int approachingThreshold = Mathf.CeilToInt(safeCapacity * (2f / 3f));
        bool isApproaching = !isOverloaded && safeCapacity > 0 && safeLoad >= approachingThreshold;
        int dangerStartIndex = Mathf.FloorToInt(safeCapacity * 0.5f);

        lineLoadText.text = $"LINE LOAD  {safeLoad} / {safeCapacity}";

        while (lineLoadPips.Count < safeCapacity)
        {
            GameObject pipObject = CreateUiObject($"Capacity Pip {lineLoadPips.Count + 1}", lineLoadPipRoot);
            Image pipImage = AddImage(pipObject, Color.white);
            pipImage.preserveAspect = true;
            lineLoadPips.Add(pipImage);
        }

        const float availableWidth = 338f;
        float spacing = safeCapacity > 1 ? Mathf.Clamp(6f - safeCapacity * 0.2f, 2f, 5f) : 0f;
        float pipSize = safeCapacity > 0
            ? Mathf.Max(4f, Mathf.Min(24f, (availableWidth - spacing * (safeCapacity - 1)) / safeCapacity))
            : 0f;
        float rowWidth = safeCapacity > 0 ? pipSize * safeCapacity + spacing * (safeCapacity - 1) : 0f;

        for (int i = 0; i < lineLoadPips.Count; i++)
        {
            Image pip = lineLoadPips[i];
            bool isVisibleSlot = i < safeCapacity;
            pip.gameObject.SetActive(isVisibleSlot);
            if (!isVisibleSlot)
            {
                continue;
            }

            bool isOccupied = i < safeLoad;
            bool isDanger = isOccupied && (isOverloaded || (isApproaching && i >= dangerStartIndex));
            pip.sprite = isDanger
                ? lineLoadDangerSprite
                : (isOccupied ? lineLoadFilledSprite : lineLoadEmptySprite);

            RectTransform pipRect = pip.rectTransform;
            pipRect.anchorMin = new Vector2(0.5f, 0.5f);
            pipRect.anchorMax = new Vector2(0.5f, 0.5f);
            pipRect.pivot = new Vector2(0.5f, 0.5f);
            pipRect.sizeDelta = new Vector2(pipSize, pipSize);
            pipRect.anchoredPosition = new Vector2(
                -rowWidth * 0.5f + pipSize * 0.5f + i * (pipSize + spacing),
                0f);
        }
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
        GameObject rowObject = CreateUiObject($"Catch Rig Row {catchIndex + 1}", contentRoot);
        entryObjects.Add(rowObject);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 196f;
        layoutElement.preferredHeight = 196f;
        layoutElement.flexibleHeight = 0f;

        CreateCatchRigPart(
            "Rig Ring",
            rowObject.transform,
            ringSprite,
            new Vector2(32f, 0f),
            new Vector2(46f, 46f),
            0f);
        CreateCatchRigPart(
            "Attachment Clip",
            rowObject.transform,
            clipSprite,
            new Vector2(65f, 0f),
            new Vector2(48f, 48f),
            52f);
        CreateCatchRigPart(
            "Anchor Medallion",
            rowObject.transform,
            medallionSprite,
            new Vector2(96f, 0f),
            new Vector2(48f, 48f),
            0f);

        if (compactCatchCardPrefab == null)
        {
            Debug.LogError("CatchChainView requires a serialized CompactCatchCardView prefab.", this);
            return;
        }

        CompactCatchCardView cardView = Instantiate(compactCatchCardPrefab, rowObject.transform);
        cardView.name = $"Catch Card {catchIndex + 1}";
        RectTransform cardRect = cardView.GetComponent<RectTransform>();
        SetAnchoredRect(cardRect, Vector2.zero, Vector2.one, 112f, 8f, -4f, -8f);
        cardView.SetCard(
            caughtInstance,
            HasActiveEffect(activeEffects, catchIndex) ? passiveEffectSprite : null);
        cardView.SetState(isSelected ? CompactCatchCardState.Selected : CompactCatchCardState.Normal);
        int capturedCatchIndex = catchIndex;
        cardView.SetSelectionHandler(() => SelectCatch(capturedCatchIndex));
    }

    /// <summary>
    /// Creates one authored connector part for a catch row without applying selection-state tint.
    /// </summary>
    private static void CreateCatchRigPart(
        string objectName,
        Transform parent,
        Sprite sprite,
        Vector2 anchoredPosition,
        Vector2 size,
        float rotationDegrees)
    {
        GameObject partObject = CreateUiObject(objectName, parent);
        RectTransform partRect = partObject.GetComponent<RectTransform>();
        partRect.anchorMin = new Vector2(0f, 0.5f);
        partRect.anchorMax = new Vector2(0f, 0.5f);
        partRect.pivot = new Vector2(0.5f, 0.5f);
        partRect.anchoredPosition = anchoredPosition;
        partRect.sizeDelta = size;
        partRect.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);

        Image partImage = AddImage(partObject, Color.white);
        partImage.sprite = sprite;
        partImage.type = Image.Type.Simple;
        partImage.preserveAspect = true;
    }

    /// <summary>
    /// Forwards a catch-card click to the gameplay controller for Release selection.
    /// </summary>
    private void SelectCatch(int catchIndex)
    {
        selectCatchAction?.Invoke(catchIndex);
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
    /// Checks whether the compact card should show its optional passive-effect icon.
    /// </summary>
    private static bool HasActiveEffect(ActiveCatchEffectRecord[] activeEffects, int catchIndex)
    {
        for (int i = 0; i < activeEffects.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffects[i];

            if (record != null
                && record.SourceCatchIndex == catchIndex
                && record.Effect != null)
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
