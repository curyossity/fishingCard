using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Places projected hand hits in the HUD's normal EventSystem and modal sorting order.</summary>
public sealed class JournalHandRaycaster : BaseRaycaster
{
    [SerializeField] private JournalTechniqueHandPresentation hand;
    private readonly List<RaycastResult> mappedHits = new List<RaycastResult>();
    private Canvas Hud => hand == null || hand.Surface == null ? null : hand.Surface.canvas;
    public override Camera eventCamera => Hud == null || Hud.renderMode == RenderMode.ScreenSpaceOverlay ? null : Hud.worldCamera;
    public override int sortOrderPriority => Hud == null ? 0 : Hud.sortingOrder;
    public override int renderOrderPriority => Hud == null ? 0 : Hud.rootCanvas.renderOrder;

    /// <summary>Transforms a visible HUD point to texture pixels, preserving standard modal priority and click dispatch.</summary>
    public override void Raycast(PointerEventData eventData, List<RaycastResult> results)
    {
        if (hand == null || hand.Target == null || hand.Surface == null || !hand.Surface.isActiveAndEnabled) return;
        RectTransform rect = hand.Surface.rectTransform;
        if (!RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position, eventCamera) || !hand.Surface.Raycast(eventData.position, eventCamera)) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventCamera, out Vector2 local)) return;
        Vector2 uv = new Vector2((local.x - rect.rect.xMin) / rect.rect.width, (local.y - rect.rect.yMin) / rect.rect.height);
        mappedHits.Clear(); hand.RaycastProjected(eventData, Vector2.Scale(uv, new Vector2(hand.Target.width, hand.Target.height)), mappedHits);
        foreach (RaycastResult hit in mappedHits)
        {
            RaycastResult result = hit; result.module = this; result.screenPosition = eventData.position;
            result.distance = 0; result.depth = hand.Surface.depth; result.sortingLayer = Hud.sortingLayerID; result.sortingOrder = Hud.sortingOrder;
            result.index = results.Count; results.Add(result);
        }
    }
}
