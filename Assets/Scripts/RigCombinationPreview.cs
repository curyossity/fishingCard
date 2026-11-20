using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Highlights inspected tray abilities and their exact committed/preview links outside supplied card artwork.</summary>
[DefaultExecutionOrder(100)]
public sealed class RigCombinationPreview : MonoBehaviour
{
    [SerializeField] private TackleCardTooltip tooltip;
    [SerializeField] private RectTransform overlay;
    [SerializeField] private RigRelationshipGlow glowTemplate;
    [SerializeField] private Color inspectedColor = new Color32(255, 212, 114, 255);
    [SerializeField] private Color supportingColor = new Color32(111, 232, 172, 255);
    [SerializeField] private Color disconnectedColor = new Color32(244, 138, 118, 255);
    private CardInstance[] committed = Array.Empty<CardInstance>(), projected = Array.Empty<CardInstance>();
    private IReadOnlyList<TackleRigCardView> cards = Array.Empty<TackleRigCardView>();
    private readonly List<RigRelationshipGlow> markers = new List<RigRelationshipGlow>();
    private readonly HashSet<int> connected = new HashSet<int>(), disconnected = new HashSet<int>();
    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform pendingPortrait;
    private TackleCardTooltipSource inspected;
    private int pendingId = -1;
    private bool dirty;
    private string explanation;
    private double revealedAt;
    public int InspectedId => inspected != null ? inspected.Member?.InstanceId ?? -1 : -1;
    public bool IsSupporting(int id) => connected.Contains(id);
    public bool IsDisconnected(int id) => disconnected.Contains(id);

    /// <summary>Replaces the entire comparison after a gap change; neither snapshot is mutated.</summary>
    public void Bind(CardInstance[] original, CardInstance[] preview, IReadOnlyList<TackleRigCardView> tray, RectTransform pending, int newId)
    {
        committed = original; projected = preview; cards = tray; pendingPortrait = pending; pendingId = newId; dirty = true;
    }
    /// <summary>Clears all transient identities and overlays at a phase boundary.</summary>
    private void OnDisable()
    {
        inspected = null; connected.Clear(); disconnected.Clear();
        foreach (var marker in markers) if (marker != null) marker.gameObject.SetActive(false);
    }
    /// <summary>Tracks the shared tooltip's pointer/focus lifecycle and existing scroll masks without taking input.</summary>
    private void LateUpdate()
    {
        var next = tooltip.Source;
        if (next != null && !cards.Any(c => c.gameObject.activeInHierarchy && c.InspectionTarget == next)) next = null;
        if (next != inspected || dirty)
        {
            if (next != inspected) revealedAt = Time.realtimeSinceStartupAsDouble;
            inspected = next; dirty = false; connected.Clear(); disconnected.Clear();
            if (inspected != null && inspected.Member != null)
            {
                int id = inspected.Member.InstanceId;
                var before = RigRuntime.InspectAbility(committed, id); var after = RigRuntime.InspectAbility(projected, id);
                connected.UnionWith(after.SupportingIds); connected.UnionWith(after.RecipientIds);
                disconnected.UnionWith(before.SupportingIds); disconnected.UnionWith(before.RecipientIds); disconnected.ExceptWith(connected);
                string text = "<color=#FFD472>Gold: inspected creature</color>";
                if (after.SupportingIds.Count > 0) text += "\n<color=#6FE8AC>Supports this ability: " + Names(after.SupportingIds) + "</color>";
                if (after.RecipientIds.Count > 0) text += "\n<color=#6FE8AC>Affected by this ability: " + Names(after.RecipientIds) + "</color>";
                if (disconnected.Count > 0) text += "\n<color=#F48A76>Disconnected by insertion: " + Names(disconnected) + "</color>";
                if (connected.Count == 0 && disconnected.Count == 0) text += "\nNo creature links in this position.";
                text += "\nPositional links; strike conditions still apply.";
                explanation = text;
            }
        }
        int used = 0;
        if (inspected != null)
        {
            tooltip.SetRelationshipContext(inspected, explanation);
            foreach (var card in cards)
            {
                if (!card.gameObject.activeInHierarchy || card.BoundInstance == null) continue;
                int id = card.BoundInstance.InstanceId;
                if (id != InspectedId && !connected.Contains(id) && !disconnected.Contains(id)) continue;
                var source = (TackleCardTooltipSource)card.InspectionTarget;
                Draw(source.FaceBounds, card.GetComponentInParent<UnityEngine.UI.ScrollRect>()?.viewport,
                    id == InspectedId ? inspectedColor : connected.Contains(id) ? supportingColor : disconnectedColor, ref used);
            }
            if (connected.Contains(pendingId)) Draw(pendingPortrait, null, supportingColor, ref used);
        }
        for (int i = used; i < markers.Count; i++) markers[i].gameObject.SetActive(false);
    }
    /// <summary>Names offscreen and newly landed links in the tooltip so scrolling never hides their identities.</summary>
    private string Names(IEnumerable<int> ids)
    {
        var names = ids.Select(id => projected.Concat(committed).First(m => m.InstanceId == id).Definition.DisplayName).Distinct().ToArray();
        return string.Join(", ", names.Take(3)) + (names.Length > 3 ? " (+" + (names.Length - 3) + " more)" : "");
    }

    /// <summary>Places a pooled feathered glow around fully visible faces, revealing once without continuous flashing.</summary>
    private void Draw(RectTransform face, RectTransform viewport, Color color, ref int used)
    {
        if (face == null) return;
        if (viewport != null)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, face); var clip = viewport.rect;
            if (bounds.min.x < clip.xMin - 1 || bounds.max.x > clip.xMax + 1 || bounds.min.y < clip.yMin - 1 || bounds.max.y > clip.yMax + 1) return;
        }
        if (used == markers.Count) markers.Add(Instantiate(glowTemplate, overlay));
        var marker = markers[used++]; face.GetWorldCorners(corners);
        Vector2 min = overlay.InverseTransformPoint(corners[0]), max = overlay.InverseTransformPoint(corners[2]);
        marker.rectTransform.anchoredPosition = new Vector2(min.x - RigRelationshipGlow.Feather, max.y + RigRelationshipGlow.Feather);
        marker.rectTransform.sizeDelta = max - min + Vector2.one * (RigRelationshipGlow.Feather * 2);
        float reveal = Mathf.Clamp01((float)(Time.realtimeSinceStartupAsDouble - revealedAt) / .14f);
        color.a *= 1 - Mathf.Pow(1 - reveal, 3);
        marker.color = color; marker.gameObject.SetActive(true);
    }
}
