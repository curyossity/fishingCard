using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

/// <summary>Connects this project's installed local MCP server to the Editor without coupling gameplay to tooling.</summary>
[InitializeOnLoad]
public static class UnityMcpSetup
{
    private const string BaseUrl = "http://127.0.0.1:8080";
    private static bool connecting;

    /// <summary>Reconnects installed tooling after package import or domain reload.</summary>
    static UnityMcpSetup() { EditorApplication.delayCall += AutoConnect; }

    /// <summary>Runs automatic setup only on machines where this project's local server has been installed.</summary>
    private static void AutoConnect()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        { EditorApplication.delayCall += AutoConnect; return; }
        if (File.Exists(ServerExecutable) && LocatorType != null) Connect();
    }

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    private static string ServerExecutable => Path.Combine(ProjectRoot, ".local-tools", "unity-mcp", "venv", "Scripts", "mcp-for-unity.exe");
    private static Type LocatorType => Type.GetType("MCPForUnity.Editor.Services.MCPServiceLocator, MCPForUnity.Editor");

    /// <summary>Starts the project-local server if needed and connects the package's official HTTP bridge.</summary>
    [MenuItem("Fishing Cards/Tools/Connect Unity MCP")]
    public static async void Connect()
    {
        if (connecting) return;
        connecting = true;
        try
        {
            Type locator = LocatorType;
            if (locator == null || !File.Exists(ServerExecutable))
                throw new InvalidOperationException("Unity MCP package or local Python server is not installed. See Docs/Unity MCP Setup.md.");
            using (HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
            {
                if (!await IsHealthy(http))
                {
                    System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ServerExecutable,
                        Arguments = "--transport http --http-url " + BaseUrl + " --default-instance fishingCards",
                        WorkingDirectory = ProjectRoot,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    start.EnvironmentVariables["UNITY_MCP_DISABLE_TELEMETRY"] = "true";
                    start.EnvironmentVariables["UNITY_MCP_LOG_DIR"] = Path.Combine(ProjectRoot, "Logs", "UnityMcp");
                    using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start)) { }
                    bool ready = false;
                    for (int attempt = 0; attempt < 60 && !ready; attempt++)
                    { await Task.Delay(500); ready = await IsHealthy(http); }
                    if (!ready) throw new InvalidOperationException("Local Unity MCP server did not become ready; inspect Logs/UnityMcp.");
                }
            }

            Type cacheType = Type.GetType("MCPForUnity.Editor.Services.EditorConfigurationCache, MCPForUnity.Editor", true);
            object cache = cacheType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            cacheType.GetMethod("SetUseHttpTransport").Invoke(cache, new object[] { true });
            cacheType.GetMethod("SetHttpBaseUrl").Invoke(cache, new object[] { BaseUrl });
            EditorPrefs.SetString("MCPForUnity.HttpTransportScope", "local");
            EditorPrefs.SetString("MCPForUnity.UvxPath", Path.Combine(ProjectRoot, ".local-tools", "unity-mcp", "bootstrap", "bin", "uvx.exe"));
            EditorPrefs.SetBool("MCPForUnity.TelemetryDisabled", true);
            object bridge = locator.GetProperty("Bridge", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            bool running = (bool)bridge.GetType().GetProperty("IsRunning").GetValue(bridge);
            if (!running) running = await (Task<bool>)bridge.GetType().GetMethod("StartAsync").Invoke(bridge, null);
            if (!running) throw new InvalidOperationException("Unity MCP bridge could not connect to the local server.");
            string status = Path.Combine(ProjectRoot, "Library", "UnityMcpSetup.json");
            File.WriteAllText(status, "{\"connected\":true,\"url\":\"" + BaseUrl + "/mcp\",\"utc\":\"" + DateTime.UtcNow.ToString("o") + "\"}");
            Debug.Log("Unity MCP connected for fishingCards at " + BaseUrl + "/mcp.");
        }
        catch (Exception error) { Debug.LogWarning("Unity MCP setup: " + error.GetBaseException().Message); }
        finally { connecting = false; }
    }

    /// <summary>Recognizes the expected service before reusing a listener on the local MCP port.</summary>
    private static async Task<bool> IsHealthy(HttpClient http)
    {
        try
        {
            string response = await http.GetStringAsync(BaseUrl + "/health");
            if (!response.Contains("MCP for Unity server is running"))
                throw new InvalidOperationException("Port 8080 belongs to a different service.");
            return response.Contains("healthy");
        }
        catch (HttpRequestException) { return false; }
        catch (TaskCanceledException) { return false; }
    }
}
