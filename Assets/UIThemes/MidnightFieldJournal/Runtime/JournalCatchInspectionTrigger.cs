using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Relays hover and navigation inspection without changing compact-card selection or release behavior.</summary>
public sealed class JournalCatchInspectionTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private JournalCatchChainPresentation owner;
    private int index;
    private bool hovered, focused;
    public bool Interested => hovered || focused;

    /// <summary>Binds a pooled physical slot to its current controller-owned index.</summary>
    public void Configure(JournalCatchChainPresentation presentation, int slot) { owner = presentation; index = slot; }
    /// <summary>Requests delayed inspection for the pointer.</summary>
    public void OnPointerEnter(PointerEventData data) { hovered = true; owner?.Inspect(index, this, false); }
    /// <summary>Dismisses pointer ownership while respecting navigation focus.</summary>
    public void OnPointerExit(PointerEventData data) { hovered = false; if (!Interested) owner?.EndInspection(this); }
    /// <summary>Offers immediate inspection for keyboard/controller focus, not pointer-driven selection.</summary>
    public void OnSelect(BaseEventData data) { if (data is PointerEventData) return; focused = true; owner?.Inspect(index, this, true); }
    /// <summary>Dismisses navigation ownership independently of the pointer.</summary>
    public void OnDeselect(BaseEventData data) { focused = false; if (!Interested) owner?.EndInspection(this); }
    /// <summary>Releases stale ownership when a physical slot is hidden or destroyed.</summary>
    private void OnDisable() { hovered = focused = false; owner?.EndInspection(this); }
}
