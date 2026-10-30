using System;
using TMPro;
using UnityEngine;

/// <summary>Shared elastic travel, stopped feedback, focus, particles and cancellation for timing instruments.</summary>
public abstract class JournalEncounterTimingView : EncounterTimingView
{
    [SerializeField] protected CanvasGroup visibility;
    [SerializeField] protected TMP_Text heading, subtitle, instruction, stopLabel;
    [SerializeField] protected UnityEngine.UI.Button stopButton;
    protected EncounterTiming Timing { get; private set; }
    private FishingRunView owner;
    private RectTransform motionRoot;
    private Vector2 restingPosition;
    private long occurrence = -1;
    private bool lastStopped, focusPending;
    private float entranceStartedAt = -1, exitStartedAt = -1, entranceProgress, exitProgress;
    private JournalRarityCelebration celebration;
    public float EntranceProgress => entranceProgress;
    public float ExitProgress => exitProgress;
    public UnityEngine.UI.Button StopButton => stopButton;
    public JournalRarityCelebration Celebration => celebration;
    protected abstract RectTransform Marker { get; }
    protected virtual Vector2 CelebrationPoint => Marker.anchoredPosition + new Vector2(0, -60);

    /// <summary>Binds exact occurrence input and resets travel only for a new occurrence or timing model.</summary>
    public sealed override void Present(EncounterTiming state, Func<long, bool> action, BiomeDefinition biome, int depth)
    {
        if (motionRoot == null) { motionRoot = (RectTransform)transform; restingPosition = motionRoot.anchoredPosition; }
        bool differentModel = !ReferenceEquals(Timing, state);
        Timing = state;
        if (owner == null) owner = GetComponentInParent<FishingRunView>();
        bool active = state != null && state.Active;
        gameObject.SetActive(active);
        if (!active) return;
        subtitle.text = (biome == null ? "Uncharted Waters" : biome.DisplayName) + " · " + depth + " m";
        if (differentModel || occurrence != state.Occurrence)
        {
            ResetMotion();
            motionRoot.anchoredPosition = restingPosition + JournalRarityExitMotion.EntranceOffset(0, motionRoot.rect.height);
            occurrence = state.Occurrence; lastStopped = false; focusPending = true;
            long token = occurrence;
            stopButton.onClick.RemoveAllListeners(); stopButton.onClick.AddListener(() => action?.Invoke(token));
            BindOccurrence();
        }
        Render();
    }

    /// <summary>Builds instrument-specific zone bindings without owning gameplay selection.</summary>
    protected abstract void BindOccurrence();
    /// <summary>Renders the current marker, widths and exact stopped result.</summary>
    protected abstract void DrawInstrument();

    /// <summary>Tracks visual travel independently of the authoritative encounter clock.</summary>
    protected virtual void Update() { Render(); }
    /// <summary>Accepts Stop only after this occurrence reaches its resting position.</summary>
    public sealed override bool HasCompletedEntrance(long token)
    { return !isActiveAndEnabled || occurrence == token && entranceProgress >= 1; }
    /// <summary>Releases reveal only after this occurrence is completely off-screen.</summary>
    public sealed override bool HasCompletedExit(long token)
    { return !isActiveAndEnabled || occurrence == token && exitProgress >= 1; }
    /// <summary>Restores the reusable pose and clears particles when hidden or cancelled.</summary>
    protected virtual void OnDisable() { ResetMotion(); }

    /// <summary>Clears presentation travel without modifying outcome, balance or encounter state.</summary>
    private void ResetMotion()
    {
        entranceStartedAt = exitStartedAt = -1; entranceProgress = exitProgress = 0;
        if (celebration != null) celebration.Clear();
        if (motionRoot != null) motionRoot.anchoredPosition = restingPosition;
    }

    /// <summary>Uses the original one-second elastic entry/exit and the same stopped-result pulse.</summary>
    private void Render()
    {
        if (Timing == null || !Timing.Active) return;
        bool ready = owner == null || !owner.IsEncounterTransitionPlaying;
        if (ready && !Timing.Stopped && entranceProgress < 1)
        {
            if (entranceStartedAt < 0) entranceStartedAt = Time.unscaledTime;
            entranceProgress = Mathf.Clamp01((Time.unscaledTime - entranceStartedAt) / JournalRarityExitMotion.Duration);
            motionRoot.anchoredPosition = restingPosition + JournalRarityExitMotion.EntranceOffset(entranceProgress, motionRoot.rect.height);
        }
        bool interactive = ready && entranceProgress >= 1;
        visibility.alpha = ready ? 1 : 0; visibility.interactable = visibility.blocksRaycasts = interactive;
        stopButton.interactable = interactive && !Timing.Stopped;
        if (interactive && focusPending && !Timing.Stopped) { focusPending = false; stopButton.Select(); }
        DrawInstrument();
        if (Timing.Stopped && !lastStopped)
        {
            lastStopped = true;
            if (Timing.CelebrateStop) PlayCelebration();
        }
        Marker.localScale = Vector3.one * (Timing.Stopped ? 1 + Mathf.Sin(Timing.FeedbackProgress * Mathf.PI) * .08f : 1);
        if (!Timing.ReadyToReveal) return;
        if (exitStartedAt < 0) exitStartedAt = Time.unscaledTime;
        exitProgress = Mathf.Clamp01((Time.unscaledTime - exitStartedAt) / JournalRarityExitMotion.Duration);
        motionRoot.anchoredPosition = restingPosition + Vector2.up * motionRoot.rect.height * JournalRarityExitMotion.Evaluate(exitProgress);
    }

    /// <summary>Reuses the existing native glint burst for a successful precision or legacy top-rarity stop.</summary>
    private void PlayCelebration()
    {
        if (celebration == null)
        {
            var root = new GameObject("Timing Reward Particles", typeof(RectTransform), typeof(CanvasRenderer));
            var rect = root.GetComponent<RectTransform>(); rect.SetParent(Marker.parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(900, 700);
            celebration = root.AddComponent<JournalRarityCelebration>(); celebration.raycastTarget = false;
        }
        celebration.rectTransform.anchoredPosition = CelebrationPoint;
        celebration.transform.SetAsLastSibling(); celebration.Play(occurrence);
    }
}
