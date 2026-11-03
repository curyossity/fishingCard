using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Reconstructs the reviewed equipment shop from independent artwork and native controls.</summary>
public static class JournalTackleShopDesignSync
{
    public const string ArtRoot = JournalTackleShopSetup.Root + "DesignR067/";
    public static bool IsAvailable => File.Exists(ArtRoot + "ShopBackground4K.png");
    private static readonly Color Ink = new Color32(40, 59, 61, 255), Pale = new Color32(239, 224, 194, 255);
    private static TMP_FontAsset display, body;

    /// <summary>Replaces only the shop page and repairs its shared wallet references during prefab regeneration.</summary>
    public static void Bind(GameObject root)
    {
        var flow = root.GetComponent<JournalHarborFlow>();
        var stage = root.transform.Find("Hub Content/16x9 Design Stage"); var shop = stage.Find("shop");
        var theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Cinzel Regular.asset") ?? theme.DisplayFont;
        body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Source Serif 4 Regular.asset") ?? theme.BodyFont;
        Import(ArtRoot + "ShopBackground4K.png"); Import(ArtRoot + "EquipmentCardMaster.png");
        foreach (Transform child in shop.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        var view = shop.GetComponent<JournalTackleShopView>() ?? shop.gameObject.AddComponent<JournalTackleShopView>();
        ImageAt("Approved concept / restored 4K background", shop, new Rect(0, 0, 3840, 2160), SpriteAt(ArtRoot + "ShopBackground4K.png"));
        var back = Hit("Navigation / Harbor", shop, new Rect(450, 111, 500, 54));
        Label("Harbor label", back.transform, "‹  HARBOR", new Rect(0, 0, 500, 54), 31, Ink, false, false);
        UnityEventTools.AddIntPersistentListener(back.onClick, flow.Navigate, 0);
        Label("Shop title", shop, "TACKLE SHOP", new Rect(300, 190, 1810, 104), 74, Ink, false, false);
        Label("Shop subtitle", shop, "Choose what you take to sea.", new Rect(1750, 215, 580, 60), 37, Ink, true, true);
        Label("Loadout heading", shop, "EXPEDITION EQUIPMENT", new Rect(180, 325, 1250, 60), 34, Ink, false, false);
        var count = Label("Equipped count", shop, "0 / 3 EQUIPPED", new Rect(1850, 325, 480, 60), 28, Ink, false, true);
        var gold = Label("Gold Balance", shop, "0", new Rect(3300, 110, 270, 70), 48, Pale, false, true); gold.alignment = TextAlignmentOptions.TopRight;
        Label("Gold unit", shop, "GOLD", new Rect(3590, 110, 190, 70), 48, Pale, false, false);
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath);
        var items = catalog.Items; var portraits = new[] { "rod", "reel", "hook" }.Select(id => SpriteAt(JournalHarborBuilder.Root + "Art/" + id + ".png")).ToArray();
        var master = SpriteAt(ArtRoot + "EquipmentCardMaster.png");
        var slotArt = new Image[3]; var slotNames = new TMP_Text[3]; var pluses = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            float x = 355 + 740 * i; var slot = Hit("Equipment slot / " + (EquipmentSlot)i, shop, new Rect(x, 425, 300, 300f * 1322 / 1190));
            ImageAt("Independent equipment frame", slot.transform, new Rect(0, 0, 300, 300f * 1322 / 1190), master);
            pluses[i] = Label("Empty slot", slot.transform, "+", new Rect(90, 125, 120, 70), 61, Ink, true, true);
            slotNames[i] = Label("Equipped item name", slot.transform, "", new Rect(35, 38, 230, 70), 24, Ink, false, true);
            slotArt[i] = ImageAt("Equipped artwork", slot.transform, new Rect(40, 100, 220, 125), null); slotArt[i].preserveAspect = true; slotArt[i].enabled = false;
            Label("Reserved category", shop, ((EquipmentSlot)i).ToString().ToUpperInvariant(), new Rect(x - 30, 780, 360, 60), 32, Ink, false, true);
            UnityEventTools.AddIntPersistentListener(slot.onClick, view.InspectSlot, i);
        }
        Solid("Collection divider", shop, new Rect(155, 865, 2190, 2), new Color(.40f, .35f, .25f, .35f));
        Label("Collection heading", shop, "THE COLLECTION", new Rect(170, 900, 1700, 60), 39, Ink, false, false);
        var catalogCount = Label("Catalog count", shop, "9 ITEMS", new Rect(1980, 910, 350, 50), 28, Ink, false, true);
        var marks = new Graphic[4]; string[] filters = { "ALL", "RODS", "REELS", "HOOKS" };
        for (int i = 0; i < 4; i++)
        {
            var tab = Hit("Browse " + filters[i], shop, new Rect(170 + 380 * i, 970, 300, 65));
            Label("Category label", tab.transform, filters[i], new Rect(0, 0, 300, 45), 27, Ink, false, false);
            marks[i] = Solid("Active category", tab.transform, new Rect(0, 53, 110, 3), Ink);
            marks[i].enabled = i == 0;
            UnityEventTools.AddIntPersistentListener(tab.onClick, view.Filter, i);
        }
        var scrollRoot = Place("Catalog scroll", shop, new Rect(150, 1070, 2210, 920)); var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
        var viewport = Place("Viewport", scrollRoot, new Rect(0, 0, 2210, 920)); viewport.gameObject.AddComponent<RectMask2D>();
        var input = viewport.gameObject.AddComponent<Image>(); input.color = Color.clear; input.raycastTarget = true;
        var content = Place("Catalog content", viewport, new Rect(0, 0, 2210, 1));
        var grid = content.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(500, 500f * 1322 / 1190); grid.spacing = new Vector2(240, 665 - grid.cellSize.y);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.padding = new RectOffset(105, 0, 20, 20);
        var size = content.gameObject.AddComponent<ContentSizeFitter>(); size.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content; scroll.viewport = viewport; scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 100; scroll.movementType = ScrollRect.MovementType.Clamped;
        var cards = new GameObject[9]; var states = new TMP_Text[9]; var buttons = new Button[9]; var selections = new Graphic[9];
        for (int i = 0; i < 9; i++)
        {
            int index = i % 3; var card = Hit("Inspect " + items[index].DisplayName.ToUpperInvariant() + (i < 3 ? "" : " / placeholder " + i), content, new Rect(0, 0, 500, grid.cellSize.y)); cards[i] = card.gameObject; buttons[i] = card;
            ImageAt("Independent equipment card frame", card.transform, new Rect(0, 0, 500, grid.cellSize.y), master);
            Label("Category", card.transform, items[index].Slot.ToString().ToUpperInvariant(), new Rect(60, 43, 380, 25), 17, Ink, false, true);
            Label("Name", card.transform, items[index].DisplayName, new Rect(32, 69, 436, 48), 30, Ink, false, true);
            var art = ImageAt("Independent equipment portrait", card.transform, new Rect(85, 150, 330, 200), portraits[index]); art.preserveAspect = true;
            states[i] = Label("Price or ownership", card.transform, items[index].Price + " GOLD", new Rect(55, 453, 390, 48), 29, Pale, false, true);
            selections[i] = Outline(card.transform, new Rect(-8, -8, 516, grid.cellSize.y + 16));
            selections[i].enabled = i == 1;
            UnityEventTools.AddIntPersistentListener(card.onClick, view.Select, index);
        }
        for (int i = 0; i < 2; i++)
        {
            var arrow = Hit(i == 0 ? "Catalog previous" : "Catalog next", shop, new Rect(2375, i == 0 ? 1080 : 1900, 80, 80));
            var chevron = Place("Arrow", arrow.transform, new Rect(0, 0, 80, 80)).gameObject.AddComponent<JournalStatScrollArrow>(); chevron.color = Ink; chevron.raycastTarget = false; chevron.Upwards = i == 0;
            UnityEventTools.AddIntPersistentListener(arrow.onClick, view.Scroll, i == 0 ? 1 : -1);
        }
        Label("Inspection eyebrow", shop, "EQUIPMENT DETAILS", new Rect(2665, 270, 980, 60), 32, new Color32(196, 161, 112, 255), false, true);
        var detail = Place("Enlarged equipment card", shop, new Rect(2600, 420, 1140, 1140f * 1322 / 1190));
        ImageAt("Independent detail frame", detail, new Rect(0, 0, 1140, detail.sizeDelta.y), master);
        var category = Label("Equipment type", detail, "REEL", new Rect(180, 90, 780, 34), 24, Ink, false, true);
        var name = Label("Equipment name", detail, "TIDEGLASS REEL", new Rect(140, 133, 860, 80), 55, Ink, false, true);
        var detailArt = ImageAt("Independent detail artwork", detail, new Rect(215, 280, 710, 520), portraits[1]); detailArt.preserveAspect = true;
        var state = Label("Ownership state", detail, "PERMANENT UNLOCK", new Rect(190, 777, 760, 38), 24, Ink, false, true);
        Label("Effect heading", detail, "EXPEDITION EFFECT", new Rect(190, 941, 760, 48), 28, Pale, false, true);
        var effect = Label("Effect description", detail, items[1].Description, new Rect(215, 1015, 710, 159), 38, Pale, true, true);
        var action = Hit("Purchase or Equip", shop, new Rect(2665, 1790, 980, 120));
        var actionPaper = Solid("Action paper", action.transform, new Rect(0, 0, 980, 120), new Color32(235, 216, 183, 255)); actionPaper.raycastTarget = false;
        var actionLabel = Label("Action label", action.transform, "BUY", new Rect(45, 33, 890, 60), 36, Ink, false, true);
        UnityEventTools.AddPersistentListener(action.onClick, view.Activate);
        var message = Label("Slot hint", shop, "Equip one Rod, one Reel and one Hook.", new Rect(2670, 1960, 980, 100), 31, Pale, true, true);
        var data = new SerializedObject(view); Set(data, "flow", flow); Set(data, "detailCategory", category); Set(data, "detailName", name); Set(data, "detailEffect", effect); Set(data, "detailState", state); Set(data, "actionLabel", actionLabel); Set(data, "message", message); Set(data, "action", action); Set(data, "detailArt", detailArt); Set(data, "catalogScroll", scroll); Set(data, "catalogCount", catalogCount); Set(data, "equippedCount", count);
        SetArray(data, "cardStates", states); SetArray(data, "cardEffects", Array.Empty<TMP_Text>()); SetArray(data, "itemButtons", buttons); SetArray(data, "portraits", portraits); SetArray(data, "slotArt", slotArt); SetArray(data, "slotNames", slotNames); SetArray(data, "slotPluses", pluses); SetArray(data, "catalogCards", cards); SetArray(data, "selectionMarks", selections); SetArray(data, "filterMarks", marks);
        SetArray(data, "loadoutLabels", stage.Find("harbor").GetComponentsInChildren<TMP_Text>(true).Where(t => t.name.StartsWith("Editable / Driftwood Rod", StringComparison.Ordinal)).ToArray()); SetArray(data, "loadoutHeadings", Array.Empty<TMP_Text>());
        Set(data, "collectionHand", stage.Find("collection").GetComponentsInChildren<TMP_Text>(true).First(t => t.text.StartsWith("Starting hand:", StringComparison.Ordinal))); data.ApplyModifiedPropertiesWithoutUndo();
        var flowData = new SerializedObject(flow); SetArray(flowData, "goldLabels", root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.name == "Gold Balance").ToArray()); flowData.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Imports full-resolution independent source art without downscaling or lossy texture compression.</summary>
    private static void Import(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport); var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.maxTextureSize = 4096; importer.npotScale = TextureImporterNPOTScale.None; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp; importer.filterMode = FilterMode.Bilinear;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
    }

    /// <summary>Loads an exact independent sprite identity.</summary>
    private static Sprite SpriteAt(string path) { return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Missing shop sprite: " + path); }
    /// <summary>Places native UI in the reviewed top-left 4K coordinate system.</summary>
    private static RectTransform Place(string name, Transform parent, Rect bounds) { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(bounds.x, -bounds.y); r.sizeDelta = bounds.size; return r; }
    /// <summary>Creates a separate raster illustration layer without baking any live text.</summary>
    private static Image ImageAt(string name, Transform parent, Rect bounds, Sprite sprite) { var image = Place(name, parent, bounds).gameObject.AddComponent<Image>(); image.sprite = sprite; image.raycastTarget = false; return image; }
    /// <summary>Creates a native color layer.</summary>
    private static Image Solid(string name, Transform parent, Rect bounds, Color color) { var image = ImageAt(name, parent, bounds, null); image.color = color; return image; }
    /// <summary>Builds a separate editable field using the existing licensed theme fonts.</summary>
    private static TMP_Text Label(string name, Transform parent, string copy, Rect bounds, float fontSize, Color color, bool prose, bool center) { var t = Place(name, parent, bounds).gameObject.AddComponent<TextMeshProUGUI>(); t.font = prose ? body : display; t.text = copy; t.color = color; t.fontSize = fontSize; t.enableAutoSizing = true; t.fontSizeMax = fontSize; t.fontSizeMin = fontSize * .8f; t.alignment = center ? TextAlignmentOptions.Top : TextAlignmentOptions.TopLeft; t.textWrappingMode = TextWrappingModes.Normal; t.richText = false; t.raycastTarget = false; t.margin = Vector4.zero; return t; }
    /// <summary>Creates navigation/purchase targets with a faint wash instead of a hover frame.</summary>
    private static Button Hit(string name, Transform parent, Rect bounds) { var r = Place(name, parent, bounds); var image = r.gameObject.AddComponent<Image>(); var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = image; var c = b.colors; c.normalColor = Color.clear; c.highlightedColor = new Color(1, .94f, .78f, .07f); c.selectedColor = new Color(1, .94f, .78f, .08f); c.pressedColor = new Color(1, .94f, .78f, .15f); c.disabledColor = Color.clear; b.colors = c; return b; }
    /// <summary>Shows the inspected catalog item with an independent native keyline.</summary>
    private static Graphic Outline(Transform parent, Rect bounds) { var r = Place("Selected item keyline", parent, bounds); var line = r.gameObject.AddComponent<JournalCardOutline>(); line.color = new Color32(59, 105, 101, 255); line.raycastTarget = false; return line; }
    /// <summary>Persists a native field reference in the reusable prefab.</summary>
    private static void Set(SerializedObject data, string field, UnityEngine.Object value) { data.FindProperty(field).objectReferenceValue = value; }
    /// <summary>Persists pooled field collections without runtime hierarchy searches.</summary>
    private static void SetArray(SerializedObject data, string field, UnityEngine.Object[] values) { var p = data.FindProperty(field); p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
}
