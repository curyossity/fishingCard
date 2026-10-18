using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays runtime-owned line load and capacity with reusable individual pip images.</summary>
public sealed class LineLoadMeterView : MonoBehaviour
{
    [SerializeField] private TMP_Text loadText;
    [SerializeField] private RectTransform pipRoot;
    [SerializeField] private Image pipPrefab;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite filledSprite;
    [SerializeField] private Sprite dangerSprite;
    private readonly List<Image> pips = new List<Image>();
    private int visibleCapacity;

    /// <summary>Refreshes the display without determining overload consequences or changing run state.</summary>
    public void SetLoad(int currentLoad, int capacity)
    {
        currentLoad = Mathf.Max(0, currentLoad);
        capacity = Mathf.Max(0, capacity);
        visibleCapacity = capacity;
        if (loadText != null) loadText.text = $"LINE LOAD  {currentLoad} / {capacity}";
        if (pipRoot == null || pipPrefab == null) return;
        while (pips.Count < capacity)
        {
            Image pip = Instantiate(pipPrefab, pipRoot);
            pip.name = $"Capacity Pip {pips.Count + 1}";
            pip.raycastTarget = false;
            pip.preserveAspect = true;
            pips.Add(pip);
        }
        for (int i = 0; i < pips.Count; i++)
        {
            pips[i].gameObject.SetActive(i < capacity);
            bool warning = currentLoad > capacity;
            pips[i].sprite = i >= currentLoad ? emptySprite : warning ? dangerSprite : filledSprite;
        }
        LayoutPips();
    }

    /// <summary>Reflows load pips when the container changes size.</summary>
    private void OnRectTransformDimensionsChange() { LayoutPips(); }

    /// <summary>Fits any capacity in centered rows while retaining the authored 4:3 pip silhouette.</summary>
    private void LayoutPips()
    {
        if (pipRoot == null || visibleCapacity == 0) return;
        float width = pipRoot.rect.width, height = pipRoot.rect.height;
        int columns = Mathf.Min(visibleCapacity, Mathf.Max(1, Mathf.FloorToInt(width / 85)));
        int rows = Mathf.CeilToInt(visibleCapacity / (float)columns);
        float cell = Mathf.Min(77, Mathf.Min(width / columns, height / rows * 4f / 3f));
        float step = Mathf.Min(85, width / columns);
        for (int i = 0; i < visibleCapacity && i < pips.Count; i++)
        {
            RectTransform rect = pips[i].rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.sizeDelta = new Vector2(cell, cell * .75f);
            int rowCount = Mathf.Min(columns, visibleCapacity - i / columns * columns);
            rect.anchoredPosition = new Vector2((i % columns - (rowCount - 1) * .5f) * step, -i / columns * height / rows);
        }
    }
}
