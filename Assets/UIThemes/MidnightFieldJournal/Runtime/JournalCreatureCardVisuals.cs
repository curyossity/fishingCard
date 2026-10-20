using TMPro;
using UnityEngine;

/// <summary>Renders masked journal metadata and central-only layout independently of run rules.</summary>
public sealed class JournalCreatureCardVisuals : CreatureCardVisualExtension
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private TMP_Text tagsText, rarityText, typeText, nameText;
    [SerializeField] private TMP_Text catchName, catchText, baitName, baitText;
    [SerializeField] private RectTransform portraitRegion, portrait;
    [SerializeField] private GameObject rarityRoot, roleDecoration;
    [SerializeField] private GameObject eventDecoration;
    [SerializeField] private TMP_Text eventTypeText;
    [SerializeField] private JournalEventEmblem eventEmblem;
    [SerializeField] private JournalCardEmblem rarityDiamond;
    [SerializeField] private bool showRarity;
    [SerializeField] private JournalCardOutline catchSelection, baitSelection;
    [SerializeField] private JournalRoleFade catchFade, baitFade;
    [SerializeField] private JournalRoleFade catchGlow, baitGlow;
    private bool metadataVisible;

    /// <summary>Uses central-only portraits without replacing compact-chain sources.</summary>
    public override Sprite GetArtwork(CardDefinition card)
    {
        if (theme == null || card == null) return null;
        var eventEntry = theme.GetCentralEvent(card.UniqueId);
        if (eventEntry != null) return eventEntry.sprite;
        var entry = theme.GetCentralCard(card.UniqueId);
        if (entry != null) return entry.sprite;
        return theme.TryGetArtwork(card.UniqueId, out Sprite sprite) ? sprite : null;
    }

    /// <summary>Applies recorded layout after the shared view handles content, concealment and event rules.</summary>
    public override void Refresh(CardDefinition card, bool informationHidden, bool identityHidden, bool catchLayout)
    {
        metadataVisible = card != null && catchLayout && !identityHidden;
        tagsText.text = metadataVisible ? string.Join("  \u00b7  ", CreatureTagVocabulary.Normalize(card.Tags)).ToUpperInvariant() : string.Empty;
        rarityRoot.SetActive(metadataVisible && showRarity);
        roleDecoration.SetActive(catchLayout);
        eventDecoration.SetActive(!catchLayout);
        rarityText.text = metadataVisible ? card.Rarity.ToString().ToUpperInvariant() : string.Empty;
        Color ink = card.Rarity == CardRarity.Rare ? new Color32(152, 97, 45, 255)
            : card.Rarity == CardRarity.Uncommon ? new Color32(84, 140, 148, 255) : theme.ParchmentInk;
        rarityText.color = rarityDiamond.color = ink;
        Bounds(typeText.rectTransform, catchLayout ? Pixels(140, 65, 980, 40) : Pixels(460,70,340,38));
        Bounds(nameText.rectTransform, Pixels(140, 114, 980, 82));
        typeText.fontSize = catchLayout ? 28 : 26;
        var eventEntry = theme.GetCentralEvent(card.UniqueId);
        nameText.fontSizeMax = eventEntry != null ? eventEntry.titleSize : 66;
        if (catchLayout) typeText.text = card.CardType == CardType.ApexEncounter ? "A P E X" : card.CardType == CardType.Treasure ? "T R E A S U R E" : "C R E A T U R E";
        var entry = theme.GetCentralCard(card.UniqueId);
        Rect art = entry != null ? entry.portraitBounds : new Rect(110, 190, 1040, 585.311f);
        if (!catchLayout) art = eventEntry != null ? eventEntry.portraitBounds : new Rect(100,205,1060,596.567f);
        Bounds(portraitRegion, Pixels(art.x, art.y, art.width, art.height));
        portrait.localScale = Vector3.one;
        portrait.localRotation = Quaternion.identity;
        portrait.anchoredPosition = Vector2.zero;
        if (!catchLayout)
        {
            typeText.text = card.CardType == CardType.Location ? "L O C A T I O N" : "E V E N T";
            eventTypeText.text = string.Join(" ", card.CardType.ToString().ToUpperInvariant().ToCharArray());
            eventEmblem.SetCategory(card.CardType);
            ApplyText(catchText, new Rect(225,1070,810,205),46,32);
            catchText.alignment = TextAlignmentOptions.Top;
            if(eventEntry != null) catchText.text = EventLineBreaks(catchText.text,eventEntry.descriptionLineBreaks);
            return;
        }
        catchText.alignment = baitText.alignment = TextAlignmentOptions.TopLeft;
        ApplyText(catchName, entry != null ? entry.catchNameBounds : new Rect(120,1021,460,70), entry != null ? entry.catchNameSize : 42);
        ApplyText(baitName, entry != null ? entry.baitNameBounds : new Rect(690,1021,450,70), entry != null ? entry.baitNameSize : 42);
        ApplyText(catchText, entry != null ? entry.catchTextBounds : new Rect(150,catchName.text.Length==0?1021:1108,414,catchName.text.Length==0?271:180), entry != null ? entry.catchTextSize : 34, entry != null ? 26 : 22);
        ApplyText(baitText, entry != null ? entry.baitTextBounds : new Rect(701,baitName.text.Length==0?1021:1108,414,baitName.text.Length==0?271:180), entry != null ? entry.baitTextSize : 34, entry != null ? 26 : 22);
    }

    /// <summary>Adds authored presentation line breaks while retaining live text and its highlight markup.</summary>
    private static string EventLineBreaks(string text,int[] breaks)
    {
        if(breaks==null||breaks.Length==0)return text;
        var words=text.Split(new[]{' ','\n','\r','\t'},System.StringSplitOptions.RemoveEmptyEntries);
        var formatted=new System.Text.StringBuilder();
        for(int i=0;i<words.Length;i++)
        {
            if(i>0)formatted.Append(System.Array.IndexOf(breaks,i)>=0?'\n':' ');
            formatted.Append(words[i]);
        }
        return formatted.ToString();
    }

    /// <summary>Places a card-space text field relative to its independently selectable role target.</summary>
    private static void ApplyText(TMP_Text text, Rect pixels, float size, float minimumSize=26)
    {
        if (text == null || pixels.width <= 0) return;
        RectTransform parent = (RectTransform)text.transform.parent;
        Rect p = new Rect(parent.anchorMin, parent.anchorMax-parent.anchorMin);
        Rect b = Pixels(pixels.x,pixels.y,pixels.width,pixels.height);
        Bounds(text.rectTransform, new Rect((b.x-p.x)/p.width,(b.y-p.y)/p.height,b.width/p.width,b.height/p.height));
        text.fontSize = text.fontSizeMax = size;
        text.fontSizeMin = Mathf.Min(minimumSize,size);
        text.enableAutoSizing = true;
    }

    /// <summary>Converts approved top-left coordinates on the 1260 by 1400 card into Unity anchors.</summary>
    public static Rect Pixels(float x,float y,float width,float height) { return new Rect(x/1260,1-(y+height)/1400,width/1260,height/1400); }
    /// <summary>Updates one region without retaining old portrait offsets.</summary>
    private static void Bounds(RectTransform rect,Rect b) { rect.anchorMin=b.min;rect.anchorMax=b.max;rect.offsetMin=rect.offsetMax=Vector2.zero; }
    /// <summary>Controls native rarity visibility without changing gameplay data.</summary>
    public void SetRarityVisible(bool enabled) { showRarity=enabled;rarityRoot.SetActive(enabled&&metadataVisible); }
    /// <summary>Dims both roles until selection, then illuminates the chosen role without blocking either target.</summary>
    public override void RefreshSelection(EncounterEffectChoice choice,bool active)
    {
        if(catchSelection!=null)catchSelection.gameObject.SetActive(false);
        if(baitSelection!=null)baitSelection.gameObject.SetActive(false);
        if(catchGlow!=null)catchGlow.gameObject.SetActive(active&&choice==EncounterEffectChoice.Catch);
        if(baitGlow!=null)baitGlow.gameObject.SetActive(active&&choice==EncounterEffectChoice.Bait);
        if(catchFade!=null)catchFade.gameObject.SetActive(active&&choice!=EncounterEffectChoice.Catch);
        if(baitFade!=null)baitFade.gameObject.SetActive(active&&choice!=EncounterEffectChoice.Bait);
    }
    /// <summary>Removes identity-bearing decoration from blank and concealed presentations.</summary>
    public override void Clear() { metadataVisible=false;tagsText.text="";rarityText.text="";rarityRoot.SetActive(false);eventDecoration.SetActive(false);RefreshSelection(EncounterEffectChoice.None,false); }
}
