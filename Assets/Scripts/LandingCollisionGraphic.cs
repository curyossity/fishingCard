using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders the fixed housing or one independent panel silhouette using preserved source pixels.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingCollisionGraphic : MaskableGraphic
{
    [SerializeField] private Sprite source;
    [SerializeField] private Rect region;
    [SerializeField] private bool housing;
    [SerializeField] private LandingPanelArtwork panel;
    public static Rect CreatureRegion => new Rect(650, 302, 470, 240);
    public static Rect ScoreRegion => new Rect(1126, 302, 510, 240);
    public override Texture mainTexture => !housing && panel != null && panel.Face != null ? panel.Face.texture
        : source != null ? source.texture : Texture2D.whiteTexture;

    /// <summary>Reconstructs the untouched shell around movable native faces, revealing dark recesses only during travel.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (!housing && panel != null) { Face(mesh); return; }
        if (source == null) return;
        if (!housing)
        {
            Panel(mesh, region, rectTransform.rect, false);
            return;
        }
        Rect shell = TackleMachineGeometry.StageBounds, face = CreatureRegion, score = ScoreRegion;
        // Both panels share a continuous dark recess, including the newly visible space between smaller faces.
        Strip(mesh, new Rect(shell.x, shell.y, shell.width, face.y - shell.y));
        Strip(mesh, new Rect(shell.x, face.yMax, shell.width, shell.yMax - face.yMax));
        Strip(mesh, new Rect(shell.x, face.y, face.x - shell.x, face.height));
        Strip(mesh, new Rect(score.xMax, face.y, shell.xMax - score.xMax, face.height));
        Strip(mesh, new Rect(face.x, face.y, score.xMax - face.x, face.height), true);
    }

    /// <summary>Leaves the source slice's cabinet pixels undrawn, including at rotated, stretched and zoomed panel edges.</summary>
    private void Face(VertexHelper mesh)
    {
        if (panel.Face == null || panel.VertexCount < 3) return;
        Rect local = rectTransform.rect, pixels = panel.Face.rect;
        Texture texture = panel.Face.texture;
        for (int i = 0; i < panel.VertexCount; i++)
        {
            Vector2 point = panel.Point(i);
            mesh.AddVert(new Vector2(local.xMin + point.x * local.width, local.yMin + point.y * local.height), color,
                new Vector2((pixels.x + point.x * pixels.width) / texture.width, (pixels.y + point.y * pixels.height) / texture.height));
        }
        for (int i = 1; i < panel.VertexCount - 1; i++) mesh.AddTriangle(0, i, i + 1);
    }

    /// <summary>Projects a stage rectangle into the original housing's local space.</summary>
    private void Strip(VertexHelper mesh, Rect area, bool recess = false)
    {
        Rect shell = TackleMachineGeometry.StageBounds, local = rectTransform.rect;
        var target = new Rect(local.xMin + area.x - shell.x, local.yMax - (area.yMax - shell.y), area.width, area.height);
        Panel(mesh, area, target, recess);
    }

    /// <summary>Samples the same texture coordinates at rest; recessed space uses one dark glass texel from the original art.</summary>
    private void Panel(VertexHelper mesh, Rect area, Rect target, bool recess)
    {
        Rect shell = TackleMachineGeometry.StageBounds, pixels = source.rect;
        Vector2 lo = new Vector2((pixels.x + (area.x - shell.x) / shell.width * pixels.width) / source.texture.width,
            (pixels.y + (1 - (area.yMax - shell.y) / shell.height) * pixels.height) / source.texture.height);
        Vector2 hi = new Vector2((pixels.x + (area.xMax - shell.x) / shell.width * pixels.width) / source.texture.width,
            (pixels.y + (1 - (area.y - shell.y) / shell.height) * pixels.height) / source.texture.height);
        if (recess) lo = hi = new Vector2((pixels.x + (900 - shell.x) / shell.width * pixels.width) / source.texture.width,
            (pixels.y + (1 - (548 - shell.y) / shell.height) * pixels.height) / source.texture.height);
        int first = mesh.currentVertCount;
        mesh.AddVert(new Vector2(target.xMin, target.yMin), color, lo);
        mesh.AddVert(new Vector2(target.xMin, target.yMax), color, new Vector2(lo.x, hi.y));
        mesh.AddVert(new Vector2(target.xMax, target.yMax), color, hi);
        mesh.AddVert(new Vector2(target.xMax, target.yMin), color, new Vector2(hi.x, lo.y));
        mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
    }
}
