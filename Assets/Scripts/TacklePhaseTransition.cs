using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Changes the Tackle Apparatus between Discover, Landing and Build with a roller shutter. The outgoing phase canvas is rendered once
/// into a texture just before the views switch and held on screen; a slatted shutter drops over the machine window, lands with a small
/// bounce and jolt, the held frame is removed while the window is covered, and the shutter rolls back up over the new phase. The three
/// phases share a pixel-identical outer shell, so only the interior appears to change. Presentation only: nothing waits on it.
/// </summary>
public sealed class TacklePhaseTransition : MonoBehaviour
{
    [SerializeField] private Canvas overlay;
    [SerializeField] private RectTransform content, interiorWindow;
    [SerializeField] private RawImage fullSnapshot, blocker;
    [SerializeField] private TackleShutterGraphic shutter;
    /// <summary>Bounding box of the machine body (excluding the external pipes) in 1920x1080 stage pixels, top-left origin. Authored by setup.</summary>
    [SerializeField] private Rect bodyBounds = new Rect(580, 163, 1220, 681);
    public Rect BodyBounds => bodyBounds;
    public const float CloseSeconds = .18f, SettleSeconds = .08f, OpenSeconds = .22f;
    public const float Duration = CloseSeconds + SettleSeconds + OpenSeconds, ReducedDuration = .2f;
    /// <summary>When the held outgoing frame is removed: after the shutter has landed, before it starts to open.</summary>
    public const float SwapAt = CloseSeconds + .04f;
    private RenderTexture texture;
    private double startedAt;
    private bool playing, reduced;
    public bool IsPlaying => playing;
    /// <summary>Seconds until the new interior is half uncovered; Landing delays its creature entrance by this much.</summary>
    public float LeadSeconds => isActiveAndEnabled ? (LandingScoreRush.ReducedMotionPreferred ? ReducedDuration : CloseSeconds + SettleSeconds + OpenSeconds * .5f) : 0;

    /// <summary>Shutter extent over time: gravity drop, small rebound on landing, then an eased roll back up.</summary>
    public static float Extent(float age)
    {
        if (age <= 0) return 0;
        if (age < CloseSeconds) { float u = age / CloseSeconds; return u * u; }
        if (age < CloseSeconds + SettleSeconds) return 1 - .035f * Mathf.Sin(Mathf.PI * (age - CloseSeconds) / SettleSeconds);
        float open = Mathf.Clamp01((age - CloseSeconds - SettleSeconds) / OpenSeconds);
        return 1 - Mathf.SmoothStep(0, 1, open);
    }

    /// <summary>Downward jolt of the whole held frame when the shutter lands; zero outside the impact window.</summary>
    public static float Jolt(float age)
    {
        float hit = age - CloseSeconds;
        if (hit < 0 || hit > SettleSeconds) return 0;
        return 4 * Mathf.Exp(-hit * 40) * Mathf.Cos(hit * 70);
    }

    /// <summary>Snapshots the outgoing phase canvas (still showing its last frame) and starts the change; call before the views switch.</summary>
    public bool Begin(Canvas outgoing, RectTransform outgoingStage)
    {
        if (!isActiveAndEnabled || outgoing == null || outgoingStage == null || !outgoing.gameObject.activeInHierarchy) return false;
        // Measure while the outgoing canvas is still in overlay mode; the snapshot briefly moves it into camera space.
        PlaceInterior(outgoingStage);
        if (!Snapshot(outgoing)) return false;
        reduced = LandingScoreRush.ReducedMotionPreferred;
        startedAt = Time.realtimeSinceStartupAsDouble; playing = true;
        overlay.enabled = true; blocker.gameObject.SetActive(true);
        fullSnapshot.texture = texture; fullSnapshot.color = Color.white; fullSnapshot.gameObject.SetActive(true);
        interiorWindow.gameObject.SetActive(!reduced);
        Render(0);
        return true;
    }

#if UNITY_EDITOR
    private bool frozen;
    /// <summary>Editor review only: holds the transition at an exact age so slow captures sample the intended frame.</summary>
    public void Seek(float age)
    {
        if (!playing) return;
        frozen = true; startedAt = Time.realtimeSinceStartupAsDouble - age; Render(age);
    }
    /// <summary>Editor review only: resumes normal timing (from the current seek point).</summary>
    public void Release() { frozen = false; }
#endif

