using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Displays completed-run snapshots and existing progression commands in a journal result panel.</summary>
public sealed class JournalResultModal : RunResultPresentation
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private JournalModalHost host;
    private GameObject root;
    private TMP_Text title, gold, stats, haul, released, lost, upgradeText, unlockText, upgradeLabel, unlockLabel;
    private UnityEngine.UI.Button upgrade, unlock, next;
    private Func<bool> purchaseAction, unlockAction;
    private Action nextAction;
    public bool IsVisible => root != null && root.activeInHierarchy;
    public UnityEngine.UI.Button NextRunButton => next;

    /// <summary>Creates stable result columns and scrolling lists without copying gameplay result state.</summary>
    private void EnsureLayout()
    {
        if (root != null) return;
        var panel = JournalModalLayout.Panel(transform, theme, "Run Results", out root);
        title = JournalModalLayout.Text("Result Title", panel, new Rect(.04f, .89f, .92f, .07f), theme, 60, true);
        gold = JournalModalLayout.Text("Gold", panel, new Rect(.04f, .82f, .92f, .06f), theme, 42);
        stats = JournalModalLayout.Text("Run Stats", panel, new Rect(.04f, .70f, .92f, .10f), theme, 34);
        haul = Column(panel, "BROUGHT HOME", .04f); released = Column(panel, "RELEASED", .36f); lost = Column(panel, "LOST", .68f);
        upgradeText = JournalModalLayout.Text("Upgrade Status", panel, new Rect(.04f, .15f, .44f, .10f), theme, 30);
        upgrade = JournalModalLayout.Button("Upgrade Line", panel, new Rect(.04f, .085f, .44f, .06f), theme, out upgradeLabel);
        unlockText = JournalModalLayout.Text("Unlock Status", panel, new Rect(.52f, .15f, .44f, .10f), theme, 30);
        unlock = JournalModalLayout.Button("Unlock Technique", panel, new Rect(.52f, .085f, .44f, .06f), theme, out unlockLabel);
        next = JournalModalLayout.Button("Start Next Run", panel, new Rect(.04f, .015f, .92f, .06f), theme, out TMP_Text nextLabel); nextLabel.text = "START NEXT RUN";
        upgrade.onClick.AddListener(() => { if (upgrade.interactable) purchaseAction?.Invoke(); });
        unlock.onClick.AddListener(() => { if (unlock.interactable) unlockAction?.Invoke(); });
        next.onClick.AddListener(() => { if (!next.interactable) return; next.interactable = false; nextAction?.Invoke(); });
    }
    /// <summary>Creates an independently scrollable result list so long hauls do not truncate.</summary>
    private TMP_Text Column(Transform panel, string name, float x)
    {
        JournalModalLayout.Text(name, panel, new Rect(x, .63f, .28f, .055f), theme, 36, true).text = name;
        var content = JournalModalLayout.Scroll(panel, new Rect(x, .28f, .28f, .34f), name + " List");
        var text = JournalModalLayout.Text("Entries", content, new Rect(0, 0, 1, 1), theme, 34); text.enableAutoSizing = false; text.alignment = TextAlignmentOptions.TopLeft;
        return text;
    }
    /// <summary>Uses the controller's completed result, legality and callbacks; hidden values reveal only in these result snapshots.</summary>
    public override void Refresh(bool runActive, FishingRunResult result, int totalGold, RunProgressionRuntime progression,
        bool canPurchaseUpgrade, string upgradeReason, bool canUnlockTechnique, string techniqueReason,
        Func<bool> purchaseUpgrade, Func<bool> unlockTechnique, Action startNextRun)
    {
        if (runActive || result == null || !result.HasResult) { Hide(); return; }
        EnsureLayout(); bool opening = !IsVisible;
        purchaseAction = purchaseUpgrade; unlockAction = unlockTechnique; nextAction = startNextRun;
        host.Show(root); title.text = result.EndedByLineBreak ? "LINE BREAK" : "RUN COMPLETE";
        gold.text = "+" + result.GoldAwarded + " GOLD     TOTAL " + Mathf.Max(0, totalGold);
        string load = result.EndedByLineBreak ? "FORCED SURFACE" : result.WasOverloaded ? "OVERLOADED AT SURFACE" : "LINE HELD WITHIN CAPACITY";
        stats.text = "HAUL VALUE " + result.HaulValue + "     DEPTH " + result.SurfaceDepth + " m\nLINE LOAD " + result.SurfaceLineLoad + " / " + result.LineCapacity + "     " + load;
        haul.text = List(result.Haul); released.text = List(result.ReleasedCatches); lost.text = List(result.LostCatches);
        upgrade.interactable = canPurchaseUpgrade; upgradeLabel.text = "UPGRADE LINE +" + progression.LineCapacityPerUpgrade + "  |  " + progression.LineCapacityUpgradeCost + " GOLD";
        upgradeText.text = "LINE CAPACITY BONUS +" + progression.CurrentLineCapacityBonus + "     UPGRADES " + progression.PurchasedLineCapacityUpgrades + "/" + progression.MaximumLineCapacityUpgrades
            + (string.IsNullOrWhiteSpace(upgradeReason) ? "" : "\n" + upgradeReason);
        // The active game has no configured unlock. Keep the existing command only when authored data supplies one.
        bool hasUnlock = progression.UnlockableTechniqueCard != null; unlock.gameObject.SetActive(hasUnlock); unlockText.gameObject.SetActive(hasUnlock);
        unlock.interactable = canUnlockTechnique; unlockLabel.text = hasUnlock ? "UNLOCK " + progression.UnlockableTechniqueCard.DisplayName + "  |  " + progression.TechniqueUnlockCost + " GOLD" : "";
        unlockText.text = (hasUnlock ? progression.UnlockableTechniqueCard.RulesText : "") + "\n" + techniqueReason;
        next.interactable = true; JournalModalLayout.Navigation(new[] { upgrade, unlock, next });
        var focused = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
        if (opening || focused == null || !focused.transform.IsChildOf(root.transform) || focused.GetComponent<UnityEngine.UI.Selectable>()?.IsInteractable() == false) next.Select();
    }
    /// <summary>Formats role-aware results without changing values or mutating the haul.</summary>
    private static string List(CardInstance[] catches)
    {
        if (catches == null || catches.Length == 0) return "None";
        var text = new StringBuilder(); foreach (var card in catches)
        {
            if (text.Length > 0) text.AppendLine(); text.Append(card?.Definition == null ? "Unknown catch" : card.Definition.DisplayName);
            if (card != null) text.Append(card.IsBait ? " [BAIT]  (0 Value)" : "  (" + card.HaulValueContribution + " Value)");
        }
        return text.ToString();
    }
    /// <summary>Hides results without discarding runtime rewards or progression.</summary>
    public override void Hide() { purchaseAction = unlockAction = null; nextAction = null; if (root != null) host.Hide(root); }
}
