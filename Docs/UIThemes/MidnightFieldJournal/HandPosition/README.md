# Technique hand position

Lowered the hand48 pixels at3840×2160 (24 pixels at1080p), from(980,1294) to(980,1342). Size remains2500×866. The complete projection surface and hit mapping move together; fan and hover code are unchanged. Sidebar pile counts retain their previous screen position.

[4K preview](Gameplay4K.png) · [1080p preview](Gameplay1080p.png) · [Verification](Verification.json). Both renders were inspected; resting cards remain visible without bottom-edge clipping. The HUD builder and canvas changed; routine template regeneration also reserialized the central card prefab. The rendered central design is unchanged. Gameplay, scene, artwork and fan/runtime code retain their hashes. This is a Unity-only refinement, with no pending Penpot import. Current baseline:[MFJ-SYNC-005.2](UnityCheckpoint.json).
