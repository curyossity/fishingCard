using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Validates original Landing imports, placeholder removal and live artwork/input bindings.</summary>
public static class TackleLandingArtChecks
{
    /// <summary>Checks unchanged exported bytes, sprite settings and complete serialized art references independently of scoring rules.</summary>
    public static int ValidateAssets()
    {
        int checks = 0;
        Action<bool, string> check = (ok, error) => { if (!ok) throw new InvalidOperationException(error); checks++; };
        string root = TackleDiscoverArtSetup.Root;
        foreach (var path in Directory.GetFiles(root + "Art/Landing", "*.png"))
        {
            check(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes("Docs/UIThemes/TackleApparatusConcept/AssetLibrary/Exports/Landing/" + Path.GetFileName(path))), "Export pixels changed: " + path);
            var import = (TextureImporter)AssetImporter.GetAtPath(path);
            check(import.textureType == TextureImporterType.Sprite && import.spriteImportMode == SpriteImportMode.Single
                && import.filterMode == FilterMode.Point && !import.mipmapEnabled && !import.isReadable
                && import.textureCompression == TextureImporterCompression.Uncompressed && import.maxTextureSize == 2048, "Incorrect import: " + path);
        }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath);
        var input = new SerializedObject(prefab.GetComponent<LandingChallengeView>());
        var art = prefab.GetComponent<TackleLandingPresentation>();
        check(art != null && input.FindProperty("artwork").objectReferenceValue == art, "Landing art is not wired.");
        foreach (string name in new[] { "strike", "returnToHarbor" }) check(input.FindProperty(name).objectReferenceValue != null, "Input unbound: " + name);
        var data = new SerializedObject(art);
        var marker = (UnityEngine.UI.Image)data.FindProperty("fishMarker").objectReferenceValue;
        check(marker.sprite != null && AssetDatabase.GetAssetPath(marker.sprite) == TackleLandingArtSetup.IndicatorPath,
            "Generic fish indicator was replaced by a species illustration.");
        check(marker.transform.childCount == 0 && marker.rectTransform == data.FindProperty("fish").objectReferenceValue,
            "Decorative fish body or separate hidden scoring marker remains.");
        check(File.ReadAllBytes(TackleLandingArtSetup.IndicatorPath).SequenceEqual(File.ReadAllBytes("Docs/UIThemes/TackleApparatusConcept/LandingIndicator/generic-fish.png")),
            "Generic marker differs from its original pixel source.");
        foreach (string field in new[] { "phase", "haul", "creature", "tags", "points", "resistance", "attempts", "movement", "message", "grade", "breakdown", "emptyRig", "portrait", "fishMarker", "track", "fish", "goodZone", "perfectZone", "progressFill", "lampContent", "lampTemplate", "lampOn", "lampOff", "rigContent", "rigCardPrefab", "rigScroll", "scoreScroll" })
            check(data.FindProperty(field).objectReferenceValue != null, "Presentation unbound: " + field);
        var scaler = prefab.GetComponent<UnityEngine.UI.CanvasScaler>();
        check(scaler.referenceResolution == new Vector2(1920, 1080) && scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand, "Incorrect stage scaling.");
        foreach (var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if (image.sprite != null) check(AssetDatabase.GetAssetPath(image.sprite).StartsWith(root, StringComparison.Ordinal), "Unexpected placeholder/theme image: " + image.name);
            else check(image.gameObject == prefab || image.color.a == 0, "Visible unsourced rectangle: " + image.name);
        }
        foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true))
            check(AssetDatabase.GetAssetPath(text.font).StartsWith(root + "Fonts/", StringComparison.Ordinal), "Default placeholder font remains: " + text.name);
        check(!File.ReadAllText("Assets/Editor/LandingChallengeSetup.cs").Contains("Install Plain Controls"), "Plain generator remains available.");
        return checks;
    }

    /// <summary>Exercises the real skinned phase, shared rig portraits, exact scoring geometry, scrolls and physical button hit targets with saves disabled.</summary>
    public static int ValidateLiveArtwork(string output = "Docs/Validation/TackleLandingArt/")
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Use a fresh comparison Play Mode session.");
        int checks = 0;
        Action<bool, string> check = (ok, error) => { if (!ok) throw new InvalidOperationException(error); checks++; };
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var controller = harbor.Controller;
        controller.SetDebugSaveResetPending(true); check(!controller.RunActive, "Expected fresh Harbor.");
        controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Schooling", "Small" }); harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        var chain = (CatchChainRuntime)typeof(FishingRunController).GetField("catchChainRuntime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
        var definitions = AssetDatabase.FindAssets("t:CardDefinition").Select(id => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
        string[] species = { "creature_sardine", "creature_mackerel", "creature_glass_minnow", "creature_garfish" };
        for (int i = 0; i < 8; i++) chain.Add(definitions.First(c => c.UniqueId == species[i % 4]), new EffectResolver(), i % 2 == 0 ? CatchAttachmentRole.Catch : CatchAttachmentRole.Bait);
        var slot = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(); SlotReelAnimationChecks.SettleForControlFixture(slot, controller);
        check(controller.TryAcceptSlotMachine(controller.SlotMachine.Revision), "Could not start Landing.");
        var view = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>(); var art = view.GetComponent<TackleLandingPresentation>(); art.Present(controller);
        check(!slot.gameObject.activeInHierarchy && art.gameObject.activeInHierarchy, "Discover overlaps Landing.");
        var data = new SerializedObject(art); var model = controller.Landing;
        var track = (RectTransform)data.FindProperty("track").objectReferenceValue;
        check(Math.Abs(model.FishWidth / model.PerfectWidth - 2.0 / 3) < .00001, "Baseline fish is not two-thirds of the initial Perfect width.");
        foreach (string name in new[] { "fish", "goodZone", "perfectZone" })
        {
            var rect = (RectTransform)data.FindProperty(name).objectReferenceValue;
            double expected = name == "fish" ? model.FishWidth : name == "goodZone" ? model.GoodWidth : model.PerfectWidth;
            check(Math.Abs(rect.rect.width - track.rect.width * expected) < .01, "Art changed the scoring width: " + name);
        }
        var rigScroll = (UnityEngine.UI.ScrollRect)data.FindProperty("rigScroll").objectReferenceValue;
        check(rigScroll.content.rect.height > rigScroll.viewport.rect.height, "Long rig does not scroll.");
        var cards = rigScroll.content.GetComponentsInChildren<TackleRigCardView>(); check(cards.Length == 8, "Rig was capped or duplicated.");
        for (int i = 0; i < cards.Length; i++)
        {
            check(ReferenceEquals(cards[i].BoundInstance, controller.CatchChain[i]), "Committed rig order changed.");
            var cardData = new SerializedObject(cards[i]); var portrait = (UnityEngine.UI.Image)cardData.FindProperty("portrait").objectReferenceValue;
            check(portrait.enabled && portrait.sprite != null, "Provided rig portrait missing.");
        }
        Canvas.ForceUpdateCanvases();
        var input = new SerializedObject(view); var button = (UnityEngine.UI.Button)input.FindProperty("strike").objectReferenceValue;
        var eventSystem = EventSystem.current; var pointer = new PointerEventData(eventSystem);
        pointer.position = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)button.transform).TransformPoint(((RectTransform)button.transform).rect.center));
        var hits = new List<RaycastResult>(); eventSystem.RaycastAll(pointer, hits);
        check(hits.Count > 0 && ExecuteEvents.GetEventHandler<UnityEngine.UI.Button>(hits[0].gameObject) == button.gameObject, "Strike hit target is obstructed.");
        var feedback = button.GetComponent<TackleButtonFeedback>(); var fields = new SerializedObject(feedback);
        var visual = (RectTransform)fields.FindProperty("visual").objectReferenceValue; Vector2 rest = visual.anchoredPosition;
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler); check(visual.anchoredPosition.y < rest.y, "Pressed artwork did not move.");
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler); check(visual.anchoredPosition == rest, "Pressed artwork did not restore.");
        button.interactable = false; feedback.RefreshAvailability(); check(visual.GetComponent<CanvasGroup>().alpha < 1, "Disabled button art was not dimmed.");
        button.interactable = true; feedback.RefreshAvailability();
        foreach (var text in view.GetComponentsInChildren<TMP_Text>())
            if (text.GetComponentInParent<UnityEngine.UI.ScrollRect>() == null) check(!text.isTextOverflowing, "Live text overflow: " + text.name);
        foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 800), new Vector2Int(2560, 1080) })
            SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "Landing-" + size.x + "x" + size.y + ".png", size.x, size.y, output);
        check(controller.Landing.Points == 0 && controller.Landing.AttemptsRemaining == 4, "Artwork validation consumed a strike.");
        return checks;
    }
}
