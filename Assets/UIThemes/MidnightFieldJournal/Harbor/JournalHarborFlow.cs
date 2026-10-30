using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Routes the journal hub's presentation pages and delegates expedition lifecycle to the existing controller.</summary>
public sealed class JournalHarborFlow : MonoBehaviour
{
    [SerializeField] private GameObject hubContent;
    [SerializeField] private GameObject[] pages = Array.Empty<GameObject>();
    [SerializeField] private TMP_Text[] goldLabels = Array.Empty<TMP_Text>();
    [SerializeField] private FishingRunController controller;
    [SerializeField] private GameObject runCanvas;
    [SerializeField] private JournalResultModal resultModal;

    public bool HubVisible => hubContent != null && hubContent.activeSelf;
    public int CurrentPage { get; private set; }
    public FishingRunController Controller => controller;

    /// <summary>Opens the Harbor before a run and opts the result modal into the hub return route.</summary>
    private void Awake()
    {
        if (controller == null || runCanvas == null || hubContent == null || pages.Length != 4)
            throw new InvalidOperationException("Journal Harbor requires its controller, run canvas and four authored pages.");
        if (resultModal != null) resultModal.BindHarborReturn(ReturnToHarbor);
        ReturnToHarbor();
    }

    /// <summary>Releases only this flow's optional result callback when its scene is unloaded.</summary>
    private void OnDestroy()
    {
        if (resultModal != null) resultModal.BindHarborReturn(null);
    }

    /// <summary>Moves between authored hub pages without purchasing equipment or changing the saved deck.</summary>
    public void Navigate(int page)
    {
        if (!HubVisible || page < 0 || page >= pages.Length) return;
        CurrentPage = page;
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == page);
        foreach (TMP_Text label in goldLabels) if (label != null) label.text = controller.TotalGold.ToString();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            foreach (UnityEngine.UI.Button button in pages[page].GetComponentsInChildren<UnityEngine.UI.Button>())
                if (button.IsInteractable()) { button.Select(); break; }
        }
    }

    /// <summary>Returns from completed-run results; an active expedition cannot be silently abandoned.</summary>
    public void ReturnToHarbor()
    {
        if (controller == null || controller.RunActive) return;
        if (resultModal != null) resultModal.Hide();
        runCanvas.SetActive(false);
        hubContent.SetActive(true);
        Navigate(0);
    }

    /// <summary>Starts a fresh expedition from the Harbor's Set Sail button.</summary>
    public void StartNewRun() { TryStartNewRun(); }

    /// <summary>Starts exactly one run through the shared controller; duplicate clicks cannot reset an expedition.</summary>
    public bool TryStartNewRun()
    {
        if (!HubVisible || controller == null || controller.RunActive) return false;
        runCanvas.SetActive(true);
        controller.StartRun();
        if (!controller.RunActive) { runCanvas.SetActive(false); return false; }
        hubContent.SetActive(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        return true;
    }
}
