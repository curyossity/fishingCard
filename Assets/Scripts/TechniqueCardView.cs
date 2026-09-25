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
public sealed class TechniqueCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Stable Visual Root")]
    [SerializeField] private RectTransform visualRoot;

    [Header("Content")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rulesText;
    [SerializeField] private TMP_Text keywordText;

    [Header("Interaction")]
    [SerializeField] private Button useButton;
    [SerializeField] private Image hoverFrame;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private Image disabledOverlay;
    [SerializeField] private GameObject playableBadge;
    [SerializeField] private GameObject lockedIndicator;
    [SerializeField] private TechniqueCardVisualState state;

    private Action useAction;
    private bool pointerInside;
    private bool hasCard;

    public TechniqueCardVisualState State => state;

    private void Awake()
    {
        if (useButton != null)
        {
            useButton.onClick.AddListener(InvokeUse);
        }

        ApplyState();
    }

    private void OnDestroy()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(InvokeUse);
        }
    }

    /// <summary>Populates a technique from its immutable card definition.</summary>
    public void SetCard(CardDefinition card, string keyword, bool playable)
    {
        hasCard = card != null;
        SetContent(
            card == null ? string.Empty : card.DisplayName,
            card == null ? null : card.Artwork,
            card == null ? string.Empty : card.RulesText,
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

    /// <summary>Applies one authored visual state without changing the slot geometry.</summary>
    public void SetState(TechniqueCardVisualState newState)
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

    private void ApplyState()
    {
        bool disabled = state == TechniqueCardVisualState.Disabled;
        bool hovered = !disabled && (pointerInside || state == TechniqueCardVisualState.Hovered);
        SetVisible(hoverFrame, hovered);
        SetVisible(selectedFrame, state == TechniqueCardVisualState.Selected);
        SetVisible(disabledOverlay, disabled);
        SetVisible(playableBadge, state == TechniqueCardVisualState.Playable);
        SetVisible(lockedIndicator, disabled);

        if (useButton != null)
        {
            useButton.interactable = hasCard && !disabled;
        }

        if (visualRoot != null)
        {
            visualRoot.localScale = hovered ? Vector3.one * 1.04f : Vector3.one;
            visualRoot.anchoredPosition = hovered ? new Vector2(0f, 14f) : Vector2.zero;
        }
    }

    private void InvokeUse()
    {
        useAction?.Invoke();
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetVisible(Image image, bool visible)
    {
        if (image != null)
        {
            image.gameObject.SetActive(visible);
        }
    }

    private static void SetVisible(GameObject target, bool visible)
    {
        if (target != null)
        {
            target.SetActive(visible);
        }
    }
}
