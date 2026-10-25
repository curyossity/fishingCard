using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Owns one live modifier tooltip per run, including pointer travel, input locks and source lifetime.</summary>
public sealed class JournalStatTooltipHost : MonoBehaviour
{
    private JournalStatInspectionTrigger source;
    private JournalStatTooltip tooltip;
    private JournalStatTooltipStyle style;
    private FishingRunView run;
    private int occurrence;
    private CardDefinition creature;
    private float showAt, exitAt;
    private bool hovered;
    private Vector3 sourcePosition;
    private readonly List<CanvasGroup> groups=new List<CanvasGroup>();
    private readonly List<UnityEngine.UI.Graphic> modalGraphics=new List<UnityEngine.UI.Graphic>();
    private readonly Vector3[] corners=new Vector3[4];
    public bool HasActiveSource => source!=null;
    public JournalStatTooltip Tooltip => tooltip;

    /// <summary>Claims a visible modified field and resets scroll ownership for a genuinely new source.</summary>
    public void Begin(JournalStatInspectionTrigger trigger,JournalStatTooltipStyle resources,bool immediate)
    {
        if(!trigger.TryRead(out CardStatBreakdown data))return;
        if(source!=trigger){Hide();source=trigger;occurrence=data.OccurrenceId;creature=data.Creature;style=resources;}
        run=GetComponent<FishingRunView>();sourcePosition=source.Rect.position;showAt=Time.unscaledTime+(immediate?0:.18f);exitAt=0;
    }
    /// <summary>Invalidates only the field relinquishing ownership, not a newly hovered neighbour.</summary>
    public void End(JournalStatInspectionTrigger trigger){if(source==trigger)Hide();}
    /// <summary>Allows wheel/drag interaction after leaving the small source number.</summary>
    public void SetTooltipHovered(bool value){hovered=value;if(value)exitAt=0;}

    /// <summary>Rebinds real data while dismissing stale occurrences, modal takeover, transitions and scrolled-out cards.</summary>
    private void LateUpdate()
    {
        if(source==null)return;
        if(!source.isActiveAndEnabled||!Allowed()||!source.TryRead(out CardStatBreakdown data)
            ||data.OccurrenceId!=occurrence||data.Creature!=creature){Hide();return;}
        var selected=EventSystem.current==null?null:EventSystem.current.currentSelectedGameObject;
        bool tooltipFocused=tooltip!=null&&selected!=null&&selected.transform.IsChildOf(tooltip.transform);
        if(!source.Interested&&!hovered&&!tooltipFocused)
        {if(exitAt==0)exitAt=Time.unscaledTime+.25f;if(Time.unscaledTime>=exitAt){Hide();return;}}
        else exitAt=0;
        // A parent chain scroll invalidates the anchor; numeric pop/tilt around a fixed center does not.
        if(Vector3.Distance(source.Rect.position,sourcePosition)>2){Hide();return;}
        if(Time.unscaledTime<showAt)return;
        if(tooltip==null)
        {
            Canvas canvas=GetComponentInParent<Canvas>();
            Transform parent=run!=null&&run.TooltipLayer!=null?run.TooltipLayer.transform:canvas!=null?canvas.rootCanvas.transform:null;
            if(parent==null)return;
            var go=new GameObject("Stat Modifier Tooltip",typeof(RectTransform));go.transform.SetParent(parent,false);
            tooltip=go.AddComponent<JournalStatTooltip>();tooltip.Initialize(style,this);
        }
        tooltip.gameObject.SetActive(true);tooltip.Bind(data);Place();
        if(EventSystem.current!=null&&InputNavigation(out int direction))
        {
            var e=new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,direction)};tooltip.Scroll.OnScroll(e);
        }
    }

    /// <summary>Uses the project's input package for focused tooltip scrolling without requiring pointer input.</summary>
    private bool InputNavigation(out int direction)
    {
        direction=0;
#if ENABLE_INPUT_SYSTEM
        var keyboard=UnityEngine.InputSystem.Keyboard.current;
        if(keyboard!=null){if(keyboard.upArrowKey.wasPressedThisFrame)direction=1;if(keyboard.downArrowKey.wasPressedThisFrame)direction=-1;if(keyboard.escapeKey.wasPressedThisFrame)Hide();}
#endif
        return direction!=0;
    }

    /// <summary>Honors ancestor masks and CanvasGroups, and blocks inspection whenever modal or encounter movement owns input.</summary>
    private bool Allowed()
    {
        if(run!=null)
        {
            if(run.IsEncounterTransitionPlaying||run.IsSurfaceTransitionPlaying)return false;
            modalGraphics.Clear();if(run.ModalLayer!=null)run.ModalLayer.GetComponentsInChildren(false,modalGraphics);
            foreach(var g in modalGraphics)if(g.raycastTarget&&g.isActiveAndEnabled&&g.canvasRenderer.GetInheritedAlpha()>.01f)return false;
        }
        groups.Clear();source.GetComponentsInParent(false,groups);
        foreach(var g in groups){if(g.alpha<.01f||!g.interactable||!g.blocksRaycasts)return false;if(g.ignoreParentGroups)break;}
        var masks=source.GetComponentsInParent<UnityEngine.UI.RectMask2D>();
        foreach(var mask in masks)
        {
            if(!mask.isActiveAndEnabled)continue;source.Rect.GetWorldCorners(corners);var rect=(RectTransform)mask.transform;
            Vector3 a=rect.InverseTransformPoint(corners[0]),b=rect.InverseTransformPoint(corners[2]);
            if(!rect.rect.Overlaps(Rect.MinMaxRect(a.x,a.y,b.x,b.y)))return false;
        }
        return true;
    }

    /// <summary>Clamps and flips the complete variable-height tooltip beside its number within the dedicated overlay.</summary>
    private void Place()
    {
        var rect=(RectTransform)tooltip.transform;var parent=(RectTransform)rect.parent;var canvas=parent.GetComponentInParent<Canvas>();
        Camera camera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
        source.Rect.GetWorldCorners(corners);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,RectTransformUtility.WorldToScreenPoint(camera,corners[2]),camera,out Vector2 right);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,RectTransformUtility.WorldToScreenPoint(camera,corners[0]),camera,out Vector2 left);
        Rect available=parent.rect;float scale=Mathf.Max(.1f,Mathf.Min(1,Mathf.Min((available.width-24)/680,(available.height-24)/rect.rect.height)));
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.localScale=Vector3.one*scale;
        float w=680*scale,h=rect.rect.height*scale,x=right.x+10+w*.5f;
        if(x+w*.5f>available.xMax-12)x=left.x-10-w*.5f;
        rect.anchoredPosition=new Vector2(Mathf.Clamp(x,available.xMin+w*.5f+12,available.xMax-w*.5f-12),
            Mathf.Clamp((left.y+right.y)*.5f,available.yMin+h*.5f+12,available.yMax-h*.5f-12));rect.SetAsLastSibling();
    }

    /// <summary>Releases hover, scroll and navigation state without destroying the pooled tooltip.</summary>
    private void Hide(){source=null;hovered=false;exitAt=0;if(tooltip!=null)tooltip.gameObject.SetActive(false);}
    /// <summary>Closes the overlay with the run HUD.</summary>
    private void OnDisable(){Hide();}
    /// <summary>Destroys the overlay owned by this host when the run view is disposed.</summary>
    private void OnDestroy(){if(tooltip!=null)Destroy(tooltip.gameObject);}
}
