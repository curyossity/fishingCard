using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extends a standard Unity Button transition to child graphics such as an icon and runtime label.
/// </summary>
[AddComponentMenu("")]
public sealed class LinkedGraphicButton : Button
{
    private Graphic[] linkedGraphics = Array.Empty<Graphic>();
    private Color[] linkedBaseColors = Array.Empty<Color>();

    /// <summary>
    /// Registers child graphics that must follow this button's visual state.
    /// </summary>
    public void SetLinkedGraphics(params Graphic[] graphics)
    {
        linkedGraphics = graphics ?? Array.Empty<Graphic>();
        linkedBaseColors = new Color[linkedGraphics.Length];

        for (int i = 0; i < linkedGraphics.Length; i++)
        {
            linkedBaseColors[i] = linkedGraphics[i] == null ? Color.white : linkedGraphics[i].color;
        }

        DoStateTransition(currentSelectionState, true);
    }

    /// <summary>
    /// Applies the configured ColorBlock transition to both the plate and registered child graphics.
    /// </summary>
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        Color stateColor = state switch
        {
            SelectionState.Normal => colors.normalColor,
            SelectionState.Highlighted => colors.highlightedColor,
            SelectionState.Pressed => colors.pressedColor,
            SelectionState.Selected => colors.selectedColor,
            SelectionState.Disabled => colors.disabledColor,
            _ => Color.white
        };
        float duration = instant ? 0f : colors.fadeDuration;

        for (int i = 0; i < linkedGraphics.Length; i++)
        {
            Graphic graphic = linkedGraphics[i];
            if (graphic == null)
            {
                continue;
            }

            Color baseColor = linkedBaseColors[i];
            Color targetColor = new Color(
                baseColor.r * stateColor.r * colors.colorMultiplier,
                baseColor.g * stateColor.g * colors.colorMultiplier,
                baseColor.b * stateColor.b * colors.colorMultiplier,
                baseColor.a * stateColor.a);
            graphic.CrossFadeColor(targetColor, duration, true, true);
        }
    }
}

public enum FishingRunSection
{
    Background,
    TopNavigation,
    MainContent,
    TechniqueHand
}

public sealed class FishingRunView : MonoBehaviour
{
    [Header("Creature Card Art")]
    [SerializeField] private CreatureCardView creatureCardPrefab;
    [SerializeField] private Sprite fallbackCreatureCardFace;
    [SerializeField] private Sprite rarityHookSprite;

    [Header("Top Navigation Art")]
    [SerializeField] private Sprite regionPlaqueSprite;
    [SerializeField] private Sprite depthPlaqueSprite;
    [SerializeField] private Sprite deckPlaqueSprite;
    [SerializeField] private Sprite compassIconSprite;
    [SerializeField] private Sprite deckIconSprite;
    [SerializeField] private Sprite settingsIconSprite;
    [SerializeField] private TMP_FontAsset navigationFont;

    [Header("Right Status Art")]
    [SerializeField] private Sprite rightStatusOuterSprite;
    [SerializeField] private Sprite rightStatusDepthSprite;
    [SerializeField] private Sprite rightStatusTensionSprite;
    [SerializeField] private Sprite depthTrackSprite;
    [SerializeField] private Sprite depthNodeActiveSprite;
    [SerializeField] private Sprite depthNodeInactiveSprite;
    [SerializeField] private Sprite tensionTrackSprite;
    [SerializeField] private Sprite tensionFillTealSprite;
    [SerializeField] private Sprite tensionFillCoralSprite;
    [SerializeField] private Sprite descendPlateSprite;
    [SerializeField] private Sprite descendIconSprite;
    [SerializeField] private Sprite releasePlateSprite;
    [SerializeField] private Sprite releaseIconSprite;
    [SerializeField] private Sprite surfacePlateSprite;
    [SerializeField] private Sprite surfaceIconSprite;

    [Header("Stable Layout Regions")]
    [SerializeField] private RectTransform backgroundRegion;
    [SerializeField] private RectTransform topNavigationBar;
    [SerializeField] private RectTransform mainContent;
    [SerializeField] private RectTransform catchRigPanel;
    [SerializeField] private RectTransform encounterPanel;
    [SerializeField] private RectTransform runControlsPanel;
    [SerializeField] private RectTransform techniqueHand;
    [SerializeField] private Canvas tooltipLayer;
    [SerializeField] private Canvas transitionLayer;
    [SerializeField] private Canvas modalLayer;

