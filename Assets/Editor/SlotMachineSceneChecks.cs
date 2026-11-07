using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Exercises the installed Harbor and Discover controls in a fresh Play Mode session.</summary>
public static class SlotMachineSceneChecks
{
    private static string outputDirectory = "Docs/Validation/SlotMachine/";
    /// <summary>Tests either acceptance or final respin without editing player saves or inventing a landing result.</summary>
    public static int ValidateLive(bool respin, string output = null)
    {
        outputDirectory = string.IsNullOrWhiteSpace(output) ? "Docs/Validation/SlotMachine/" : output.TrimEnd('/', '\\') + "/";
        if (!Application.isPlaying) throw new InvalidOperationException("Enter the installed comparison scene in Play Mode.");
        int checks = 0;
        Action<bool, string> assert = (condition, message) =>
        { if (!condition) throw new InvalidOperationException(message); checks++; };
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        var controller = harbor.Controller;
        assert(!controller.RunActive && harbor.HubVisible, "Expected a fresh Harbor session.");
        controller.StartRun();
        assert(!controller.RunActive, "Run started without four player-selected tags.");
        assert(!harbor.TryStartNewRun(), "Set Sail skipped tag preparation.");
        var preparation = harbor.GetComponentInChildren<SlotTagPreparationView>(true);
        assert(preparation.gameObject.activeInHierarchy, "Tag preparation did not open.");
        var hud = UnityEngine.Object.FindAnyObjectByType<JournalGameplayHud>(FindObjectsInactive.Include);
        var data = new SerializedObject(preparation);
        var confirm = data.FindProperty("confirm").objectReferenceValue as UnityEngine.UI.Button;
        assert(!confirm.interactable, "Empty preparation was confirmable.");
        for (int i = 0; i < 4; i++)
            ((UnityEngine.UI.Button)data.FindProperty("tagButtons").GetArrayElementAtIndex(i).objectReferenceValue).onClick.Invoke();
        assert(confirm.interactable, "Exactly four tags did not enable Set Sail.");
        assert(!((UnityEngine.UI.Button)data.FindProperty("tagButtons").GetArrayElementAtIndex(4).objectReferenceValue).interactable,
            "A fifth tag remained selectable.");
        Capture(harbor.GetComponent<Canvas>(), "Preparation1080.png");
        confirm.onClick.Invoke();
        assert(controller.RunActive && !harbor.HubVisible && controller.SlotMachine.Active, "Confirmed preparation did not enter Discover.");
        assert(controller.ExpeditionSlotTags.Count == 4 && controller.SlotMachine.RespinsRemaining == 1, "Frozen loadout or single respin missing.");
        assert(!controller.Timing.Active && controller.CurrentEncounter == null && controller.CatchChain.Length == 0,
            "Tag bar/creature/rig was active before selection.");
        assert(!controller.CanDescend && !controller.CanSurface, "Legacy actions available in Discover.");
        assert(!hud.DescendButton.gameObject.activeSelf && !hud.SurfaceButton.gameObject.activeSelf, "Obsolete actions still visible.");
        foreach (var meter in hud.GetComponentsInChildren<LineLoadMeterView>(true))
            assert(!meter.gameObject.activeSelf, "Obsolete capacity meter still visible.");
        assert(!controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Small", "Heavy" }), "Active symbols changed mid-expedition.");
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        assert(view != null, "Plain Discover controls missing.");
        SlotReelAnimationChecks.SettleForControlFixture(view, controller);
        Canvas.ForceUpdateCanvases();
        foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>())
            assert(!text.isTextOverflowing, "Discover label overflow: " + text.name);
        var viewData = new SerializedObject(view);
        var respinButton = viewData.FindProperty("respinButton").objectReferenceValue as UnityEngine.UI.Button;
        assert(!respinButton.interactable, "Respin enabled before selecting one locked reel.");
        Capture(view.GetComponentInParent<Canvas>(), "Discover1080.png");
        long revision = controller.SlotMachine.Revision;
        if (respin)
        {
            string preserved = controller.SlotMachine.GetTag(0);
            ((UnityEngine.UI.Button)viewData.FindProperty("lockButtons").GetArrayElementAtIndex(0).objectReferenceValue).onClick.Invoke();
            assert(controller.SlotMachine.IsLocked(0) && respinButton.interactable, "Lock did not enable respin.");
            assert(!controller.TrySetSlotReelLocked(controller.SlotMachine.Revision, 1, true), "Controller allowed two locked reels.");
            respinButton.onClick.Invoke();
            SlotReelAnimationChecks.SettleForControlFixture(view, controller);
            assert(controller.SlotMachine.RespinsRemaining == 0 && controller.SlotMachine.GetTag(0) == preserved, "Respin spent the wrong allowance or changed the locked face.");
            assert(!controller.TryRespinSlotMachine(controller.SlotMachine.Revision), "Second respin accepted.");
            assert(controller.SlotMachine.Active && !controller.AwaitingLandingChallenge && controller.CurrentEncounter == null,
                "Respin revealed a creature before Accept Tags.");
            assert(view.gameObject.activeInHierarchy && !respinButton.interactable, "Final reels hidden or respin left enabled.");
            var accept = (UnityEngine.UI.Button)viewData.FindProperty("acceptButton").objectReferenceValue;
            assert(accept.interactable, "Final result could not be accepted.");
            for (int i = 0; i < 3; i++)
            {
                var button = (UnityEngine.UI.Button)viewData.FindProperty("lockButtons").GetArrayElementAtIndex(i).objectReferenceValue;
                var label = (TMP_Text)viewData.FindProperty("reelLabels").GetArrayElementAtIndex(i).objectReferenceValue;
                assert(!button.interactable && !controller.TrySetSlotReelLocked(controller.SlotMachine.Revision, i, true), "Lock changed after respin.");
                assert(string.Equals(label.text, controller.SlotMachine.GetTag(i), StringComparison.OrdinalIgnoreCase), "Displayed final reel differs from runtime result.");
            }
            assert(!controller.TryAcceptSlotMachine(revision), "Stale initial Accept committed the respun result.");
            Capture(view.GetComponentInParent<Canvas>(), "FinalRespin1080.png");
            accept.onClick.Invoke();
        }
        else ((UnityEngine.UI.Button)viewData.FindProperty("acceptButton").objectReferenceValue).onClick.Invoke();
        assert(!controller.SlotMachine.Active && controller.AwaitingLandingChallenge && controller.CurrentEncounter != null,
            "Final result did not hand off to landing.");
        assert(controller.CatchChain.Length == 0 && !controller.TryChooseEncounterRole(EncounterEffectChoice.Catch)
            && !controller.TryDescend() && !controller.TrySurface(), "Selection authorized acquisition or an obsolete exit.");
        assert(!controller.TryAcceptSlotMachine(revision), "Stale/double acceptance accepted.");
        assert(!((GameObject)viewData.FindProperty("discoverContent").objectReferenceValue).activeInHierarchy,
            "Reels remained visible during landing handoff.");
        foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>())
            assert(!text.isTextOverflowing, "Discover label overflow: " + text.name);
        return checks;
    }

    /// <summary>Captures existing UI at a requested viewport size without changing or importing artwork.</summary>
    internal static void Capture(Canvas canvas, string filename, int width = 1920, int height = 1080, string output = null)
    {
        if (output != null) outputDirectory = output.TrimEnd('/', '\\') + "/";
        Directory.CreateDirectory(outputDirectory);
        var cameraObject = new GameObject("Slot Validation Camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
        camera.transform.position = new Vector3(0, 0, -1000); camera.orthographic = true; camera.orthographicSize = 1080;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(18, 34, 42, 255);
        var mode = canvas.renderMode; var previousCamera = canvas.worldCamera; float distance = canvas.planeDistance;
        var target = new RenderTexture(width, height, 24); var previousTarget = RenderTexture.active; Texture2D image = null;
        try
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 100;
            camera.targetTexture = target; Canvas.ForceUpdateCanvases();
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target };
                if (!UnityEngine.Rendering.RenderPipeline.SupportsRenderRequest(camera, request))
                    throw new InvalidOperationException("The active render pipeline does not support UI capture requests.");
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
            }
            else camera.Render();
            RenderTexture.active = target;
            image = new Texture2D(width, height, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(outputDirectory + filename, image.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode = mode; canvas.worldCamera = previousCamera; canvas.planeDistance = distance;
            camera.targetTexture = null; RenderTexture.active = previousTarget; target.Release();
            UnityEngine.Object.DestroyImmediate(target); if (image != null) UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(cameraObject); Canvas.ForceUpdateCanvases();
        }
    }
}
