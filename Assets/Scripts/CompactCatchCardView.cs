using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CompactCatchCardState
{
    Normal,
    Selected,
    Disabled,
    ReleaseCandidate
}

/// <summary>Displays one catch in the rig without reproducing the full creature rules face.</summary>
public sealed class CompactCatchCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ICreatureStatInspectionSource
{
    private static readonly Color TitleColor = new Color(0.937f, 0.886f, 0.761f, 1f);
    private static readonly Color StatColor = new Color(0.082f, 0.239f, 0.239f, 1f);

    [Header("Content")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image passiveEffectIcon;
    [SerializeField] private GameObject baitMarker;
    [SerializeField] private RectTransform rigAttachmentPoint;
    [SerializeField] private Color titleInk = TitleColor;
    [SerializeField] private Color statInk = StatColor;
    [SerializeField] private bool uppercaseName = true;
    [SerializeField] private TextAlignmentOptions titleAlignment = TextAlignmentOptions.Center;
    [SerializeField] private Graphic baitFrame;

    [Header("Interaction")]
    [SerializeField] private Button selectionButton;
    [SerializeField] private Graphic hoverFrame;
    [SerializeField] private bool pointerHoverFrame = true;
    [SerializeField] private Graphic selectedFrame;
    [SerializeField] private Graphic disabledOverlay;
    [SerializeField] private Graphic releaseCandidateFrame;
    [SerializeField] private CompactCatchCardState state;

    private Action selectionAction;
    private bool pointerInside;
    private bool focused;
    private CreatureCardStatAnimator statAnimator;
    private bool statFeedbackEnabled;
    private CardInstance boundInstance;
    public CardInstance BoundInstance => boundInstance;
    /// <summary>Reads the same resolved instance used by the compact stat animation, respecting hidden sale values.</summary>
    public bool TryGetStatBreakdown(CreatureStat stat, out CardStatBreakdown breakdown)
    {
        breakdown = null;
        if (boundInstance?.Definition == null || stat == CreatureStat.Value && (boundInstance.IsBait || boundInstance.HidesOwnValueDuringRun)) return false;
        breakdown = new CardStatBreakdown(boundInstance, stat, boundInstance.InstanceId);
        return breakdown.Current != breakdown.Baseline && breakdown.Reasons.Length > 0;
    }
    public bool HasPendingStatFeedback => statAnimator != null && statAnimator.IsAnimating;

    /// <summary>Opts a pooled compact card into the same number animation used by the central creature.</summary>
    public void EnableStatFeedback()
    {
        statFeedbackEnabled = true;
        if (statAnimator != null) return;
        statAnimator = GetComponent<CreatureCardStatAnimator>();
        if (statAnimator == null) statAnimator = gameObject.AddComponent<CreatureCardStatAnimator>();
        statAnimator.BindTextFields(weightText, valueText);
    }

    /// <summary>Prevents ended-run snapshots from replaying effects as newly attached creatures.</summary>
    public void DisableStatFeedback()
    {
        statFeedbackEnabled = false;
        if (statAnimator != null) statAnimator.ResetPresentation();
    }

    /// <summary>Lets the chain coordinator serialize animations while preserving queued numbers.</summary>
    public void SetStatFeedbackPaused(bool paused) { if (statAnimator != null) statAnimator.SetPaused(paused, true); }

    /// <summary>Settles queued feedback immediately when manual inspection takes over scrolling.</summary>
    public void CompleteStatFeedback()
    {
        if (statAnimator == null) return;
        statAnimator.Complete();
    }

    public RectTransform RigAttachmentPoint => rigAttachmentPoint;
    public CompactCatchCardState State => state;

    /// <summary>Registers the physical card's selection callback once.</summary>
    private void Awake()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.AddListener(InvokeSelection);
        }

