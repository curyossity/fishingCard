using UnityEngine;

/// <summary>Owns one wheel's material instance so paper and decorations use its symbol rotation while rim and light stay fixed.</summary>
public sealed class TackleReelSurface : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.RawImage surface;
    [SerializeField] private Material surfaceMaterial;
    private Material instance;
    private float currentRadius = 172;
    private static readonly int PhaseId = Shader.PropertyToID("_Phase");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SizeId = Shader.PropertyToID("_CylinderSize");
    public float Phase { get; private set; }
    public Material MaterialInstance => instance;

    /// <summary>Authors the independent surface renderer without mutating the shared material.</summary>
    public void Configure(UnityEngine.UI.RawImage image, Material material) { surface = image; surfaceMaterial = material; }

    /// <summary>Creates the private renderer state lazily, retaining it across Discover's enable/disable cycle.</summary>
    private void OnEnable() { ApplyRotation(Phase, currentRadius); }

    /// <summary>Copies the exact unscaled animation phase used by the native symbols and captions.</summary>
    public void ApplyRotation(float radians, float radius)
    {
        Phase = radians;
        currentRadius = radius;
        if (surface == null || surfaceMaterial == null) return;
        if (instance == null) instance = new Material(surfaceMaterial) { name = "Reel Surface (instance)" };
        surface.material = instance;
        instance.SetFloat(PhaseId, radians); instance.SetFloat(RadiusId, radius);
        Vector2 size = surface.rectTransform.rect.size;
        instance.SetVector(SizeId, new Vector4(size.x, size.y, 0, 0));
    }

    /// <summary>Releases only this wheel's runtime material; authored assets remain untouched.</summary>
    private void OnDestroy()
    {
        if (instance != null)
        {
            if (Application.isPlaying) Destroy(instance);
            else DestroyImmediate(instance);
        }
    }
}
