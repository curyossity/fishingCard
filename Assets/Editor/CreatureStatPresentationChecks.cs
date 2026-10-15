using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Checks actual stat glyphs on the production creature prefab at compact and full presentation sizes.</summary>
public static class CreatureStatPresentationChecks
{
    private const string Output = "Docs/Validation/CreatureStats";
    private static int assertions;

    /// <summary>Renders isolated preview cards and verifies stat meshes without editing the open scene or card assets.</summary>
    [MenuItem("Fishing Cards/Validation/Creature Stat Presentation")]
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        Scene preview = EditorSceneManager.NewPreviewScene();
        RenderTexture previous = RenderTexture.active;
        assertions = 0;
        try
        {
            GameObject cameraObject = new GameObject("Stat validation camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, preview);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.scene = preview; camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.07f, 0.08f);
            camera.nearClipPlane = -10f; camera.farClipPlane = 10f;
            GameObject canvasObject = new GameObject("Stat validation canvas", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, preview);
            Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1f;
            CreatureCardView prefab = AssetDatabase.LoadAssetAtPath<CreatureCardView>("Assets/Prefabs/UI/Components/CreatureCardView.prefab");
            Require(prefab != null, "Production creature card prefab missing.");
            CreatureCardView face = UnityEngine.Object.Instantiate(prefab, canvas.transform);
            RectTransform rect = face.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            TMP_Text weight = Field<TMP_Text>(face, "weightText"), value = Field<TMP_Text>(face, "valueText");
            // Serialized defaults must already be safe, as well as the runtime event/catch layout switch.
            Require(weight.fontSizeMin == 10f && value.fontSizeMin == 10f
                && weight.textWrappingMode == TextWrappingModes.NoWrap && value.textWrappingMode == TextWrappingModes.NoWrap
                && weight.overflowMode == TextOverflowModes.Overflow && value.overflowMode == TextOverflowModes.Overflow,
                "Prefab stat fields restored the generic minimum/wrapping/ellipsis settings.");
            RenderTexture target = new RenderTexture(700, 900, 24, RenderTextureFormat.ARGB32);
            try
            {
                camera.targetTexture = target;
                foreach (int height in new[] { 480, 611, 800 })
                {
                    rect.sizeDelta = new Vector2(height * CreatureCardView.ReferenceAspectRatio, height);
                    foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }))
                    {
                        CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                        if (card.CardType != CardType.Creature && card.CardType != CardType.Treasure && card.CardType != CardType.ApexEncounter) continue;
                        face.SetCard(card, card.Weight, card.Value, false);
                        AssertGlyphs(weight, card.Weight.ToString(), card.DisplayName + " Weight at " + height);
                        string expectedValue = card.HidesOwnValueDuringRunForRole(CatchAttachmentRole.Catch) ? "?" : card.Value.ToString();
                        AssertGlyphs(value, expectedValue, card.DisplayName + " Value at " + height);
                        if (card.DisplayName == "Garfish" && height == 611) Capture(camera, target, "garfish-main-card.png");
                    }
                    CardDefinition garfish = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Garfish.asset");
                    face.SetCard(garfish, 0, 9999, false);
                    AssertGlyphs(weight, "0", "Zero Weight"); AssertGlyphs(value, "9999", "Modified Value");
                    face.SetCard(garfish, 1, 9, true, true);
                    AssertGlyphs(weight, "?", "Concealed Weight"); AssertGlyphs(value, "?", "Concealed Value");
                    CardDefinition hazard = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Reef Snag.asset");
                    face.SetCard(hazard, 0, 0, false);
                    Require(!weight.transform.parent.gameObject.activeSelf && !value.transform.parent.gameObject.activeSelf,
                        "Event layout exposed creature stats.");
                    face.SetCard(garfish, 1, 9, false);
                    AssertGlyphs(weight, "1", "Weight after event"); AssertGlyphs(value, "9", "Value after event");
                    face.SetBlank(); face.SetCard(garfish, 1, 9, false);
                    AssertGlyphs(weight, "1", "Weight after blank"); AssertGlyphs(value, "9", "Value after blank");
                }
            }
            finally { camera.targetTexture = null; target.Release(); UnityEngine.Object.DestroyImmediate(target); }
            File.WriteAllText(Path.Combine(Output, "unity-result.json"), "{\"status\":\"passed\",\"assertions\":" + assertions
                + ",\"unity\":\"" + Application.unityVersion + "\",\"heights\":[480,611,800]}");
            Debug.Log("Creature stat presentation passed: " + assertions + " assertions.");
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(Output, "unity-failure.txt"), error.ToString()); throw;
        }
        finally { RenderTexture.active = previous; EditorSceneManager.ClosePreviewScene(preview); }
    }

    /// <summary>Checks rendered characters rather than merely checking the assigned text string.</summary>
    private static void AssertGlyphs(TMP_Text text, string expected, string context)
    {
        Canvas.ForceUpdateCanvases(); text.ForceMeshUpdate(true, true);
        Require(text.gameObject.activeInHierarchy && text.enabled && text.GetParsedText().StartsWith(expected, StringComparison.Ordinal), context + " missing text.");
        Require(!text.isTextTruncated && !text.isTextOverflowing, context + " does not fit its authored box.");
        for (int i = 0; i < expected.Length; i++)
            Require(text.textInfo.characterInfo[i].isVisible, context + " has an invisible stat glyph.");
    }

    /// <summary>Captures the actual rendered Garfish in the isolated preview scene.</summary>
    private static void Capture(Camera camera, RenderTexture target, string name)
    {
        camera.Render(); RenderTexture.active = target;
        Texture2D image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        try { image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply(); File.WriteAllBytes(Path.Combine(Output, name), image.EncodeToPNG()); }
        finally { UnityEngine.Object.DestroyImmediate(image); }
    }

    /// <summary>Reads a serialized view reference for actual prefab checks.</summary>
    private static T Field<T>(object owner, string name) { return (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner); }
    /// <summary>Counts assertions and reports their presentation context.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
