using UnityEngine;

/// <summary>Draws a hollow rounded edge without tinting the parchment, fields or separate chain hardware.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalCardOutline : UnityEngine.UI.MaskableGraphic
{
    [SerializeField] private float inset = 9;
    [SerializeField] private float thickness = 4;
    [SerializeField] private float radius = 20;
    [SerializeField] private bool useCornerRadii;
    [SerializeField] private Vector4 cornerRadii;

    /// <summary>Builds a rounded border with a transparent center, respecting Canvas masking.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = GetPixelAdjustedRect();
        float w = Mathf.Max(0, r.width - 2 * inset), h = Mathf.Max(0, r.height - 2 * inset);
        int steps = useCornerRadii ? 12 : 8;
        for (int corner = 0; corner < 4; corner++)
        {
            float outer = Mathf.Clamp(useCornerRadii ? cornerRadii[corner] : radius, 0, Mathf.Min(w, h) * .5f);
            float inner = Mathf.Max(0, outer - thickness);
            Vector2 center = r.center + new Vector2((corner == 0 || corner == 3 ? 1 : -1) * (w * .5f - outer),
                (corner < 2 ? 1 : -1) * (h * .5f - outer));
            for (int i = 0; i <= steps; i++)
            {
                float angle = (corner * 90f + i * 90f / steps) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                mesh.AddVert(center + direction * outer, color, Vector2.zero);
                mesh.AddVert(center + direction * inner, color, Vector2.zero);
            }
        }
        int count = 4 * (steps + 1);
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            mesh.AddTriangle(i * 2, next * 2, i * 2 + 1);
            mesh.AddTriangle(i * 2 + 1, next * 2, next * 2 + 1);
        }
    }
}
