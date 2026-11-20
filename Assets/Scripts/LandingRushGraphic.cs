using UnityEngine;
using UnityEngine.UI;

/// <summary>Reusable native UI mesh for score trails and payout sparks; it never intercepts input.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingRushGraphic : MaskableGraphic
{
    private Vector2 origin, destination;
    private float phase, payout, intensity;
    private bool multiply;
    private float impact;

    /// <summary>Updates a bounded, deterministic visual envelope without particle objects or gameplay randomness.</summary>
    public void Render(Vector3 fromWorld, Vector3 toWorld, float travel, float burst, float strength, bool multiplier = false, float arrival = 0)
    {
        origin = rectTransform.InverseTransformPoint(fromWorld); destination = rectTransform.InverseTransformPoint(toWorld);
        phase = travel; payout = burst; intensity = strength; multiply = multiplier; impact = arrival; SetVerticesDirty();
    }

    /// <summary>Builds a curved light trail plus a brief radial burst in canvas reference coordinates.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (phase > 0 && phase < 1)
        {
            var tint = multiply ? new Color(.65f, .86f, .76f) : new Color(1, .82f, .44f);
            Vector2 previous = Curve(Mathf.Max(0, phase - .34f));
            for (int i = 1; i <= 22; i++)
            {
                float t = Mathf.Lerp(Mathf.Max(0, phase - .34f), phase, i / 22f);
                // Keep the leading light readable against both the pale portrait and the dark score glass.
                Vector2 next = Curve(t); float alpha = i / 22f * Mathf.Lerp(.65f, 1, Mathf.Sin(phase * Mathf.PI));
                tint.a = alpha * .22f; Line(mesh, previous, next, 10 + intensity * 6, tint);
                tint.a = alpha; Line(mesh, previous, next, multiply ? 5 : 3.5f, tint); previous = next;
            }
            Diamond(mesh, Curve(phase), 7 + intensity * 4, new Color(1, .95f, .76f, Mathf.Lerp(.8f, 1, Mathf.Sin(phase * Mathf.PI))));
            Diamond(mesh, Curve(phase), 3, new Color(1, 1, .94f, 1));
        }
        if (impact > 0 && impact < 1)
        {
            float length = (multiply ? 72 : 35) * impact;
            Line(mesh, destination + new Vector2(-length, -28), destination + new Vector2(length, -28),
                3 * (1 - impact), new Color(1, .86f, .5f, (1 - impact) * .7f));
        }
        if (payout > 0 && payout < 1)
        {
            int count = 12 + Mathf.RoundToInt(intensity * 24);
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.399963f, distance = (35 + (i % 7) * 14) * payout;
                Vector2 point = destination + new Vector2(Mathf.Cos(angle) * distance * 1.6f, Mathf.Sin(angle) * distance - payout * payout * 36);
                Diamond(mesh, point, (2 + i % 3) * (1 - payout), new Color(1, .75f + (i % 3) * .07f, .3f, (1 - payout) * .85f));
            }
        }
    }

    /// <summary>Bends light above the timing track while retaining exact source and score destinations.</summary>
    private Vector2 Curve(float t)
    {
        Vector2 control = new Vector2((origin.x + destination.x) * .5f + 45, Mathf.Max(origin.y, destination.y) + 25);
        return (1 - t) * (1 - t) * origin + 2 * (1 - t) * t * control + t * t * destination;
    }
    /// <summary>Adds one tapered line segment to the existing UI mesh.</summary>
    private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f;
        Quad(mesh, a - normal, a + normal, b + normal, b - normal, tint);
    }
    /// <summary>Draws a tiny brass spark using the standard UI material.</summary>
    private static void Diamond(VertexHelper mesh, Vector2 center, float size, Color tint)
        => Quad(mesh, center + Vector2.up * size, center + Vector2.right * size, center + Vector2.down * size, center + Vector2.left * size, tint);
    /// <summary>Appends one colored quad without allocating a particle or material.</summary>
    private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero); mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
        mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
    }
}
