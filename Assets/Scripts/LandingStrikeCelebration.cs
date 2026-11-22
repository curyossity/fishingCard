using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws the ivory/brass strike ribbon and a bounded water burst outside the aiming-track mask.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingStrikeCelebration : MaskableGraphic
{
    private LandingAccuracy accuracy;
    private float time;
    private bool visible, reduced;
    private Vector2 origin;
    private static readonly Vector2[] DropOutline = {
        new Vector2(0, 1.4f), new Vector2(-.22f, .55f), new Vector2(-.58f, .1f), new Vector2(-.62f, -.35f),
        new Vector2(-.30f, -.7f), new Vector2(.25f, -.7f), new Vector2(.60f, -.35f), new Vector2(.54f, .1f), new Vector2(.20f, .55f) };

    /// <summary>Samples the contact owner's existing clock and frozen splash origin; creates no particles or random game events.</summary>
    public void Render(LandingAccuracy grade, float elapsed, bool reducedMotion, Vector2 splashOrigin)
    { accuracy = grade; time = elapsed; reduced = reducedMotion; origin = splashOrigin; visible = elapsed >= 0; SetVerticesDirty(); }

    /// <summary>Clears all geometry at cancellation, phase exit and a fresh receipt.</summary>
    public void Clear() { visible = false; SetVerticesDirty(); }

    /// <summary>Places water behind the ribbon, retaining an opaque readable center for native result text.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); if (!visible) return;
        bool perfect = accuracy == LandingAccuracy.Perfect, miss = accuracy == LandingAccuracy.Miss;
        if (!reduced && !miss) Water(mesh, perfect);
        Ribbon(mesh, miss, perfect);
    }

    /// <summary>Builds a warm enamel face with stepped brass bevels, inset corner studs and folded ribbon tails.</summary>
    private static void Ribbon(VertexHelper mesh, bool miss, bool perfect)
    {
        Color shadow = new Color32(39, 27, 18, 235), dark = new Color32(102, 63, 26, 255);
        Color gold = miss ? (Color)new Color32(132, 111, 78, 255) : new Color32(236, 179, 62, 255);
        Color light = miss ? (Color)new Color32(182, 160, 116, 255) : new Color32(255, 232, 142, 255);
        Color face = miss ? (Color)new Color32(49, 51, 44, 255) : new Color32(255, 248, 218, 255);
        for (int side = -1; side <= 1; side += 2)
        {
            // Five-point swallowtails sit behind the heavy center plaque.
            Vector2 a = new Vector2(side * 275, 22), b = new Vector2(side * 380, 22);
            Vector2 c = new Vector2(side * 358, -13), d = new Vector2(side * 380, -48), e = new Vector2(side * 275, -48);
            Triangle(mesh, a + Vector2.down * 5, b + Vector2.down * 5, c + Vector2.down * 5, shadow);
            Triangle(mesh, a + Vector2.down * 5, c + Vector2.down * 5, e + Vector2.down * 5, shadow);
            Triangle(mesh, c + Vector2.down * 5, d + Vector2.down * 5, e + Vector2.down * 5, shadow);
            Triangle(mesh, a, b, c, gold); Triangle(mesh, a, c, e, gold); Triangle(mesh, c, d, e, gold);
            Vector2 inset = Vector2.right * (side * 5);
            Triangle(mesh, a + inset + Vector2.down * 5, b - inset + Vector2.down * 5, c - inset, face);
            Triangle(mesh, a + inset + Vector2.down * 5, c - inset, e + inset + Vector2.up * 5, face);
            Triangle(mesh, c - inset, d - inset + Vector2.up * 5, e + inset + Vector2.up * 5, face);
            Triangle(mesh, new Vector2(side * 308, -51), new Vector2(side * 275, -48), new Vector2(side * 308, -69), dark);
        }
        Plate(mesh, new Vector2(0, -7), new Vector2(630, 110), 10, shadow);
        Plate(mesh, Vector2.zero, new Vector2(630, 110), 10, dark);
        Plate(mesh, new Vector2(0, 2), new Vector2(624, 106), 9, light);
        Plate(mesh, Vector2.zero, new Vector2(616, 98), 7, gold);
        Plate(mesh, new Vector2(0, 2), new Vector2(606, 88), 5, face);
        // Highlights remain at the material edges, leaving the text's ivory field quiet.
        Line(mesh, new Vector2(-293, 38), new Vector2(293, 38), 2, new Color32(255, 255, 239, 255));
        Line(mesh, new Vector2(-293, -39), new Vector2(293, -39), 2, light);
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            {
                Vector2 at = new Vector2(x * 288, y * 30);
                Plate(mesh, at + Vector2.down, new Vector2(7, 7), 2, dark);
                Plate(mesh, at, new Vector2(5, 5), 1, gold);
                if (perfect) Quad(mesh, at + new Vector2(-1, 1), new Vector2(2, 2), light);
            }
    }

    /// <summary>Emits an asymmetric cyan flash, faceted drops, rising bubbles and a few warm glints around the true contact.</summary>
    private void Water(VertexHelper mesh, bool perfect)
    {
        float strength = perfect ? 1 : .65f;
        float flash = Mathf.Pow(1 - Mathf.Clamp01((time - .055f) / .28f), 1.4f);
        Glow(mesh, origin + Vector2.up * 55, new Vector2(230, 205) * strength, new Color(.28f, .88f, 1, flash * .60f));
        Glow(mesh, origin, new Vector2(125, 105) * strength, new Color(.88f, 1, 1, flash * .98f));
        for (int i = 0; i < (perfect ? 9 : 5); i++)
        {
            float angle = (i * 137.5f + 12) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float length = (160 + i % 4 * 45) * strength;
            float expansion = 1 - Mathf.Pow(1 - Mathf.Clamp01(time / .13f), 3);
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Ray(mesh, origin + direction * 14, origin + direction * length * expansion, normal * (13 + i % 3 * 5), new Color(.60f, .96f, 1, flash * .42f));
        }
        int count = perfect ? 14 : 7;
        for (int i = 0; i < count; i++)
        {
            float delay = (i % 3) * .012f, life = .35f + i % 4 * .055f;
            float t = time - delay, p = t / life;
            if (t < 0 || p >= 1) continue;
            float angle = (i * 137.5f + 24) * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(angle) * (310 + i % 4 * 70), Mathf.Sin(angle) * (650 + i % 3 * 180) + 180) * strength;
            Vector2 at = origin + velocity * t + Vector2.down * (t * t * 440);
            float alpha = 1 - Mathf.Clamp01((p - .45f) / .55f);
            Drop(mesh, at, velocity.normalized, (12 + i % 4 * 5) * strength * (1 - .25f * p), alpha);
        }
        for (int i = 0; i < (perfect ? 6 : 3); i++)
        {
            float delay = .045f + i % 3 * .023f, t = time - delay;
            if (t < 0 || t >= .5f) continue;
            float side = i % 2 == 0 ? -1 : 1;
            Vector2 at = origin + new Vector2(side * (28 + i * 19) * strength, 30 + t * (300 + i * 65));
            Bubble(mesh, at, (8 + i % 3 * 8) * strength, Mathf.Min(1, t / .04f) * (1 - t / .5f));
        }
        for (int i = 0; i < (perfect ? 5 : 2); i++)
        {
            float t = time - .035f * (i % 3);
            if (t < 0 || t > .32f) continue;
            float pulse = Mathf.Sin(Mathf.PI * t / .32f);
            float height = i == 0 ? 230 : i == 1 ? -70 : i == 2 ? 165 : i == 3 ? -115 : 280;
            Vector2 at = origin + new Vector2((i % 2 == 0 ? -1 : 1) * (82 + i * 24), height);
            Star(mesh, at, (7 + i % 2 * 3) * strength * pulse, pulse);
        }
    }

    /// <summary>Draws one faceted water silhouette with a pale rim, blue body and a small white reflection.</summary>
    private static void Drop(VertexHelper mesh, Vector2 center, Vector2 direction, float size, float alpha)
    {
        Vector2 up = -direction, right = new Vector2(up.y, -up.x);
        for (int ring = 0; ring < 2; ring++)
        {
            float scale = ring == 0 ? 1 : .73f;
            Color tint = ring == 0 ? new Color(.86f, 1, 1, alpha) : new Color(.32f, .70f, .86f, alpha);
            for (int i = 0; i < DropOutline.Length; i++)
            {
                Vector2 a = DropOutline[i] * (size * scale), b = DropOutline[(i + 1) % DropOutline.Length] * (size * scale);
                Triangle(mesh, center, center + right * a.x + up * a.y, center + right * b.x + up * b.y, tint);
            }
        }
        Line(mesh, center + right * (-size * .23f) + up * (size * .2f), center + right * (-size * .23f) - up * (size * .32f), Mathf.Max(2, size * .2f), new Color(1, 1, .98f, alpha));
    }

    /// <summary>Draws a thin translucent bubble with a stronger upper-left reflection and a quiet blue lower rim.</summary>
    private static void Bubble(VertexHelper mesh, Vector2 center, float radius, float alpha)
    {
        for (int i = 0; i < 20; i++)
        {
            float a = i * Mathf.PI * 2 / 20, b = (i + 1) * Mathf.PI * 2 / 20;
            Vector2 from = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            Vector2 to = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius;
            Color rim = i >= 4 && i <= 9 ? new Color(.94f, 1, 1, alpha) : new Color(.47f, .83f, .98f, alpha * .7f);
            Triangle(mesh, center, from, to, new Color(.44f, .84f, 1, alpha * .07f));
            Line(mesh, from, to, i >= 4 && i <= 9 ? 3 : 2, rim);
        }
    }

    /// <summary>Draws a small gold four-point material glint rather than a screen-filling starburst.</summary>
    private static void Star(VertexHelper mesh, Vector2 center, float size, float alpha)
    {
        Color gold = new Color(1, .78f, .24f, alpha), white = new Color(1, 1, .84f, alpha);
        Triangle(mesh, center + Vector2.up * size, center + Vector2.left * size * .25f, center + Vector2.down * size, gold);
        Triangle(mesh, center + Vector2.up * size, center + Vector2.down * size, center + Vector2.right * size * .25f, gold);
        Triangle(mesh, center + Vector2.left * size * .65f, center + Vector2.down * size * .2f, center + Vector2.right * size * .65f, white);
        Triangle(mesh, center + Vector2.left * size * .65f, center + Vector2.right * size * .65f, center + Vector2.up * size * .2f, white);
    }

    /// <summary>Feathers a soft oval from a lit center to a transparent perimeter using the standard canvas material.</summary>
    private static void Glow(VertexHelper mesh, Vector2 center, Vector2 radius, Color tint)
    {
        for (int i = 0; i < 24; i++)
        {
            float a = i * Mathf.PI * 2 / 24, b = (i + 1) * Mathf.PI * 2 / 24;
            Vector2 from = center + Vector2.Scale(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), radius);
            Vector2 to = center + Vector2.Scale(new Vector2(Mathf.Cos(b), Mathf.Sin(b)), radius);
            int first = mesh.currentVertCount;
            mesh.AddVert(center, tint, Vector2.zero); Color edge = tint; edge.a = 0;
            mesh.AddVert(from, edge, Vector2.zero); mesh.AddVert(to, edge, Vector2.zero); mesh.AddTriangle(first, first + 1, first + 2);
        }
    }

    /// <summary>Adds a tapered translucent water-light ray with a transparent far end.</summary>
    private static void Ray(VertexHelper mesh, Vector2 from, Vector2 to, Vector2 halfWidth, Color tint)
    {
        int first = mesh.currentVertCount; Color end = tint; end.a = 0;
        mesh.AddVert(from - halfWidth, tint, Vector2.zero); mesh.AddVert(from + halfWidth, tint, Vector2.zero);
        mesh.AddVert(to, end, Vector2.zero); mesh.AddTriangle(first, first + 1, first + 2);
    }

    /// <summary>Builds a cut-corner plaque from fixed triangles without a generated texture or per-frame objects.</summary>
    private static void Plate(VertexHelper mesh, Vector2 center, Vector2 size, float corner, Color tint)
    {
        Vector2 h = size * .5f;
        Quad(mesh, center, new Vector2(size.x - corner * 2, size.y), tint);
        Quad(mesh, center, new Vector2(size.x, size.y - corner * 2), tint);
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                Triangle(mesh, center + new Vector2(x * (h.x - corner), y * h.y), center + new Vector2(x * h.x, y * (h.y - corner)), center + new Vector2(x * (h.x - corner), y * (h.y - corner)), tint);
    }

    /// <summary>Adds a solid rectangle using two triangles.</summary>
    private static void Quad(VertexHelper mesh, Vector2 center, Vector2 size, Color tint)
    { Vector2 h = size * .5f; Triangle(mesh, center - h, center + new Vector2(-h.x, h.y), center + h, tint); Triangle(mesh, center - h, center + h, center + new Vector2(h.x, -h.y), tint); }

    /// <summary>Adds one flat-colored triangle to the shared bounded UI mesh.</summary>
    private static void Triangle(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Color tint)
    { int first = mesh.currentVertCount; mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero); mesh.AddVert(c, tint, Vector2.zero); mesh.AddTriangle(first, first + 1, first + 2); }

    /// <summary>Adds a narrow faceted stroke without allocating a material.</summary>
    private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
    {
        Vector2 normal = new Vector2(from.y - to.y, to.x - from.x).normalized * (width * .5f);
        Triangle(mesh, from - normal, from + normal, to + normal, tint); Triangle(mesh, from - normal, to + normal, to - normal, tint);
    }
}
