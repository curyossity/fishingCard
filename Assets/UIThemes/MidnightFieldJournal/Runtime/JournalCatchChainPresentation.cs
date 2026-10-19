using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Composes the detailed parchment chain from resolved state using independent scrolling brass links.</summary>
public sealed class JournalCatchChainPresentation : CatchChainPresentation
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private CompactCatchCardView cardPrefab;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform hardware;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private LineLoadMeterView loadMeter;
    [SerializeField] private float cardWidth = 680;
    [SerializeField] private float cardHeight = 382.022472f;
    [SerializeField] private float pitch = 450;
    [SerializeField] private float topPadding = 110;
    [SerializeField] private float bottomPadding = 76;
    [SerializeField] private JournalCatchTooltip tooltipPrefab;
    private JournalCatchTooltip tooltip;
    private JournalCatchInspectionTrigger inspectionSource;
    private CardInstance[] attachments = Array.Empty<CardInstance>();
    private CardInstance inspectedInstance;
    private FishingRunView runView;
    private int inspectionIndex = -1;
    private float showAt;
    private Vector2 inspectionScroll;
    private readonly List<CanvasGroup> inspectionGroups = new List<CanvasGroup>();
    private readonly List<UnityEngine.UI.Graphic> modalGraphics = new List<UnityEngine.UI.Graphic>();
    private readonly Vector3[] inspectionCorners = new Vector3[4];
    private readonly List<CompactCatchCardView> cards = new List<CompactCatchCardView>();
    private readonly List<UnityEngine.UI.Image> links = new List<UnityEngine.UI.Image>();
    private Action<int> selectCatch;
    private int count;

    public IReadOnlyList<CompactCatchCardView> Cards => cards;
    public IReadOnlyList<UnityEngine.UI.Image> Links => links;

    /// <summary>Reuses physical views, refreshes resolved fields and retains the user's current scroll position.</summary>
    public override void Refresh(CardInstance[] catches, ActiveCatchEffectRecord[] activeEffects,
        int currentLineLoad, int lineCapacity, int selectedCatchIndex, Action<int> selectCatchAction)
    {
        if (content == null || viewport == null || hardware == null || cardPrefab == null || theme == null) return;
        selectCatch = selectCatchAction;
        attachments = catches ?? Array.Empty<CardInstance>();
        if (inspectionIndex >= attachments.Length || (inspectionIndex >= 0 && attachments[inspectionIndex] != inspectedInstance)) HideInspection();
        count = catches?.Length ?? 0;
        while (cards.Count < count)
        {
            int index = cards.Count;
            CompactCatchCardView card = Instantiate(cardPrefab, content);
            card.name = "Attached Card " + (index + 1);
            card.SetSelectionHandler(() => selectCatch?.Invoke(index));
            card.gameObject.AddComponent<JournalCatchInspectionTrigger>().Configure(this, index);
            cards.Add(card);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            CompactCatchCardView card = cards[i];
            card.gameObject.SetActive(i < count);
            if (i >= count) continue;
            CardInstance instance = catches[i];
            card.SetCard(instance);
            theme.TryGetCompactArtwork(instance?.Definition?.UniqueId, out Sprite artwork);
            Rect bounds = theme.GetCompactArtworkBounds(instance?.Definition?.UniqueId);
            card.SetArtwork(artwork, bounds);
            card.SetState(i == selectedCatchIndex ? CompactCatchCardState.Selected : CompactCatchCardState.Normal);
        }
        if (emptyText != null) emptyText.gameObject.SetActive(count == 0);
        if (loadMeter != null) loadMeter.SetLoad(currentLineLoad, lineCapacity);
        LayoutChain();
        if (tooltip != null && tooltip.gameObject.activeSelf && inspectedInstance != null) tooltip.Bind(inspectedInstance);
    }

    /// <summary>Requests role-aware inspection without selecting or changing the attachment.</summary>
    public void Inspect(int index, JournalCatchInspectionTrigger source, bool immediate)
    {
        if (index < 0 || index >= attachments.Length || attachments[index]?.Definition == null) return;
        HideInspection(); inspectionSource = source; inspectionIndex = index; inspectedInstance = attachments[index];
        showAt = Time.unscaledTime + (immediate ? 0 : .18f); inspectionScroll = content.anchoredPosition;
    }

    /// <summary>Clears only the trigger that owns the visible or pending inspection.</summary>
    public void EndInspection(JournalCatchInspectionTrigger source) { if (source == inspectionSource) HideInspection(); }

    /// <summary>Dismisses inspection on scroll, input takeover, hidden content or attachment replacement.</summary>
    public void HideInspection()
    {
        inspectionSource = null; inspectionIndex = -1; inspectedInstance = null;
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    /// <summary>Positions a nonblocking tooltip on its source card, never at the moving pointer.</summary>
    private void LateUpdate()
    {
        if (inspectionSource == null) return;
        if (!inspectionSource.Interested || content.anchoredPosition != inspectionScroll || !InspectionAllowed()) { HideInspection(); return; }
        if (Time.unscaledTime < showAt || tooltipPrefab == null) return;
        if (tooltip == null)
        {
            runView = GetComponentInParent<FishingRunView>();
            var canvas = GetComponentInParent<Canvas>();
            Transform parent = runView != null && runView.TooltipLayer != null ? runView.TooltipLayer.transform : canvas != null ? canvas.rootCanvas.transform : null;
            if (parent == null) return;
            tooltip = Instantiate(tooltipPrefab, parent); tooltip.name = "Catch Inspection Tooltip";
            var rect = (RectTransform)tooltip.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            tooltip.gameObject.SetActive(false);
        }
        bool opening = !tooltip.gameObject.activeSelf;
        tooltip.gameObject.SetActive(true); if (opening) tooltip.Bind(inspectedInstance); tooltip.Place((RectTransform)inspectionSource.transform);
    }

    /// <summary>Honors visibility, modal takeover and normal HUD locks without consulting or mutating gameplay rules.</summary>
    private bool InspectionAllowed()
    {
        inspectionGroups.Clear(); GetComponentsInParent(false, inspectionGroups);
        foreach (var group in inspectionGroups) { if (group.alpha < .01f || !group.interactable || !group.blocksRaycasts) return false; if (group.ignoreParentGroups) break; }
        if (runView == null) runView = GetComponentInParent<FishingRunView>();
        if (runView != null)
        {
            if (runView.IsSurfaceTransitionPlaying || runView.IsEncounterTransitionPlaying) return false;
            modalGraphics.Clear(); if (runView.ModalLayer != null) runView.ModalLayer.GetComponentsInChildren(false, modalGraphics);
            foreach (var graphic in modalGraphics) if (graphic.raycastTarget && graphic.isActiveAndEnabled && graphic.canvasRenderer.GetInheritedAlpha() > .01f) return false;
        }
        var source = (RectTransform)inspectionSource.transform;
        source.GetWorldCorners(inspectionCorners);
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        foreach (var corner in inspectionCorners) { float y = viewport.InverseTransformPoint(corner).y; min = Mathf.Min(min, y); max = Mathf.Max(max, y); }
        return max > viewport.rect.yMin && min < viewport.rect.yMax;
    }

    /// <summary>Dismisses an inspection when its chain leaves the visible run.</summary>
    private void OnDisable() { HideInspection(); }
    /// <summary>Releases the separately parented tooltip with its owning chain.</summary>
    private void OnDestroy() { if (tooltip != null) { if (Application.isPlaying) Destroy(tooltip.gameObject); else DestroyImmediate(tooltip.gameObject); } }

    /// <summary>Updates only the heading; no artwork contains the label.</summary>
    public override void SetHeading(string heading)
    {
        if (headingText != null) headingText.text = string.IsNullOrWhiteSpace(heading) ? "CATCH CHAIN" : heading;
    }

    /// <summary>Reflows cards and links on responsive container changes rather than polling gameplay state.</summary>
    private void OnRectTransformDimensionsChange() { LayoutChain(); }

    /// <summary>Sizes the viewport content for any attachment count and clamps scrolling only when its extent changes.</summary>
    private void LayoutChain()
    {
        if (viewport == null || content == null || hardware == null) return;
        float scale = Mathf.Min(1f, Mathf.Max(0, viewport.rect.width - 12) / cardWidth);
        float requiredHeight = count == 0 ? 0 : (topPadding + cardHeight + (count - 1) * pitch + bottomPadding) * scale;
        float height = Mathf.Max(viewport.rect.height, requiredHeight);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        Vector2 position = content.anchoredPosition;
        position.y = Mathf.Clamp(position.y, 0, Mathf.Max(0, height - viewport.rect.height));
        content.anchoredPosition = position;
        for (int i = 0; i < count && i < cards.Count; i++)
        {
            RectTransform rect = cards[i].GetComponent<RectTransform>();
            Place(rect, (topPadding + i * pitch) * scale, cardWidth * scale, cardHeight * scale);
        }
        int needed = count == 0 ? 0 : count + 1;
        while (links.Count < needed)
        {
            GameObject link = new GameObject("Brass Connector " + (links.Count + 1), typeof(RectTransform), typeof(UnityEngine.UI.Image));
            link.layer = gameObject.layer;
            link.transform.SetParent(hardware, false);
            UnityEngine.UI.Image image = link.GetComponent<UnityEngine.UI.Image>();
            theme.TryGetSprite("BrassConnectorV3", out Sprite sprite);
            image.sprite = sprite; image.raycastTarget = false;
            links.Add(image);
        }
        for (int i = 0; i < links.Count; i++)
        {
            links[i].gameObject.SetActive(i < needed);
            if (i >= needed) continue;
            float y, h;
            if (i == 0) { h = 104; y = topPadding + 17 - .845f * h; }
            else if (i == count) { h = 104; y = topPadding + (count - 1) * pitch + cardHeight - 14 - .151f * h; }
            else
            {
                float from = topPadding + (i - 1) * pitch + cardHeight - 14;
                float to = topPadding + i * pitch + 14;
                h = (to - from) / (.845f - .151f); y = from - .151f * h;
            }
            Place(links[i].rectTransform, y * scale, h * (2f / 3f) * scale, h * scale);
        }
        hardware.SetAsLastSibling();
    }

    /// <summary>Positions a centered top-anchored sprite canvas without stretching its source geometry.</summary>
    private static void Place(RectTransform rect, float top, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, -top);
        rect.sizeDelta = new Vector2(width, height);
    }
}
