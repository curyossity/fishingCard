# Unity MCP setup

Installed for this workspace on 2026-10-16 from [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp), release **v10.3.0** (commit `aa5fc638d623f56178d50329d9ad8c541a57fe66`).

- Unity package: `com.coplaydev.unity-mcp` in `Packages/manifest.json`, pinned to the release's Git package path. Unity Package Manager has fetched it into the package cache and recorded the verified release commit in `Packages/packages-lock.json`, including its Newtonsoft JSON dependency.
- Local server: `.local-tools/unity-mcp/venv/Scripts/mcp-for-unity.exe`, installed with Python 3.13.6 and uv 0.13.1. The server is a normal installed wheel, independent of the temporary source checkout. Binaries, environment, and cache are ignored by Git.
- MCP endpoint: `http://127.0.0.1:8080/mcp`; local loopback only. Default instance selection is `fishingCards`.
- Codex: `[mcp_servers.unityMCP]` in `C:/Users/user/.codex/config.toml`, with a 60-second startup timeout and 120-second tool timeout. Existing settings were preserved; backup: `config.toml.before-unity-mcp-20261016.bak` in the same folder.
- Logs: `Logs/UnityMcp/`. Telemetry is disabled for the local server and the Editor integration.

## Using the integration

1. Keep this project open in Unity 6000.5.9f1. Package resolution and compilation completed during installation, and the Editor connected successfully.
2. `UnityMcpSetup` automatically starts/reuses the installed local server and connects the package's HTTP bridge after import or domain reload. To retry manually, use **Fishing Cards → Tools → Connect Unity MCP**. The upstream package UI is under **Window → MCP for Unity**.
3. Restart Codex so it loads the newly registered server and tools.

Successful Editor connection writes `Library/UnityMcpSetup.json`. The Python server's `/api/instances` endpoint lists connected Editors. After this connection, select `fishingCards` when multiple Unity projects are open.

The setup helper is Editor-only. It uses the package's configuration/bridge APIs and does not alter gameplay assets or scenes. It runs automatically only when the local server executable exists; another checkout without the local Python installation is unaffected. Do not replace existing MCP configurations with an upstream "configure all clients" command unless those other clients also need configuration.

## Verification

The installed server reported v10.3.0, passed an MCP initialization handshake, and listed 48 tools after the Editor connected. The Unity Git dependency resolved to the expected release commit, and both package assemblies compiled. The server discovered `fishingCards` on Unity 6000.5.9f1, and a read-only `mcpforunity://editor/state` request returned `success: true`, verifying the full MCP-to-Editor connection. The test client logged a non-fatal session-close warning after successful requests. See [installation check](Validation/UnityMcp/installation-check.json).

The project Editor setup helper also passed compilation against Unity reference assemblies. No scene changes or gameplay mutations were used for verification.
