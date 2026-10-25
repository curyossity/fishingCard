using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Shows the R045 dynamic breakdown with three clipped rows and arrow-only scrolling.</summary>
public sealed class JournalStatTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private JournalStatTooltipStyle style;
    private JournalStatTooltipHost host;
    private TMP_Text baseline, current, baselineLabel, currentLabel;
    private RectTransform viewport, content;
    private UnityEngine.UI.ScrollRect scroll;
    private UnityEngine.UI.Button up, down;
    private readonly List<Row> rows = new List<Row>();
    private string signature;
    public UnityEngine.UI.ScrollRect Scroll => scroll;
    public bool UpVisible => up != null && up.gameObject.activeSelf;
    public bool DownVisible => down != null && down.gameObject.activeSelf;
    private static readonly Color Ink = new Color32(48,68,72,255), Sepia = new Color32(155,113,63,255);
    private sealed class Row { public RectTransform root; public TMP_Text name, amount; public UnityEngine.UI.Image icon; }

    /// <summary>Constructs the data-bound elements once; all text and glyphs remain independent of the sliced frame.</summary>
    public void Initialize(JournalStatTooltipStyle resources, JournalStatTooltipHost owner)
    {
        style = resources; host = owner;
        var frame = gameObject.AddComponent<UnityEngine.UI.Image>(); frame.sprite = style.frame;
        frame.type = UnityEngine.UI.Image.Type.Sliced; frame.raycastTarget = true;
        baselineLabel = Label("Default Label",22,false,true); currentLabel = Label("Current Label",22,false,true);
        baseline = Label("Default",50); current = Label("Current",50,true);
        foreach(var number in new[]{baseline,current})
        {number.overflowMode=TextOverflowModes.Overflow;number.enableAutoSizing=true;number.fontSizeMin=30;number.fontSizeMax=50;}
        var arrow = Label("Comparison Arrow",38); arrow.text="→"; arrow.color=Sepia; Position(arrow.rectTransform,305,94,70,55);
        Position(baselineLabel.rectTransform,88,45,245,36);Position(currentLabel.rectTransform,347,45,245,36);
        Position(baseline.rectTransform,88,86,245,66);Position(current.rectTransform,347,86,245,66);
        var divider=Child("Divider",transform);Position(divider,76,169,528,1);
        var line=divider.gameObject.AddComponent<UnityEngine.UI.Image>();line.color=new Color(Sepia.r,Sepia.g,Sepia.b,.5f);line.raycastTarget=false;
        viewport=Child("Reasons Viewport",transform);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        content=Child("Reasons Content",viewport);content.anchorMin=content.anchorMax=new Vector2(0,1);content.pivot=new Vector2(0,1);
        scroll=gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.content=content;scroll.viewport=viewport;
        scroll.horizontal=false;scroll.vertical=true;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
        scroll.inertia=false;scroll.scrollSensitivity=72;scroll.onValueChanged.AddListener(_=>RefreshArrows());
        up=Arrow("Scroll Up",true);down=Arrow("Scroll Down",false);
        up.onClick.AddListener(()=>Move(-1));down.onClick.AddListener(()=>Move(1));
    }

    /// <summary>Updates real sources in place and preserves the player's scroll offset across ordinary HUD refreshes.</summary>
    public void Bind(CardStatBreakdown data)
    {
        string next=data.Stat+":"+data.Baseline+":"+data.Current;
        foreach(var r in data.Reasons)next+="|"+r.Name+":"+r.Amount;
        if(signature==next)return;
        signature=next;int visible=Mathf.Min(3,data.Reasons.Length);bool overflow=data.Reasons.Length>3;
        ((RectTransform)transform).sizeDelta=new Vector2(680,276+visible*72+(overflow?44:0));
        baselineLabel.text="DEFAULT "+data.Stat.ToString().ToUpperInvariant();currentLabel.text="CURRENT "+data.Stat.ToString().ToUpperInvariant();
        baseline.text=data.Baseline.ToString();current.text=data.Current.ToString();current.color=AmountColor(data.Stat,data.Current-data.Baseline);
        Position(viewport,44,216,592,visible*72);content.sizeDelta=new Vector2(592,data.Reasons.Length*72);
        while(rows.Count<data.Reasons.Length)rows.Add(CreateRow(rows.Count));
        for(int i=0;i<rows.Count;i++)
        {
            Row row=rows[i];row.root.gameObject.SetActive(i<data.Reasons.Length);if(i>=data.Reasons.Length)continue;
            CardStatReason reason=data.Reasons[i];row.name.text=reason.Name;row.amount.text=(reason.Amount>0?"+":"−")+Mathf.Abs(reason.Amount);
            row.amount.color=AmountColor(data.Stat,reason.Amount);
            row.icon.sprite=style.GetIcon(reason.Source,out bool silhouette);row.icon.enabled=row.icon.sprite!=null;
            row.icon.material=silhouette?style.silhouetteMaterial:null;row.icon.color=silhouette?Ink:Color.white;
        }
        Position(up.GetComponent<RectTransform>(),316,178,48,36);
        Position(down.GetComponent<RectTransform>(),316,216+visible*72+6,48,36);
        scroll.StopMovement();float max=Mathf.Max(0,content.rect.height-viewport.rect.height);
        content.anchoredPosition=new Vector2(0,Mathf.Clamp(content.anchoredPosition.y,0,max));
        scroll.vertical=overflow;RefreshArrows();
    }

    /// <summary>Reports pointer ownership so moving into the tooltip does not close it before scrolling.</summary>
    public void OnPointerEnter(PointerEventData data){host?.SetTooltipHovered(true);}
    /// <summary>Lets the host apply a brief exit grace period between the source number and tooltip.</summary>
    public void OnPointerExit(PointerEventData data){host?.SetTooltipHovered(false);}

    /// <summary>Creates one pooled reason row without a background tile.</summary>
    private Row CreateRow(int index)
    {
        var row=new Row {root=Child("Reason "+index,content)};Position(row.root,0,index*72,592,72);
        var icon=Child("Source Icon",row.root);Position(icon,16,12,48,48);row.icon=icon.gameObject.AddComponent<UnityEngine.UI.Image>();
        row.icon.preserveAspect=true;row.icon.raycastTarget=false;
        row.name=Label("Source Name",30,true,false,row.root);row.name.alignment=TextAlignmentOptions.TopLeft;Position(row.name.rectTransform,90,18,390,43);
        row.amount=Label("Signed Amount",34,true,false,row.root);row.amount.alignment=TextAlignmentOptions.TopRight;Position(row.amount.rectTransform,495,15,76,47);
        return row;
    }

    /// <summary>Styles native text using existing local licensed fonts and no new typography assets.</summary>
    private TMP_Text Label(string name,float size,bool bold=false,bool heading=false,Transform parent=null)
    {
        var t=Child(name,parent??transform).gameObject.AddComponent<TextMeshProUGUI>();
        t.font=heading?style.theme.DisplayFont:bold?style.theme.BodyBold:style.theme.BodyFont;t.fontSize=size;t.color=heading?new Color32(114,92,68,255):Ink;
        t.fontStyle=FontStyles.Normal;t.alignment=TextAlignmentOptions.Top;t.raycastTarget=false;
        t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
    }

    /// <summary>Creates an arrow-only control; it has no numbered or above/below caption and no scrollbar.</summary>
    private UnityEngine.UI.Button Arrow(string name,bool upwards)
    {
        var r=Child(name,transform);var hit=r.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=Color.clear;
        var button=r.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=hit;button.transition=UnityEngine.UI.Selectable.Transition.None;
        var glyph=Child("Arrow",r).gameObject.AddComponent<JournalStatScrollArrow>();glyph.Upwards=upwards;glyph.color=Sepia;glyph.raycastTarget=false;
        var gr=glyph.rectTransform;gr.anchorMin=Vector2.zero;gr.anchorMax=Vector2.one;gr.offsetMin=gr.offsetMax=Vector2.zero;return button;
    }

    /// <summary>Moves exactly one row while leaving wheel and drag input to ScrollRect.</summary>
    private void Move(int direction)
    {float max=Mathf.Max(0,content.rect.height-viewport.rect.height);content.anchoredPosition=new Vector2(0,Mathf.Clamp(content.anchoredPosition.y+direction*72,0,max));RefreshArrows();}
    /// <summary>Derives cue visibility from real content bounds, including fractional wheel positions.</summary>
    private void RefreshArrows()
    {float max=Mathf.Max(0,content.rect.height-viewport.rect.height);up.gameObject.SetActive(content.anchoredPosition.y>.5f);down.gameObject.SetActive(content.anchoredPosition.y<max-.5f);}
    /// <summary>Uses the same benefit semantics as existing stat feedback.</summary>
    private static Color AmountColor(CreatureStat stat,int amount)
    {return amount==0?Ink:(stat==CreatureStat.Value?amount>0:amount<0)?new Color32(24,112,58,255):new Color32(174,44,38,255);}
    /// <summary>Creates top-left anchored reusable UI geometry.</summary>
    private static RectTransform Child(string name,Transform parent)
    {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
    /// <summary>Applies the exact top-left geometry from the approved 4K design.</summary>
    private static void Position(RectTransform r,float x,float y,float w,float h)
    {r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
}
