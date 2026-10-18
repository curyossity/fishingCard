using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Imports audited theme resources and creates isolated fonts/configuration without editing a scene.</summary>
public static class MidnightFieldJournalAssetSetup
{
    public const string Root = "Assets/UIThemes/MidnightFieldJournal/";
    public const string ThemePath = Root + "Profiles/MidnightFieldJournalTheme.asset";
    private const string Work = "Temp/MidnightFieldJournal/";
    private const string Evidence = "Docs/UIThemes/MidnightFieldJournal/";

    [Serializable] public sealed class Catalog { public Source[] sprites; public string[] unillustratedCardIds; }
    [Serializable] public sealed class Source
    {
        public string id, path, cardUniqueId, sha256;
        public int width, height, alphaMin, alphaMax;
        public int[] visibleBoundsPixels, halfAlphaBoundsPixels;
        public float designWidth, designHeight;
    }
    [Serializable] private sealed class MoveRecord { public string from, to, guid; }
    [Serializable] private sealed class MoveReport { public List<MoveRecord> moves = new List<MoveRecord>(); }
    [Serializable] private sealed class FontRecord { public string path, family, style, unsupported; public int glyphs, kerningPairs; public bool dynamic; }
    [Serializable] private sealed class FontReport { public List<FontRecord> fonts = new List<FontRecord>(); }

    /// <summary>Moves only candidate-theme drafts with their GUIDs, then copies verified staged originals.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Prepare Isolated Assets")]
    public static void PrepareAssets()
    {
        RequireEditMode();
        Catalog catalog = LoadCatalog();
        foreach (Source item in catalog.sprites) VerifyHash(File.Exists(item.path) ? item.path : Work + "Staging/Images/" + item.id + ".png", item.sha256);
        MoveReport report = File.Exists(Evidence + "UI02Migration.json")
            ? JsonUtility.FromJson<MoveReport>(File.ReadAllText(Evidence + "UI02Migration.json")) : new MoveReport();
        EnsureFolder(Root + "Art/Archive");
        EnsureFolder(Root + "Profiles");
        EnsureFolder(Root + "Fonts/Sources");
        EnsureFolder(Root + "Runtime");
        // AssetDatabase defers moved GUID/path visibility during StartAssetEditing. Validate each move immediately instead.
        {
            Move("Assets/FishingUIAssets/MidnightFieldJournal", Root + "Art/Archive/Draft", report);
            Move("Assets/Prefabs/UI/MidnightFieldJournal", Root + "Prefabs", report);
            foreach (Source item in catalog.sprites)
            {
                EnsureFolder(Path.GetDirectoryName(item.path).Replace('\\', '/'));
                string draftName = item.id == "Background" ? "Background" : item.id;
                Move(Root + "Art/Archive/Draft/" + draftName + ".png", item.path, report);
                if (!File.Exists(item.path)) File.Copy(Work + "Staging/Images/" + item.id + ".png", item.path);
                VerifyHash(item.path, item.sha256);
            }
            Move(Root + "Art/Archive/Draft/CreatureVisualProfile.asset", Root + "Profiles/CreatureVisualProfile.asset", report);
            Move("Assets/Editor/MidnightFieldJournalPrefabBuilder.cs", Root + "Editor/MidnightFieldJournalPrefabBuilder.cs", report);
            Move("Assets/Scripts/LineLoadMeterView.cs", Root + "Runtime/LineLoadMeterView.cs", report);
            foreach (string file in Directory.Exists(Work + "Staging/Fonts") ? Directory.GetFiles(Work + "Staging/Fonts") : Array.Empty<string>())
            {
                if (Path.GetFileName(file) == "Cinzel-Variable.ttf") continue;
                CopyVerified(file, Root + "Fonts/Sources/" + Path.GetFileName(file));
            }
            CopyStagedIfPresent(Work + "FontProvenance.json", Root + "Fonts/FontProvenance.json");
            CopyStagedIfPresent(Work + "SourceCatalog.json", Root + "Profiles/SourceAssetCatalog.json");
            foreach (string file in new[] { "Cinzel-Regular.ttf", "Cinzel-SemiBold.ttf", "Cinzel-Bold.ttf", "SourceSerif4-Regular.ttf", "SourceSerif4-Semibold.ttf", "SourceSerif4-Bold.ttf", "Cinzel-OFL.txt", "SourceSerif4-OFL.txt" })
                if (!File.Exists(Root + "Fonts/Sources/" + file)) throw new FileNotFoundException("Theme font source or license is missing.", file);
        }
        File.WriteAllText(Evidence + "UI02Migration.json", JsonUtility.ToJson(report, true));
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (Source item in catalog.sprites) ConfigureSprite(item);
        Debug.Log("Midnight Field Journal: imported 20 audited images and preserved candidate draft GUIDs. No scene installed.");
    }

