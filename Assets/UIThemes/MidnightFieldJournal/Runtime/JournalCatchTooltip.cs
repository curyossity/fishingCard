using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Displays live attachment tags and active-role rules over the approved pointer-free engraved master.</summary>
public sealed class JournalCatchTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text creatureName, abilityName, description;
    [SerializeField] private RectTransform tagsRoot;
    [SerializeField] private TMP_Text tagPrototype;
    [SerializeField] private GameObject catchIcon, baitIcon;
    private readonly List<TMP_Text> tags = new List<TMP_Text>();
    private readonly Vector3[] corners = new Vector3[4];
    public string DisplayedName => creatureName.text;
    public string DisplayedAbility => abilityName.text;
    public string DisplayedDescription => description.text;

    /// <summary>Reads effective tags and the current role only; no hidden value, opposite-role rules or runtime internals are printed.</summary>
    public void Bind(CardInstance instance)
    {
        CardDefinition card = instance?.Definition;
        if (card == null) { gameObject.SetActive(false); return; }
        creatureName.text = card.DisplayName.ToUpperInvariant();
        catchIcon.SetActive(!instance.IsBait); baitIcon.SetActive(instance.IsBait);
        var names = new List<string>();
        foreach (var effect in instance.ActiveEffects)
            if (effect != null && !string.IsNullOrWhiteSpace(effect.DisplayName) && !names.Contains(effect.DisplayName)) names.Add(effect.DisplayName);
        string heading = string.Join(" / ", names);
        string rules = instance.IsBait ? card.BaitRulesText : card.RulesText;
        if (!string.IsNullOrEmpty(heading) && (rules ?? "").StartsWith(heading + ":", StringComparison.OrdinalIgnoreCase)) rules = rules.Substring(heading.Length + 1).Trim();
        bool empty = names.Count == 0 && (string.IsNullOrWhiteSpace(rules) || card.GetEffects(instance.AttachmentRole).Length > 0);
        abilityName.text = empty ? "No effect" : heading;
        description.text = empty ? string.Empty : rules ?? string.Empty;
        string[] currentTags = instance.Tags ?? Array.Empty<string>();
        while (tags.Count < currentTags.Length)
        {
            var pill = Instantiate(tagPrototype.transform.parent.gameObject, tagsRoot);
            pill.name = "Effective Tag " + tags.Count; tags.Add(pill.GetComponentInChildren<TMP_Text>(true));
        }
        float total = 0;
        for (int i = 0; i < tags.Count; i++)
        {
            var tag = tags[i]; tag.transform.parent.gameObject.SetActive(i < currentTags.Length); if (i >= currentTags.Length) continue;
            tag.text = currentTags[i]; tag.ForceMeshUpdate();
            float w = Mathf.Max(100, tag.preferredWidth + 36); ((RectTransform)tag.transform.parent).sizeDelta = new Vector2(w, 46); total += w + 14;
        }
        float x = -(total - 14) * .5f;
        float scale = Mathf.Min(1, tagsRoot.rect.width / Mathf.Max(1, total));
        tagsRoot.localScale = new Vector3(scale, scale, 1);
        for (int i = 0; i < currentTags.Length; i++) { var rect = (RectTransform)tags[i].transform.parent; rect.anchoredPosition = new Vector2(x + rect.sizeDelta.x * .5f, 0); x += rect.sizeDelta.x + 14; }
    }

    /// <summary>Flips beside its source and clamps the entire transparent canvas inside the tooltip layer.</summary>
    public void Place(RectTransform source)
    {
        var rect = (RectTransform)transform; var parent = (RectTransform)rect.parent;
        Canvas canvas = parent.GetComponentInParent<Canvas>(); Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        source.GetWorldCorners(corners);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(camera, corners[2]), camera, out Vector2 right);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(camera, corners[0]), camera, out Vector2 left);
        Rect available = parent.rect;
        float scale = Mathf.Min(1, Mathf.Min((available.width - 24) / 900, (available.height - 24) / 640));
        rect.localScale = Vector3.one * Mathf.Max(.1f, scale); float w = 900 * scale, h = 640 * scale;
        float x = right.x + 12 + w * .5f;
        if (x + w * .5f > available.xMax - 12) x = left.x - 12 - w * .5f;
        rect.anchoredPosition = new Vector2(Mathf.Clamp(x, available.xMin + w * .5f + 12, available.xMax - w * .5f - 12), Mathf.Clamp((left.y + right.y) * .5f, available.yMin + h * .5f + 12, available.yMax - h * .5f - 12));
        rect.SetAsLastSibling();
    }
}
