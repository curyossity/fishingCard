using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Plain Landing UI: displays authoritative motion/scoring and forwards one shared strike command for keyboard and button.</summary>
public sealed class LandingChallengeView : MonoBehaviour
{
    [SerializeField] private TMP_Text creature, progress, attempts, behavior, feedback, breakdown, rig;
    [SerializeField] private RectTransform track, fish, goodZone, perfectZone, progressFill;
    [SerializeField] private UnityEngine.UI.Button strike, returnToHarbor;
    [SerializeField] private UnityEngine.Events.UnityEvent returnedToHarbor = new UnityEngine.Events.UnityEvent();
    [SerializeField] private InputAction strikeInput = new InputAction("Landing Strike", InputActionType.Button, "<Keyboard>/space");
    private FishingRunController controller;
    private UnityEngine.UI.Image fishImage;
    private long shownOccurrence = -1, shownRevision = -1;
    private bool shownFailure;
    private double highlightUntil;
    private bool showingHighlight;
    public UnityEngine.Events.UnityEvent ReturnedToHarbor => returnedToHarbor;

    /// <summary>Binds the authored native controls once without constructing artwork at runtime.</summary>
    private void Awake()
    {
        fishImage = fish.GetComponent<UnityEngine.UI.Image>();
        strike.onClick.AddListener(Strike);
        returnToHarbor.onClick.AddListener(Return);
        strikeInput.performed += OnStrikeInput;
    }

    /// <summary>Enables only this phase's keyboard action.</summary>
    private void OnEnable() { strikeInput.Enable(); }
    /// <summary>Releases the keyboard when Discover, results dismissal or another phase hides this view.</summary>
    private void OnDisable() { strikeInput.Disable(); shownRevision = -1; shownFailure = false; }
    /// <summary>Releases the action and callbacks owned by this view.</summary>
    private void OnDestroy() { strikeInput.performed -= OnStrikeInput; strikeInput.Dispose(); }

    /// <summary>Opens only Landing, its success handoff, or its plain failure result; the slot machine is a separate screen.</summary>
    public void Present(FishingRunController owner)
    {
        controller = owner;
        bool active = owner != null && owner.UsesLandingChallenge
            && (owner.RunActive && (owner.Landing.State == LandingChallengeState.Active || owner.AwaitingRigPlacement) || owner.LandingFailureVisible);
        gameObject.SetActive(active);
        if (!active) return;
        Render();
    }

    /// <summary>Renders current positions after the controller samples its clock; this method never scores or consumes attempts.</summary>
    private void LateUpdate() { if (controller != null) Render(); }

    /// <summary>Updates moving rectangles every frame and rebuilds score/rig text only when the occurrence or strike revision changes.</summary>
    private void Render()
    {
        var model = controller.Landing;
        bool failure = controller.LandingFailureVisible;
        bool fresh = shownOccurrence != model.Occurrence;
        SetTrackRect(fish, model.FishPosition, model.FishWidth);
        SetTrackRect(goodZone, model.ZonePosition, model.GoodWidth);
        SetTrackRect(perfectZone, model.ZonePosition, model.PerfectWidth);
        fishImage.color = model.IsTelegraphingBurst ? new Color(1, .6f, .15f) : Color.white;
        progressFill.anchorMax = new Vector2(model.Resistance > 0 ? Mathf.Clamp01((float)(model.Points / model.Resistance)) : 0, 1);
        behavior.text = model.IsTelegraphingBurst ? "BURST SOON" : "Movement: " + model.Pattern;
        strike.gameObject.SetActive(!failure && !controller.AwaitingRigPlacement);
        strike.interactable = controller.RunActive && model.CanStrike;
        returnToHarbor.gameObject.SetActive(failure);
        if (failure && !shownFailure) returnToHarbor.Select();
        shownFailure = failure;
        if (shownRevision != model.Revision || fresh)
        {
            shownOccurrence = model.Occurrence; shownRevision = model.Revision;
            creature.text = failure ? "EXPEDITION ENDED" : controller.CurrentEncounter?.DisplayName ?? "Landing";
            progress.text = "LANDING POINTS  " + Number(model.Points) + " / " + Number(model.Resistance);
            attempts.text = "ATTEMPTS REMAINING  " + model.AttemptsRemaining;
            feedback.text = failure ? "LANDING FAILED — Full Catch value retained"
                : controller.AwaitingRigPlacement ? "LANDED — Ready for rig placement"
                : model.LastScore?.Accuracy.ToString().ToUpperInvariant() ?? "Time your strike when the fish overlaps the moving zone";
            breakdown.text = Breakdown(model.LastScore);
            if (failure) breakdown.text += "\n\nFull accumulated Catch value: " + controller.LastHaulValue
                + "\nBait sale value: 0. The failed creature was not acquired.";
            if (controller.AwaitingRigPlacement) breakdown.text += "\n\nThe creature is landed and ready for rig placement.";
            highlightUntil = Time.realtimeSinceStartupAsDouble + .35; showingHighlight = true;
            RenderRig(true);
            if (fresh && EventSystem.current != null) (failure ? returnToHarbor : strike).Select();
        }
        if (showingHighlight && Time.realtimeSinceStartupAsDouble >= highlightUntil) { showingHighlight = false; RenderRig(false); }
        if (model.Error != null) feedback.text = model.Error;
    }