    /// <summary>Creates static Latin SDF fonts, bounded dynamic fallbacks and stable visual-resource mappings.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Build Isolated Fonts And Theme")]
    public static void BuildThemeAndFonts()
    {
        RequireEditMode();
        EnsureFolder(Root + "Fonts/TMP");
        string glyphs = RequiredCharacters();
        FontReport report = new FontReport();
        TMP_FontAsset bodyFallback = FontAsset("SourceSerif4-Regular", "Body Fallback", glyphs, true, report);
        TMP_FontAsset displayFallback = FontAsset("Cinzel-Regular", "Display Fallback", glyphs, true, report);
        displayFallback.fallbackFontAssetTable = new List<TMP_FontAsset> { bodyFallback };
        TMP_FontAsset body = FontAsset("SourceSerif4-Regular", "Source Serif 4 Regular", glyphs, false, report);
        TMP_FontAsset semibold = FontAsset("SourceSerif4-Semibold", "Source Serif 4 Semibold", glyphs, false, report);
        TMP_FontAsset bold = FontAsset("SourceSerif4-Bold", "Source Serif 4 Bold", glyphs, false, report);
        TMP_FontAsset displayRegular = FontAsset("Cinzel-Regular", "Cinzel Regular", glyphs, false, report);
        TMP_FontAsset display = FontAsset("Cinzel-SemiBold", "Cinzel Semibold", glyphs, false, report);
        TMP_FontAsset displayBold = FontAsset("Cinzel-Bold", "Cinzel Bold", glyphs, false, report);
        foreach (TMP_FontAsset f in new[] { body, semibold, bold }) f.fallbackFontAssetTable = new List<TMP_FontAsset> { bodyFallback };
        foreach (TMP_FontAsset f in new[] { displayRegular, display, displayBold }) f.fallbackFontAssetTable = new List<TMP_FontAsset> { displayFallback };
        LinkWeights(body, semibold, bold);
        LinkWeights(displayRegular, display, displayBold);
        foreach (TMP_FontAsset f in new[] { body, semibold, bold, displayRegular, display, displayBold, bodyFallback, displayFallback })
        {
            EditorUtility.SetDirty(f); AssetDatabase.SaveAssetIfDirty(f);
        }
        Catalog catalog = LoadCatalog();
        MidnightFieldJournalTheme theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(ThemePath);
        if (theme == null) { theme = ScriptableObject.CreateInstance<MidnightFieldJournalTheme>(); AssetDatabase.CreateAsset(theme, ThemePath); }
        SerializedObject data = new SerializedObject(theme);
        data.FindProperty("displayFont").objectReferenceValue = display;
        data.FindProperty("bodyFont").objectReferenceValue = body;
        data.FindProperty("bodySemibold").objectReferenceValue = semibold;
        data.FindProperty("bodyBold").objectReferenceValue = bold;
        SerializedProperty entries = data.FindProperty("sprites"); entries.arraySize = catalog.sprites.Length;
        for (int n = 0; n < catalog.sprites.Length; n++)
        {
            Source item = catalog.sprites[n]; SerializedProperty p = entries.GetArrayElementAtIndex(n);
            p.FindPropertyRelative("sourceId").stringValue = item.id;
            p.FindPropertyRelative("sprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(item.path);
            p.FindPropertyRelative("cardUniqueId").stringValue = item.cardUniqueId;
            p.FindPropertyRelative("sourceHash").stringValue = item.sha256;
            int[] b = item.visibleBoundsPixels;
            p.FindPropertyRelative("visibleBoundsTopLeft").rectValue = new Rect(b[0] / (float)item.width, b[1] / (float)item.height, (b[2] - b[0]) / (float)item.width, (b[3] - b[1]) / (float)item.height);
            p.FindPropertyRelative("designCanvas").vector2Value = new Vector2(item.designWidth, item.designHeight);
            SerializedProperty framing = p.FindPropertyRelative("compactArtworkBounds");
            p.FindPropertyRelative("techniqueRulesBreakAfterWords").intValue = item.id == "CutLooseV2" || item.id == "SlipPastV2" ? 3
                : item.id == "SteadyLineV2" ? 5 : item.id == "PrizeHookV2" ? 4 : 0;
            if (framing.rectValue.width <= 0)
                framing.rectValue = item.id == "CrabV2" ? MidnightFieldJournalTheme.CompactBounds(120, 75, 440, 250)
                    : MidnightFieldJournalTheme.CompactBounds(64, 100, 552, 221);
        }
        SerializedProperty absent = data.FindProperty("unillustratedCardIds"); absent.arraySize = catalog.unillustratedCardIds.Length;
        for (int n = 0; n < absent.arraySize; n++) absent.GetArrayElementAtIndex(n).stringValue = catalog.unillustratedCardIds[n];
        data.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(theme);
        JournalDesignSync.BindCentral(theme);
        File.WriteAllText(Evidence + "UI02FontAudit.json", JsonUtility.ToJson(report, true));
        Debug.Log("Midnight Field Journal: isolated fonts and theme asset prepared.");
    }

    /// <summary>Configures painted sprites without destructive NPOT resizing or CPU-readable runtime copies.</summary>
    private static void ConfigureSprite(Source source)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(source.path);
        if (importer == null) throw new InvalidOperationException("Sprite was not imported: " + source.path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.isReadable = false;
        importer.alphaIsTransparency = source.alphaMin < 255; importer.sRGBTexture = true;
        importer.mipmapEnabled = source.path.Contains("/Techniques/") || source.path.Contains("/Cards/Technique/")
            || source.path.Contains("/Cards/CompactCatch/") || source.path.Contains("/Creatures/");
        importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = importer.mipmapEnabled ? FilterMode.Trilinear : FilterMode.Bilinear;
        TextureImporterSettings settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
    }

    /// <summary>Prebakes supported UI glyphs and embeds each font's own material/atlas under its asset.</summary>
    private static TMP_FontAsset FontAsset(string sourceName, string assetName, string glyphs, bool dynamic, FontReport report)
    {
        string path = Root + "Fonts/TMP/" + assetName + ".asset";
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        Font source = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/Sources/" + sourceName + ".ttf");
        if (source == null) throw new InvalidOperationException("Missing local font: " + sourceName);
        string supported = new string(glyphs.Where(source.HasCharacter).ToArray());
        string unsupported = new string(glyphs.Where(c => !source.HasCharacter(c)).ToArray());
        if (font == null)
        {
            font = TMP_FontAsset.CreateFontAsset(source, 80, 8, GlyphRenderMode.SDF16, dynamic ? 1024 : 2048, dynamic ? 1024 : 2048, AtlasPopulationMode.Dynamic, false);
            if (font == null) throw new InvalidOperationException("Could not create " + assetName);
            font.name = assetName;
            if (!dynamic && !font.TryAddCharacters(supported, out string missing, true)) throw new InvalidOperationException("Atlas did not fit " + assetName + ": " + missing);
            font.atlasPopulationMode = dynamic ? AtlasPopulationMode.Dynamic : AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font, path);
            if (!AssetDatabase.Contains(font.material)) AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (Texture2D atlas in font.atlasTextures) if (!AssetDatabase.Contains(atlas)) AssetDatabase.AddObjectToAsset(atlas, font);
        }
        else if (!dynamic && supported.Any(c => !font.characterLookupTable.ContainsKey(c)))
        {
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            try
            {
                if (!font.TryAddCharacters(supported, out string missing, true)) throw new InvalidOperationException("Expanded text no longer fits " + assetName + ": " + missing);
            }
            finally { font.atlasPopulationMode = AtlasPopulationMode.Static; }
        }
        SerializedObject data = new SerializedObject(font); data.FindProperty("m_ClearDynamicDataOnBuild").boolValue = dynamic; data.ApplyModifiedPropertiesWithoutUndo();
        font.material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0);
        EditorUtility.SetDirty(font.material); AssetDatabase.SaveAssetIfDirty(font.material);
        if (!dynamic)
        {
            // TMP imports whole-font kerning tables. Static atlases only need pairs whose glyphs are baked.
            HashSet<uint> indices = new HashSet<uint>(font.glyphTable.Select(g => g.index));
            font.fontFeatureTable.glyphPairAdjustmentRecords.RemoveAll(p =>
                !indices.Contains(p.firstAdjustmentRecord.glyphIndex) || !indices.Contains(p.secondAdjustmentRecord.glyphIndex));
            font.ReadFontAssetDefinition();
        }
        report.fonts.Add(new FontRecord { path = path, family = font.faceInfo.familyName, style = font.faceInfo.styleName, unsupported = unsupported, glyphs = font.characterTable.Count, kerningPairs = font.fontFeatureTable.glyphPairAdjustmentRecords.Count, dynamic = dynamic });
        return font;
    }

    /// <summary>Links actual weight-table indices used by this project's TMP renderer.</summary>
    private static void LinkWeights(TMP_FontAsset regular, TMP_FontAsset semibold, TMP_FontAsset bold)
    {
        foreach (TMP_FontAsset font in new[] { regular, semibold, bold })
        {
            SerializedObject data = new SerializedObject(font); SerializedProperty table = data.FindProperty("m_FontWeightTable");
            table.GetArrayElementAtIndex(4).FindPropertyRelative("regularTypeface").objectReferenceValue = regular;
            table.GetArrayElementAtIndex(6).FindPropertyRelative("regularTypeface").objectReferenceValue = semibold;
            table.GetArrayElementAtIndex(7).FindPropertyRelative("regularTypeface").objectReferenceValue = bold;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    /// <summary>Collects existing English card/UI content and Latin punctuation without changing authored text.</summary>
    private static string RequiredCharacters()
    {
        StringBuilder value = new StringBuilder("WEIGHT VALUE CATCH BAIT TECHNIQUE LINE LOAD DEPTH DECK CURRENT HAUL COASTAL WATERS DESCEND SURFACE UNKNOWN CREATURE — – ‘ ’ “ ” • … ×");
        for (int c = 32; c <= 255; c++) if (!char.IsControl((char)c)) value.Append((char)c);
        foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }))
        {
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            if (card == null || card.IsBackupTechnique) continue;
            value.Append(card.DisplayName).Append(card.RulesText).Append(card.BaitRulesText);
        }
        return new string(value.ToString().Where(c => !char.IsControl(c)).Distinct().OrderBy(c => c).ToArray());
    }

    /// <summary>Rejects asset operations while gameplay or compilation owns live presentation state.</summary>
    private static void RequireEditMode()
    { if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new InvalidOperationException("Exit Play Mode and finish compilation before preparing theme assets."); }

    /// <summary>Loads the verified catalog produced by source staging.</summary>
    public static Catalog LoadCatalog()
    { return JsonUtility.FromJson<Catalog>(File.ReadAllText(File.Exists(Root + "Profiles/SourceAssetCatalog.json") ? Root + "Profiles/SourceAssetCatalog.json" : Work + "SourceCatalog.json")); }

    /// <summary>Creates only missing asset folders using Unity's folder identity tracking.</summary>
    public static void EnsureFolder(string path)
    {
        path = path.TrimEnd('/'); if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    /// <summary>Moves an existing asset without changing its GUID; conflicting destinations stop migration.</summary>
    private static void Move(string from, string to, MoveReport report)
    {
        string guid = AssetDatabase.AssetPathToGUID(from); if (string.IsNullOrEmpty(guid)) return;
        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(to))) throw new InvalidOperationException("Migration destination already exists: " + to);
        EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
        string error = AssetDatabase.MoveAsset(from, to); if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        if (AssetDatabase.AssetPathToGUID(to) != guid) throw new InvalidOperationException("Migration changed GUID: " + from);
        report.moves.Add(new MoveRecord { from = from, to = to, guid = guid });
        File.WriteAllText(Evidence + "UI02Migration.json", JsonUtility.ToJson(report, true));
    }

    /// <summary>Copies immutable staged input without overwriting an unrelated existing resource.</summary>
    private static void CopyVerified(string from, string to)
    { if (!File.Exists(to)) File.Copy(from, to); VerifyHash(to, Hash(from)); }

    /// <summary>Supports regeneration from checked-in canonical sources after ignored staging has been cleaned.</summary>
    private static void CopyStagedIfPresent(string from, string to)
    { if (File.Exists(to)) return; if (File.Exists(from)) CopyVerified(from, to); else throw new FileNotFoundException("Audited source is missing from both staging and theme.", to); }

    /// <summary>Checks source provenance before resources are consumed by Unity.</summary>
    private static void VerifyHash(string path, string expected)
    { if (!File.Exists(path) || Hash(path) != expected) throw new InvalidOperationException("Source hash mismatch: " + path); }

    /// <summary>Computes a stable content digest without modifying or importing a file.</summary>
    private static string Hash(string path)
    { using (SHA256 sha = SHA256.Create()) using (FileStream file = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant(); }
}
