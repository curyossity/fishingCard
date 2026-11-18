using UnityEngine;

/// <summary>Draws an invisible stencil aperture whose convex contour follows the supplied reel rim.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ReelApertureMask : UnityEngine.UI.MaskableGraphic
{
    [SerializeField] private Vector2[] contour = System.Array.Empty<Vector2>();

    /// <summary>Stores native-art normalized points, with Y measured downward from the texture's top edge.</summary>
    public void Configure(Vector2[] points)
    {
        contour = (Vector2[])points.Clone(); raycastTarget = false; SetVerticesDirty();
    }

    /// <summary>Triangulates the convex opening without changing any reel dimensions or generating a texture.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        if (contour.Length < 3) return;
        Rect rect = rectTransform.rect;
        Vector2 center = Vector2.zero;
        foreach (var point in contour) center += point;
        center /= contour.Length;
        AddVertex(mesh, rect, center);
        foreach (var point in contour) AddVertex(mesh, rect, point);
        for (int i = 0; i < contour.Length; i++) mesh.AddTriangle(0, i + 1, (i + 1) % contour.Length + 1);
    }

    /// <summary>Maps the authored source contour onto the current unscaled reel rectangle.</summary>
    private void AddVertex(UnityEngine.UI.VertexHelper mesh, Rect rect, Vector2 point)
    {
        mesh.AddVert(new Vector3(rect.xMin + point.x * rect.width, rect.yMax - point.y * rect.height), color, Vector2.zero);
    }
}
