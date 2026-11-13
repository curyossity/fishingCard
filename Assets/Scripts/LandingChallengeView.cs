using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Owns Landing input and phase visibility; the Tackle presentation renders authoritative state without scoring.</summary>
public sealed class LandingChallengeView : MonoBehaviour
{
    [SerializeField] private TackleLandingPresentation artwork;
    [SerializeField] private UnityEngine.UI.Button strike, returnToHarbor;
    [SerializeField] private UnityEngine.Events.UnityEvent returnedToHarbor = new UnityEngine.Events.UnityEvent();
    [SerializeField] private InputAction strikeInput = new InputAction("Landing Strike", InputActionType.Button, "<Keyboard>/space");
    private FishingRunController controller;
    private bool shownFailure;
    private long shownOccurrence = -1;
    public UnityEngine.Events.UnityEvent ReturnedToHarbor => returnedToHarbor;

    /// <summary>Binds both native controls to the same guarded controller command.</summary>
    private void Awake() { strike.onClick.AddListener(Strike); returnToHarbor.onClick.AddListener(Return); strikeInput.performed += OnStrikeInput; }
    /// <summary>Enables only this phase's keyboard action.</summary>
    private void OnEnable() { strikeInput.Enable(); }
    /// <summary>Releases input and presentation identity on phase exit.</summary>
    private void OnDisable() { strikeInput.Disable(); shownOccurrence = -1; shownFailure = false; }
    /// <summary>Releases the action owned by this view.</summary>
    private void OnDestroy() { strikeInput.performed -= OnStrikeInput; strikeInput.Dispose(); }

    /// <summary>Shows Landing, its successful Build handoff or its ended-expedition result, independently of Discover.</summary>
    public void Present(FishingRunController owner)
    {
        controller = owner;
        bool active = owner != null && owner.UsesLandingChallenge
            && (owner.RunActive && (owner.Landing.State == LandingChallengeState.Active || owner.AwaitingRigPlacement) || owner.LandingFailureVisible);
        gameObject.SetActive(active);
        if (active) Render();
    }
    /// <summary>Renders after the controller advances the authoritative motion clock.</summary>
    private void LateUpdate() { if (controller != null) Render(); }
    /// <summary>Updates command availability and artwork without altering attempts, points or rewards.</summary>
    private void Render()
    {
        bool failure = controller.LandingFailureVisible;
        bool fresh = shownOccurrence != controller.Landing.Occurrence;
        strike.gameObject.SetActive(!failure && !controller.AwaitingRigPlacement);
        strike.interactable = controller.RunActive && controller.Landing.CanStrike;
        returnToHarbor.gameObject.SetActive(failure);
        artwork.Present(controller);
        if ((fresh || failure && !shownFailure) && EventSystem.current != null) (failure ? returnToHarbor : strike).Select();
        shownOccurrence = controller.Landing.Occurrence; shownFailure = failure;
    }
    /// <summary>Leaves unrelated menus and text inputs in control of their keyboard focus.</summary>
    private void OnStrikeInput(InputAction.CallbackContext context)
    {
        var focused = EventSystem.current?.currentSelectedGameObject;
        if (focused != null && !focused.transform.IsChildOf(transform)) return;
        Strike();
    }
    /// <summary>Forwards keyboard/button input with occurrence and revision checks shared by both paths.</summary>
    private void Strike() { if (controller != null) controller.TryStrikeLanding(controller.Landing.Occurrence, controller.Landing.Revision); }
    /// <summary>Dismisses only an ended expedition before using the authored Harbor route.</summary>
    private void Return() { if (controller == null || !controller.LandingFailureVisible) return; controller.DismissLandingResult(); returnedToHarbor.Invoke(); }
}
