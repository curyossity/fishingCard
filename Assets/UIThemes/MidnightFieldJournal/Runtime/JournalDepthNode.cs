using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws the guide's hollow node or amber current-tier marker without a raster state overlay.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalDepthNode : MaskableGraphic
{
    private bool current;
    /// <summary>Updates only the marker's presentation state.</summary>
    public void SetCurrent(bool value) { current = value; SetVerticesDirty(); }
    /// <summary>Builds concentric ink discs with transparent amber halo geometry.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); Vector2 center = rectTransform.rect.center; float scale = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 82f;
        if (current)
        {
            Disc(mesh,center,41*scale,new Color(0.91f,0.64f,0.30f,.025f));
            Disc(mesh,center,33*scale,new Color(0.91f,0.64f,0.30f,.055f));
            Disc(mesh,center,25*scale,new Color(0.91f,0.64f,0.30f,.12f));
        }
        Disc(mesh,center,(current?23:19)*scale,current?(Color)new Color32(226,162,89,255):new Color32(155,169,170,255));
        Disc(mesh,center,(current?21:17)*scale,current?(Color)new Color32(246,191,116,255):new Color32(17,30,37,255));
        if(current){Disc(mesh,center,15*scale,new Color32(255,230,188,255));Disc(mesh,center,13*scale,new Color32(237,170,86,255));}
    }
    /// <summary>Appends a smooth filled native circle.</summary>
    private static void Disc(VertexHelper mesh,Vector2 center,float radius,Color tint)
    {
        int start=mesh.currentVertCount;mesh.AddVert(center,tint,Vector2.zero);
        for(int i=0;i<=48;i++){float a=i*Mathf.PI*2/48;mesh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,tint,Vector2.zero);}
        for(int i=0;i<48;i++)mesh.AddTriangle(start,start+i+1,start+i+2);
    }
}
