using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Checks complete creature coverage through the real rig, Landing and card renderers without changing run or save state.</summary>
public static class CreatureSpriteChecks
{
    public const string Output = "Docs/Validation/CreatureSprites/";
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    /// <summary>Verifies selected source identities, lossless crop dimensions, native imports and all species bindings.</summary>
    public static int Validate()
    {
        int checks = 0;
        Action<bool, string> check = (ok, message) => { if (!ok) throw new InvalidOperationException(message); checks++; };
        var entries = CreatureSpriteSetup.ReadLibrary();
        check(Directory.GetFiles(CreatureSpriteSetup.Destination, "*.png").Length == entries.Length, "Unexpected creature sprite or discarded variant imported.");
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var root = new GameObject("Creature sprite review", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(root, preview);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            ((RectTransform)root.transform).sizeDelta = new Vector2(1920, 1280);
            var landing = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath), root.transform);
            landing.SetActive(false);
            var landingView = landing.GetComponent<TackleLandingPresentation>();
            var journal = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(MidnightFieldJournalAssetSetup.Root + "Prefabs/CreatureCard.prefab"), root.transform);
            var central = journal.GetComponent<CreatureCardView>();
            var chain = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<JournalCatchChainPresentation>(MidnightFieldJournalAssetSetup.Root + "Prefabs/CatchChain.prefab"), root.transform);
            var instances = entries.Select((e, i) => new CardInstance(i + 1, AssetDatabase.LoadAssetAtPath<CardDefinition>(e.definition), null,
                i % 2 == 0 ? CatchAttachmentRole.Catch : CatchAttachmentRole.Bait)).ToArray();
            chain.Refresh(instances, Array.Empty<ActiveCatchEffectRecord>(), 0, 99, -1, i => { });
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i]; var definition = instances[i].Definition;
                var expected = AssetDatabase.LoadAssetAtPath<Sprite>(CreatureSpriteSetup.Destination + entry.filename);
                check(expected != null && definition.Artwork == expected && definition.EncounterArtwork == expected, "Wrong definition sprite: " + entry.id);
                check(definition.ArtworkLayout == CardArtworkLayout.MaskedRegion && definition.EncounterArtworkScale == 1
                    && definition.EncounterArtworkRotation == 0 && definition.EncounterArtworkOffset == Vector2.zero && definition.CardFaceArtwork == null,
                    "Old illustration framing/face remains: " + entry.id);
                using (var hash = SHA256.Create())
                    check(BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(CreatureSpriteSetup.Source + "Sprites/" + entry.filename))).Replace("-", "").ToLowerInvariant() == entry.sha256,
                        "Approved source changed: " + entry.id);
                check(expected.rect.width == entry.bounds[2] + 8 && expected.rect.height == entry.bounds[3] + 8, "Unexpected crop/resampling: " + entry.id);
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(expected));
                check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && !importer.isReadable && importer.alphaIsTransparency
                    && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.npotScale == TextureImporterNPOTScale.None,
                    "Incorrect pixel import: " + entry.id);
                var rig = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath), root.transform).GetComponent<TackleRigCardView>();
                rig.Bind(instances[i]);
                var rigImage = (Image)typeof(TackleRigCardView).GetField("portrait", Fields).GetValue(rig);
                check(rigImage.enabled && rigImage.sprite == expected && rigImage.preserveAspect && rigImage.rectTransform.pivot == new Vector2(.5f, .5f), "Rig portrait missing or off-center: " + entry.id);
                var name = (TMPro.TMP_Text)typeof(TackleRigCardView).GetField("nameLabel", Fields).GetValue(rig);
                name.ForceMeshUpdate(true, true);
                check(name.textInfo.lineCount == 1 && name.textBounds.size.x <= name.rectTransform.rect.width + 1, "Name exceeds its plate: " + entry.id);
                var rect = (RectTransform)rig.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(-950 + i % 6 * 316, 495 - i / 6 * 285);
                typeof(TackleLandingPresentation).GetField("selectedCreature", Fields).SetValue(landingView, definition);
                typeof(TackleLandingPresentation).GetMethod("BindCreature", Fields).Invoke(landingView, null);
                var landingImage = (Image)typeof(TackleLandingPresentation).GetField("portrait", Fields).GetValue(landingView);
                check(landingImage.enabled && landingImage.sprite == expected && landingImage.preserveAspect && landingImage.rectTransform.pivot == new Vector2(.5f, .5f), "Landing portrait missing or off-center: " + entry.id);
                central.SetCard(definition, 0, 0, false);
                var centralImage = (Image)typeof(CreatureCardView).GetField("creatureArtwork", Fields).GetValue(central);
                check(centralImage.enabled && centralImage.sprite == expected && centralImage.preserveAspect, "Central card reverted to old theme art: " + entry.id);
                var compactImage = chain.Cards[i].transform.Find("Artwork Mask/Runtime Artwork").GetComponent<Image>();
                check(compactImage.enabled && compactImage.sprite == expected && compactImage.preserveAspect, "Compact card reverted to old theme art: " + entry.id);
            }
            var marker = (Image)typeof(TackleLandingPresentation).GetField("fishMarker", Fields).GetValue(landingView);
            check(AssetDatabase.GetAssetPath(marker.sprite) == TackleLandingArtSetup.IndicatorPath, "Generic moving indicator changed.");
            journal.SetActive(false); chain.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();
            Capture(canvas, preview);
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "Verification.json", "{\"passed\":true,\"creatures\":" + entries.Length + ",\"assertions\":" + checks
            + ",\"views\":[\"Tackle rig\",\"Landing portrait\",\"Journal central\",\"Journal compact\"],\"genericIndicatorPreserved\":true}\n");
        return checks;
    }

    /// <summary>Renders the actual rig prefab at its authored size with the project's active render pipeline.</summary>
    private static void Capture(Canvas canvas, Scene scene)
    {
        Directory.CreateDirectory(Output);
        var cameraObject = new GameObject("Creature review camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, scene);
        var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false; camera.scene = scene;
        camera.orthographic = true; camera.orthographicSize = 640; camera.transform.position = new Vector3(0, 0, -1000);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(23, 32, 31, 255); canvas.worldCamera = camera;
        var target = new RenderTexture(1920, 1280, 24); var previous = RenderTexture.active; Texture2D image = null;
        try
        {
            camera.targetTexture = target;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target };
                if (!UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(camera, request)) throw new InvalidOperationException("UI capture not supported by this render pipeline.");
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
            }
            else camera.Render();
            RenderTexture.active = target; image = new Texture2D(1920, 1280, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, 1920, 1280), 0, 0); image.Apply(); File.WriteAllBytes(Output + "RigRoster.png", image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null; RenderTexture.active = previous; target.Release(); UnityEngine.Object.DestroyImmediate(target);
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