    /// <summary>Briefly highlights only the committed instances attributed by the resolved score.</summary>
    private void RenderRig(bool highlight)
    {
        if (controller.LandingFailureVisible)
        {
            rig.text = "RIG CLEARED\n\nAll expedition creatures were removed.\n\nPermanent progression is preserved.";
            return;
        }
        var model = controller.Landing;
        var list = new StringBuilder("CURRENT RIG\n\n");
        foreach (var member in model.Rig)
        {
            bool contributed = false;
            if (highlight && model.LastScore != null) foreach (var source in model.LastScore.Contributions) contributed |= source.SourceId == member.Id;
            list.Append(contributed ? "<color=#FFD26A>" : "<color=#D8E0E4>").Append(member.Name).Append(" [")
                .Append(member.Role.ToString().ToUpperInvariant()).Append("]</color>\n\n");
        }
        if (model.Rig.Count == 0) list.Append("No creatures attached.");
        rig.text = list.ToString();
    }

    /// <summary>Places a normalized center/width within the authored responsive track.</summary>
    private void SetTrackRect(RectTransform rectangle, double position, double width)
    {
        rectangle.sizeDelta = new Vector2((float)width * track.rect.width, rectangle.sizeDelta.y);
        rectangle.anchoredPosition = new Vector2(((float)position - .5f) * track.rect.width, 0);
    }

    /// <summary>Accepts a key-down only while focus belongs to this phase, so text fields and covering menus retain their input.</summary>
    private void OnStrikeInput(InputAction.CallbackContext context)
    {
        var focused = EventSystem.current?.currentSelectedGameObject;
        if (focused != null && !focused.transform.IsChildOf(transform)) return;
        Strike();
    }

    /// <summary>Uses the same occurrence/revision-checked command as Space; controller/model guards reject duplicate submissions.</summary>
    private void Strike()
    {
        if (controller != null) controller.TryStrikeLanding(controller.Landing.Occurrence, controller.Landing.Revision);
    }

    /// <summary>Dismisses only an ended expedition, then invokes the authored Harbor route.</summary>
    private void Return() { if (controller == null || !controller.LandingFailureVisible) return; controller.DismissLandingResult(); returnedToHarbor.Invoke(); }

    /// <summary>Displays the resolved sequence without rerunning abilities or delaying its outcome.</summary>
    private static string Breakdown(LandingScore score)
    {
        if (score == null) return "Good and Perfect regions move together. The fish moves independently.\nSpace or STRIKE uses one attempt.";
        var text = new StringBuilder(score.Accuracy.ToString().ToUpperInvariant()).Append(" — Accuracy base: ").Append(Number(score.Base));
        foreach (var source in score.Contributions)
        {
            text.Append('\n').Append(source.Label);
            if (source.Bonus != 0) text.Append("  +").Append(Number(source.Bonus));
            if (source.Multiplier != 1) text.Append("  ×").Append(Number(source.Multiplier));
        }
        return text.Append("\n\nTOTAL: +").Append(Number(score.Points)).Append(" Landing Points").ToString();
    }

    /// <summary>Shows fractional scores without introducing scoring-rounding rules.</summary>
    private static string Number(decimal value) { return value.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture); }
}
