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
    private float charge, impactStrength;
    private bool impactMultiply;

    /// <summary>Samples independent charge, flight, arrival and payout strengths without particle objects or gameplay randomness.</summary>
    public void Render(Vector3 fromWorld, Vector3 toWorld, float travel, float burst, float strength, bool multiplier = false, float arrival = 0,
        float preparation = 0, float arrivalStrength = -1, bool arrivalMultiplier = false)
    {
        origin = rectTransform.InverseTransformPoint(fromWorld); destination = rectTransform.InverseTransformPoint(toWorld);
        phase = travel; payout = burst; intensity = strength; multiply = multiplier; impact = arrival;
        charge = preparation; impactStrength = arrivalStrength < 0 ? strength : arrivalStrength;
        impactMultiply = arrivalStrength < 0 ? multiplier : arrivalMultiplier; SetVerticesDirty();
    }

    /// <summary>Builds contribution-weighted comets and localized arrivals while retaining the existing payout burst.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (charge > 0)
        {
            Color tint = multiply ? new Color(.65f, .90f, .83f, charge * .7f) : new Color(1, .82f, .44f, charge * .55f);
            float distance = 12 + (1 - charge) * 32;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 at = origin + new Vector2(side * distance, multiply ? side * 8 : -10);
                Line(mesh, at - Vector2.right * side * 10, at, 2 + charge * 2, tint);
                Diamond(mesh, at, 2 + charge * 3, tint);
            }
        }
        if (phase > 0 && phase < 1)
        {
            var tint = multiply ? new Color(.65f, .86f, .76f) : new Color(1, .82f, .44f);
            float tail = .20f + intensity * .28f;
            Vector2 previous = Curve(Mathf.Max(0, phase - tail));
            for (int i = 1; i <= 22; i++)
            {
                float t = Mathf.Lerp(Mathf.Max(0, phase - tail), phase, i / 22f);
                // Keep the leading light readable against both the pale portrait and the dark score glass.
                Vector2 next = Curve(t); float alpha = i / 22f * Mathf.Lerp(.65f, 1, Mathf.Sin(phase * Mathf.PI));
                tint.a = alpha * .20f; Line(mesh, previous, next, 7 + intensity * 16, tint);
                tint.a = alpha; Line(mesh, previous, next, (multiply ? 3.5f : 2.5f) + intensity * 4, tint);
                if (multiply && intensity > .3f)
                {
                    Vector2 offset = new Vector2(0, 5 + intensity * 3);
                    tint.a = alpha * intensity * .65f; Line(mesh, previous + offset, next + offset, 2, tint);
                    Line(mesh, previous - offset, next - offset, 2, tint);
                }
                previous = next;
            }
            Diamond(mesh, Curve(phase), 5 + intensity * 9, new Color(1, .95f, .76f, Mathf.Lerp(.8f, 1, Mathf.Sin(phase * Mathf.PI))));
            Diamond(mesh, Curve(phase), 2 + intensity * 2, new Color(1, 1, .94f, 1));
        }
        if (impact > 0 && impact < 1)
        {
            float length = (22 + impactStrength * (impactMultiply ? 105 : 80)) * impact;
            Line(mesh, destination + new Vector2(-length, -28), destination + new Vector2(length, -28),
                (2 + impactStrength * 3) * (1 - impact), new Color(1, .86f, .5f, (1 - impact) * .7f));
            if (impactStrength > .18f)
            {
                Color tint = impactMultiply ? new Color(.65f, .90f, .83f) : new Color(1, .82f, .44f);
                Glow(mesh, destination, new Vector2(45 + impactStrength * 75, 28 + impactStrength * 20),
                    new Color(tint.r, tint.g, tint.b, (1 - impact) * impactStrength * .20f));
                int count = 4 + Mathf.RoundToInt(impactStrength * 8);
                for (int i = 0; i < count; i++)
                {
                    float side = i % 2 == 0 ? -1 : 1;
                    Vector2 start = destination + new Vector2(side * 14, -18);
                    Vector2 velocity = new Vector2(side * (30 + i % 4 * 16), (i % 3 - 1) * 25);
                    Vector2 point = start + velocity * impact + Vector2.down * (impact * impact * 14);
                    tint.a = (1 - impact) * .85f;
                    Line(mesh, point - velocity.normalized * (3 + impactStrength * 6) * (1 - impact), point, 2, tint);
                }
            }
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
        Vector2 control = new Vector2((origin.x + destination.x) * .5f + 45, Mathf.Max(origin.y, destination.y) + 25 + intensity * 35);
        return (1 - t) * (1 - t) * origin + 2 * (1 - t) * t * control + t * t * destination;
    }
    /// <summary>Feathers the score-glass arrival accent to transparent edges without a new material.</summary>
    private static void Glow(VertexHelper mesh, Vector2 center, Vector2 radius, Color tint)
    {
        for (int i = 0; i < 20; i++)
        {
            float a = i * Mathf.PI * 2 / 20, b = (i + 1) * Mathf.PI * 2 / 20;
            int first = mesh.currentVertCount; Color edge = tint; edge.a = 0;
            mesh.AddVert(center, tint, Vector2.zero);
            mesh.AddVert(center + Vector2.Scale(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), radius), edge, Vector2.zero);
            mesh.AddVert(center + Vector2.Scale(new Vector2(Mathf.Cos(b), Mathf.Sin(b)), radius), edge, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
        }
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
