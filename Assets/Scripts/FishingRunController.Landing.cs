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
    private double landingFeedbackUntil, landingRevealUntil, landingOutcomeUntil;
    public bool LandingFeedbackPending { get; private set; }
    /// <summary>True while the Landed/Escaped beat plays after score feedback; Build, auto-upgrade and the Harbor return wait for it.</summary>
    public bool LandingOutcomePending { get; private set; }
    /// <summary>Real-time start shared by the outcome gate's deadline and its presentation.</summary>
    public double LandingOutcomeStartedAt { get; private set; }
    /// <summary>Which beat closes the current challenge; meaningful only once it has ended.</summary>
    public LandingOutcomeKind LandingOutcome => landing.State == LandingChallengeState.Succeeded ? LandingOutcomeKind.Landed : LandingOutcomeKind.Escaped;
    /// <summary>Schedule of the current outcome beat: the escape tally covers every creature in the paid haul.</summary>
    public LandingOutcomeTiming LandingOutcomeTiming => LandingOutcomeTiming.For(LandingOutcome, lastSurfaceResult.EndedByLandingFailure ? lastSurfaceResult.Haul.Length : 0);
    /// <summary>True while the encounter creature's entrance plays; motion is held and strikes are rejected.</summary>
    public bool LandingRevealPending { get; private set; }
    /// <summary>Real-time start shared by the gate deadline and the reveal presentation, so both end together.</summary>
    public double LandingRevealStartedAt { get; private set; }
    public LandingChallengeRuntime Landing => landing;
    public LandingStrikeReceipt LastLandingStrike { get; private set; }
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
        LandingFeedbackPending = false; landingFeedbackUntil = 0;
        LandingRevealPending = false; landingRevealUntil = 0;
        LandingOutcomePending = false; landingOutcomeUntil = 0;
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
        LandingOutcomePending = false; landingOutcomeUntil = 0;
        // The entrance is cosmetic: it holds the motion clock and blocks strikes, never delaying or changing an outcome.
        // The entrance starts as the Discover-to-Landing transition locks in, so it is never played behind it.
        LandingRevealStartedAt = Time.realtimeSinceStartupAsDouble + (fishingRunView != null ? fishingRunView.PhaseTransitionLead : 0);
        LandingRevealPending = fishingRunView != null && fishingRunView.HasLandingRevealPresentation;
        landingRevealUntil = LandingRevealPending ? LandingRevealStartedAt + LandingRevealTiming.For(CurrentEncounter.Rarity).Duration : 0;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor preview only: re-arms the cosmetic entrance gate on the current, unstruck-or-idle Landing so a reveal of any rarity can be
    /// reviewed without hunting for that creature. Changes no score, attempt, motion setting or encounter identity.
    /// </summary>
    public bool PreviewLandingReveal(CardRarity rarity)
    {
        if (!UsesLandingChallenge || !runActive || LandingFeedbackPending || awaitingRigPlacement
            || landing.State != LandingChallengeState.Active) return false;
        LandingRevealStartedAt = Time.realtimeSinceStartupAsDouble;
        LandingRevealPending = true;
        landingRevealUntil = LandingRevealStartedAt + LandingRevealTiming.For(rarity).Duration;
        return true;
    }
