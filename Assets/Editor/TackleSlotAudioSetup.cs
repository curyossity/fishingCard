using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Installs recorded mechanical Discover cues, routed voices and a shared sound control without changing reel art.</summary>
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
        var existing = root.GetComponent<TackleSlotAudio>();
        if (existing != null) { DeepwaterAudioSetup.ConfigureDiscover(root); return; }
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
        var stage=root.transform.Find("1920x1080 Stage");
        var toggleRect=Rect(stage,"Sound",new Rect(1585,156,185,40));var toggle=toggleRect.gameObject.AddComponent<UnityEngine.UI.Toggle>();
        var socket=Rect(toggleRect,"Sound socket",new Rect(0,5,30,30)).gameObject.AddComponent<UnityEngine.UI.Image>();socket.color=new Color32(50,62,53,255);
        var mark=Rect(socket.transform,"Enabled",new Rect(6,6,18,18)).gameObject.AddComponent<UnityEngine.UI.Image>();mark.color=new Color32(233,188,99,255);mark.raycastTarget=false;
        var label=Rect(toggleRect,"Caption",new Rect(40,0,140,40)).gameObject.AddComponent<TextMeshProUGUI>();
        label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root+"Fonts/VT323.asset");label.fontSize=27;label.text="SOUND";label.color=new Color32(233,220,177,255);label.raycastTarget=false;
        toggle.targetGraphic=socket;toggle.graphic=mark;toggle.SetIsOnWithoutNotify(true);data.FindProperty("soundToggle").objectReferenceValue=toggle;
        data.ApplyModifiedPropertiesWithoutUndo();var owner=new SerializedObject(presentation);owner.FindProperty("slotAudio").objectReferenceValue=host;owner.ApplyModifiedPropertiesWithoutUndo();
        DeepwaterAudioSetup.ConfigureDiscover(root);
    }
    /// <summary>Places native UI in the existing reference stage without replacing its layout or artwork.</summary>
    private static RectTransform Rect(Transform parent,string name,Rect bounds)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(bounds.x,-bounds.y);rect.sizeDelta=bounds.size;return rect;
    }
}
