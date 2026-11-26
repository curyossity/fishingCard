"""Encode the Landing creature-reveal preview frames into one MP4 per rarity plus a combined sequence.

Unity writes the frames and Temp/CreatureRevealPreview/manifest.json via
Fishing Cards > Preview > Creature Reveal > Render Videos (Play mode). This script then muxes each
sequence with the frozen runtime Reveal cue using the free local ffmpeg build. Standard library only.

Run from the repository root:  python -I Tools/Rendering/encode_creature_reveal_preview.py
"""
from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
FFMPEG = ROOT / ".local-tools/earshot/ffmpeg/ffmpeg.exe"
MANIFEST = ROOT / "Temp/CreatureRevealPreview/manifest.json"


def ffmpeg(*arguments: str) -> None:
    """Runs ffmpeg without console input, failing loudly with its error output."""
    result = subprocess.run([str(FFMPEG), "-nostdin", "-hide_banner", "-loglevel", "error", "-y", *arguments],
                            cwd=ROOT, capture_output=True, text=True, timeout=300)
    if result.returncode != 0:
        sys.exit(f"ffmpeg failed: {result.stderr.strip()}")


def main() -> None:
    if not FFMPEG.exists():
        sys.exit(f"Missing local ffmpeg at {FFMPEG}")
    if not MANIFEST.exists():
        sys.exit("No frames found. In Unity Play mode run Fishing Cards > Preview > Creature Reveal > Render Videos first.")
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    fps, output = manifest["fps"], ROOT / manifest["output"]
    output.mkdir(parents=True, exist_ok=True)
    videos = []
    for clip in manifest["clips"]:
        target = output / f"Reveal-{clip['rarity']}.mp4"
        seconds = clip["count"] / fps
        # Resample to the runtime pitch, apply the cue gain, start it when the face lands, and pad with silence;
        # -shortest ends at the last video frame. MP3 audio: this ffmpeg build's native AAC encoder stalls on these buffers.
        audio = (f"[1:a]asetrate={round(clip['sampleRate'] * clip['pitch'])},aresample=48000,"
                 f"volume={clip['gainDb']}dB,adelay={clip['delayMs']}:all=1,apad[a]")
        ffmpeg("-framerate", str(fps), "-i", f"{clip['frames']}%04d.png", "-i", clip["audio"],
               "-filter_complex", audio, "-map", "0:v", "-map", "[a]", "-shortest",
               "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "16", "-r", str(fps),
               "-c:a", "libmp3lame", "-b:a", "192k", str(target))
        videos.append(target)
        print(f"{target.relative_to(ROOT)}  {seconds:.2f} s")
    listing = MANIFEST.with_name("concat.txt")
    listing.write_text("".join(f"file '{v.as_posix()}'\n" for v in videos), encoding="utf-8")
    combined = output / "Reveal-All.mp4"
    ffmpeg("-f", "concat", "-safe", "0", "-i", str(listing), "-c", "copy", str(combined))
    print(f"{combined.relative_to(ROOT)}\nOpen {(output / 'index.html').relative_to(ROOT)} to compare them side by side.")


if __name__ == "__main__":
    main()
