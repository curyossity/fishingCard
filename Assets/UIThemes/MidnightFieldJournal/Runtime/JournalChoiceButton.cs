using UnityEngine.EventSystems;

/// <summary>Rejects pointer releases after a modal row has been rebound to a different choice.</summary>
public sealed class JournalChoiceButton : UnityEngine.UI.Button
{
    public long Generation { get; set; }
    private long pressed;
    /// <summary>Records the exact option set shown at pointer press.</summary>
    public override void OnPointerDown(PointerEventData data) { pressed = Generation; base.OnPointerDown(data); }
    /// <summary>Prevents stale presses from activating a replacement row.</summary>
    public override void OnPointerClick(PointerEventData data) { if (pressed == Generation) base.OnPointerClick(data); }
    /// <summary>Keeps controller-focused options visible when a choice list extends below the viewport.</summary>
    public override void OnSelect(BaseEventData data)
    {
        base.OnSelect(data); var scroll = GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scroll == null || scroll.content == null || scroll.viewport == null) return;
        UnityEngine.Canvas.ForceUpdateCanvases();
        var bounds = UnityEngine.RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, transform);
        var viewport = scroll.viewport.rect; float offset = bounds.min.y < viewport.yMin ? viewport.yMin - bounds.min.y : bounds.max.y > viewport.yMax ? viewport.yMax - bounds.max.y : 0;
        var position = scroll.content.anchoredPosition; position.y = UnityEngine.Mathf.Clamp(position.y + offset, 0, UnityEngine.Mathf.Max(0, scroll.content.rect.height - viewport.height)); scroll.content.anchoredPosition = position;
    }
}
