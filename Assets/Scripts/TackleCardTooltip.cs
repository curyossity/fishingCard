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
    private LandingContributionView detailSource;
    private Canvas canvas;
    private double showAt, hideAt = double.PositiveInfinity;
    private Vector2 sourcePosition;
    private bool buildPhase;
    private string relationshipContext;
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
    private void BeforeRender() { if (IsVisible && (source != null || detailSource != null)) Position(); }
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
        if (detailSource != null) detailSource.SetInspection(false);
        detailSource = null;
        relationshipContext = null;
        source = null; hideAt = double.PositiveInfinity;
        if (panel != null) panel.gameObject.SetActive(false);
        if (outline != null) outline.gameObject.SetActive(false);
    }
    /// <summary>Advances presentation deadlines and dismisses a source moved by scrolling.</summary>
    private void LateUpdate()
    {
        if (source == null && detailSource == null) return;
        var anchor = detailSource != null ? detailSource.Anchor : source.FaceBounds;
        if ((detailSource != null ? !detailSource.isActiveAndEnabled : !source.isActiveAndEnabled || source.Member == null) || !Application.isFocused
            || ((Vector2)anchor.position - sourcePosition).sqrMagnitude > .25f
            || Time.realtimeSinceStartupAsDouble >= hideAt) { Hide(); return; }
        if (!IsVisible && Time.realtimeSinceStartupAsDouble >= showAt) Show();
    }
    /// <summary>Uses the same overlay for resolved source explanations, including sources outside the visible cabinet.</summary>
    public void RequestDetail(LandingContributionView owner)
    {
        Hide(); detailSource = owner; sourcePosition = owner.Anchor.position;
        title.text = owner.Title.ToUpperInvariant(); rarity.text = owner.Subtitle;
        metadata.text = owner.Amount; abilityName.text = "CONTRIBUTION";
        rules.text = owner.Explanation; context.text = owner.Context;
        LayoutAndShow(); owner.SetInspection(true);
    }
    /// <summary>Ignores stale exit events from a previously inspected contribution.</summary>
    public void DismissDetail(LandingContributionView owner) { if (detailSource == owner) Hide(); }
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
        context.richText = true;
        if (!string.IsNullOrEmpty(relationshipContext)) context.text += "\n\n" + relationshipContext;
        LayoutAndShow();
    }
    /// <summary>Adds model-derived insertion relationships to the inspected card without owning their evaluation.</summary>
    public void SetRelationshipContext(TackleCardTooltipSource owner, string text)
    {
        if (source != owner || relationshipContext == text) return;
        relationshipContext = text;
        if (IsVisible) Show();
    }
    /// <summary>Fits either creature rules or a resolved contribution inside the shared native frame.</summary>
    private void LayoutAndShow()
    {
        float y = 24;
        y = Place(title, y, 12); y = Place(rarity, y, 14); y = Place(metadata, y, 24);
        y = Place(abilityName, y, 12); y = Place(rules, y, string.IsNullOrEmpty(context.text) ? 0 : 20);
        if (!string.IsNullOrEmpty(context.text)) y = Place(context, y, 0);
        context.gameObject.SetActive(!string.IsNullOrEmpty(context.text));
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y + 24);
        panel.gameObject.SetActive(true); outline.gameObject.SetActive(detailSource == null && string.IsNullOrEmpty(relationshipContext));
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
        (detailSource != null ? detailSource.Anchor : source.FaceBounds).GetWorldCorners(corners);
        Vector2 bottom = overlay.InverseTransformPoint(corners[0]), top = overlay.InverseTransformPoint(corners[2]);
        float width = panel.rect.width, height = panel.rect.height;
        bool above = detailSource != null || source.Above;
        float x = above ? (bottom.x + top.x - width) * .5f : top.x + 20;
        float y = above ? top.y + height + 20 : top.y;
        if (!above && x + width > safe.xMax) x = bottom.x - width - 20;
        if (above && y > safe.yMax) y = bottom.y - 20;
        // Cabinet details stay above the insertion controls in the machine's lower recess.
        if (!above && buildPhase) y = Mathf.Max(y, -50 + height);
        if (detailSource != null) y = Mathf.Max(y, height + 8); // Keep source explanations above Landing's timing track and Build's insertion controls.
        panel.anchoredPosition = new Vector2(Mathf.Clamp(x, safe.xMin, Mathf.Max(safe.xMin, safe.xMax - width)), Mathf.Clamp(y, safe.yMin + height, Mathf.Max(safe.yMin + height, safe.yMax)));
        outline.anchoredPosition = new Vector2(bottom.x - 4, top.y + 4);
        outline.sizeDelta = new Vector2(top.x - bottom.x + 8, top.y - bottom.y + 8);
    }
}
