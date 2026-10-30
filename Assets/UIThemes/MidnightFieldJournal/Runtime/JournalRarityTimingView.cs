using TMPro;
using UnityEngine;

/// <summary>Legacy rarity geometry sharing the tag instrument's input and elastic travel.</summary>
public sealed class JournalRarityTimingView : JournalEncounterTimingView
{
    [SerializeField] private RectTransform marker, burst, wash;
    [SerializeField] private RectTransform[] zones, legends;
    protected override RectTransform Marker => marker;
    public static readonly Color[] Colours = { new Color32(96,132,127,255),new Color32(70,124,152,255),new Color32(202,162,79,255),new Color32(147,97,126,255),new Color32(213,201,172,255) };
    private static readonly Color Ink = new Color32(48,68,72,255);
    private static readonly Color[] ResultColours = { new Color32(53,89,80,255),new Color32(45,93,116,255),new Color32(132,85,21,255),new Color32(105,65,95,255),Ink };

    /// <summary>Binds the legacy profile's independent intervals and labels.</summary>
    protected override void BindOccurrence()
    {
        var state = (EncounterRarityTiming)Timing;
        for (int i = 0; i < zones.Length; i++)
        {
            bool used = i < state.Zones.Count;
            zones[i].gameObject.SetActive(used); legends[i].gameObject.SetActive(used);
            if (!used) continue;
            var zone = state.Zones[i]; int index = zone.IsMiss ? 4 : (int)zone.Rarity;
            zones[i].anchorMin = new Vector2(zone.Start,0); zones[i].anchorMax = new Vector2(zone.End,1);
            zones[i].offsetMin = zones[i].offsetMax = Vector2.zero;
            zones[i].GetComponent<UnityEngine.UI.Image>().color = Colours[index];
            var pattern = zones[i].GetComponentInChildren<JournalRarityGraphic>();
            pattern.drawing = (JournalRarityGraphic.Drawing)index; pattern.SetVerticesDirty();
            var legend = legends[i]; legend.anchoredPosition = new Vector2(56+i*1048f/state.Zones.Count,-194);
            legend.sizeDelta = new Vector2(1048f/state.Zones.Count,36);
            var icon = legend.GetComponentInChildren<JournalRarityGraphic>();
            icon.drawing = (JournalRarityGraphic.Drawing)index; icon.color = Colours[index]; icon.SetVerticesDirty();
            legend.GetComponentInChildren<TMP_Text>().text = zone.Label;
        }
    }

    /// <summary>Shows the original stopped pulse while shared presentation gates reveal.</summary>
    protected override void DrawInstrument()
    {
        var state = (EncounterRarityTiming)Timing;
        marker.anchoredPosition = new Vector2(56+1048*state.Position,-79);
        heading.text = state.Stopped ? (state.Outcome.IsMiss ? "MISS" : "UP TO "+state.Outcome.Label.ToUpperInvariant())+" — STOPPED" : "ENCOUNTER RARITY";
        heading.color = state.Stopped ? ResultColours[state.Outcome.IsMiss ? 4 : (int)state.Outcome.Rarity] : Ink;
        stopButton.targetGraphic.color = state.Stopped ? new Color32(109,96,72,255) : Ink;
        instruction.text = state.Stopped ? (state.Outcome.IsMiss ? "Open water. No encounter, no penalty." : "The creature reveals next. Then choose Catch or Bait.") : "Stop the marker to set the maximum creature rarity.";
        stopLabel.text = state.Stopped ? "STOPPED" : "STOP";
        wash.gameObject.SetActive(state.Stopped); burst.gameObject.SetActive(state.Stopped);
        if (!state.Stopped) return;
        var zone = state.Outcome;
        wash.anchoredPosition = new Vector2(56+1048*zone.Start,-108); wash.sizeDelta = new Vector2(1048*(zone.End-zone.Start),62);
        burst.anchoredPosition = new Vector2(56+1048*state.Position,-101);
        burst.localScale = Vector3.one*(1+Mathf.Sin(state.FeedbackProgress*Mathf.PI)*.25f);
    }
}
