# Slot-machine sound/visual alignment

21 November 2026. Timing correction to the selected Charged Cabinet palette. No cue WAV, gain, score rule, random roll, reel trajectory or input gate was changed.

The previous contact sounds waited for `IsSpinning` to become false, after the elastic recovery was already over. With the current 34% recovery fraction, the visible brake contacts are at approximately **0.693 / 0.792 / 0.891 seconds**, while complete settlement remains **1.05 / 1.20 / 1.35 seconds**. This put the old contacts 357–459 ms after the primary braking action.

## Corrected

- `TackleReelAnimator` exposes remaining time to its existing brake/catch transition and final settlement, plus the normalized derivative of forward travel. Its actual animation is unchanged.
- Contacts are DSP-scheduled up to 60 ms ahead of the visible brake/catch. The bounce no longer creates extra symbol ticks.
- Rolling pitch and gain follow forward velocity, ending during elastic recovery rather than continuing uniformly until all tail motion ends.
- The pair/triple cue meets final settlement and result-lamp activation. The previous 90 ms post-settlement delay is removed; the earlier contact now naturally leaves space before the result.
- Locked reels remain silent. An update hitch drops contacts more than 80 ms overdue rather than playing a late burst. Cancellation stops queued contacts and results. Mute/unmute retains visual progress without replaying contacts that already passed.

## Evidence

`TimingVerification.json` and `FrameTrace.csv` come from an isolated clone of the actual Discover prefab during Play mode. The original run was left running; the test cabinet's canvas/input and voices were disabled/muted so it did not take focus, play over the game, or issue gameplay commands.

The audit observes the rendered reel phase's first direction reversal independently of the brake flag and compares its frame bracket with the requested DSP contact deadline. It covers three moving reels, a stationary locked reel, quiet recovery, the final result deadline, a simulated long frame without catch-up contacts and cancellation after scheduling. The completed run passed 116 assertions, including per-frame quiet-recovery checks; the count varies with frame rate.

`MuteVerification.json` records 11 additional passing checks from `Tools/Audio/SlotAudioMuteAudit.cs.txt`. They exercise mute immediately before and after scheduled contact/result deadlines on the isolated cabinet, without changing the shared sound preference: future cues are cancelled and may be scheduled again before their visual event, while deadlines already consumed by the audio clock cannot replay between visual updates. The existing asset validation also passed all 38 checks.

`Tools/Audio/SlotAudioSyncAudit.cs.txt` is an executable Unity MCP method body. It warms the cloned materials/fonts and waits for stable frames before measurement, so the in-memory test compilation's one-off long frame is not mistaken for a normal spin. Hardware/output latency and display buffering remain device-dependent. This validates event timing, not human listening or device loopback audio.

The installed bank's source hashes remain unchanged. The current authored Discover storyboards now derive contacts and result timing from the actual Discover prefab's animation parameters. Historical A/B clips remain the selected timbre reference, not the latest Discover synchronization record.

Unity's existing URP Blitter/Volume initialization errors were present during the surrounding Play session; their stacks originate in the render pipeline and Journal technique-hand camera initialization. They were not used as evidence of an audio failure and were not changed by this timing fix.
