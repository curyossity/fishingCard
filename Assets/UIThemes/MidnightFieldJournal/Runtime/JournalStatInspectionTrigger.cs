using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Connects a modified number to inspection without intercepting its parent card's click action.</summary>
public sealed class JournalStatInspectionTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private JournalStatTooltipStyle style;
    [SerializeField] private CreatureStat stat;
    private ICreatureStatInspectionSource source;
    private JournalStatTooltipHost host;
    private bool hovered, focused;
    public bool Interested => hovered || focused;
    public RectTransform Rect => (RectTransform)transform;

    /// <summary>Reads current visibility and modifiers from the shared card view, never from formatted text.</summary>
    public bool TryRead(out CardStatBreakdown data)
    {
        if(source==null)source=GetComponentInParent<ICreatureStatInspectionSource>();
        data=null;return source!=null&&source.TryGetStatBreakdown(stat,out data);
    }
    /// <summary>Starts delayed pointer inspection only when the current field has a visible modification.</summary>
    public void OnPointerEnter(PointerEventData e){hovered=true;Request(false);}
    /// <summary>Keeps a grace period for pointer travel into the scrollable tooltip.</summary>
    public void OnPointerExit(PointerEventData e){hovered=false;}
    /// <summary>Offers immediate keyboard/controller inspection without treating a pointer click as focus ownership.</summary>
    public void OnSelect(BaseEventData e){if(e is PointerEventData)return;focused=true;Request(true);}
    /// <summary>Releases navigation inspection when focus leaves this number.</summary>
    public void OnDeselect(BaseEventData e){focused=false;}
    /// <summary>Invalidates stale pooled-card ownership as soon as a number leaves the hierarchy.</summary>
    private void OnDisable(){hovered=focused=false;if(host!=null)host.End(this);}
    /// <summary>Creates one host per run view, falling back to the local canvas for isolated previews.</summary>
    private void Request(bool immediate)
    {
        if(style==null||!TryRead(out _))return;
        var run=GetComponentInParent<FishingRunView>();var canvas=GetComponentInParent<Canvas>();
        GameObject owner=run!=null?run.gameObject:canvas!=null?canvas.rootCanvas.gameObject:null;if(owner==null)return;
        host=owner.GetComponent<JournalStatTooltipHost>();if(host==null)host=owner.AddComponent<JournalStatTooltipHost>();
        host.Begin(this,style,immediate);
    }
}