    [Header("Independent Section Visibility")]
    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private CanvasGroup topNavigationGroup;
    [SerializeField] private CanvasGroup mainContentGroup;
    [SerializeField] private CanvasGroup techniqueHandGroup;

    private static readonly Color BoatColor = new Color(0.12f, 0.20f, 0.22f, 1f);
    private static readonly Color AccentColor = new Color(0.24f, 0.74f, 0.70f, 1f);
    private static readonly Color SurfaceColor = new Color(0.88f, 0.67f, 0.25f, 1f);
    private static readonly Color ReleaseColor = new Color(0.78f, 0.32f, 0.28f, 1f);
    private static readonly Color MutedTextColor = new Color(0.65f, 0.71f, 0.73f, 1f);

    private RectTransform gameplayRoot;
    private TMP_Text biomeText;
    private TMP_Text depthText;
    private TMP_Text deckText;
    private RectTransform depthZoneRowsRoot;
    private Image depthTrackImage;
    private GameObject[] depthZoneRows = Array.Empty<GameObject>();
    private Image[] depthZoneNodeImages = Array.Empty<Image>();
    private TMP_Text[] depthZoneNameTexts = Array.Empty<TMP_Text>();
    private TMP_Text[] depthZoneRangeTexts = Array.Empty<TMP_Text>();
    private TMP_Text tensionText;
    private Image tensionFill;
    private CreatureCardView encounterCardView;
    private Text boatCapacityText;
    private Text boatCatchCountText;
    private Button descendButton;
    private Button releaseButton;
    private Button surfaceButton;
    private Button settingsButton;
    private TMP_Text releaseButtonText;
    private Func<bool> descendAction;
    private Func<bool> releaseAction;
    private Func<bool> surfaceAction;
    private Font uiFont;
    private bool? lastRunActive;

    public RectTransform BackgroundRegion => backgroundRegion;
    public RectTransform TopNavigationBar => topNavigationBar;
    public RectTransform MainContent => mainContent;
    public RectTransform CatchRigPanel => catchRigPanel;
    public RectTransform EncounterPanel => encounterPanel;
    public RectTransform RunControlsPanel => runControlsPanel;
    public RectTransform TechniqueHand => techniqueHand;
    public Canvas TooltipLayer => tooltipLayer;
    public Canvas TransitionLayer => transitionLayer;
    public Canvas ModalLayer => modalLayer;
    public event Action SettingsRequested;

    /// <summary>
    /// Builds the runtime gameplay composition before its first state refresh.
    /// </summary>
    private void Awake()
    {
        EnsureLayout();
    }

    /// <summary>
    /// Redraws the boat, run location, encounter, and permanent action controls from current run state.
    /// </summary>
    public void Refresh(
        bool runActive,
        BiomeDefinition biome,
        int depth,
        CardDefinition encounter,
        int resolvedEncounterWeight,
        int resolvedEncounterValue,
        bool encounterInformationHidden,
        int lineCapacity,
        int currentLineLoad,
        int catchCount,
        int remainingDeckCount,
        int selectedCatchIndex,
        bool canDescend,
        Func<bool> descendAction,
        Func<bool> releaseAction,
        Func<bool> surfaceAction)
    {
        EnsureLayout();
        this.descendAction = descendAction;
        this.releaseAction = releaseAction;
        this.surfaceAction = surfaceAction;
        if (!lastRunActive.HasValue || lastRunActive.Value != runActive)
        {
            SetRunContentVisible(runActive);
            lastRunActive = runActive;
        }

        if (!runActive)
        {
            return;
        }

        biomeText.text = biome == null ? "UNCHARTED WATERS" : biome.DisplayName.ToUpperInvariant();
        depthText.text = $"DEPTH  {Mathf.Max(0, depth)} m";
        deckText.text = $"DECK  {Mathf.Max(0, remainingDeckCount)}";
        boatCapacityText.text = $"LINE CAPACITY  {Mathf.Max(0, lineCapacity)}";
        boatCatchCountText.text = $"ATTACHED  {Mathf.Max(0, catchCount)}";
        RefreshDepthZones(biome, depth);
        RefreshTension(currentLineLoad, lineCapacity);

        encounterCardView.SetCard(
            encounter,
            resolvedEncounterWeight,
            resolvedEncounterValue,
            encounterInformationHidden);

        descendButton.interactable = canDescend;
        releaseButton.interactable = selectedCatchIndex >= 0;
        releaseButtonText.text = selectedCatchIndex >= 0
            ? $"RELEASE CATCH {selectedCatchIndex + 1:00}"
            : "RELEASE";
        surfaceButton.interactable = true;
    }

