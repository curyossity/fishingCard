using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Harbor preparation draft; commits exactly four supported tags through the controller before Set Sail.</summary>
public sealed class SlotTagPreparationView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] tagButtons = Array.Empty<UnityEngine.UI.Button>();
    [SerializeField] private TMP_Text[] tagLabels = Array.Empty<TMP_Text>();
    [SerializeField] private TMP_Text status;
    [SerializeField] private UnityEngine.UI.Button confirm, cancel;
    [SerializeField] private CanvasGroup blockedHarborContent;
    private bool previousInteractable, previousRaycasts;
    private bool inputBlocked;
    private readonly List<string> draft = new List<string>();
    private FishingRunController controller;
    private Action start;

    /// <summary>Copies the previous preparation for editing without changing any active expedition.</summary>
    public void Open(FishingRunController owner, Action begin)
    {
        if (owner == null || owner.RunActive) return;
        controller = owner; start = begin;
        draft.Clear(); draft.AddRange(owner.PreparedSlotTags);
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
            string tag = CreatureTagVocabulary.All[i];
            tagButtons[i].onClick.RemoveAllListeners();
            tagButtons[i].onClick.AddListener(() => Toggle(tag));
        }
        confirm.onClick.RemoveAllListeners(); confirm.onClick.AddListener(Commit);
        cancel.onClick.RemoveAllListeners(); cancel.onClick.AddListener(() => gameObject.SetActive(false));
        Refresh();
        if (UnityEngine.EventSystems.EventSystem.current != null) tagButtons[0].Select();
    }

    /// <summary>Restores Harbor interaction when preparation is cancelled, confirmed or unloaded.</summary>
    private void OnDisable()
    {
        if (!inputBlocked || blockedHarborContent == null) return;
        blockedHarborContent.interactable = previousInteractable;
        blockedHarborContent.blocksRaycasts = previousRaycasts;
        inputBlocked = false;
    }

    /// <summary>Updates only a local four-tag preparation draft.</summary>
    private void Toggle(string tag)
    {
        if (draft.Contains(tag)) draft.Remove(tag);
        else if (draft.Count < 4) draft.Add(tag);
        Refresh();
    }

    /// <summary>Commits preparation atomically before requesting the expedition start.</summary>
    private void Commit()
    {
        if (!controller.TryPrepareSlotTags(draft.ToArray())) return;
        start?.Invoke();
        if (controller.RunActive) gameObject.SetActive(false);
        else { status.text = controller.SlotFailure ?? "Choose four tags."; }
    }

    /// <summary>Shows explicit selection state and prevents a partial loadout from starting an expedition.</summary>
    private void Refresh()
    {
        status.text = "Choose 4 active tags — " + draft.Count + "/4";
        confirm.interactable = draft.Count == 4;
        for (int i = 0; i < tagButtons.Length; i++)
        {
            string tag = CreatureTagVocabulary.All[i];
            bool selected = draft.Contains(tag);
            tagLabels[i].text = (selected ? "[x] " : "[ ] ") + tag;
            tagButtons[i].interactable = selected || draft.Count < 4;
        }
    }
}
