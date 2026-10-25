using UnityEngine;

/// <summary>Resolution-independent R045 chevron without a text label or font dependency.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalStatScrollArrow : UnityEngine.UI.MaskableGraphic
{
    public bool Upwards;
    /// <summary>Draws the two thin strokes inside the larger arrow hit area.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();Rect r=rectTransform.rect;float sign=Upwards?1:-1;
        Vector2 a=r.center+new Vector2(-12,-5*sign),b=r.center+new Vector2(0,5*sign),c=r.center+new Vector2(12,-5*sign);
        Segment(mesh,a,b);Segment(mesh,b,c);
    }
    /// <summary>Adds one antialias-independent mesh stroke using the approved 2.4px width.</summary>
    private void Segment(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b)
    {Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*1.2f;int i=mesh.currentVertCount;mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);}
}
