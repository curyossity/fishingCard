using UnityEditor;
using UnityEngine;

/// <summary>Clips the moving paper and native faces while exposing separately animated drum bands around the opening.</summary>
public static class TackleReelApertureSetup
{
    private const string ApertureName = "Curved Reel Aperture", RimName = "Reel Edge Bands";

    /// <summary>Traces the inner paper edge of the preserved 412x490 drum, narrowing at its top and bottom.</summary>
    private static Vector2[] Contour()
    {
        float[] rows = {18,30,55,95,160,245,330,395,445,468,475};
        float[] left = {64,50,40,31,27,26,28,34,44,59,76};
        float[] right = {348,360,372,382,386,387,384,379,370,356,339};
        var points = new Vector2[rows.Length * 2];
        for (int i = 0; i < rows.Length; i++)
        {
            points[i] = new Vector2(left[i] / 412, rows[i] / 490);
            int opposite = rows.Length - i - 1;
            points[rows.Length + i] = new Vector2(right[opposite] / 412, rows[opposite] / 490);
        }
        return points;
    }

    /// <summary>Idempotently wraps the current full-size wheels, or unwraps them when restoring the compact version.</summary>
    public static void Configure(GameObject root, bool enabled)
    {
        foreach (var reel in root.GetComponentsInChildren<TackleReelAnimator>(true))
        {
            var aperture = reel.transform.Find(ApertureName) as RectTransform;
            var rim = reel.transform.Find(RimName) ?? reel.transform.Find("Fixed Reel Rim");
            var surface = reel.GetComponentInChildren<TackleReelSurface>(true);
            var fields = new SerializedObject(reel);
            if (!enabled)
            {
                if (aperture != null)
                {
                    surface.transform.SetParent(reel.transform, false); surface.transform.SetAsFirstSibling();
                    UnityEngine.Object.DestroyImmediate(aperture.gameObject);
                }
                if (rim != null) UnityEngine.Object.DestroyImmediate(rim.gameObject);
                surface.ConfigureEdges(null);
                fields.FindProperty("faceContent").objectReferenceValue = null;
                fields.ApplyModifiedPropertiesWithoutUndo(); continue;
            }
            if (rim == null)
            {
                rim = new GameObject(RimName, typeof(RectTransform), typeof(UnityEngine.UI.RawImage)).transform;
                Stretch((RectTransform)rim, reel.transform);
            }
            rim.name = RimName;
            rim.SetAsFirstSibling();
            var image = rim.GetComponent<UnityEngine.UI.RawImage>();
            image.texture = surface.GetComponent<UnityEngine.UI.RawImage>().texture; image.raycastTarget = false;
            surface.ConfigureEdges(image);
            if (aperture == null)
            {
                aperture = new GameObject(ApertureName, typeof(RectTransform), typeof(ReelApertureMask), typeof(UnityEngine.UI.Mask)).GetComponent<RectTransform>();
                Stretch(aperture, reel.transform);
            }
            aperture.GetComponent<ReelApertureMask>().Configure(Contour());
            aperture.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            surface.transform.SetParent(aperture, false); surface.transform.SetAsFirstSibling();
            fields.FindProperty("faceContent").objectReferenceValue = aperture;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    /// <summary>Matches the existing cylinder rectangle exactly; its native projection remains authoritative.</summary>
    private static void Stretch(RectTransform rect, Transform parent)
    {
        rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f,.5f); rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
