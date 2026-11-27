using TMPro;
using UnityEngine;

/// <summary>
/// The player's side of the Landing machine. At rest the Strike gauge shows a hook, READY and the accuracy bases; while a strike's
/// score plays, the score readout takes the display (grade in the tab, running damage in the window) and each contributing rig
/// creature appears beside the number with its name in the label slot. The encounter creature lives on the opposite card, so the
/// two sides never swap identities. Presentation only: values come from the frozen settings and the score presentation.
/// </summary>
public sealed class LandingStrikePanel : MonoBehaviour
{
    [SerializeField] private GameObject idle;
    [SerializeField] private TMP_Text legend, label, ready;
    [SerializeField] private UnityEngine.UI.Image hook;
    [SerializeField] private CanvasGroup readout;
    [SerializeField] private RectTransform value, operation;
    [SerializeField] private LandingScoringPortrait contributors;
    /// <summary>Damage number anchors: centered alone, or shifted right of a contributor portrait.</summary>
    [SerializeField] private Vector2 valueAlone = new Vector2(0, -6), valueBeside = new Vector2(92, -12), operationBeside = new Vector2(92, 26);
    public const string IdleLabel = "SPACE TO STRIKE", StrikeLabel = "YOUR STRIKE";

    private void OnEnable() { if (contributors != null) contributors.Changed += Layout; }
    private void OnDisable() { if (contributors != null) contributors.Changed -= Layout; }

    /// <summary>Places the damage number beside a contributor portrait or centered alone; runs on every contributor swap.</summary>
    private void Layout()
    {
        bool beside = contributors != null && contributors.IsShowingContributors;
        if (value != null) value.anchoredPosition = beside ? valueBeside : valueAlone;
        if (operation != null) { operation.gameObject.SetActive(beside); operation.anchoredPosition = operationBeside; }
    }

    /// <summary>Names the strike before the score presentation snapshots the panel, so restoring it never reverts to the idle prompt.</summary>
    public void BeginStrike() { if (label != null) label.text = StrikeLabel; }

    /// <summary>Called by the Landing presentation after the score readout has rendered this frame.</summary>
    public void Refresh(LandingChallengeRuntime model)
    {
        bool striking = readout != null && readout.gameObject.activeSelf;
        if (idle != null) idle.SetActive(!striking);
        bool beside = contributors != null && contributors.IsShowingContributors;
        Layout();
        // At rest the gauge mirrors the challenge: ready while it runs, then the line's verdict once it has ended.
        var state = model != null ? model.State : LandingChallengeState.Active;
        bool open = state == LandingChallengeState.Active;
        if (ready != null)
        {
            ready.text = open ? "READY" : state == LandingChallengeState.Succeeded ? "LANDED" : "SPENT";
            ready.color = open ? new Color32(242, 230, 200, 255) : state == LandingChallengeState.Succeeded ? new Color32(150, 220, 170, 255) : new Color32(143, 131, 110, 255);
        }
        if (hook != null) hook.color = open ? Color.white : new Color(.55f, .5f, .45f, 1);
        // The contributor's own name owns the label while it is shown; otherwise the slot names the phase of the strike.
        if (!beside && label != null)
            label.text = striking ? StrikeLabel : open ? IdleLabel : state == LandingChallengeState.Succeeded ? "CREATURE LANDED" : "NO ATTEMPTS LEFT";
        if (!striking && legend != null && model != null)
            legend.text = open ? "GOOD " + LandingScoreRush.Format(model.GoodPoints) + "   ·   PERFECT " + LandingScoreRush.Format(model.PerfectPoints) : "";
    }
}