    /// <summary>
    /// Creates the stable gameplay regions and their reusable controls once.
    /// </summary>
    private void EnsureLayout()
    {
        if (gameplayRoot != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (mainContent != null)
        {
            gameplayRoot = mainContent;
        }
        else
        {
            GameObject rootObject = CreateUiObject("Fishing Run View", transform);
            gameplayRoot = rootObject.GetComponent<RectTransform>();
            SetAnchoredRect(gameplayRoot, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);
        }

        CreateLocationHeader();
        CreateEncounterCard();
        CreateCoreActions();
        CreateBoatCardAndRig();
    }

    /// <summary>
    /// Creates the three supplied navigation plaques and their runtime-owned labels.
    /// </summary>
    private void CreateLocationHeader()
    {
        RectTransform parent = topNavigationBar != null ? topNavigationBar : gameplayRoot;
        GameObject headerObject = CreateUiObject("Location Header", parent);
        RectTransform headerRect = headerObject.GetComponent<RectTransform>();
        if (topNavigationBar != null)
        {
            SetAnchoredRect(headerRect, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);
        }
        else
        {
            SetAnchoredRect(headerRect, new Vector2(0f, 0.9259259f), Vector2.one, 0f, 0f, 0f, 0f);
        }

        biomeText = CreateNavigationPlaque(
            "Region Plaque",
            headerRect,
            regionPlaqueSprite,
            new Vector2(0f, 0.5f),
            new Vector2(92f, 0f),
            new Vector2(520f, 72f),
            TextAlignmentOptions.Center,
            30f);
        depthText = CreateNavigationPlaque(
            "Depth Plaque",
            headerRect,
            depthPlaqueSprite,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(395f, 70f),
            TextAlignmentOptions.Center,
            28f);
        deckText = CreateNavigationPlaque(
            "Deck Plaque",
            headerRect,
            deckPlaqueSprite,
            new Vector2(1f, 0.5f),
            new Vector2(-96f, 0f),
            new Vector2(245f, 68f),
            TextAlignmentOptions.Center,
            27f);

        CreateNavigationIcon(
            "Region Compass Icon",
            headerRect,
            compassIconSprite,
            new Vector2(0f, 0.5f),
            new Vector2(20f, 0f),
            new Vector2(0f, 0.5f),
            new Vector2(56f, 56f));

        RectTransform deckPlaqueRect = deckText.rectTransform.parent as RectTransform;
        CreateNavigationIcon(
            "Deck Icon",
            deckPlaqueRect,
            deckIconSprite,
            new Vector2(0f, 0.5f),
            new Vector2(34f, 0f),
            new Vector2(0.5f, 0.5f),
            new Vector2(46f, 46f));
        deckText.rectTransform.offsetMin = new Vector2(68f, 10f);

        settingsButton = CreateSettingsButton(headerRect);
    }

    /// <summary>
    /// Creates a non-interactive navigation icon without assigning gameplay meaning to it.
    /// </summary>
    private static Image CreateNavigationIcon(
        string objectName,
        RectTransform parent,
        Sprite sprite,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 pivot,
        Vector2 size)
    {
        GameObject iconObject = CreateUiObject(objectName, parent);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = anchor;
        iconRect.anchorMax = anchor;
        iconRect.pivot = pivot;
        iconRect.anchoredPosition = anchoredPosition;
        iconRect.sizeDelta = size;
        Image icon = AddImage(iconObject, Color.white);
        icon.sprite = sprite;
        icon.type = Image.Type.Simple;
        icon.preserveAspect = true;
        return icon;
    }

    /// <summary>
    /// Creates the settings command surface and forwards activation without owning settings behavior.
    /// </summary>
    private Button CreateSettingsButton(RectTransform parent)
    {
        GameObject buttonObject = CreateUiObject("Settings Button", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(-16f, 0f);
        buttonRect.sizeDelta = new Vector2(58f, 58f);

        Image icon = AddImage(buttonObject, Color.white);
        icon.sprite = settingsIconSprite;
        icon.type = Image.Type.Simple;
        icon.preserveAspect = true;
        icon.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = icon;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.90f, 0.90f, 0.90f, 1f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.68f, 0.68f, 0.68f, 1f);
        colors.disabledColor = new Color(0.38f, 0.38f, 0.38f, 0.55f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(InvokeSettings);
        return button;
    }

    /// <summary>
    /// Forwards the settings request to a future settings controller or modal owner.
    /// </summary>
    private void InvokeSettings()
    {
        SettingsRequested?.Invoke();
    }

    /// <summary>
    /// Creates one fixed-size supplied plaque with an independent TextMeshPro runtime label.
    /// </summary>
    private TMP_Text CreateNavigationPlaque(
        string objectName,
        RectTransform parent,
        Sprite sprite,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 size,
        TextAlignmentOptions alignment,
        float fontSize)
    {
        GameObject plaqueObject = CreateUiObject(objectName, parent);
        RectTransform plaqueRect = plaqueObject.GetComponent<RectTransform>();
        plaqueRect.anchorMin = anchor;
        plaqueRect.anchorMax = anchor;
        plaqueRect.pivot = anchor.x < 0.5f
            ? new Vector2(0f, 0.5f)
            : anchor.x > 0.5f
                ? new Vector2(1f, 0.5f)
                : new Vector2(0.5f, 0.5f);
        plaqueRect.anchoredPosition = anchoredPosition;
        plaqueRect.sizeDelta = size;

        Image plaqueImage = AddImage(plaqueObject, Color.white);
        plaqueImage.sprite = sprite;
        plaqueImage.type = Image.Type.Simple;

        GameObject labelObject = CreateUiObject("Label", plaqueRect);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = navigationFont != null ? navigationFont : TMP_Settings.defaultFontAsset;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Normal;
        label.alignment = alignment;
        label.color = new Color(0.075f, 0.12f, 0.12f, 1f);
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f;
        label.fontSizeMax = fontSize;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        SetAnchoredRect(label.rectTransform, Vector2.zero, Vector2.one, 28f, 10f, -28f, -10f);
        return label;
    }

    /// <summary>
    /// Creates the portrait creature card used as the complete current Encounter presentation.
    /// </summary>
    private void CreateEncounterCard()
    {
        RectTransform parent = encounterPanel != null ? encounterPanel : gameplayRoot;
        GameObject regionObject = CreateUiObject("Current Encounter Region", parent);
        RectTransform regionRect = regionObject.GetComponent<RectTransform>();
        if (encounterPanel != null)
        {
            SetAnchoredRect(regionRect, Vector2.zero, Vector2.one, 12f, 12f, -12f, -12f);
        }
        else
        {
            SetAnchoredRect(regionRect, new Vector2(0.02f, 0.44f), new Vector2(0.39f, 0.91f), 0f, 0f, 0f, 0f);
        }

        if (creatureCardPrefab == null)
        {
            Debug.LogError("FishingRunView requires a serialized CreatureCardView prefab.", this);
            return;
        }

        encounterCardView = Instantiate(creatureCardPrefab, regionRect);
        encounterCardView.name = "Current Encounter Card";
        RectTransform cardRect = encounterCardView.GetComponent<RectTransform>();
        SetAnchoredRect(cardRect, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);
        AspectRatioFitter aspectRatio = encounterCardView.gameObject.AddComponent<AspectRatioFitter>();
        aspectRatio.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatio.aspectRatio = 873f / 1358f;
        encounterCardView.Initialize(fallbackCreatureCardFace, rarityHookSprite);
    }

    /// <summary>
    /// Creates the permanent Descend, Release, and Surface action controls.
    /// </summary>
    private void CreateCoreActions()
    {
        RectTransform parent = runControlsPanel != null ? runControlsPanel : gameplayRoot;
        GameObject statusObject = CreateUiObject("Right Status", parent);
        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        if (runControlsPanel != null)
        {
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = new Vector2(0f, -22f);
            statusRect.sizeDelta = new Vector2(416f, 748f);
        }
        else
        {
            statusRect.anchorMin = new Vector2(1f, 0.5f);
            statusRect.anchorMax = new Vector2(1f, 0.5f);
            statusRect.pivot = new Vector2(1f, 0.5f);
            statusRect.anchoredPosition = new Vector2(-24f, 0f);
            statusRect.sizeDelta = new Vector2(416f, 748f);
        }

        Image outerImage = AddImage(statusObject, Color.white);
        outerImage.sprite = rightStatusOuterSprite;
        outerImage.type = Image.Type.Simple;

        CreateDepthStatus(statusRect);
        CreateTensionStatus(statusRect);

        GameObject actionsObject = CreateUiObject("Core Actions", statusRect);
        RectTransform actionsRect = actionsObject.GetComponent<RectTransform>();
        SetAnchoredRect(actionsRect, Vector2.zero, Vector2.one, 24f, 22f, -24f, -386f);

        descendButton = CreateActionButton(
            "Descend",
            actionsRect,
            new Vector2(0.02f, 0.68f),
            new Vector2(0.98f, 0.96f),
            descendPlateSprite,
            descendIconSprite,
            Color.white,
            "DESCEND",
            InvokeDescend,
            out _);
        releaseButton = CreateActionButton(
            "Release",
            actionsRect,
            new Vector2(0.02f, 0.36f),
            new Vector2(0.98f, 0.64f),
            releasePlateSprite,
            releaseIconSprite,
            Color.white,
            "RELEASE",
            InvokeRelease,
            out releaseButtonText);
        surfaceButton = CreateActionButton(
            "Surface",
            actionsRect,
            new Vector2(0.02f, 0.04f),
            new Vector2(0.98f, 0.32f),
            surfacePlateSprite,
            surfaceIconSprite,
            new Color(0.08f, 0.13f, 0.12f, 1f),
            "SURFACE",
            InvokeSurface,
            out _);
    }

    /// <summary>
    /// Creates the supplied Depth Zone subsection and its runtime-authored tier rows.
    /// </summary>
    private void CreateDepthStatus(RectTransform statusRect)
    {
        GameObject depthObject = CreateUiObject("Depth Zone", statusRect);
        RectTransform depthRect = depthObject.GetComponent<RectTransform>();
        depthRect.anchorMin = new Vector2(0.5f, 1f);
        depthRect.anchorMax = new Vector2(0.5f, 1f);
        depthRect.pivot = new Vector2(0.5f, 1f);
        depthRect.anchoredPosition = new Vector2(0f, -8f);
        depthRect.sizeDelta = new Vector2(416f, 256f);

        Image depthImage = AddImage(depthObject, Color.white);
        depthImage.sprite = rightStatusDepthSprite;
        depthImage.type = Image.Type.Simple;

        TMP_Text heading = CreateStatusText("Heading", depthRect, 25f, TextAlignmentOptions.Center, Color.white);
        SetAnchoredRect(heading.rectTransform, Vector2.zero, Vector2.one, 28f, 201f, -28f, -14f);
        heading.text = "DEPTH ZONE";

        GameObject rowsObject = CreateUiObject("Zone Rows", depthRect);
        depthZoneRowsRoot = rowsObject.GetComponent<RectTransform>();
        SetAnchoredRect(depthZoneRowsRoot, Vector2.zero, Vector2.one, 34f, 22f, -34f, -58f);

        GameObject trackObject = CreateUiObject("Depth Track", depthZoneRowsRoot);
        depthTrackImage = AddImage(trackObject, Color.white);
        depthTrackImage.sprite = depthTrackSprite;
        depthTrackImage.type = Image.Type.Tiled;
    }

    /// <summary>
    /// Creates the supplied Tension subsection with a dynamic line-strain fill.
    /// </summary>
    private void CreateTensionStatus(RectTransform statusRect)
    {
        GameObject tensionObject = CreateUiObject("Tension", statusRect);
        RectTransform tensionRect = tensionObject.GetComponent<RectTransform>();
        tensionRect.anchorMin = new Vector2(0.5f, 1f);
        tensionRect.anchorMax = new Vector2(0.5f, 1f);
        tensionRect.pivot = new Vector2(0.5f, 1f);
        tensionRect.anchoredPosition = new Vector2(0f, -270f);
        tensionRect.sizeDelta = new Vector2(416f, 108f);

        Image tensionImage = AddImage(tensionObject, Color.white);
        tensionImage.sprite = rightStatusTensionSprite;
        tensionImage.type = Image.Type.Simple;

        tensionText = CreateStatusText("Label", tensionRect, 21f, TextAlignmentOptions.Center, Color.white);
        SetAnchoredRect(tensionText.rectTransform, Vector2.zero, Vector2.one, 30f, 51f, -30f, -13f);

        GameObject trackObject = CreateUiObject("Track", tensionRect);
        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.5f, 0f);
        trackRect.anchorMax = new Vector2(0.5f, 0f);
        trackRect.pivot = new Vector2(0.5f, 0f);
        trackRect.anchoredPosition = new Vector2(0f, 11f);
        trackRect.sizeDelta = new Vector2(352f, 40f);
        Image trackImage = AddImage(trackObject, Color.white);
        trackImage.sprite = tensionTrackSprite;
        trackImage.type = Image.Type.Simple;

        GameObject fillObject = CreateUiObject("Fill", trackRect);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.5f, 0.5f);
        fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(338f, 24f);
        tensionFill = AddImage(fillObject, Color.white);
        tensionFill.sprite = tensionFillTealSprite;
        tensionFill.type = Image.Type.Filled;
        tensionFill.fillMethod = Image.FillMethod.Horizontal;
        tensionFill.fillOrigin = 0;
    }

