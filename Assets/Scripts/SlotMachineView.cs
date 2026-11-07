using System;
using TMPro;
using UnityEngine;

/// <summary>Artwork-independent Discover controls; all mutations are forwarded with their exact state revision.</summary>
public sealed class SlotMachineView : MonoBehaviour
{
    [SerializeField] private GameObject discoverContent;
    [SerializeField] private TMP_Text heading;
    [SerializeField] private TMP_Text resultDescription;
    [SerializeField] private TMP_Text[] reelLabels = Array.Empty<TMP_Text>();
    [SerializeField] private UnityEngine.UI.Button[] lockButtons = Array.Empty<UnityEngine.UI.Button>();
    [SerializeField] private TMP_Text[] lockLabels = Array.Empty<TMP_Text>();
    [SerializeField] private UnityEngine.UI.Button acceptButton, respinButton;
    [SerializeField] private TackleDiscoverPresentation artwork;
    public bool IsAnimating => isActiveAndEnabled && artwork != null && artwork.IsSpinning;

    /// <summary>Displays only Discover or its landing handoff; never interprets an outcome as a caught creature.</summary>
    public void Present(FishingRunController controller)
    {
        bool active = controller != null && controller.UsesSlotMachine && controller.RunActive
            && (controller.SlotMachine.Active || artwork == null && controller.AwaitingLandingChallenge || controller.SlotFailure != null);
        gameObject.SetActive(active);
        if (!active) return;
        var machine = controller.SlotMachine;
        bool discovering = machine.Active;
        discoverContent.SetActive(discovering);
        heading.text = controller.SlotFailure ?? (discovering ? "DISCOVER — choose a result" : "LANDING CHALLENGE");
        if (!discovering) return;
        var match = new SlotMatch(new TagSlotMachineRuntime.Result(machine.Occurrence,
            new[] { machine.GetTag(0), machine.GetTag(1), machine.GetTag(2) }));
        heading.text = controller.SlotFailure ?? (match.Kind == SlotMatchKind.Triple ? "DISCOVER — TRIPLE"
            : match.Kind == SlotMatchKind.Pair ? "DISCOVER — PAIR" : "DISCOVER — THREE DIFFERENT");
        resultDescription.text = match.Kind == SlotMatchKind.ThreeDifferent ? "Any eligible biome creature · baseline rarity"
            : match.GuaranteedTag + " guaranteed · " + (match.Kind == SlotMatchKind.Pair
                ? match.PreferredTag + " preferred · modest rarity bonus" : "stronger rarity bonus");
        if (artwork != null)
        {
            heading.text = "ENCOUNTER TAGS";
            resultDescription.text = controller.SlotFailure ?? (match.Kind == SlotMatchKind.ThreeDifferent ? "OPEN WATERS / ANY TAG"
                : match.GuaranteedTag.ToUpperInvariant() + (match.Kind == SlotMatchKind.Triple ? " TRIPLE" : " MATCH / " + match.PreferredTag.ToUpperInvariant() + " PREFERRED"));
            if (controller.SlotFailure == null && machine.RespinsRemaining == 0) resultDescription.text = "FINAL / " + resultDescription.text;
            artwork.Present(controller, match, () => Present(controller));
            if (IsAnimating) resultDescription.text = "SPINNING...";
        }
        long revision = machine.Revision;
        bool anyLocked = machine.IsLocked(0) || machine.IsLocked(1) || machine.IsLocked(2);
        for (int i = 0; i < TagSlotMachineRuntime.ReelCount; i++)
        {
            int reel = i;
            bool locked = machine.IsLocked(i);
            reelLabels[i].text = artwork != null ? machine.GetTag(i).ToUpperInvariant() : machine.GetTag(i);
            lockLabels[i].text = locked ? (artwork != null ? "LOCKED" : "UNLOCK") : "LOCK";
            lockButtons[i].interactable = !IsAnimating && machine.RespinsRemaining > 0 && (locked || !anyLocked);
            lockButtons[i].onClick.RemoveAllListeners();
            lockButtons[i].onClick.AddListener(() => controller.TrySetSlotReelLocked(revision, reel, !locked));
            lockButtons[i].GetComponent<TackleButtonFeedback>()?.RefreshAvailability();
        }
        acceptButton.interactable = !IsAnimating && machine.CanAccept;
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => controller.TryAcceptSlotMachine(revision));
        respinButton.interactable = !IsAnimating && machine.CanSpin && machine.HasSpun;
        respinButton.onClick.RemoveAllListeners();
        respinButton.onClick.AddListener(() => controller.TryRespinSlotMachine(revision));
        acceptButton.GetComponent<TackleButtonFeedback>()?.RefreshAvailability();
        respinButton.GetComponent<TackleButtonFeedback>()?.RefreshAvailability();
        if (!respinButton.interactable && acceptButton.interactable
            && UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject == respinButton.gameObject) acceptButton.Select();
    }
}
