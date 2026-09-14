using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Isolated source-asset rendering checks; never opens or saves gameplay scenes.</summary>
public static class Task17VisualChecks
{
    [Serializable] private sealed class Record { public Entry[] assets; }
    [Serializable] private sealed class Entry
    {
        public string path;
        public string kind;
        public float initialOpacity;
        public int[] border;
    }

    private static readonly Color Teal = new Color32(23, 61, 64, 255);
    private static readonly Color Ivory = new Color32(238, 229, 204, 255);
    private static Canvas canvas;
    private static Camera camera;
    private static RenderTexture target;
    private static string output;
    private static int captures;

    /// <summary>Imports the exact source metadata, captures UI sheets, and reports failures.</summary>
    public static void Run()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            output = args[Array.IndexOf(args, "-validationOutput") + 1];
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Record record = JsonUtility.FromJson<Record>(File.ReadAllText("Assets/record.json"));
            if (record.assets.Length != 27) throw new Exception("Expected 27 assets.");
            foreach (Entry entry in record.assets)
            {
                Sprite sprite = Load(entry.path);
                Vector4 expected = new Vector4(entry.border[0], entry.border[1], entry.border[2], entry.border[3]);
                if (sprite.border != expected) throw new Exception("Imported border mismatch: " + entry.path);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/FishingUIAssets/" + entry.path);
                if (importer.mipmapEnabled != (entry.kind == "icon")) throw new Exception("Mipmap setting mismatch: " + entry.path);
                FilterMode filter = entry.kind == "icon" ? FilterMode.Trilinear : FilterMode.Bilinear;
                if (importer.filterMode != filter) throw new Exception("Filter setting mismatch: " + entry.path);
            }
            foreach (int size in new[] { 32, 48, 64 }) CaptureIcons(record, size);
            CaptureSlices(record);
            CaptureComposites(record, false);
            CaptureComposites(record, true);
            File.WriteAllText(Path.Combine(output, "result.json"), "{\"unity\":\"" + Application.unityVersion + "\",\"assets\":27,\"captures\":" + captures + ",\"importAndRenderChecks\":\"passed\",\"visualReview\":\"required\"}");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            if (!string.IsNullOrEmpty(output)) File.WriteAllText(Path.Combine(output, "failure.txt"), error.ToString());
            EditorApplication.Exit(1);
        }
    }

    /// <summary>Loads a real imported Sprite and rejects missing imports.</summary>
    private static Sprite Load(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FishingUIAssets/" + path);
        if (sprite == null) throw new Exception("Missing Sprite: " + path);
        return sprite;
    }

    /// <summary>Creates an offscreen camera canvas with one UI unit per output pixel.</summary>
    private static void Begin(int width, int height)
    {
        target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        target.Create();
        camera = new GameObject("Validation camera", typeof(Camera)).GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Teal;
        camera.orthographic = true;
        camera.orthographicSize = height / 2f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100;
        camera.targetTexture = target;
        canvas = new GameObject("Validation canvas", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        canvas.referencePixelsPerUnit = 100;
    }

    /// <summary>Positions a validation element in top-left pixel coordinates.</summary>
    private static RectTransform Place(GameObject gameObject, Rect rect)
    {
        RectTransform transform = gameObject.GetComponent<RectTransform>();
        transform.SetParent(canvas.transform, false);
        transform.anchorMin = transform.anchorMax = new Vector2(0, 1);
        transform.pivot = new Vector2(0, 1);
        transform.anchoredPosition = new Vector2(rect.x, -rect.y);
        transform.sizeDelta = rect.size;
        return transform;
    }

    /// <summary>Draws a source sprite without modifying its pixels.</summary>
    private static Image Draw(string path, Rect rect, float opacity = 1, bool sliced = false)
    {
        Image image = new GameObject(path, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        Place(image.gameObject, rect);
        image.sprite = Load(path);
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.pixelsPerUnitMultiplier = 8;
        image.color = new Color(1, 1, 1, opacity);
        image.raycastTarget = false;
        return image;
    }

    /// <summary>Adds an untextured test background to inspect alpha on light and dark colors.</summary>
    private static void Background(Rect rect, Color color)
    {
        Image image = new GameObject("Test background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        Place(image.gameObject, rect);
        image.color = color;
    }

    /// <summary>Adds harness labels; this font is not part of the production UI.</summary>
    private static void Label(string text, Rect rect, Color color, int size = 16)
    {
        Text label = new GameObject("Test label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)).GetComponent<Text>();
        Place(label.gameObject, rect);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAnchor.MiddleLeft;
    }

    /// <summary>Compares all symbols at the same actual pixel size on both backgrounds.</summary>
    private static void CaptureIcons(Record record, int size)
    {
        Begin(1280, 800);
        Label("ICON SOURCE CHECK - " + size + " px slots", new Rect(24, 8, 1100, 40), Ivory, 22);
        Entry[] icons = record.assets.Where(entry => entry.kind == "icon").ToArray();
        for (int i = 0; i < icons.Length; i++)
        {
            int x = 24 + i % 3 * 420;
            int y = 60 + i / 3 * 120;
            Label(Path.GetFileNameWithoutExtension(icons[i].path), new Rect(x, y, 380, 25), Ivory);
            Draw(icons[i].path, new Rect(x + 12, y + 32, size, size));
            Background(new Rect(x + 120, y + 28, 110, 82), Ivory);
            Draw(icons[i].path, new Rect(x + 128, y + 32, size, size));
        }
        Save("icons-" + size + ".png");
    }

    /// <summary>Exercises minimum, reference and expanded rectangles at a fixed border scale.</summary>
    private static void CaptureSlices(Record record)
    {
        Entry[] entries = record.assets.Where(entry => entry.kind == "panel" || entry.kind == "frame").ToArray();
        Begin(1600, 1080);
        Label("SLICING - 240x130 / 360x155 / 600x170 - border multiplier 8", new Rect(24, 0, 1500, 40), Ivory, 22);
        for (int i = 0; i < entries.Length; i++)
        {
            int y = 45 + i * 205;
            Label(entries[i].path, new Rect(24, y, 1300, 25), Ivory);
            Rect[] rects = { new Rect(24, y + 28, 240, 130), new Rect(300, y + 28, 360, 155), new Rect(720, y + 28, 600, 170) };
            foreach (Rect rect in rects)
            {
                Draw(entries[i].path, rect, 1, true);
                Color textColor = entries[i].path.Contains("tooltip") ? Teal : Ivory;
                Label("Readable content 128", new Rect(rect.x + 38, rect.y + 52, rect.width - 76, 32), textColor, 16);
            }
        }
        Save("sliced-surfaces.png");
    }

    /// <summary>Composites the overlays over readable content using their documented alpha.</summary>
    private static void CaptureComposites(Record record, bool lightBackground)
    {
        Begin(1280, 800);
        Entry[] entries = record.assets.Where(entry => entry.kind != "icon").ToArray();
        for (int i = 0; i < entries.Length; i++)
        {
            int x = 20 + i % 3 * 420;
            int y = 10 + i / 3 * 260;
            Label(Path.GetFileNameWithoutExtension(entries[i].path), new Rect(x, y, 390, 26), Ivory);
            Rect rect = new Rect(x, y + 36, 390, 200);
            Background(rect, lightBackground ? Ivory : Teal);
            Color foreground = lightBackground ? Teal : Ivory;
            if (entries[i].kind == "panel") Draw(entries[i].path, rect, 1, true);
            Color contentColor = entries[i].path.Contains("summary") ? Ivory : entries[i].path.Contains("tooltip") ? Teal : foreground;
            Label("Haul value 128\nWeight removed 12\nEffect lost", new Rect(x + 44, y + 78, 300, 100), contentColor, 20);
            if (entries[i].kind != "panel") Draw(entries[i].path, rect, entries[i].initialOpacity, entries[i].kind == "frame");
        }
        Save(lightBackground ? "overlay-compositing-light.png" : "overlay-compositing-dark.png");
    }

    /// <summary>Captures actual UI rendering, rejects blank output, and releases test objects.</summary>
    private static void Save(string name)
    {
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D capture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        capture.Apply();
        Color32[] pixels = capture.GetPixels32();
        Color32 first = pixels[0];
        int changed = pixels.Count(pixel => Math.Abs(pixel.r - first.r) + Math.Abs(pixel.g - first.g) + Math.Abs(pixel.b - first.b) > 30);
        if (changed < 1000) throw new Exception("Blank capture: " + name);
        File.WriteAllBytes(Path.Combine(output, name), capture.EncodeToPNG());
        captures++;
        RenderTexture.active = previous;
        UnityEngine.Object.DestroyImmediate(capture);
        UnityEngine.Object.DestroyImmediate(canvas.gameObject);
        UnityEngine.Object.DestroyImmediate(camera.gameObject);
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
    }
}
