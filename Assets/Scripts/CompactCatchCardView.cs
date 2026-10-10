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
public sealed class CompactCatchCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    [Header("Interaction")]
    [SerializeField] private Button selectionButton;
    [SerializeField] private Image hoverFrame;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private Image disabledOverlay;
    [SerializeField] private Image releaseCandidateFrame;
    [SerializeField] private CompactCatchCardState state;

    private Action selectionAction;
    private bool pointerInside;

    public RectTransform RigAttachmentPoint => rigAttachmentPoint;
    public CompactCatchCardState State => state;

    private void Awake()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.AddListener(InvokeSelection);
        }

        ApplyState();
    }

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
        CardDefinition definition = caughtInstance?.Definition;
        SetContent(
            definition == null ? string.Empty : definition.DisplayName.ToUpperInvariant(),
            definition == null ? null : definition.Artwork,
            caughtInstance == null ? string.Empty : caughtInstance.CurrentWeight.ToString(),
            caughtInstance == null
                ? string.Empty
                : caughtInstance.HidesOwnValueDuringRun ? "?" : caughtInstance.CurrentValue.ToString(),
            optionalPassiveIcon,
            caughtInstance?.IsBait == true);
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
        SetContent(displayName, portrait, weight.ToString(), value.ToString(), optionalPassiveIcon, isBait);
    }

    /// <summary>Assigns the controller-owned catch-selection command.</summary>
    public void SetSelectionHandler(Action action)
    {
        selectionAction = action;
    }

    /// <summary>Changes interaction feedback while preserving card content and geometry.</summary>
    public void SetState(CompactCatchCardState newState)
    {
        state = newState;
        ApplyState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        ApplyState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        ApplyState();
    }

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
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = TitleColor;
        }

        if (weightText != null)
        {
            weightText.alignment = TextAlignmentOptions.Center;
            weightText.color = StatColor;
        }

        if (valueText != null)
        {
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = StatColor;
        }
    }

    private void ApplyState()
    {
        bool disabled = state == CompactCatchCardState.Disabled;
        if (selectionButton != null)
        {
            selectionButton.interactable = !disabled;
        }

        SetVisible(hoverFrame, pointerInside && !disabled);
        SetVisible(selectedFrame, state == CompactCatchCardState.Selected);
        SetVisible(disabledOverlay, disabled);
        SetVisible(releaseCandidateFrame, state == CompactCatchCardState.ReleaseCandidate);
    }

    private void InvokeSelection()
    {
        selectionAction?.Invoke();
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
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

    private static void SetVisible(Image image, bool visible)
    {
        if (image != null)
        {
            image.gameObject.SetActive(visible);
        }
    }
}
