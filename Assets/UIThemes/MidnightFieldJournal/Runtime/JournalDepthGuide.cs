using System.Collections.Generic;
using UnityEngine;

/// <summary>Pools illustrated tier rows from the current biome; the controller remains authoritative for depth.</summary>
public sealed class JournalDepthGuide : MonoBehaviour
{
    [SerializeField] private JournalDepthTierView prototype;
    [SerializeField] private RectTransform line;
    [SerializeField] private Sprite shallows, reef, deepWater;
    private readonly List<JournalDepthTierView> rows=new List<JournalDepthTierView>();

    /// <summary>Adapts to authored tier counts, clears stale rows and highlights all matching depth ranges.</summary>
    public void Refresh(BiomeDefinition biome,int depth)
    {
        var tiers=biome==null?null:biome.DepthTiers;int count=0;
        if(tiers!=null)foreach(var tier in tiers)if(tier!=null)count++;
        while(rows.Count<count){var row=Instantiate(prototype,transform);row.name="Illustrated Biome Tier "+(rows.Count+1);rows.Add(row);}
        for(int i=count;i<rows.Count;i++)rows[i].gameObject.SetActive(false);
        float h=((RectTransform)transform).rect.height,padding=Mathf.Min(95*h/550,h/Mathf.Max(2,count*2)),step=count>1?(h-2*padding)/(count-1):0;
        line.gameObject.SetActive(count>1);line.anchorMin=line.anchorMax=new Vector2(0,1);line.pivot=new Vector2(.5f,1);line.anchoredPosition=new Vector2(50,-padding);line.sizeDelta=new Vector2(2,Mathf.Max(0,h-2*padding));
        int index=0;
        if(tiers==null)return;
        foreach(var tier in tiers)
        {
            if(tier==null)continue;var row=rows[index];row.gameObject.SetActive(true);var r=(RectTransform)row.transform;
            float rowHeight=count>1?Mathf.Min(175,step):175,cy=count==1?h/2:padding+index*step;
            r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(0,rowHeight);r.anchoredPosition=new Vector2(0,-cy);
            // These are decorative illustrations, never biome identity or encounter selection rules.
            Sprite art=index==0?shallows:index==count-1?deepWater:reef;
            row.Refresh(tier,depth,art,rowHeight);index++;
        }
    }
}
