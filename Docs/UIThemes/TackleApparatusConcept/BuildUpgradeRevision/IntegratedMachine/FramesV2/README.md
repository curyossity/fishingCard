# Build housing v2 — integrated shared frames

This version preserves the existing Build casing, wooden rig recess, bottom hardware and alpha. Its two upper frames are integrated into the whole machine sprite and match Landing's native positions and sizes.

`Sources/landing-frame-reference.png` is a frozen copy of the normalized Landing master. `ComposeBuildFrames.cs` copies its upper working area at 1:1 coordinates into the original unframed Build master. The cream creature frame is pixel-identical to Landing. The right effects frame retains every original border pixel; only its dark interior is replaced to remove the score progress trough. Built-in image editing supplied that blank interior, and `Prompt.txt` records the exact prompt. No external image API was used.

The interior fill covers the whole dark well, avoiding a rectangular repair patch where the bar used to be. `upper-frame-mask.png` and `effects-interior-mask.png` document the boundaries. `CompositionVerification.json` records zero alpha changes, zero visible changes outside the upper working area, zero creature-frame differences and zero effects-border differences. The original lower wooden rig and outer silhouette are unchanged.

The complete result is `../integrated-build-machine-v2.png`, **1739 × 904**, drawn at the shared **490,163 / 1310 × 681** bounds. The active exporter copies it byte-for-byte. Separate `build-portrait` and `build-effect` overlays are removed from the prefab and runtime assets. Native portraits, tags, level, resolved sale value and current-level ability descriptions remain live. The insertion heading sits below the new frames so their lower borders stay visible.

To reproduce, compile this directory's `ComposeBuildFrames.cs` with System.Drawing and System.Web.Extensions, then run it with the repository root. Run `../../export_runtime_assets.py`, render its RuntimeAssets composition, then stamp hashes with the exporter again and install through `RigBuildingSetup.InstallCurrentScene()` outside Play mode. Run Landing's normalizer against the updated runtime Build master to refresh its base hash; the Landing output remains byte-identical because the shared outer pixels and its generated interior are unchanged.

The original `../integrated-build-machine-source.png` is retained as an immutable v1 input. Frozen source references make the versioned derivation reproducible. Runtime checks independently compare the active Build frames against active Landing pixels and reject restored overlay images.
