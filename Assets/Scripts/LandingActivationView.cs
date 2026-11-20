using TMPro;
using UnityEngine;

/// <summary>Draws transient source feedback outside card faces and cabinet clipping masks.</summary>
public sealed class LandingActivationView : MonoBehaviour
{
    [SerializeField] private CanvasGroup outline;
    [SerializeField] private TMP_Text badge;
    [SerializeField] private UnityEngine.UI.Image badgeBackground;
    private RectTransform face, viewport, layer, rectangle;
    private readonly Vector3[] corners = new Vector3[4];

    /// <summary>Caches the pooled source's existing face and scroll viewport without taking control of either.</summary>
    public void Bind(TackleRigCardView card)
    {
        face = ((TackleCardTooltipSource)card.InspectionTarget).FaceBounds;
        viewport = card.GetComponentInParent<UnityEngine.UI.ScrollRect>()?.viewport;
        rectangle = (RectTransform)transform; layer = (RectTransform)transform.parent;
        badge.enableAutoSizing = true; badge.fontSizeMin = 18; badge.fontSizeMax = 25; badge.overflowMode = TextOverflowModes.Ellipsis;
    }
    /// <summary>Places a pulse or softer support outline only when the entire source face is visible.</summary>
    public void Render(float pulse, string amount, bool attempt, bool inspected, bool supporting)
    {
        bool active = face != null && face.gameObject.activeInHierarchy && (pulse > 0 || inspected || supporting);
        if (active && viewport != null)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, face);
            var clip = viewport.rect;
            active = bounds.min.y >= clip.yMin - 1 && bounds.max.y <= clip.yMax + 1 && bounds.min.x >= clip.xMin - 1 && bounds.max.x <= clip.xMax + 1;
        }
        gameObject.SetActive(active); if (!active) return;
        face.GetWorldCorners(corners);
        Vector2 min = layer.InverseTransformPoint(corners[0]), max = layer.InverseTransformPoint(corners[2]);
        rectangle.anchoredPosition = new Vector2(min.x - 3, max.y + 3); rectangle.sizeDelta = max - min + Vector2.one * 6;
        outline.alpha = inspected ? 1 : supporting ? .35f : pulse;
        badge.text = amount; badge.gameObject.SetActive(pulse > 0 && !string.IsNullOrEmpty(amount));
        badgeBackground.gameObject.SetActive(badge.gameObject.activeSelf);
        badgeBackground.color = attempt ? new Color32(33, 71, 71, 255) : new Color32(36, 29, 17, 255);
        badge.color = attempt ? new Color32(155, 221, 214, 255) : new Color32(233, 188, 99, 255);
    }
}
