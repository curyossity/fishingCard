using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks the common machine silhouette and the preserved, functional Discover rollback asset.</summary>
public static class TackleDiscoverHousingChecks
{
    /// <summary>Compares actual decoded art pixels and all three serialized stage transforms, independently of provenance reports.</summary>
    public static int Validate()
    {
        int checks = 0;
        Action<bool,string> check = (condition,message) => { if (!condition) throw new InvalidOperationException(message); checks++; };
        var paths = new[] { TackleDiscoverHousingSetup.SelectedHousingPath, TackleLandingArtSetup.HousingPath, RigBuildingSetup.ArtPath + "build-machine.png" };
        var basis = Load("Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine/integrated-build-machine-source.png");
        var mask = Load(TackleDiscoverHousingSetup.SelectedSource + "discover-interior-mask.png");
        try
        {
            var shell = basis.GetPixels32(); var editable = mask.GetPixels32();
            foreach (string path in paths)
            {
                var image = Load(path);
                try
                {
                    check(image.width == basis.width && image.height == basis.height, "Housing dimensions differ: " + path);
                    var pixels = image.GetPixels32(); int alpha = 0, exterior = 0;
                    for (int i = 0; i < shell.Length; i++)
                    {
                        if (pixels[i].a != shell[i].a) alpha++;
                        if (editable[i].a == 0 && shell[i].a != 0 &&
                            (pixels[i].r != shell[i].r || pixels[i].g != shell[i].g || pixels[i].b != shell[i].b)) exterior++;
                    }
                    check(alpha == 0, "Housing silhouette differs: " + path);
                    check(exterior == 0, "Protected housing exterior differs: " + path);
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(basis); UnityEngine.Object.DestroyImmediate(mask); }
        string[] prefabs = { TackleDiscoverArtSetup.PrefabPath, LandingChallengeSetup.PrefabPath, RigBuildingSetup.PrefabPath };
        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[i]);
            var housing = prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(image => AssetDatabase.GetAssetPath(image.sprite) == paths[i]);
            var rect = housing.rectTransform; var bounds = TackleMachineGeometry.StageBounds;
            check(rect.anchoredPosition == new Vector2(bounds.x,-bounds.y) && rect.sizeDelta == bounds.size && rect.localScale == Vector3.one,
                "Housing stage geometry differs: " + prefabs[i]);
        }
        var original = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverHousingSetup.OriginalPrefab);
        check(original != null && original.GetComponentsInChildren<TackleReelAnimator>(true).Length == 3
            && original.GetComponentsInChildren<TackleReelSurface>(true).Length == 3, "Original animated Discover prefab was not retained.");
        var oldHousing = original.transform.Find("1920x1080 Stage/machine-housing").GetComponent<UnityEngine.UI.Image>();
        check(AssetDatabase.GetAssetPath(oldHousing.sprite) == TackleDiscoverArtSetup.Root + "Art/Common/machine-housing.png"
            && oldHousing.rectTransform.sizeDelta == new Vector2(1310,657), "Original housing was modified.");
        check(File.ReadAllBytes(paths[0]).SequenceEqual(File.ReadAllBytes(TackleDiscoverHousingSetup.SelectedSource + "discover-machine.png")), "Discover import changed the delivered source.");
        if (TackleDiscoverHousingSetup.UsesFullSizeReels)
        {
            var active = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
            for (int i = 0; i < 3; i++)
            {
                var wheel = (RectTransform)active.transform.Find("1920x1080 Stage/Cylindrical Reel " + i);
                var previous = (RectTransform)original.transform.Find("1920x1080 Stage/Cylindrical Reel " + i);
                check(wheel.sizeDelta == previous.sizeDelta && wheel.localScale == previous.localScale,
                    "Full-size wheel dimensions or scale do not match the original: " + i);
            }
        }
        return checks;
    }

    /// <summary>Loads a disposable readable copy without changing any Unity importer settings.</summary>
    private static Texture2D Load(string path)
    {
        var image = new Texture2D(2,2,TextureFormat.RGBA32,false);
        if (!image.LoadImage(File.ReadAllBytes(path))) { UnityEngine.Object.DestroyImmediate(image); throw new InvalidOperationException("Could not decode " + path); }
        return image;
    }
}
