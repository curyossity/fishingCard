using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Moves only the visual face on press and shows independent focus corners for pointer or keyboard input.</summary>
[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class TackleButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform visual;
    [SerializeField] private GameObject focusCorners;
    private UnityEngine.UI.Button button;
    private Vector2 origin;
    private bool hovered, selected;
    private CanvasGroup visualGroup;

    /// <summary>Caches the input surface and its resting visual offset once.</summary>
    private void Awake() { button = GetComponent<UnityEngine.UI.Button>(); visualGroup = visual.GetComponent<CanvasGroup>(); origin = visual.anchoredPosition; }
    /// <summary>Applies disabled styling to artwork and text together when the authoritative command availability changes.</summary>
    public void RefreshAvailability()
    {
        if (button == null) button = GetComponent<UnityEngine.UI.Button>();
        if (visualGroup == null) visualGroup = visual.GetComponent<CanvasGroup>();
        visualGroup.alpha = button.IsInteractable() ? 1 : .4f;
        if (!button.IsInteractable()) visual.anchoredPosition = origin;
        RefreshFocus();
    }
    /// <summary>Highlights an available pointer target.</summary>
    public void OnPointerEnter(PointerEventData data) { hovered = true; RefreshFocus(); }
    /// <summary>Clears pointer-only focus when leaving the rectangular hit area.</summary>
    public void OnPointerExit(PointerEventData data) { hovered = false; RefreshFocus(); }
    /// <summary>Offsets the visual face by the approved four pixels while preserving its hit area.</summary>
    public void OnPointerDown(PointerEventData data) { if (button.IsInteractable()) visual.anchoredPosition = origin + Vector2.down * 4; }
    /// <summary>Restores the resting position on release.</summary>
    public void OnPointerUp(PointerEventData data) { visual.anchoredPosition = origin; }
    /// <summary>Displays keyboard/gamepad focus independently of hover.</summary>
    public void OnSelect(BaseEventData data) { selected = true; RefreshFocus(); }
    /// <summary>Releases keyboard/gamepad focus without clearing hover.</summary>
    public void OnDeselect(BaseEventData data) { selected = false; RefreshFocus(); }
    /// <summary>Resets motion and focus on phase exit.</summary>
    private void OnDisable() { hovered = selected = false; if (visual != null) visual.anchoredPosition = origin; if (focusCorners != null) focusCorners.SetActive(false); }
    /// <summary>Shows focus only for an interactable command.</summary>
    private void RefreshFocus() { focusCorners.SetActive(button != null && button.IsInteractable() && (hovered || selected)); }
}
