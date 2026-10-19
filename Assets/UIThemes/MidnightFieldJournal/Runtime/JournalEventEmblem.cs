using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws editable event category symbols as native UI strokes, independently of the painted frame.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalEventEmblem : MaskableGraphic
{
    [SerializeField] private CardType category = CardType.Hazard;

    /// <summary>Updates the visual category without interpreting or resolving event rules.</summary>
    public void SetCategory(CardType value) { category=value;SetVerticesDirty(); }

    /// <summary>Uses the same forty-unit source geometry as the Penpot event medallions.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if(category==CardType.Hazard)
        {
            Path(mesh,new[]{new Vector2(20,6),new Vector2(34,31),new Vector2(6,31),new Vector2(20,6)});
            Stroke(mesh,new Vector2(20,15),new Vector2(20,23));
            Stroke(mesh,new Vector2(20,26),new Vector2(20,28));
        }
        else if(category==CardType.Environment)
        {
            Path(mesh,new[]{new Vector2(12,29),new Vector2(12,12),new Vector2(7,17)});
            Stroke(mesh,new Vector2(12,12),new Vector2(17,17));
            Path(mesh,new[]{new Vector2(28,29),new Vector2(28,12),new Vector2(23,17)});
            Stroke(mesh,new Vector2(28,12),new Vector2(33,17));
        }
        else
            Path(mesh,new[]{new Vector2(20,6),new Vector2(24,16),new Vector2(34,20),new Vector2(24,24),new Vector2(20,34),new Vector2(16,24),new Vector2(6,20),new Vector2(16,16),new Vector2(20,6)});
    }

    /// <summary>Connects consecutive points of an open or explicitly closed source path.</summary>
    private void Path(VertexHelper mesh,Vector2[] points) { for(int i=1;i<points.Length;i++)Stroke(mesh,points[i-1],points[i]); }

    /// <summary>Scales a source stroke to the current canvas rectangle.</summary>
    private void Stroke(VertexHelper mesh,Vector2 a,Vector2 b)
    {
        Rect r=rectTransform.rect;
        a=new Vector2(r.xMin+a.x*r.width/40,r.yMax-a.y*r.height/40);
        b=new Vector2(r.xMin+b.x*r.width/40,r.yMax-b.y*r.height/40);
        Vector2 delta=b-a;
        Vector2 n=new Vector2(-delta.y,delta.x).normalized*1.25f*Mathf.Min(r.width,r.height)/40;
        int k=mesh.currentVertCount;
        mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);
        mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);
        mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);
    }
}
