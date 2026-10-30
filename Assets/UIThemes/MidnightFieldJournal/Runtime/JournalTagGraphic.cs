using UnityEngine;

/// <summary>Native R051 tag glyphs, tapered light and ivory-edged anchor; resolution independent at 4K.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalTagGraphic : UnityEngine.UI.MaskableGraphic
{
    public enum Drawing { Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive, Precision, Marker, Channel, ButtonRule }
    public Drawing drawing;
    private UnityEngine.UI.VertexHelper mesh;
    private Vector2 scale, origin;
    private static readonly Color Ink = new Color32(41,61,64,255), Ivory = new Color32(244,233,211,255);

    /// <summary>Builds independent meshes using the approved Penpot coordinates.</summary>
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper helper)
    {
        helper.Clear(); mesh=helper;
        Rect r=rectTransform.rect;
        origin=new Vector2(r.xMin,r.yMax);
        scale=new Vector2(r.width/32,r.height/32);
        if (drawing==Drawing.Precision) { Precision(); return; }
        if (drawing==Drawing.Marker) { scale=new Vector2(r.width/52,r.height/210); AnchorCursor(); return; }
        if (drawing==Drawing.Channel || drawing==Drawing.ButtonRule)
        {
            scale=Vector2.one;
            RoundedBox(r.width,r.height,drawing==Drawing.Channel?7:3,drawing==Drawing.Channel?1.3f:1);
            return;
        }
        switch(drawing)
        {
            case Drawing.Fish: Fish(0,0,1); break;
            case Drawing.Predator:
                Q(3,12,16,2,29,12); Poly(29,12,25,26,7,26,3,12);
                Poly(5,13,10,20,14,13,18,20,23,13); Poly(11,7,8,4); Poly(21,7,24,4); break;
            case Drawing.Schooling: Fish(1,1,.55f); Fish(14,9,.55f); Fish(1,17,.55f); break;
            case Drawing.Small:
                Q(10,16,16,9,22,16); Q(22,16,16,23,10,16); Poly(10,16,7,12,7,20,10,16);
                Poly(3,6,0,6,0,26,3,26); Poly(29,6,32,6,32,26,29,26); break;
            case Drawing.Heavy:
                C(10,10,10,-1,22,-1,22,10); Poly(7,11,25,11,29,29,3,29,7,11); break;
            case Drawing.Anchored: Anchor(16,5,1,1.7f,color); break;
            case Drawing.Armored:
                Poly(16,2,29,8,29,17); Q(29,17,27,27,16,31); Q(16,31,5,27,3,17); Poly(3,17,3,8,16,2);
                Poly(16,7,16,26); Poly(8,11,24,11); Poly(9,17,23,17); Poly(12,23,20,23); break;
            case Drawing.Elusive:
                C(25,3,2,3,2,29,25,29); C(25,29,14,24,14,8,25,3);
                Poly(1,10,5,10); Poly(0,17,5,17); Poly(1,24,5,24); break;
        }
    }

    /// <summary>Draws a small fish using the source's curved outline and tail.</summary>
    private void Fish(float x,float y,float s)
    {
        C(x+5*s,y+16*s,x+11*s,y+7*s,x+22*s,y+7*s,x+28*s,y+16*s);
        C(x+28*s,y+16*s,x+22*s,y+25*s,x+11*s,y+25*s,x+5*s,y+16*s);
        Poly(x+5*s,y+16*s,x+s,y+10*s,x+s,y+22*s,x+5*s,y+16*s);
        Circle(x+22*s,y+14*s,.6f*s,1,color);
    }

    /// <summary>Draws the source anchor with one eyelet, crossbar and curved flukes.</summary>
    private void Anchor(float x,float y,float s,float width,Color tint)
    {
        Circle(x,y,3*s,width,tint); Line(x,y+3*s,x,y+23*s,width,tint);
        Line(x-8*s,y+7*s,x+8*s,y+7*s,width,tint);
        Curve(new Vector2(x-13*s,y+14*s),new Vector2(x-13*s,y+27*s),new Vector2(x+13*s,y+27*s),new Vector2(x+13*s,y+14*s),width,tint);
        Line(x-13*s,y+14*s,x-15*s,y+19*s,width,tint); Line(x-13*s,y+14*s,x-8*s,y+16*s,width,tint);
        Line(x+13*s,y+14*s,x+15*s,y+19*s,width,tint); Line(x+13*s,y+14*s,x+8*s,y+16*s,width,tint);
    }

    /// <summary>Keeps the eyelet above the channel and the anchor below its lower edge.</summary>
    private void AnchorCursor()
    {
        Line(26,25,26,160,7,Ivory); Line(26,25,26,160,3.5f,Ink);
        Circle(26,16,11,6,Ivory); Circle(26,16,11,3,Ink); Circle(26,16,2,3,Ink);
        // Source anchor eyelet at166; stem172..202 and flukes187..205.
        CursorAnchor(7,Ivory); CursorAnchor(3.5f,Ink);
    }

    /// <summary>Layers an ivory halo and ink stroke to remain visible over every zone.</summary>
    private void CursorAnchor(float w,Color tint)
    {
        Circle(26,166,6,w,tint); Line(26,172,26,202,w,tint); Line(14,181,38,181,w,tint);
        Curve(new Vector2(6,187),new Vector2(6,211),new Vector2(46,211),new Vector2(46,187),w,tint);
        Line(6,187,3,194,w,tint);Line(6,187,13,190,w,tint);Line(46,187,49,194,w,tint);Line(46,187,39,190,w,tint);
    }

    /// <summary>Draws the centre light with the original nonuniform alpha stops, fading upward.</summary>
    private void Precision()
    {
        Rect r=rectTransform.rect;
        for(int i=0;i<24;i++)
        {
            float a=i/24f,b=(i+1)/24f;
            float wa=PrecisionHalfWidth(a)*r.width,wb=PrecisionHalfWidth(b)*r.width;
            Color ca=color,cb=color;ca.a*=LightAlpha(a);cb.a*=LightAlpha(b);
            int k=mesh.currentVertCount;
            mesh.AddVert(new Vector2(r.center.x-wa,r.yMax-a*r.height),ca,Vector2.zero);
            mesh.AddVert(new Vector2(r.center.x+wa,r.yMax-a*r.height),ca,Vector2.zero);
            mesh.AddVert(new Vector2(r.center.x+wb,r.yMax-b*r.height),cb,Vector2.zero);
            mesh.AddVert(new Vector2(r.center.x-wb,r.yMax-b*r.height),cb,Vector2.zero);
            mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
        }
    }

    /// <summary>Samples the source cubic light contour by height instead of drawing an opaque triangle.</summary>
    private static float PrecisionHalfWidth(float y)
    {
        float lo=0,hi=1;
        for(int i=0;i<12;i++){float t=(lo+hi)*.5f;float v=1.1f*t*(1-t)*(1-t)+2.2f*t*t*(1-t)+t*t*t;if(v<y)lo=t;else hi=t;}
        float u=(lo+hi)*.5f;return .6f*u*(1-u)*(1-u)+1.05f*u*u*(1-u)+.5f*u*u*u;
    }
    /// <summary>Evaluates the approved four gradient stops from transparent top to ochre base.</summary>
    private static float LightAlpha(float y)
    {if(y<.28f)return Mathf.Lerp(0,.04f,y/.28f);if(y<.62f)return Mathf.Lerp(.04f,.24f,(y-.28f)/.34f);return Mathf.Lerp(.24f,.62f,(y-.62f)/.38f);}

    /// <summary>Adds a quiet rounded channel rule with an editable native stroke.</summary>
    private void RoundedBox(float w,float h,float radius,float stroke)
    {
        PolyW(stroke,color,radius,0,w-radius,0);PolyW(stroke,color,w,radius,w,h-radius);PolyW(stroke,color,w-radius,h,radius,h);PolyW(stroke,color,0,h-radius,0,radius);
        for(int corner=0;corner<4;corner++)
        {float cx=corner==0||corner==3?radius:w-radius,cy=corner<2?radius:h-radius;
         float a=corner==0?Mathf.PI:corner==1?-Mathf.PI/2:corner==2?0:Mathf.PI/2;
         for(int j=0;j<8;j++){float p=a+j*Mathf.PI/16,q=p+Mathf.PI/16;Line(cx+Mathf.Cos(p)*radius,cy+Mathf.Sin(p)*radius,cx+Mathf.Cos(q)*radius,cy+Mathf.Sin(q)*radius,stroke,color);}}
    }
    /// <summary>Adds a sequence of crisp connected strokes.</summary>
    private void Poly(params float[] xy) { PolyW(1.7f,color,xy); }
    /// <summary>Adds a sequence with explicit stroke weight and tint.</summary>
    private void PolyW(float w,Color tint,params float[] xy){for(int i=2;i<xy.Length;i+=2)Line(xy[i-2],xy[i-1],xy[i],xy[i+1],w,tint);}
    /// <summary>Samples a quadratic as a cubic.</summary>
    private void Q(float x,float y,float cx,float cy,float ex,float ey)
    {C(x,y,x+(cx-x)*2/3,y+(cy-y)*2/3,ex+(cx-ex)*2/3,ey+(cy-ey)*2/3,ex,ey);}
    /// <summary>Samples a source cubic into native UI strokes.</summary>
    private void C(float x,float y,float a,float b,float c,float d,float ex,float ey)
    {Curve(new Vector2(x,y),new Vector2(a,b),new Vector2(c,d),new Vector2(ex,ey),1.7f,color);}
    /// <summary>Builds a smooth cubic line strip.</summary>
    private void Curve(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float w,Color tint)
    {Vector2 prev=a;for(int i=1;i<=20;i++){float t=i/20f,u=1-t;Vector2 p=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;Line(prev.x,prev.y,p.x,p.y,w,tint);prev=p;}}
    /// <summary>Builds an eyelet or glyph circle.</summary>
    private void Circle(float x,float y,float radius,float w,Color tint)
    {for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius,w,tint);}}
    /// <summary>Adds one source-space stroke transformed to the authored RectTransform.</summary>
    private void Line(float ax,float ay,float bx,float by,float width,Color tint)
    {
        Vector2 a=origin+new Vector2(ax*scale.x,-ay*scale.y),b=origin+new Vector2(bx*scale.x,-by*scale.y);
        Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*width*Mathf.Min(scale.x,scale.y)*.5f;
        int k=mesh.currentVertCount;mesh.AddVert(a-n,tint,Vector2.zero);mesh.AddVert(b-n,tint,Vector2.zero);mesh.AddVert(b+n,tint,Vector2.zero);mesh.AddVert(a+n,tint,Vector2.zero);mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
    }
}
