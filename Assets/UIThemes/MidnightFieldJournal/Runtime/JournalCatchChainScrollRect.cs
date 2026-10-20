using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Follows the newest attachment until the player scrolls away, then resumes when they return to the bottom.</summary>
public sealed class JournalCatchChainScrollRect : UnityEngine.UI.ScrollRect
{
    private bool followNewest = true;
    private bool userDragging;
    private bool userMotion;
    [SerializeField, Min(.01f)] private float followDuration = .32f;
    private bool layoutKnown;
    private int previousCount;
    private float previousEnd;
    private bool animating;
    private float animationStart;
    private float animationEnd;
    private float animationElapsed;
    private bool feedbackFocus, feedbackResting;
    public int UserGestureRevision { get; private set; }
    public bool IsFeedbackScrolling => feedbackFocus && animating;
    public bool HasFeedbackFocus => feedbackFocus;
    public bool FollowingNewest => followNewest && !userDragging;

    /// <summary>Restores the appropriate offset after content growth, shrinkage or viewport resizing.</summary>
    public void RestoreAfterLayout(int attachmentCount)
    {
        if (content == null || viewport == null) return;
        float end = Mathf.Max(0, content.rect.height - viewport.rect.height);
        bool appended = layoutKnown && attachmentCount > previousCount && end > previousEnd + .5f;
        bool extentChanged = !layoutKnown || Mathf.Abs(end - previousEnd) > .5f || attachmentCount < previousCount;
        layoutKnown = true; previousCount = attachmentCount; previousEnd = end;
        if (appended) feedbackResting = false;
        if (feedbackFocus || feedbackResting && !extentChanged) return;
        if (end <= .5f) { followNewest = true; userMotion = false; animating = false; }
        Vector2 position = content.anchoredPosition;
        position.y = Mathf.Clamp(position.y, 0, end);
        if (FollowingNewest)
        {
            StopMovement();
            if (appended && Application.isPlaying && isActiveAndEnabled)
            {
                animationStart = position.y; animationEnd = end; animationElapsed = 0;
                animating = true; userMotion = false;
            }
            else if (extentChanged || !animating) { animating = false; position.y = end; }
        }
        else animating = false;
        content.anchoredPosition = position;
    }

    /// <summary>Distinguishes real wheel intent from programmatic layout and scroll notifications.</summary>
    public override void OnScroll(PointerEventData eventData)
    {
        base.OnScroll(eventData);
        if (!IsActive() || eventData.scrollDelta.sqrMagnitude == 0) return;
        UserGestureRevision++; feedbackFocus = feedbackResting = false;
        animating = false;
        userMotion = true; followNewest = AtBottom();
    }

    /// <summary>Stops following as soon as a real left-button/touch drag begins.</summary>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (!IsActive() || eventData.button != PointerEventData.InputButton.Left) return;
        UserGestureRevision++; feedbackFocus = feedbackResting = false;
        animating = false;
        userDragging = true; userMotion = true; followNewest = false;
    }

    /// <summary>Re-enables following if the player finishes their drag at the newest attachment.</summary>
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (eventData.button != PointerEventData.InputButton.Left) return;
        userDragging = false; followNewest = AtBottom();
    }

    /// <summary>Includes inertial motion when deciding whether a user gesture has returned to the bottom.</summary>
    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (animating && content != null)
        {
            StopMovement();
            animationElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(animationElapsed / Mathf.Max(.01f, followDuration));
            Vector2 position = content.anchoredPosition;
            position.y = Mathf.Lerp(animationStart, animationEnd, t * t * (3 - 2 * t));
            content.anchoredPosition = position;
            if (t >= 1) animating = false;
        }
        if (!userMotion || userDragging) return;
        followNewest = AtBottom();
        if (velocity.sqrMagnitude < .01f) userMotion = false;
    }

    /// <summary>Clears a gesture interrupted by hiding the HUD without losing the player's retained position.</summary>
    protected override void OnDisable() { feedbackFocus = feedbackResting = false; animating = false; layoutKnown = false; userDragging = false; userMotion = false; base.OnDisable(); }

    /// <summary>Scrolls a stat-feedback recipient fully into view before its count begins.</summary>
    public void FocusFeedbackCard(RectTransform card, float padding = 0)
    {
        if (content == null || viewport == null || card == null) return;
        feedbackFocus = true; feedbackResting = false; userMotion = false; StopMovement();
        float top = -card.anchoredPosition.y - Mathf.Max(0, padding);
        float bottom = -card.anchoredPosition.y + card.rect.height + Mathf.Max(0, padding);
        float current = content.anchoredPosition.y;
        float destination = top < current ? top : bottom > current + viewport.rect.height ? bottom - viewport.rect.height : current;
        destination = Mathf.Clamp(destination, 0, Mathf.Max(0, content.rect.height - viewport.rect.height));
        animationStart = current; animationEnd = destination; animationElapsed = 0;
        animating = Mathf.Abs(destination - current) > .5f;
    }

    /// <summary>Retains the last inspected recipient until the next extent change or new attachment.</summary>
    public void EndFeedbackFocus()
    {
        feedbackFocus = false; feedbackResting = true; animating = false; StopMovement();
    }

    /// <summary>Compares offsets in content units rather than using a count-dependent normalized tolerance.</summary>
    private bool AtBottom()
    {
        if (content == null || viewport == null) return true;
        return content.anchoredPosition.y >= Mathf.Max(0, content.rect.height - viewport.rect.height) - .5f;
    }
}
