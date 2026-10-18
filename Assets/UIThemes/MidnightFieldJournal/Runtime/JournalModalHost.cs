using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Owns modal input/focus separately from gameplay legality and controller resolution.</summary>
public sealed class JournalModalHost : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] gameplayGroups;
    [SerializeField] private UnityEngine.UI.Button resumeFocus;
    private GameObject current, previousFocus;
    public bool IsOpen => current != null && current.activeSelf;

    /// <summary>Shows one modal, preempting an informational panel while keeping gameplay input locked.</summary>
    public void Show(GameObject modal)
    {
        if (!IsOpen && EventSystem.current != null) previousFocus = EventSystem.current.currentSelectedGameObject;
        if (current != null && current != modal) current.SetActive(false);
        current = modal; current.SetActive(true); current.transform.SetAsLastSibling(); SetLocked(true);
    }
    /// <summary>Restores focus only when the dismissed modal still owns the input layer.</summary>
    public void Hide(GameObject modal)
    {
        if (modal != null) modal.SetActive(false);
        if (current != modal) return;
        current = null; SetLocked(false);
        if (EventSystem.current == null) return;
        var old = previousFocus == null ? null : previousFocus.GetComponent<UnityEngine.UI.Selectable>();
        if (old != null && old.isActiveAndEnabled && old.IsInteractable()) old.Select();
        else if (resumeFocus != null && resumeFocus.IsInteractable() && resumeFocus.isActiveAndEnabled) resumeFocus.Select();
        else EventSystem.current.SetSelectedGameObject(null);
        previousFocus = null;
    }
    /// <summary>Locks flat controls and the projected hand's parent groups without changing command state.</summary>
    private void SetLocked(bool locked)
    {
        foreach (CanvasGroup group in gameplayGroups)
        { if (group == null) continue; bool enabled = !locked && group.alpha > 0; group.interactable = enabled; group.blocksRaycasts = enabled; }
    }
    /// <summary>Removes ownership when the whole UI is unloaded.</summary>
    private void OnDisable() { current = null; previousFocus = null; }
}