    /// <summary>Samples the change on the unscaled clock and releases the overlay when finished.</summary>
    private void Update()
    {
        if (!playing) return;
#if UNITY_EDITOR
        if (frozen) return;
#endif
        float age = (float)(Time.realtimeSinceStartupAsDouble - startedAt);
        if (age >= (reduced ? ReducedDuration : Duration)) { Finish(); return; }
        Render(age);
    }

    private void Render(float age)
    {
        if (reduced)
        {
            fullSnapshot.color = new Color(1, 1, 1, 1 - Mathf.SmoothStep(0, 1, age / ReducedDuration));
            return;
        }
        shutter.Extent = Extent(age);
        content.anchoredPosition = new Vector2(0, -Jolt(age));
        // The held outgoing frame disappears only while the window is fully covered; labels outside it swap with the landing.
        fullSnapshot.gameObject.SetActive(age < SwapAt);
    }

    /// <summary>Ends immediately, leaving the incoming phase fully visible; safe to call at any time.</summary>
    public void Finish()
    {
        playing = false;
#if UNITY_EDITOR
        frozen = false;
#endif
        if (fullSnapshot != null) { fullSnapshot.gameObject.SetActive(false); fullSnapshot.texture = null; }
        if (shutter != null) shutter.Extent = 0;
        if (content != null) content.anchoredPosition = Vector2.zero;
        if (interiorWindow != null) interiorWindow.gameObject.SetActive(false);
        if (blocker != null) blocker.gameObject.SetActive(false);
        if (overlay != null) overlay.enabled = false;
    }

    /// <summary>Places the shutter over the outgoing stage's machine body using screen-normalized anchors.</summary>
    private void PlaceInterior(RectTransform stage)
    {
        Rect r = bodyBounds; Rect s = stage.rect;
        // Stage children use top-left anchoring in 1920x1080 reference pixels; overlay world space is screen pixels.
        Vector3 topLeft = stage.TransformPoint(new Vector3(s.xMin + r.xMin * s.width / 1920f, s.yMax - r.yMin * s.height / 1080f));
        Vector3 bottomRight = stage.TransformPoint(new Vector3(s.xMin + r.xMax * s.width / 1920f, s.yMax - r.yMax * s.height / 1080f));
        // Normalized anchors avoid relying on the overlay's own layout, which has not run the first time it is used.
        float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
        Vector2 min = Vector2.Min(topLeft, bottomRight), max = Vector2.Max(topLeft, bottomRight);
        interiorWindow.anchorMin = new Vector2(min.x / w, min.y / h); interiorWindow.anchorMax = new Vector2(max.x / w, max.y / h);
        interiorWindow.pivot = new Vector2(.5f, .5f); interiorWindow.offsetMin = interiorWindow.offsetMax = Vector2.zero;
    }

    /// <summary>Renders only the outgoing canvas into a screen-sized texture, then restores its overlay mode.</summary>
    private bool Snapshot(Canvas canvas)
    {
        int width = Mathf.Max(1, Screen.width), height = Mathf.Max(1, Screen.height);
        if (texture == null || texture.width != width || texture.height != height)
        {
            if (texture != null) texture.Release();
            texture = new RenderTexture(width, height, 24) { name = "Tackle phase snapshot" };
        }
        var cameraObject = new GameObject("Tackle phase snapshot camera", typeof(Camera)) { hideFlags = HideFlags.HideAndDontSave };
        var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
        camera.transform.position = new Vector3(0, 0, -1000); camera.orthographic = true; camera.orthographicSize = height * .5f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.targetTexture = texture;
        var mode = canvas.renderMode; var previousCamera = canvas.worldCamera; float distance = canvas.planeDistance;
        try
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 100;
            Canvas.ForceUpdateCanvases();
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = texture };
                if (!UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(camera, request)) return false;
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
            }
            else camera.Render();
            return true;
        }
        catch (System.Exception error) { Debug.LogWarning("Phase transition snapshot unavailable: " + error.Message); return false; }
        finally
        {
            canvas.renderMode = mode; canvas.worldCamera = previousCamera; canvas.planeDistance = distance;
            camera.targetTexture = null; Destroy(cameraObject);
        }
    }

    private void Awake() { Finish(); }
    private void OnDisable() { Finish(); }
    private void OnDestroy() { if (texture != null) { texture.Release(); Destroy(texture); } }
}
