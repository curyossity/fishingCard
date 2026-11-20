using UnityEngine;
using UnityEngine.UI;

/// <summary>A feathered native UI halo with a transparent center, leaving supplied card artwork readable.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class RigRelationshipGlow : MaskableGraphic
{
    public const float Feather = 28;
    private const int Bands = 14, CornerSegments = 8, Perimeter = 4 * (CornerSegments + 1);

    /// <summary>Builds a soft rounded falloff outside the face without a solid border, sprite or bloom dependency.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        var outer = rectTransform.rect;
        var face = new Rect(outer.xMin + Feather, outer.yMin + Feather, outer.width - Feather * 2, outer.height - Feather * 2);
        if (face.width <= 0 || face.height <= 0) return;
        for (int band = 0; band <= Bands; band++)
        {
            float distance = Mathf.Lerp(-3, Feather, band / (float)Bands);
            float alpha = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-3, 1, distance))
                * Mathf.Pow(1 - Mathf.Max(0, distance) / Feather, 2) * .62f;
            var tint = color; tint.a *= alpha;
            float radius = Mathf.Min(12, Mathf.Min(face.width, face.height) * .25f);
            for (int corner = 0; corner < 4; corner++)
            {
                Vector2 center = new Vector2(corner == 0 || corner == 3 ? face.xMax - radius : face.xMin + radius,
                    corner < 2 ? face.yMax - radius : face.yMin + radius);
                for (int segment = 0; segment <= CornerSegments; segment++)
                {
                    float angle = (corner * 90 + segment * 90f / CornerSegments) * Mathf.Deg2Rad;
                    Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius + distance);
                    mesh.AddVert(point, tint, Vector2.zero);
                }
            }
        }
        for (int band = 0; band < Bands; band++)
            for (int point = 0; point < Perimeter; point++)
            {
                int a = band * Perimeter + point, b = band * Perimeter + (point + 1) % Perimeter;
                mesh.AddTriangle(a, b, b + Perimeter); mesh.AddTriangle(a, b + Perimeter, a + Perimeter);
            }
    }
}
