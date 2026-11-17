# Integrated Build housing — 17 November 2026

**Active variant: [FramesV2](FramesV2/README.md).** `integrated-build-machine-v2.png` adds Landing's exact creature/effects frames to this housing. The effects well has no score progress trough. Both frames are baked into the whole machine; the assembled frame overlays are removed. The casing and wooden rig below are pixel-identical to v1. The original source described below remains the immutable generation input, while the runtime exporter now copies v2.

This is the active Build background. The complete machine casing and recessed wooden insertion compartment are authored together in one transparent sprite. It replaces both the generic empty machine housing and the separately composited tray backgrounds.

The earlier generic housing's recess ended above the wooden tray. A screenshot-extracted brass rim and a stretched wood region crossed its metal footer, so transparency and matching the rim's original pixels could not resolve the structural mismatch. The page 08 standalone rail also had a different housing. Those tray approaches are retired.

`integrated-build-machine-source.png` was created with built-in image editing using the existing blank housing and approved page 04 composition as references. `Prompt.txt` contains the original generation prompt; `Generation.json` records provenance, source hash, references and runtime geometry. No external image API was used. Original supplied assets remain preserved for the other phases.

The **1739 × 904** source is copied **byte-for-byte** to the runtime `build-machine.png`. Unity draws the complete sprite at **490,163 / 1310 × 681**, preserving its aspect ratio within 0.001. There is no screenshot extraction, wood-patch enlargement or separate tray face in the active Build composition. The wooden grain, brass bevel, depth shadows and surrounding green casing belong to this single asset.

Cards, clips, connecting ropes, text, insertion sockets and the selection arrow remain independent native UI elements. All rig sizes use the same body and the same pooled decorations. The insertion viewport is **625,574 / 1020 × 223**, aligned with the authored opening; longer rigs scroll horizontally. This changes presentation only.

To reproduce imports, run `../export_runtime_assets.py`, render `../Refinement/RuntimeAssets` with `../ConceptRenderer.cs` and the AssetLibrary fonts, then run the exporter again to stamp hashes. Its `copySource` branch copies the complete housing without decoding or resampling it. Install through `RigBuildingSetup.InstallCurrentScene()` outside Play mode. This removes the superseded runtime tray PNGs after replacing their prefab references.

Checks reject a nonuniformly scaled housing, any separate tray image, retained obsolete runtime sprites, altered source bytes or a mismatched viewport. Live captures cover empty and populated rigs, upgrades and aspect ratios; full-loop checks cover long rig scrolling.
