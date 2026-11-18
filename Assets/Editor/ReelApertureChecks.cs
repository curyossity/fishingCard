using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks actual stencil rendering and native-face ownership independently of the aperture mesh builder.</summary>
public static class ReelApertureChecks
{
    /// <summary>Verifies that each moving layer shares one hidden mask while its original rim stays outside.</summary>
    public static int ValidateAssets()
    {
        int checks = 0;
        Action<bool,string> check = (value,message) => { if (!value) throw new InvalidOperationException(message); checks++; };
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
        foreach (var reel in root.GetComponentsInChildren<TackleReelAnimator>(true))
        {
            var aperture = reel.GetComponentInChildren<ReelApertureMask>(true);
            check(aperture != null && !aperture.raycastTarget && !aperture.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic,
                "A hidden, non-intercepting curved aperture is missing.");
            var fields = new SerializedObject(reel);
            check(fields.FindProperty("faceContent").objectReferenceValue == aperture.transform &&
                reel.GetComponentInChildren<TackleReelSurface>(true).transform.parent == aperture.transform,
                "Paper and native faces do not share the same aperture.");
            var rim = reel.transform.Find("Fixed Reel Rim");
            check(rim != null && rim.parent == reel.transform && !rim.IsChildOf(aperture.transform) &&
                rim.GetComponent<UnityEngine.UI.RawImage>().texture != null && rim.GetSiblingIndex() < aperture.transform.GetSiblingIndex(),
                "The fixed supplied rim is not behind the clipped moving layers.");
        }
        return checks;
    }

    /// <summary>Renders a bright probe through each mask, checking all four narrow corners and the wide middle on the GPU.</summary>
    public static int ValidateLiveArtwork(string output = "Docs/Validation/DiscoverIntegrated/FullSize/CurvedAperture/")
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Run the real Discover phase first.");
        int checks = 0;
        Action<bool,string> check = (value,message) => { if (!value) throw new InvalidOperationException(message); checks++; };
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        check(view != null && !view.IsAnimating, "Discover must be visible and settled.");
        var reels = view.GetComponentsInChildren<TackleReelAnimator>();
        var probes = new System.Collections.Generic.List<GameObject>();
        Texture2D before = null, after = null;
        try
        {
            Capture(view, "StencilBaseline.png", output); before = Read(output + "StencilBaseline.png");
            foreach (var reel in reels)
            {
                var aperture = reel.GetComponentInChildren<ReelApertureMask>();
                check(reel.GetComponentsInChildren<TackleReelFace>().All(face => face.transform.IsChildOf(aperture.transform)),
                    "A native symbol or caption escaped the mask.");
                var surface = reel.GetComponentInChildren<TackleReelSurface>();
                var rendered = surface.GetComponent<UnityEngine.UI.RawImage>().materialForRendering;
                check(rendered.GetFloat("_StencilComp") == (float)UnityEngine.Rendering.CompareFunction.Equal &&
                    Mathf.Abs(rendered.GetFloat("_Phase") - reel.Phase) < .0001f, "Paper stencil or rendered rotation is stale.");
                var probe = new GameObject("Aperture rendering probe", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                probes.Add(probe); var rect = (RectTransform)probe.transform; rect.SetParent(aperture.transform,false);
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = probe.GetComponent<UnityEngine.UI.Image>(); image.color = Color.magenta; image.raycastTarget = false;
            }
            Capture(view, "StencilProbe.png", output); after = Read(output + "StencilProbe.png");
            foreach (var reel in reels)
            {
                var rect = (RectTransform)reel.transform;
                foreach (var point in new[] { new Vector2(.10f,.05f), new Vector2(.90f,.05f), new Vector2(.12f,.95f), new Vector2(.88f,.95f) })
                {
                    var pixel = Pixel(rect,point);
                    check(before.GetPixel(pixel.x,pixel.y) == after.GetPixel(pixel.x,pixel.y), "Moving graphics covered a narrow rim corner.");
                }
                foreach (var point in new[] { new Vector2(.08f,.5f), new Vector2(.5f,.5f), new Vector2(.92f,.5f) })
                {
                    var pixel = Pixel(rect,point); Color color = after.GetPixel(pixel.x,pixel.y);
                    check(color.r > .85f && color.b > .85f && color.g < .15f, "The wide reel body was clipped or shrunk.");
                }
            }
            return checks;
        }
        finally
        {
            foreach (var probe in probes) UnityEngine.Object.DestroyImmediate(probe);
            if (before != null) UnityEngine.Object.DestroyImmediate(before);
            if (after != null) UnityEngine.Object.DestroyImmediate(after);
            Canvas.ForceUpdateCanvases();
        }
    }

    /// <summary>Maps a normalized reel point into the reference-resolution capture's bottom-left pixel coordinates.</summary>
    private static Vector2Int Pixel(RectTransform reel, Vector2 point)
    {
        return new Vector2Int(Mathf.RoundToInt(reel.anchoredPosition.x + point.x * reel.rect.width),
            1079 - Mathf.RoundToInt(-reel.anchoredPosition.y + point.y * reel.rect.height));
    }

    /// <summary>Refreshes clipping before synchronous capture without advancing the authoritative slot result.</summary>
    private static void Capture(SlotMachineView view, string name, string output)
    {
        foreach (var graphic in view.GetComponentsInChildren<UnityEngine.UI.MaskableGraphic>())
        { graphic.RecalculateClipping(); graphic.RecalculateMasking(); graphic.SetAllDirty(); }
        Canvas.ForceUpdateCanvases();
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), name, output: output);
    }

    /// <summary>Loads a disposable readable capture without changing any authored sprite importer.</summary>
    private static Texture2D Read(string path)
    {
        var image = new Texture2D(2,2,TextureFormat.RGBA32,false);
        if (!image.LoadImage(File.ReadAllBytes(path)))
        { UnityEngine.Object.DestroyImmediate(image); throw new InvalidOperationException("Could not read stencil capture."); }
        return image;
    }
}
