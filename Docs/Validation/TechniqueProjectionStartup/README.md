# Technique projection startup correction

21 November 2026. Fix for the Unity Console exception `Blitter is already initialized`.

The surrounding session previously recorded a failed URP initialization from `Camera.Render` inside `JournalTechniqueHandPresentation.RenderNow`, reached through an `OnEnable` refresh. URP allocates Blitter resources before its volume initialization completes; an early failure can leave those resources behind, causing subsequent pipeline creation attempts to report the duplicate initialization. At the start of this correction, the configured pipeline asset existed but `RenderPipelineManager.currentPipeline` was null.

The hand now returns without consuming dirty work while the configured SRP is not initialized. `LateUpdate` retries after Unity's normal initialization. Ready URP rendering uses `SingleCameraRequest` through `RenderPipeline.SubmitRenderRequest`, following [Unity's off-screen rendering API](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/rendering/renderpipeline/submitrenderrequest). The built-in pipeline retains its camera render path. No package source, render-pipeline settings, card art or gameplay rules were changed.

After compilation/domain reload in the existing Play session, the active pipeline was a healthy `UniversalRenderPipeline`; no manual cleanup of Unity's global resources or player-run restart was required. The pre-fix Console entry was retained.

`Tools/Rendering/TechniqueProjectionAudit.cs.txt` is an executable Unity MCP method body. It creates an isolated hand prefab under a hidden, non-interacting canvas and binds four sample definitions without a player controller. `Verification.json` records 11 passing checks: target creation, independent cards, idle render suppression, inspection invalidation, resize, resource release, re-enable, disabled projection camera, preserved pipeline identity, nonempty rendered pixels and no errors during the audit. `Hand.png` is its actual render-target output, visually checked for the four projected cards. The player's run and sound preferences were not changed. A fresh Editor-process launch was not tested.
