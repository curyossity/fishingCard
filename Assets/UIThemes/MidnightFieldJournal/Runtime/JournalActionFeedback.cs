using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Provides pointer and navigation focus feedback on the static action face without enlarging its hit rectangle.</summary>
public sealed class JournalActionFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform face;
    [SerializeField] private JournalCardOutline focus;
    private bool hovered, selected;
    private UnityEngine.UI.Button button;
    /// <summary>Caches the availability source used by visual feedback.</summary>
    private void Awake() { button = GetComponent<UnityEngine.UI.Button>(); }
    /// <summary>Highlights pointer focus.</summary>
    public void OnPointerEnter(PointerEventData data) { hovered = true; Refresh(); }
    /// <summary>Restores pointer state.</summary>
    public void OnPointerExit(PointerEventData data) { hovered = false; Refresh(); }
    /// <summary>Highlights controller or keyboard focus.</summary>
    public void OnSelect(BaseEventData data) { selected = !(data is PointerEventData) && !hovered; Refresh(); }
    /// <summary>Restores navigation state.</summary>
    public void OnDeselect(BaseEventData data) { selected = false; Refresh(); }
    /// <summary>Rechecks availability because commands may lock while the pointer remains still.</summary>
    private void LateUpdate() { Refresh(); }
    /// <summary>Clears visual state when the control is hidden.</summary>
    private void OnDisable() { hovered = selected = false; if (face != null) face.localScale = Vector3.one; if (focus != null) focus.enabled = false; }
    /// <summary>Keeps feedback separate from controller-owned command availability.</summary>
    private void Refresh()
    {
        if (button == null) button = GetComponent<UnityEngine.UI.Button>();
        bool active = (hovered || selected) && button != null && button.IsInteractable();
        if (face != null) face.localScale = Vector3.one * (active ? 1.018f : 1);
        if (focus != null) focus.enabled = selected && !hovered && button != null && button.IsInteractable();
    }
}
