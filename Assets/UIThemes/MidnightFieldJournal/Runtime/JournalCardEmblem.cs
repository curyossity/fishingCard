using UnityEngine;

/// <summary>Draws the three independent Penpot emblems as resolution-independent native UI geometry.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalCardEmblem : UnityEngine.UI.MaskableGraphic
{
    public enum Emblem { Hook, TwinSpark, Diamond }
    [SerializeField] private Emblem emblem;

    /// <summary>Builds the approved paths without requiring raster slices or an SVG runtime package.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        if (emblem == Emblem.Diamond)
        {
            Vector2[] outer = { new Vector2(16,0),new Vector2(32,16),new Vector2(16,32),new Vector2(0,16) };
            Vector2[] inner = { new Vector2(16,11),new Vector2(21,16),new Vector2(16,21),new Vector2(11,16) };
            for(int i=0;i<4;i++) Quad(mesh,Map(outer[i],32,32),Map(outer[(i+1)%4],32,32),Map(inner[(i+1)%4],32,32),Map(inner[i],32,32));
            return;
        }
        if (emblem == Emblem.TwinSpark)
        {
            Path(mesh,new[]{new Vector2(25,2),new Vector2(32,18),new Vector2(48,25),new Vector2(32,32),new Vector2(25,48),new Vector2(18,32),new Vector2(2,25),new Vector2(18,18),new Vector2(25,2)},68,52,2.1f);
            Path(mesh,new[]{new Vector2(53,9),new Vector2(57,18),new Vector2(66,22),new Vector2(57,26),new Vector2(53,35),new Vector2(49,26),new Vector2(40,22),new Vector2(49,18),new Vector2(53,9)},68,52,2.1f);
            return;
        }
        Line(mesh,new Vector2(28,9),new Vector2(28,42),44,64,2.5f);
        Vector2 previous=new Vector2(28,42);
        for(int i=1;i<=24;i++)
        {
            float t=i/24f,u=1-t;
            Vector2 next=u*u*u*new Vector2(28,42)+3*u*u*t*new Vector2(28,62)+3*u*t*t*new Vector2(5,62)+t*t*t*new Vector2(5,44);
            Line(mesh,previous,next,44,64,2.5f);previous=next;
        }
        Path(mesh,new[]{new Vector2(5,44),new Vector2(5,33),new Vector2(1,41)},44,64,2.5f);
        Line(mesh,new Vector2(5,33),new Vector2(11,40),44,64,2.5f);
        previous=new Vector2(32,6);
        for(int i=1;i<=24;i++) { float a=i*Mathf.PI*2/24;Vector2 next=new Vector2(28+4*Mathf.Cos(a),6+4*Mathf.Sin(a));Line(mesh,previous,next,44,64,2.2f);previous=next; }
    }

    /// <summary>Converts top-left SVG coordinates into the current scaled UI rectangle.</summary>
    private Vector2 Map(Vector2 p,float width,float height) { Rect r=rectTransform.rect;return new Vector2(r.xMin+p.x/width*r.width,r.yMax-p.y/height*r.height); }
    /// <summary>Draws a connected source path using the source's stroke width.</summary>
    private void Path(UnityEngine.UI.VertexHelper mesh,Vector2[] p,float w,float h,float stroke) { for(int i=1;i<p.Length;i++)Line(mesh,p[i-1],p[i],w,h,stroke); }
    /// <summary>Adds one scale-aware stroke segment.</summary>
    private void Line(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,float w,float h,float stroke)
    {
        a=Map(a,w,h);b=Map(b,w,h);Vector2 d=b-a;Vector2 n=new Vector2(-d.y,d.x).normalized*stroke*Mathf.Min(rectTransform.rect.width/w,rectTransform.rect.height/h)*.5f;
        Quad(mesh,a-n,b-n,b+n,a+n);
    }
    /// <summary>Adds one independently colored quad to the shared canvas mesh.</summary>
    private void Quad(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d)
    { int n=mesh.currentVertCount;mesh.AddVert(a,color,Vector2.zero);mesh.AddVert(b,color,Vector2.zero);mesh.AddVert(c,color,Vector2.zero);mesh.AddVert(d,color,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3); }
}
