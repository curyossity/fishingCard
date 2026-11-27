using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws a transient white silhouette over a machine panel without exposing rectangular crop corners.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingPanelFlash : MaskableGraphic
{
    [SerializeField] private LandingPanelArtwork panel;
    public float Opacity => color.a;

    /// <summary>Sets the non-intercepting flash; an invisible flash emits no geometry.</summary>
    public void Show(float opacity) => color = new Color(1, 1, 1, Mathf.Clamp01(opacity));
    /// <summary>Same silhouette in a tint, such as the creature's hurt flash.</summary>
    public void Show(float opacity, Color tint) => color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01(opacity));

    /// <summary>Follows the original panel outline so deformation and camera zoom retain clean transparent edges.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); if (panel == null || color.a <= 0) return;
        Rect local = rectTransform.rect;
        for (int i = 0; i < panel.VertexCount; i++)
        {
            Vector2 point = panel.Point(i);
            mesh.AddVert(new Vector2(local.xMin + point.x * local.width, local.yMin + point.y * local.height), color, Vector2.zero);
        }
        for (int i = 1; i < panel.VertexCount - 1; i++) mesh.AddTriangle(0, i, i + 1);
    }
}
