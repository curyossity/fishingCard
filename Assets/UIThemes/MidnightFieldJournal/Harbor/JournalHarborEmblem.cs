using UnityEngine;

/// <summary>Draws the approved Journal emblems as resolution-independent Penpot path geometry.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalHarborEmblem : UnityEngine.UI.MaskableGraphic
{
    public enum Kind { Book, Anchor, Coin, Check }
    [SerializeField] private Kind kind;

    /// <summary>Selects an authored emblem without changing its source proportions.</summary>
    public void Configure(Kind value) { kind=value; SetVerticesDirty(); }

    /// <summary>Tessellates the same source paths used by the approved Penpot screen.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        if(kind==Kind.Check) { Line(mesh,15,52,39,76); Line(mesh,39,76,86,22); return; }
        if(kind==Kind.Coin)
        {
            Circle(mesh,50,50,38); Circle(mesh,50,50,30);
            Line(mesh,43,36,61,45); Line(mesh,61,45,55,64); Line(mesh,55,64,37,55); Line(mesh,37,55,43,36); return;
        }
        if(kind==Kind.Anchor)
        {
            Line(mesh,50,12,50,79); Circle(mesh,50,22,10); Line(mesh,24,44,76,44);
            Curve(mesh,new Vector2(10,63),new Vector2(12,88),new Vector2(50,90));
            Curve(mesh,new Vector2(50,90),new Vector2(88,88),new Vector2(90,63));
            Line(mesh,10,63,8,78); Line(mesh,10,63,24,69); Line(mesh,90,63,92,78); Line(mesh,90,63,76,69); return;
        }
        Curve(mesh,new Vector2(50,23),new Vector2(28,6),new Vector2(8,20)); Line(mesh,8,20,8,82);
        Curve(mesh,new Vector2(8,82),new Vector2(28,73),new Vector2(50,89));
        Curve(mesh,new Vector2(50,89),new Vector2(72,73),new Vector2(92,82)); Line(mesh,92,82,92,20);
        Curve(mesh,new Vector2(92,20),new Vector2(72,6),new Vector2(50,23)); Line(mesh,50,23,50,89);
        Line(mesh,18,33,39,38); Line(mesh,18,46,39,51); Line(mesh,61,38,82,33); Line(mesh,61,51,82,46);
    }

    /// <summary>Approximates the source quadratic curves without a raster or SVG runtime dependency.</summary>
    private void Curve(UnityEngine.UI.VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c)
    {
        Vector2 previous=a;
        for(int i=1;i<=20;i++) { float t=i/20f,u=1-t; Vector2 next=u*u*a+2*u*t*b+t*t*c; Line(mesh,previous.x,previous.y,next.x,next.y); previous=next; }
    }

    /// <summary>Draws a source circle at a stable native UI resolution.</summary>
    private void Circle(UnityEngine.UI.VertexHelper mesh,float x,float y,float radius)
    {
        for(int i=0;i<64;i++) { float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64; Line(mesh,x+radius*Mathf.Cos(a),y+radius*Mathf.Sin(a),x+radius*Mathf.Cos(b),y+radius*Mathf.Sin(b)); }
    }

    /// <summary>Converts a three-unit SVG stroke into a nonblocking Canvas quad.</summary>
    private void Line(UnityEngine.UI.VertexHelper mesh,float ax,float ay,float bx,float by)
    {
        Rect r=rectTransform.rect;
        Vector2 a=new Vector2(r.xMin+ax*r.width/100,r.yMax-ay*r.height/100),b=new Vector2(r.xMin+bx*r.width/100,r.yMax-by*r.height/100);
        Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*1.5f*Mathf.Min(r.width,r.height)/100;
        int i=mesh.currentVertCount; mesh.AddVert(a-n,color,Vector2.zero); mesh.AddVert(b-n,color,Vector2.zero); mesh.AddVert(b+n,color,Vector2.zero); mesh.AddVert(a+n,color,Vector2.zero);
        mesh.AddTriangle(i,i+1,i+2); mesh.AddTriangle(i,i+2,i+3);
    }
}
