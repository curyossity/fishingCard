using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks native reel audio against accepted spin/lock state and cancellation without changing results or player saves.</summary>
public static class TackleSlotAudioChecks
{
    public const string Output="Docs/Validation/SlotAudio/";
    private static int checks;
    public static string Result { get; private set; }="Not run";
    /// <summary>Validates saved bindings, 2D mixer routing, cue content and a loop that starts/ends without discontinuities.</summary>
    public static int ValidateAssets()
    {
        checks=0;var root=AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
        var audio=root.GetComponent<TackleSlotAudio>();Check(audio!=null,"Discover sound is not installed.");var data=new SerializedObject(audio);
        Check(new SerializedObject(root.GetComponent<TackleDiscoverPresentation>()).FindProperty("slotAudio").objectReferenceValue==audio,"Presenter does not own audio hooks.");
        foreach(string field in new[]{"start","roll","tick","stop","latch","unlatch","pair","triple","soundToggle","rolling"}) Check(data.FindProperty(field).objectReferenceValue!=null,"Missing slot sound binding: "+field);
        foreach(var source in root.GetComponentsInChildren<AudioSource>(true)) Check(!source.playOnAwake&&source.spatialBlend==0&&source.outputAudioMixerGroup!=null,"Audio voice is unrouted, 3D or plays on load.");
        Check(data.FindProperty("voices").arraySize==6&&((AudioSource)data.FindProperty("rolling").objectReferenceValue).loop,"Audio pool or rolling loop is invalid.");
        foreach(string cue in new[]{"start","roll","tick","stop","latch","unlatch","pair","triple"})
        {
            var clip=(AudioClip)data.FindProperty(cue).objectReferenceValue;var pcm=new float[clip.samples];clip.GetData(pcm,0);
            Check(pcm.Any(v=>Mathf.Abs(v)>.01f)&&pcm.All(v=>Mathf.Abs(v)<=.66f),"Silent/clipped slot cue: "+cue);
            Check(cue=="roll"?Mathf.Abs(pcm[0]-pcm.Last())<.001f:Mathf.Abs(pcm[0])<.001f&&Mathf.Abs(pcm.Last())<.01f,"Cue edges can click: "+cue);
        }
        return checks;
    }
    /// <summary>Starts a fresh real Discover flow with persistence disabled.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Use a fresh comparison Play session.");
        checks=0;Result="Running";Directory.CreateDirectory(Output);
        var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;owner.SetDebugSaveResetPending(true);Application.runInBackground=true;owner.StartCoroutine(Run());
    }
    /// <summary>Verifies one-shot revision behavior, exact stop counts, locked-reel silence, mute and phase cancellation.</summary>
    private static IEnumerator Run()
    {
        bool preference=GameAudioPreferences.SoundEnabled;
        try
        {
            GameAudioPreferences.SetSoundEnabled(true);
            var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
            owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
            ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();
            var view=UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();var audio=view.GetComponent<TackleSlotAudio>();var data=new SerializedObject(audio);
            var reels=view.GetComponentsInChildren<TackleReelAnimator>();var toggle=(UnityEngine.UI.Toggle)data.FindProperty("soundToggle").objectReferenceValue;
            Check(view.IsAnimating&&audio.Starts==1&&audio.IsRolling,"Initial spin did not start audio exactly once.");
            Check(view.GetComponentsInChildren<AudioSource>().Any(s=>s.isPlaying&&!s.loop),"Launch clack did not play.");
            long revision=owner.SlotMachine.Revision;var tags=Enumerable.Range(0,3).Select(owner.SlotMachine.GetTag).ToArray();
            for(int i=0;i<5;i++)view.Present(owner);
            Check(audio.Starts==1&&owner.SlotMachine.Revision==revision,"Refreshing replayed launch or changed the result.");
            yield return Wait(view);
            Check(audio.Stops==3&&!audio.IsRolling,"Initial reels did not stop their audio exactly once.");
            Check(audio.Matches==(tags.Distinct().Count()<3?1:0),"Match cue did not follow the settled result.");
            Check(owner.CurrentEncounter==null&&owner.SlotMachine.Active&&tags.SequenceEqual(Enumerable.Range(0,3).Select(owner.SlotMachine.GetTag)),"Audio changed or accepted the outcome.");
            for(int i=0;i<5;i++)view.Present(owner);
            Check(audio.Stops==3&&audio.Starts==1,"Settled refresh replayed reel sounds.");
            Check(owner.TrySetSlotReelLocked(owner.SlotMachine.Revision,1,true)&&audio.Latches==1,"Lock latch did not follow an accepted command.");
            Check(!owner.TrySetSlotReelLocked(revision,1,false)&&audio.Latches==1,"Rejected lock played a sound.");
            Check(owner.TrySetSlotReelLocked(owner.SlotMachine.Revision,1,false)&&audio.Latches==2,"Unlock did not play its latch.");
            Check(owner.TrySetSlotReelLocked(owner.SlotMachine.Revision,1,true),"Could not lock for respin.");
            float lockedPhase=reels[1].Phase;
            Check(owner.TryRespinSlotMachine(owner.SlotMachine.Revision)&&audio.Starts==2,"Optional respin did not start audio.");
            toggle.isOn=false;
            Check(!audio.IsRolling&&view.GetComponentsInChildren<AudioSource>().All(s=>!s.isPlaying)&&!GameAudioPreferences.SoundEnabled,"Muting did not silence every voice.");
            toggle.isOn=true;yield return null;
            Check(audio.IsRolling,"Unmuting did not resume the current rolling texture.");
            yield return Wait(view);
            Check(audio.Stops==5&&reels[1].Phase==lockedPhase,"Locked reel emitted a stop or moved during respin.");
            Check(!audio.IsRolling&&!owner.TryRespinSlotMachine(owner.SlotMachine.Revision),"Audio retained a loop or allowed another respin.");
            toggle.isOn=false;
            var next=owner.SlotMachine.Revision;Check(owner.TryAcceptSlotMachine(next),"Could not accept final tags.");
            yield return null;
            Check(!view.gameObject.activeInHierarchy&&view.GetComponentsInChildren<AudioSource>(true).All(s=>!s.isPlaying),"Discover sound leaked into Landing.");
            var landing=UnityEngine.Object.FindAnyObjectByType<LandingScoreRush>();
            Check(!((UnityEngine.UI.Toggle)new SerializedObject(landing).FindProperty("soundToggle").objectReferenceValue).isOn,"Landing did not inherit Discover's sound preference.");
            Result="Passed "+checks+" live slot audio checks";File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");
        }
        finally { GameAudioPreferences.SetSoundEnabled(preference); }
    }
    /// <summary>Waits on actual reel completion, retaining a deadline for broken presentation callbacks.</summary>
    private static IEnumerator Wait(SlotMachineView view)
    { double until=Time.realtimeSinceStartupAsDouble+5;while(view.IsAnimating&&Time.realtimeSinceStartupAsDouble<until)yield return null;Check(!view.IsAnimating,"Reel audio fixture timed out."); }
    /// <summary>Reports precise failure state for asynchronous editor verification.</summary>
    private static void Check(bool value,string message){if(!value){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