        ApplyState();
    }

    /// <summary>Unregisters the callback when this card view is disposed.</summary>
    private void OnDestroy()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.RemoveListener(InvokeSelection);
        }
    }

    /// <summary>Populates the compact card from one resolved catch instance.</summary>
    public void SetCard(CardInstance caughtInstance, Sprite optionalPassiveIcon = null)
    {
        if (statAnimator != null)
        {
            if (!ReferenceEquals(boundInstance, caughtInstance)) statAnimator.ResetPresentation();
            else statAnimator.PrepareForRefresh();
        }
        boundInstance = caughtInstance;
        CardDefinition definition = caughtInstance?.Definition;
        SetContent(
            definition == null ? string.Empty : uppercaseName ? definition.DisplayName.ToUpperInvariant() : definition.DisplayName,
            definition == null ? null : definition.Artwork,
            caughtInstance == null ? string.Empty : caughtInstance.CurrentWeight.ToString(),
            caughtInstance == null
                ? string.Empty
                : caughtInstance.IsBait
                    ? "0"
                    : caughtInstance.HidesOwnValueDuringRun ? "?" : caughtInstance.HaulValueContribution.ToString(),
            optionalPassiveIcon,
            caughtInstance?.IsBait == true);
        if (statFeedbackEnabled && statAnimator != null && definition != null)
            statAnimator.Present(definition, new EncounterStatFeedback(caughtInstance.InstanceId,
                definition.Weight, caughtInstance.IsBait ? 0 : caughtInstance.BaseValue, caughtInstance.IsBait),
                caughtInstance.CurrentWeight, caughtInstance.HaulValueContribution, false,
                !caughtInstance.IsBait && caughtInstance.HidesOwnValueDuringRun);
    }

    /// <summary>Populates direct values for editor previews and presentation tests.</summary>
    public void SetPreview(
        string displayName,
        Sprite portrait,
        int weight,
        int value,
        Sprite optionalPassiveIcon,
        bool isBait = false)
    {
        if (statAnimator != null) statAnimator.ResetPresentation();
        boundInstance = null;
        SetContent(displayName, portrait, weight.ToString(), isBait ? "0" : value.ToString(), optionalPassiveIcon, isBait);
    }

    /// <summary>Assigns the controller-owned catch-selection command.</summary>
    public void SetSelectionHandler(Action action)
    {
        selectionAction = action;
    }

    /// <summary>Overrides only this view's portrait and framing, including an explicit empty missing-art slot.</summary>
    public void SetArtwork(Sprite artwork, Rect normalizedBounds)
    {
        SetImage(portraitImage, artwork);
        if (portraitImage == null) return;
        RectTransform region = portraitImage.transform.parent as RectTransform;
        if (region == null) return;
        region.anchorMin = normalizedBounds.min;
        region.anchorMax = normalizedBounds.max;
        region.offsetMin = region.offsetMax = Vector2.zero;
    }

    /// <summary>Changes interaction feedback while preserving card content and geometry.</summary>
    public void SetState(CompactCatchCardState newState)
    {
        state = newState;
        ApplyState();
    }

    /// <summary>Shows pointer feedback outside the card's content fields.</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        ApplyState();
    }

    /// <summary>Clears pointer feedback without clearing keyboard focus.</summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        ApplyState();
    }

    /// <summary>Shows the same focus outline for keyboard or controller selection.</summary>
    public void OnSelect(BaseEventData eventData) { focused = pointerHoverFrame || (!(eventData is PointerEventData) && !pointerInside); ApplyState(); }

    /// <summary>Clears navigation focus without clearing pointer hover.</summary>
    public void OnDeselect(BaseEventData eventData) { focused = false; ApplyState(); }

    /// <summary>Clears transient input state when a pooled card is hidden.</summary>
    private void OnDisable() { pointerInside = false; focused = false; ApplyState(); }

    /// <summary>Assigns independent live fields and role styling.</summary>
    private void SetContent(
        string displayName,
        Sprite portrait,
        string weight,
        string value,
        Sprite passiveIcon,
        bool isBait)
    {
        SetText(nameText, displayName);
        SetText(weightText, weight);
        SetText(valueText, value);
        ConfigureTextPresentation();
        SetImage(portraitImage, portrait);
        SetImage(passiveEffectIcon, passiveIcon);
        EnsureBaitMarker();
        baitMarker.SetActive(isBait);
        SetVisible(baitFrame, isBait);
    }

    /// <summary>Creates a compact role badge for prefabs authored before Bait attachments existed.</summary>
    private void EnsureBaitMarker()
    {
        if (baitMarker != null)
        {
            return;
        }

        baitMarker = new GameObject(
            "Bait Marker",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline));
        baitMarker.layer = gameObject.layer;
        RectTransform markerRect = baitMarker.GetComponent<RectTransform>();
        markerRect.SetParent(transform, false);
        markerRect.anchorMin = new Vector2(0.065f, 0.60f);
        markerRect.anchorMax = new Vector2(0.235f, 0.71f);
        markerRect.offsetMin = Vector2.zero;
        markerRect.offsetMax = Vector2.zero;

        Image markerBackground = baitMarker.GetComponent<Image>();
        markerBackground.color = new Color32(13, 57, 58, 238);
        markerBackground.raycastTarget = false;

        Outline markerOutline = baitMarker.GetComponent<Outline>();
        markerOutline.effectColor = new Color32(207, 163, 72, 255);
        markerOutline.effectDistance = new Vector2(1.5f, -1.5f);
        markerOutline.useGraphicAlpha = false;

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.layer = gameObject.layer;
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(markerRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(3f, 1f);
        labelRect.offsetMax = new Vector2(-3f, -1f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "BAIT";
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color32(239, 226, 194, 255);
        label.fontSize = 20f;
        label.fontWeight = FontWeight.Bold;
        label.raycastTarget = false;
        if (nameText != null)
        {
            label.font = nameText.font;
        }

        if (hoverFrame != null)
        {
            markerRect.SetSiblingIndex(hoverFrame.transform.GetSiblingIndex());
        }
    }

    /// <summary>Aligns runtime text with the master card's authored title and stat regions.</summary>
    private void ConfigureTextPresentation()
    {
        if (nameText != null)
        {
            nameText.alignment = titleAlignment;
            nameText.color = titleInk;
        }

        if (weightText != null)
        {
            weightText.alignment = TextAlignmentOptions.Center;
            weightText.color = statInk;
        }

        if (valueText != null)
        {
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = statInk;
        }
    }

    /// <summary>Updates only independent feedback overlays and selection availability.</summary>
    private void ApplyState()
    {
        bool disabled = state == CompactCatchCardState.Disabled;
        if (selectionButton != null)
        {
            selectionButton.interactable = !disabled;
        }

        SetVisible(hoverFrame, !disabled && (pointerHoverFrame ? pointerInside || focused : focused && !pointerInside));
        SetVisible(selectedFrame, state == CompactCatchCardState.Selected);
        SetVisible(disabledOverlay, disabled);
        SetVisible(releaseCandidateFrame, state == CompactCatchCardState.ReleaseCandidate);
    }

    /// <summary>Forwards enabled input; the controller remains authoritative about action legality.</summary>
    private void InvokeSelection()
    {
        if (state != CompactCatchCardState.Disabled) selectionAction?.Invoke();
    }

    /// <summary>Assigns text without retaining an earlier pooled value.</summary>
    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    /// <summary>Hides missing art rather than retaining another card's portrait.</summary>
    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    /// <summary>Toggles an independent presentation layer.</summary>
    private static void SetVisible(Graphic image, bool visible)
    {
        if (image != null)
        {
            image.gameObject.SetActive(visible);
        }
    }
}
