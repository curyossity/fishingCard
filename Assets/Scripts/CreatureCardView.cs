using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EncounterEffectChoice
{
    Catch,
    Bait
}

/// <summary>Renders catchable and event encounters from supplied visual layers and runtime-owned fields.</summary>
public sealed class CreatureCardView : MonoBehaviour
{
    public const float ReferenceAspectRatio = 1122f / 1402f;
    public const float EventReferenceAspectRatio = 1101f / 1429f;
    private const int TagMarkerCount = 4;
    private const float EffectFontSizeMin = 10f;
    private const float EffectFontSizeMax = 18f;
    private const float TagTooltipHeight = 32f;
    private const float TagTooltipMinimumWidth = 88f;
    private const float TagTooltipHorizontalPadding = 22f;
    private const float TagTooltipCardGap = 5f;

    private static readonly float[] TagMarkerVerticalAnchors = { 0.669f, 0.585f, 0.502f, 0.419f };

    private static readonly Color32 CatchNameColor = new Color32(239, 226, 194, 255);
    private static readonly Color32 CatchRulesColor = new Color32(25, 55, 54, 255);
    private static readonly Color32 EventContentColor = new Color32(78, 30, 34, 255);
    private static readonly Color32 TagTooltipBackgroundColor = new Color32(14, 50, 52, 245);
    private static readonly Color32 TagTooltipBorderColor = new Color32(196, 157, 82, 255);
    private static readonly Color32 TagTooltipTextColor = new Color32(239, 226, 194, 255);
    private static readonly Color32 SelectedEffectOverlayColor = new Color32(21, 91, 85, 30);
    private static readonly Color32 SelectedEffectBorderColor = new Color32(197, 151, 62, 235);

