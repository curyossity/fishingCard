# Deepwater acoustic source ledger

21 November 2026. Sources for the active `Assets/Audio/Deepwater/` family. All acquisition used public unauthenticated downloads or the guarded offline Earshot MCP. No API key, hosted generation, hosted critique, audio upload or paid tool was used.

## Active recorded material

- **Concert harp:** Versilian Studios' VCSL, CC0-1.0, original D4/F4/A4/C5/G5 notes. Natural recorded string attacks and decays; short edits and nearest recorded note transposition supply the tonal palette. `Recordings/sources.json` pins the exact Git revisions, public file URLs and SHA-256 digests. `Recordings/VCSL-LICENSE.txt` contains the license.
- **Pizzicato strings:** VSCO-2 Community Edition; recordings by Sam Gossner and Simon Dalzell, sample cutting by Elan Hickler / Soundemote. CC0-1.0. One low cello-section pizzicato recording, transposed/filtered for support. `Recordings/sources.json` identifies its source; `Recordings/VSCO-2-CE-LICENSE.txt` records the license. The upstream filename calls it D2; its MIDI 50 is scientific D3.
- **Card handling:** Kenney.nl, Casino Audio, CC0-1.0. Original `card-place-1/2/3.ogg` and `card-shuffle.ogg` supply the active edits. Four placement and three slide recordings were retained as audition candidates. [Author's upload and license](https://opengameart.org/node/12742), [official pack page](https://kenney.nl/assets/casino-audio). The official direct ZIP returned HTML; the same pack was acquired from the author's OpenGameArt upload. `Recordings/cards.json` records archive/member hashes, and `Recordings/Kenney-Casino-LICENSE.txt` preserves the supplied license. No chip/coin/dice sample was introduced as a scoring reward.

Original files stay under **Editor** and are not player assets. The game ships the edited, bounded mono PCM family and one library asset.

## Active local reconstructed percussion

Earshot 1.0.0, pinned source revision `7a7f7c5c70d4a763a1eca4b3666af7c77e313f97`, rendered through `Tools/Audio/earshot_local.py`'s MCP server:

| Design | Generated files | Active role |
| --- | --- | --- |
| `rim/click`, three takes | `rim_1/2/3.wav` | Cross-stick contact support; never a loud machine ratchet |
| `shaker/tick`, three takes | `shaker_1/2/3.wav` | Soft motion bed and symbol crossings |
| `kick/soft`, one take | `kick.wav` | Low support for launch and major contacts |

These are **local reconstructions from CC0 recording models**, rather than original recorded files. `Percussion Credits.md` preserves the source families credited by Earshot (Karoryfer Big Rusty Drums and Versilian Studios VCSL). `percussion.json` hashes the rendered stems. Earshot's MIT license is included as `Earshot-LICENSE.txt`.

## Discarded candidates and limits

The loose `harp.wav`, `pizzicato.wav`, `marimba.wav`, `brush_*.wav`, `card_*.wav` and `shuffle.wav` files are initial Earshot candidates, **not active builder sources**. Brush reconstruction was rejected; actual instrument recordings replaced the reconstructed tonal candidates, and recorded Kenney handling replaced the procedural card designs. Earshot's `paper/card` and `cards/shuffle` are procedural physical models; their favorable realness classifications did not establish recorded provenance. No claim that every Earshot design comes from a recording is made.

Candidate classifications are recorded in `Docs/Validation/AudioDirection/`. Some tiny layered phrases still receive uncertain or synthetic classifications even with recorded sources. Provenance, engineering measurements, model inference and human listening are separate evidence.

## Rebuild

`Tools/Audio/build_deepwater_audio.py` composes the final family offline using the checked-in recorded files and selected reconstructed percussion. It performs mono downmix, source-rate resampling, leading-silence removal, contact alignment, high/low-pass filtering, nearest-note transposition, bounded layering and edge fades. It writes 48 kHz / 16-bit WAVs with peak amplitude at most 0.64, plus a hashed render manifest. Runtime cue gain, variation, priority and bus are authored in the imported sound library.

`Tools/Audio/fetch_deepwater_sources.py` is an explicit source-install/update utility. Its `--cards` mode retrieves only selected members of the CC0 card archive. Normal renders and gameplay require no network. `Tools/Audio/build_audio_review.py` builds the local listening page from the current manifest and Unity's runtime schedule exports.
