using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Checks visible overshoot/recoil on the real reel prefab without selecting creatures or changing saves.</summary>
public static class SlotReelStopChecks
{
    /// <summary>Samples the real renderer at controlled times, verifies a bounded spring stop, and captures its three key poses.</summary>
    public static string ValidateAndCapture()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode before checking the reel renderer.");
        int checks = 0;
        Action<bool, string> check = (condition, message) => { if (!condition) throw new InvalidOperationException(message); checks++; };
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath));
        try
        {
            root.SetActive(true);
            var art = root.GetComponent<TackleDiscoverPresentation>(); art.enabled = false;
            var view = root.GetComponent<SlotMachineView>(); view.enabled = false;
            var artData = new SerializedObject(art);
            var sprites = new Dictionary<string, Sprite>();
            var mappings = artData.FindProperty("tagArtwork");
            for (int i = 0; i < mappings.arraySize; i++)
                sprites.Add(mappings.GetArrayElementAtIndex(i).FindPropertyRelative("tag").stringValue,
                    (Sprite)mappings.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue);
            var tags = new[] { "Fish", "Predator", "Schooling", "Small" };
            var reels = root.GetComponentsInChildren<TackleReelAnimator>();
            var reel = reels[0];
            var tuning = new SerializedObject(reel);
            float settle = tuning.FindProperty("settleFraction").floatValue;
            float period = tuning.FindProperty("elasticPeriod").floatValue;
            const float seconds = 1.05f;
            float peakTime = seconds * (1 - settle);
            float recoilTime = peakTime + seconds * settle * period * .5f;
            Action<TackleReelAnimator> spin = r => r.Spin(tags, "Fish", tag => sprites[tag], seconds);
            Func<TackleReelAnimator, float> degrees = r => Mathf.DeltaAngle(0, r.Phase * Mathf.Rad2Deg);
            spin(reel); reel.Advance(peakTime);
            float peak = degrees(reel);
            check(peak > 25 && peak < 65 && reel.IsSpinning, "Stop has no clear bounded forward overshoot.");
            reel.Advance(recoilTime - peakTime);
            check(degrees(reel) < -1 && degrees(reel) > -peak * .5f && reel.IsSpinning, "Stop did not recoil with a smaller undershoot.");
            var surface = reel.GetComponentInChildren<TackleReelSurface>();
            check(surface.Phase == reel.Phase && surface.MaterialInstance.GetFloat("_Phase") == reel.Phase,
                "Paper/dividers/plaques did not follow the spring motion.");
            reel.Advance(seconds - recoilTime + .001f);
            check(!reel.IsSpinning && Mathf.Abs(degrees(reel)) < .001f, "Reel did not finish exactly at its selected face.");
            float stopped = reel.Phase; reel.Advance(1);
            check(reel.Phase == stopped, "Stopped reel continued drifting.");

            // Test the rendered motion rather than duplicating its easing equation; small/large steps must agree.
            float reference = 0;
            foreach (int rate in new[] { 30, 60, 144 })
            {
                spin(reel);
                float time = 0;
                const float sample = .82f;
                while (time < sample)
                {
                    float delta = Mathf.Min(1f / rate, sample - time);
                    reel.Advance(delta); time += delta;
                    check(Mathf.Abs(degrees(reel)) <= 180 && !float.IsNaN(reel.Phase), "Non-finite reel motion.");
                }
                if (rate == 30) reference = degrees(reel);
                else check(Mathf.Abs(degrees(reel) - reference) < .01f, "Spring response depends on frame rate.");
                reel.Advance(10);
                check(!reel.IsSpinning && Mathf.Abs(degrees(reel)) < .001f, "Large frame step failed to settle.");
            }

            // All three wheels show one selected tag at different stages of its stop, as an explicit visual fixture.
            for (int i = 0; i < reels.Length; i++) { spin(reels[i]); reels[i].Advance(i == 0 ? peakTime : i == 1 ? recoilTime : seconds + .001f); }
            var viewData = new SerializedObject(view);
            ((TMPro.TMP_Text)viewData.FindProperty("resultDescription").objectReferenceValue).text = "STOP PREVIEW / OVERSHOOT - RECOIL - REST";
            ((TMPro.TMP_Text)artData.FindProperty("biomeLabel").objectReferenceValue).text = "SHALLOWS / MOTION PREVIEW";
            ((TMPro.TMP_Text)artData.FindProperty("haulLabel").objectReferenceValue).text = "HAUL 0";
            ((UnityEngine.UI.Button)viewData.FindProperty("acceptButton").objectReferenceValue).interactable = false;
            ((UnityEngine.UI.Button)viewData.FindProperty("respinButton").objectReferenceValue).interactable = false;
            SlotMachineSceneChecks.Capture(root.GetComponent<Canvas>(), "Overshoot-Recoil-Rest.png", output: "Docs/Validation/ElasticReelStop/");
            float peakDegrees = degrees(reels[0]), recoilDegrees = degrees(reels[1]);
            return checks + " stop checks passed; peak " + peakDegrees.ToString("F1") + " degrees; recoil " + recoilDegrees.ToString("F1") + " degrees.";
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}
