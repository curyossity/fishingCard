using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Harbor preparation draft; commits exactly four supported tags through the controller before Set Sail.</summary>
public sealed class SlotTagPreparationView : MonoBehaviour, UnityEngine.EventSystems.ICancelHandler
{
    [SerializeField] private UnityEngine.UI.Button[] tagButtons = Array.Empty<UnityEngine.UI.Button>();
    [SerializeField] private TMP_Text[] tagLabels = Array.Empty<TMP_Text>();
    [SerializeField] private TMP_Text status;
    [SerializeField] private UnityEngine.UI.Button confirm, cancel;
    [SerializeField] private CanvasGroup blockedHarborContent;
    [SerializeField] private TackleTagPreparationPresentation artwork;
    [SerializeField] private DeepwaterSoundPlayer sound;
    private bool previousInteractable, previousRaycasts;
    private bool inputBlocked;
    private readonly List<string> draft = new List<string>();
    private FishingRunController controller;
    private Action start;
    private GameObject returnFocus;
    public IReadOnlyList<string> Draft => draft.AsReadOnly();

    /// <summary>Copies the previous preparation for editing without changing any active expedition.</summary>
    public void Open(FishingRunController owner, Action begin)
    {
        if (owner == null || owner.RunActive || gameObject.activeInHierarchy && controller != null) return;
        controller = owner; start = begin;
        returnFocus = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
        draft.Clear(); draft.AddRange(CreatureTagVocabulary.Normalize(new List<string>(owner.PreparedSlotTags).ToArray()));
        gameObject.SetActive(true);
        if (blockedHarborContent != null && !inputBlocked)
        {
            previousInteractable = blockedHarborContent.interactable;
            previousRaycasts = blockedHarborContent.blocksRaycasts;
            blockedHarborContent.interactable = blockedHarborContent.blocksRaycasts = false;
            inputBlocked = true;
        }
        for (int i = 0; i < tagButtons.Length; i++)
        {
            tagButtons[i].gameObject.SetActive(i < CreatureTagVocabulary.All.Length);
            if (i >= CreatureTagVocabulary.All.Length) continue;
            string tag = CreatureTagVocabulary.All[i];
            tagButtons[i].onClick.RemoveAllListeners();
            tagButtons[i].onClick.AddListener(() => Toggle(tag));
        }
        confirm.onClick.RemoveAllListeners(); confirm.onClick.AddListener(Commit);
        cancel.onClick.RemoveAllListeners(); cancel.onClick.AddListener(Close);
        Refresh();
        if (UnityEngine.EventSystems.EventSystem.current != null)
            foreach (var button in tagButtons) if (button.IsInteractable()) { button.Select(); break; }
    }

    /// <summary>Restores Harbor interaction when preparation is cancelled, confirmed or unloaded.</summary>
    private void OnDisable()
    {
        if (inputBlocked && blockedHarborContent != null)
        {
            blockedHarborContent.interactable = previousInteractable;
            blockedHarborContent.blocksRaycasts = previousRaycasts;
            inputBlocked = false;
        }
        if (returnFocus != null && returnFocus.activeInHierarchy && UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(returnFocus);
        returnFocus = null; start = null; controller = null;
    }

    /// <summary>Discards only the local draft and returns to the Harbor without starting or saving an expedition.</summary>
    public void Close() { gameObject.SetActive(false); }
    /// <summary>Supports the input module's native cancel action from any focused preparation control.</summary>
    public void OnCancel(UnityEngine.EventSystems.BaseEventData data) { Close(); data.Use(); }

    /// <summary>Updates only a local four-tag preparation draft.</summary>
    private void Toggle(string tag)
    {
        if (draft.Contains(tag)) { draft.Remove(tag); if(sound!=null)sound.Play(DeepwaterSoundCue.TagDeselect); }
        else if (draft.Count < 4) { draft.Add(tag); if(sound!=null)sound.Play(DeepwaterSoundCue.TagSelect); }
        Refresh();
    }

    /// <summary>Commits preparation atomically before requesting the expedition start.</summary>
    private void Commit()
    {
        if (controller == null || controller.RunActive || !controller.TryPrepareSlotTags(draft.ToArray())) return;
        start?.Invoke();
        if (controller.RunActive) gameObject.SetActive(false);
        else if (artwork != null) artwork.ShowError(controller.SlotFailure ?? "Could not set sail. Please review the expedition setup.");
        else status.text = controller.SlotFailure ?? "Choose four tags.";
    }

    /// <summary>Shows explicit selection state and prevents a partial loadout from starting an expedition.</summary>
    private void Refresh()
    {
        status.text = draft.Count + " / 4 SELECTED";
        confirm.interactable = draft.Count == 4;
        for (int i = 0; i < tagButtons.Length; i++)
        {
            if (i >= CreatureTagVocabulary.All.Length) continue;
            string tag = CreatureTagVocabulary.All[i];
            bool selected = draft.Contains(tag);
            tagLabels[i].text = artwork != null ? tag.ToUpperInvariant() : (selected ? "[x] " : "[ ] ") + tag;
            tagButtons[i].interactable = selected || draft.Count < 4;
        }
        if (artwork != null) artwork.Present(draft, confirm.interactable);
    }
}
