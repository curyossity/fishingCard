using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class FishingRunView : MonoBehaviour
{
    [Header("Creature Card Art")]
    [SerializeField] private Sprite fallbackCreatureCardFace;
    [SerializeField] private Sprite rarityHookSprite;

    private static readonly Color PanelColor = new Color(0.045f, 0.065f, 0.075f, 0.98f);
    private static readonly Color BoatColor = new Color(0.12f, 0.20f, 0.22f, 1f);
    private static readonly Color AccentColor = new Color(0.24f, 0.74f, 0.70f, 1f);
    private static readonly Color DepthColor = new Color(0.42f, 0.70f, 0.88f, 1f);
    private static readonly Color SurfaceColor = new Color(0.88f, 0.67f, 0.25f, 1f);
    private static readonly Color ReleaseColor = new Color(0.78f, 0.32f, 0.28f, 1f);
    private static readonly Color MutedTextColor = new Color(0.65f, 0.71f, 0.73f, 1f);

    private RectTransform gameplayRoot;
    private Text biomeText;
    private Text depthText;
    private Text tierText;
    private CreatureCardView encounterCardView;
    private Text boatCapacityText;
    private Text boatCatchCountText;
    private Button descendButton;
    private Button releaseButton;
    private Button surfaceButton;
    private Text releaseButtonText;
    private Func<bool> descendAction;
    private Func<bool> releaseAction;
    private Func<bool> surfaceAction;
    private Font uiFont;

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
        BiomeDepthTierDefinition depthTier,
        int depth,
        CardDefinition encounter,
        int resolvedEncounterWeight,
        int resolvedEncounterValue,
        bool encounterInformationHidden,
        int lineCapacity,
        int catchCount,
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
        gameplayRoot.gameObject.SetActive(runActive);

        if (!runActive)
        {
            return;
        }

        biomeText.text = biome == null ? "UNCHARTED WATERS" : biome.DisplayName.ToUpperInvariant();
        depthText.text = $"DEPTH {Mathf.Max(0, depth)}";
        tierText.text = depthTier == null ? "BIOME EDGE" : depthTier.DisplayName.ToUpperInvariant();
        boatCapacityText.text = $"LINE CAPACITY  {Mathf.Max(0, lineCapacity)}";
        boatCatchCountText.text = $"ATTACHED  {Mathf.Max(0, catchCount)}";

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
        GameObject rootObject = CreateUiObject("Fishing Run View", transform);
        gameplayRoot = rootObject.GetComponent<RectTransform>();
        SetAnchoredRect(gameplayRoot, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);

        CreateLocationHeader();
        CreateEncounterCard();
        CreateCoreActions();
        CreateBoatCardAndRig();
    }

    /// <summary>
    /// Creates the compact biome, depth, and depth-tier header.
    /// </summary>
    private void CreateLocationHeader()
    {
        GameObject headerObject = CreateUiObject("Location Header", gameplayRoot);
        RectTransform headerRect = headerObject.GetComponent<RectTransform>();
        SetAnchoredRect(headerRect, new Vector2(0.02f, 0.92f), new Vector2(0.60f, 0.98f), 0f, 0f, 0f, 0f);
        AddImage(headerObject, PanelColor);

        biomeText = CreateText("Biome", headerRect, 19, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(biomeText.rectTransform, Vector2.zero, new Vector2(0.48f, 1f), 14f, 0f, 0f, 0f);

        tierText = CreateText("Depth Tier", headerRect, 12, FontStyle.Bold, TextAnchor.MiddleCenter, MutedTextColor);
        SetAnchoredRect(tierText.rectTransform, new Vector2(0.45f, 0f), new Vector2(0.76f, 1f), 0f, 0f, 0f, 0f);

        depthText = CreateText("Depth", headerRect, 15, FontStyle.Bold, TextAnchor.MiddleRight, DepthColor);
        SetAnchoredRect(depthText.rectTransform, new Vector2(0.72f, 0f), Vector2.one, 0f, 0f, -14f, 0f);
    }

    /// <summary>
    /// Creates the portrait creature card used as the complete current Encounter presentation.
    /// </summary>
    private void CreateEncounterCard()
    {
        GameObject regionObject = CreateUiObject("Current Encounter Region", gameplayRoot);
        RectTransform regionRect = regionObject.GetComponent<RectTransform>();
        SetAnchoredRect(regionRect, new Vector2(0.02f, 0.44f), new Vector2(0.39f, 0.91f), 0f, 0f, 0f, 0f);

        GameObject cardObject = CreateUiObject("Current Encounter Card", regionRect);
        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        SetAnchoredRect(cardRect, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);
        AspectRatioFitter aspectRatio = cardObject.AddComponent<AspectRatioFitter>();
        aspectRatio.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatio.aspectRatio = 2f / 3f;

        encounterCardView = cardObject.AddComponent<CreatureCardView>();
        encounterCardView.Initialize(fallbackCreatureCardFace, rarityHookSprite);
    }

    /// <summary>
    /// Creates the permanent Descend, Release, and Surface action controls.
    /// </summary>
    private void CreateCoreActions()
    {
        GameObject actionsObject = CreateUiObject("Core Actions", gameplayRoot);
        RectTransform actionsRect = actionsObject.GetComponent<RectTransform>();
        SetAnchoredRect(actionsRect, new Vector2(0.41f, 0.46f), new Vector2(0.60f, 0.90f), 0f, 0f, 0f, 0f);
        AddImage(actionsObject, PanelColor);

        Text titleText = CreateText("Title", actionsRect, 15, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetAnchoredRect(titleText.rectTransform, new Vector2(0f, 0.84f), Vector2.one, 12f, 0f, -8f, 0f);
        titleText.text = "CORE ACTIONS";

        descendButton = CreateActionButton("Descend", actionsRect, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.80f), AccentColor, "DESCEND", InvokeDescend, out _);
        releaseButton = CreateActionButton("Release", actionsRect, new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.54f), ReleaseColor, "RELEASE", InvokeRelease, out releaseButtonText);
        surfaceButton = CreateActionButton("Surface", actionsRect, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.28f), SurfaceColor, "SURFACE", InvokeSurface, out _);
    }

    /// <summary>
    /// Creates the boat/start card and the visible line that joins it to the Catch Chain.
    /// </summary>
    private void CreateBoatCardAndRig()
    {
        GameObject rigObject = CreateUiObject("Fishing Rig", gameplayRoot);
        RectTransform rigRect = rigObject.GetComponent<RectTransform>();
        SetAnchoredRect(rigRect, new Vector2(0.66f, 0.765f), new Vector2(0.665f, 0.835f), 0f, 0f, 0f, 0f);
        AddImage(rigObject, new Color(0.76f, 0.88f, 0.84f, 1f));

        GameObject boatObject = CreateUiObject("Boat Start Card", gameplayRoot);
        RectTransform boatRect = boatObject.GetComponent<RectTransform>();
        SetAnchoredRect(boatRect, new Vector2(0.66f, 0.83f), new Vector2(0.94f, 0.97f), 0f, 0f, 0f, 0f);
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
    /// Creates a fixed action button and binds its click callback.
    /// </summary>
    private Button CreateActionButton(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        string label,
        UnityEngine.Events.UnityAction clickAction,
        out Text labelText)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetAnchoredRect(buttonRect, anchorMin, anchorMax, 0f, 0f, 0f, 0f);
        Image buttonImage = AddImage(buttonObject, color);
        buttonImage.raycastTarget = true;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(clickAction);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.86f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.disabledColor = new Color(0.38f, 0.40f, 0.41f, 0.75f);
        button.colors = colors;

        labelText = CreateText("Label", buttonRect, 13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetAnchoredRect(labelText.rectTransform, Vector2.zero, Vector2.one, 4f, 0f, -4f, 0f);
        labelText.text = label;
        labelText.resizeTextForBestFit = true;
        labelText.resizeTextMinSize = 9;
        labelText.resizeTextMaxSize = 13;
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
