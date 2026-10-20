using TMPro;
using UnityEngine;

/// <summary>Shares number feedback between current encounters and attached creatures; gameplay values never wait for presentation.</summary>
public sealed class CreatureCardStatAnimator : MonoBehaviour
{
    public const float RapidSecondsPerInteger = .04f;
    public const float RapidInitialHold = .05f;
    public const float RapidPunchDuration = .18f;
    public const float RapidMaximumCountDuration = .8f;
    [SerializeField] private TMP_Text weightText, valueText;
    [SerializeField, Min(.01f)] private float secondsPerInteger = RapidSecondsPerInteger;
    [SerializeField, Min(0)] private float initialHold = RapidInitialHold;
    [SerializeField, Min(.1f)] private float maximumCountDuration = RapidMaximumCountDuration;
    [SerializeField, Range(1, 1.5f)] private float modifiedScale = 1.22f;
    [SerializeField, Range(0, .4f)] private float stepPunch = .14f;
    [SerializeField, Range(0, .4f)] private float finalPunch = .22f;
    [SerializeField, Min(.1f)] private float punchDuration = RapidPunchDuration;
    [SerializeField, Range(0, 8)] private float tiltDegrees = 2.5f;
    [SerializeField] private Color positiveInk = new Color32(24, 112, 58, 255);
    [SerializeField] private Color negativeInk = new Color32(174, 44, 38, 255);
    private sealed class Counter
    {
        public TMP_Text Text;
        public Color NormalInk;
        public FontStyles NormalStyle;
        public FontWeight NormalWeight;
        public Vector3 NormalScale;
        public Quaternion NormalRotation;
        public int Displayed, Target, Baseline, Step;
        public bool LowerIsBetter;
        public float Wait;
        public float PunchAge, PunchAmount;
        public int Beat;
        public bool Initialized;
    }
    private Counter weight, value;
    private CardDefinition card;
    private int revision;
    private bool presenting;
    private bool playbackPaused;
    private bool deferEmphasis;
    private bool isBait;

    public bool IsAnimating => Pending(weight) || Pending(value);

    /// <summary>Applies canonical rapid timing when rebuilding or migrating the opted-in prefab.</summary>
    public void ConfigureRapidTiming()
    {
        secondsPerInteger = RapidSecondsPerInteger; initialHold = RapidInitialHold;
        punchDuration = RapidPunchDuration;
        maximumCountDuration = RapidMaximumCountDuration;
    }

    /// <summary>Binds compact-card numeric fields before their first animated presentation.</summary>
    public void BindTextFields(TMP_Text weightField, TMP_Text valueField)
    {
        ResetPresentation();
        weightText = weightField; valueText = valueField;
        weight = value = null;
    }

    /// <summary>Includes the final spring settle in sequential presentation completion.</summary>
    private bool Pending(Counter counter)
    {
        return presenting && counter != null && counter.Initialized && counter.Text != null
            && (counter.Displayed != counter.Target || counter.PunchAge < punchDuration);
    }

    /// <summary>Settles at resolved values without discarding occurrence or masking state.</summary>
    public void Complete()
    {
        EnsureCounters();
        foreach (Counter counter in new[] { weight, value })
        {
            if (!counter.Initialized || counter.Text == null) continue;
            counter.Displayed = counter.Target; counter.PunchAge = punchDuration;
            Render(counter);
        }
    }

