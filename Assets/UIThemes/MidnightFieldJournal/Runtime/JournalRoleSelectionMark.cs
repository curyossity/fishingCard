using UnityEngine;

/// <summary>Draws a small selected-role check without relying on font glyph availability.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalRoleSelectionMark : UnityEngine.UI.MaskableGraphic
{
    /// <summary>Builds a crisp two-stroke check at any Canvas scale.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Stroke(mesh, new Vector2(.08f, .48f), new Vector2(.38f, .18f));
        Stroke(mesh, new Vector2(.38f, .18f), new Vector2(.94f, .86f));
    }

    /// <summary>Maps normalized check geometry to the authored role-header rectangle.</summary>
    private void Stroke(UnityEngine.UI.VertexHelper mesh, Vector2 from, Vector2 to)
    {
        Rect rect = GetPixelAdjustedRect();
        Vector2 a = rect.min + Vector2.Scale(from, rect.size), b = rect.min + Vector2.Scale(to, rect.size);
        Vector2 d = b - a, n = new Vector2(-d.y, d.x).normalized * Mathf.Min(rect.width, rect.height) * .065f;
        int start = mesh.currentVertCount;
        mesh.AddVert(a - n, color, Vector2.zero); mesh.AddVert(b - n, color, Vector2.zero);
        mesh.AddVert(b + n, color, Vector2.zero); mesh.AddVert(a + n, color, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
    }
}
