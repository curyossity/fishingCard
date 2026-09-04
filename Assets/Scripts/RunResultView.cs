using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public sealed class RunResultView : MonoBehaviour
{
    private static readonly Color BackdropColor = new Color(0.015f, 0.025f, 0.03f, 0.94f);
    private static readonly Color PanelColor = new Color(0.055f, 0.075f, 0.08f, 1f);
    private static readonly Color GoldColor = new Color(0.96f, 0.73f, 0.24f, 1f);
    private static readonly Color HaulColor = new Color(0.37f, 0.82f, 0.60f, 1f);
    private static readonly Color ReleasedColor = new Color(0.47f, 0.72f, 0.82f, 1f);
    private static readonly Color LostColor = new Color(0.94f, 0.38f, 0.31f, 1f);
    private static readonly Color MutedTextColor = new Color(0.68f, 0.73f, 0.74f, 1f);

    private RectTransform backdropRoot;
    private Text goldText;
    private Text runStatsText;
    private Text haulText;
    private Text releasedText;
    private Text lostText;
    private Font uiFont;

    /// <summary>
    /// Creates the results layout and keeps it hidden until a run has ended.
    /// </summary>
    private void Awake()
    {
        EnsureLayout();
        backdropRoot.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the latest completed run result, or hides the panel while a run is active.
    /// </summary>
    public void Refresh(bool runActive, FishingRunResult result, int totalGold)
    {
        EnsureLayout();
        bool shouldShow = !runActive && result != null && result.HasResult;
        backdropRoot.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        string loadStatus = result.WasOverloaded ? "OVERLOADED AT SURFACE" : "LINE HELD WITHIN CAPACITY";
        goldText.text = $"+{result.GoldAwarded} GOLD     TOTAL {Mathf.Max(0, totalGold)}";
        runStatsText.text = $"HAUL VALUE {result.HaulValue}     DEPTH {result.SurfaceDepth}\n"
            + $"LINE LOAD {result.SurfaceLineLoad} / {result.LineCapacity}     {loadStatus}";
        haulText.text = BuildCatchList(result.Haul, true);
        releasedText.text = BuildCatchList(result.ReleasedCatches, true);
        lostText.text = BuildCatchList(result.LostCatches, true);
    }

    /// <summary>
    /// Creates the full-screen result backdrop and its stable summary columns once.
    /// </summary>
    private void EnsureLayout()
    {
        if (backdropRoot != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject backdropObject = CreateUiObject("Run Result Backdrop", transform);
        backdropRoot = backdropObject.GetComponent<RectTransform>();
        SetAnchoredRect(backdropRoot, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);
        AddImage(backdropObject, BackdropColor);

        GameObject panelObject = CreateUiObject("Run Result Panel", backdropRoot);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.12f, 0.10f);
        panelRect.anchorMax = new Vector2(0.88f, 0.90f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        AddImage(panelObject, PanelColor);

        Text titleText = CreateText("Title", panelRect, 30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetAnchoredRect(titleText.rectTransform, new Vector2(0f, 1f), Vector2.one, 24f, -62f, -24f, -14f);
        titleText.text = "RUN COMPLETE";

        goldText = CreateText("Gold", panelRect, 21, FontStyle.Bold, TextAnchor.MiddleCenter, GoldColor);
        SetAnchoredRect(goldText.rectTransform, new Vector2(0f, 1f), Vector2.one, 24f, -100f, -24f, -66f);

        runStatsText = CreateText("Run Stats", panelRect, 14, FontStyle.Bold, TextAnchor.MiddleCenter, MutedTextColor);
        SetAnchoredRect(runStatsText.rectTransform, new Vector2(0f, 1f), Vector2.one, 24f, -154f, -24f, -104f);

        CreateResultColumn(panelRect, "BROUGHT HOME", 0.04f, 0.35f, HaulColor, out haulText);
        CreateResultColumn(panelRect, "RELEASED", 0.36f, 0.67f, ReleasedColor, out releasedText);
        CreateResultColumn(panelRect, "LOST", 0.68f, 0.96f, LostColor, out lostText);
    }

    /// <summary>
    /// Creates one outcome column for successful, released, or lost catches.
    /// </summary>
    private void CreateResultColumn(
        RectTransform parent,
        string heading,
        float anchorMinX,
        float anchorMaxX,
        Color headingColor,
        out Text contentText)
    {
        Text headingText = CreateText(heading, parent, 15, FontStyle.Bold, TextAnchor.UpperLeft, headingColor);
        SetAnchoredRect(
            headingText.rectTransform,
            new Vector2(anchorMinX, 1f),
            new Vector2(anchorMaxX, 1f),
            0f,
            -190f,
            0f,
            -164f);
        headingText.text = heading;

        contentText = CreateText($"{heading} List", parent, 14, FontStyle.Normal, TextAnchor.UpperLeft, Color.white);
        SetAnchoredRect(
            contentText.rectTransform,
            new Vector2(anchorMinX, 0f),
            new Vector2(anchorMaxX, 1f),
            0f,
            24f,
            0f,
            -194f);
        contentText.resizeTextForBestFit = true;
        contentText.resizeTextMinSize = 10;
        contentText.resizeTextMaxSize = 14;
    }

    /// <summary>
    /// Formats catch names and resolved values as readable result lines.
    /// </summary>
    private static string BuildCatchList(CardInstance[] catches, bool includeValue)
    {
        CardInstance[] safeCatches = catches ?? Array.Empty<CardInstance>();

        if (safeCatches.Length == 0)
        {
            return "None";
        }

        StringBuilder summary = new StringBuilder();

        for (int i = 0; i < safeCatches.Length; i++)
        {
            if (i > 0)
            {
                summary.AppendLine();
            }

            CardInstance caughtInstance = safeCatches[i];
            string catchName = caughtInstance?.Definition == null ? "Unknown catch" : caughtInstance.Definition.DisplayName;
            summary.Append(catchName);

            if (includeValue && caughtInstance != null)
            {
                summary.Append($"  ({caughtInstance.CurrentValue} Value)");
            }
        }

        return summary.ToString();
    }

    /// <summary>
    /// Creates a UI GameObject under the requested parent.
    /// </summary>
    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    /// <summary>
    /// Adds a non-interactive Image using the requested color.
    /// </summary>
    private static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Creates configured legacy UI text for the generated results layout.
    /// </summary>
    private Text CreateText(
        string objectName,
        Transform parent,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Applies anchors and edge offsets to a generated UI element.
    /// </summary>
    private static void SetAnchoredRect(
        RectTransform rectTransform,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float left,
        float bottom,
        float right,
        float top)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(right, top);
    }
}
