using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Renders the preparation draft using the expedition's Tackle artwork; the controller owns commitment.</summary>
public sealed class TackleTagPreparationPresentation : MonoBehaviour
{
    [Serializable] private sealed class TagPlate
    {
        public string tag;
        public UnityEngine.UI.Image face, symbol, lamp;
        public TMP_Text state;
        public TackleButtonFeedback feedback;
    }
    [SerializeField] private TagPlate[] plates = Array.Empty<TagPlate>();
    [SerializeField] private Sprite selectedFace, unselectedFace, lampOn, lampOff;
    [SerializeField] private TMP_Text instruction;
    [SerializeField] private TMP_Text failure;
    [SerializeField] private GameObject[] guidance = Array.Empty<GameObject>();
    [SerializeField] private UnityEngine.UI.Image[] readyLamps = Array.Empty<UnityEngine.UI.Image>();
    [SerializeField] private TackleButtonFeedback confirmFeedback;

    /// <summary>Distinguishes selected, available and excluded tags using both text and light, without granting tag abilities.</summary>
    public void Present(IReadOnlyList<string> draft, bool ready)
    {
        failure.gameObject.SetActive(false); foreach (var item in guidance) item.SetActive(true);
        foreach (var plate in plates)
        {
            bool selected = false; foreach (string tag in draft) selected |= tag == plate.tag;
            plate.face.sprite = selected ? selectedFace : unselectedFace;
            plate.symbol.color = selected ? Color.white : new Color32(180, 186, 171, 255);
            plate.state.text = selected ? "IN YOUR REELS" : "NOT SELECTED";
            plate.lamp.sprite = selected ? lampOn : lampOff;
            // Keep the excluded philosophy readable even when a full selection prevents adding a fifth symbol.
            plate.feedback.RefreshAvailability();
        }
        for (int i = 0; i < readyLamps.Length; i++) readyLamps[i].sprite = i < draft.Count ? lampOn : lampOff;
        instruction.text = ready ? "Remove a tag to choose a different one." : "Choose " + (4 - draft.Count) + " more tag" + (draft.Count == 3 ? "." : "s.");
        confirmFeedback.RefreshAvailability();
    }

    /// <summary>Shows a complete start failure in the guidance area while preserving the four-tag counter and editable draft.</summary>
    public void ShowError(string message)
    {
        foreach (var item in guidance) item.SetActive(false);
        failure.text = message; failure.gameObject.SetActive(true);
    }
}
