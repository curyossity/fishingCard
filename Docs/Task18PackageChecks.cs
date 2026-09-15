using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using TMPro;

public static partial class Task17VisualChecks
{
    /// <summary>Imports local TMP essentials into the disposable validation project.</summary>
    public static void PrepareTmp()
    {
        if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            EditorApplication.Exit(0);
            return;
        }
        AssetDatabase.importPackageCompleted += _ => EditorApplication.Exit(0);
        AssetDatabase.importPackageCancelled += _ => EditorApplication.Exit(1);
        AssetDatabase.importPackageFailed += (_, error) => { Debug.LogError(error); EditorApplication.Exit(1); };
        TMP_PackageResourceImporter.ImportResources(true, false, false);
    }

    /// <summary>Renders package references from existing sprites, never from generated replacement artwork.</summary>
    public static void RunPackage()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            output = args[Array.IndexOf(args, "-validationOutput") + 1];
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string reference in Directory.GetFiles("Assets/ReferenceChecks", "*.png"))
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(reference.Replace('\\', '/'));
                if (importer.textureType != TextureImporterType.Default || importer.mipmapEnabled ||
                    importer.maxTextureSize != 4096 || importer.textureCompression != TextureImporterCompression.Uncompressed)
                    throw new InvalidOperationException("Invalid reference import settings: " + reference);
            }
            string[] paths = Directory.GetFiles("Assets/FishingUIAssets", "*.png", SearchOption.AllDirectories)
                .Select(path => path.Replace('\\', '/').Substring("Assets/FishingUIAssets/".Length)).OrderBy(path => path).ToArray();
            CaptureContactSheet(paths);
            CapturePackageSlices(paths);
            CaptureStates();
            CaptureCardStates();
            CaptureTiles();
            CaptureAlphaEdges(paths);
            CaptureTypography(1920, 1080);
            CaptureTypography(1366, 768);
            CaptureBlankLayout();
            StringBuilder inventory = new StringBuilder("# Imported Package Inventory\n\n| File | Texture size | Sprite rect | Border | PPU | Filter | Mipmaps |\n| --- | --- | --- | --- | --- | --- | --- |\n");
            foreach (string path in paths)
            {
                Sprite sprite = Load(path);
                inventory.AppendLine("| " + path + " | " + sprite.texture.width + "x" + sprite.texture.height + " | " + sprite.rect + " | " + sprite.border + " | " + sprite.pixelsPerUnit + " | " + sprite.texture.filterMode + " | " + sprite.texture.mipmapCount + " | ");
            }
            File.WriteAllText(Path.Combine(output, "imported-inventory.md"), inventory.ToString());
            File.WriteAllText(Path.Combine(output, "result.json"), "{\"unity\":\"" + Application.unityVersion + "\",\"assets\":" + paths.Length + ",\"captures\":" + captures + ",\"visualReview\":\"required\"}");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            if (!string.IsNullOrEmpty(output)) File.WriteAllText(Path.Combine(output, "failure.txt"), error.ToString());
            EditorApplication.Exit(1);
        }
    }

    /// <summary>Shows card interaction frames over the actual blank card surface.</summary>
    private static void CaptureCardStates()
    {
        Begin(1600, 760);
        Label("CARD STATES - actual base and independent overlays", new Rect(24, 8, 1500, 38), Ivory, 24);
        string[] labels = { "normal", "hover", "selected", "selected + focus" };
        for (int i = 0; i < labels.Length; i++)
        {
            int x = 28 + i * 390;
            Label(labels[i], new Rect(x, 52, 350, 30), Ivory, 18);
            Rect card = new Rect(x + 42, 92, 270, 405);
            Draw("Cards/Creature/creature-card-base.png", card);
            Draw("Frames/creature-card-frame-9slice.png", card, 1, true);
            if (i == 1) Draw("Controls/card-hover-frame-9slice.png", card, 1, true);
            if (i >= 2) Draw("Controls/card-selected-frame-9slice.png", card, 1, true);
            if (i == 3) Draw("Controls/controller-focus-frame-9slice.png",
                new Rect(card.x - 8, card.y - 8, card.width + 16, card.height + 16), 1, true);
        }
        Save("card-state-contrast.png");
    }

    /// <summary>Renders all material tiles through a two-by-two UV repeat.</summary>
    private static void CaptureTiles()
    {
        Begin(1600, 900);
        string[] paths = {
            "Backgrounds/teal-paper-tile.png", "Backgrounds/ivory-paper-tile.png",
            "Frames/oxidized-teal-tile.png", "Frames/aged-brass-tile.png"
        };
        for (int i = 0; i < paths.Length; i++)
        {
            int x = 24 + i % 2 * 790;
            int y = 20 + i / 2 * 430;
            Label(paths[i] + " - 2x2 repeat", new Rect(x, y, 740, 28), Ivory, 18);
            Raw(path: paths[i], rect: new Rect(x, y + 36, 740, 360), uv: new Rect(0, 0, 2, 2));
        }
        Save("material-tiles-2x2.png");
    }

    /// <summary>Renders an alpha-transition crop for every transparent source on teal and ivory.</summary>
    private static void CaptureAlphaEdges(string[] paths)
    {
        var samples = paths.Select(path => new { path, sample = FindAlphaEdge(path) })
            .Where(item => item.sample.HasValue).ToArray();
        const int rowsPerPage = 8;
        for (int page = 0; page * rowsPerPage < samples.Length; page++)
        {
            Begin(1600, 1200);
            Label("ALPHA EDGES AT HIGH ZOOM - teal / ivory", new Rect(20, 2, 1500, 34), Ivory, 22);
            for (int row = 0; row < rowsPerPage && page * rowsPerPage + row < samples.Length; row++)
            {
                var item = samples[page * rowsPerPage + row];
                int y = 42 + row * 144;
                Label(item.path, new Rect(18, y, 610, 28), Ivory, 15);
                Rect uv = item.sample.Value;
                Background(new Rect(650, y, 420, 126), Teal);
                Raw(item.path, new Rect(650, y, 420, 126), uv);
                Background(new Rect(1120, y, 420, 126), Ivory);
                Raw(item.path, new Rect(1120, y, 420, 126), uv);
            }
            Save("alpha-edges-" + page + ".png");
        }
    }

    /// <summary>Finds a source alpha transition and returns a 64-pixel UV crop around it.</summary>
    private static Rect? FindAlphaEdge(string path)
    {
        Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        decoded.LoadImage(File.ReadAllBytes("Assets/FishingUIAssets/" + path));
        Color32[] pixels = decoded.GetPixels32();
        int found = -1;
        int bestGradient = 0;
        for (int y = 1; y < decoded.height - 1; y++)
            for (int x = 1; x < decoded.width - 1; x++)
            {
                int index = y * decoded.width + x;
                int alpha = pixels[index].a;
                int gradient = Math.Max(
                    Math.Max(Math.Abs(alpha - pixels[index - 1].a), Math.Abs(alpha - pixels[index + 1].a)),
                    Math.Max(Math.Abs(alpha - pixels[index - decoded.width].a), Math.Abs(alpha - pixels[index + decoded.width].a)));
                if (gradient > bestGradient) { bestGradient = gradient; found = index; }
            }
        if (found < 0) { UnityEngine.Object.DestroyImmediate(decoded); return null; }
        int px = found % decoded.width;
        int py = found / decoded.width;
        const int sampleWidth = 192;
        const int sampleHeight = 64;
        float xMin = Mathf.Clamp(px - sampleWidth / 2, 0, decoded.width - sampleWidth);
        float yMin = Mathf.Clamp(py - sampleHeight / 2, 0, decoded.height - sampleHeight);
        Rect uv = new Rect(xMin / decoded.width, yMin / decoded.height,
            Mathf.Min(sampleWidth, decoded.width) / (float)decoded.width,
            Mathf.Min(sampleHeight, decoded.height) / (float)decoded.height);
        UnityEngine.Object.DestroyImmediate(decoded);
        return uv;
    }

    /// <summary>Draws an imported texture with an explicit UV window.</summary>
    private static RawImage Raw(string path, Rect rect, Rect uv)
    {
        RawImage image = new GameObject(path, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage)).GetComponent<RawImage>();
        Place(image.gameObject, rect);
        image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FishingUIAssets/" + path);
        image.uvRect = uv;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>Checks the bundled display and body fonts in representative runtime fields.</summary>
    private static void CaptureTypography(int width, int height)
    {
        Begin(width, height);
        float scale = width / 1920f;
        TMP_FontAsset display = CreateFont("Fonts/Marcellus/Marcellus-Regular.ttf");
        TMP_FontAsset body = CreateFont("Fonts/SourceSerif4/SourceSerif4-Variable.ttf");
        Draw("Backgrounds/gameplay-tabletop-16x9.png", new Rect(0, 0, width, height));
        TMPLine("ABYSSAL LANTERNFISH", new Rect(90 * scale, 80 * scale, 620 * scale, 90 * scale),
            display, 64 * scale, 26 * scale, Ivory);
        TMPLine("CATCH RIG", new Rect(90 * scale, 205 * scale, 410 * scale, 58 * scale),
            display, 38 * scale, 20 * scale, Ivory);
        TMPLine("DESCEND", new Rect(90 * scale, 300 * scale, 360 * scale, 90 * scale),
            display, 46 * scale, 20 * scale, Ivory);
        TMPLine("128", new Rect(800 * scale, 80 * scale, 152 * scale, 70 * scale),
            body, 58 * scale, 24 * scale, Ivory, FontStyles.Bold);
        TMPLine("Reduce line load by 2. Draw 1 technique.", new Rect(800 * scale, 205 * scale, 330 * scale, 130 * scale),
            body, 26 * scale, 14 * scale, Ivory);
        TMPLine("Tension: warning", new Rect(800 * scale, 370 * scale, 330 * scale, 46 * scale),
            body, 22 * scale, 14 * scale, Ivory);
        Label(width + " x " + height + " actual bundled-font fit", new Rect(90 * scale, 470 * scale, 900 * scale, 40 * scale), Ivory, Mathf.Max(12, Mathf.RoundToInt(20 * scale)));
        Save("typography-" + width + "x" + height + ".png");
        UnityEngine.Object.DestroyImmediate(display);
        UnityEngine.Object.DestroyImmediate(body);
    }

    /// <summary>Creates a transient dynamic TMP asset from the bundled source font.</summary>
    private static TMP_FontAsset CreateFont(string path)
    {
        EnsureTmpSettings();
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/FishingUIAssets/" + path);
        if (font == null) throw new InvalidOperationException("Font failed to import: " + path);
        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);
        if (asset == null) throw new InvalidOperationException("TMP font creation failed: " + path);
        asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        return asset;
    }

    /// <summary>Provides the minimal TMP settings object required by an isolated validation project.</summary>
    private static void EnsureTmpSettings()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null ||
            Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
            throw new InvalidOperationException("TMP Essential Resources failed to import in the validation project.");
    }

    /// <summary>Adds auto-sized TMP text and fails if representative content is clipped.</summary>
    private static void TMPLine(string value, Rect rect, TMP_FontAsset font, float maximum, float minimum,
        Color color, FontStyles style = FontStyles.Normal)
    {
        TextMeshProUGUI label = new GameObject("TMP fit check", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        Place(label.gameObject, rect);
        label.font = font;
        label.text = value;
        label.fontStyle = style;
        label.color = color;
        label.fontSizeMax = maximum;
        label.fontSizeMin = minimum;
        label.enableAutoSizing = true;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Overflow;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.ForceMeshUpdate();
        if (label.isTextOverflowing) throw new InvalidOperationException("TMP overflow: " + value);
    }

    /// <summary>Shows every runtime PNG separately, retaining aspect and identifying masks.</summary>
    private static void CaptureContactSheet(string[] paths)
    {
        Begin(2048, ((paths.Length + 7) / 8) * 200 + 50);
        Label("STATIC UI KIT - source sprites (red masks are hidden at runtime)", new Rect(20, 0, 2000, 42), Ivory, 24);
        for (int i = 0; i < paths.Length; i++)
        {
            int x = i % 8 * 256;
            int y = i / 8 * 200 + 50;
            Background(new Rect(x + 5, y + 5, 246, 152), i % 2 == 0 ? Ivory : Teal);
            Draw(paths[i], new Rect(x + 12, y + 10, 232, 142)).preserveAspect = true;
            Label(paths[i].Replace('/', '\n'), new Rect(x + 8, y + 155, 242, 43), Ivory, 11);
        }
        Save("ui-component-contact-sheet.png");
    }

    /// <summary>Audits every sliced sprite at three sizes using orientation-aware rectangles.</summary>
    private static void CapturePackageSlices(string[] paths)
    {
        string[] sliced = paths.Where(path => Load(path).border != Vector4.zero).ToArray();
        for (int page = 0; page * 6 < sliced.Length; page++)
        {
            Begin(1600, 1200);
            for (int row = 0; row < 6 && page * 6 + row < sliced.Length; row++)
            {
                string path = sliced[page * 6 + row];
                Label(path, new Rect(20, row * 200, 1500, 28), Ivory);
                bool vertical = path.Contains("depth-track");
                for (int size = 0; size < 3; size++)
                {
                    Rect rect = vertical ? new Rect(24 + size * 500, row * 200 + 34, 32 + size * 8, 100 + size * 25)
                        : new Rect(24 + size * 500, row * 200 + 34, 220 + size * 100, 100 + size * 25);
                    Background(rect, size == 1 ? Ivory : Teal);
                    Draw(path, rect, 1, true);
                }
            }
            Save("package-slices-" + page + ".png");
        }
    }

    /// <summary>Documents real button state surfaces and independent selected/focus overlays.</summary>
    private static void CaptureStates()
    {
        Begin(1600, 1000);
        string[] names = { "normal", "hover", "pressed", "disabled", "destructive" };
        Label("INTERACTION SURFACES - actual imported assets", new Rect(24, 10, 1500, 40), Ivory, 26);
        for (int i = 0; i < names.Length; i++)
        {
            int y = 70 + i * 145;
            Label(names[i], new Rect(24, y, 170, 40), Ivory, 20);
            Draw("Controls/action-button-" + names[i] + "-9slice.png", new Rect(200, y, 370, 95), 1, true);
            Draw("Controls/action-button-" + names[i] + "-9slice.png", new Rect(620, y, 370, 95), 1, true);
            Draw("Controls/controller-focus-frame-9slice.png", new Rect(612, y - 8, 386, 111), 1, true);
        }
        Label("Focus overlay", new Rect(620, 815, 370, 30), Ivory, 20);
        Label("Card hover / selected", new Rect(1090, 70, 460, 40), Ivory, 20);
        Draw("Controls/card-hover-frame-9slice.png", new Rect(1080, 130, 190, 300), 1, true);
        Draw("Controls/card-selected-frame-9slice.png", new Rect(1330, 130, 190, 300), 1, true);
        Draw("Meters/line-load-fill-safe-9slice.png", new Rect(1080, 490, 420, 48), 1, true);
        Draw("Meters/line-load-fill-warning-9slice.png", new Rect(1080, 570, 420, 48), 1, true);
        Draw("Meters/line-load-fill-critical-9slice.png", new Rect(1080, 650, 420, 48), 1, true);
        Save("visual-states.png");
    }

    /// <summary>Assembles only the supplied blank screen shell; names, values and artwork stay absent.</summary>
    private static void CaptureBlankLayout()
    {
        Begin(1920, 1080);
        Draw("Backgrounds/gameplay-tabletop-16x9.png", new Rect(0, 0, 1920, 1080));
        Draw("Backgrounds/bathymetric-overlay.png", new Rect(0, 0, 1920, 1080), 0.15f);
        Draw("Frames/screen-border-9slice.png", new Rect(0, 0, 1920, 1080), 1, true);
        Draw("Frames/top-navigation-base-9slice.png", new Rect(48, 8, 1824, 64), 1, true);
        Draw("Frames/catch-rig-panel-9slice.png", new Rect(48, 90, 410, 810), 1, true);
        Draw("Frames/run-controls-panel-9slice.png", new Rect(1452, 90, 420, 810), 1, true);
        Draw("Frames/panel-header-strip-9slice.png", new Rect(70, 110, 360, 64), 1, true);
        Draw("Rig/main-line-segment.png", new Rect(94, 195, 9, 555));
        for (int i = 0; i < 3; i++)
        {
            int y = 195 + i * 185;
            Draw("Rig/branch-line-segment.png", new Rect(97, y + 65, 46, 6));
            Draw("Rig/attachment-clasp.png", new Rect(124, y + 46, 27, 36)).preserveAspect = true;
            Draw("Cards/CompactCatch/compact-catch-card-base.png", new Rect(152, y, 249, 166));
            Draw("Frames/compact-card-frame-9slice.png", new Rect(152, y, 249, 166), 1, true);
        }
        Draw("Rig/hook-terminal.png", new Rect(80, 744, 36, 48)).preserveAspect = true;
        Draw("Meters/line-load-track-9slice.png", new Rect(74, 827, 350, 40), 1, true);
        Rect card = new Rect(752, 100, 407, 610);
        Draw("Cards/Creature/creature-card-base.png", card);
        Draw("Frames/creature-card-frame-9slice.png", card, 1, true);
        Draw("Cards/Creature/creature-title-plate-9slice.png", new Rect(790, 146, 330, 65), 1, true);
        Draw("Cards/Creature/creature-stat-plate-9slice.png", new Rect(790, 489, 140, 65), 1, true);
        Draw("Cards/Creature/creature-stat-plate-9slice.png", new Rect(980, 489, 140, 65), 1, true);
        Draw("Cards/Creature/creature-effect-panel-9slice.png", new Rect(790, 574, 330, 82), 1, true);
        Draw("Frames/technique-hand-tray-9slice.png", new Rect(485, 732, 935, 308), 1, true);
        for (int i = 0; i < 4; i++)
        {
            int x = 510 + i * 225;
            Draw("Cards/Technique/technique-card-base.png", new Rect(x, 750, 187, 280));
            Draw("Frames/technique-card-frame-9slice.png", new Rect(x, 750, 187, 280), 1, true);
            Draw("Cards/Technique/technique-title-plate-9slice.png", new Rect(x + 20, 778, 147, 40), 1, true);
            Draw("Cards/Technique/technique-rules-panel-9slice.png", new Rect(x + 20, 913, 147, 88), 1, true);
        }
        Draw("Meters/depth-track-9slice.png", new Rect(1486, 163, 34, 225), 1, true);
        Draw("Meters/tension-track-9slice.png", new Rect(1490, 428, 334, 32), 1, true);
        string[] actions = { "normal", "destructive", "normal" };
        for (int i = 0; i < actions.Length; i++) Draw("Controls/action-button-" + actions[i] + "-9slice.png", new Rect(1482, 506 + i * 114, 360, 90), 1, true);
        Save("gameplay-layout-blank-1920x1080.png");
    }
}
