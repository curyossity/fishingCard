using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Validates integrated Landing art, its invariant Build silhouette and live input/scoring geometry.</summary>
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
        checks+=ValidateSharedHousing(prefab);
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

    /// <summary>Compares every alpha value and every protected visible exterior pixel against Build, then checks identical placement and scale.</summary>
    private static int ValidateSharedHousing(GameObject prefab)
    {
        int checks=0;Action<bool,string> check=(ok,error)=>{if(!ok)throw new InvalidOperationException(error);checks++;};
        var images=prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        var landing=images.Single(i=>i.name=="landing-machine");
        var build=AssetDatabase.LoadAssetAtPath<GameObject>(RigBuildingSetup.PrefabPath).GetComponentsInChildren<UnityEngine.UI.Image>(true).Single(i=>i.name=="build-machine");
        check(AssetDatabase.GetAssetPath(landing.sprite)==TackleLandingArtSetup.HousingPath,"Landing uses the wrong machine source.");
        check(File.ReadAllBytes(TackleLandingArtSetup.HousingPath).SequenceEqual(File.ReadAllBytes(TackleLandingArtSetup.HousingSource)),"Landing housing was resampled during import.");
        check(landing.rectTransform.anchoredPosition==build.rectTransform.anchoredPosition&&landing.rectTransform.sizeDelta==build.rectTransform.sizeDelta&&landing.rectTransform.localScale==build.rectTransform.localScale,"Landing/Build machine anchors or scale differ.");
        check(landing.rectTransform.sizeDelta==TackleMachineGeometry.StageBounds.size&&landing.type==UnityEngine.UI.Image.Type.Simple,"Machine geometry is sliced or does not use the shared bounds.");
        foreach(var retired in new[]{"machine-housing","portrait-frame","score-readout","progress-trough","timing-track","status-plaque"})
            check(!images.Any(i=>i.name==retired),"A separate background still overlays the integrated housing: "+retired);
        var importer=(TextureImporter)AssetImporter.GetAtPath(TackleLandingArtSetup.HousingPath);
        check(importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&!importer.isReadable&&importer.textureCompression==TextureImporterCompression.Uncompressed&&importer.spriteBorder==Vector4.zero,"Integrated housing import settings are incorrect.");
        var basis=new Texture2D(2,2,TextureFormat.RGBA32,false);var variant=new Texture2D(2,2,TextureFormat.RGBA32,false);var mask=new Texture2D(2,2,TextureFormat.RGBA32,false);
        try
        {
            basis.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(build.sprite)));variant.LoadImage(File.ReadAllBytes(TackleLandingArtSetup.HousingSource));
            mask.LoadImage(File.ReadAllBytes("Docs/UIThemes/TackleApparatusConcept/LandingIntegrated/landing-interior-mask.png"));
            check(basis.width==variant.width&&basis.height==variant.height&&mask.width==basis.width&&mask.height==basis.height,"Landing/Build native dimensions differ.");
            var original=basis.GetPixels32();var result=variant.GetPixels32();var editable=mask.GetPixels32();int alphaDifferent=0,outerDifferent=0,protectedPixels=0,interiorDifferent=0;
            for(int i=0;i<original.Length;i++)
            {
                if(original[i].a!=result[i].a)alphaDifferent++;
                bool different=original[i].r!=result[i].r||original[i].g!=result[i].g||original[i].b!=result[i].b;
                if(editable[i].a==0){protectedPixels++;if(original[i].a>0&&different)outerDifferent++;}
                else if(different)interiorDifferent++;
            }
            check(alphaDifferent==0,"Landing's silhouette differs from Build.");
            check(outerDifferent==0&&protectedPixels>700000,"Protected casing or hardware pixels changed.");
            check(interiorDifferent>0,"The Landing interior was not created.");
        }
        finally{UnityEngine.Object.DestroyImmediate(basis);UnityEngine.Object.DestroyImmediate(variant);UnityEngine.Object.DestroyImmediate(mask);}
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
        controller.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        var definitions = AssetDatabase.FindAssets("t:CardDefinition").Select(id => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
        string[] species = { "creature_sardine", "creature_mackerel", "creature_glass_minnow", "creature_garfish", "creature_cleaner_shrimp", "creature_clinging_crab", "creature_coastal_cod", "creature_flounder" };
        for (int i = 0; i < species.Length; i++)
            check(controller.Rig.TryAcquire(controller.Rig.Revision, definitions.First(c => c.UniqueId == species[i]), i, out _, out _), "Could not prepare unique-species rig fixture.");
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
            Capture(view, "Landing-" + size.x + "x" + size.y + ".png", size.x, size.y, output);
        check(controller.Landing.Points == 0 && controller.Landing.AttemptsRemaining == 4, "Artwork validation consumed a strike.");
        return checks;
    }

    /// <summary>Captures a disposable native canvas without changing the live view's clipping or camera state.</summary>
    private static void Capture(LandingChallengeView view,string file,int width,int height,string output)
    {
        var copy=UnityEngine.Object.Instantiate(view.gameObject);
        try
        {
            copy.name="Landing capture fixture";copy.GetComponent<LandingChallengeView>().enabled=false;copy.GetComponent<TackleLandingPresentation>().enabled=false;
            foreach(var image in copy.GetComponentsInChildren<UnityEngine.UI.MaskableGraphic>()){image.RecalculateClipping();image.RecalculateMasking();image.SetAllDirty();}
            Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(copy.GetComponent<Canvas>(),file,width,height,output);
        }
        finally{UnityEngine.Object.DestroyImmediate(copy);}
    }
}
