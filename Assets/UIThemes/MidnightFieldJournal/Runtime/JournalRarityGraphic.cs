using UnityEngine;
using UnityEngine.UI;

/// <summary>Independent resolution-independent timing ornament, zone pattern, marker or stop burst.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalRarityGraphic : MaskableGraphic
{
    public enum Drawing { Waves, DoubleWaves, Star, Diamond, Miss, Marker, Burst }
    public Drawing drawing;

    /// <summary>Builds the native geometry for one reusable timing element.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); Rect r = rectTransform.rect;
        if (drawing == Drawing.Marker)
        {
            Quad(mesh, r.xMin + 16, r.yMin + 24, 6, 66, new Color32(48,68,72,255));
            Quad(mesh, r.xMin + 18, r.yMin + 24, 2, 66, color);
            Color dark=new Color32(48,68,72,255);
            Diamond(mesh, new Vector2(r.center.x, r.yMax - 16), 19, 16,dark);
            Diamond(mesh, new Vector2(r.center.x, r.yMin + 10), 10, 11,dark);
            Diamond(mesh,new Vector2(r.center.x,r.yMax-16),15,12);
            Diamond(mesh,new Vector2(r.center.x,r.yMin+10),7,8);
            Diamond(mesh,new Vector2(r.center.x,r.yMax-16),5,7,new Color32(155,113,63,255));return;
        }
        if (drawing == Drawing.Burst)
        {
            for (int i=0; i<12; i++) { float a=i*Mathf.PI/6; Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)); Stroke(mesh,r.center+d*23,r.center+d*34,2); } return;
        }
        bool legend = r.width < 50;
        for (float x=legend?r.width*.5f:20; x<r.width; x+=40)
        {
            Vector2 center=new Vector2(r.xMin+x,r.center.y);
            if (drawing==Drawing.Diamond) Diamond(mesh,center,5,8);
            else if (drawing==Drawing.Star)
            { Stroke(mesh,center+Vector2.up*8,center-Vector2.up*8,2);Stroke(mesh,center+Vector2.right*8,center-Vector2.right*8,2); Stroke(mesh,center+new Vector2(-4,-4),center+new Vector2(4,4),1); }
            else if (drawing==Drawing.Miss) Stroke(mesh,center+new Vector2(-7,-12),center+new Vector2(7,12),2);
            else
            {
                int rows=drawing==Drawing.DoubleWaves?2:1;
                for(int j=0;j<rows;j++)
                    for(int i=0;i<12;i++)
                    { float a=-12+i*2,b=a+2; float y=(j-(rows-1)*.5f)*10; Stroke(mesh,center+new Vector2(a,y+Mathf.Sin(a*.25f)*3),center+new Vector2(b,y+Mathf.Sin(b*.25f)*3),1.5f); }
            }
        }
    }
    /// <summary>Adds an ivory diamond as two native UI triangles.</summary>
    private void Diamond(VertexHelper m,Vector2 c,float w,float h,Color? tint=null)
    {Color ink=tint??color;int n=m.currentVertCount;m.AddVert(c+Vector2.up*h,ink,Vector2.zero);m.AddVert(c+Vector2.right*w,ink,Vector2.zero);m.AddVert(c-Vector2.up*h,ink,Vector2.zero);m.AddVert(c-Vector2.right*w,ink,Vector2.zero);m.AddTriangle(n,n+1,n+2);m.AddTriangle(n,n+2,n+3); }
    /// <summary>Adds a colored rectangle.</summary>
    private void Quad(VertexHelper m,float x,float y,float w,float h,Color tint)
    { int n=m.currentVertCount;m.AddVert(new Vector2(x,y),tint,Vector2.zero);m.AddVert(new Vector2(x+w,y),tint,Vector2.zero);m.AddVert(new Vector2(x+w,y+h),tint,Vector2.zero);m.AddVert(new Vector2(x,y+h),tint,Vector2.zero);m.AddTriangle(n,n+1,n+2);m.AddTriangle(n,n+2,n+3); }
    /// <summary>Adds a crisp line segment.</summary>
    private void Stroke(VertexHelper m,Vector2 a,Vector2 b,float width)
    { Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*width*.5f;int k=m.currentVertCount;foreach(Vector2 p in new[]{a-n,b-n,b+n,a+n})m.AddVert(p,color,Vector2.zero);m.AddTriangle(k,k+1,k+2);m.AddTriangle(k,k+2,k+3); }
}
