using System;
using TMPro;
using UnityEngine;

/// <summary>Displays R046 timing state; the controller owns motion, outcomes, selection and reveal.</summary>
public sealed class JournalRarityTimingView : EncounterRarityTimingView
{
    [SerializeField] private CanvasGroup visibility;
    [SerializeField] private TMP_Text heading, subtitle, instruction, stopLabel;
    [SerializeField] private UnityEngine.UI.Button stopButton;
    [SerializeField] private RectTransform marker, burst, wash;
    [SerializeField] private RectTransform[] zones, legends;
    private EncounterRarityTiming timing;
    private Func<long,bool> stop;
    private FishingRunView owner;
    private long occurrence = -1;
    private bool lastStopped;
    private bool focusPending;
    public UnityEngine.UI.Button StopButton => stopButton;
    public static readonly Color[] Colours = { new Color32(96,132,127,255),new Color32(70,124,152,255),new Color32(202,162,79,255),new Color32(147,97,126,255),new Color32(213,201,172,255) };
    private static readonly Color Ink=new Color32(48,68,72,255);
    private static readonly Color[] ResultColours={new Color32(53,89,80,255),new Color32(45,93,116,255),new Color32(132,85,21,255),new Color32(105,65,95,255),Ink};

    /// <summary>Rebinds a new occurrence and rejects callbacks captured for previous timing attempts.</summary>
    public override void Present(EncounterRarityTiming state,Func<long,bool> action,BiomeDefinition biome,int depth)
    {
        timing=state;stop=action;
        if(owner==null)owner=GetComponentInParent<FishingRunView>();
        bool active=state!=null&&state.Active;
        gameObject.SetActive(active);if(!active)return;
        subtitle.text=(biome==null?"Uncharted Waters":biome.DisplayName)+" · "+depth+" m";
        if(occurrence!=state.Occurrence)
        {
            occurrence=state.Occurrence;lastStopped=false;focusPending=true;
            long token=occurrence;stopButton.onClick.RemoveAllListeners();stopButton.onClick.AddListener(()=>stop?.Invoke(token));
            for(int i=0;i<zones.Length;i++)
            {
                bool used=i<state.Zones.Count;zones[i].gameObject.SetActive(used);legends[i].gameObject.SetActive(used);if(!used)continue;
                var zone=state.Zones[i];int index=zone.IsMiss?4:(int)zone.Rarity;
                zones[i].anchorMin=new Vector2(zone.Start,0);zones[i].anchorMax=new Vector2(zone.End,1);zones[i].offsetMin=zones[i].offsetMax=Vector2.zero;
                zones[i].GetComponent<UnityEngine.UI.Image>().color=Colours[index];
                var pattern=zones[i].GetComponentInChildren<JournalRarityGraphic>();pattern.drawing=(JournalRarityGraphic.Drawing)index;pattern.SetVerticesDirty();
                var legend=legends[i];legend.anchoredPosition=new Vector2(56+i*1048f/state.Zones.Count,-194);legend.sizeDelta=new Vector2(1048f/state.Zones.Count,36);
                var icon=legend.GetComponentInChildren<JournalRarityGraphic>();icon.drawing=(JournalRarityGraphic.Drawing)index;icon.color=Colours[index];icon.SetVerticesDirty();
                legend.GetComponentInChildren<TMP_Text>().text=zone.Label;
            }
        }
        Render();
    }

    /// <summary>Tracks authoritative motion and separate presentation feedback without advancing gameplay.</summary>
    private void Update() { Render(); }

    /// <summary>Shows only the rarity result while the delayed creature remains unrevealed.</summary>
    private void Render()
    {
        if(timing==null||!timing.Active)return;
        bool ready=owner==null||!owner.IsEncounterTransitionPlaying;
        visibility.alpha=ready?1:0;visibility.interactable=ready;visibility.blocksRaycasts=ready;
        marker.anchoredPosition=new Vector2(56+1048*timing.Position,-79);
        stopButton.interactable=ready&&!timing.Stopped;
        if(ready&&focusPending&&!timing.Stopped){focusPending=false;stopButton.Select();}
        if(timing.Stopped&&!lastStopped)
        {
            lastStopped=true;heading.text=(timing.Outcome.IsMiss?"MISS":"UP TO "+timing.Outcome.Label.ToUpperInvariant())+" — STOPPED";
            heading.color=ResultColours[timing.Outcome.IsMiss?4:(int)timing.Outcome.Rarity];
            stopButton.targetGraphic.color=new Color32(109,96,72,255);
            instruction.text=timing.Outcome.IsMiss?"Open water. No encounter, no penalty.":"The creature reveals next. Then choose Catch or Bait.";
            stopLabel.text="STOPPED";
            var zone=timing.Outcome;wash.anchoredPosition=new Vector2(56+1048*zone.Start,-108);wash.sizeDelta=new Vector2(1048*(zone.End-zone.Start),62);
        }
        else if(!timing.Stopped)
        {heading.text="ENCOUNTER RARITY";heading.color=Ink;stopButton.targetGraphic.color=Ink;instruction.text="Stop the marker to set the maximum creature rarity.";stopLabel.text="STOP";}
        wash.gameObject.SetActive(timing.Stopped);burst.gameObject.SetActive(timing.Stopped);
        if(timing.Stopped)
        {burst.anchoredPosition=new Vector2(56+1048*timing.Position,-101);float pulse=1+Mathf.Sin(timing.FeedbackProgress*Mathf.PI)*.25f;burst.localScale=Vector3.one*pulse;marker.localScale=Vector3.one*(1+Mathf.Sin(timing.FeedbackProgress*Mathf.PI)*.08f);}
        else marker.localScale=Vector3.one;
    }
}
