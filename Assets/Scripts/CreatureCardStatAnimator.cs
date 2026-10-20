using TMPro;
using UnityEngine;

/// <summary>Animates only the active encounter's displayed integers; resolved gameplay values never wait for presentation.</summary>
public sealed class CreatureCardStatAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text weightText, valueText;
    [SerializeField, Min(.05f)] private float secondsPerInteger = .16f;
    [SerializeField, Min(0)] private float initialHold = .25f;
    [SerializeField, Range(1, 1.5f)] private float modifiedScale = 1.22f;
    [SerializeField] private Color positiveInk = new Color32(24, 112, 58, 255);
    [SerializeField] private Color negativeInk = new Color32(174, 44, 38, 255);
    private sealed class Counter
    {
        public TMP_Text Text;
        public Color NormalInk;
        public Vector3 NormalScale;
        public int Displayed, Target, Baseline, Polarity;
        public float Wait;
        public bool Initialized;
    }
    private Counter weight, value;
    private CardDefinition card;
    private int revision;
    private bool presenting;
    private bool playbackPaused;

    /// <summary>Holds the baseline while an incoming card is moving into view.</summary>
    public void SetPaused(bool paused) { playbackPaused = paused; }

    /// <summary>Pauses animation before shared stat fields are rebound; unanimated snapshots retain their supplied values.</summary>
    public void PrepareForRefresh()
    {
        EnsureCounters(); presenting = false;
        RestoreStyle(weight); RestoreStyle(value);
    }

    /// <summary>Rebinds occurrence-aware targets without restarting an unchanged animation on ordinary HUD refreshes.</summary>
    public void Present(CardDefinition definition, EncounterStatFeedback feedback, int finalWeight, int finalValue, bool weightHidden, bool valueHidden)
    {
        EnsureCounters();
        bool fresh = card != definition || revision != feedback.EncounterRevision;
        card = definition; revision = feedback.EncounterRevision; presenting = true;
        Bind(weight, feedback.BaselineWeight, finalWeight, fresh, true, weightHidden);
        Bind(value, feedback.BaselineValue, finalValue, fresh, false, valueHidden);
    }

    /// <summary>Initializes field styles only once, before runtime emphasis is applied.</summary>
    private void EnsureCounters()
    {
        if (weight == null) weight = CreateCounter(weightText);
        if (value == null) value = CreateCounter(valueText);
    }

    /// <summary>Captures the prefab's default text colour and transform scale.</summary>
    private static Counter CreateCounter(TMP_Text text)
    {
        return new Counter { Text = text, NormalInk = text == null ? Color.white : text.color,
            NormalScale = text == null ? Vector3.one : text.rectTransform.localScale };
    }

    /// <summary>Distinguishes chain-modified values from role-only changes and resumes from the currently visible integer.</summary>
    private void Bind(Counter counter, int baseline, int target, bool fresh, bool lowerIsBetter, bool hidden)
    {
        if (counter.Text == null) return;
        if (hidden)
        {
            counter.Initialized = false; RestoreStyle(counter); return;
        }
        bool modified = target != baseline;
        bool start = fresh || !counter.Initialized;
        bool changed = start || target != counter.Target || baseline != counter.Baseline;
        bool wasModified = counter.Target != counter.Baseline;
        if (start) counter.Displayed = modified ? baseline : target;
        else if (changed && !modified && !wasModified) counter.Displayed = target;
        counter.Target = target; counter.Baseline = baseline; counter.Initialized = true;
        if (changed)
        {
            counter.Wait = initialHold;
            int direction = modified ? target.CompareTo(baseline) : target.CompareTo(counter.Displayed);
            counter.Polarity = lowerIsBetter ? -direction : direction;
        }
        if (!Application.isPlaying) counter.Displayed = target;
        Render(counter);
    }

    /// <summary>Advances each visible integer using unscaled time; refreshes and pause do not consume or reset the count.</summary>
    private void Update()
    {
        if (!presenting || playbackPaused) return;
        Tick(weight); Tick(value);
    }

    /// <summary>Shows at most one integer per frame, retaining each step long enough to be read.</summary>
    private void Tick(Counter counter)
    {
        if (counter == null || !counter.Initialized || counter.Text == null) return;
        if (counter.Displayed != counter.Target)
        {
            counter.Wait -= Time.unscaledDeltaTime;
            if (counter.Wait <= 0)
            {
                counter.Displayed += counter.Target > counter.Displayed ? 1 : -1;
                counter.Wait = secondsPerInteger;
            }
        }
        Render(counter);
    }

    /// <summary>Keeps final modified stats emphasised and restores default appearance once a removed modifier counts back to baseline.</summary>
    private void Render(Counter counter)
    {
        counter.Text.text = counter.Displayed.ToString();
        bool emphasised = counter.Target != counter.Baseline || counter.Displayed != counter.Target;
        counter.Text.color = emphasised ? counter.Polarity > 0 ? positiveInk : negativeInk : counter.NormalInk;
        counter.Text.rectTransform.localScale = counter.NormalScale * (emphasised ? modifiedScale : 1);
    }

    /// <summary>Clears presentation state when the card is blanked, hidden or reused outside the current encounter.</summary>
    public void ResetPresentation()
    {
        PrepareForRefresh(); card = null; playbackPaused = false;
        weight.Initialized = value.Initialized = false;
    }

    /// <summary>Restores authored styling without changing the caller's supplied text.</summary>
    private static void RestoreStyle(Counter counter)
    {
        if (counter?.Text == null) return;
        counter.Text.color = counter.NormalInk; counter.Text.rectTransform.localScale = counter.NormalScale;
    }

    /// <summary>Discards pending counters when the slot leaves presentation.</summary>
    private void OnDisable() { ResetPresentation(); }
}
