# Task 1.7 Acceptance

Reviewed 2026-09-15 in Unity 6000.5.9f1 with UGUI 2.5.0. Task 1.7's static asset checks are complete.

- Imported all 27 sprites and checked their actual Unity border values.
- Rendered all 18 icons at 32, 48 and 64 pixels against teal and ivory.
- Fixed noisy minification by enabling mipmaps and trilinear filtering on icons only. Source PNGs were not modified. Silhouettes, loss cues and warning/critical shapes remain distinguishable; use labels for meaning, especially at 32 pixels.
- Inspected five sliced surfaces at 240x130, 360x155 and 600x170, with pixelsPerUnitMultiplier = 8. Corners remain protected, straight edges are continuous and sample text remains inside the available region.
- Inspected all nine overlays on both light and dark backgrounds at the manifest's opacity settings. Transparent centers preserve text, hatches remain subordinate, and the modal source dims the content underneath.
- All six captures are nonblank. The attached result JSON is the automated result; its visualReview field deliberately requires this separate human-readable review.

Reproduce using `powershell -NoProfile -ExecutionPolicy Bypass -File "Docs/Run Task 1.7 Visual Checks.ps1"`. The script creates an isolated project under ignored Temp and writes captures under Logs; it does not open, change or save gameplay scenes. Unity licensing/cache access is required.

This accepts the static asset kit, not the future gameplay prefab, TextMeshPro integration, input behavior or full-screen responsive layout. Those remain in their owning integration tasks.
