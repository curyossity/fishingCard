using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TechniqueCardVisualState
{
    Normal,
    Hovered,
    Selected,
    Playable,
    Disabled
}

/// <summary>Renders a technique card and forwards use intent without resolving card effects.</summary>
public sealed class TechniqueCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Stable Visual Root")]
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private bool externalPose;
    [SerializeField] private bool inspectWhileUnavailable;

    [Header("Content")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rulesText;
    [SerializeField] private TMP_Text keywordText;

    [Header("Interaction")]
    [SerializeField] private Button useButton;
    [SerializeField] private Graphic hoverFrame;
    [SerializeField] private bool pointerHoverFrame = true;
    [SerializeField] private Graphic selectedFrame;
    [SerializeField] private Graphic disabledOverlay;
    [SerializeField] private GameObject playableBadge;
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private TechniqueCardVisualState state;

    private Action useAction;
    private bool pointerInside;
    private bool hasCard;
    private bool focused;

    public TechniqueCardVisualState State => state;

    /// <summary>Registers the view's use listener once for this physical UI instance.</summary>
    private void Awake()
    {
        if (useButton != null)
        {
            useButton.onClick.AddListener(InvokeUse);
        }

        ApplyState();
    }

    /// <summary>Removes the listener when the view is disposed.</summary>
    private void OnDestroy()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(InvokeUse);
        }
    }

    /// <summary>Populates a technique from its immutable card definition.</summary>
    public void SetCard(CardDefinition card, string keyword, bool playable, string displayRules = null)
    {
        hasCard = card != null;
        SetContent(
            card == null ? string.Empty : card.DisplayName.ToUpperInvariant(),
            card == null ? null : card.Artwork,
            card == null ? string.Empty : displayRules ?? card.RulesText,
            keyword);
        SetState(playable ? TechniqueCardVisualState.Playable : TechniqueCardVisualState.Disabled);
    }

    /// <summary>Populates direct values for editor previews and presentation tests.</summary>
    public void SetPreview(string displayName, Sprite artwork, string rules, string keyword)
    {
        hasCard = true;
        SetContent(displayName, artwork, rules, keyword);
        ApplyState();
    }

    /// <summary>Assigns the controller-owned technique-use command.</summary>
    public void SetUseHandler(Action action)
    {
        useAction = action;
    }

    /// <summary>Uses an explicit theme portrait, including an empty slot for missing artwork.</summary>
    public void SetArtwork(Sprite artwork)
    {
        if (artworkImage == null) return;
        artworkImage.sprite = artwork; artworkImage.enabled = artwork != null;
    }

    /// <summary>Applies one authored visual state without changing the slot geometry.</summary>
    public void SetState(TechniqueCardVisualState newState)
    {
        state = newState;
        ApplyState();
    }

    /// <summary>Shows pointer hover feedback independently of the hand's projected pose.</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        ApplyState();
    }

    /// <summary>Clears pointer hover while retaining navigation focus.</summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        ApplyState();
    }

    /// <summary>Shows navigation focus with the same independent feedback layer.</summary>
    public void OnSelect(BaseEventData eventData) { focused = pointerHoverFrame || (!(eventData is PointerEventData) && !pointerInside); ApplyState(); }

    /// <summary>Clears keyboard/controller focus.</summary>
    public void OnDeselect(BaseEventData eventData) { focused = false; ApplyState(); }

    /// <summary>Restores transient presentation when a slot is hidden or its hand is disabled.</summary>
    private void OnDisable() { pointerInside = false; focused = false; ApplyState(); }

    /// <summary>Updates authored content without modifying its source card.</summary>
    private void SetContent(string displayName, Sprite artwork, string rules, string keyword)
    {
        SetText(nameText, displayName);
        SetText(rulesText, rules);
        SetText(keywordText, keyword);
        if (artworkImage != null)
        {
            artworkImage.sprite = artwork;
            artworkImage.enabled = artwork != null;
        }
    }

    /// <summary>Applies state layers; an external hand pose owns transforms when configured.</summary>
    private void ApplyState()
    {
        bool disabled = state == TechniqueCardVisualState.Disabled;
        bool hovered = !disabled && (pointerInside || focused || state == TechniqueCardVisualState.Hovered);
        SetVisible(hoverFrame, !disabled && (pointerHoverFrame ? hovered : focused && !pointerInside));
        SetVisible(selectedFrame, state == TechniqueCardVisualState.Selected);
        SetVisible(disabledOverlay, disabled);
        SetVisible(playableBadge, state == TechniqueCardVisualState.Playable);
        SetVisible(lockedIndicator, disabled);

        if (useButton != null)
        {
            useButton.interactable = hasCard && (!disabled || inspectWhileUnavailable);
        }

        if (visualRoot != null && !externalPose)
        {
            visualRoot.localScale = hovered ? Vector3.one * 1.04f : Vector3.one;
            visualRoot.anchoredPosition = hovered ? new Vector2(0f, 14f) : Vector2.zero;
        }
    }

    /// <summary>Forwards enabled use only; legal play remains a controller decision.</summary>
    private void InvokeUse()
    {
        if (hasCard && state != TechniqueCardVisualState.Disabled) useAction?.Invoke();
    }

    /// <summary>Assigns a live text field or clears it for an empty slot.</summary>
    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    /// <summary>Toggles one independent state graphic.</summary>
    private static void SetVisible(Graphic image, bool visible)
    {
        if (image != null)
        {
            image.gameObject.SetActive(visible);
        }
    }

    /// <summary>Toggles optional state indicators.</summary>
    private static void SetVisible(GameObject target, bool visible)
    {
        if (target != null)
        {
            target.SetActive(visible);
        }
    }
}
