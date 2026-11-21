using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Exports three native Unity scoring sequences and their exact scheduled cues for browser review.</summary>
public static class LandingScoreShowcase
{
    public const string Output = "Docs/Validation/LandingScoreDirection/";
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    /// <summary>Samples disposable rigs through the real model and renderer without changing saves or creature authoring.</summary>
    public static string Export()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh Play session before exporting.");
        Directory.CreateDirectory(Output);
        var clips = new List<string>();
        foreach (int level in new[] { 0, 7, 30 })
        {
            var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath));
            try { clips.Add(Capture(root, level)); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        File.WriteAllText(Output + "clips.js", "const clips=[" + string.Join(",", clips) + "];\n");
        return "Exported three scoring sequences to " + Output;
    }
    /// <summary>Captures fixed-clock frames from actual native views with scoring performed once by Landing.</summary>
    private static string Capture(GameObject root, int level)
    {
        var rig = new RigRuntime();
        if (level > 0)
        {
            foreach (string name in new[] { "Sardine", "Glass Minnow", "Mackerel", "Cuttlefish", "Stormjaw Tuna" })
            {
                var card = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
                for (int i = 0; i < (name == "Sardine" || name == "Cuttlefish" || name == "Stormjaw Tuna" ? level : 1); i++)
                    if (!rig.TryAcquire(rig.Revision, card, rig.Members.Length, out _, out var error)) throw new InvalidOperationException(error);
            }
        }
        var model = new LandingChallengeRuntime(); var rules = rig.SnapshotForLanding(out int extra, out int protect); double clock = 0;
        model.Begin(1, 2000, LandingChallengeChecks.Settings(), rules, new System.Random(12), clock, extra, protect);
        LandingChallengeChecks.Strike(model, LandingAccuracy.Perfect, ref clock);
        var receipt = new LandingStrikeReceipt(model, rig.Members);
        var view = root.GetComponent<TackleLandingPresentation>(); view.enabled = false;
        var viewData = new SerializedObject(view);
        foreach (var pair in new Dictionary<string,string> { {"phase","SHALLOWS / LAND"}, {"creature","COD"}, {"tags","PREDATOR"}, {"haul","HAUL " + rig.HaulValue}, {"attempts", "3 ATTEMPTS LEFT"}, {"movement","STEADY"}, {"resistance","2000"}, {"message","RIG ACTIVATING · SPACE / FINISH TO COMPLETE"} })
            ((TMP_Text)viewData.FindProperty(pair.Key).objectReferenceValue).text = pair.Value;
        ((UnityEngine.UI.Image)viewData.FindProperty("portrait").objectReferenceValue).sprite = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset").Artwork;
        typeof(TackleLandingPresentation).GetMethod("PrepareLamps",Hidden).Invoke(view,new object[]{3});
        foreach (var lamp in (List<UnityEngine.UI.Image>)typeof(TackleLandingPresentation).GetField("lamps",Hidden).GetValue(view))
            lamp.sprite = (Sprite)viewData.FindProperty("lampOn").objectReferenceValue;
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.text=="STRIKE")) text.text="FINISH";
        typeof(TackleLandingPresentation).GetMethod("RefreshRig",Hidden).Invoke(view,new object[]{rig.Members,model.LastScore,false});
        var cards = root.GetComponentsInChildren<TackleRigCardView>();
        var feedback = root.GetComponent<LandingStrikeFeedback>(); feedback.Present(receipt,cards,true,true);
        var rush = root.GetComponent<LandingScoreRush>(); var data = new SerializedObject(rush);
        ((UnityEngine.UI.Toggle)data.FindProperty("reducedMotionToggle").objectReferenceValue).SetIsOnWithoutNotify(false);
        ((UnityEngine.UI.Toggle)data.FindProperty("soundToggle").objectReferenceValue).SetIsOnWithoutNotify(false);
        rush.Present(receipt,cards,true);
        var sequence = rush.Sequence; var events = rush.SoundEvents.ToArray();
        var images = new List<string>(); var canvas = root.GetComponent<Canvas>();
        var cameraObject = new GameObject("Scoring review camera",typeof(Camera)); var camera = cameraObject.GetComponent<Camera>();
        camera.enabled=false; camera.orthographic=true; camera.orthographicSize=1080; camera.transform.position=new Vector3(0,0,-1000);
        var target = new RenderTexture(960,540,24); var texture = new Texture2D(960,540,TextureFormat.RGB24,false);
        var previous = RenderTexture.active;
        try
        {
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=100;camera.targetTexture=target;
            int frames = Mathf.CeilToInt((sequence.Duration + .20f)*24);
            for (int frame=0;frame<frames;frame++)
            {
                float time = frame/24f;
                typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{Mathf.Min(time,sequence.Duration)});
                ((TMP_Text)viewData.FindProperty("points").objectReferenceValue).text=LandingScoreRush.Format(rush.DisplayedTotal);
                var progress=(RectTransform)viewData.FindProperty("progressFill").objectReferenceValue;
                progress.anchorMax=new Vector2((float)(rush.DisplayedTotal/2000),1);
                Canvas.ForceUpdateCanvases();
                if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline!=null)
                    UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                else camera.Render();
                RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,960,540),0,0);texture.Apply();
                images.Add("'data:image/jpeg;base64,"+Convert.ToBase64String(texture.EncodeToJPG(85))+"'");
                if(frame==Mathf.RoundToInt(sequence.Steps.Last().ImpactAt*24)) File.WriteAllBytes(Output+"Tier-"+sequence.Tier+".png",texture.EncodeToPNG());
            }
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;target.Release();
            UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(cameraObject);
        }
        byte[] sound=Mix(events,sequence.Duration+.20f);
        return "{name:'"+new[]{"Ordinary","Strong combo","Exceptional combo"}[sequence.Tier]+"',score:'"+model.LastScore.Points.ToString(System.Globalization.CultureInfo.InvariantCulture)+"',frames:["+string.Join(",",images)+"],audio:'data:audio/wav;base64,"+Convert.ToBase64String(sound)+"'}";
    }
    /// <summary>Mixes the runtime event list at its authored pitches and gain; output is a staged review, not a loopback recording.</summary>
    public static byte[] Mix(LandingScoreRush.SoundEvent[] events,float duration)
    {
        const int rate=48000;var mix=new float[Mathf.CeilToInt((duration+.5f)*rate)];
        foreach(var cue in events)
        {
            var samples=new float[cue.Clip.samples];cue.Clip.GetData(samples,0);
            float stride=cue.Pitch*cue.Clip.frequency/rate;
            for(int i=0;i<samples.Length/stride;i++)
            {
                int destination=(int)(cue.Time*rate)+i;if(destination>=mix.Length)break;
                float position=i*stride;int a=(int)position;
                mix[destination]+=Mathf.Lerp(samples[a],samples[Math.Min(a+1,samples.Length-1)],position-a)*cue.Gain;
            }
        }
        using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+mix.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(mix.Length*2);
            foreach(float sample in mix)writer.Write((short)(Mathf.Clamp(sample,-.95f,.95f)*short.MaxValue));return stream.ToArray();
        }
    }
}
