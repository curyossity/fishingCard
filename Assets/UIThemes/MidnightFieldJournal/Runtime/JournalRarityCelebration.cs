using UnityEngine;
using UnityEngine.UI;

/// <summary>A reusable, pointer-free burst of native UI gold/ivory particles for a top-rarity stop.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class JournalRarityCelebration : MaskableGraphic
{
    private const int ParticleCount=42;
    private const float Duration=1.05f;
    private struct Spark { public Vector2 velocity; public float delay,life,size,rotation; public Color tint; }
    private readonly Spark[] sparks=new Spark[ParticleCount];
    private float startedAt;
    private long playedOccurrence=long.MinValue;
    private bool playing;
    public bool IsPlaying=>playing;
    public int EmissionCount {get;private set;}

    /// <summary>Recognizes the highest displayed creature rarity, excluding optional Miss intervals.</summary>
    public static bool ShouldCelebrate(EncounterRarityTiming timing)
    {
        if(timing==null||!timing.Stopped||timing.Outcome.IsMiss)return false;
        foreach(var zone in timing.Zones)
            if(!zone.IsMiss&&(int)zone.Rarity>(int)timing.Outcome.Rarity)return false;
        return true;
    }

    /// <summary>Emits once per stopped occurrence using private visual randomness, independent of encounter selection.</summary>
    public bool Play(long occurrence)
    {
        if(playedOccurrence==occurrence)return false;
        playedOccurrence=occurrence;EmissionCount++;playing=true;startedAt=Time.unscaledTime;
        var random=new System.Random(unchecked((int)(occurrence^(occurrence>>32))));
        for(int i=0;i<sparks.Length;i++)
        {
            float angle=Mathf.Lerp(12,168,(float)random.NextDouble())*Mathf.Deg2Rad;
            float speed=Mathf.Lerp(260,570,(float)random.NextDouble());
            sparks[i]=new Spark {velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed,
                delay=(float)random.NextDouble()*.08f,life=Mathf.Lerp(.55f,.95f,(float)random.NextDouble()),
                size=Mathf.Lerp(5,13,(float)random.NextDouble()),rotation=(float)random.NextDouble()*Mathf.PI,
                tint=i%3==0?new Color32(255,245,211,255):new Color32(235,183,85,255)};
        }
        raycastTarget=false;SetVerticesDirty();return true;
    }

    /// <summary>Removes all particles on cancellation/reuse without making the same stop emit twice.</summary>
    public void Clear() {playing=false;SetVerticesDirty();}

    /// <summary>Advances only the visual burst; no gameplay clock or selection state changes.</summary>
    private void Update()
    {
        if(!playing)return;
        if(Time.unscaledTime-startedAt>=Duration)Clear();else SetVerticesDirty();
    }

    /// <summary>Clears transient particles when their instrument is hidden.</summary>
    protected override void OnDisable() {Clear();base.OnDisable();}

    /// <summary>Draws pooled glint particles, their fine trails and soft geometric halos in the normal UI canvas.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();if(!playing)return;
        float elapsed=Time.unscaledTime-startedAt;
        for(int i=0;i<sparks.Length;i++)
        {
            Spark spark=sparks[i];float age=elapsed-spark.delay;if(age<0||age>=spark.life)continue;
            float t=age/spark.life;float alpha=(1-t)*(1-t);
            Vector2 position=spark.velocity*age+Vector2.down*(190*age*age);
            float size=spark.size*(1-.45f*t);Color tint=spark.tint;tint.a=alpha;
            Color halo=tint;halo.a*=.12f;Diamond(mesh,position,size*2.5f,spark.rotation+age,halo);
            Vector2 direction=spark.velocity.normalized;
            Line(mesh,position-direction*(12+20*t),position,1.8f,tint);
            Diamond(mesh,position,size,spark.rotation+age*2,tint);
            if(i%3==0)
            {Line(mesh,position-Vector2.up*size*1.5f,position+Vector2.up*size*1.5f,1.5f,tint);Line(mesh,position-Vector2.right*size*1.5f,position+Vector2.right*size*1.5f,1.5f,tint);}
        }
    }

    /// <summary>Adds one rotated glint with its own independent opacity.</summary>
    private static void Diamond(VertexHelper mesh,Vector2 center,float radius,float angle,Color tint)
    {
        int start=mesh.currentVertCount;
        for(int i=0;i<4;i++){float a=angle+i*Mathf.PI*.5f;mesh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,tint,Vector2.zero);}
        mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
    }

    /// <summary>Adds one fine particle trail without allocating a separate GameObject.</summary>
    private static void Line(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color tint)
    {
        Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*width*.5f;int start=mesh.currentVertCount;
        mesh.AddVert(a-n,tint,Vector2.zero);mesh.AddVert(b-n,tint,Vector2.zero);mesh.AddVert(b+n,tint,Vector2.zero);mesh.AddVert(a+n,tint,Vector2.zero);
        mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
    }
}
