using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TechniqueHandView : MonoBehaviour
{
    private const int SlotCount = 4;

    [SerializeField] private bool fillParentRegion;
    [SerializeField] private Sprite handPadSprite;
    [SerializeField] private Sprite leftDecorationSprite;
    [SerializeField] private Sprite rightDecorationSprite;
    [SerializeField] private TMP_FontAsset displayFont;
    [SerializeField] private TMP_FontAsset bodyFont;

    private static readonly Color PlayableCardColor = new Color(0.09f, 0.18f, 0.19f, 1f);
    private static readonly Color LockedCardColor = new Color(0.105f, 0.115f, 0.12f, 1f);
    private static readonly Color EmptyCardColor = new Color(0.075f, 0.085f, 0.09f, 1f);
    private static readonly Color AccentColor = new Color(0.29f, 0.76f, 0.72f, 1f);
    private static readonly Color MutedTextColor = new Color(0.62f, 0.67f, 0.68f, 1f);
    private static readonly Color LockedTextColor = new Color(0.76f, 0.49f, 0.43f, 1f);

    private RectTransform panelRoot;
    private RectTransform handContentRoot;
    private RectTransform slotsRoot;
    private TextMeshProUGUI pileCountText;
    private readonly SlotView[] slots = new SlotView[SlotCount];
    private Func<int, bool> useCardAction;

    private sealed class SlotView
    {
        public Image Background;
        public Image Accent;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Rules;
        public TextMeshProUGUI Status;
        public Button UseButton;
        public TextMeshProUGUI UseButtonText;
    }

    /// <summary>
    /// Creates the hand layout when this view first becomes active.
    /// </summary>
    private void Awake()
    {
        EnsureLayout();
    }

    /// <summary>
    /// Displays four stable hand slots, pile counts, and current playability state.
    /// </summary>
    public void Refresh(
        CardDefinition[] hand,
        bool[] playableSlots,
        string[] restrictionReasons,
        int drawPileCount,
        int discardPileCount,
        Func<int, bool> useCardAction)
    {
        EnsureLayout();
        this.useCardAction = useCardAction;
        pileCountText.text = $"DRAW {drawPileCount}     DISCARD {discardPileCount}";

        CardDefinition[] safeHand = hand ?? Array.Empty<CardDefinition>();
        bool[] safePlayableSlots = playableSlots ?? Array.Empty<bool>();
        string[] safeRestrictionReasons = restrictionReasons ?? Array.Empty<string>();

        for (int i = 0; i < SlotCount; i++)
        {
            CardDefinition card = i < safeHand.Length ? safeHand[i] : null;
            bool isPlayable = i < safePlayableSlots.Length && safePlayableSlots[i];
            string restrictionReason = i < safeRestrictionReasons.Length
                ? safeRestrictionReasons[i]
                : string.Empty;

            RefreshSlot(i, card, isPlayable, restrictionReason);
        }
    }

    /// <summary>
    /// Creates the panel and its four reusable card slots once.
    /// </summary>
    private void EnsureLayout()
    {
        if (panelRoot != null)
        {
            return;
        }

        GameObject panelObject = CreateUiObject("Technique Hand Panel", transform);
        panelRoot = panelObject.GetComponent<RectTransform>();
        panelRoot.anchorMin = fillParentRegion ? Vector2.zero : new Vector2(0.02f, 0.04f);
        panelRoot.anchorMax = fillParentRegion ? Vector2.one : new Vector2(0.60f, 0.42f);
        panelRoot.offsetMin = Vector2.zero;
        panelRoot.offsetMax = Vector2.zero;
        CreateHandPad();
        CreateHandDecorations();

        GameObject contentObject = CreateUiObject("Hand Content", panelObject.transform);
        handContentRoot = contentObject.GetComponent<RectTransform>();
        handContentRoot.anchorMin = new Vector2(0.225f, 0f);
        handContentRoot.anchorMax = new Vector2(0.775f, 1f);
        handContentRoot.offsetMin = Vector2.zero;
        handContentRoot.offsetMax = Vector2.zero;

        TextMeshProUGUI titleText = CreateText(
            "Title",
            handContentRoot,
            displayFont,
            17,
            FontWeight.Regular,
            TextAlignmentOptions.MidlineLeft,
            Color.white);
        SetAnchoredRect(
            titleText.rectTransform,
            new Vector2(0f, 1f),
            Vector2.one,
            12f,
            -34f,
            -180f,
            -4f);
        titleText.text = "TECHNIQUE HAND";

        pileCountText = CreateText(
            "Pile Counts",
            handContentRoot,
            bodyFont,
            12,
            FontWeight.SemiBold,
            TextAlignmentOptions.MidlineRight,
            MutedTextColor);
        SetAnchoredRect(
            pileCountText.rectTransform,
            new Vector2(0f, 1f),
            Vector2.one,
            180f,
            -34f,
            -12f,
            -4f);

        GameObject slotsObject = CreateUiObject("Slots", handContentRoot);
        slotsRoot = slotsObject.GetComponent<RectTransform>();
        SetAnchoredRect(slotsRoot, Vector2.zero, Vector2.one, 10f, 10f, -10f, -40f);

        HorizontalLayoutGroup layout = slotsObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        for (int i = 0; i < SlotCount; i++)
        {
            slots[i] = CreateSlot(i);
        }
    }

    /// <summary>
    /// Creates the supplied bottom-center hand backing without stretching its authored proportions.
    /// </summary>
    private void CreateHandPad()
    {
        GameObject padObject = CreateUiObject("Technique Hand Pad", panelRoot);
        RectTransform padRect = padObject.GetComponent<RectTransform>();
        padRect.anchorMin = new Vector2(0f, 0f);
        padRect.anchorMax = new Vector2(1f, 0f);
        padRect.pivot = new Vector2(0.5f, 0f);
        padRect.anchoredPosition = new Vector2(0f, -240f);
        padRect.sizeDelta = Vector2.zero;

        Image padImage = AddImage(padObject, Color.white);
        padImage.sprite = handPadSprite;
        padImage.type = Image.Type.Simple;
        padImage.preserveAspect = true;
        padImage.raycastTarget = false;

        AspectRatioFitter fitter = padObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        fitter.aspectRatio = 1920f / 494f;
    }

    /// <summary>
    /// Places optional supplied decorative clusters above the pad and below interactive hand content.
    /// </summary>
    private void CreateHandDecorations()
    {
        CreateDecoration(
            "Left Map and Dividers",
            leftDecorationSprite,
            new Vector2(0f, 0f),
            new Vector2(54f, -100),
            false);
        CreateDecoration(
            "Right Rope and Compass",
            rightDecorationSprite,
            new Vector2(1f, 0f),
            new Vector2(-100f, -200),
            false);
    }

    /// <summary>
    /// Creates one non-interactive, uncropped decoration at its authored 3:2 aspect ratio.
    /// </summary>
    private void CreateDecoration(
        string objectName,
        Sprite sprite,
        Vector2 anchor,
        Vector2 anchoredPosition,
        bool useCenteredShadow)
    {
        if (sprite == null)
        {
            return;
        }

        GameObject decorationObject = CreateUiObject(objectName, panelRoot);
        RectTransform decorationRect = decorationObject.GetComponent<RectTransform>();
        decorationRect.anchorMin = anchor;
        decorationRect.anchorMax = anchor;
        decorationRect.pivot = new Vector2(0.5f, 0f);
        decorationRect.anchoredPosition = anchoredPosition;
        decorationRect.sizeDelta = new Vector2(690f, 460f);

        Image decorationImage = AddImage(decorationObject, Color.white);
        decorationImage.sprite = sprite;
        decorationImage.type = Image.Type.Simple;
        decorationImage.preserveAspect = true;
        decorationImage.raycastTarget = false;

        if (useCenteredShadow)
        {
            Outline decorationShadow = decorationObject.AddComponent<Outline>();
            decorationShadow.effectColor = new Color(0.015f, 0.025f, 0.025f, 0.52f);
            decorationShadow.effectDistance = new Vector2(5f, 5f);
            decorationShadow.useGraphicAlpha = true;
        }
        else
        {
            Shadow decorationShadow = decorationObject.AddComponent<Shadow>();
            decorationShadow.effectColor = new Color(0.015f, 0.025f, 0.025f, 0.72f);
            decorationShadow.effectDistance = new Vector2(8f, -10f);
            decorationShadow.useGraphicAlpha = true;
        }
    }

    /// <summary>
    /// Updates one hand position without changing the fixed four-slot layout.
    /// </summary>
    private void RefreshSlot(int slotIndex, CardDefinition card, bool isPlayable, string restrictionReason)
    {
        SlotView slot = slots[slotIndex];
        bool hasCard = card != null;

        slot.Background.color = !hasCard
            ? EmptyCardColor
            : isPlayable ? PlayableCardColor : LockedCardColor;
        slot.Accent.color = isPlayable ? AccentColor : MutedTextColor;
        slot.Name.text = hasCard ? card.DisplayName.ToUpperInvariant() : "EMPTY";
        slot.Name.color = hasCard ? Color.white : MutedTextColor;
        slot.Rules.text = hasCard ? card.RulesText : string.Empty;
        slot.Status.text = !hasCard
            ? "NO CARD"
            : isPlayable ? "PLAYABLE" : restrictionReason.ToUpperInvariant();
        slot.Status.color = isPlayable ? AccentColor : LockedTextColor;
        slot.UseButton.interactable = hasCard && isPlayable;
        slot.UseButtonText.text = "USE";
    }

    /// <summary>
    /// Creates one fixed-format Technique card slot and binds its use command.
    /// </summary>
    private SlotView CreateSlot(int slotIndex)
    {
        GameObject slotObject = CreateUiObject($"Technique Slot {slotIndex + 1}", slotsRoot);
        LayoutElement layoutElement = slotObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 92f;
        layoutElement.preferredWidth = 150f;
        layoutElement.flexibleWidth = 1f;

        SlotView slot = new SlotView();
        slot.Background = AddImage(slotObject, EmptyCardColor);

        GameObject accentObject = CreateUiObject("Accent", slotObject.transform);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        SetAnchoredRect(accentRect, Vector2.zero, new Vector2(0f, 1f), 0f, 0f, 5f, 0f);
        slot.Accent = AddImage(accentObject, MutedTextColor);

        slot.Name = CreateText(
            "Name",
            slotObject.transform,
            displayFont,
            16,
            FontWeight.Regular,
            TextAlignmentOptions.TopLeft,
            Color.white);
        SetAnchoredRect(
            slot.Name.rectTransform,
            new Vector2(0f, 1f),
            Vector2.one,
            12f,
            -40f,
            -8f,
            -8f);
        slot.Name.enableAutoSizing = true;
        slot.Name.fontSizeMin = 11;
        slot.Name.fontSizeMax = 16;

        slot.Rules = CreateText(
            "Rules",
            slotObject.transform,
            bodyFont,
            12,
            FontWeight.Regular,
            TextAlignmentOptions.TopLeft,
            MutedTextColor);
        SetAnchoredRect(slot.Rules.rectTransform, Vector2.zero, Vector2.one, 12f, 62f, -8f, -46f);
        slot.Rules.enableAutoSizing = true;
        slot.Rules.fontSizeMin = 9;
        slot.Rules.fontSizeMax = 12;

        slot.Status = CreateText(
            "Status",
            slotObject.transform,
            bodyFont,
            10,
            FontWeight.SemiBold,
            TextAlignmentOptions.MidlineLeft,
            LockedTextColor);
        SetAnchoredRect(
            slot.Status.rectTransform,
            Vector2.zero,
            new Vector2(1f, 0f),
            12f,
            36f,
            -8f,
            58f);
        slot.Status.enableAutoSizing = true;
        slot.Status.fontSizeMin = 8;
        slot.Status.fontSizeMax = 10;

        GameObject buttonObject = CreateUiObject("Use", slotObject.transform);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetAnchoredRect(buttonRect, Vector2.zero, new Vector2(1f, 0f), 10f, 8f, -10f, 34f);
        Image buttonImage = AddImage(buttonObject, AccentColor);
        buttonImage.raycastTarget = true;
        slot.UseButton = buttonObject.AddComponent<Button>();
        slot.UseButton.targetGraphic = buttonImage;
        int capturedSlotIndex = slotIndex;
        slot.UseButton.onClick.AddListener(() => InvokeUse(capturedSlotIndex));

        slot.UseButtonText = CreateText(
            "Label",
            buttonObject.transform,
            displayFont,
            12,
            FontWeight.Regular,
            TextAlignmentOptions.Center,
            Color.white);
        SetAnchoredRect(slot.UseButtonText.rectTransform, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);

        return slot;
    }

    /// <summary>
    /// Forwards a playable slot click to the current gameplay callback.
    /// </summary>
    private void InvokeUse(int slotIndex)
    {
        useCardAction?.Invoke(slotIndex);
    }

    /// <summary>
    /// Creates a UI GameObject on Unity's UI layer under the requested parent.
    /// </summary>
    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    /// <summary>
    /// Adds an image with the requested color; callers may enable raycasts for buttons.
    /// </summary>
    private static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Creates configured TextMeshPro text for the generated hand layout.
    /// </summary>
    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        TMP_FontAsset font,
        float fontSize,
        FontWeight fontWeight,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontWeight = fontWeight;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        if (font == displayFont)
        {
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = 2f;
            text.outlineColor = new Color32(9, 44, 43, 150);
            text.outlineWidth = 0.06f;
        }

        return text;
    }

    /// <summary>
    /// Applies anchors and edge offsets to a generated UI element.
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
