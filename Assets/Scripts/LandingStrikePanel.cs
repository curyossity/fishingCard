using TMPro;
using UnityEngine;

/// <summary>
/// The player's side of the Landing machine, showing only who is attacking. At rest the Strike gauge shows a hook, READY and the
/// accuracy bases. During a strike it shows the hook and the grade for the accuracy hit, and each contributing rig creature alone
/// (portrait, with its name and level in the label slot). The numbers themselves float off the creature card at each ram
/// (LandingDamageNumbers), and the encounter creature lives on the opposite card, so the two sides never swap identities.
/// </summary>
public sealed class LandingStrikePanel : MonoBehaviour
{
    [SerializeField] private GameObject idle, face;
    [SerializeField] private TMP_Text legend, label, ready, grade;
    [SerializeField] private UnityEngine.UI.Image hook;
    [SerializeField] private CanvasGroup readout;
    [SerializeField] private LandingScoringPortrait contributors;
    public const string IdleLabel = "SPACE TO STRIKE", StrikeLabel = "YOUR STRIKE";

    private void OnEnable() { if (contributors != null) contributors.Changed += Layout; }
    private void OnDisable() { if (contributors != null) contributors.Changed -= Layout; }

    /// <summary>The strike face (hook and grade) gives way to a contributor's portrait the moment one appears.</summary>
    private void Layout()
    {
        bool striking = readout != null && readout.gameObject.activeSelf;
        bool beside = contributors != null && contributors.IsShowingContributors;
        if (face != null) face.SetActive(striking && !beside);
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
        if (grade != null && model?.LastScore != null)
        {
            var accuracy = model.LastScore.Accuracy;
            grade.text = accuracy.ToString().ToUpperInvariant();
            grade.color = accuracy == LandingAccuracy.Perfect ? new Color32(243, 210, 126, 255) : accuracy == LandingAccuracy.Good ? new Color32(242, 230, 200, 255) : new Color32(143, 131, 110, 255);
        }
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
