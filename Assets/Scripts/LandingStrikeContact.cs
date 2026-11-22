using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Stages compression, glass illumination and a quality stamp at the accepted strike without moving aiming indicators.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingStrikeContact : MaskableGraphic
{
    [SerializeField] private TackleButtonFeedback strikeButton;
    [SerializeField] private CanvasGroup stamp;
    [SerializeField] private TMP_Text stampFace, stampShadow;
    [SerializeField] private LandingStrikeCelebration celebration;
    private LandingAccuracy accuracy;
    private float position, contactAt, age;
    private bool active, reduced;
    private string quality;
    private decimal shownPoints;
    public bool IsContactActive => active;
    public float StrikePosition => position;
    public float ContactAt => contactAt;
    public Vector2 RecoilOffset { get; private set; }

    /// <summary>Starts only for a newly accepted receipt, sharing the accuracy sound's existing impact deadline.</summary>
    public void Begin(LandingStrikeReceipt receipt, float accuracyAt)
    {
        Clear();
        if (receipt?.Score == null) return;
        accuracy = receipt.Score.Accuracy; position = Mathf.Clamp01((float)receipt.FishPosition);
        contactAt = accuracy == LandingAccuracy.Miss ? 0 : accuracyAt;
        quality = accuracy.ToString().ToUpperInvariant(); shownPoints = decimal.MinValue;
        if (stampFace != null) { stampFace.text = quality; stampFace.color = accuracy == LandingAccuracy.Miss ? (Color)new Color32(225, 218, 196, 255) : new Color32(53, 37, 17, 255); }
        if (stampShadow != null) stampShadow.text = quality;
        active = true;
    }

    /// <summary>Samples the score owner's unscaled clock; reduced motion retains a stationary contact without recoil or button movement.</summary>
    public void Render(float elapsed, bool reducedMotion, decimal deliveredPoints)
    {
        if (!active) return;
        age = Mathf.Max(0, elapsed); reduced = reducedMotion;
        if (age >= .78f) { Clear(); return; }
        if (shownPoints != deliveredPoints)
        {
            shownPoints = deliveredPoints;
            string text = quality + (deliveredPoints > 0 ? " +" + LandingScoreRush.Format(deliveredPoints) : "");
            if (stampFace != null) stampFace.text = text;
            if (stampShadow != null) stampShadow.text = text;
        }
        float press = age < .045f ? 1 : 1 - Mathf.Clamp01((age - .045f) / .095f);
        if (strikeButton != null) strikeButton.SetActionPress(reduced ? 0 : press * press);
        float impactAge = age - contactAt;
        float kick = impactAge < 0 || impactAge > .23f ? 0 : Mathf.Exp(-impactAge * 24) * Mathf.Cos(impactAge * 35);
        float force = accuracy == LandingAccuracy.Perfect ? 5 : accuracy == LandingAccuracy.Good ? 2.5f : -.65f;
        RecoilOffset = reduced ? Vector2.zero : Vector2.up * (kick * force);
        RenderStamp(impactAge);
        SetVerticesDirty();
    }

    /// <summary>Restores the button face and removes the overlay on skip, interruption and phase exit.</summary>
    public void Clear()
    {
        active = false; RecoilOffset = Vector2.zero;
        if (strikeButton != null) strikeButton.SetActionPress(0);
        if (stamp != null) { stamp.alpha = 0; stamp.transform.localScale = Vector3.one; }
        if (celebration != null) celebration.Clear();
        SetVerticesDirty();
    }

    /// <summary>Releases only owned presentation state when the track disappears.</summary>
    protected override void OnDisable() { Clear(); base.OnDisable(); }

    /// <summary>Samples the result ribbon above the track, projecting the water origin independently to the frozen hit position.</summary>
    private void RenderStamp(float time)
    {
        if (stamp == null) return;
        float duration = accuracy == LandingAccuracy.Perfect ? .54f : accuracy == LandingAccuracy.Good ? .46f : .25f;
        stamp.alpha = time < 0 ? 0 : 1 - Mathf.Clamp01((time - duration + .13f) / .13f);
        Rect bounds = rectTransform.rect;
        float enter = 1 - Mathf.Pow(1 - Mathf.Clamp01(time / .065f), 3);
        float fullScale = accuracy == LandingAccuracy.Perfect ? 1 : accuracy == LandingAccuracy.Good ? .82f : .68f;
        float scale = fullScale * (time < .065f ? Mathf.Lerp(.72f, 1.08f, enter) : 1 + .08f * Mathf.Exp(-(time - .065f) * 28) * Mathf.Cos((time - .065f) * 36));
        var stampRect = (RectTransform)stamp.transform;
        var parent = (RectTransform)stampRect.parent;
        Vector2 local = StampLocalPoint(new Vector3(bounds.center.x, bounds.yMax + 48 - (reduced ? 0 : (1 - enter) * 9)), parent);
        Vector2 anchor = parent.rect.min + Vector2.Scale(parent.rect.size, stampRect.anchorMin);
        stampRect.anchoredPosition = local - anchor;
        Vector3 sizing = reduced ? Vector3.one * fullScale : new Vector3(scale, scale * Mathf.Lerp(.82f, 1, enter), 1);
        stamp.transform.localScale = sizing;
        Vector2 hit = StampLocalPoint(new Vector3(Mathf.Lerp(bounds.xMin, bounds.xMax, position), bounds.center.y), parent);
        Vector2 offset = hit - local;
        if (celebration != null) celebration.Render(accuracy, time, reduced, new Vector2(offset.x / sizing.x, offset.y / sizing.y));
    }

    /// <summary>Projects into the shared stage without world-space inversion, which is undefined while a newly enabled canvas has zero scale.</summary>
    private Vector3 StampLocalPoint(Vector3 point, Transform parent)
    {
        for (Transform cursor = rectTransform; cursor != null && cursor != parent; cursor = cursor.parent)
            point = cursor.localPosition + cursor.localRotation * Vector3.Scale(point, cursor.localScale);
        return point;
    }

    /// <summary>Builds a compression-and-release impact with soft reflections, short pressure fronts and a few deterministic brass flecks.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (!active) return;
        Rect bounds = rectTransform.rect;
        Vector2 center = new Vector2(Mathf.Lerp(bounds.xMin, bounds.xMax, position), bounds.center.y);
        bool perfect = accuracy == LandingAccuracy.Perfect, miss = accuracy == LandingAccuracy.Miss;
        float t = age - contactAt;
        Color tint = Tint();
        if (reduced)
        {
            if (t < 0 || t > .25f) return;
            tint.a = .85f * (1 - Mathf.Clamp01((t - .16f) / .09f));
            // A fixed shape retains grade distinction without the flash, travelling fronts or stamped motion.
            if (!miss) Line(mesh, center + Vector2.down * 30, center + Vector2.up * 30, perfect ? 4 : 2, tint);
            for (int side = -1; side <= 1; side += 2)
            {
                Line(mesh, center + new Vector2(side * 18, -10), center + new Vector2(side * 29, -5), 2, tint);
                if (perfect) Line(mesh, center + new Vector2(side * 18, 10), center + new Vector2(side * 29, 5), 2, tint);
            }
            return;
        }
        if (t < 0)
        {
            float compression = Mathf.Clamp01(age / Mathf.Max(.001f, contactAt));
            float gap = Mathf.Lerp(54, 12, compression * compression);
            tint.a = .25f + .40f * compression;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 at = center + Vector2.right * (side * gap);
                Line(mesh, at + Vector2.down * 23, at + Vector2.up * 23, 2, tint);
                Glow(mesh, at, new Vector2(14, 66), Alpha(tint, .12f));
            }
            return;
        }
        float duration = perfect ? .29f : miss ? .22f : .24f;
        if (t >= duration) return;
        float progress = Mathf.Clamp01(t / duration);
        float fade = (1 - progress) * (1 - progress);
        float travel = 1 - Mathf.Pow(1 - progress, 3);
        float height = bounds.height * .43f;
        if (miss)
        {
            center.y -= travel * 12;
            for (int side = -1; side <= 1; side += 2)
            {
                float spread = 14 + travel * 22;
                Line(mesh, center + new Vector2(side * spread, -6), center + new Vector2(side * (spread + 14), -10), 3, Alpha(tint, fade * .7f));
            }
            return;
        }
        float flash = Mathf.Pow(1 - Mathf.Clamp01(t / .085f), 2);
        if (perfect) tint = new Color(.42f, .90f, 1);
        // A broad reflection makes the whole glass acknowledge impact, while the hot core marks the true position.
        Glow(mesh, center, new Vector2(perfect ? 200 : 130, bounds.height * 1.5f), Alpha(tint, flash * (perfect ? .40f : .26f)));
        Glow(mesh, center, new Vector2(perfect ? 48 : 30, bounds.height), Alpha(tint, flash * .65f));
        Line(mesh, center + Vector2.down * height, center + Vector2.up * height, perfect ? 5 : 3, new Color(1, .98f, .84f, flash));
        Line(mesh, center + Vector2.left * 20, center + Vector2.right * 20, 3, new Color(1, .98f, .84f, flash * .8f));
        float distance = 12 + travel * (perfect ? 175 : 115);
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 at = center + Vector2.right * (side * distance);
            Glow(mesh, center + Vector2.right * (side * distance * .5f), new Vector2(distance, height * 1.8f), Alpha(tint, fade * .16f));
            Glow(mesh, at, new Vector2(24, height * 2.1f), Alpha(tint, fade * .30f));
            Line(mesh, at + new Vector2(-side * 8, -height * .75f), at, perfect ? 3 : 2, Alpha(tint, fade * .85f));
            Line(mesh, at, at + new Vector2(-side * 8, height * .75f), perfect ? 3 : 2, Alpha(tint, fade * .85f));
            // Reflections run along the upper and lower glass edge rather than across any printed fields.
            for (int edge = -1; edge <= 1; edge += 2)
                Line(mesh, at + new Vector2(-side * 18, edge * height), at + new Vector2(side * 10, edge * height), 2, Alpha(tint, fade * .6f));
        }
    }

    /// <summary>Returns the grade's material accent; shape and motion also distinguish outcomes.</summary>
    private Color Tint() => accuracy == LandingAccuracy.Perfect ? new Color(1, .84f, .47f) : accuracy == LandingAccuracy.Miss ? new Color(.78f, .57f, .45f) : new Color(.65f, .86f, .76f);

    /// <summary>Preserves an accent's RGB while assigning an independent reflection envelope.</summary>
    private static Color Alpha(Color tint, float alpha) { tint.a = alpha; return tint; }

    /// <summary>Feathers a rectangular glass reflection to transparent edges using a small shared-material mesh.</summary>
    private static void Glow(VertexHelper mesh, Vector2 center, Vector2 size, Color tint)
    {
        int first = mesh.currentVertCount;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
            {
                float px = x == 0 ? -.5f : x == 1 ? -.18f : x == 2 ? .18f : .5f;
                float py = y == 0 ? -.5f : y == 1 ? -.18f : y == 2 ? .18f : .5f;
                Color vertex = tint; if (x == 0 || x == 3 || y == 0 || y == 3) vertex.a = 0;
                mesh.AddVert(center + new Vector2(px * size.x, py * size.y), vertex, Vector2.zero);
            }
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
            {
                int corner = first + y * 4 + x;
                mesh.AddTriangle(corner, corner + 4, corner + 5); mesh.AddTriangle(corner, corner + 5, corner + 1);
            }
    }

    /// <summary>Appends one narrow UI stroke using the existing canvas material and clip region.</summary>
    private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
    {
        Vector2 normal = new Vector2(from.y - to.y, to.x - from.x).normalized * (width * .5f);
        int first = mesh.currentVertCount;
        mesh.AddVert(from - normal, tint, Vector2.zero); mesh.AddVert(from + normal, tint, Vector2.zero);
        mesh.AddVert(to + normal, tint, Vector2.zero); mesh.AddVert(to - normal, tint, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
    }
}
