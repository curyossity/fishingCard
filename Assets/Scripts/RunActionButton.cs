using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RunActionTheme
{
    Descend,
    Release,
    Surface
}

public enum RunActionButtonVisualState
{
    Normal,
    Hover,
    Pressed,
    Disabled,
    Dangerous
}

/// <summary>Owns consistent maritime action-button presentation and command forwarding.</summary>
public sealed class RunActionButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler
{
    [Header("Structure")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image focusFrame;

    [Header("State Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField] private Sprite dangerousSprite;

    [Header("Theme Icons")]
    [SerializeField] private Sprite descendIcon;
    [SerializeField] private Sprite releaseIcon;
    [SerializeField] private Sprite surfaceIcon;
    [SerializeField] private RunActionTheme theme;
    [SerializeField] private bool dangerous;

    private Action action;
    private bool pointerInside;
    private bool pointerDown;
    private bool selected;

    public RunActionTheme Theme => theme;
    public bool Interactable => button != null && button.interactable;
    public RunActionButtonVisualState VisualState => ResolveVisualState();

    private void Awake()
    {
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(InvokeAction);
        }

        ApplyTheme();
        ApplyVisualState();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(InvokeAction);
        }
    }

    /// <summary>Assigns the action family and its standard icon and command label.</summary>
    public void SetTheme(RunActionTheme newTheme)
    {
        theme = newTheme;
        dangerous = newTheme == RunActionTheme.Release;
        ApplyTheme();
        ApplyVisualState();
    }

    /// <summary>Overrides the standard command label while retaining theme geometry.</summary>
    public void SetLabel(string text)
    {
        if (label != null)
        {
            label.text = text ?? string.Empty;
        }
    }

    /// <summary>Changes availability without reducing label legibility.</summary>
    public void SetInteractable(bool value)
    {
        if (button != null)
        {
            button.interactable = value;
        }

        pointerDown = false;
        ApplyVisualState();
    }

    /// <summary>Applies or removes the destructive-action treatment independently of theme.</summary>
    public void SetDangerous(bool value)
    {
        dangerous = value;
        ApplyVisualState();
    }

    /// <summary>Assigns the controller-owned gameplay command.</summary>
    public void SetAction(Action command)
    {
        action = command;
    }

    /// <summary>Exercises an authored state without resizing the button during visual validation.</summary>
    public void SetPreviewState(RunActionButtonVisualState previewState)
    {
        SetInteractable(previewState != RunActionButtonVisualState.Disabled);
        pointerInside = previewState == RunActionButtonVisualState.Hover;
        pointerDown = previewState == RunActionButtonVisualState.Pressed;
        dangerous = previewState == RunActionButtonVisualState.Dangerous;
        ApplyVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        ApplyVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        pointerDown = false;
        ApplyVisualState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Interactable)
        {
            pointerDown = true;
            ApplyVisualState();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        ApplyVisualState();
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        ApplyVisualState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        ApplyVisualState();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (Interactable)
        {
            StartCoroutine(ShowSubmitPress());
        }
    }

    private IEnumerator ShowSubmitPress()
    {
        pointerDown = true;
        ApplyVisualState();
        yield return null;
        pointerDown = false;
        ApplyVisualState();
    }

    private void ApplyTheme()
    {
        if (icon != null)
        {
            icon.sprite = theme switch
            {
                RunActionTheme.Release => releaseIcon,
                RunActionTheme.Surface => surfaceIcon,
                _ => descendIcon
            };
            icon.enabled = icon.sprite != null;
        }

        SetLabel(theme switch
        {
            RunActionTheme.Release => "RELEASE",
            RunActionTheme.Surface => "SURFACE",
            _ => "DESCEND"
        });
    }

    private void ApplyVisualState()
    {
        if (background != null)
        {
            background.sprite = ResolveVisualState() switch
            {
                RunActionButtonVisualState.Hover => hoverSprite,
                RunActionButtonVisualState.Pressed => pressedSprite,
                RunActionButtonVisualState.Disabled => disabledSprite,
                RunActionButtonVisualState.Dangerous => dangerousSprite,
                _ => normalSprite
            };
            background.color = Color.white;
        }

        if (focusFrame != null)
        {
            focusFrame.gameObject.SetActive(selected);
        }
    }

    private RunActionButtonVisualState ResolveVisualState()
    {
        if (!Interactable)
        {
            return RunActionButtonVisualState.Disabled;
        }

        if (pointerDown)
        {
            return RunActionButtonVisualState.Pressed;
        }

        if (pointerInside)
        {
            return RunActionButtonVisualState.Hover;
        }

        return dangerous ? RunActionButtonVisualState.Dangerous : RunActionButtonVisualState.Normal;
    }

    private void InvokeAction()
    {
        action?.Invoke();
    }
}
