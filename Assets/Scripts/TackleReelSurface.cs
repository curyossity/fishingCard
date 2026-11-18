using UnityEngine;

/// <summary>Presentation-only alternatives for the rotating drum bands.</summary>
public enum ReelEdgeAnimationStyle { ContinuousWear, SegmentedBands }

/// <summary>Owns each wheel's paper/edge materials, using one cylinder phase with fixed clipping, outline and light.</summary>
public sealed class TackleReelSurface : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.RawImage surface;
    [SerializeField] private Material surfaceMaterial;
    [SerializeField] private UnityEngine.UI.RawImage edgeSurface;
    [SerializeField] private ReelEdgeAnimationStyle edgeAnimation;
    private Material instance, edgeInstance;
    private float currentRadius = 172;
    private static readonly int PhaseId = Shader.PropertyToID("_Phase");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SizeId = Shader.PropertyToID("_CylinderSize");
    private static readonly int AnimateEdgesId = Shader.PropertyToID("_AnimateEdges");
    private static readonly int EdgeLayerId = Shader.PropertyToID("_EdgeLayer");
    private static readonly int EdgeStyleId = Shader.PropertyToID("_EdgeStyle");
    public float Phase { get; private set; }
    public Material MaterialInstance => instance;
    public Material EdgeMaterialInstance => edgeInstance;

    /// <summary>Authors the independent surface renderer without mutating the shared material.</summary>
    public void Configure(UnityEngine.UI.RawImage image, Material material) { surface = image; surfaceMaterial = material; }

    /// <summary>Binds the drum's outer material layer separately from the curved paper aperture.</summary>
    public void ConfigureEdges(UnityEngine.UI.RawImage image) { edgeSurface = image; }

    /// <summary>Creates the private renderer state lazily, retaining it across Discover's enable/disable cycle.</summary>
    private void OnEnable() { ApplyRotation(Phase, currentRadius); }

    /// <summary>Copies the exact unscaled animation phase used by the native symbols and captions.</summary>
    public void ApplyRotation(float radians, float radius)
    {
        Phase = radians;
        currentRadius = radius;
        if (surface == null || surfaceMaterial == null) return;
        ApplyLayer(surface,ref instance,0,radians,radius);
        if (edgeSurface != null) ApplyLayer(edgeSurface,ref edgeInstance,1,radians,radius);
    }

    /// <summary>Reuses one private material per layer and synchronizes any uGUI stencil-derived rendering copy.</summary>
    private void ApplyLayer(UnityEngine.UI.RawImage image, ref Material material, int layer, float radians, float radius)
    {
        if (material == null) material = new Material(surfaceMaterial) { name = layer == 0 ? "Reel Surface (instance)" : "Reel Edges (instance)" };
        image.material = material;
        Vector2 size = image.rectTransform.rect.size;
        SetLayerState(material,layer,radians,radius,size);
        // uGUI caches a stencil-derived copy; animate that renderer material along with the private base material.
        var rendered = image.materialForRendering;
        if (rendered != material) SetLayerState(rendered,layer,radians,radius,size);
    }

    /// <summary>Copies the same resolved cylinder state into paper and edge layers without consuming gameplay randomness.</summary>
    private void SetLayerState(Material material, int layer, float radians, float radius, Vector2 size)
    {
        material.SetFloat(PhaseId,radians); material.SetFloat(RadiusId,radius);
        material.SetVector(SizeId,new Vector4(size.x,size.y,0,0));
        material.SetFloat(AnimateEdgesId,edgeSurface != null ? 1 : 0);
        material.SetFloat(EdgeLayerId,layer);
        material.SetFloat(EdgeStyleId,(int)edgeAnimation);
    }

    /// <summary>Releases only this wheel's runtime material; authored assets remain untouched.</summary>
    private void OnDestroy()
    {
        foreach (var material in new[] { instance,edgeInstance })
        {
            if (material == null) continue;
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }
    }
}
