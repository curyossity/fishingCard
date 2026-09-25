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
    [Header("Content")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image passiveEffectIcon;
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
            definition == null ? string.Empty : definition.DisplayName,
            definition == null ? null : definition.Artwork,
            caughtInstance == null ? string.Empty : caughtInstance.CurrentWeight.ToString(),
            caughtInstance == null ? string.Empty : caughtInstance.CurrentValue.ToString(),
            optionalPassiveIcon);
    }

    /// <summary>Populates direct values for editor previews and presentation tests.</summary>
    public void SetPreview(string displayName, Sprite portrait, int weight, int value, Sprite optionalPassiveIcon)
    {
        SetContent(displayName, portrait, weight.ToString(), value.ToString(), optionalPassiveIcon);
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

    private void SetContent(string displayName, Sprite portrait, string weight, string value, Sprite passiveIcon)
    {
        SetText(nameText, displayName);
        SetText(weightText, weight);
        SetText(valueText, value);
        SetImage(portraitImage, portrait);
        SetImage(passiveEffectIcon, passiveIcon);
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
