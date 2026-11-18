using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks rendered drum-band motion independently of the paper and confirms the socket outline stays fixed.</summary>
public static class ReelEdgeMotionChecks
{
    /// <summary>Captures the real locked-respin trajectory at fixed visual time steps for a reproducible motion preview.</summary>
    public static int CaptureRespinFrames(string output)
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        var controller = harbor.Controller;
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        if (view == null || view.IsAnimating || !controller.TrySetSlotReelLocked(controller.SlotMachine.Revision,1,true)
            || !controller.TryRespinSlotMachine(controller.SlotMachine.Revision))
            throw new InvalidOperationException("A settled first spin is required for the motion preview.");
        Directory.CreateDirectory(output);
        var reels = view.GetComponentsInChildren<TackleReelAnimator>();
        for (int frame = 0; frame < 32; frame++)
        {
            foreach (var reel in reels) reel.Advance(.05f);
            view.Present(controller);
            Capture(view,"Frame-"+frame.ToString("D2")+".png",output);
        }
        return 32;
    }

    /// <summary>Compares two actual rendered angles at edge-only pixels, then restores the authoritative presentation phase.</summary>
    public static int ValidateLive(string output = "Docs/Validation/DiscoverIntegrated/FullSize/RotatingEdges/")
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Run Discover first.");
        int checks = 0;
        Action<bool,string> check = (condition,message) => { if (!condition) throw new InvalidOperationException(message); checks++; };
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        check(view != null && !view.IsAnimating,"Discover must be visible and settled.");
        var reels = view.GetComponentsInChildren<TackleReelAnimator>();
        var surfaces = reels.Select(reel => reel.GetComponentInChildren<TackleReelSurface>()).ToArray();
        var phases = reels.Select(reel => reel.Phase).ToArray();
        var radii = reels.Select(reel => new SerializedObject(reel).FindProperty("radius").floatValue).ToArray();
        Texture2D before = null,after = null,source = null;
        var changes = new System.Collections.Generic.List<object>();
        try
        {
            for (int i = 0; i < reels.Length; i++) surfaces[i].ApplyRotation(phases[i]+.19f,radii[i]);
            Capture(view,"Edges-A.png",output); before = Read(output+"Edges-A.png");
            for (int i = 0; i < reels.Length; i++) surfaces[i].ApplyRotation(phases[i]+.91f,radii[i]);
            Capture(view,"Edges-B.png",output); after = Read(output+"Edges-B.png");
            source = Read(TackleDiscoverArtSetup.Root+"Art/Discover/reel-drum.png");
            for (int i = 0; i < reels.Length; i++)
            {
                var rect = (RectTransform)reels[i].transform;
                var edge = reels[i].transform.Find("Reel Edge Bands").GetComponent<UnityEngine.UI.RawImage>();
                check(surfaces[i].EdgeMaterialInstance != null && edge.materialForRendering == surfaces[i].EdgeMaterialInstance,
                    "The outer drum has no private animated material.");
                check(Mathf.Abs(edge.materialForRendering.GetFloat("_Phase")-(phases[i]+.91f)) < .0001f,
                    "The outer drum did not receive the paper's phase.");
                int left = 0,right = 0;
                float leftPeak = 0,rightPeak = 0;
                // Every sample lies outside the curved paper aperture, including its widest center.
                for (float y = .20f; y < .80f; y += .01f)
                    foreach (float x in new[] {.045f,.052f,.060f,.940f,.950f,.960f})
                    {
                        var pixel = Pixel(rect,new Vector2(x,y));
                        float difference = Difference(before.GetPixel(pixel.x,pixel.y),after.GetPixel(pixel.x,pixel.y));
                        if (x < .5f) leftPeak = Mathf.Max(leftPeak,difference); else rightPeak = Mathf.Max(rightPeak,difference);
                        if (difference > .015f)
                        { if (x < .5f) left++; else right++; }
                    }
                // Material travel must cover the visible band rather than only changing a few seam pixels.
                check(left > 20 && right > 20 && leftPeak > .08f && rightPeak > .08f,
                    "The drum's visible bands stayed frozen or unreadable: "+i+" ("+left+", "+right+").");
                foreach (float y in new[] {.3f,.5f,.7f})
                {
                    int row = Mathf.Clamp(Mathf.FloorToInt((1-y)*source.height),0,source.height-1);
                    int first = 0,last = source.width-1;
                    // The supplied rim has a translucent fringe; its first opaque texel can lie inside the paper.
                    while (first < source.width-1 && source.GetPixel(first,row).a < .02f) first++;
                    while (last > 0 && source.GetPixel(last,row).a < .02f) last--;
                    foreach (int x in new[] {first,last})
                    {
                        var pixel = Pixel(rect,new Vector2((x+.5f)/source.width,y));
                        check(Difference(before.GetPixel(pixel.x,pixel.y),after.GetPixel(pixel.x,pixel.y)) < .005f,
                            "The fixed socket lip moved with the drum: reel "+i+", row "+y+", source X "+x+".");
                    }
                }
                changes.Add(new {reel=i,leftChangedSamples=left,rightChangedSamples=right,leftPeak=leftPeak,rightPeak=rightPeak});
            }
            Directory.CreateDirectory(output);
            File.WriteAllText(output+"EdgeMotionVerification.json",Newtonsoft.Json.JsonConvert.SerializeObject(new {checks=checks,changes=changes},Newtonsoft.Json.Formatting.Indented));
            return checks;
        }
        finally
        {
            for (int i = 0; i < surfaces.Length; i++) surfaces[i].ApplyRotation(phases[i],radii[i]);
            foreach (var image in new[] {before,after,source}) if (image != null) UnityEngine.Object.DestroyImmediate(image);
        }
    }

    /// <summary>Measures a visible RGB difference without depending on texture alpha or a particular source palette.</summary>
    private static float Difference(Color a,Color b) => Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);

    /// <summary>Maps a reel-relative point onto the native 1080p capture.</summary>
    private static Vector2Int Pixel(RectTransform rect,Vector2 point) => new Vector2Int(
        Mathf.RoundToInt(rect.anchoredPosition.x+point.x*rect.rect.width),1079-Mathf.RoundToInt(-rect.anchoredPosition.y+point.y*rect.rect.height));

    /// <summary>Refreshes cached mask materials before drawing a controlled diagnostic angle.</summary>
    private static void Capture(SlotMachineView view,string filename,string output)
    {
        foreach (var graphic in view.GetComponentsInChildren<UnityEngine.UI.MaskableGraphic>())
        { graphic.RecalculateClipping();graphic.RecalculateMasking();graphic.SetAllDirty(); }
        Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(),filename,output:output);
    }

    /// <summary>Loads a disposable readable image without changing import settings.</summary>
    private static Texture2D Read(string path)
    {
        var image = new Texture2D(2,2,TextureFormat.RGBA32,false);
        if (!image.LoadImage(File.ReadAllBytes(path)))
        { UnityEngine.Object.DestroyImmediate(image);throw new InvalidOperationException("Could not read "+path); }
        return image;
    }
}
