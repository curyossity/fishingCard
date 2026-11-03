# MFJ-SYNC-013 — Reviewed Tackle Shop

Integrated into the existing Midnight Field Journal comparison scene and Harbor prefab. R067 is completed; rejected/older shop drafts are preserved as superseded, never queued as additional imports.

The approved `GeneratedConceptOnly` composition is preserved. The original 1672 × 941 file was processed locally with the official free Real-ESRGAN x4plus model to 6688 × 3764, then reduced to **3840 × 2160**. This is super-resolution restoration, not a claim of native 4K image generation. The original remains unchanged. Fine engraving was inspected at 1:1; the whole screen was checked through actual Unity renders. No paid API/service was used. The model and portable executable remain outside Assets; [license record](../../../Licenses/RealESRGAN-ncnn-vulkan-MIT.txt).

Unity imports the restored background uncompressed, max texture size 4096, NPOT None, no mipmaps and Clamp wrapping. The same restored background replaces the preview image in both Penpot shop states. New theme art is isolated in `Assets/UIThemes/MidnightFieldJournal/Harbor/TackleShop/DesignR067/`. The background contains no baked text, cards, controls or gameplay state. Equipment frames and illustrations remain separate sprites; labels, state, effect descriptions and prices are native TMP fields.

The top Rod/Reel/Hook slots and right inspection remain fixed. The lower catalog uses a clipped native ScrollRect, wheel/drag and arrow navigation, with functional category filters. Nine visible entries reuse the three approved equipment designs three times as explicitly requested placeholders; they do not create nine separate purchases. Clicking one inspects it; buying and equipping are separate actions. Slots show their type without prices.

Equipment now supports one Rod, one Reel and one Hook at once. Equipping replaces only the same category and unequipping clears only that slot. Their existing effects operate together. A version-1 progression document migrates to a version-2 loadout in memory without resetting gold, ownership, capacity purchases or the old equipped item. The next successful transaction persists the migration in the same existing save key. Unreadable saves remain read-only. No Journal rewards or extra currency were added.

Validation includes rule/transaction/save migration checks, real pointer-driven purchase/equip and expedition benefits with isolated saves, wheel scrolling with fixed slots, filtering, and actual 4K/1080p rendering of the initial and scrolled catalog. Test keys were removed after Play Mode; normal saves were not reset. Rendering checks found and fixed text wrapping, font weight and missing arrow glyphs. The shared vector chevron is reused. The other Harbor pages and original game art are retained.

**Final verification completed on 2 November 2026:** after Unity reconnected, the capture helper passed for both initial and scrolled states at 3840 × 2160 and 1920 × 1080. Both scrolled images were visually inspected. Catalog clipping and fixed equipment-slot position checks passed, the active scene remained clean, and Unity reported zero console errors. This resolves the earlier connection-related verification gap. Both latest runtime and Editor source assemblies also compile successfully using Unity's generated response files and isolated Roslyn output.

- [Initial Unity 4K](shop-Unity4K.png)
- [Scrolled Unity 4K](shop-scrolled-Unity4K.png)
- [Initial Unity 1080p](shop-Unity1080p.png)
- [Scrolled Unity 1080p](shop-scrolled-Unity1080p.png)
- [1:1 background detail](BackgroundDetail1to1.png)
- [Verification](Verification.json)
- [Checkpoint](UnityCheckpoint.json)

The Penpot prototype still uses discrete scroll-state links; Unity implements continuous scrolling. Actual wallet/ownership captions in Unity naturally differ from the Penpot sample values.