    /// <summary>Holds the baseline while an incoming card is moving into view.</summary>
    public void SetPaused(bool paused, bool deferStyle = false)
    {
        playbackPaused = paused; deferEmphasis = deferStyle;
        if (weight != null && weight.Initialized && weight.Text != null) Render(weight);
        if (value != null && value.Initialized && value.Text != null) Render(value);
    }

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
        bool roleChanged = !fresh && isBait != feedback.IsBait;
        isBait = feedback.IsBait;
        card = definition; revision = feedback.EncounterRevision; presenting = true;
        Bind(weight, feedback.BaselineWeight, finalWeight, fresh, true, weightHidden);
        // Bait's zero is immediate; returning to Catch starts at authored Value, never at Bait's zero.
        Bind(value, feedback.BaselineValue, finalValue, fresh || roleChanged || isBait, false, valueHidden);
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
            NormalStyle = text == null ? FontStyles.Normal : text.fontStyle,
            NormalWeight = text == null ? FontWeight.Regular : text.fontWeight,
            NormalScale = text == null ? Vector3.one : text.rectTransform.localScale,
            NormalRotation = text == null ? Quaternion.identity : text.rectTransform.localRotation };
    }

    /// <summary>Distinguishes effect modifiers from role-only changes and resumes from the currently visible integer.</summary>
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
        if (start)
        {
            counter.Displayed = modified ? baseline : target;
            counter.PunchAge = punchDuration; counter.Beat = 0;
        }
        else if (changed && !modified && !wasModified) counter.Displayed = target;
        counter.Target = target; counter.Baseline = baseline; counter.Initialized = true;
        counter.LowerIsBetter = lowerIsBetter;
        if (changed)
        {
            counter.Wait = initialHold;
            long distance = System.Math.Abs((long)target - counter.Displayed);
            int steps = Mathf.Max(1, Mathf.FloorToInt(maximumCountDuration / Mathf.Max(.01f, secondsPerInteger)));
            counter.Step = (int)System.Math.Max(1, (distance + steps - 1) / steps);
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

    /// <summary>Rapidly advances once per rendered frame with a new pop/tilt on every change; large deltas use bounded strides.</summary>
    private void Tick(Counter counter)
    {
        if (counter == null || !counter.Initialized || counter.Text == null) return;
        counter.PunchAge = Mathf.Min(punchDuration, counter.PunchAge + Time.unscaledDeltaTime);
        if (counter.Displayed != counter.Target)
        {
            counter.Wait -= Time.unscaledDeltaTime;
            if (counter.Wait <= 0)
            {
                counter.Displayed = counter.Target > counter.Displayed
                    ? (int)System.Math.Min(counter.Target, (long)counter.Displayed + counter.Step)
                    : (int)System.Math.Max(counter.Target, (long)counter.Displayed - counter.Step);
                // Carry fractional timing forward without doing invisible bursts of updates within one frame.
                counter.Wait = Mathf.Max(0, counter.Wait + Mathf.Max(.01f, secondsPerInteger));
                counter.PunchAge = 0;
                counter.PunchAmount = counter.Displayed == counter.Target ? finalPunch : stepPunch;
                counter.Beat++;
            }
        }
        Render(counter);
    }

    /// <summary>Pops and rocks each counting beat with a damped spring, then settles at the appropriate persistent style.</summary>
    private void Render(Counter counter)
    {
        counter.Text.text = counter.Displayed.ToString();
        bool emphasised = counter.Target != counter.Baseline || counter.Displayed != counter.Target;
        bool deferred = playbackPaused && deferEmphasis && Pending(counter);
        emphasised &= !deferred;
        bool bold = !deferred && (emphasised || Pending(counter));
        counter.Text.fontStyle = bold ? counter.NormalStyle | FontStyles.Bold : counter.NormalStyle;
        counter.Text.fontWeight = bold ? FontWeight.Bold : counter.NormalWeight;
        // Follow the visible amount, not the destination or travel direction. Crossing default never flashes neutral.
        int direction = counter.Displayed.CompareTo(counter.Baseline);
        if (direction == 0 && counter.Target != counter.Baseline) direction = counter.Target.CompareTo(counter.Baseline);
        int polarity = counter.LowerIsBetter ? -direction : direction;
        counter.Text.color = !deferred && polarity != 0 ? polarity > 0 ? positiveInk : negativeInk : counter.NormalInk;
        float t = Mathf.Clamp01(counter.PunchAge / punchDuration);
        float spring = t >= 1 || deferred ? 0 : Mathf.Exp(-7 * t) * Mathf.Cos(3 * Mathf.PI * t);
        counter.Text.rectTransform.localScale = counter.NormalScale * ((emphasised ? modifiedScale : 1) * (1 + counter.PunchAmount * spring));
        float tilt = spring * tiltDegrees * (counter.Beat % 2 == 0 ? -1 : 1);
        counter.Text.rectTransform.localRotation = counter.NormalRotation * Quaternion.Euler(0, 0, tilt);
    }

    /// <summary>Clears presentation state when the card is blanked, hidden or reused outside the current encounter.</summary>
    public void ResetPresentation()
    {
        PrepareForRefresh(); card = null; playbackPaused = deferEmphasis = false;
        weight.Initialized = value.Initialized = false;
    }

    /// <summary>Restores authored styling without changing the caller's supplied text.</summary>
    private static void RestoreStyle(Counter counter)
    {
        if (counter?.Text == null) return;
        counter.Text.color = counter.NormalInk; counter.Text.rectTransform.localScale = counter.NormalScale;
        counter.Text.fontStyle = counter.NormalStyle; counter.Text.fontWeight = counter.NormalWeight;
        counter.Text.rectTransform.localRotation = counter.NormalRotation;
    }

    /// <summary>Discards pending counters when the slot leaves presentation.</summary>
    private void OnDisable() { ResetPresentation(); }
}
