# Free local audio tooling

Earshot 1.0.0 is installed in the ignored `.local-tools/earshot/` folder, in its own
Python 3.13 environment. Unity and the game's sound assets are unchanged by this
installation. `team-audio` remains the primary sound skill; Earshot supports local
sound generation and model-assisted analysis, with `audio-design` supporting mixing.

## Local-only policy

The user requires free versions only and prohibits supplying API keys, including
existing keys. Every invocation must use `earshot_local.py` or the project MCP
server `deepwater-earshot-local`.

The launcher removes inherited credential environment variables without reading
their values, disables implicit Hugging Face authentication and `.netrc` lookup,
and enables offline mode. It blocks the optional hosted-critique module and its
CLI commands. The MCP server filters out every tool marked as network-using,
including `llm_critique`; the client config also disables that tool. No key or
remote service endpoint is configured. The upstream package contains optional
cloud code, and transitive dependencies may satisfy its `llm` dependency check;
that feature remains blocked by the launcher regardless of the dependency report.

Only explicit `doctor --fetch` permits public model downloads, through a direct
unauthenticated downloader. It does not upload audio, use keys or enable cloud
inference. Normal synthesis and analysis use the installed CPU models and local
files only. No CUDA runtime or paid audio service is needed.

## Commands

Run from the repository root in PowerShell:

```powershell
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py doctor
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py mcp --tools
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py make "a short soft click" -o Temp/click.wav
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py describe C:/absolute/path/clip.wav
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/check_earshot.py
```

Codex's project `.codex/config.toml` registers the same launcher over local stdio.
Reload/restart the client's MCP connection or reopen the project to discover it.
It has 19 local tools, a 60-second startup deadline and a 300-second tool deadline
for CPU analysis. No login or API key is required.

The local listening models are CLAP `laion/larger_clap_general` at
`16c8cc3159a3c8a31e8ff5ef1f66b0d9ab3667db` and AudioSet AST
`MIT/ast-finetuned-audioset-10-10-0.4593` at
`f826b80d28226b62986cc218e5cec390b1096902`, using safetensors. Their file hashes
are in `.local-tools/earshot/models.json`. Public configs/tokenizers and model
weights are kept in plain local folders, so inference requires no Hub connection.

## Sources and verification

- [Earshot](https://github.com/Leonxlnx/earshot), MIT-licensed, pinned source revision
  `7a7f7c5c70d4a763a1eca4b3666af7c77e313f97`. Package source and license are
  retained under `.local-tools/earshot/source/`.
- CPU-only PyTorch and the `listen`, `mcp` and `images` extras are installed. The
  optional hosted API is not enabled. Installed dependencies can be inspected with
  this isolated environment's `pip list` without changing the system Python.
- FFmpeg/FFprobe come from [Gyan's public FFmpeg builds](https://github.com/GyanD/codexffmpeg/releases),
  a distributor listed by [FFmpeg](https://ffmpeg.org/download.html). Their archive
  digest was verified against release metadata. The source URL/hash are in
  `.local-tools/earshot/ffmpeg.json`; the license is beside the executables.
- `check_earshot.py` verifies the actual stdio handshake, local-only tool list,
  disabled hosted import, absent keys, offline CPU model loading, generation,
  measurement and audio classification. Its report is stored in
  `.local-tools/earshot/verification.json`; disposable test audio stays outside Assets.

Model classifications and comparisons provide evidence about sound categories,
timing and signal properties. They do not replace a person's judgment of emotional
fit or satisfying game feel. No creative sound selection is approved by this install.
