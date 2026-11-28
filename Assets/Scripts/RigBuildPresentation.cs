using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Owns tactile Build feedback, pooled change labels and the pending ghost; never commits or modifies a rig.</summary>
public sealed class RigBuildPresentation : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private RectTransform flightLayer;
    [SerializeField] private TMP_Text changeTemplate, receiptChange;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private CanvasGroup[] secondaryLayers;
    // Retained for historical prefab/review consumers; live receipt cues use the shared sound library.
    [SerializeField, HideInInspector] private AudioSource voice;
    [SerializeField, HideInInspector] private AudioClip clipSnap, upgradeSound;
    private TackleRigCardView ghost, flying, receiving;
    private CanvasGroup ghostAlpha, flyingAlpha, receivingAlpha;
    private readonly Dictionary<TackleRigCardView,TMP_Text> labels=new Dictionary<TackleRigCardView,TMP_Text>();
    private readonly Dictionary<RectTransform,Motion> motions=new Dictionary<RectTransform,Motion>();
    private readonly List<RectTransform> motionKeys=new List<RectTransform>();
    private readonly Vector3[] corners=new Vector3[4];
    private IReadOnlyList<UnityEngine.UI.Image> ropes=Array.Empty<UnityEngine.UI.Image>();
    private Vector3 flightStart,flightEnd;
    private Vector3 receiveScale;
    private Vector3 levelRest;
    private RectTransform clipTransform;
    private Vector3 clipRest;
    private RigAcquisitionReceipt receipt;
    private double started;
    private bool landed;
    private struct Motion {public Vector2 From,To;public double Start;}
    public TackleRigCardView Ghost => ghost;
    public bool ReducedMotion => PlayerPrefs.GetInt("Deepwater.Landing.ReducedMotion",0)!=0;

    /// <summary>Creates one translucent, non-intercepting candidate face in the selected rail gap.</summary>
    public TackleRigCardView ShowGhost(TackleRigCardView prefab,RectTransform parent,CardInstance member)
    {
        if(ghost==null)
        {
            ghost=Instantiate(prefab,parent);ghost.name="Pending creature preview";ghostAlpha=Group(ghost.gameObject);
            ghostAlpha.blocksRaycasts=false;ghostAlpha.interactable=false;ghost.InspectionTarget.interactable=false;
        }
        ghost.gameObject.SetActive(true);ghost.Bind(member,true);ghost.SetClipVisible(true);ghost.SetAcquiredHighlight(false);ghostAlpha.alpha=.55f;
        return ghost;
    }
    /// <summary>Releases the preview and any old placement labels when an acquisition receipt is shown.</summary>
    public void HideGhost(){if(ghost!=null)ghost.gameObject.SetActive(false);}
    /// <summary>Sets a new visual destination, preserving existing relative order and interrupting an old ease from its current position.</summary>
    public void Place(RectTransform rect,Vector2 target,bool animate)
    {
        if(!animate||ReducedMotion){motions.Remove(rect);rect.anchoredPosition=target;return;}
        if(motions.TryGetValue(rect,out var previous)&&previous.To==target)return;
        if((rect.anchoredPosition-target).sqrMagnitude<.1f)return;
        motions[rect]=new Motion{From=rect.anchoredPosition,To=target,Start=Time.realtimeSinceStartupAsDouble};
    }
    /// <summary>Displays only actual changed members; full explanations remain inspectable through the shared tooltip.</summary>
    public void ShowChanges(IReadOnlyList<TackleRigCardView> cards,RigBuildChange[] changes)
    {
        foreach(var text in labels.Values)text.gameObject.SetActive(false);
        foreach(var card in cards)
        {
            var change=changes.FirstOrDefault(c=>c.SourceId==card.BoundInstance?.InstanceId);if(change==null)continue;
            Label(card,change.ShortText,change.Direction>0?new Color32(111,232,172,255):change.Direction<0?new Color32(244,138,118,255):new Color32(233,188,99,255));
        }
    }
    /// <summary>Stages an already committed member; authoritative acquisition and continuation never depend on animation progress.</summary>
    public void BeginReceipt(RigAcquisitionReceipt value,TackleRigCardView target,TackleRigCardView prefab,RectTransform from,IReadOnlyList<UnityEngine.UI.Image> railRopes)
    {
        Finish();receipt=value;receiving=target;ropes=railRopes;started=Time.realtimeSinceStartupAsDouble;landed=false;receiveScale=target.transform.localScale;levelRest=levelLabel.rectTransform.localScale;
        clipTransform=target.ClipTransform;clipRest=clipTransform!=null?clipTransform.localScale:Vector3.one;
        var change=RigBuildChange.Compare(value.PreviousMembers,value.Members).FirstOrDefault(c=>c.SourceId==value.Member.InstanceId);
        receiptChange.text=value.PreviousLevel>0?"ABILITY UPGRADED · "+(change?.Description.Replace("\n"," · ")??"Level "+value.PreviousLevel+" → "+value.Member.Level):"CREATURE ATTACHED";
        receiptChange.color=value.PreviousLevel>0?new Color32(111,232,172,255):new Color32(233,188,99,255);
        if(secondaryLayers!=null)foreach(var layer in secondaryLayers)if(layer!=null)layer.alpha=0;
        receiptChange.gameObject.SetActive(true);
        if(value.PreviousLevel>0||ReducedMotion)
        {sound.Play(value.PreviousLevel>0?DeepwaterSoundCue.Upgrade:DeepwaterSoundCue.Attach);landed=true;return;}
        sound.Play(DeepwaterSoundCue.BuildTravel);
        if(flying==null)
        {
            flying=Instantiate(prefab,flightLayer);flying.name="Placement flight";flyingAlpha=Group(flying.gameObject);
            flyingAlpha.blocksRaycasts=false;flyingAlpha.interactable=false;flying.InspectionTarget.interactable=false;
        }
        flying.gameObject.SetActive(true);flying.Bind(value.Member);flying.SetClipVisible(true);flying.SetAcquiredHighlight(false);
        receivingAlpha=Group(target.gameObject);receivingAlpha.alpha=0;
        var rect=(RectTransform)flying.transform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
        rect.sizeDelta=((RectTransform)target.transform).rect.size;
        flightStart=flightLayer.InverseTransformPoint(from.TransformPoint(from.rect.center));
        var targetRect=(RectTransform)target.transform;flightEnd=flightLayer.InverseTransformPoint(targetRect.TransformPoint(targetRect.rect.center));
        rect.localPosition=flightStart;rect.localScale=Vector3.one*.65f;flyingAlpha.alpha=1;
    }
    /// <summary>Samples eased visual motion on an unscaled clock; there are no delayed gameplay commands.</summary>
    private void LateUpdate()
    {
        motionKeys.Clear();motionKeys.AddRange(motions.Keys);
        foreach(var rect in motionKeys)
        {
            var motion=motions[rect];float t=ReducedMotion?1:Mathf.Clamp01((float)(Time.realtimeSinceStartupAsDouble-motion.Start)/.16f);
            rect.anchoredPosition=Vector2.LerpUnclamped(motion.From,motion.To,1-Mathf.Pow(1-t,3));if(t>=1)motions.Remove(rect);
        }
        PositionLabels();
        if(receipt==null||receiving==null)return;
        float age=(float)(Time.realtimeSinceStartupAsDouble-started);
        if(!landed)
        {
            float t=ReducedMotion?1:Mathf.Clamp01(age/.32f);var rect=(RectTransform)flying.transform;
            rect.localPosition=Vector3.LerpUnclamped(flightStart,flightEnd,1-Mathf.Pow(1-t,3));rect.localScale=Vector3.one*Mathf.Lerp(.65f,1,t);
            if(t>=1){landed=true;flying.gameObject.SetActive(false);receivingAlpha.alpha=1;sound.Play(DeepwaterSoundCue.Attach);}
        }
        float impactAge=receipt.PreviousLevel>0?age:age-.32f;
        float pulse=ReducedMotion||impactAge<0?0:Mathf.Exp(-impactAge*12)*Mathf.Sin(impactAge*24);
        receiving.transform.localScale=receiveScale*(1+Mathf.Max(0,pulse)*.055f);
        if(receipt.PreviousLevel>0)levelLabel.rectTransform.localScale=levelRest*(1+Mathf.Max(0,pulse)*.12f);
        if(clipTransform!=null)clipTransform.localScale=clipRest* (1+Mathf.Max(0,pulse)*.16f);
        foreach(var rope in ropes)if(rope!=null&&rope.gameObject.activeSelf){var rect=rope.rectTransform;rect.localRotation=Quaternion.Euler(0,0,ReducedMotion?0:pulse*.8f);}
    }
    /// <summary>Restores every moved target and silences owned audio on interruption or receipt completion.</summary>
    public void Finish()
    {
        foreach(var pair in motions)if(pair.Key!=null)pair.Key.anchoredPosition=pair.Value.To;motions.Clear();
        if(receiving!=null)receiving.transform.localScale=receiveScale;
        if(receipt!=null&&levelLabel!=null)levelLabel.rectTransform.localScale=levelRest;
        if(receivingAlpha!=null)receivingAlpha.alpha=1;
        if(clipTransform!=null)clipTransform.localScale=clipRest;
        foreach(var rope in ropes)if(rope!=null)rope.rectTransform.localRotation=Quaternion.identity;
        if(flying!=null)flying.gameObject.SetActive(false);receipt=null;receiving=null;receivingAlpha=null;clipTransform=null;
        if(receiptChange!=null)receiptChange.gameObject.SetActive(false);if(sound!=null)sound.StopAll();
        if(secondaryLayers!=null)foreach(var layer in secondaryLayers)if(layer!=null)layer.alpha=.4f;
    }
    /// <summary>Creates a reusable outside-face native label; face artwork and live card fields are untouched.</summary>
    private void Label(TackleRigCardView card,string text,Color tint)
    {
        if(!labels.TryGetValue(card,out var label)){label=Instantiate(changeTemplate,flightLayer);labels.Add(card,label);}
        label.text=text;label.color=tint;label.gameObject.SetActive(true);
    }
    /// <summary>Keeps consequence captions below visible faces and outside the rope/clip region and scroll masks.</summary>
    private void PositionLabels()
    {
        foreach(var pair in labels)
        {
            var label=pair.Value;if(!label.gameObject.activeSelf)continue;
            var card=pair.Key;var face=((TackleCardTooltipSource)card.InspectionTarget).FaceBounds;
            bool visible=card.gameObject.activeInHierarchy;var viewport=card.GetComponentInParent<UnityEngine.UI.ScrollRect>()?.viewport;
            if(visible&&viewport!=null){var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(viewport,face);var clip=viewport.rect;visible=bounds.min.x>=clip.xMin-1&&bounds.max.x<=clip.xMax+1&&bounds.min.y>=clip.yMin-1&&bounds.max.y<=clip.yMax+1;}
            label.enabled=visible;if(!visible)continue;
            face.GetWorldCorners(corners);Vector2 min=flightLayer.InverseTransformPoint(corners[0]),max=flightLayer.InverseTransformPoint(corners[2]);
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.pivot=new Vector2(0,1);
            label.rectTransform.anchoredPosition=new Vector2(min.x,min.y-7);label.rectTransform.sizeDelta=new Vector2(max.x-min.x,21);
        }
    }
    /// <summary>Acknowledges a deliberate new legal preview; repeated layout and hover refreshes remain silent.</summary>
    public void PreviewChanged(){if(sound!=null)sound.Play(DeepwaterSoundCue.Preview);}
    /// <summary>One dry card-handling tick of the haul count-up, rising in pitch as the value climbs.</summary>
    public void HaulTick(float pitch){if(sound!=null)sound.Play(DeepwaterSoundCue.Preview,pitch,-3);}
    /// <summary>Gets or creates a native presentation group without adding an input surface.</summary>
    private static CanvasGroup Group(GameObject root){var group=root.GetComponent<CanvasGroup>();return group!=null?group:root.AddComponent<CanvasGroup>();}
    /// <summary>Keeps the previous strike inspectable while giving placement consequences visual priority.</summary>
    private void OnEnable(){if(secondaryLayers!=null)foreach(var group in secondaryLayers)if(group!=null)group.alpha=.4f;}
    /// <summary>Phase exit cannot retain a half-hidden receiving card or a floating preview.</summary>
    private void OnDisable(){Finish();HideGhost();if(secondaryLayers!=null)foreach(var group in secondaryLayers)if(group!=null)group.alpha=1;}
}
