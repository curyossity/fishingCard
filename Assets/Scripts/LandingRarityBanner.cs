using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rare and Legendary entrance banner: a tilted enamel tag stamped across the encounter creature's card when its face lands.
/// It shares the strike ribbon's construction (brass bevel, studs, swallowtails) but uses a dark enamel face so it never reads as
/// a scoring result. Four studs echo the card faces' rarity hooks. Presentation only; it never changes the encounter.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingRarityBanner : MaskableGraphic
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text face, shadow;
    [SerializeField] private LandingScoreRush preferences;
    private CardRarity rarity;
    private double stampedAt;
    private bool active;
    private Quaternion restRotation;
    private Vector3 restScale;
    private bool captured;
    public const float Tilt = -4, StampSeconds = .16f, FoldSeconds = .16f;
    public bool IsShowing => active;
    public CardRarity Rarity => rarity;
    public float Age => (float)(Time.realtimeSinceStartupAsDouble - stampedAt);
    private bool Reduced => preferences != null && preferences.ReducedMotion;

    /// <summary>Only rarities that deserve an announcement get a banner; routine creatures stay quiet.</summary>
    public static bool Announces(CardRarity value) => value >= CardRarity.Rare;
    /// <summary>Single word on the plate; hook studs carry the tier.</summary>
    public static string Word(CardRarity value) => value == CardRarity.Legendary ? "LEGENDARY" : value == CardRarity.Rare ? "RARE" : "";
    /// <summary>Seconds from the stamp until the fold begins; Legendary lingers slightly longer.</summary>
    public static float Hold(CardRarity value) => value == CardRarity.Legendary ? 1.4f : 1.1f;
    public static float Lifetime(CardRarity value) => Hold(value) + FoldSeconds;

    /// <summary>Stamp scale: slams from 1.3 past 0.96 and settles on exactly 1.</summary>
    public static float StampScale(float age)
    {
        if (age <= 0) return 1.3f;
        if (age < .09f) { float u = age / .09f; return Mathf.Lerp(1.3f, .96f, u * u); }
        if (age < StampSeconds) { float u = (age - .09f) / (StampSeconds - .09f); return Mathf.Lerp(.96f, 1, 1 - (1 - u) * (1 - u)); }
        return 1;
    }

    /// <summary>Rotation in degrees: arrives over-rotated and settles to the resting tilt.</summary>
    public static float StampRotation(float age) => Tilt - 6 * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / StampSeconds)));

    /// <summary>Vertical projection while folding away with the cards' turn language; 1 while held.</summary>
    public static float Fold(float age, CardRarity value)
    {
        float u = Mathf.Clamp01((age - Hold(value)) / FoldSeconds);
        return Mathf.Max(.015f, Mathf.Cos(u * Mathf.PI * .5f));
    }

    /// <summary>Whole-banner opacity; reduced motion fades in and out instead of stamping and folding.</summary>
    public static float Opacity(float age, CardRarity value, bool reduced)
    {
        if (age < 0 || age >= Lifetime(value)) return 0;
        float fadeIn = Mathf.Clamp01(age / (reduced ? .15f : .04f));
        float fadeOut = reduced ? 1 - Mathf.Clamp01((age - Hold(value)) / FoldSeconds) : (age > Hold(value) + FoldSeconds * .8f ? 0 : 1);
        return fadeIn * fadeOut;
    }

    /// <summary>Legendary glint position across the plate (-1..1), or NaN when no glint is visible.</summary>
    public static float Glint(float age, CardRarity value)
    {
        const float start = .22f, sweep = .38f;
        if (value != CardRarity.Legendary || age < start || age > start + sweep) return float.NaN;
        return Mathf.Lerp(-1.2f, 1.2f, Mathf.SmoothStep(0, 1, (age - start) / sweep));
    }

    /// <summary>Starts the banner at an absolute real-time stamp moment so live play and frame exports share one clock.</summary>
    public void Stamp(CardRarity value, double at)
    {
        if (!Announces(value)) { Clear(); return; }
        Capture(); rarity = value; stampedAt = at; active = true;
        face.text = shadow.text = Word(value);
        bool legendary = value == CardRarity.Legendary;
        face.color = legendary ? new Color32(255, 214, 102, 255) : new Color32(255, 248, 218, 255);
        shadow.color = legendary ? new Color32(70, 42, 8, 230) : new Color32(9, 32, 38, 230);
        Tick();
    }

    /// <summary>Samples the stamp, hold, glint and fold; clears itself after its lifetime.</summary>
    public void Tick()
    {
        if (!active) return;
        float age = Age;
        if (age >= Lifetime(rarity)) { Clear(); return; }
        bool reduced = Reduced;
        group.alpha = Opacity(age, rarity, reduced);
        float scale = reduced ? 1 : StampScale(age), fold = reduced ? 1 : Fold(age, rarity);
        rectTransform.localScale = new Vector3(restScale.x * scale, restScale.y * scale * fold, restScale.z);
        rectTransform.localRotation = restRotation * Quaternion.Euler(0, 0, reduced ? Tilt : StampRotation(age));
        SetVerticesDirty();
    }

    /// <summary>Hides immediately and restores the authored pose: on a strike, skip-to-next, failure, new encounter or phase exit.</summary>
    public void Clear()
    {
        active = false;
        if (group != null) group.alpha = 0;
        if (captured) { rectTransform.localScale = restScale; rectTransform.localRotation = restRotation; }
        SetVerticesDirty();
    }

    private void Capture()
    {
        if (captured) return;
        restScale = rectTransform.localScale; restRotation = rectTransform.localRotation; captured = true;
    }

    protected override void OnDisable() { Clear(); base.OnDisable(); }

    /// <summary>Plate, bevel, swallowtails, four hook studs and the Legendary glint, all native geometry.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); if (!active) return;
        bool legendary = rarity == CardRarity.Legendary;
        Color shade = new Color32(20, 14, 10, 220), dark = new Color32(92, 58, 24, 255);
        Color gold = legendary ? (Color)new Color32(246, 192, 66, 255) : new Color32(214, 160, 62, 255);
        Color light = legendary ? (Color)new Color32(255, 238, 160, 255) : new Color32(242, 212, 136, 255);
        Color enamel = legendary ? (Color)new Color32(26, 21, 18, 255) : new Color32(28, 82, 96, 255);
        Color inner = legendary ? (Color)new Color32(40, 32, 24, 255) : new Color32(22, 64, 76, 255);
        Color tail = legendary ? (Color)new Color32(54, 40, 26, 255) : new Color32(20, 58, 68, 255);
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 a = new Vector2(side * 148, 18), b = new Vector2(side * 213, 18), c = new Vector2(side * 198, -8);
            Vector2 d = new Vector2(side * 213, -34), e = new Vector2(side * 148, -34), drop = Vector2.down * 4;
            Tri(mesh, a + drop, b + drop, c + drop, shade); Tri(mesh, a + drop, c + drop, e + drop, shade); Tri(mesh, c + drop, d + drop, e + drop, shade);
            Tri(mesh, a, b, c, gold); Tri(mesh, a, c, e, gold); Tri(mesh, c, d, e, gold);
            Vector2 inset = Vector2.right * (side * 4);
            Tri(mesh, a + inset + Vector2.down * 4, b - inset + Vector2.down * 4, c - inset, tail);
            Tri(mesh, a + inset + Vector2.down * 4, c - inset, e + inset + Vector2.up * 4, tail);
            Tri(mesh, c - inset, d - inset + Vector2.up * 4, e + inset + Vector2.up * 4, tail);
            Tri(mesh, new Vector2(side * 172, -36), new Vector2(side * 148, -34), new Vector2(side * 172, -50), dark);
        }
        Plate(mesh, new Vector2(0, -6), new Vector2(332, 84), 8, shade);
        Plate(mesh, Vector2.zero, new Vector2(332, 84), 8, dark);
        Plate(mesh, new Vector2(0, 2), new Vector2(326, 80), 7, light);
        Plate(mesh, Vector2.zero, new Vector2(318, 72), 6, gold);
        Plate(mesh, new Vector2(0, 1), new Vector2(306, 62), 4, enamel);
        Plate(mesh, new Vector2(0, -12), new Vector2(298, 30), 3, inner);
        Line(mesh, new Vector2(-147, 30), new Vector2(147, 30), 2, legendary ? new Color32(255, 247, 210, 255) : new Color32(150, 210, 214, 255));
        foreach (float x in new[] { -154f, 154f }) foreach (float y in new[] { 27f, -27f }) Stud(mesh, new Vector2(x, y), 3.5f, light, dark);
        int lit = rarity == CardRarity.Legendary ? 4 : 3;
        for (int i = 0; i < 4; i++)
        {
            var center = new Vector2((i - 1.5f) * 22, -18);
            if (i < lit) Stud(mesh, center, 6, legendary ? new Color32(255, 222, 120, 255) : new Color32(238, 232, 200, 255), dark);
            else { Disc(mesh, center, 6, new Color32(10, 16, 18, 255)); Disc(mesh, center + new Vector2(0, 1), 4, new Color32(32, 44, 48, 255)); }
        }
        float glint = Glint(Age, rarity);
        if (!float.IsNaN(glint) && !Reduced)
        {
            float x = glint * 153, w = 16;
            float left = Mathf.Max(-153, x - w), right = Mathf.Min(153, x + w);
            if (right > left)
            {
                Color sheen = new Color(1, .96f, .78f, .55f);
                int start = mesh.currentVertCount;
                mesh.AddVert(new Vector3(left - 8, 31), sheen, Vector2.zero); mesh.AddVert(new Vector3(right - 8, 31), sheen, Vector2.zero);
                mesh.AddVert(new Vector3(right + 8, -29), sheen, Vector2.zero); mesh.AddVert(new Vector3(left + 8, -29), sheen, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
            }
        }
    }

    private static void Tri(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        int i = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero); mesh.AddVert(b, color, Vector2.zero); mesh.AddVert(c, color, Vector2.zero);
        mesh.AddTriangle(i, i + 1, i + 2);
    }

    /// <summary>Chamfered rectangle so the plate keeps the cabinet's cut-corner silhouette.</summary>
    private static void Plate(VertexHelper mesh, Vector2 center, Vector2 size, float cut, Color color)
    {
        float x = size.x * .5f, y = size.y * .5f;
        Vector2[] p = { new Vector2(-x + cut, y), new Vector2(x - cut, y), new Vector2(x, y - cut), new Vector2(x, -y + cut),
            new Vector2(x - cut, -y), new Vector2(-x + cut, -y), new Vector2(-x, -y + cut), new Vector2(-x, y - cut) };
        int start = mesh.currentVertCount;
        mesh.AddVert(center, color, Vector2.zero);
        foreach (var point in p) mesh.AddVert(center + point, color, Vector2.zero);
        for (int i = 0; i < p.Length; i++) mesh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % p.Length);
    }

    private static void Line(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color color)
    {
        Plate(mesh, (from + to) * .5f, new Vector2(Vector2.Distance(from, to), width), 0, color);
    }

    private static void Disc(VertexHelper mesh, Vector2 center, float radius, Color color)
    {
        int start = mesh.currentVertCount; mesh.AddVert(center, color, Vector2.zero);
        for (int i = 0; i <= 12; i++) { float a = i * Mathf.PI * 2 / 12; mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector2.zero); }
        for (int i = 0; i < 12; i++) mesh.AddTriangle(start, start + 1 + i, start + 2 + i);
    }

    /// <summary>Raised brass stud with a dark rim and a small highlight.</summary>
    private static void Stud(VertexHelper mesh, Vector2 center, float radius, Color top, Color rim)
    {
        Disc(mesh, center + new Vector2(0, -1), radius + 1.5f, rim);
        Disc(mesh, center, radius, top);
        Disc(mesh, center + new Vector2(-radius * .3f, radius * .3f), radius * .35f, new Color(1, 1, 1, .7f));
    }
}
