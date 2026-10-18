using UnityEngine.EventSystems;

/// <summary>Rejects a pointer release when its hand binding changed after the pointer was pressed.</summary>
public sealed class JournalTechniqueButton : UnityEngine.UI.Button
{
    private long binding, pressedBinding;
    private bool pressed;
    /// <summary>Records the current hand generation without changing gameplay state.</summary>
    public void Bind(long generation) { binding = generation; }
    /// <summary>Associates a physical pointer press with the hand that was visible at press time.</summary>
    public override void OnPointerDown(PointerEventData eventData)
    { pressed = true; pressedBinding = binding; base.OnPointerDown(eventData); }
    /// <summary>Forwards the click only when the pressed binding remains current.</summary>
    public override void OnPointerClick(PointerEventData eventData)
    {
        bool stale = pressed && pressedBinding != binding; pressed = false;
        if (!stale) base.OnPointerClick(eventData);
    }
    /// <summary>Navigation submission always addresses the currently focused binding.</summary>
    public override void OnSubmit(BaseEventData eventData) { pressed = false; base.OnSubmit(eventData); }
}
