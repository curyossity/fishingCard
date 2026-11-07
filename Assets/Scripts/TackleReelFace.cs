using TMPro;
using UnityEngine;

/// <summary>Projects one tag's illustration and live TMP glyphs onto a shared cylindrical reel surface.</summary>
public sealed class TackleReelFace : MonoBehaviour
{
    private RectTransform cylinder;
    private TMP_Text fallback, label;
    private UnityEngine.UI.Image symbol;
    private float angle, radius, opacity = 1;

    /// <summary>Binds native graphic companions; no functional text is baked into artwork.</summary>
    public void Initialize(RectTransform surface, UnityEngine.UI.Image illustration, TMP_Text text, TMP_Text caption, float cylinderRadius)
    {
        cylinder = surface; symbol = illustration; fallback = text; label = caption; radius = cylinderRadius;
        symbol.gameObject.AddComponent<CylindricalReelGraphic>().Bind(this);
        fallback.OnPreRenderText += ProjectFallback;
        label.OnPreRenderText += ProjectLabel;
    }

    /// <summary>Updates presentation angle while retaining unmodified source text meshes for each new frame.</summary>
    public void SetAngle(float radians, bool visible, float alpha = 1)
    {
        gameObject.SetActive(visible);
        if (!visible) return;
        angle = radians;
        opacity = alpha;
        symbol.SetVerticesDirty();
        if (fallback.gameObject.activeInHierarchy) fallback.ForceMeshUpdate();
        label.ForceMeshUpdate();
    }

    /// <summary>Maps a point to the visible half-cylinder, compressing glyphs and sprites near its silhouette.</summary>
    public Vector3 Project(Transform graphic, Vector3 vertex, ref Color32 color)
    {
        Vector3 local = cylinder.InverseTransformPoint(graphic.TransformPoint(vertex));
        Vector3 center = cylinder.rect.center;
        local -= center;
        float theta = Mathf.Asin(Mathf.Clamp(local.y / radius, -1, 1)) + angle;
        float facing = Mathf.Cos(theta);
        local.y = radius * Mathf.Sin(theta);
        float light = .58f + .42f * Mathf.Max(0, facing);
        color.r = (byte)(color.r * light); color.g = (byte)(color.g * light); color.b = (byte)(color.b * light);
        color.a = (byte)(color.a * opacity * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .18f, facing)));
        return graphic.InverseTransformPoint(cylinder.TransformPoint(local + center));
    }

    /// <summary>Projects native fallback glyphs after TMP has generated their undeformed positions.</summary>
    private void ProjectFallback(TMP_TextInfo info) { ProjectText(fallback, info); }

    /// <summary>Projects the lower tag caption together with its illustration.</summary>
    private void ProjectLabel(TMP_TextInfo info) { ProjectText(label, info); }

    /// <summary>Deforms each used glyph vertex in place without touching TMP's shared font materials.</summary>
    private void ProjectText(TMP_Text text, TMP_TextInfo info)
    {
        for (int material = 0; material < info.meshInfo.Length; material++)
        {
            var mesh = info.meshInfo[material];
            for (int i = 0; i < mesh.vertexCount; i++)
                mesh.vertices[i] = Project(text.transform, mesh.vertices[i], ref mesh.colors32[i]);
        }
    }

    /// <summary>Releases callbacks when an expedition view is destroyed.</summary>
    private void OnDestroy()
    {
        if (fallback != null) fallback.OnPreRenderText -= ProjectFallback;
        if (label != null) label.OnPreRenderText -= ProjectLabel;
    }
}
