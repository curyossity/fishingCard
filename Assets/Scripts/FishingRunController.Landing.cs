using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class FishingRunController
{
    [Header("Landing Challenge")]
    [SerializeField] private LandingChallengeProfile landingChallengeProfile;
    private readonly LandingChallengeRuntime landing = new LandingChallengeRuntime();
    private LandingSettings landingSettings;
    private System.Random landingRandom;
    private LandingRigMember[] preparedLandingRig = Array.Empty<LandingRigMember>();
    private string landingConfigurationError;
    private bool landingTerminalHandled, awaitingRigPlacement, landingResultDismissed;
    private int lastLandingInputFrame = -1;
    private int preparedExtraAttempts, preparedProtectedMisses;
    public LandingChallengeRuntime Landing => landing;
    public bool UsesLandingChallenge => UsesSlotMachine && landingChallengeProfile != null;
    public bool AwaitingRigPlacement => awaitingRigPlacement;
    public bool LandingFailureVisible => !landingResultDismissed && lastSurfaceResult.EndedByLandingFailure;
    public bool LandingBlocksLegacyActions => UsesLandingChallenge;
    public event Action<CardDefinition, int> CreatureLanded;

    /// <summary>Validates and freezes landing tuning before an expedition starts; no shared asset holds runtime state.</summary>
    private bool PrepareLandingSettings()
    {
        if (!UsesLandingChallenge) return true;
        try { landingSettings = landingChallengeProfile.CreateSettings(); return true; }
        catch (ArgumentException error) { slotPreparationFailure = error.Message; return false; }
        catch (OverflowException error) { slotPreparationFailure = error.Message; return false; }
    }

    /// <summary>Resets encounter scoring and supplies an independent random stream so movement never rerolls creature discovery.</summary>
    private void ResetLandingForExpedition(int seed)
    {
        landing.Reset(); awaitingRigPlacement = false; landingTerminalHandled = false; landingResultDismissed = false;
        lastLandingInputFrame = -1; landingRandom = new System.Random(unchecked(seed ^ 0x46a32d17));
        preparedLandingRig = Array.Empty<LandingRigMember>(); landingConfigurationError = null;
    }

    /// <summary>Freezes only committed species and their current-level primary abilities for the next challenge.</summary>
    private bool PrepareLandingRig()
    {
        if (!UsesLandingChallenge) return true;
        try
        {
            preparedLandingRig = rig.SnapshotForLanding(out preparedExtraAttempts, out preparedProtectedMisses);
            var ids = new HashSet<int>();
            foreach (var member in preparedLandingRig) if (!ids.Add(member.Id)) throw new ArgumentException("Duplicate rig instance identity.");
            foreach (LandingAccuracy accuracy in Enum.GetValues(typeof(LandingAccuracy)))
            {
                decimal maximumTotal = checked(LandingScoring.Resolve(accuracy, landingSettings, preparedLandingRig).Points * checked(landingSettings.Attempts + preparedExtraAttempts));
                if (maximumTotal < 0) throw new ArgumentException("Landing totals must be nonnegative.");
            }
            landingConfigurationError = null; return true;
        }
        catch (ArgumentException error) { landingConfigurationError = error.Message; return false; }
        catch (OverflowException error) { landingConfigurationError = error.Message; return false; }
    }

    /// <summary>Begins only after successful slot selection and prevalidated scoring data.</summary>
    private void BeginSelectedLanding()
    {
        if (!UsesLandingChallenge) return;
        decimal resistance = landingSettings.Resistance(CurrentEncounter.LandingResistance, Math.Max(0, currentDepth - startingDepth));
        landing.Begin(CurrentEncounterRevision, resistance, landingSettings, preparedLandingRig, landingRandom, Time.realtimeSinceStartupAsDouble,
            preparedExtraAttempts, preparedProtectedMisses, CurrentEncounter);
        landingTerminalHandled = false; awaitingRigPlacement = false; lastLandingInputFrame = Time.frameCount;
    }

    /// <summary>Samples the authoritative clock independently of view frame order; a strike briefly freezes both moving elements.</summary>
    private void AdvanceLanding()
    {
        if (runActive && UsesLandingChallenge && !awaitingRigPlacement && landing.State == LandingChallengeState.Active)
            landing.AdvanceTo(Time.realtimeSinceStartupAsDouble);
    }

    /// <summary>Commits one occurrence/revision-bound strike, rejecting held keys, duplicate UI submission and input during feedback.</summary>
    public bool TryStrikeLanding(long occurrence, long revision)
    {
        if (!UsesLandingChallenge || !CoreActionInputReady || awaitingRigPlacement || landingTerminalHandled
            || lastLandingInputFrame == Time.frameCount || !encounterRuntime.AwaitingLandingChallenge
            || landing.EncounterRevision != CurrentEncounterRevision) return false;
        if (!landing.TryStrike(occurrence, revision, Time.realtimeSinceStartupAsDouble)) return false;
        lastLandingInputFrame = Time.frameCount;
        if (landing.State == LandingChallengeState.Succeeded)
        {
            landingTerminalHandled = true;
            if (!encounterRuntime.TryFinishLanding(CurrentEncounterRevision)) throw new InvalidOperationException("Landing lost its encounter ownership.");
            awaitingRigPlacement = true;
            CreatureLanded?.Invoke(CurrentEncounter, CurrentEncounterRevision);
            BeginRigAcquisition();
        }
        else if (landing.State == LandingChallengeState.Failed)
        {
            landingTerminalHandled = true;
            CompleteLandingFailure();
        }
        RefreshViews();
        return true;
    }

    /// <summary>Snapshots and pays the full prior Catch haul once, excluding the failed creature and every Bait sale value.</summary>
    private void CompleteLandingFailure()
    {
        if (!runActive || lastSurfaceResult.EndedByLandingFailure) return;
        lastSurfaceResult.RecordLandingFailure(rig.CreateHaulSnapshot(), currentDepth);
        runRewardRuntime.AwardGold(lastSurfaceResult);
        SavePersistentWallet();
        EndActiveRun();
        RunCompleted?.Invoke(expeditionId, lastSurfaceResult);
    }

    /// <summary>Closes only the completed landing result; it does not abandon an active expedition or grant a new creature.</summary>
    public void DismissLandingResult()
    {
        if (runActive || !LandingFailureVisible) return;
        landingResultDismissed = true; RefreshViews();
    }
}