#endif

    /// <summary>Ends the cosmetic entrance early (Space/Strike) or at its deadline; the same input frame cannot also strike.</summary>
    public void CompleteLandingReveal()
    {
        if (!LandingRevealPending) return;
        LandingRevealPending = false;
        lastLandingInputFrame = Time.frameCount;
        RefreshViews();
    }

    /// <summary>Samples the authoritative clock independently of view frame order; a strike briefly freezes both moving elements.</summary>
    private void AdvanceLanding()
    {
        if (LandingRevealPending)
        {
            double now = Time.realtimeSinceStartupAsDouble;
            landing.HoldPresentationClock(now);
            if (now < landingRevealUntil && fishingRunView != null && fishingRunView.HasLandingRevealPresentation) return;
            CompleteLandingReveal();
        }
        if (LandingFeedbackPending)
        {
            double now = Time.realtimeSinceStartupAsDouble;
            landing.HoldPresentationClock(now);
            if (now < landingFeedbackUntil && fishingRunView != null && fishingRunView.HasLandingRushPresentation) return;
            CompleteLandingFeedback();
        }
        if (LandingOutcomePending)
        {
            if (Time.realtimeSinceStartupAsDouble < landingOutcomeUntil && fishingRunView != null && fishingRunView.HasLandingOutcomePresentation) return;
            CompleteLandingOutcome();
        }
        if (runActive && UsesLandingChallenge && !awaitingRigPlacement && landing.State == LandingChallengeState.Active)
            landing.AdvanceTo(Time.realtimeSinceStartupAsDouble);
    }

    /// <summary>Commits one occurrence/revision-bound strike, rejecting held keys, duplicate UI submission and input during feedback.</summary>
    public bool TryStrikeLanding(long occurrence, long revision)
    {
        if (!UsesLandingChallenge || !CoreActionInputReady || LandingRevealPending || awaitingRigPlacement || landingTerminalHandled
            || lastLandingInputFrame == Time.frameCount || !encounterRuntime.AwaitingLandingChallenge
            || landing.EncounterRevision != CurrentEncounterRevision) return false;
        if (!landing.TryStrike(occurrence, revision, Time.realtimeSinceStartupAsDouble)) return false;
        LastLandingStrike = new LandingStrikeReceipt(landing, CatchChain);
        LandingFeedbackPending = fishingRunView != null && fishingRunView.HasLandingRushPresentation;
        if (LandingFeedbackPending) landingFeedbackUntil = Time.realtimeSinceStartupAsDouble + new LandingImpactTiming(new LandingScoreSequence(LastLandingStrike.Score)).Duration;
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
        if (landingTerminalHandled && !LandingFeedbackPending) BeginLandingOutcome();
        RefreshViews();
        return true;
    }

    /// <summary>Finishes only the cosmetic hold; terminal score and failure settlement have already resolved.</summary>
    public void CompleteLandingFeedback()
    {
        if (!LandingFeedbackPending) return;
        LandingFeedbackPending = false;
        lastLandingInputFrame = Time.frameCount;
        if (landingTerminalHandled) BeginLandingOutcome();
        RefreshViews();
    }

    /// <summary>
    /// Starts the cosmetic Landed/Escaped beat once score feedback has finished. The outcome, payout and rig cleanup have already
    /// resolved; only the duplicate auto-upgrade, Build and the Harbor return wait, bounded by the schedule's own deadline.
    /// </summary>
    private void BeginLandingOutcome()
    {
        if (LandingOutcomePending) return;
        LandingOutcomePending = fishingRunView != null && fishingRunView.HasLandingOutcomePresentation;
        if (!LandingOutcomePending) { ContinueAfterLandingOutcome(); return; }
        LandingOutcomeStartedAt = Time.realtimeSinceStartupAsDouble;
        landingOutcomeUntil = LandingOutcomeStartedAt + LandingOutcomeTiming.Duration;
    }

#if UNITY_EDITOR
    /// <summary>Editor review only: keeps the outcome gate open while a capture seeks through the beat; CompleteLandingOutcome ends it.</summary>
    public void HoldLandingOutcomeForReview() { if (LandingOutcomePending) landingOutcomeUntil = double.MaxValue; }
#endif

    /// <summary>Ends the outcome beat early (Space/Skip) or at its deadline; the same input frame cannot also act on the next screen.</summary>
    public void CompleteLandingOutcome()
    {
        if (!LandingOutcomePending) return;
        LandingOutcomePending = false;
        lastLandingInputFrame = Time.frameCount;
        ContinueAfterLandingOutcome();
        RefreshViews();
    }

    /// <summary>A landed duplicate upgrades automatically once its outcome has been shown; a new species opens Build instead.</summary>
    private void ContinueAfterLandingOutcome()
    {
        if (runActive && awaitingRigPlacement && pendingRigCreature != null && rig.FindSpecies(pendingRigCreature.UniqueId) != null)
            TryCommitRigPlacement(buildOccurrence, rig.Revision, -1);
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
        if (runActive || !LandingFailureVisible || LandingFeedbackPending || LandingOutcomePending) return;
        landingResultDismissed = true; RefreshViews();
    }
}
