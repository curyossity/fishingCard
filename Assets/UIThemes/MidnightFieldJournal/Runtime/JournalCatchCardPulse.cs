using UnityEngine;

/// <summary>Gives one attached card a gentle centered pop and warm flash without moving its chain hardware.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalCatchCardPulse : UnityEngine.UI.MaskableGraphic
{
    [SerializeField, Min(.1f)] private float duration = .38f;
    [SerializeField, Range(0, .1f)] private float enlargement = .045f;
    [SerializeField, Range(0, .4f)] private float flashOpacity = .18f;
    private RectTransform motionRoot;
    private float elapsed;
    public bool IsPlaying { get; private set; }
    public float MotionScale => motionRoot == null ? 1 : motionRoot.localScale.x;

    /// <summary>Wraps only the card visuals once, leaving its layout, input root and independent connectors stationary.</summary>
    public static JournalCatchCardPulse GetOrCreate(CompactCatchCardView card)
    {
        var existing = card.GetComponentInChildren<JournalCatchCardPulse>(true);
        if (existing != null) return existing;
        Transform owner = card.transform;
        var children = new Transform[owner.childCount];
        for (int i = 0; i < children.Length; i++) children[i] = owner.GetChild(i);
        var visual = new GameObject("Card Pulse Visuals", typeof(RectTransform));
        visual.layer = card.gameObject.layer;
        var root = (RectTransform)visual.transform; root.SetParent(owner, false);
        Stretch(root);
        foreach (Transform child in children) child.SetParent(root, false);
        var overlay = new GameObject("Warm Ivory Flash", typeof(RectTransform), typeof(CanvasRenderer), typeof(JournalCatchCardPulse));
        overlay.layer = card.gameObject.layer; overlay.transform.SetParent(root, false);
        Stretch((RectTransform)overlay.transform);
        var pulse = overlay.GetComponent<JournalCatchCardPulse>(); pulse.motionRoot = root;
        pulse.raycastTarget = false; pulse.Stop();
        return pulse;
    }

    /// <summary>Starts one non-repeating prelude after its recipient has scrolled into view.</summary>
    public void Begin() { elapsed = 0; IsPlaying = true; Apply(0); }

    /// <summary>Restores exact resting visuals on completion, cancellation, removal or run shutdown.</summary>
    public void Stop()
    {
        IsPlaying = false;
        if (motionRoot != null) motionRoot.localScale = Vector3.one;
        color = new Color(1, .94f, .79f, 0);
    }

    /// <summary>Uses unscaled time, matching number feedback even while game time is paused.</summary>
    private void Update()
    {
        if (!IsPlaying) return;
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        if (t >= 1) Stop(); else Apply(t);
    }

    /// <summary>Rises and falls smoothly once; the flash stays soft and does not recolour benefit or harm.</summary>
    private void Apply(float t)
    {
        float pulse = Mathf.Sin(Mathf.PI * t);
        if (motionRoot != null) motionRoot.localScale = Vector3.one * (1 + enlargement * pulse);
        color = new Color(1, .94f, .79f, flashOpacity * pulse);
    }

    /// <summary>Fills a rounded parchment silhouette with transparent outside corners and normal Canvas masking.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear(); Rect r = GetPixelAdjustedRect();
        const float inset = 3;
        r = Rect.MinMaxRect(r.xMin + inset, r.yMin + inset, r.xMax - inset, r.yMax - inset);
        float radius = Mathf.Min(20, Mathf.Min(r.width, r.height) * .5f);
        mesh.AddVert(r.center, color, Vector2.zero);
        const int steps = 8;
        for (int corner = 0; corner < 4; corner++)
        {
            Vector2 center = r.center + new Vector2((corner == 0 || corner == 3 ? 1 : -1) * (r.width * .5f - radius),
                (corner < 2 ? 1 : -1) * (r.height * .5f - radius));
            for (int i = 0; i <= steps; i++)
            {
                float angle = (corner * 90 + i * 90f / steps) * Mathf.Deg2Rad;
                mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, color, Vector2.zero);
            }
        }
        const int count = 4 * (steps + 1);
        for (int i = 0; i < count; i++) mesh.AddTriangle(0, i + 1, (i + 1) % count + 1);
    }

    /// <summary>Keeps visual motion centered independently of the top-anchored chain layout.</summary>
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    /// <summary>Never leaves a pooled or hidden card enlarged or illuminated.</summary>
    protected override void OnDisable() { Stop(); base.OnDisable(); }
}
