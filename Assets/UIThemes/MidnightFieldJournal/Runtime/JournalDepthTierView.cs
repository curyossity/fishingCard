using TMPro;
using UnityEngine;

/// <summary>Displays one authored depth tier with independent label, range, engraving and native marker.</summary>
public sealed class JournalDepthTierView : MonoBehaviour
{
    [SerializeField] private TMP_Text label, range;
    [SerializeField] private UnityEngine.UI.Image engraving;
    [SerializeField] private JournalDepthNode node;
    [SerializeField] private Material inactiveIllustrationMaterial;

    /// <summary>Rebinds a pooled row, including active and inactive presentation, without changing tier data.</summary>
    public void Refresh(BiomeDepthTierDefinition tier,int depth,Sprite art,float height)
    {
        bool active=tier.ContainsDepth(depth);
        label.text=tier.DisplayName.ToUpperInvariant();
        range.text=tier.MaximumDepth<0?tier.MinimumDepth+"+":tier.MinimumDepth+"–"+tier.MaximumDepth;
        label.color=active?(Color)new Color32(243,200,136,255):new Color32(157,157,157,230);
        range.color=active?(Color)new Color32(200,168,117,255):new Color32(123,123,123,210);
        node.SetCurrent(active);engraving.sprite=art;engraving.enabled=art!=null;
        engraving.material=active?null:inactiveIllustrationMaterial;
        engraving.color=new Color(1,1,1,active?1:.42f);
        float scale=Mathf.Min(1,height/175),fontScale=Mathf.Min(1,height/110);
        label.fontSize=label.fontSizeMax=32*fontScale;label.fontSizeMin=Mathf.Min(16,label.fontSize);range.fontSize=range.fontSizeMax=24*fontScale;range.fontSizeMin=Mathf.Min(12,range.fontSize);
        var artRect=engraving.rectTransform;artRect.sizeDelta=new Vector2(205,153.75f)*scale;
        node.rectTransform.sizeDelta=Vector2.one*(82*scale);
    }
}
