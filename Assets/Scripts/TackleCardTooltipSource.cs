using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Inspects a pooled rig card through pointer or native focus without issuing gameplay commands.</summary>
[RequireComponent(typeof(TackleRigCardView))]
public sealed class TackleCardTooltipSource : UnityEngine.UI.Selectable, IScrollHandler, IBeginDragHandler
{
    [SerializeField] private RectTransform faceBounds;
    [SerializeField] private bool above;
    private TackleCardTooltip host;
    private TackleRigCardView card;
    private bool hovered, focused;
    private CardInstance bound;
    private int boundLevel, boundValue;
    public RectTransform FaceBounds => faceBounds;
    public bool Above => above;
    public bool AfterPlacement { get; private set; }
    public CardInstance Member => card != null ? card.BoundInstance : null;

    /// <summary>Caches the local view; the overlay lives outside all card clipping masks.</summary>
    protected override void Awake() { base.Awake(); card = GetComponent<TackleRigCardView>(); }
    /// <summary>Resolves only the containing phase's serialized overlay.</summary>
    protected override void OnEnable() { base.OnEnable(); host = GetComponentInParent<TackleCardTooltip>(); }
    /// <summary>Recycling, phase exit and removal cannot retain an inspected identity.</summary>
    protected override void OnDisable()
    {
        hovered = focused = false;
        if (host != null) host.Dismiss(this);
        base.OnDisable();
    }
    /// <summary>Invalidates stale snapshots, including same-species previews for another gap.</summary>
    public void Rebind(CardInstance member, bool afterPlacement)
    {
        if (ReferenceEquals(bound, member) && boundLevel == member?.Level && boundValue == member?.HaulValueContribution && AfterPlacement == afterPlacement) return;
        if (host != null) host.Dismiss(this);
        bound = member; boundLevel = member?.Level ?? 0; boundValue = member?.HaulValueContribution ?? 0;
        AfterPlacement = afterPlacement;
        // Binding may happen under a stationary pointer after a placement preview changes.
        if ((hovered || focused) && member != null && host != null) host.Request(this, focused);
    }
    /// <summary>Starts a delayed inspection without altering placement selection.</summary>
    public override void OnPointerEnter(PointerEventData data)
    { base.OnPointerEnter(data); hovered = true; if (host != null) host.Request(this, false); }
    /// <summary>Closes pointer inspection even when a click previously focused this source.</summary>
    public override void OnPointerExit(PointerEventData data)
    { base.OnPointerExit(data); hovered = false; if (host != null) host.Release(this); }
    /// <summary>Shows the same information immediately for keyboard and gamepad navigation.</summary>
    public override void OnSelect(BaseEventData data)
    {
        base.OnSelect(data); focused = true;
        RevealInScroll();
        if (host != null) host.Request(this, true);
    }
    /// <summary>Releases native focus inspection without taking focus from the next control.</summary>
    public override void OnDeselect(BaseEventData data)
    { base.OnDeselect(data); focused = false; if (!hovered && host != null) host.Release(this); }
    /// <summary>Hides details while leaving scrolling to the existing parent ScrollRect.</summary>
    public void OnScroll(PointerEventData data)
    {
        if (host != null) host.Dismiss(this);
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, data, ExecuteEvents.scrollHandler);
    }
    /// <summary>Dismisses inspection when the parent begins dragging the rig.</summary>
    public void OnBeginDrag(PointerEventData data) { if (host != null) host.Dismiss(this); }
    /// <summary>Brings an offscreen focused card into its existing scroll viewport.</summary>
    private void RevealInScroll()
    {
        var scroll = GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scroll == null || scroll.viewport == null || scroll.content == null) return;
        Canvas.ForceUpdateCanvases();
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, faceBounds);
        var visible = scroll.viewport.rect;
        Vector2 shift = Vector2.zero;
        if (scroll.horizontal) shift.x = bounds.min.x < visible.xMin ? visible.xMin - bounds.min.x : bounds.max.x > visible.xMax ? visible.xMax - bounds.max.x : 0;
        if (scroll.vertical) shift.y = bounds.min.y < visible.yMin ? visible.yMin - bounds.min.y : bounds.max.y > visible.yMax ? visible.yMax - bounds.max.y : 0;
        scroll.StopMovement(); scroll.content.anchoredPosition += shift;
    }
}