    /// <summary>
    /// Reconciles the authored biome tiers and emphasizes the tier containing the current depth.
    /// </summary>
    private void RefreshDepthZones(BiomeDefinition biome, int depth)
    {
        BiomeDepthTierDefinition[] tiers = biome?.DepthTiers ?? Array.Empty<BiomeDepthTierDefinition>();
        EnsureDepthZoneRowCount(tiers.Length);

        for (int i = 0; i < depthZoneRows.Length; i++)
        {
            bool visible = i < tiers.Length && tiers[i] != null;
            depthZoneRows[i].SetActive(visible);
            if (!visible)
            {
                continue;
            }

            BiomeDepthTierDefinition tier = tiers[i];
            bool isActive = tier.ContainsDepth(depth);
            Color rowColor = isActive ? AccentColor : MutedTextColor;
            depthZoneNodeImages[i].sprite = isActive ? depthNodeActiveSprite : depthNodeInactiveSprite;
            depthZoneNameTexts[i].text = tier.DisplayName.ToUpperInvariant();
            depthZoneNameTexts[i].color = rowColor;
            depthZoneNameTexts[i].fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            depthZoneRangeTexts[i].text = tier.MaximumDepth < 0
                ? $"{tier.MinimumDepth}+ m"
                : $"{tier.MinimumDepth}–{tier.MaximumDepth} m";
            depthZoneRangeTexts[i].color = rowColor;
        }
    }

