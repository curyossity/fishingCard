using UnityEngine;

/// <summary>Connects the existing settings event to a read-only display panel without introducing gameplay preferences.</summary>
public sealed class JournalSettingsModal : MonoBehaviour
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private JournalModalHost host;
    [SerializeField] private FishingRunView view;
    private GameObject root;
    private TMPro.TMP_Text configuration;
    private UnityEngine.UI.Button close;
    public bool IsVisible => root != null && root.activeInHierarchy;
    public UnityEngine.UI.Button CloseButton => close;
    /// <summary>Subscribes once to the controller-independent settings entry point.</summary>
    private void OnEnable() { if (view != null) view.SettingsRequested += Show; }
    /// <summary>Releases the event and any owned input lock when the UI is unloaded.</summary>
    private void OnDisable() { if (view != null) view.SettingsRequested -= Show; if (root != null) host.Hide(root); }
    /// <summary>Displays actual current presentation configuration while preserving the existing lack of adjustable preferences.</summary>
    private void Show()
    {
        if (host.IsOpen) return;
        if (root == null)
        {
            var panel = JournalModalLayout.Panel(transform, theme, "Settings", out root);
            JournalModalLayout.Text("Title", panel, new Rect(.06f, .78f, .88f, .12f), theme, 60, true).text = "SETTINGS";
            configuration = JournalModalLayout.Text("Display", panel, new Rect(.06f, .35f, .88f, .35f), theme, 44);
            close = JournalModalLayout.Button("Return to Game", panel, new Rect(.06f, .10f, .88f, .12f), theme, out TMPro.TMP_Text label); label.text = "RETURN TO GAME";
            close.onClick.AddListener(() => host.Hide(root));
        }
        configuration.text = "DISPLAY\n" + Screen.width + " x " + Screen.height + "\n" + (Screen.fullScreen ? "Fullscreen" : "Windowed");
        host.Show(root); JournalModalLayout.Navigation(new[] { close }); close.Select();
    }
}
