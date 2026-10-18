using UnityEngine;

/// <summary>Draws an independent journal-style rarity hook with native UI geometry.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalRarityHook : UnityEngine.UI.MaskableGraphic
{
    /// <summary>Builds the hook's eye, stem, curve and barb without copying old icon artwork.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Vector2 last = new Vector2(.58f, .73f);
        for (int i = 1; i <= 18; i++)
        {
            float a = i * 360f / 18 * Mathf.Deg2Rad;
            Vector2 point = new Vector2(.58f + .11f * Mathf.Sin(a), .84f - .11f * Mathf.Cos(a));
            Stroke(mesh, last, point); last = point;
        }
        Stroke(mesh, new Vector2(.58f, .73f), new Vector2(.58f, .30f));
        last = new Vector2(.58f, .30f);
        for (int i = 1; i <= 16; i++)
        {
            float a = i * 190f / 16 * Mathf.Deg2Rad;
            Vector2 point = new Vector2(.39f + .19f * Mathf.Cos(a), .30f - .19f * Mathf.Sin(a));
            Stroke(mesh, last, point); last = point;
        }
        Stroke(mesh, last, new Vector2(.28f, .40f));
    }

    /// <summary>Adds one scalable stroked segment to the hook mesh.</summary>
    private void Stroke(UnityEngine.UI.VertexHelper mesh, Vector2 a, Vector2 b)
    {
        Rect rect = GetPixelAdjustedRect(); a = rect.min + Vector2.Scale(a, rect.size); b = rect.min + Vector2.Scale(b, rect.size);
        Vector2 n = new Vector2(-(b - a).y, (b - a).x).normalized * rect.width * .035f;
        int v = mesh.currentVertCount;
        mesh.AddVert(a - n, color, Vector2.zero); mesh.AddVert(a + n, color, Vector2.zero);
        mesh.AddVert(b + n, color, Vector2.zero); mesh.AddVert(b - n, color, Vector2.zero);
        mesh.AddTriangle(v, v + 1, v + 2); mesh.AddTriangle(v, v + 2, v + 3);
    }
}
