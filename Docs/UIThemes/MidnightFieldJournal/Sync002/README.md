# Event card integration — MFJ-SYNC-002

All five active coastal event designs are editable in Penpot and integrated in the isolated Unity comparison theme: Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot and Fresh Shell Fragments. Revisions MFJ-R020–MFJ-R024 are verified; the next revision is MFJ-R025.

[Unity gameplay4K](GameplayReefSnag4K.png) · [Hazards/Environment4K](Events14K.png) · [Opportunities4K](Events24K.png) · [Penpot roster](PenpotEvents14K.png) · [Verification](Verification.json) · [Current checkpoint](UnityCheckpoint.json)

Each face is1260×1400, matching creature geometry. The shared frame has a full-width navy rules panel with copper engraving. Each event has its own transparent illustration. Name, category, type, description and type emblem remain native UI; no gameplay information is baked into reusable art. Exact event identities, framing, title sizes and presentation-only word breaks live in SourceAssetCatalog.json. Dynamic rules still come from the unchanged CardDefinition, including native highlight markup. Central event mappings never replace creature/compact artwork.

`JournalEventDesignSync` imports hash-verified images and rebinds the mappings during ordinary theme regeneration. `JournalCreatureCardVisuals` resets event/creature decoration and text layout when the same view is reused. `JournalEventEmblem` draws resolution-independent category symbols. The existing CreatureCardView still owns masking, card content and role input. Runtime event timing/cancellation and all gameplay definitions remain unchanged.

**Validation: 950 assertions passed.** Five-event fit/content/lifecycle checks, all30encounter designs, saved comparison scene, actual controller event cancellation and pre-action resolution, Surface/results/restart, and actual output/raycast checks at3840×2160,1920×1080,3440×1440,1920×1200. Native4K and1080p event captures were inspected. This is Editor/Play Mode evidence; the prior standalone build blocker was not retried. The gameplay screenshot is an in-memory test fixture using existing definitions, not a changed starter configuration.

Preservation: all changes under Assets are inside Assets/UIThemes/MidnightFieldJournal. Original scenes/build settings, shared gameplay/card data, fonts, existing GUIDs, creature illustrations, Catch Chain and Technique sources are preserved. No old assets were deleted. [Before hashes](Before.json), [changed paths](Verification.json), [source hashes](ImportedSources.json). Original event template backup and source catalog backup are retained here.

Source boundary: the frame is1190×1322, displayed1260×1400 (approximately6% enlargement, matching the accepted creature frame). Event illustrations1672×941 exceed their1060×596.57display footprint. Runtime text and symbols are native; the captured output is4K, but the raster sources are not individually4K. No cropped/sliced mockup pieces or new paid dependencies were used.

Remaining missing central/Technique identities:11; compact missing identities:20. Future selective synchronization must compare against this checkpoint while retaining both prior checkpoints.
