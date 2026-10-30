using TMPro;
using UnityEngine;

/// <summary>R051 monochrome tag channel, proportional precision lights and anchor cursor.</summary>
public sealed class JournalTagTimingView : JournalEncounterTimingView
{
    [SerializeField] private RectTransform marker, burst;
    [SerializeField] private RectTransform[] zones, precisionAreas;
    [SerializeField] private TMP_Text[] labels;
    [SerializeField] private TMP_Text phase;
    private float resizeStartedAt = -1;
    protected override RectTransform Marker => marker;
    public RectTransform[] ZoneRects => zones;
    public RectTransform[] PrecisionRects => precisionAreas;
    private static readonly Color Ink = new Color32(41,61,64,255);

    /// <summary>Starts from this occurrence's authoritative widths, preserving run fatigue.</summary>
    protected override void BindOccurrence()
    { resizeStartedAt = -1; }

    /// <summary>Animates only presentation widths; the captured stop remains authoritative.</summary>
    protected override void DrawInstrument()
    {
        var state = (EncounterTagTiming)Timing;
        if (state.Stopped && resizeStartedAt < 0) resizeStartedAt = Time.unscaledTime;
        float t = !state.Stopped ? 0 : state.ResizeSeconds <= 0 ? 1 : Mathf.Clamp01((Time.unscaledTime-resizeStartedAt)/state.ResizeSeconds);
        t = t*t*(3-2*t);
        float start = 0, markerPosition = state.Position;
        for (int i = 0; i < zones.Length; i++)
        {
            var zone = state.Zones[i];
            float share = Mathf.Lerp(zone.End-zone.Start, state.NextShares[i], t);
            zones[i].anchorMin = new Vector2(start,0); zones[i].anchorMax = new Vector2(start+share,1);
            zones[i].offsetMin = zones[i].offsetMax = Vector2.zero;
            float width = 1440*share;
            precisionAreas[i].sizeDelta = new Vector2(width*(zone.PrecisionEnd-zone.PrecisionStart)/(zone.End-zone.Start),60);
            // Every tag keeps its glyph at the five-percent floor. The phase field names narrow spans.
            labels[i].gameObject.SetActive(width >= 112);
            if (state.Stopped && zone.Tag == state.Outcome.Tag)
                markerPosition = start + share*Mathf.InverseLerp(zone.Start,zone.End,state.Position);
            start += share;
        }
        marker.anchoredPosition = new Vector2(80+1440*markerPosition,-88);
        heading.text = state.Stopped ? state.Outcome.Tag.ToUpperInvariant()+(state.PrecisionHit ? " · PRECISION" : " · STOPPED") : "FOLLOW THE LIGHT";
        heading.color = Ink;
        if (state.Stopped) phase.text = state.FixedEncounter ? "PROMISED ENCOUNTER" : state.UsesFallbackPool ? "OPEN WATERS" : "TAG SELECTED";
        else
        {
            phase.text = "CHOOSE A TAG";
            for (int i=0;i<state.Zones.Count;i++) if (state.Position < state.Zones[i].End || i==state.Zones.Count-1)
            { if (!labels[i].gameObject.activeSelf) phase.text=state.Zones[i].Tag.ToUpperInvariant(); break; }
        }
        instruction.text = !state.Stopped ? "Stop on a tag. Aim for its marked centre." : state.FixedEncounter ? "Your promised creature reveals next." : state.UsesFallbackPool ? "No eligible species with this tag. Drawing from these waters." : "The creature reveals next. Then choose Catch or Bait.";
        stopLabel.text = state.Stopped ? "STOPPED" : "STOP";
        stopButton.targetGraphic.color = Ink;
        burst.gameObject.SetActive(state.Stopped);
        if (state.Stopped)
        {
            burst.anchoredPosition = new Vector2(80+1440*markerPosition,-254);
            burst.localScale = Vector3.one*(1+Mathf.Sin(state.FeedbackProgress*Mathf.PI)*.25f);
        }
    }
}
