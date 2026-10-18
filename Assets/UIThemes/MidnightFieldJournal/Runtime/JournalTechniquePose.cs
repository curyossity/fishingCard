using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Owns the complete card plane's resting/inspect pose and a stable resting hit region.</summary>
public sealed class JournalTechniquePose : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private RectTransform rect, restHit;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private bool hovered, focused;
    private int order;
    private Action<JournalTechniquePose, bool> feedback;
    public bool Inspecting => hovered || focused;
    public RectTransform RestHit => restHit;

    /// <summary>Updates the resting position while preserving focus when an unchanged hand refreshes.</summary>
    public void Configure(RectTransform hitPlane, Vector3 position, Quaternion rotation, int siblingOrder,
        Action<JournalTechniquePose, bool> onFeedback)
    {
        rect = GetComponent<RectTransform>(); restHit = hitPlane;
        restPosition = position; restRotation = rotation; order = siblingOrder; feedback = onFeedback;
        ApplyPose();
    }

    /// <summary>Raises and straightens a hovered card so its live rules remain readable.</summary>
    public void OnPointerEnter(PointerEventData data) { hovered = true; ApplyPose(); }
    /// <summary>Restores the rest pose unless keyboard/controller focus still owns inspection.</summary>
    public void OnPointerExit(PointerEventData data) { hovered = false; ApplyPose(); }
    /// <summary>Offers the same inspect pose to navigation input.</summary>
    public void OnSelect(BaseEventData data) { focused = true; ApplyPose(); }
    /// <summary>Clears navigation inspection independently of pointer hover.</summary>
    public void OnDeselect(BaseEventData data) { focused = false; ApplyPose(); }
    /// <summary>Clears stale focus when the physical hand contents change.</summary>
    public void ResetInspection() { hovered = false; focused = false; ApplyPose(); }
    /// <summary>Restores a hidden slot so it cannot retain an elevated pose on reuse.</summary>
    private void OnDisable() { ResetInspection(); }

    /// <summary>Transforms the entire card including its illustration, text and input surface.</summary>
    private void ApplyPose()
    {
        if (rect == null) return;
        rect.localPosition = restPosition + (Inspecting ? new Vector3(0, 65, -100) : Vector3.zero);
        rect.localRotation = Inspecting ? Quaternion.Euler(10, 0, 0) : restRotation;
        rect.localScale = Vector3.one;
        if (Inspecting) rect.SetAsLastSibling(); else rect.SetSiblingIndex(order);
        feedback?.Invoke(this, Inspecting);
    }
}
