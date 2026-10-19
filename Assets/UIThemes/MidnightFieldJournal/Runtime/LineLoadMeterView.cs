using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays runtime-owned load and capacity in the engraved gauge without calculating gameplay consequences.</summary>
public sealed class LineLoadMeterView : MonoBehaviour
{
    [SerializeField] private TMP_Text loadText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private RectTransform pipRoot;
    [SerializeField] private Image pipPrefab;
    [SerializeField] private Color emptyColor=new Color32(229,213,184,255);
    [SerializeField] private Color filledColor=new Color32(49,74,83,255);
    [SerializeField] private Color capacityColor=new Color32(143,105,58,255);
    [SerializeField] private Color dangerColor=new Color32(147,74,53,255);
    private readonly List<Image> pips = new List<Image>();
    private int visibleCapacity;

    /// <summary>Refreshes the display without determining overload consequences or changing run state.</summary>
    public void SetLoad(int currentLoad, int capacity)
    {
        currentLoad = Mathf.Max(0, currentLoad);
        capacity = Mathf.Max(0, capacity);
        visibleCapacity = capacity;
        bool warning=currentLoad>capacity,full=capacity>0&&currentLoad==capacity;
        if(loadText!=null){loadText.text=$"{currentLoad} / {capacity}";loadText.color=warning?dangerColor:new Color32(38,53,57,255);}
        if(statusText!=null)
        {
            statusText.text=warning?$"OVERLOADED  +{currentLoad-capacity}":full?"AT CAPACITY":"WITHIN CAPACITY";
            statusText.color=warning?dangerColor:new Color32(101,91,73,255);
        }
        if (pipRoot == null || pipPrefab == null) return;
        while (pips.Count < capacity)
        {
            Image pip = Instantiate(pipPrefab, pipRoot);
            pip.name = $"Capacity Pip {pips.Count + 1}";
            pip.raycastTarget = false;
            pip.preserveAspect = false;
            pips.Add(pip);
        }
        for (int i = 0; i < pips.Count; i++)
        {
            pips[i].gameObject.SetActive(i < capacity);
            bool filled=i<currentLoad;
            pips[i].color=!filled?emptyColor:warning?dangerColor:full?capacityColor:filledColor;
            if(pips[i] is JournalLoadSegment segment)segment.SetState(filled,warning);
        }
        LayoutPips();
    }

    /// <summary>Reflows load pips when the container changes size.</summary>
    private void OnRectTransformDimensionsChange() { LayoutPips(); }

    /// <summary>Fits the actual capacity into the single authored rail, including after capacity or viewport changes.</summary>
    private void LayoutPips()
    {
        if (pipRoot == null || visibleCapacity == 0) return;
        float width = pipRoot.rect.width, height = pipRoot.rect.height;
        float gap=Mathf.Min(6,width/(visibleCapacity*2f));
        float cell=Mathf.Max(0,(width-gap*(visibleCapacity-1))/visibleCapacity);
        for (int i = 0; i < visibleCapacity && i < pips.Count; i++)
        {
            RectTransform rect = pips[i].rectTransform;
            rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.pivot=new Vector2(0,.5f);
            rect.sizeDelta=new Vector2(cell,height);rect.anchoredPosition=new Vector2(i*(cell+gap),0);
        }
    }
}
