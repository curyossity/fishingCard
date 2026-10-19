using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws a rounded engraved gauge segment and highlight using native UI geometry, without a sprite.</summary>
public sealed class JournalLoadSegment : Image
{
    [SerializeField] private Color borderColor=new Color32(91,99,94,255);
    [SerializeField] private bool highlighted;

    /// <summary>Changes the state trim without creating materials or additional UI objects.</summary>
    public void SetState(bool filled,bool warning)
    {
        borderColor=filled?(warning?new Color32(147,74,53,255):new Color32(38,53,57,255)):new Color32(166,140,107,255);
        highlighted=false;SetVerticesDirty();
    }

    /// <summary>Builds a rounded border, inset face and narrow engraved upper highlight.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();Rect r=GetPixelAdjustedRect();
        Rounded(mesh,r,3,borderColor);
        r=new Rect(r.x+1,r.y+1,Mathf.Max(0,r.width-2),Mathf.Max(0,r.height-2));Rounded(mesh,r,2,color);
        if(highlighted&&r.width>6&&r.height>5)Rounded(mesh,new Rect(r.x+2,r.yMax-4,r.width-4,2),1,borderColor);
    }

    /// <summary>Appends one rounded convex face, clamping the radius for narrow capacity segments.</summary>
    private static void Rounded(VertexHelper mesh,Rect r,float radius,Color tint)
    {
        if(r.width<=0||r.height<=0)return;
        radius=Mathf.Min(radius,Mathf.Min(r.width,r.height)/2);int start=mesh.currentVertCount;
        mesh.AddVert(r.center,tint,Vector2.zero);
        const int steps=4;int ring=4*(steps+1);
        for(int corner=0;corner<4;corner++)
        {
            Vector2 center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
            for(int i=0;i<=steps;i++)
            {
                float angle=(corner*90+i*90f/steps)*Mathf.Deg2Rad;
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint,Vector2.zero);
            }
        }
        for(int i=0;i<ring;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%ring);
    }
}
