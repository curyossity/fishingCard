using UnityEngine;
using UnityEngine.UI;

/// <summary>Reusable native UI mesh for score trails and payout sparks; it never intercepts input.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingRushGraphic : MaskableGraphic
{
    private Vector2 origin, destination;
    private float phase, payout, intensity, impact;
    private bool impactMultiplier, flyingMultiplier;

    /// <summary>Updates a bounded, deterministic visual envelope without particle objects or gameplay randomness.</summary>
    public void Render(Vector3 fromWorld, Vector3 toWorld, float travel, float burst, float strength, float ripple = 0, bool multiplyImpact = false, bool multiplyFlight = false)
    {
        origin = rectTransform.InverseTransformPoint(fromWorld); destination = rectTransform.InverseTransformPoint(toWorld);
        phase = travel; payout = burst; intensity = strength; impact = ripple; impactMultiplier = multiplyImpact; flyingMultiplier = multiplyFlight; SetVerticesDirty();
    }

    /// <summary>Builds a curved light trail plus a brief radial burst in canvas reference coordinates.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (phase > 0 && phase < 1)
        {
            Color tint = flyingMultiplier ? new Color(.65f, 1, .88f) : new Color(1, .84f, .42f);
            // Keep the source-to-score connection legible behind the travelling bead.
            Vector2 connection = origin;
            tint.a = .32f;
            for (int i = 1; i <= 28; i++)
            {
                Vector2 next = Curve(i / 28f);
                Line(mesh, connection, next, 1.8f, tint); connection = next;
            }
            Vector2 previous = Curve(Mathf.Max(0, phase - .34f));
            for (int i = 1; i <= 22; i++)
            {
                float t = Mathf.Lerp(Mathf.Max(0, phase - .34f), phase, i / 22f);
                Vector2 next = Curve(t); float alpha = i / 22f;
                tint.a = alpha * .22f; Line(mesh, previous, next, 11 + intensity * 8, tint);
                tint.a = alpha; Line(mesh, previous, next, flyingMultiplier ? 4 : 3, tint); previous = next;
            }
            Diamond(mesh, Curve(phase), 8 + intensity * 3, new Color(1, .98f, .8f));
            if (flyingMultiplier) Ring(mesh, Curve(phase), 12, 2, new Color(.65f, 1, .88f, .8f));
        }
        if (impact > 0 && impact < 1)
        {
            Color tint = impactMultiplier ? new Color(.65f, 1, .88f, 1 - impact) : new Color(1, .84f, .42f, 1 - impact);
            Ring(mesh, destination, Mathf.Lerp(8, impactMultiplier ? 74 : 36, impact), 3 * (1 - impact) + 1, tint);
            if (impactMultiplier) Ring(mesh, destination, Mathf.Lerp(4, 45, impact), 1.5f, tint);
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
        Vector2 control = new Vector2((origin.x + destination.x) * .5f + 50, Mathf.Max(origin.y, destination.y) + 38);
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
    /// <summary>Draws a short-lived impact ripple using the same pooled canvas mesh.</summary>
    private static void Ring(VertexHelper mesh, Vector2 center, float radius, float width, Color tint)
    {
        for (int i = 0; i < 40; i++)
        {
            float a = i * Mathf.PI * 2 / 40, b = (i + 1) * Mathf.PI * 2 / 40;
            Line(mesh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, width, tint);
        }
    }
    /// <summary>Appends one colored quad without allocating a particle or material.</summary>
    private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero); mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
        mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
    }
}
