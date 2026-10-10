using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays creature-effect choices on the existing modal layer and forwards selections to runtime owners.</summary>
public sealed class CreatureEffectChoiceView : MonoBehaviour
{
    private RectTransform content;
    private TMP_Text heading;
    private TMP_Text instructions;
    private TMP_FontAsset font;
    private readonly List<Button> rows = new List<Button>();
    private readonly List<TMP_Text> rowLabels = new List<TMP_Text>();
    private readonly List<TMP_Text> rowDetails = new List<TMP_Text>();
    private readonly List<Image> rowPortraits = new List<Image>();
    private string choiceSignature;
    private Action<int> select;

    /// <summary>Builds a responsive modal with a scrolling choice list and shared game typography.</summary>
    public void Initialize(TMP_FontAsset bodyFont)
    {
        font = bodyFont != null ? bodyFont : TMP_Settings.defaultFontAsset;
        RectTransform root = (RectTransform)transform;
        Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image shade = gameObject.AddComponent<Image>();
        shade.color = new Color(0.015f, 0.045f, 0.05f, 0.94f);
        shade.raycastTarget = true;

        RectTransform panel = CreateRect("Choice Panel", root);
        Stretch(panel, new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.88f), Vector2.zero, Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.16f, 0.17f, 1f);
        heading = CreateText("Title", panel, 30f, FontWeight.Bold);
        Stretch(heading.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -64f), new Vector2(-28f, -16f));
        instructions = CreateText("Instructions", panel, 20f, FontWeight.Regular);
        Stretch(instructions.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(28f, -118f), new Vector2(-28f, -70f));

        RectTransform viewport = CreateRect("Choices Viewport", panel);
        Stretch(viewport, Vector2.zero, Vector2.one, new Vector2(24f, 24f), new Vector2(-24f, -130f));
        viewport.gameObject.AddComponent<Image>().color = new Color(0.03f, 0.12f, 0.13f, 1f);
        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.viewport = viewport;
        content = CreateRect("Choices", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;
    }

    /// <summary>Shows the current runtime-owned options without deciding their eligibility or results.</summary>
    public void Show(string title, string prompt, string[] labels, string[] details, Sprite[] portraits, Action<int> onSelect)
    {
        gameObject.SetActive(true);
        heading.text = title;
        instructions.text = prompt;
        select = onSelect;
        while (rows.Count < labels.Length)
        {
            CreateRow(rows.Count);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < labels.Length;
            rows[i].gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            rowLabels[i].text = labels[i];
            rowDetails[i].text = details != null && i < details.Length ? details[i] : string.Empty;
            rowPortraits[i].sprite = portraits != null && i < portraits.Length ? portraits[i] : null;
            rowPortraits[i].enabled = rowPortraits[i].sprite != null;
        }

        string signature = title + "\n" + prompt + "\n" + string.Join("\n", labels);
        // Keep keyboard focus valid when switching from Catch targets to creature offers.
        if (choiceSignature != signature)
        {
            choiceSignature = signature;
            content.anchoredPosition = Vector2.zero;
            if (labels.Length > 0)
            {
                rows[0].Select();
            }
        }
    }

    /// <summary>Hides the modal when its runtime queue has finished.</summary>
    public void Hide()
    {
        select = null;
        choiceSignature = null;
        gameObject.SetActive(false);
    }

    /// <summary>Creates one reusable portrait and description button for the scrolling choice list.</summary>
    private void CreateRow(int index)
    {
        RectTransform row = CreateRect("Choice " + (index + 1), content);
        Image plate = row.gameObject.AddComponent<Image>();
        plate.color = new Color(0.09f, 0.24f, 0.25f, 1f);
        Button button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = plate;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.45f, 0.85f, 0.78f, 1f);
        colors.selectedColor = new Color(0.85f, 0.71f, 0.37f, 1f);
        button.colors = colors;
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 124f;
        button.onClick.AddListener(() =>
        {
            select?.Invoke(index);
        });
        RectTransform portrait = CreateRect("Portrait", row);
        Stretch(portrait, Vector2.zero, new Vector2(0f, 1f), new Vector2(12f, 12f), new Vector2(96f, -12f));
        Image image = portrait.gameObject.AddComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        TMP_Text label = CreateText("Name", row, 24f, FontWeight.Bold);
        Stretch(label.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(110f, -44f), new Vector2(-18f, -10f));
        TMP_Text detail = CreateText("Description", row, 19f, FontWeight.Regular);
        Stretch(detail.rectTransform, Vector2.zero, Vector2.one, new Vector2(110f, 10f), new Vector2(-18f, -48f));
        rows.Add(button);
        rowLabels.Add(label);
        rowDetails.Add(detail);
        rowPortraits.Add(image);
    }

    /// <summary>Creates readable wrapping text while keeping all input on the option buttons.</summary>
    private TMP_Text CreateText(string name, Transform parent, float size, FontWeight weight)
    {
        RectTransform rect = CreateRect(name, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.fontWeight = weight;
        text.color = new Color(0.94f, 0.91f, 0.78f, 1f);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = size;
        text.raycastTarget = false;
        text.richText = false;
        return text;
    }

    /// <summary>Creates a UI transform without requiring scene-specific object references.</summary>
    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }

    /// <summary>Positions a UI element relative to its parent bounds.</summary>
    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max, Vector2 lower, Vector2 upper)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = lower;
        rect.offsetMax = upper;
    }
}
