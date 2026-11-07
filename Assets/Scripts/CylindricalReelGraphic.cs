using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Bends a native sprite mesh around the same cylinder as its native text companions.</summary>
[RequireComponent(typeof(UnityEngine.UI.Image))]
public sealed class CylindricalReelGraphic : BaseMeshEffect
{
    private TackleReelFace face;
    private readonly List<UIVertex> source = new List<UIVertex>();

    /// <summary>Connects a presentation-only cylinder without modifying the sprite or shared material.</summary>
    public void Bind(TackleReelFace owner) { face = owner; graphic.SetVerticesDirty(); }

    /// <summary>Subdivides sprite triangles so their interior follows the curved surface rather than a flat quad.</summary>
    public override void ModifyMesh(VertexHelper mesh)
    {
        if (!IsActive() || face == null) return;
        source.Clear(); mesh.GetUIVertexStream(source); mesh.Clear();
        const int divisions = 12;
        for (int triangle = 0; triangle < source.Count; triangle += 3)
        {
            var a = source[triangle]; var b = source[triangle + 1]; var c = source[triangle + 2];
            for (int row = 0; row < divisions; row++)
                for (int column = 0; column < divisions - row; column++)
                {
                    AddTriangle(mesh, a, b, c, row, column, row + 1, column, row, column + 1, divisions);
                    if (column + row < divisions - 1)
                        AddTriangle(mesh, a, b, c, row + 1, column, row + 1, column + 1, row, column + 1, divisions);
                }
        }
    }

    /// <summary>Interpolates source UVs before projecting a small triangle, keeping illustration detail intact.</summary>
    private void AddTriangle(VertexHelper mesh, UIVertex a, UIVertex b, UIVertex c,
        int r0, int c0, int r1, int c1, int r2, int c2, int divisions)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(Vertex(a, b, c, r0, c0, divisions));
        mesh.AddVert(Vertex(a, b, c, r1, c1, divisions));
        mesh.AddVert(Vertex(a, b, c, r2, c2, divisions));
        mesh.AddTriangle(start, start + 1, start + 2);
    }

    /// <summary>Preserves source texture coordinates and applies cylinder lighting and front-face visibility.</summary>
    private UIVertex Vertex(UIVertex a, UIVertex b, UIVertex c, int row, int column, int divisions)
    {
        float u = (float)row / divisions, v = (float)column / divisions;
        UIVertex vertex = a;
        vertex.position = a.position * (1 - u - v) + b.position * u + c.position * v;
        vertex.uv0 = a.uv0 * (1 - u - v) + b.uv0 * u + c.uv0 * v;
        Color32 color = vertex.color;
        vertex.position = face.Project(transform, vertex.position, ref color);
        vertex.color = color;
        return vertex;
    }
}
