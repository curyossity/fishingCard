using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>One non-intercepting, screen-clamped tooltip per phase, driven by bound rig snapshots.</summary>
public sealed class TackleCardTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform overlay, panel, outline;
    [SerializeField] private TMP_Text title, rarity, metadata, abilityName, rules, context;
    [SerializeField, Min(0)] private float hoverDelay = .18f, hideDelay = .08f;
    private TackleCardTooltipSource source;
    private Canvas canvas;
    private double showAt, hideAt = double.PositiveInfinity;
    private Vector2 sourcePosition;
    private bool buildPhase;
    private readonly Vector3[] corners = new Vector3[4];
    private readonly InputAction dismiss = new InputAction("Dismiss creature details", InputActionType.Button, "<Keyboard>/escape");
    public bool IsVisible => panel != null && panel.gameObject.activeSelf;
    public TackleCardTooltipSource Source => source;

    /// <summary>Uses phase-local input and a canvas outside both ScrollRect masks.</summary>
    private void Awake() { canvas = GetComponent<Canvas>(); buildPhase = GetComponent<RigBuildingView>() != null; dismiss.AddBinding("<Gamepad>/buttonEast"); dismiss.performed += OnDismiss; Hide(); }
    /// <summary>Enables only the presentation dismissal action.</summary>
    private void OnEnable() { dismiss.Enable(); Canvas.willRenderCanvases += BeforeRender; }
    /// <summary>Clears pending hover work at every phase transition.</summary>
    private void OnDisable() { dismiss.Disable(); Canvas.willRenderCanvases -= BeforeRender; Hide(); }
    /// <summary>Reclamps visible details after canvas scaling, aspect changes and camera-backed rendering.</summary>
    private void BeforeRender() { if (IsVisible && source != null) Position(); }
    /// <summary>Releases the owned native input action.</summary>
    private void OnDestroy() { dismiss.performed -= OnDismiss; dismiss.Dispose(); }
    /// <summary>Dismisses without submitting a placement, strike or navigation command.</summary>
    private void OnDismiss(InputAction.CallbackContext _) { Hide(); }
    /// <summary>Replaces pending inspection immediately when moving between pooled cards.</summary>
    public void Request(TackleCardTooltipSource owner, bool immediate)
    {
        Hide();
        if (owner == null || owner.Member == null || !owner.Member.IsRigCreature) return;
        source = owner; showAt = Time.realtimeSinceStartupAsDouble + (immediate ? 0 : hoverDelay);
        sourcePosition = owner.FaceBounds.position;
        if (immediate) Show();
    }
    /// <summary>Allows a short pointer-exit grace period without intercepting any raycasts.</summary>
    public void Release(TackleCardTooltipSource owner) { if (source == owner) hideAt = Time.realtimeSinceStartupAsDouble + hideDelay; }
    /// <summary>Rejects stale leave/disable notifications from a previously inspected source.</summary>
    public void Dismiss(TackleCardTooltipSource owner) { if (source == owner) Hide(); }
    /// <summary>Clears both pending and visible inspection; no state survives phase exit.</summary>
    public void Hide()
    {
        source = null; hideAt = double.PositiveInfinity;
        if (panel != null) panel.gameObject.SetActive(false);
        if (outline != null) outline.gameObject.SetActive(false);
    }
    /// <summary>Advances presentation deadlines and dismisses a source moved by scrolling.</summary>
    private void LateUpdate()
    {
        if (source == null) return;
        if (!source.isActiveAndEnabled || source.Member == null || !Application.isFocused
            || ((Vector2)source.FaceBounds.position - sourcePosition).sqrMagnitude > .25f
            || Time.realtimeSinceStartupAsDouble >= hideAt) { Hide(); return; }
        if (!IsVisible && Time.realtimeSinceStartupAsDouble >= showAt) Show();
    }
    /// <summary>Formats only authoritative current-level data; the tooltip never evaluates abilities.</summary>
    private void Show()
    {
        var member = source.Member;
        title.text = member.Definition.DisplayName.ToUpperInvariant();
        rarity.text = member.Definition.Rarity.ToString().ToUpperInvariant();
        string value = member.HidesOwnValueDuringRun ? "?" : member.HaulValueContribution.ToString();
        metadata.text = "<color=#E9BC63>" + string.Join(" / ", member.Tags) + "</color>    Lv " + member.Level + "    SALE " + value;
        abilityName.text = member.PrimaryAbility.Name;
        rules.text = RigAbilityText.Describe(member.PrimaryAbility, member.Level);
        string plain = RigAbilityText.Describe(member.PrimaryAbility, member.Level, false);
        context.text = source.AfterPlacement ? "AFTER PLACEMENT\nValues preview the selected insertion. Abilities help future landings."
            : plain.IndexOf("contiguous", StringComparison.OrdinalIgnoreCase) >= 0 ? "Contiguous: a connected group. Another tag breaks the group."
            : plain.IndexOf("consecutive", StringComparison.OrdinalIgnoreCase) >= 0 ? "Consecutive: back-to-back strikes in this encounter."
            : "";
        float y = 24;
        y = Place(title, y, 12); y = Place(rarity, y, 14); y = Place(metadata, y, 24);
        y = Place(abilityName, y, 12); y = Place(rules, y, string.IsNullOrEmpty(context.text) ? 0 : 20);
        if (!string.IsNullOrEmpty(context.text)) y = Place(context, y, 0);
        context.gameObject.SetActive(!string.IsNullOrEmpty(context.text));
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y + 24);
        panel.gameObject.SetActive(true); outline.gameObject.SetActive(true);
        Position();
    }
    /// <summary>Wraps at the authored font size and grows the panel rather than shrinking rules.</summary>
    private float Place(TMP_Text text, float y, float gap)
    {
        float width = panel.rect.width - 48;
        float height = Mathf.Ceil(text.GetPreferredValues(text.text, width, float.PositiveInfinity).y);
        text.rectTransform.anchoredPosition = new Vector2(24, -y);
        text.rectTransform.sizeDelta = new Vector2(width, height);
        return y + height + gap;
    }
    /// <summary>Places side/above details in canvas units, keeping the entire panel inside the safe area.</summary>
    private void Position()
    {
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Rect safePixels = camera != null && camera.targetTexture != null ? new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height) : Screen.safeArea;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, safePixels.min, camera, out var minimum);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, safePixels.max, camera, out var maximum);
        Rect safe = Rect.MinMaxRect(minimum.x + 16, minimum.y + 16, maximum.x - 16, maximum.y - 16);
        source.FaceBounds.GetWorldCorners(corners);
        Vector2 bottom = overlay.InverseTransformPoint(corners[0]), top = overlay.InverseTransformPoint(corners[2]);
        float width = panel.rect.width, height = panel.rect.height;
        float x = source.Above ? (bottom.x + top.x - width) * .5f : top.x + 20;
        float y = source.Above ? top.y + height + 20 : top.y;
        if (!source.Above && x + width > safe.xMax) x = bottom.x - width - 20;
        if (source.Above && y > safe.yMax) y = bottom.y - 20;
        // Cabinet details stay above the insertion controls in the machine's lower recess.
        if (!source.Above && buildPhase) y = Mathf.Max(y, -50 + height);
        panel.anchoredPosition = new Vector2(Mathf.Clamp(x, safe.xMin, Mathf.Max(safe.xMin, safe.xMax - width)), Mathf.Clamp(y, safe.yMin + height, Mathf.Max(safe.yMin + height, safe.yMax)));
        outline.anchoredPosition = new Vector2(bottom.x - 4, top.y + 4);
        outline.sizeDelta = new Vector2(top.x - bottom.x + 8, top.y - bottom.y + 8);
    }
}
