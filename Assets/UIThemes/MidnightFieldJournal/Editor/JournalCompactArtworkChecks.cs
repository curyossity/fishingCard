using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Verifies exact-species compact coverage, pooled missing-art clearing and independent central/compact framing.</summary>
public static class JournalCompactArtworkChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Output = "Docs/Validation/CompactPortraits/";
    private static int assertions;

    /// <summary>Exercises the actual chain renderer and captures all approved compact portraits without changing gameplay assets.</summary>
    public static void Run()
    {
        assertions = 0; Directory.CreateDirectory(Output);
        var theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        var definitions = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(id => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
        var roster = theme.CentralCards.Select(e => definitions.Single(c => c.UniqueId == e.cardUniqueId)).OrderBy(c => c.DisplayName).ToArray();
        Scene active = SceneManager.GetActiveScene(); bool dirty = active.isDirty; Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var root = new GameObject("Compact portrait review", typeof(RectTransform), typeof(Canvas)); SceneManager.MoveGameObjectToScene(root, preview);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; root.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            var chain = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<JournalCatchChainPresentation>(Root + "Prefabs/CatchChain.prefab"), root.transform);
            var instances = roster.Select((c, i) => new CardInstance(i + 1, c, null, i % 2 == 0 ? CatchAttachmentRole.Catch : CatchAttachmentRole.Bait)).ToArray();
            chain.Refresh(instances, Array.Empty<ActiveCatchEffectRecord>(), 0, 99, -1, i => { }); Canvas.ForceUpdateCanvases();
            Require(chain.Cards.Count == roster.Length, "Chain omitted physical cards.");
            for (int i = 0; i < roster.Length; i++)
            {
                Require(theme.TryGetCompactArtwork(roster[i].UniqueId, out var expected) && expected != null, "Compact art missing: " + roster[i].DisplayName);
                var image = chain.Cards[i].transform.Find("Artwork Mask/Runtime Artwork").GetComponent<UnityEngine.UI.Image>();
                Require(image.enabled && image.sprite == expected && image.preserveAspect && !image.raycastTarget, "Wrong/hidden compact portrait: " + roster[i].DisplayName);
                if (theme.TryGetArtwork(roster[i].UniqueId, out var original)) Require(expected == original, "Approved compact art was replaced.");
                else Require(expected == theme.GetCentralCard(roster[i].UniqueId).sprite, "Central illustration identity changed.");
                Require(chain.Cards[i].transform.Find("Creature Name").GetComponent<TMP_Text>().text == roster[i].DisplayName, "Pooled name mismatch.");
                Require(chain.Cards[i].transform.Find("Value Value").GetComponent<TMP_Text>().text == (instances[i].IsBait ? "0" : instances[i].HidesOwnValueDuringRun ? "?" : instances[i].HaulValueContribution.ToString()), "Role/value mask changed.");
            }
            var unmapped = definitions.First(c => c.UniqueId == "treasure_pearl_cache");
            chain.Refresh(new[] { new CardInstance(1000, unmapped) }, Array.Empty<ActiveCatchEffectRecord>(), 0, 99, -1, i => { });
            Require(!chain.Cards[0].transform.Find("Artwork Mask/Runtime Artwork").GetComponent<UnityEngine.UI.Image>().enabled, "Unmapped identity retained another creature's art.");
            chain.Refresh(instances, Array.Empty<ActiveCatchEffectRecord>(), 0, 99, -1, i => { });
            for (int i = 0; i < roster.Length; i++)
            {
                var rect = chain.Cards[i].GetComponent<RectTransform>(); rect.SetParent(root.transform, false);
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(680, 382.022472f);
                rect.anchoredPosition = new Vector2(-1080 + (i % 4) * 720, 850 - (i / 4) * 410);
            }
            chain.gameObject.SetActive(false); Canvas.ForceUpdateCanvases();
            var cameraObject = new GameObject("Portrait review camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, preview);
            var camera = cameraObject.GetComponent<Camera>(); camera.scene = preview; camera.orthographic = true; camera.orthographicSize = 1080; camera.transform.position = new Vector3(0, 0, -1000); camera.backgroundColor = new Color32(15, 27, 33, 255); camera.clearFlags = CameraClearFlags.SolidColor; canvas.worldCamera = camera;
            Capture(camera, 3840, 2160, "CompactRoster4K.png"); Capture(camera, 1920, 1080, "CompactRoster1080p.png");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
        Require(SceneManager.GetActiveScene() == active && active.isDirty == dirty, "Review changed open scene.");
        File.WriteAllText(Output + "Verification.json", "{\"passed\":true,\"assertions\":" + assertions + ",\"approvedCreaturesCovered\":" + roster.Length + ",\"originalCompactOverridesPreserved\":true,\"unknownArtClearedOnReuse\":true}");
        Debug.Log("Compact portrait coverage verified: " + roster.Length + " creatures, " + assertions + " assertions.");
    }

    /// <summary>Records real output pixels as review evidence, never as runtime card artwork.</summary>
    private static void Capture(Camera camera, int width, int height, string file)
    {
        var target = new RenderTexture(width, height, 24); var previous = RenderTexture.active; Texture2D image = null;
        try { camera.targetTexture = target; camera.Render(); RenderTexture.active = target; image = new Texture2D(width, height, TextureFormat.RGBA32, false); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes(Output + file, image.EncodeToPNG()); }
        finally { camera.targetTexture = null; RenderTexture.active = previous; target.Release(); UnityEngine.Object.DestroyImmediate(target); if (image != null) UnityEngine.Object.DestroyImmediate(image); }
    }
    /// <summary>Stops before silently accepting a blank, wrong-identity or stale pooled portrait.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