    /// <summary>
    /// Creates enough reusable text rows for the biome's authored depth tiers.
    /// </summary>
    private void EnsureDepthZoneRowCount(int requiredCount)
    {
        float rowHeight = requiredCount > 0 ? 176f / requiredCount : 0f;
        RectTransform trackRect = depthTrackImage.rectTransform;
        trackRect.anchorMin = new Vector2(0f, 1f);
        trackRect.anchorMax = new Vector2(0f, 1f);
        trackRect.pivot = new Vector2(0.5f, 1f);
        trackRect.anchoredPosition = new Vector2(22f, -rowHeight * 0.5f);
        trackRect.sizeDelta = new Vector2(8f, Mathf.Max(0f, rowHeight * (requiredCount - 1)));
        depthTrackImage.gameObject.SetActive(requiredCount > 1);

        if (depthZoneRows.Length == requiredCount)
        {
            return;
        }

        for (int i = 0; i < depthZoneRows.Length; i++)
        {
            if (depthZoneRows[i] != null)
            {
                Destroy(depthZoneRows[i]);
            }
        }

        depthZoneRows = new GameObject[requiredCount];
        depthZoneNodeImages = new Image[requiredCount];
        depthZoneNameTexts = new TMP_Text[requiredCount];
        depthZoneRangeTexts = new TMP_Text[requiredCount];

        for (int i = 0; i < requiredCount; i++)
        {
            GameObject rowObject = CreateUiObject($"Zone {i + 1}", depthZoneRowsRoot);
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -i * rowHeight);
            rowRect.sizeDelta = new Vector2(0f, rowHeight);

            GameObject nodeObject = CreateUiObject("Node", rowRect);
            RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
            nodeRect.anchorMin = new Vector2(0f, 0.5f);
            nodeRect.anchorMax = new Vector2(0f, 0.5f);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.anchoredPosition = new Vector2(22f, 0f);
            nodeRect.sizeDelta = new Vector2(44f, 44f);
            Image nodeImage = AddImage(nodeObject, Color.white);
            nodeImage.sprite = depthNodeInactiveSprite;
            nodeImage.type = Image.Type.Simple;
            nodeImage.preserveAspect = true;

            TMP_Text nameText = CreateStatusText("Name", rowRect, 17f, TextAlignmentOptions.MidlineLeft, MutedTextColor);
            SetAnchoredRect(nameText.rectTransform, Vector2.zero, new Vector2(0.66f, 1f), 50f, 0f, 0f, 0f);
            TMP_Text rangeText = CreateStatusText("Range", rowRect, 15f, TextAlignmentOptions.MidlineRight, MutedTextColor);
            SetAnchoredRect(rangeText.rectTransform, new Vector2(0.60f, 0f), Vector2.one, 0f, 0f, -16f, 0f);

            depthZoneRows[i] = rowObject;
            depthZoneNodeImages[i] = nodeImage;
            depthZoneNameTexts[i] = nameText;
            depthZoneRangeTexts[i] = rangeText;
        }
    }

    /// <summary>
    /// Presents Line Load ratio as line tension without changing gameplay state or thresholds.
    /// </summary>
    private void RefreshTension(int currentLineLoad, int lineCapacity)
    {
        int safeLoad = Mathf.Max(0, currentLineLoad);
        int safeCapacity = Mathf.Max(0, lineCapacity);
        float ratio = safeCapacity > 0 ? (float)safeLoad / safeCapacity : (safeLoad > 0 ? 1f : 0f);
        string state = ratio > 1f ? "CRITICAL" : ratio >= 0.75f ? "HIGH" : ratio >= 0.5f ? "MODERATE" : "LOW";
        bool warning = ratio >= 0.75f;

        tensionText.text = $"TENSION: {state}";
        tensionText.color = warning ? ReleaseColor : Color.white;
        tensionFill.fillAmount = Mathf.Clamp01(ratio);
        tensionFill.sprite = warning ? tensionFillCoralSprite : tensionFillTealSprite;
    }

    /// <summary>
    /// Creates TextMeshPro content using the navigation typeface shared by supplied UI frames.
    /// </summary>
    private TMP_Text CreateStatusText(
        string objectName,
        Transform parent,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = navigationFont != null ? navigationFont : TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.enableAutoSizing = true;
        text.fontSizeMin = 11f;
        text.fontSizeMax = fontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Creates the boat/start card and the visible line that joins it to the Catch Chain.
    /// </summary>
    private void CreateBoatCardAndRig()
    {
        RectTransform parent = catchRigPanel != null ? catchRigPanel : gameplayRoot;
        GameObject rigObject = CreateUiObject("Fishing Rig", parent);
        RectTransform rigRect = rigObject.GetComponent<RectTransform>();
        SetAnchoredRect(
            rigRect,
            catchRigPanel != null ? new Vector2(0.10f, 0.76f) : new Vector2(0.66f, 0.765f),
            catchRigPanel != null ? new Vector2(0.115f, 0.84f) : new Vector2(0.665f, 0.835f),
            0f,
            0f,
            0f,
            0f);
        AddImage(rigObject, new Color(0.76f, 0.88f, 0.84f, 1f));

        GameObject boatObject = CreateUiObject("Boat Start Card", parent);
        RectTransform boatRect = boatObject.GetComponent<RectTransform>();
        SetAnchoredRect(
            boatRect,
            catchRigPanel != null ? new Vector2(0f, 0.82f) : new Vector2(0.66f, 0.83f),
            catchRigPanel != null ? Vector2.one : new Vector2(0.94f, 0.97f),
            0f,
            0f,
            0f,
            0f);
        AddImage(boatObject, BoatColor);

        GameObject accentObject = CreateUiObject("Boat Accent", boatRect);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        SetAnchoredRect(accentRect, Vector2.zero, new Vector2(0f, 1f), 0f, 0f, 6f, 0f);
        AddImage(accentObject, SurfaceColor);

        Text labelText = CreateText("Label", boatRect, 10, FontStyle.Bold, TextAnchor.UpperLeft, SurfaceColor);
        SetAnchoredRect(labelText.rectTransform, new Vector2(0f, 0.64f), new Vector2(1f, 1f), 14f, 0f, -10f, -6f);
        labelText.text = "START CARD";

        Text nameText = CreateText("Name", boatRect, 18, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(nameText.rectTransform, new Vector2(0f, 0.32f), new Vector2(1f, 0.78f), 14f, 0f, -10f, 0f);
        nameText.text = "FISHING BOAT";

        boatCapacityText = CreateText("Capacity", boatRect, 11, FontStyle.Bold, TextAnchor.LowerLeft, MutedTextColor);
        SetAnchoredRect(boatCapacityText.rectTransform, Vector2.zero, new Vector2(0.62f, 0.38f), 14f, 7f, 0f, 0f);

        boatCatchCountText = CreateText("Catch Count", boatRect, 11, FontStyle.Bold, TextAnchor.LowerRight, MutedTextColor);
        SetAnchoredRect(boatCatchCountText.rectTransform, new Vector2(0.55f, 0f), new Vector2(1f, 0.38f), 0f, 7f, -10f, 0f);
    }

    /// <summary>
    /// Invokes the current Descend command supplied by the gameplay controller.
    /// </summary>
    private void InvokeDescend()
    {
        descendAction?.Invoke();
    }

    /// <summary>
    /// Invokes the current selected-catch Release command supplied by the gameplay controller.
    /// </summary>
    private void InvokeRelease()
    {
        releaseAction?.Invoke();
    }

    /// <summary>
    /// Invokes the current Surface command supplied by the gameplay controller.
    /// </summary>
    private void InvokeSurface()
    {
        surfaceAction?.Invoke();
    }

    /// <summary>
    /// Shows or hides one major gameplay section without changing its runtime state.
    /// </summary>
    public void SetSectionVisible(FishingRunSection section, bool visible)
    {
        CanvasGroup group = section switch
        {
            FishingRunSection.Background => backgroundGroup,
            FishingRunSection.TopNavigation => topNavigationGroup,
            FishingRunSection.MainContent => mainContentGroup,
            FishingRunSection.TechniqueHand => techniqueHandGroup,
            _ => null
        };

        SetCanvasGroupVisible(group, visible);
    }

    /// <summary>
    /// Applies run-level visibility while leaving modal, tooltip, and transition layers available.
    /// </summary>
    private void SetRunContentVisible(bool visible)
    {
        SetSectionVisible(FishingRunSection.Background, visible);
        SetSectionVisible(FishingRunSection.TopNavigation, visible);
        SetSectionVisible(FishingRunSection.MainContent, visible);
        SetSectionVisible(FishingRunSection.TechniqueHand, visible);

        if (backgroundGroup == null && topNavigationGroup == null && mainContentGroup == null && techniqueHandGroup == null)
        {
            gameplayRoot.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Updates visual, input, and navigation participation for a section group.
    /// </summary>
    private static void SetCanvasGroupVisible(CanvasGroup group, bool visible)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    /// <summary>
    /// Creates a fixed action button and binds its click callback.
    /// </summary>
    private Button CreateActionButton(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Sprite plateSprite,
        Sprite iconSprite,
        Color labelColor,
        string label,
        UnityEngine.Events.UnityAction clickAction,
        out TMP_Text labelText)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetAnchoredRect(buttonRect, anchorMin, anchorMax, 0f, 0f, 0f, 0f);
        Image buttonImage = AddImage(buttonObject, Color.white);
        buttonImage.sprite = plateSprite;
        buttonImage.type = Image.Type.Simple;
        buttonImage.preserveAspect = true;
        buttonImage.raycastTarget = true;
        LinkedGraphicButton button = buttonObject.AddComponent<LinkedGraphicButton>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(clickAction);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.76f, 0.76f, 0.76f, 1f);
        colors.disabledColor = new Color(0.42f, 0.44f, 0.44f, 0.62f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        GameObject iconObject = CreateUiObject("Icon", buttonRect);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(54f, 0f);
        iconRect.sizeDelta = new Vector2(62f, 62f);
        Image iconImage = AddImage(iconObject, Color.white);
        iconImage.sprite = iconSprite;
        iconImage.type = Image.Type.Simple;
        iconImage.preserveAspect = true;

        labelText = CreateStatusText("Label", buttonRect, 24f, TextAlignmentOptions.Center, labelColor);
        SetAnchoredRect(labelText.rectTransform, Vector2.zero, Vector2.one, 88f, 8f, -22f, -8f);
        labelText.fontStyle = FontStyles.Bold;
        labelText.text = label;
        button.SetLinkedGraphics(iconImage, labelText);
        return button;
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
    /// Adds a non-interactive image with the requested color.
    /// </summary>
    private static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Creates configured legacy UI text for the generated gameplay layout.
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
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
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
