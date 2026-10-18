using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Renders individual real hand slots through an isolated perspective camera into the HUD.</summary>
public sealed class JournalTechniqueHandPresentation : TechniqueHandPresentation
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private TechniqueCardView cardPrefab;
    [SerializeField] private UnityEngine.UI.RawImage surface;
    [SerializeField] private TMP_Text pileText;
    [SerializeField] private TMP_Text restrictionText;
    [SerializeField, Range(0, 70)] private float restingTilt = 40;
    [SerializeField, Range(20, 60)] private float fieldOfView = 35;
    [SerializeField, Min(1000)] private float cameraDistance = 1500;
    [SerializeField, Min(0)] private float cardSpacing = 550;
    [SerializeField, Range(512, 4096)] private int maxRenderWidth = 3840;
    private GameObject stage;
    private Canvas stageCanvas;
    private Camera projectionCamera;
    private UnityEngine.UI.GraphicRaycaster stageRaycaster;
    private RenderTexture target;
    private readonly List<TechniqueCardView> cards = new List<TechniqueCardView>();
    private readonly List<JournalTechniquePose> poses = new List<JournalTechniquePose>();
    private readonly List<RectTransform> hitPlanes = new List<RectTransform>();
    private CardDefinition[] hand = Array.Empty<CardDefinition>();
    private bool[] playable = Array.Empty<bool>();
    private string[] restrictions = Array.Empty<string>();
    private Func<int, bool> useCard;
    private int drawCount, discardCount;
    private bool renderDirty = true;
    private long handVersion = -1, bindingGeneration;
    private readonly List<CanvasGroup> inputGroups = new List<CanvasGroup>();
    public IReadOnlyList<TechniqueCardView> Cards => cards;
    public IReadOnlyList<JournalTechniquePose> Poses => poses;
    public Camera ProjectionCamera => projectionCamera;
    public RenderTexture Target => target;
    public UnityEngine.UI.RawImage Surface => surface;
    public long RenderedFrameCount { get; private set; }

    /// <summary>Creates presentation resources only when the hand is enabled.</summary>
    private void OnEnable()
    {
        EnsureStage();
        if (hand.Length > 0) Refresh(hand, playable, restrictions, drawCount, discardCount, useCard, handVersion);
    }
    /// <summary>Releases the render target and detached camera hierarchy when the hand is hidden.</summary>
    private void OnDisable() { DisposeStage(); }
    /// <summary>Releases resources when the owning canvas or scene is removed.</summary>
    private void OnDestroy() { DisposeStage(); }

    /// <summary>Copies the current controller-owned hand and binds each slot's current index exactly once.</summary>
    public override void Refresh(CardDefinition[] newHand, bool[] newPlayable, string[] reasons,
        int drawCount, int discardCount, Func<int, bool> callback, long version = 0)
    {
        EnsureStage();
        bool changed = handVersion != version || hand.Length != (newHand?.Length ?? 0);
        if (!changed) for (int i = 0; i < hand.Length; i++) if (hand[i] != newHand[i]) changed = true;
        if (changed) foreach (var pose in poses) pose.ResetInspection();
        if (changed) bindingGeneration++;
        handVersion = version;
        hand = newHand == null ? Array.Empty<CardDefinition>() : (CardDefinition[])newHand.Clone();
        playable = newPlayable == null ? Array.Empty<bool>() : (bool[])newPlayable.Clone();
        restrictions = reasons == null ? Array.Empty<string>() : (string[])reasons.Clone();
        useCard = callback;
        this.drawCount = drawCount; this.discardCount = discardCount;
        if (pileText != null) pileText.text = $"DRAW {Mathf.Max(0, drawCount)}    DISCARD {Mathf.Max(0, discardCount)}";
        if (stageCanvas == null) return;
        while (cards.Count < hand.Length)
        {
            int index = cards.Count;
            TechniqueCardView card = Instantiate(cardPrefab, stageCanvas.transform);
            card.name = "Technique Slot " + (index + 1);
            SetLayer(card.gameObject, 31);
            card.SetUseHandler(() => { if (IsInputAllowed() && index < hand.Length && index < playable.Length && playable[index]) useCard?.Invoke(index); });
            cards.Add(card); poses.Add(card.gameObject.AddComponent<JournalTechniquePose>());
            var hit = new GameObject("Resting Hit Plane " + index, typeof(RectTransform)); hit.transform.SetParent(stageCanvas.transform, false);
            hitPlanes.Add(hit.GetComponent<RectTransform>());
        }
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(i < hand.Length && hand[i] != null);
            if (i >= hand.Length) continue;
            cards[i].SetCard(hand[i], "TECHNIQUE", i < playable.Length && playable[i], theme.FormatTechniqueRules(hand[i]));
            theme.TryGetArtwork(hand[i]?.UniqueId, out Sprite artwork); cards[i].SetArtwork(artwork);
            float center = i - (hand.Length - 1) * .5f;
            float step = Mathf.Min(cardSpacing, 2200f / Mathf.Max(1, hand.Length));
            Vector3 position = new Vector3(center * step, -Mathf.Abs(center) * 5, 0);
            Quaternion rotation = Quaternion.Euler(restingTilt, 0, center * 1.2f);
            RectTransform rect = cards[i].GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(620, 640);
            RectTransform hit = hitPlanes[i]; hit.anchorMin = hit.anchorMax = new Vector2(.5f, .5f);
            hit.sizeDelta = rect.sizeDelta; hit.localPosition = position; hit.localRotation = rotation;
            poses[i].Configure(hit, position, rotation, i, ShowRestriction);
            var button = cards[i].GetComponent<UnityEngine.UI.Button>();
            if (button is JournalTechniqueButton guarded) guarded.Bind(bindingGeneration);
            var navigation = button.navigation; navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            navigation.selectOnLeft = i > 0 ? cards[i - 1].GetComponent<UnityEngine.UI.Button>() : null;
            navigation.selectOnRight = i + 1 < hand.Length ? cards[i + 1].GetComponent<UnityEngine.UI.Button>() : null;
            button.navigation = navigation;
        }
        if (restrictionText != null && changed) restrictionText.text = string.Empty;
        renderDirty = true; RenderNow();
    }

    /// <summary>Refreshes the render target at output-pixel resolution, including focused cards.</summary>
    private void LateUpdate() { RenderNow(); }

    /// <summary>Renders the actual card meshes; also supports isolated Editor validation.</summary>
    public void RenderNow()
    {
        if (surface == null || projectionCamera == null) return;
        Canvas parent = surface.canvas;
        float scale = parent == null ? 1 : parent.scaleFactor;
        int width = Mathf.Clamp(Mathf.CeilToInt(surface.rectTransform.rect.width * scale), 256, maxRenderWidth);
        float aspect = Mathf.Max(.5f, surface.rectTransform.rect.width / Mathf.Max(1, surface.rectTransform.rect.height));
        int height = Mathf.Clamp(Mathf.CeilToInt(width / aspect), 128, 2160);
        if (target == null || target.width != width || target.height != height)
        {
            ReleaseTarget(); target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { name = "Journal Technique Hand", antiAliasing = 2, filterMode = FilterMode.Bilinear };
            target.Create(); surface.texture = target; projectionCamera.targetTexture = target;
            renderDirty = true;
        }
        if (!renderDirty) return;
        projectionCamera.aspect = width / (float)height;
        projectionCamera.transform.localPosition = new Vector3(0, 0, -cameraDistance * Mathf.Max(1, (2500f / 866f) / projectionCamera.aspect));
        Canvas.ForceUpdateCanvases(); projectionCamera.Render(); RenderedFrameCount++;
        renderDirty = false;
    }

    /// <summary>Maps HUD pointer coordinates to the offscreen camera's projected individual card graphics.</summary>
    public void RaycastProjected(PointerEventData pointer, Vector2 texturePoint, List<RaycastResult> hits)
    {
        if (stageRaycaster == null || !IsInputAllowed()) return;
        var mapped = new PointerEventData(EventSystem.current) { position = texturePoint };
        stageRaycaster.Raycast(mapped, hits);
        JournalTechniquePose front = hits.Count == 0 ? null : hits[0].gameObject.GetComponentInParent<JournalTechniquePose>();
        int frontOrder = front == null ? -1 : front.transform.GetSiblingIndex(), stable = -1;
        for (int i = 0; i < poses.Count && i < hand.Length; i++)
            if (poses[i].Inspecting && poses[i].transform.GetSiblingIndex() >= frontOrder
                && RectTransformUtility.RectangleContainsScreenPoint(poses[i].RestHit, texturePoint, projectionCamera))
            { stable = i; frontOrder = poses[i].transform.GetSiblingIndex(); }
        if (stable >= 0)
        { hits.Clear(); hits.Add(new RaycastResult { gameObject = cards[stable].gameObject, depth = 1000 }); }
    }

    /// <summary>Honors the owning HUD's CanvasGroup locks even though card meshes live on an offscreen stage.</summary>
    public bool IsInputAllowed()
    {
        if (!isActiveAndEnabled || surface == null) return false;
        inputGroups.Clear(); surface.GetComponentsInParent(false, inputGroups);
        foreach (CanvasGroup group in inputGroups)
        {
            if (!group.interactable || !group.blocksRaycasts) return false;
            if (group.ignoreParentGroups) break;
        }
        return true;
    }

    /// <summary>Shows controller-authored restrictions outside the printed card face.</summary>
    private void ShowRestriction(JournalTechniquePose pose, bool inspecting)
    {
        renderDirty = true;
        if (restrictionText == null) return;
        int index = poses.IndexOf(pose);
        if (inspecting) restrictionText.text = index >= 0 && index < restrictions.Length ? restrictions[index] : string.Empty;
        else if (!poses.Exists(p => p.Inspecting)) restrictionText.text = string.Empty;
    }

    /// <summary>Creates one detached camera and world-space Canvas, safely outside the gameplay camera's frustum.</summary>
    private void EnsureStage()
    {
        if (stage != null || surface == null || cardPrefab == null || theme == null) return;
        stage = new GameObject("Journal Technique Projection Stage") { hideFlags = HideFlags.DontSave };
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stage, gameObject.scene);
        stage.transform.position = new Vector3(0, 0, 100000);
        GameObject cameraObject = new GameObject("Hand Perspective Camera", typeof(Camera)); cameraObject.transform.SetParent(stage.transform, false);
        projectionCamera = cameraObject.GetComponent<Camera>(); projectionCamera.enabled = false;
        projectionCamera.scene = gameObject.scene; projectionCamera.clearFlags = CameraClearFlags.SolidColor; projectionCamera.backgroundColor = Color.clear;
        projectionCamera.cullingMask = 1 << 31; projectionCamera.fieldOfView = fieldOfView;
        projectionCamera.nearClipPlane = 10; projectionCamera.farClipPlane = 5000; projectionCamera.allowHDR = false;
        cameraObject.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        GameObject canvasObject = new GameObject("Projected Individual Cards", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster));
        canvasObject.layer = 31; canvasObject.transform.SetParent(stage.transform, false);
        stageCanvas = canvasObject.GetComponent<Canvas>(); stageCanvas.renderMode = RenderMode.WorldSpace; stageCanvas.worldCamera = projectionCamera;
        canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(3000, 1000);
        stageRaycaster = canvasObject.GetComponent<UnityEngine.UI.GraphicRaycaster>(); stageRaycaster.ignoreReversedGraphics = false;
        // Only the HUD proxy participates in EventSystem sorting; this raycaster is called with remapped coordinates.
        stageRaycaster.enabled = false;
    }

    /// <summary>Sets the private camera's culling layer throughout each generated card hierarchy.</summary>
    private static void SetLayer(GameObject root, int layer)
    { root.layer = layer; foreach (Transform child in root.transform) SetLayer(child.gameObject, layer); }

    /// <summary>Releases GPU ownership before resizing or disposing the hand.</summary>
    private void ReleaseTarget()
    {
        if (surface != null) surface.texture = null;
        if (projectionCamera != null) projectionCamera.targetTexture = null;
        if (target == null) return;
        target.Release(); Dispose(target); target = null;
    }

    /// <summary>Removes all detached resources and clears stale pooled references.</summary>
    private void DisposeStage()
    {
        ReleaseTarget(); if (stage != null) Dispose(stage);
        stage = null; stageCanvas = null; projectionCamera = null; stageRaycaster = null;
        cards.Clear(); poses.Clear(); hitPlanes.Clear();
    }

    /// <summary>Releases offscreen resources explicitly for Editor previews that do not receive runtime lifecycle callbacks.</summary>
    public void ReleaseResources() { DisposeStage(); }

    /// <summary>Uses the appropriate destruction lifetime for play and isolated Editor previews.</summary>
    private static void Dispose(UnityEngine.Object value)
    { if (Application.isPlaying) Destroy(value); else DestroyImmediate(value); }
}
