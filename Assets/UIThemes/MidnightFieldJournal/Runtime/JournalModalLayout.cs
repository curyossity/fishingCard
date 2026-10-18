using TMPro;
using UnityEngine;

/// <summary>Builds matching native journal modal surfaces using theme-local typography and palette.</summary>
public static class JournalModalLayout
{
    /// <summary>Creates a full-screen input shade with an independently decorated paper panel.</summary>
    public static RectTransform Panel(Transform parent, MidnightFieldJournalTheme theme, string name, out GameObject root)
    {
        root = Rect(name, parent, new Rect(0, 0, 1, 1)).gameObject;
        var shade = root.AddComponent<UnityEngine.UI.Image>(); shade.color = new Color(.02f, .04f, .05f, .94f); shade.raycastTarget = true;
        RectTransform panel = Rect("Journal Panel", root.transform, new Rect(.12f, .09f, .76f, .82f));
        var paper = panel.gameObject.AddComponent<UnityEngine.UI.Image>(); paper.color = theme.LightInk; paper.raycastTarget = false;
        var edge = Rect("Copper Edge", panel, new Rect(0, 0, 1, 1)).gameObject.AddComponent<JournalCardOutline>(); edge.color = theme.Copper; edge.raycastTarget = false;
        root.SetActive(false); return panel;
    }
    /// <summary>Creates a stable normalized UI region.</summary>
    public static RectTransform Rect(string name, Transform parent, Rect bounds)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = bounds.min; rect.anchorMax = bounds.max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
    }
    /// <summary>Creates a wrapping, noninteractive text surface without leaking TMP markup from runtime labels.</summary>
    public static TMP_Text Text(string name, Transform parent, Rect bounds, MidnightFieldJournalTheme theme, float size, bool heading = false)
    {
        var text = Rect(name, parent, bounds).gameObject.AddComponent<TextMeshProUGUI>(); text.font = heading ? theme.DisplayFont : theme.BodyFont;
        text.fontSize = size; text.enableAutoSizing = true; text.fontSizeMin = size * .7f; text.fontSizeMax = size;
        text.color = theme.ParchmentInk; text.alignment = TextAlignmentOptions.MidlineLeft; text.raycastTarget = false; text.richText = false;
        text.textWrappingMode = TextWrappingModes.Normal; return text;
    }
    /// <summary>Creates a native paper command with independent hover/navigation feedback.</summary>
    public static UnityEngine.UI.Button Button(string name, Transform parent, Rect bounds, MidnightFieldJournalTheme theme, out TMP_Text label)
    {
        var rect = Rect(name, parent, bounds); var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.85f, .75f, .56f, .5f);
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(.65f, .9f, .9f); colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color(.5f, .7f, .7f); button.colors = colors;
        label = Text("Label", rect, new Rect(.025f, .05f, .95f, .9f), theme, 36); return button;
    }
    /// <summary>Builds a clamped scroll root/viewport/content hierarchy with responsive row sizing.</summary>
    public static RectTransform Scroll(Transform parent, Rect bounds, string name)
    {
        var root = Rect(name, parent, bounds); var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        var viewport = Rect("Viewport", root, new Rect(0, 0, 1, 1)); var hit = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear; hit.raycastTarget = true; viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var content = Rect("Content", viewport, new Rect(0, 1, 1, 0)); content.pivot = new Vector2(.5f, 1);
        var layout = content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.spacing = 18; layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childControlWidth = true; layout.childControlHeight = true; layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = content; return content;
    }
    /// <summary>Keeps controller navigation within the active modal and excludes disabled commands.</summary>
    public static void Navigation(System.Collections.Generic.IList<UnityEngine.UI.Button> buttons)
    {
        var active = new System.Collections.Generic.List<UnityEngine.UI.Button>();
        foreach (var button in buttons) if (button.gameObject.activeInHierarchy && button.interactable) active.Add(button);
        for (int i = 0; i < active.Count; i++) active[i].navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
            selectOnUp = active[(i + active.Count - 1) % active.Count], selectOnDown = active[(i + 1) % active.Count], selectOnLeft = active[i], selectOnRight = active[i] };
    }
}
