using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Shows runtime choice labels in reusable journal rows, keeping all game decisions in the controller.</summary>
public sealed class JournalChoiceModal : CreatureChoicePresentation
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private JournalModalHost host;
    private GameObject root;
    private RectTransform content;
    private TMP_Text heading, instructions;
    private readonly List<JournalChoiceButton> rows = new List<JournalChoiceButton>();
    private readonly List<TMP_Text> labels = new List<TMP_Text>(), details = new List<TMP_Text>();
    private Action<int> select;
    private string signature;
    private long generation;
    public bool IsVisible => root != null && root.activeInHierarchy;
    public IReadOnlyList<JournalChoiceButton> Rows => rows;

    /// <summary>Creates the modal once without introducing a second choice state machine.</summary>
    private void EnsureLayout()
    {
        if (root != null) return;
        RectTransform panel = JournalModalLayout.Panel(transform, theme, "Creature and Technique Choice", out root);
        heading = JournalModalLayout.Text("Title", panel, new Rect(.04f, .87f, .92f, .09f), theme, 58, true);
        instructions = JournalModalLayout.Text("Instructions", panel, new Rect(.04f, .76f, .92f, .10f), theme, 38);
        content = JournalModalLayout.Scroll(panel, new Rect(.035f, .04f, .93f, .70f), "Choice List");
    }
    /// <summary>Shows only authorized runtime text; legacy portraits are deliberately not rendered in this theme.</summary>
    public override void Show(string title, string prompt, string[] options, string[] descriptions, Sprite[] portraits, Action<int> onSelect)
    {
        EnsureLayout(); options = options ?? Array.Empty<string>();
        string next = title + "\n" + prompt + "\n" + string.Join("\n", options) + "\n" + string.Join("\n", descriptions ?? Array.Empty<string>());
        bool changed = signature != next; generation++; signature = next;
        heading.text = title; instructions.text = prompt; select = onSelect;
        while (rows.Count < options.Length) AddRow(rows.Count);
        host.Show(root);
        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < options.Length; rows[i].gameObject.SetActive(active); if (!active) continue;
            rows[i].Generation = generation; rows[i].interactable = true; labels[i].text = options[i]; details[i].text = descriptions != null && i < descriptions.Length ? descriptions[i] : string.Empty;
        }
        var navigation = new List<UnityEngine.UI.Button>(); foreach (var row in rows) navigation.Add(row); JournalModalLayout.Navigation(navigation);
        if (changed) content.anchoredPosition = Vector2.zero;
        var focused = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
        if (options.Length > 0 && (changed || focused == null || !focused.transform.IsChildOf(root.transform))) rows[0].Select();
    }
    /// <summary>Creates one pooled, wrapping row with a binding-aware pointer button.</summary>
    private void AddRow(int index)
    {
        var row = JournalModalLayout.Rect("Choice " + (index + 1), content, new Rect(0, 0, 1, 1));
        var image = row.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.78f, .69f, .52f, .5f);
        var button = row.gameObject.AddComponent<JournalChoiceButton>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(.65f, .9f, .9f); colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color(.5f, .7f, .7f); button.colors = colors;
        row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 190;
        labels.Add(JournalModalLayout.Text("Name", row, new Rect(.025f, .51f, .95f, .43f), theme, 42));
        details.Add(JournalModalLayout.Text("Description", row, new Rect(.025f, .07f, .95f, .43f), theme, 32));
        button.onClick.AddListener(() => Choose(index)); rows.Add(button);
    }
    /// <summary>Consumes the current visual callback before invoking a potentially reentrant runtime choice.</summary>
    private void Choose(int index)
    {
        var action = select; if (action == null) return; select = null;
        foreach (var row in rows) row.interactable = false;
        action(index);
    }
    /// <summary>Dismisses the view without choosing, restoring gameplay focus only if this modal owns it.</summary>
    public override void Hide()
    { select = null; signature = null; generation++; if (root != null) host.Hide(root); }
}
