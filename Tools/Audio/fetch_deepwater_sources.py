"""Fetch selected public CC0 instrument recordings without authentication.

Explicit installation step only. Normal audio builds are completely offline.
Earshot's public-domain note credits identify these recordings; this keeps the
actual recorded attacks and decays instead of relying on spectral reconstruction.
"""
import hashlib
import io
import json
import sys
import zipfile
from pathlib import Path
from urllib.parse import quote
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / "Assets/Editor/AudioSources/Deepwater/Recordings"


def fetch(url):
    # Fixed public sources only; no credentials, account configuration or auth headers.
    return urlopen(Request(url, headers={"User-Agent": "Deepwater-CC0-source-audit"}), timeout=45).read()


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    libraries = {}
    for repo in ["VCSL", "VSCO-2-CE"]:
        data = json.loads(fetch(f"https://api.github.com/repos/sgossner/{repo}/commits/master"))
        libraries[repo] = data["sha"]
        license_data = fetch(f"https://raw.githubusercontent.com/sgossner/{repo}/{data['sha']}/LICENSE")
        (DEST / f"{repo}-LICENSE.txt").write_bytes(license_data)
    recordings = []
    for note, midi in [("D4", 62), ("F4", 65), ("A4", 69), ("C5", 72), ("G5", 79)]:
        recordings.append((f"harp_{midi}.wav", "VCSL", f"Chordophones/Composite Chordophones/Concert Harp/KSHarp_{note}_mf1.wav", "harp", midi))
    recordings.append(("pizzicato_50.wav", "VSCO-2-CE", "Strings/Cello Section/pizzT/pizzT_D2_v1_RR1.wav", "pizzicato", 50))
    manifest = []
    for filename, repo, path, voice, midi in recordings:
        url = f"https://raw.githubusercontent.com/sgossner/{repo}/{libraries[repo]}/" + quote(path, safe="/")
        audio = fetch(url)
        if audio[:4] != b"RIFF":
            raise ValueError("Expected a WAV recording: " + url)
        (DEST / filename).write_bytes(audio)
        manifest.append(dict(file=filename, voice=voice, midi=midi, url=url, license="CC0-1.0", sha256=hashlib.sha256(audio).hexdigest()))
    (DEST / "sources.json").write_text(json.dumps(dict(revisions=libraries, recordings=manifest), indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(downloaded=len(manifest), bytes=sum((DEST / x["file"]).stat().st_size for x in manifest))))


def cards():
    """Retains selected original card recordings from Kenney's own CC0 upload."""
    url = "https://opengameart.org/sites/default/files/kenney_casino-audio.zip"
    cache = ROOT / ".local-tools/earshot/kenney_casino-audio.zip"
    archive = cache.read_bytes() if cache.exists() and zipfile.is_zipfile(cache) else fetch(url)
    source = zipfile.ZipFile(io.BytesIO(archive)); manifest = []
    mapping = {f"card_{i}": f"card-place-{i}.ogg" for i in range(1, 5)}
    mapping.update({f"slide_{i}": f"card-slide-{i}.ogg" for i in range(1, 4)})
    mapping["shuffle"] = "card-shuffle.ogg"
    for voice, filename in mapping.items():
        audio = source.read("Audio/" + filename); (DEST / filename).write_bytes(audio)
        manifest.append(dict(file=filename, voice=voice, url=url, archiveMember="Audio/" + filename,
                             author="Kenney.nl", license="CC0-1.0", sha256=hashlib.sha256(audio).hexdigest()))
    (DEST / "Kenney-Casino-LICENSE.txt").write_bytes(source.read("License.txt"))
    (DEST / "cards.json").write_text(json.dumps(dict(sourcePage="https://opengameart.org/node/12742", archiveSha256=hashlib.sha256(archive).hexdigest(), recordings=manifest), indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(cardRecordings=len(manifest))))


if __name__ == "__main__":
    cards() if "--cards" in sys.argv else main()
