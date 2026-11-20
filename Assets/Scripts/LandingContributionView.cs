using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>A portrait/amount inspection target; it never submits strikes or placement commands.</summary>
public sealed class LandingContributionView : UnityEngine.UI.Selectable
{
    [SerializeField] private UnityEngine.UI.Image portrait;
    [SerializeField] private TMP_Text caption;
    [SerializeField] private bool includeName;
    private LandingStrikeFeedback owner;
    private TackleCardTooltip tooltip;
    public string Title { get; private set; }
    public string Subtitle { get; private set; }
    public string Amount { get; private set; }
    public string Explanation { get; private set; }
    public string Context { get; private set; }
    public int SourceId { get; private set; }
    public int[] SupportingIds { get; private set; }
    public RectTransform Anchor => (RectTransform)transform;

    /// <summary>Releases details before a pooled row represents another hit.</summary>
    public void Bind(LandingStrikeFeedback feedback, CardInstance member, string title, string amount, string explanation, string context, int[] support)
    {
        if (tooltip != null) tooltip.DismissDetail(this);
        owner = feedback; tooltip = GetComponentInParent<TackleCardTooltip>();
        SourceId = member?.InstanceId ?? -1; Title = member?.Definition.DisplayName ?? title;
        Subtitle = title; Amount = amount; Explanation = explanation; Context = context; SupportingIds = support ?? new int[0];
        portrait.sprite = member?.Definition.Artwork; portrait.enabled = portrait.sprite != null;
        caption.text = includeName ? Title + "  " + amount : amount.Replace(" points", "");
        gameObject.SetActive(true);
    }
    /// <summary>Connects inspection with subtle support highlights on currently visible rig cards.</summary>
    public void SetInspection(bool active) { if (owner != null) owner.Inspect(active ? SourceId : -1, active ? SupportingIds : null); }
    /// <summary>Displays the frozen reason for this resolved contribution.</summary>
    public override void OnPointerEnter(PointerEventData data) { base.OnPointerEnter(data); if (tooltip != null) tooltip.RequestDetail(this); }
    /// <summary>Releases pointer details without moving the rig.</summary>
    public override void OnPointerExit(PointerEventData data) { base.OnPointerExit(data); if (tooltip != null) tooltip.DismissDetail(this); }
    /// <summary>Provides the same explanation through native keyboard/gamepad focus.</summary>
    public override void OnSelect(BaseEventData data)
    {
        base.OnSelect(data);
        var scroll = GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scroll != null && scroll.viewport != null)
        {
            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, Anchor); var clip = scroll.viewport.rect;
            Vector2 shift = Vector2.zero;
            if (scroll.horizontal) shift.x = bounds.min.x < clip.xMin ? clip.xMin - bounds.min.x : bounds.max.x > clip.xMax ? clip.xMax - bounds.max.x : 0;
            if (scroll.vertical) shift.y = bounds.min.y < clip.yMin ? clip.yMin - bounds.min.y : bounds.max.y > clip.yMax ? clip.yMax - bounds.max.y : 0;
            scroll.StopMovement(); scroll.content.anchoredPosition += shift;
        }
        if (tooltip != null) tooltip.RequestDetail(this);
    }
    /// <summary>Clears details when focus moves to another control.</summary>
    public override void OnDeselect(BaseEventData data) { base.OnDeselect(data); if (tooltip != null) tooltip.DismissDetail(this); }
    /// <summary>Phase changes and pooling cannot retain an obsolete hit tooltip.</summary>
    protected override void OnDisable() { if (tooltip != null) tooltip.DismissDetail(this); base.OnDisable(); }
}
