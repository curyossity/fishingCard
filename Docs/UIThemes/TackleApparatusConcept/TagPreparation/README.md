# Prepare the Reels

20 November 2026. New Harbor tag-selection screen, designed around the expedition's Tackle Apparatus presentation. The old Harbor's pages and artwork are unchanged. Penpot had no connected document, so design and review took place in native Unity UI before scene integration.

## Design

The five tag plates hang from one rope rail on the existing wooden desk. Each plate shows the actual Discover reel illustration and tag name, an explicit selected state and a lamp. A separate brass plaque below every plate explains its shared philosophy; these remain readable when a full selection disables the fifth tag. Ivory faces and lit lamps identify the four chosen symbols. Sage faces, unlit lamps and text identify unselected symbols.

The title reads **Prepare the Reels**, with **Choose 4 tags for this expedition** underneath. The lower console has Back, a selected-count readout with four lamps and Set Sail. Context explains the fixed 25% per-reel chance of each chosen tag, and that creatures with other tags can still appear. A failed start uses the guidance region rather than squeezing an error into the numeric counter.

The 1920 × 1080 stage uses the same Expand canvas scaling as Discover/Landing/Build. It is a root canvas at sorting order 500, so the old Harbor's 3840 × 2160 scaling and page hierarchy do not control the new screen. Native buttons retain pointer, directional keyboard/gamepad focus, press motion and independent focus corners. Native Cancel returns without starting a run.

## Assets and authoring

All imagery is reused without pixel edits from `Assets/UIThemes/TackleApparatus/Art/Common/` and `Art/Discover/Symbols/`: desk, rope, clips, ivory/sage faces, tag illustrations, brass plaques, lamps, console, readout and primary/secondary buttons. Fonts are the existing PressStart2P and VT323 assets. Names, philosophy text, instructions and selection state remain native TMP fields; no per-state screenshot is used as runtime artwork.

`TackleTagPreparationSetup.BuildPrefab()` authors `Assets/SlotMachine/TagPreparation.prefab`, preserving its GUID. The initial native [empty design](Design-1920x1080.png) and [selected design](Design-Selected-1920x1080.png) record the design review before Harbor integration; final native captures are in [validation](../../../Validation/TagPreparation/README.md).

Install with **Fishing Cards → UI → Tackle Apparatus → Install Tag Preparation**. The normal slot/Harbor generation path retains the same prefab, independent canvas and explicit flow binding.