    [Header("Supplied Visual Layers")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image creatureArtwork;
    [SerializeField] private Image artworkOverflowLayer;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Image interactionOverlay;

    [Header("Runtime Fields")]
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text baitEffectText;

    [Header("Runtime Layout")]
    [SerializeField] private RectTransform rulesRegion;
    [SerializeField] private RectTransform baitRulesRegion;
    [SerializeField] private RectTransform rarityAnchorRoot;

    [Header("Rarity Anchors")]
    [SerializeField] private Image[] anchorSockets = new Image[4];
    [SerializeField] private Image[] anchorMarkers = new Image[4];

    [Header("Tag Rail")]
    [SerializeField] private RectTransform tagIconRoot;
    [SerializeField] private Image[] tagMarkers = new Image[TagMarkerCount];
    [SerializeField] private CardTagIconDefinition[] tagIconDefinitions =
        Array.Empty<CardTagIconDefinition>();

    private RectTransform tagTooltipRoot;
    private TMP_Text tagTooltipText;
    private CardTagTooltipTrigger activeTagTooltipTrigger;
    private Button catchEffectButton;
    private Button baitEffectButton;
    private Image catchEffectSelectionImage;
    private Image baitEffectSelectionImage;
    private Outline catchEffectSelectionOutline;
    private Outline baitEffectSelectionOutline;
    private CardDefinition displayedCard;
    private EncounterEffectChoice selectedEffectChoice = EncounterEffectChoice.Catch;

    [Header("Fallback Assets")]
    [SerializeField] private Sprite fallbackCardFace;
    [SerializeField] private Sprite eventCardFace;
    [SerializeField] private Sprite rarityHookSprite;

    public int AnchorSlotCount => anchorMarkers == null ? 0 : anchorMarkers.Length;
    public Image InteractionOverlay => interactionOverlay;
    public EncounterEffectChoice SelectedEffectChoice => selectedEffectChoice;
    public event Action<CreatureCardView, EncounterEffectChoice> EffectChoiceChanged;

    /// <summary>Preserves the existing setup API while prefab references own the card geometry.</summary>
    public void Initialize(Sprite newFallbackCardFace, Sprite newRarityHookSprite)
    {
        if (newFallbackCardFace != null)
        {
            fallbackCardFace = newFallbackCardFace;
        }

        if (newRarityHookSprite != null)
        {
            rarityHookSprite = newRarityHookSprite;
        }

        ApplyAnchorSprites();
    }

    /// <summary>Displays one encounter using its card-type layout and resolved runtime values.</summary>
    public void SetCard(CardDefinition card, int resolvedWeight, int resolvedValue, bool informationHidden)
    {
        if (card == null)
        {
            displayedCard = null;
            ClearFields();
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        bool isNewCard = displayedCard != card;
        displayedCard = card;
        Sprite encounterArtwork = card.EncounterArtwork != null ? card.EncounterArtwork : card.Artwork;
        bool usesCatchLayout = UsesCatchCardLayout(card.CardType);
        if (isNewCard && usesCatchLayout)
        {
            SelectEffectChoice(EncounterEffectChoice.Catch, false);
        }

        ApplyReferenceAspectRatio(usesCatchLayout);
        CatchAttachmentRole previewRole = selectedEffectChoice == EncounterEffectChoice.Bait
            ? CatchAttachmentRole.Bait
            : CatchAttachmentRole.Catch;
        bool hideOwnValue = previewRole == CatchAttachmentRole.Catch
            && card.HidesOwnValueDuringRunForRole(previewRole);
        Sprite background = usesCatchLayout || eventCardFace == null ? fallbackCardFace : eventCardFace;

        SetContent(
            card.DisplayName.ToUpperInvariant(),
            BuildCardTypeText(card.CardType),
            encounterArtwork,
            card.ArtworkLayout,
            background,
            informationHidden ? "?" : FormatWeight(Mathf.Max(0, resolvedWeight)),
            informationHidden || hideOwnValue ? "?" : FormatValue(Mathf.Max(0, resolvedValue)),
            informationHidden
                ? string.Empty
                : BuildHighlightedRulesText(card.RulesText, card.RulesTextHighlights),
            informationHidden
                ? string.Empty
                : BuildHighlightedRulesText(card.BaitRulesText, card.BaitRulesTextHighlights),
            card.Tags,
            card.Rarity,
            usesCatchLayout,
            card.EncounterArtworkScale,
            card.EncounterArtworkRotation,
            card.EncounterArtworkOffset);
    }

    /// <summary>Populates the visual contract directly for editor previews and UI tests.</summary>
    public void SetPreview(
        string displayName,
        string cardType,
        Sprite artwork,
        int weight,
        int value,
        string rules,
        CardRarity rarity)
    {
        gameObject.SetActive(true);
        ApplyReferenceAspectRatio(true);
        SetContent(
            displayName,
            cardType,
            artwork,
            CardArtworkLayout.MaskedRegion,
            fallbackCardFace,
            FormatWeight(Mathf.Max(0, weight)),
            FormatValue(Mathf.Max(0, value)),
            rules,
            rules,
            Array.Empty<string>(),
            rarity,
            true);
    }

    private static string FormatWeight(int weight)
    {
        return $"{weight}<size=50%>KG</size>";
    }

    private static string FormatValue(int value)
    {
        return $"{value}<size=50%>SL</size>";
    }

    /// <summary>Shows the authored blank template with no runtime values or filled rarity anchors.</summary>
    public void SetBlank()
    {
        gameObject.SetActive(true);
        ApplyReferenceAspectRatio(true);
        SetImage(cardBackground, fallbackCardFace);
        SetImage(creatureArtwork, null);
        SetImage(artworkOverflowLayer, null);
        ApplyCardLayout(true);
        SetText(cardTypeText, string.Empty);
        SetText(cardNameText, string.Empty);
        SetText(weightText, string.Empty);
        SetText(valueText, string.Empty);
        SetText(effectText, string.Empty);
        SetText(baitEffectText, string.Empty);
        HideTagIcons();
        if (cardFrame != null)
        {
            cardFrame.enabled = false;
        }

        if (anchorMarkers != null)
        {
            for (int i = 0; i < anchorMarkers.Length; i++)
            {
                if (anchorMarkers[i] != null)
                {
                    anchorMarkers[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>Updates supplied images and runtime text without changing authored geometry.</summary>
    private void SetContent(
        string displayName,
        string cardType,
        Sprite artwork,
        CardArtworkLayout artworkLayout,
        Sprite background,
        string weight,
        string value,
        string catchRules,
        string baitRules,
        string[] tags,
        CardRarity rarity,
        bool usesCatchLayout,
        float encounterArtworkScale = 1f,
        float encounterArtworkRotation = 0f,
        Vector2 encounterArtworkOffset = default)
    {
        ApplyCardLayout(usesCatchLayout);
        SetImage(cardBackground, background);
        bool usesFullCardOverlay = artwork != null && artworkLayout == CardArtworkLayout.FullCardOverlay;
        SetImage(creatureArtwork, usesFullCardOverlay ? null : artwork);
        SetImage(artworkOverflowLayer, usesFullCardOverlay ? artwork : null);
        ApplyEncounterArtworkTransform(
            usesFullCardOverlay ? encounterArtworkScale : 1f,
            usesFullCardOverlay ? encounterArtworkRotation : 0f,
            usesFullCardOverlay ? encounterArtworkOffset : Vector2.zero);

        if (cardFrame != null)
        {
            cardFrame.enabled = false;
        }

        SetText(cardTypeText, cardType);
        SetText(cardNameText, displayName);
        SetText(weightText, weight);
        SetText(valueText, value);
        SetText(effectText, catchRules);
        SetText(baitEffectText, baitRules);
        if (usesCatchLayout)
        {
            RefreshRarityAnchors(rarity);
            RefreshTagIcons(tags);
        }
        else
        {
            HideRarityAnchors();
            HideTagIcons();
        }
    }

    /// <summary>Switches between catch-only stats and the expanded event rules region.</summary>
    private void ApplyCardLayout(bool usesCatchLayout)
    {
        EnsureDualEffectFields();
        EnsureEffectChoiceButtons();
        SetParentActive(weightText, usesCatchLayout);
        SetParentActive(valueText, usesCatchLayout);

        if (rarityAnchorRoot != null)
        {
            rarityAnchorRoot.gameObject.SetActive(usesCatchLayout);
        }

        if (tagIconRoot != null)
        {
            tagIconRoot.gameObject.SetActive(usesCatchLayout);
        }

        if (rulesRegion != null)
        {
            rulesRegion.anchorMin = usesCatchLayout
                ? new Vector2(0.095f, 0.14f)
                : new Vector2(0.085f, 0.055f);
            rulesRegion.anchorMax = usesCatchLayout
                ? new Vector2(0.477f, 0.258f)
                : new Vector2(0.915f, 0.315f);
            rulesRegion.offsetMin = Vector2.zero;
            rulesRegion.offsetMax = Vector2.zero;
        }

        if (baitRulesRegion != null)
        {
            baitRulesRegion.gameObject.SetActive(usesCatchLayout);
            baitRulesRegion.anchorMin = new Vector2(0.523f, 0.14f);
            baitRulesRegion.anchorMax = new Vector2(0.905f, 0.258f);
            baitRulesRegion.offsetMin = Vector2.zero;
            baitRulesRegion.offsetMax = Vector2.zero;
        }

        if (cardNameText != null)
        {
            cardNameText.color = usesCatchLayout ? CatchNameColor : EventContentColor;
        }

        ConfigureEffectText(effectText, usesCatchLayout);
        ConfigureEffectText(baitEffectText, usesCatchLayout);
        SetEffectChoiceButtonsActive(usesCatchLayout);
    }

    /// <summary>Supports existing prefabs while the editor builder authors both supplied effect regions.</summary>
    private void EnsureDualEffectFields()
    {
        if (baitRulesRegion != null || rulesRegion == null)
        {
            return;
        }

        GameObject clone = Instantiate(rulesRegion.gameObject, rulesRegion.parent);
        clone.name = "Bait Rules Safe Region";
        baitRulesRegion = clone.GetComponent<RectTransform>();
        baitEffectText = clone.GetComponentInChildren<TMP_Text>(true);
        if (baitEffectText != null)
        {
            baitEffectText.name = "Bait Effect Text";
        }
    }

    private static void ConfigureEffectText(TMP_Text text, bool usesCatchLayout)
    {
        if (text == null)
        {
            return;
        }

        text.richText = true;
        text.fontSize = EffectFontSizeMax;
        text.fontSizeMin = EffectFontSizeMin;
        text.fontSizeMax = EffectFontSizeMax;
        text.enableAutoSizing = true;
        text.color = usesCatchLayout ? CatchRulesColor : EventContentColor;
        text.outlineColor = new Color32(0, 0, 0, 0);
        text.outlineWidth = 0f;
        text.rectTransform.offsetMin = usesCatchLayout
            ? new Vector2(12f, 8f)
            : new Vector2(22f, 10f);
        text.rectTransform.offsetMax = usesCatchLayout
            ? new Vector2(-12f, -8f)
            : new Vector2(-22f, -10f);
    }

    /// <summary>Turns the two supplied effect panels into separate reusable selection controls.</summary>
    private void EnsureEffectChoiceButtons()
    {
        if (catchEffectButton == null)
        {
            ConfigureEffectChoiceButton(
                rulesRegion,
                EncounterEffectChoice.Catch,
                out catchEffectButton,
                out catchEffectSelectionImage,
                out catchEffectSelectionOutline);
        }

        if (baitEffectButton == null)
        {
            ConfigureEffectChoiceButton(
                baitRulesRegion,
                EncounterEffectChoice.Bait,
                out baitEffectButton,
                out baitEffectSelectionImage,
                out baitEffectSelectionOutline);
        }
    }

    private void ConfigureEffectChoiceButton(
        RectTransform region,
        EncounterEffectChoice choice,
        out Button button,
        out Image selectionImage,
        out Outline selectionOutline)
    {
        button = null;
        selectionImage = null;
        selectionOutline = null;
        if (region == null)
        {
            return;
        }

        selectionImage = region.GetComponent<Image>();
        if (selectionImage == null)
        {
            selectionImage = region.gameObject.AddComponent<Image>();
        }

        selectionImage.sprite = null;
        selectionImage.type = Image.Type.Simple;
        selectionImage.raycastTarget = true;

        CanvasGroup raycastGroup = region.GetComponent<CanvasGroup>();
        if (raycastGroup == null)
        {
            raycastGroup = region.gameObject.AddComponent<CanvasGroup>();
        }

        raycastGroup.interactable = true;
        raycastGroup.blocksRaycasts = true;
        raycastGroup.ignoreParentGroups = true;

        selectionOutline = region.GetComponent<Outline>();
        if (selectionOutline == null)
        {
            selectionOutline = region.gameObject.AddComponent<Outline>();
        }

        selectionOutline.effectColor = SelectedEffectBorderColor;
        selectionOutline.effectDistance = new Vector2(2f, -2f);
        selectionOutline.useGraphicAlpha = false;

        button = region.GetComponent<Button>();
        if (button == null)
        {
            button = region.gameObject.AddComponent<Button>();
        }

        button.targetGraphic = selectionImage;
        button.transition = Selectable.Transition.None;
        if (choice == EncounterEffectChoice.Catch)
        {
            button.onClick.RemoveListener(SelectCatchEffect);
            button.onClick.AddListener(SelectCatchEffect);
        }
        else
        {
            button.onClick.RemoveListener(SelectBaitEffect);
            button.onClick.AddListener(SelectBaitEffect);
        }
    }

    private void SelectCatchEffect()
    {
        SelectEffectChoice(EncounterEffectChoice.Catch, true);
    }

    private void SelectBaitEffect()
    {
        SelectEffectChoice(EncounterEffectChoice.Bait, true);
    }

    private void SetEffectChoiceButtonsActive(bool active)
    {
        if (catchEffectButton != null)
        {
            catchEffectButton.enabled = active;
            catchEffectButton.interactable = active;
        }

        if (baitEffectButton != null)
        {
            baitEffectButton.enabled = active;
            baitEffectButton.interactable = active;
        }

        ApplyEffectChoiceVisuals(active);
    }

    /// <summary>Selects one built-in effect panel without assigning gameplay behavior to it.</summary>
    public void SelectEffectChoice(EncounterEffectChoice choice, bool notify = true)
    {
        selectedEffectChoice = choice;
        ApplyEffectChoiceVisuals(true);
        if (notify)
        {
            EffectChoiceChanged?.Invoke(this, choice);
        }
    }

    private void ApplyEffectChoiceVisuals(bool active)
    {
        ApplyEffectChoiceVisual(
            catchEffectSelectionImage,
            catchEffectSelectionOutline,
            active && selectedEffectChoice == EncounterEffectChoice.Catch,
            active);
        ApplyEffectChoiceVisual(
            baitEffectSelectionImage,
            baitEffectSelectionOutline,
            active && selectedEffectChoice == EncounterEffectChoice.Bait,
            active);
    }

    private static void ApplyEffectChoiceVisual(
        Image image,
        Outline outline,
        bool selected,
        bool active)
    {
        if (image != null)
        {
            image.enabled = active;
            image.color = selected ? SelectedEffectOverlayColor : Color.clear;
        }

        if (outline != null)
        {
            outline.enabled = active && selected;
        }
    }

    /// <summary>Applies authored TMP color and relative-size tags to matching rules-text phrases.</summary>
    private static string BuildHighlightedRulesText(
        string rules,
        CardTextHighlightDefinition[] highlights)
    {
        if (string.IsNullOrEmpty(rules) || highlights == null || highlights.Length == 0)
        {
            return rules ?? string.Empty;
        }

        StringBuilder formatted = new StringBuilder(rules.Length + 32);
        int position = 0;
        while (position < rules.Length)
        {
            if (rules[position] == '<')
            {
                int tagEnd = rules.IndexOf('>', position);
                if (tagEnd >= position)
                {
                    formatted.Append(rules, position, tagEnd - position + 1);
                    position = tagEnd + 1;
                    continue;
                }
            }

            CardTextHighlightDefinition selectedHighlight = null;
            int selectedLength = 0;
            for (int highlightIndex = 0; highlightIndex < highlights.Length; highlightIndex++)
            {
                CardTextHighlightDefinition highlight = highlights[highlightIndex];
                string highlightedText = highlight?.Text;
                if (string.IsNullOrEmpty(highlightedText)
                    || highlightedText.Length <= selectedLength
                    || position + highlightedText.Length > rules.Length
                    || string.Compare(
                        rules,
                        position,
                        highlightedText,
                        0,
                        highlightedText.Length,
                        StringComparison.OrdinalIgnoreCase) != 0
                    || (highlight.MatchWholeWord
                        && !IsWholeWordMatch(rules, position, highlightedText.Length)))
                {
                    continue;
                }

                selectedHighlight = highlight;
                selectedLength = highlightedText.Length;
            }

            if (selectedHighlight == null)
            {
                formatted.Append(rules[position]);
                position++;
                continue;
            }

            string colorHex = ColorUtility.ToHtmlStringRGB(selectedHighlight.Color);
            int sizePercent = selectedHighlight.SizePercent;
            formatted.Append("<color=#");
            formatted.Append(colorHex);
            formatted.Append("><size=");
            formatted.Append(sizePercent);
            formatted.Append("%>");
            formatted.Append(rules, position, selectedLength);
            formatted.Append("</size></color>");
            position += selectedLength;
        }

        return formatted.ToString();
    }

    /// <summary>Prevents a word highlight from matching inside a longer letter/number identifier.</summary>
    private static bool IsWholeWordMatch(string rules, int startIndex, int length)
    {
        bool startsAtBoundary = startIndex == 0 || !IsWordCharacter(rules[startIndex - 1]);
        int endIndex = startIndex + length;
        bool endsAtBoundary = endIndex >= rules.Length || !IsWordCharacter(rules[endIndex]);
        return startsAtBoundary && endsAtBoundary;
    }

    /// <summary>Defines the characters considered part of one highlightable word.</summary>
    private static bool IsWordCharacter(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_';
    }

    private void HideRarityAnchors()
    {
        if (anchorMarkers == null)
        {
            return;
        }

        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null)
            {
                anchorMarkers[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Shows one through four filled anchors while keeping all four sockets visible.</summary>
    private void RefreshRarityAnchors(CardRarity rarity)
    {
        ApplyAnchorSprites();
        if (anchorMarkers == null)
        {
            return;
        }

        int activeCount = Mathf.Clamp((int)rarity + 1, 1, 4);
        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null)
            {
                anchorMarkers[i].gameObject.SetActive(i < activeCount);
            }
        }
    }

    private void ApplyAnchorSprites()
    {
        if (anchorMarkers == null)
        {
            return;
        }

        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null && anchorMarkers[i].sprite == null)
            {
                anchorMarkers[i].sprite = rarityHookSprite;
            }
        }
    }

    /// <summary>Displays supported authored tags in rail order while leaving unsupported and unused sockets empty.</summary>
    private void RefreshTagIcons(string[] tags)
    {
        EnsureTagMarkers();
        HideTagIcons();
        if (tagIconRoot != null)
        {
            tagIconRoot.gameObject.SetActive(true);
        }

        if (tags == null || tagIconDefinitions == null)
        {
            return;
        }

        int markerIndex = 0;
        for (int tagIndex = 0; tagIndex < tags.Length && markerIndex < tagMarkers.Length; tagIndex++)
        {
            Sprite icon = FindTagIcon(tags[tagIndex]);
            if (icon == null || IsTagIconAlreadyShown(icon, markerIndex))
            {
                continue;
            }

            Image marker = tagMarkers[markerIndex];
            marker.sprite = icon;
            marker.enabled = true;
            marker.raycastTarget = true;
            EnsureTagTooltipTrigger(marker).Configure(this, tags[tagIndex]);
            marker.gameObject.SetActive(true);
            markerIndex++;
        }
    }

    /// <summary>Hides all populated tag icons without changing the supplied empty sockets in the card artwork.</summary>
    private void HideTagIcons()
    {
        if (tagMarkers == null)
        {
            return;
        }

        for (int i = 0; i < tagMarkers.Length; i++)
        {
            if (tagMarkers[i] != null)
            {
                CardTagTooltipTrigger trigger = tagMarkers[i].GetComponent<CardTagTooltipTrigger>();
                if (trigger != null)
                {
                    trigger.Configure(this, string.Empty);
                }

                tagMarkers[i].enabled = false;
                tagMarkers[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Finds an icon mapping for an authored tag without requiring capitalization to match.</summary>
    private Sprite FindTagIcon(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        for (int i = 0; i < tagIconDefinitions.Length; i++)
        {
            CardTagIconDefinition definition = tagIconDefinitions[i];
            if (definition != null
                && definition.Icon != null
                && string.Equals(definition.Tag, tag, StringComparison.OrdinalIgnoreCase))
            {
                return definition.Icon;
            }
        }

        return null;
    }

    /// <summary>Prevents duplicate tag artwork from occupying multiple rail sockets.</summary>
    private bool IsTagIconAlreadyShown(Sprite icon, int populatedCount)
    {
        for (int i = 0; i < populatedCount; i++)
        {
            if (tagMarkers[i] != null && tagMarkers[i].sprite == icon)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Creates the four runtime markers for older prefabs that predate the supplied tag rail.</summary>
    private void EnsureTagMarkers()
    {
        bool hasCompleteMarkerSet = tagMarkers != null && tagMarkers.Length == TagMarkerCount;
        if (hasCompleteMarkerSet)
        {
            for (int i = 0; i < tagMarkers.Length; i++)
            {
                if (tagMarkers[i] == null)
                {
                    hasCompleteMarkerSet = false;
                    break;
                }
            }
        }

        if (hasCompleteMarkerSet)
        {
            return;
        }

        if (tagIconRoot == null)
        {
            GameObject rootObject = new GameObject("Tag Icons", typeof(RectTransform));
            rootObject.layer = gameObject.layer;
            tagIconRoot = rootObject.GetComponent<RectTransform>();
            tagIconRoot.SetParent(transform, false);
            tagIconRoot.anchorMin = Vector2.zero;
            tagIconRoot.anchorMax = Vector2.one;
            tagIconRoot.offsetMin = Vector2.zero;
            tagIconRoot.offsetMax = Vector2.zero;
            if (cardFrame != null)
            {
                tagIconRoot.SetSiblingIndex(cardFrame.transform.GetSiblingIndex());
            }
        }

        tagMarkers = new Image[TagMarkerCount];
        for (int i = 0; i < tagMarkers.Length; i++)
        {
            GameObject markerObject = new GameObject($"Tag {i + 1:00}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            markerObject.layer = gameObject.layer;
            RectTransform markerRect = markerObject.GetComponent<RectTransform>();
            markerRect.SetParent(tagIconRoot, false);
            markerRect.anchorMin = new Vector2(0.910f, TagMarkerVerticalAnchors[i]);
            markerRect.anchorMax = markerRect.anchorMin;
            markerRect.anchoredPosition = Vector2.zero;
            markerRect.sizeDelta = new Vector2(32f, 32f);

            Image marker = markerObject.GetComponent<Image>();
            marker.preserveAspect = true;
            marker.raycastTarget = true;
            EnsureTagTooltipTrigger(marker);
            tagMarkers[i] = marker;
        }
    }

    /// <summary>Keeps the differently sized catch and event masters at their native aspect ratios.</summary>
    private void ApplyReferenceAspectRatio(bool usesCatchLayout)
    {
        AspectRatioFitter fitter = GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = usesCatchLayout ? ReferenceAspectRatio : EventReferenceAspectRatio;
        }
    }

    /// <summary>Shows a non-interactive tag-name tooltip on the run's ordered tooltip canvas.</summary>
    public void ShowTagTooltip(
        string tagName,
        RectTransform source,
        CardTagTooltipTrigger sourceTrigger)
    {
        if (string.IsNullOrWhiteSpace(tagName) || source == null)
        {
            return;
        }

        EnsureTagTooltip();
        if (tagTooltipRoot == null || tagTooltipText == null)
        {
            return;
        }

        activeTagTooltipTrigger = sourceTrigger;
        tagTooltipText.text = tagName.Trim().ToUpperInvariant();
        tagTooltipText.ForceMeshUpdate();
        float tooltipWidth = Mathf.Max(
            TagTooltipMinimumWidth,
            tagTooltipText.preferredWidth + TagTooltipHorizontalPadding);
        tagTooltipRoot.sizeDelta = new Vector2(tooltipWidth, TagTooltipHeight);
        PositionTagTooltip(source);
        tagTooltipRoot.SetAsLastSibling();
        tagTooltipRoot.gameObject.SetActive(true);
    }

    /// <summary>Hides the tag tooltip only when its currently hovered icon requests it.</summary>
    public void HideTagTooltip(CardTagTooltipTrigger sourceTrigger)
    {
        if (activeTagTooltipTrigger != sourceTrigger)
        {
            return;
        }

        activeTagTooltipTrigger = null;
        if (tagTooltipRoot != null)
        {
            tagTooltipRoot.gameObject.SetActive(false);
        }
    }

    /// <summary>Adds or returns the pointer-event bridge for a tag marker.</summary>
    private CardTagTooltipTrigger EnsureTagTooltipTrigger(Image marker)
    {
        CanvasGroup hoverGroup = marker.GetComponent<CanvasGroup>();
        if (hoverGroup == null)
        {
            hoverGroup = marker.gameObject.AddComponent<CanvasGroup>();
        }

        // Encounter motion roots intentionally reject input during presentation. Tag icons opt out
        // so their hover-only affordance remains available without making the card interactive.
        hoverGroup.interactable = true;
        hoverGroup.blocksRaycasts = true;
        hoverGroup.ignoreParentGroups = true;

        CardTagTooltipTrigger trigger = marker.GetComponent<CardTagTooltipTrigger>();
        if (trigger == null)
        {
            trigger = marker.gameObject.AddComponent<CardTagTooltipTrigger>();
        }

        return trigger;
    }

    /// <summary>Creates the shared tooltip panel beneath the run's dedicated tooltip canvas.</summary>
    private void EnsureTagTooltip()
    {
        if (tagTooltipRoot != null)
        {
            return;
        }

        FishingRunView runView = GetComponentInParent<FishingRunView>();
        Canvas tooltipCanvas = runView != null ? runView.TooltipLayer : null;
        RectTransform tooltipParent = tooltipCanvas != null
            ? tooltipCanvas.transform as RectTransform
            : GetComponentInParent<Canvas>()?.transform as RectTransform;
        if (tooltipParent == null)
        {
            return;
        }

        GameObject tooltipObject = new GameObject(
            "Tag Tooltip",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline));
        tooltipObject.layer = gameObject.layer;
        tagTooltipRoot = tooltipObject.GetComponent<RectTransform>();
        tagTooltipRoot.SetParent(tooltipParent, false);
        tagTooltipRoot.anchorMin = new Vector2(0.5f, 0.5f);
        tagTooltipRoot.anchorMax = tagTooltipRoot.anchorMin;
        tagTooltipRoot.pivot = new Vector2(0.5f, 0.5f);
        tagTooltipRoot.sizeDelta = new Vector2(TagTooltipMinimumWidth, TagTooltipHeight);

        Image background = tooltipObject.GetComponent<Image>();
        background.color = TagTooltipBackgroundColor;
        background.raycastTarget = false;

        Outline border = tooltipObject.GetComponent<Outline>();
        border.effectColor = TagTooltipBorderColor;
        border.effectDistance = new Vector2(1f, -1f);
        border.useGraphicAlpha = false;

        GameObject textObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(tagTooltipRoot, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 3f);
        textRect.offsetMax = new Vector2(-8f, -3f);

        tagTooltipText = textObject.GetComponent<TextMeshProUGUI>();
        tagTooltipText.alignment = TextAlignmentOptions.Center;
        tagTooltipText.color = TagTooltipTextColor;
        tagTooltipText.fontSize = 16f;
        tagTooltipText.fontStyle = FontStyles.Bold;
        tagTooltipText.enableAutoSizing = false;
        tagTooltipText.raycastTarget = false;
        if (effectText != null)
        {
            tagTooltipText.font = effectText.font;
        }

        tooltipObject.SetActive(false);
    }

    /// <summary>Positions the tooltip to the icon's right, falling back left at the screen edge.</summary>
    private void PositionTagTooltip(RectTransform source)
    {
        RectTransform parent = tagTooltipRoot.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        Canvas sourceCanvas = source.GetComponentInParent<Canvas>();
        Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? sourceCanvas.worldCamera
            : null;
        Canvas tooltipCanvas = parent.GetComponentInParent<Canvas>();
        Camera tooltipCamera = tooltipCanvas != null && tooltipCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? tooltipCanvas.worldCamera
            : null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(sourceCamera, source.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screenPoint,
                tooltipCamera,
                out Vector2 localPoint))
        {
            return;
        }

        RectTransform cardRect = transform as RectTransform;
        if (cardRect == null)
        {
            return;
        }

        Vector3[] cardCorners = new Vector3[4];
        cardRect.GetWorldCorners(cardCorners);
        Vector3 rightEdgeWorld = (cardCorners[2] + cardCorners[3]) * 0.5f;
        Vector3 leftEdgeWorld = (cardCorners[0] + cardCorners[1]) * 0.5f;
        Vector2 rightEdgeScreen = RectTransformUtility.WorldToScreenPoint(sourceCamera, rightEdgeWorld);
        Vector2 leftEdgeScreen = RectTransformUtility.WorldToScreenPoint(sourceCamera, leftEdgeWorld);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                rightEdgeScreen,
                tooltipCamera,
                out Vector2 rightEdgeLocal)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                leftEdgeScreen,
                tooltipCamera,
                out Vector2 leftEdgeLocal))
        {
            return;
        }

        Rect bounds = parent.rect;
        float halfWidth = tagTooltipRoot.rect.width * 0.5f;
        float halfHeight = tagTooltipRoot.rect.height * 0.5f;
        Vector2 position = new Vector2(
            rightEdgeLocal.x + TagTooltipCardGap + halfWidth,
            localPoint.y);
        if (position.x + halfWidth > bounds.xMax)
        {
            position.x = leftEdgeLocal.x - TagTooltipCardGap - halfWidth;
        }

        position.x = Mathf.Clamp(position.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
        position.y = Mathf.Clamp(
            position.y,
            bounds.yMin + halfHeight,
            bounds.yMax - halfHeight);
        tagTooltipRoot.anchoredPosition = position;
    }

    private void ClearFields()
    {
        SetBlank();
    }

    /// <summary>Removes the externally parented tooltip when this card view is destroyed.</summary>
    private void OnDestroy()
    {
        if (tagTooltipRoot != null)
        {
            Destroy(tagTooltipRoot.gameObject);
        }
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    /// <summary>Applies a card-authored transform only to the central full-card artwork overlay.</summary>
    private void ApplyEncounterArtworkTransform(float scale, float rotation, Vector2 offset)
    {
        if (artworkOverflowLayer == null)
        {
            return;
        }

        RectTransform artworkTransform = artworkOverflowLayer.rectTransform;
        float safeScale = Mathf.Max(0.1f, scale);
        artworkTransform.localScale = new Vector3(safeScale, safeScale, 1f);
        artworkTransform.localEulerAngles = new Vector3(0f, 0f, rotation);
        artworkTransform.anchoredPosition = offset;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetParentActive(TMP_Text text, bool active)
    {
        if (text != null && text.transform.parent != null)
        {
            text.transform.parent.gameObject.SetActive(active);
        }
    }

    private static bool UsesCatchCardLayout(CardType cardType)
    {
        return cardType == CardType.Creature
            || cardType == CardType.Treasure
            || cardType == CardType.ApexEncounter;
    }

    private static string BuildCardTypeText(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.ApexEncounter:
                return "APEX CATCH";
            case CardType.Creature:
            case CardType.Treasure:
                return "CATCH CARD";
            default:
                return "EVENT CARD";
        }
    }
}

[Serializable]
public sealed class CardTagIconDefinition
{
    [SerializeField] private string tag;
    [SerializeField] private Sprite icon;

    public string Tag => tag;
    public Sprite Icon => icon;
}
