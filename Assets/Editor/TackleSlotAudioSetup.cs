using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Installs original mechanical Discover cues, routed voices and a shared sound control without changing reel art.</summary>
public static class TackleSlotAudioSetup
{
    public const string AudioPath = TackleDiscoverArtSetup.Root + "Audio/Discover/";
    /// <summary>Updates the existing Discover prefab in place.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Sound")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play mode.");
        var root = PrefabUtility.LoadPrefabContents(TackleDiscoverArtSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root,TackleDiscoverArtSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }
    /// <summary>Retains sound bindings during normal Discover regeneration and reel-style changes.</summary>
    public static void Configure(GameObject root)
    {
        var presentation = root.GetComponent<TackleDiscoverPresentation>(); if (presentation == null) return;
        if (root.GetComponent<TackleSlotAudio>() != null) return;
        Directory.CreateDirectory(AudioPath);
        var host = root.AddComponent<TackleSlotAudio>(); var data = new SerializedObject(host);
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(LandingScoreRushSetup.MixerPath);
        if (mixer == null) throw new InvalidOperationException("The shared feedback mixer must be installed before Discover sound.");
        var bus = mixer.FindMatchingGroups("Master").First();
        var voices = data.FindProperty("voices"); voices.arraySize = 6;
        for (int i = 0; i < 7; i++)
        {
            var child = new GameObject(i == 6 ? "Reel rolling voice" : "Slot sound voice " + i); child.transform.SetParent(root.transform,false);
            var source = child.AddComponent<AudioSource>(); source.playOnAwake=false;source.spatialBlend=0;source.outputAudioMixerGroup=bus;source.loop=i==6;
            if(i==6) data.FindProperty("rolling").objectReferenceValue=source;else voices.GetArrayElementAtIndex(i).objectReferenceValue=source;
        }
        foreach(string cue in new[]{"start","roll","tick","stop","latch","unlatch","pair","triple"})
        {
            string file=AudioPath+cue+".wav";if(!File.Exists(file)) Write(file,cue);
            AssetDatabase.ImportAsset(file,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(file);var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
            data.FindProperty(cue).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(file);
        }
        var stage=root.transform.Find("1920x1080 Stage");
        var toggleRect=Rect(stage,"Sound",new Rect(1585,156,185,40));var toggle=toggleRect.gameObject.AddComponent<UnityEngine.UI.Toggle>();
        var socket=Rect(toggleRect,"Sound socket",new Rect(0,5,30,30)).gameObject.AddComponent<UnityEngine.UI.Image>();socket.color=new Color32(50,62,53,255);
        var mark=Rect(socket.transform,"Enabled",new Rect(6,6,18,18)).gameObject.AddComponent<UnityEngine.UI.Image>();mark.color=new Color32(233,188,99,255);mark.raycastTarget=false;
        var label=Rect(toggleRect,"Caption",new Rect(40,0,140,40)).gameObject.AddComponent<TextMeshProUGUI>();
        label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root+"Fonts/VT323.asset");label.fontSize=27;label.text="SOUND";label.color=new Color32(233,220,177,255);label.raycastTarget=false;
        toggle.targetGraphic=socket;toggle.graphic=mark;toggle.SetIsOnWithoutNotify(true);data.FindProperty("soundToggle").objectReferenceValue=toggle;
        data.ApplyModifiedPropertiesWithoutUndo();var owner=new SerializedObject(presentation);owner.FindProperty("slotAudio").objectReferenceValue=host;owner.ApplyModifiedPropertiesWithoutUndo();
    }
    /// <summary>Places native UI in the existing reference stage without replacing its layout or artwork.</summary>
    private static RectTransform Rect(Transform parent,string name,Rect bounds)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(bounds.x,-bounds.y);rect.sizeDelta=bounds.size;return rect;
    }
    /// <summary>Authors click-free PCM wood contacts, a friction loop and modest tonal match accents with deterministic noise.</summary>
    private static void Write(string file,string cue)
    {
        const int rate=44100;double length=cue=="roll"?.256:cue=="triple"?.55:cue=="pair"?.32:.14;
        var samples=new double[(int)(length*rate)];uint state=9341;double peak=.001,friction=0;
        for(int i=0;i<samples.Length;i++)
        {
            double t=(double)i/rate;state=unchecked(state*1664525+1013904223);double noise=state/(double)uint.MaxValue*2-1;
            friction=friction*.91+noise*.09;double value;
            if(cue=="roll") value=friction*.6+Resonate(t%.032,340,.006)*.13;
            else if(cue=="pair"||cue=="triple")
            {
                value=Resonate(t,220,.045)*.6+Resonate(t-.04,440,.08)*.32+Resonate(t-.08,660,.09)*.2;
                if(cue=="triple")value+=Resonate(t-.12,880,.14)*.3+Resonate(t-.17,1100,.11)*.1;
            }
            else
            {
                double fundamental=cue=="tick"?740:cue=="latch"?300:cue=="unlatch"?240:cue=="stop"?175:115;
                value=Resonate(t,fundamental,cue=="tick"?.013:.032)+Resonate(t,fundamental*2.34,.016)*.3+noise*Math.Exp(-t*190)*.25;
            }
            value*=Math.Min(1,t/.002)*Math.Min(1,(length-t)/.005);samples[i]=value;peak=Math.Max(peak,Math.Abs(value));
        }
        using(var writer=new BinaryWriter(File.Create(file)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);
            foreach(double sample in samples)writer.Write((short)(sample/peak*.65*short.MaxValue));
        }
    }
    /// <summary>Returns a damped resonance after its mechanical contact time.</summary>
    private static double Resonate(double t,double frequency,double decay)=>t<0?0:Math.Sin(t*frequency*Math.PI*2)*Math.Exp(-t/decay);
}
