using System;
using UnityEngine;

public sealed partial class FishingRunController
{
    private readonly RigRuntime rig = new RigRuntime();
    private bool rigResolutionInProgress;
    private long buildOccurrence;
    private CardDefinition pendingRigCreature;
    private int pendingRigEncounterRevision;
    public RigRuntime Rig => rig;
    public long BuildOccurrence => buildOccurrence;
    public CardDefinition PendingRigCreature => pendingRigCreature;
    public string BuildFailure { get; private set; }
    public string RigNotice { get; private set; }
    public RigAcquisitionReceipt LastRigAcquisition { get; private set; }
    public bool RigUpgradeFeedbackVisible { get; private set; }
    public event Action<CardDefinition, bool> RigAcquisitionCompleted;

    /// <summary>Clears all temporary upgrades and invalidates pending placement commands at expedition boundaries.</summary>
    private void ResetRigExpedition()
    {
        rig.Reset(); pendingRigCreature = null; buildOccurrence++; BuildFailure = RigNotice = null;
        LastRigAcquisition = null; RigUpgradeFeedbackVisible = false;
    }

    /// <summary>Reserves a landed occurrence, automatically upgrading an existing species without showing placement controls.</summary>
    private void BeginRigAcquisition()
    {
        pendingRigCreature = CurrentEncounter; pendingRigEncounterRevision = CurrentEncounterRevision;
        buildOccurrence++; BuildFailure = null;
        if (!LandingFeedbackPending && rig.FindSpecies(pendingRigCreature.UniqueId) != null)
            TryCommitRigPlacement(buildOccurrence, rig.Revision, -1);
    }

    /// <summary>Commits a single legal placement or automatic duplicate upgrade, then continues without legacy Descend/overload effects.</summary>
    public bool TryCommitRigPlacement(long occurrence, long revision, int position)
    {
        if (!UsesLandingChallenge || !CoreActionInputReady || rigResolutionInProgress || !awaitingRigPlacement
            || occurrence != buildOccurrence || revision != rig.Revision || pendingRigCreature == null
            || CurrentEncounterRevision != pendingRigEncounterRevision || HookedEncounter != pendingRigCreature) return false;
        rigResolutionInProgress = true;
        try
        {
            var creature = pendingRigCreature;
            int previousHaul = rig.HaulValue, previousLevel = rig.FindSpecies(creature.UniqueId)?.Level ?? 0;
            string biomeName = CurrentBiome?.GetDepthTier(CurrentDepth)?.DisplayName ?? CurrentBiome?.DisplayName ?? "Waters";
            if (!rig.TryAcquire(revision, creature, position, out bool upgraded, out string error))
            { BuildFailure = error; return false; }
            encounterRuntime.TakeHookedEncounter();
            var member = rig.FindSpecies(creature.UniqueId);
            RigNotice = creature.DisplayName + (upgraded ? " UPGRADED" : " PLACED") + " / LV " + member.Level;
            LastRigAcquisition = new RigAcquisitionReceipt(buildOccurrence, member, rig.Members, previousLevel, previousHaul, rig.HaulValue, biomeName);
            RigUpgradeFeedbackVisible = upgraded && fishingRunView != null && fishingRunView.HasRigBuildingPresentation;
            pendingRigCreature = null; awaitingRigPlacement = false; BuildFailure = null; buildOccurrence++;
            biomeApexRuntime.RecordCommittedApex(creature);
            ContinueAfterRigAcquisition();
            RigAcquisitionCompleted?.Invoke(creature, upgraded);
            return true;
        }
        finally { rigResolutionInProgress = false; RefreshViews(); }
    }

    /// <summary>Dismisses only the completed upgrade's cosmetic display; acquisition and continuation have already resolved.</summary>
    public bool CompleteRigUpgradeFeedback(long occurrence)
    {
        if (!RunActive || !RigUpgradeFeedbackVisible || LastRigAcquisition?.Occurrence != occurrence) return false;
        RigUpgradeFeedbackVisible = false; RefreshViews(); return true;
    }

    /// <summary>Advances one ordinary encounter using existing authored depth/biome ownership, retaining the entire rig.</summary>
    private void ContinueAfterRigAcquisition()
    {
        selectedCatchIndex = -1;
        currentDepth = checked(currentDepth + Mathf.Max(1, depthStepPerDescend));
        landing.Reset();
        RevealEncounterAtCurrentDepth();
    }
}
