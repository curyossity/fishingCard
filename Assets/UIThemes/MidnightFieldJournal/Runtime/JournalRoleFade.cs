using UnityEngine;

/// <summary>Renders role dimming or soft inner illumination using the same asymmetric boundary.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalRoleFade : UnityEngine.UI.MaskableGraphic
{
    // Clockwise from top-right; the lower outside corner follows the master's larger decorative curve.
    [SerializeField] private Vector4 cornerRadii = new Vector4(24, 24, 66, 24);
    [SerializeField, Min(0)] private float borderInset = 6;
    [SerializeField] private bool softInnerGlow;
    [SerializeField, Min(1)] private float glowWidth = 24;

    /// <summary>Fills the panel or shades inward from its boundary without covering the surrounding parchment.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float inset = Mathf.Min(borderInset, Mathf.Min(rect.width, rect.height) * .5f);
        rect = Rect.MinMaxRect(rect.xMin + inset, rect.yMin + inset, rect.xMax - inset, rect.yMax - inset);
        if (softInnerGlow)
        {
            float width = Mathf.Min(glowWidth, Mathf.Min(rect.width, rect.height) * .5f);
            AddContour(mesh, rect, 0, 0);
            AddContour(mesh, rect, width * .15f, 1);
            AddContour(mesh, rect, width * .36f, .6f);
            AddContour(mesh, rect, width * .72f, .15f);
            AddContour(mesh, rect, width, 0);
            const int count = 52;
            for (int ring = 0; ring < 4; ring++)
                for (int i = 0; i < count; i++)
                {
                    int a = ring * count + i, b = ring * count + (i + 1) % count;
                    mesh.AddTriangle(a, b, a + count);
                    mesh.AddTriangle(a + count, b, b + count);
                }
            return;
        }
        mesh.AddVert(rect.center, color, Vector2.zero);
        AddContour(mesh, rect, 0, 1);
        const int boundaryCount = 52;
        for (int i = 0; i < boundaryCount; i++) mesh.AddTriangle(0, i + 1, (i + 1) % boundaryCount + 1);
    }

    /// <summary>Builds the shared contour; glow rings are inset derivatives of the unchanged fade boundary.</summary>
    private void AddContour(UnityEngine.UI.VertexHelper mesh, Rect outer, float depth, float opacity)
    {
        Rect rect = Rect.MinMaxRect(outer.xMin + depth, outer.yMin + depth, outer.xMax - depth, outer.yMax - depth);
        Color tint = color; tint.a *= opacity;
        const int steps = 12;
        for (int corner = 0; corner < 4; corner++)
        {
            float r = Mathf.Clamp(cornerRadii[corner] - depth, 0, Mathf.Min(rect.width, rect.height) * .5f);
            Vector2 center = rect.center + new Vector2((corner == 0 || corner == 3 ? 1 : -1) * (rect.width * .5f - r),
                (corner < 2 ? 1 : -1) * (rect.height * .5f - r));
            for (int i = 0; i <= steps; i++)
            {
                float angle = (corner * 90f + i * 90f / steps) * Mathf.Deg2Rad;
                mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r, tint, Vector2.zero);
            }
        }
    }
}
