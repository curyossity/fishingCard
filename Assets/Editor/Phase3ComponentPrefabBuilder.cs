using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Phase3ComponentPrefabBuilder
{
    private const string ComponentDirectory = "Assets/Prefabs/UI/Components";
    private const string ValidationDirectory = "Docs/Validation/Phase3";
    private const string MaritimePanelPath = ComponentDirectory + "/MaritimePanel.prefab";
    private const string CreatureCardPath = ComponentDirectory + "/CreatureCardView.prefab";
    private const string CompactCatchCardPath = ComponentDirectory + "/CompactCatchCard.prefab";
    private const string TechniqueCardPath = ComponentDirectory + "/TechniqueCardView.prefab";
    private const string RunActionButtonPath = ComponentDirectory + "/RunActionButton.prefab";
    private const string DisplayFontPath = "Assets/FishingUIAssets/Fonts/TMP/Marcellus SDF.asset";
    private const string BodyFontPath = "Assets/FishingUIAssets/Fonts/TMP/Source Serif 4 SDF.asset";
    /// <summary>Imports Unity's bundled TMP resources before Phase 3 prefab generation.</summary>
    public static void PrepareTmpResources()
    {
        if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }

            return;
        }

        AssetDatabase.importPackageCompleted += _ => EditorApplication.Exit(0);
        AssetDatabase.importPackageCancelled += _ => EditorApplication.Exit(1);
        AssetDatabase.importPackageFailed += (_, error) =>
        {
            Debug.LogError(error);
            EditorApplication.Exit(1);
        };
        TMP_PackageResourceImporter.ImportResources(true, false, false);
    }

    /// <summary>
    /// Builds and validates the reusable MaritimePanel component for Task 3.1.
    /// </summary>
    [MenuItem("Fishing Cards/UI/Phase 3/Build Task 3.1 Maritime Panel")]
    public static void BuildTask31()
    {
        try
        {
            EnsureDirectories();
            Require(
                Resources.Load<TMP_Settings>("TMP Settings") != null,
                "TMP Essential Resources are missing. Run PrepareTmpResources first.");
            TMP_FontAsset displayFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf",
                DisplayFontPath);
            TMP_FontAsset bodyFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Variable.ttf",
                BodyFontPath);
            BuildMaritimePanel(displayFont);
            ValidateMaritimePanel();
            CaptureMaritimePanelVariants(displayFont, bodyFont);
            File.WriteAllText(
                Path.Combine(ValidationDirectory, "task-3.1-result.json"),
                "{\"unity\":\"" + Application.unityVersion
                + "\",\"prefab\":\"MaritimePanel\",\"variants\":4,\"resizedSamples\":4,\"status\":\"passed\"}");
            DeleteFailure("task-3.1-failure.txt");
            AssetDatabase.SaveAssets();
            Debug.Log("Task 3.1 MaritimePanel build and validation passed.");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            WriteFailure("task-3.1-failure.txt", exception);
            Debug.LogException(exception);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>Builds and validates the layered creature card component for Task 3.2.</summary>
    [MenuItem("Fishing Cards/UI/Phase 3/Build Task 3.2 Creature Card")]
    public static void BuildTask32()
    {
        try
        {
            EnsureDirectories();
            Require(
                Resources.Load<TMP_Settings>("TMP Settings") != null,
                "TMP Essential Resources are missing. Run PrepareTmpResources first.");
            TMP_FontAsset displayFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf",
                DisplayFontPath);
            TMP_FontAsset bodyFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Variable.ttf",
                BodyFontPath);
            BuildCreatureCard(displayFont, bodyFont);
            ValidateCreatureCard();
            CaptureCreatureCard(displayFont, bodyFont);
            File.WriteAllText(
                Path.Combine(ValidationDirectory, "task-3.2-result.json"),
                "{\"unity\":\"" + Application.unityVersion
                + "\",\"prefab\":\"CreatureCardView\",\"anchorSlots\":4,\"status\":\"passed\"}");
            DeleteFailure("task-3.2-failure.txt");
            AssetDatabase.SaveAssets();
            Debug.Log("Task 3.2 CreatureCardView build and validation passed.");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            WriteFailure("task-3.2-failure.txt", exception);
            Debug.LogException(exception);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>Builds and validates the compact Catch Rig card for Task 3.3.</summary>
    [MenuItem("Fishing Cards/UI/Phase 3/Build Task 3.3 Compact Catch Card")]
    public static void BuildTask33()
    {
        try
        {
            EnsureDirectories();
            Require(Resources.Load<TMP_Settings>("TMP Settings") != null, "TMP Essential Resources are missing.");
            TMP_FontAsset displayFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf",
                DisplayFontPath);
            TMP_FontAsset bodyFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Variable.ttf",
                BodyFontPath);
            BuildCompactCatchCard(displayFont, bodyFont);
            ValidateCompactCatchCard();
            CaptureCompactCatchCards();
            File.WriteAllText(
                Path.Combine(ValidationDirectory, "task-3.3-result.json"),
                "{\"unity\":\"" + Application.unityVersion
                + "\",\"prefab\":\"CompactCatchCard\",\"simultaneousCards\":4,\"states\":4,\"status\":\"passed\"}");
            DeleteFailure("task-3.3-failure.txt");
            AssetDatabase.SaveAssets();
            Debug.Log("Task 3.3 CompactCatchCard build and validation passed.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            WriteFailure("task-3.3-failure.txt", exception);
            Debug.LogException(exception);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>Builds and validates the Technique hand card for Task 3.4.</summary>
    [MenuItem("Fishing Cards/UI/Phase 3/Build Task 3.4 Technique Card")]
    public static void BuildTask34()
    {
        try
        {
            EnsureDirectories();
            Require(Resources.Load<TMP_Settings>("TMP Settings") != null, "TMP Essential Resources are missing.");
            TMP_FontAsset displayFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf",
                DisplayFontPath);
            TMP_FontAsset bodyFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Variable.ttf",
                BodyFontPath);
            BuildTechniqueCard(displayFont, bodyFont);
            ValidateTechniqueCard();
            CaptureTechniqueCards();
            File.WriteAllText(
                Path.Combine(ValidationDirectory, "task-3.4-result.json"),
                "{\"unity\":\"" + Application.unityVersion
                + "\",\"prefab\":\"TechniqueCardView\",\"simultaneousCards\":5,\"states\":5,\"status\":\"passed\"}");
            DeleteFailure("task-3.4-failure.txt");
            AssetDatabase.SaveAssets();
            Debug.Log("Task 3.4 TechniqueCardView build and validation passed.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            WriteFailure("task-3.4-failure.txt", exception);
            Debug.LogException(exception);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>Builds and validates the reusable run action button for Task 3.5.</summary>
    [MenuItem("Fishing Cards/UI/Phase 3/Build Task 3.5 Run Action Button")]
    public static void BuildTask35()
    {
        try
        {
            EnsureDirectories();
            Require(Resources.Load<TMP_Settings>("TMP Settings") != null, "TMP Essential Resources are missing.");
            TMP_FontAsset displayFont = EnsureFontAsset(
                "Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf",
                DisplayFontPath);
            BuildRunActionButton(displayFont);
            ValidateRunActionButton();
            CaptureRunActionButtons();
            File.WriteAllText(
                Path.Combine(ValidationDirectory, "task-3.5-result.json"),
                "{\"unity\":\"" + Application.unityVersion
                + "\",\"prefab\":\"RunActionButton\",\"themes\":3,\"states\":5,\"status\":\"passed\"}");
            DeleteFailure("task-3.5-failure.txt");
            AssetDatabase.SaveAssets();
            Debug.Log("Task 3.5 RunActionButton build and validation passed.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            WriteFailure("task-3.5-failure.txt", exception);
            Debug.LogException(exception);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>
    /// Creates the layered panel prefab without generating or recoloring supplied artwork.
    /// </summary>
    private static void BuildMaritimePanel(TMP_FontAsset displayFont)
    {
        GameObject root = CreateUiObject("MaritimePanel", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(420f, 520f);
        MaritimePanel panel = root.AddComponent<MaritimePanel>();

        RawImage borderMaterial = CreateRawImage("Nested Border Material", rootRect, Vector2.zero, Vector2.one);
        borderMaterial.texture = LoadTexture("Frames/aged-brass-tile.png");
        borderMaterial.uvRect = new Rect(0f, 0f, 2f, 2f);

        Image interior = CreateImage("Interior", rootRect, null, Image.Type.Simple);
        SetStretchOffsets(interior.rectTransform, 10f, 10f, -10f, -10f);

        RawImage interiorMaterial = CreateRawImage("Interior Material", interior.rectTransform, Vector2.zero, Vector2.one);
        interiorMaterial.texture = LoadTexture("Backgrounds/teal-paper-tile.png");
        interiorMaterial.uvRect = new Rect(0f, 0f, 2f, 2f);

        Image frame = CreateImage(
            "Frame",
            rootRect,
            LoadSprite("Frames/maritime-panel-9slice.png"),
            Image.Type.Sliced);

        RectTransform content = CreateRect("Content", rootRect, Vector2.zero, Vector2.one);

        RectTransform header = CreateRect("Header", rootRect, new Vector2(0f, 1f), Vector2.one);
        header.pivot = new Vector2(0.5f, 1f);
        header.sizeDelta = new Vector2(0f, 76f);
        header.anchoredPosition = Vector2.zero;
        Image headerFrame = CreateImage(
            "Header Frame",
            header,
            LoadSprite("Frames/panel-header-strip-9slice.png"),
            Image.Type.Sliced);

        RectTransform iconRect = CreateRect("Header Icon", header, new Vector2(0f, 0f), new Vector2(0f, 1f));
        iconRect.sizeDelta = new Vector2(42f, -22f);
        iconRect.anchoredPosition = new Vector2(32f, 0f);
        Image headerIcon = iconRect.gameObject.AddComponent<Image>();
        headerIcon.preserveAspect = true;
        headerIcon.raycastTarget = false;
        headerIcon.gameObject.SetActive(false);

        TextMeshProUGUI headerText = CreateText("Header Text", header, displayFont, 25f, TextAlignmentOptions.MidlineLeft);
        SetStretchOffsets(headerText.rectTransform, 60f, 10f, -28f, -10f);

        Image[] corners = new Image[4];
        Vector2[] anchors = { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        Vector2[] pivots = { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        for (int i = 0; i < corners.Length; i++)
        {
            RectTransform corner = CreateRect("Corner Ornament " + (i + 1), rootRect, anchors[i], anchors[i]);
            corner.pivot = pivots[i];
            corner.sizeDelta = new Vector2(48f, 48f);
            corners[i] = corner.gameObject.AddComponent<Image>();
            corners[i].preserveAspect = true;
            corners[i].raycastTarget = false;
            corners[i].gameObject.SetActive(false);
        }

        SerializedObject serializedPanel = new SerializedObject(panel);
        SetReference(serializedPanel, "borderMaterial", borderMaterial);
        SetReference(serializedPanel, "interior", interior);
        SetReference(serializedPanel, "interiorMaterial", interiorMaterial);
        SetReference(serializedPanel, "outerFrame", frame);
        SetReference(serializedPanel, "contentRoot", content);
        SetReference(serializedPanel, "headerRoot", header.gameObject);
        SetReference(serializedPanel, "headerFrame", headerFrame);
        SetReference(serializedPanel, "headerIcon", headerIcon);
        SetReference(serializedPanel, "headerText", headerText);
        SetReference(serializedPanel, "tealPaperTexture", LoadTexture("Backgrounds/teal-paper-tile.png"));
        SetReference(serializedPanel, "ivoryPaperTexture", LoadTexture("Backgrounds/ivory-paper-tile.png"));
        SetReference(serializedPanel, "printNoiseTexture", LoadTexture("Effects/print-noise-overlay.png"));
        SetReference(serializedPanel, "agedBrassTexture", LoadTexture("Frames/aged-brass-tile.png"));
        SetReference(serializedPanel, "oxidizedTealTexture", LoadTexture("Frames/oxidized-teal-tile.png"));
        SerializedProperty cornerProperty = serializedPanel.FindProperty("cornerOrnaments");
        cornerProperty.arraySize = corners.Length;
        for (int i = 0; i < corners.Length; i++)
        {
            cornerProperty.GetArrayElementAtIndex(i).objectReferenceValue = corners[i];
        }
        serializedPanel.ApplyModifiedPropertiesWithoutUndo();

        panel.SetHeader(string.Empty);
        panel.ApplyAppearance();
        PrefabUtility.SaveAsPrefabAsset(root, MaritimePanelPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    /// <summary>
    /// Checks the prefab's reusable fields and non-stretching sliced surfaces.
    /// </summary>
    private static void ValidateMaritimePanel()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MaritimePanelPath);
        Require(prefab != null, "MaritimePanel prefab was not created.");
        MaritimePanel panel = prefab.GetComponent<MaritimePanel>();
        Require(panel != null, "MaritimePanel component is missing.");
        Require(panel.ContentRoot != null, "MaritimePanel content root is missing.");
        Require(prefab.transform.Find("Header") != null, "MaritimePanel optional header is missing.");
        Require(prefab.transform.Find("Header/Header Icon") != null, "MaritimePanel optional icon is missing.");
        Require(prefab.transform.Find("Corner Ornament 1") != null, "MaritimePanel corner slots are missing.");

        Image frame = prefab.transform.Find("Frame").GetComponent<Image>();
        Image headerFrame = prefab.transform.Find("Header/Header Frame").GetComponent<Image>();
        Require(frame.type == Image.Type.Sliced, "MaritimePanel frame must be sliced.");
        Require(headerFrame.type == Image.Type.Sliced, "MaritimePanel header must be sliced.");
        Require(frame.sprite != null && frame.sprite.border.sqrMagnitude > 0f, "MaritimePanel frame has no slice border.");
        Require(headerFrame.sprite != null && headerFrame.sprite.border.sqrMagnitude > 0f, "MaritimePanel header has no slice border.");
    }

    /// <summary>
    /// Renders all variants across compact, wide, tall, and large panel dimensions.
    /// </summary>
    private static void CaptureMaritimePanelVariants(TMP_FontAsset displayFont, TMP_FontAsset bodyFont)
    {
        Scene previousScene = SceneManager.GetActiveScene();
        bool openedAdditively = !Application.isBatchMode;
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            openedAdditively ? NewSceneMode.Additive : NewSceneMode.Single);
        try
        {
            SceneManager.SetActiveScene(scene);
            const int width = 1400;
            const int height = 900;
            Camera camera = CreateValidationCanvas(scene, width, height, out RectTransform canvasRoot, out RenderTexture target);
            CreateSolidBackground(canvasRoot, new Color32(7, 37, 39, 255));

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MaritimePanelPath);
            Sprite infoIcon = LoadSprite("Icons/info.png");
            MaritimePanelVariant[] variants =
            {
                MaritimePanelVariant.DarkTeal,
                MaritimePanelVariant.Ivory,
                MaritimePanelVariant.Turquoise,
                MaritimePanelVariant.Coral
            };
            MaritimeBorderStyle[] borders =
            {
                MaritimeBorderStyle.AgedBrass,
                MaritimeBorderStyle.OxidizedTeal,
                MaritimeBorderStyle.AgedBrass,
                MaritimeBorderStyle.OxidizedTeal
            };
            Rect[] rects =
            {
                new Rect(35f, 445f, 300f, 410f),
                new Rect(370f, 530f, 430f, 325f),
                new Rect(835f, 445f, 520f, 410f),
                new Rect(185f, 65f, 1030f, 300f)
            };

            for (int i = 0; i < variants.Length; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.SetParent(canvasRoot, false);
                Place(instance.GetComponent<RectTransform>(), rects[i]);
                MaritimePanel panel = instance.GetComponent<MaritimePanel>();
                panel.SetAppearance(variants[i], borders[i], new Color32(181, 138, 74, 255));
                panel.SetHeader(variants[i].ToString().ToUpperInvariant(), i == 2 ? infoIcon : null);

                TextMeshProUGUI sample = CreateText(
                    "Sample Content",
                    panel.ContentRoot,
                    bodyFont,
                    i == 3 ? 24f : 19f,
                    TextAlignmentOptions.Center);
                SetStretchOffsets(sample.rectTransform, 8f, 8f, -8f, -62f);
                sample.color = variants[i] == MaritimePanelVariant.Ivory
                    ? new Color32(25, 61, 61, 255)
                    : new Color32(239, 226, 194, 255);
                sample.text = "SAFE CONTENT REGION\nResizable nine-sliced frame\nOptional header and icon";
            }

            SaveRender(camera, target, width, height, "task-3.1-maritime-panel.png");
        }
        finally
        {
            if (openedAdditively)
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousScene);
                }
            }
        }
    }

    /// <summary>Creates the portrait card prefab from independently owned visual and data layers.</summary>
    private static void BuildCreatureCard(TMP_FontAsset displayFont, TMP_FontAsset bodyFont)
    {
        GameObject root = CreateUiObject("CreatureCardView", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(400f, 600f);
        CreatureCardView view = root.AddComponent<CreatureCardView>();

        Image background = CreateImage(
            "CardBackground",
            rootRect,
            LoadSprite("Cards/Creature/creature-card-base.png"),
            Image.Type.Simple);

        RectTransform artworkMaskRect = CreateRect(
            "CreatureArtworkMask",
            rootRect,
            new Vector2(0.165f, 0.455f),
            new Vector2(0.835f, 0.79f));
        Image artworkMaskImage = artworkMaskRect.gameObject.AddComponent<Image>();
        artworkMaskImage.sprite = LoadSprite("Cards/Creature/creature-art-mask.png");
        artworkMaskImage.preserveAspect = true;
        artworkMaskImage.raycastTarget = false;
        Mask artworkMask = artworkMaskRect.gameObject.AddComponent<Mask>();
        artworkMask.showMaskGraphic = false;

        Image artwork = CreateImage("CreatureArtwork", artworkMaskRect, null, Image.Type.Simple);
        artwork.preserveAspect = true;

        Image overflow = CreateImage("ArtworkOverflowLayer", rootRect, null, Image.Type.Simple);
        overflow.enabled = false;
        overflow.preserveAspect = true;

        Image titlePlate = CreateImage(
            "Title Safe Region",
            rootRect,
            LoadSprite("Cards/Creature/creature-title-plate-9slice.png"),
            Image.Type.Sliced);
        SetAnchoredRect(titlePlate.rectTransform, new Vector2(0.075f, 0.825f), new Vector2(0.925f, 0.965f));
        titlePlate.pixelsPerUnitMultiplier = 8f;

        TextMeshProUGUI typeText = CreateText(
            "Card Type Text",
            titlePlate.rectTransform,
            bodyFont,
            14f,
            TextAlignmentOptions.Center);
        SetAnchoredRect(typeText.rectTransform, new Vector2(0.27f, 0.58f), new Vector2(0.73f, 0.9f));
        typeText.color = new Color32(212, 185, 123, 255);
        typeText.fontWeight = FontWeight.SemiBold;
        typeText.textWrappingMode = TextWrappingModes.NoWrap;

        TextMeshProUGUI nameText = CreateText(
            "Name Text",
            titlePlate.rectTransform,
            displayFont,
            30f,
            TextAlignmentOptions.Center);
        SetAnchoredRect(nameText.rectTransform, new Vector2(0.24f, 0.1f), new Vector2(0.76f, 0.62f));
        nameText.color = new Color32(239, 226, 194, 255);
        nameText.fontSizeMin = 14f;
        nameText.textWrappingMode = TextWrappingModes.NoWrap;

        Image weightPlate = CreateStatPlate("Weight Safe Region", rootRect, new Vector2(0.15f, 0.31f), new Vector2(0.47f, 0.38f));
        Image valuePlate = CreateStatPlate("Value Safe Region", rootRect, new Vector2(0.53f, 0.31f), new Vector2(0.85f, 0.38f));
        TextMeshProUGUI weightText = CreateText("Weight Text", weightPlate.rectTransform, bodyFont, 36f, TextAlignmentOptions.Center);
        TextMeshProUGUI valueText = CreateText("Value Text", valuePlate.rectTransform, bodyFont, 36f, TextAlignmentOptions.Center);
        weightText.color = new Color32(26, 60, 59, 255);
        valueText.color = weightText.color;
        weightText.fontStyle = FontStyles.Bold;
        valueText.fontStyle = FontStyles.Bold;
        SetStretchOffsets(weightText.rectTransform, 22f, 5f, -22f, -5f);
        SetStretchOffsets(valueText.rectTransform, 22f, 5f, -22f, -5f);

        Image effectPanel = CreateImage(
            "Rules Safe Region",
            rootRect,
            LoadSprite("Cards/Creature/creature-effect-panel-9slice.png"),
            Image.Type.Sliced);
        SetAnchoredRect(effectPanel.rectTransform, new Vector2(0.16f, 0.13f), new Vector2(0.84f, 0.285f));
        effectPanel.pixelsPerUnitMultiplier = 6f;
        TextMeshProUGUI effectText = CreateText(
            "Effect Text",
            effectPanel.rectTransform,
            bodyFont,
            18f,
            TextAlignmentOptions.Center);
        effectText.color = new Color32(25, 55, 54, 255);
        SetStretchOffsets(effectText.rectTransform, 30f, 18f, -30f, -18f);

        RectTransform anchorRoot = CreateRect(
            "AnchorSlots",
            rootRect,
            new Vector2(0.22f, 0.09f),
            new Vector2(0.78f, 0.16f));
        Image[] sockets = new Image[4];
        Image[] markers = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            float center = (i + 0.5f) / 4f;
            RectTransform slot = CreateRect(
                "Anchor0" + (i + 1),
                anchorRoot,
                new Vector2(center, 0.5f),
                new Vector2(center, 0.5f));
            slot.sizeDelta = new Vector2(46f, 46f);
            sockets[i] = CreateImage(
                "Socket",
                slot,
                LoadSprite("Markers/anchor-socket-empty.png"),
                Image.Type.Simple);
            sockets[i].preserveAspect = true;
            markers[i] = CreateImage(
                "Filled Anchor",
                slot,
                LoadSprite("Markers/anchor-filled.png"),
                Image.Type.Simple);
            markers[i].preserveAspect = true;
        }

        Image frame = CreateImage(
            "Card Frame",
            rootRect,
            LoadSprite("Frames/creature-card-frame-9slice.png"),
            Image.Type.Sliced);
        frame.fillCenter = false;
        frame.pixelsPerUnitMultiplier = 2f;

        Image interaction = CreateImage("InteractionOverlay", rootRect, null, Image.Type.Simple);
        interaction.enabled = false;

        SerializedObject serializedView = new SerializedObject(view);
        SetReference(serializedView, "cardBackground", background);
        SetReference(serializedView, "creatureArtwork", artwork);
        SetReference(serializedView, "artworkOverflowLayer", overflow);
        SetReference(serializedView, "cardFrame", frame);
        SetReference(serializedView, "interactionOverlay", interaction);
        SetReference(serializedView, "cardTypeText", typeText);
        SetReference(serializedView, "cardNameText", nameText);
        SetReference(serializedView, "weightText", weightText);
        SetReference(serializedView, "valueText", valueText);
        SetReference(serializedView, "effectText", effectText);
        SetReference(serializedView, "fallbackCardFace", background.sprite);
        SetReference(serializedView, "rarityHookSprite", LoadSprite("Markers/anchor-filled.png"));
        SetReferenceArray(serializedView, "anchorSockets", sockets);
        SetReferenceArray(serializedView, "anchorMarkers", markers);
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        view.SetBlank();
        PrefabUtility.SaveAsPrefabAsset(root, CreatureCardPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static Image CreateStatPlate(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        Image plate = CreateImage(
            name,
            parent,
            LoadSprite("Cards/Creature/creature-stat-plate-9slice.png"),
            Image.Type.Sliced);
        SetAnchoredRect(plate.rectTransform, anchorMin, anchorMax);
        plate.pixelsPerUnitMultiplier = 8f;
        return plate;
    }

    /// <summary>Checks card ownership layers, safe regions, stencil mask, and exact marker count.</summary>
    private static void ValidateCreatureCard()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CreatureCardPath);
        Require(prefab != null, "CreatureCardView prefab was not created.");
        CreatureCardView view = prefab.GetComponent<CreatureCardView>();
        Require(view != null, "CreatureCardView component is missing.");
        Require(view.AnchorSlotCount == 4, "CreatureCardView must expose exactly four anchor slots.");
        Require(prefab.transform.Find("CreatureArtworkMask")?.GetComponent<Mask>() != null, "Creature artwork mask is missing.");
        Require(!prefab.transform.Find("CreatureArtworkMask").GetComponent<Mask>().showMaskGraphic, "Creature mask RGB must remain hidden.");
        Require(prefab.transform.Find("Title Safe Region") != null, "Title safe region is missing.");
        Require(prefab.transform.Find("Weight Safe Region") != null, "Weight safe region is missing.");
        Require(prefab.transform.Find("Value Safe Region") != null, "Value safe region is missing.");
        Require(prefab.transform.Find("Rules Safe Region") != null, "Rules safe region is missing.");
        Require(prefab.transform.Find("ArtworkOverflowLayer") != null, "Approved artwork overflow layer is missing.");
        Require(prefab.transform.Find("InteractionOverlay") != null, "Interaction overlay is missing.");
        Image frame = prefab.transform.Find("Card Frame").GetComponent<Image>();
        Require(frame.type == Image.Type.Sliced && !frame.fillCenter, "Creature frame must be hollow and sliced.");
    }

    /// <summary>Renders representative common and legendary cards at gameplay sizes.</summary>
    private static void CaptureCreatureCard(TMP_FontAsset displayFont, TMP_FontAsset bodyFont)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const int width = 1400;
        const int height = 900;
        Camera camera = CreateValidationCanvas(scene, width, height, out RectTransform canvasRoot, out RenderTexture target);
        CreateSolidBackground(canvasRoot, new Color32(7, 37, 39, 255));
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CreatureCardPath);
        Sprite previewArtwork = LoadSprite("Icons/catch.png");

        CreatureCardView common = CreateCreaturePreview(prefab, scene, canvasRoot, new Rect(65f, 145f, 360f, 540f));
        common.SetPreview(
            "COASTAL COD",
            "CATCH CARD",
            previewArtwork,
            3,
            5,
            "A dependable catch. Gain value while it remains attached to the rig.",
            CardRarity.Common);

        CreatureCardView blank = CreateCreaturePreview(prefab, scene, canvasRoot, new Rect(555f, 205f, 280f, 420f));
        blank.SetBlank();

        CreatureCardView legendary = CreateCreaturePreview(prefab, scene, canvasRoot, new Rect(940f, 115f, 400f, 600f));
        legendary.SetPreview(
            "ABYSSAL LANTERNFISH",
            "APEX CATCH",
            previewArtwork,
            12,
            24,
            "Reduce line load by 2. Draw 1 technique after this creature is caught.",
            CardRarity.Legendary);

        SaveRender(camera, target, width, height, "task-3.2-creature-card.png");
    }

    private static CreatureCardView CreateCreaturePreview(
        GameObject prefab,
        Scene scene,
        RectTransform parent,
        Rect rect)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.SetParent(parent, false);
        Place(instance.GetComponent<RectTransform>(), rect);
        return instance.GetComponent<CreatureCardView>();
    }

    /// <summary>Builds the horizontal catch summary with fixed rig and interaction anchors.</summary>
    private static void BuildCompactCatchCard(TMP_FontAsset displayFont, TMP_FontAsset bodyFont)
    {
        GameObject root = CreateUiObject("CompactCatchCard", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(480f, 320f);
        Image background = root.AddComponent<Image>();
        background.sprite = LoadSprite("Cards/CompactCatch/compact-catch-card-base.png");
        background.raycastTarget = true;
        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;
        CompactCatchCardView view = root.AddComponent<CompactCatchCardView>();

        Image portrait = CreateImage("Portrait", rootRect, null, Image.Type.Simple);
        SetAnchoredRect(portrait.rectTransform, new Vector2(0.075f, 0.18f), new Vector2(0.34f, 0.82f));
        portrait.preserveAspect = true;

        Image disabled = CreateImage(
            "Disabled Hatch",
            rootRect,
            LoadSprite("Cards/card-disabled-overlay.png"),
            Image.Type.Simple);
        disabled.gameObject.SetActive(false);

        TextMeshProUGUI nameText = CreateText("Name", rootRect, displayFont, 29f, TextAlignmentOptions.MidlineLeft);
        SetAnchoredRect(nameText.rectTransform, new Vector2(0.37f, 0.66f), new Vector2(0.91f, 0.88f));
        nameText.color = new Color32(239, 226, 194, 255);
        nameText.textWrappingMode = TextWrappingModes.NoWrap;
        nameText.fontSizeMin = 14f;

        CreateStatIcon("Weight Icon", rootRect, "Icons/weight.png", new Vector2(0.38f, 0.39f), new Vector2(0.48f, 0.59f));
        TextMeshProUGUI weightText = CreateText("Weight", rootRect, bodyFont, 30f, TextAlignmentOptions.MidlineLeft);
        SetAnchoredRect(weightText.rectTransform, new Vector2(0.48f, 0.39f), new Vector2(0.61f, 0.59f));
        weightText.color = new Color32(239, 226, 194, 255);
        weightText.fontStyle = FontStyles.Bold;

        CreateStatIcon("Value Icon", rootRect, "Icons/value.png", new Vector2(0.64f, 0.39f), new Vector2(0.74f, 0.59f));
        TextMeshProUGUI valueText = CreateText("Value", rootRect, bodyFont, 30f, TextAlignmentOptions.MidlineLeft);
        SetAnchoredRect(valueText.rectTransform, new Vector2(0.74f, 0.39f), new Vector2(0.88f, 0.59f));
        valueText.color = weightText.color;
        valueText.fontStyle = FontStyles.Bold;

        Image passiveSocket = CreateImage(
            "Passive Effect Socket",
            rootRect,
            LoadSprite("Markers/status-socket-empty.png"),
            Image.Type.Simple);
        SetAnchoredRect(passiveSocket.rectTransform, new Vector2(0.80f, 0.12f), new Vector2(0.90f, 0.32f));
        passiveSocket.preserveAspect = true;
        Image passiveIcon = CreateImage("Passive Effect Icon", passiveSocket.rectTransform, null, Image.Type.Simple);
        SetStretchOffsets(passiveIcon.rectTransform, 8f, 8f, -8f, -8f);
        passiveIcon.preserveAspect = true;
        passiveIcon.enabled = false;

        RectTransform attachment = CreateRect(
            "Rig Attachment Point",
            rootRect,
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f));
        attachment.sizeDelta = new Vector2(40f, 40f);
        Image attachmentKnot = CreateImage(
            "Attachment Knot",
            attachment,
            LoadSprite("Markers/status-socket-empty.png"),
            Image.Type.Simple);
        attachmentKnot.preserveAspect = true;

        Image frame = CreateStateFrame("Card Frame", rootRect, "Frames/compact-card-frame-9slice.png", 3.2f);
        Image hover = CreateStateFrame("Hover Frame", rootRect, "Controls/card-hover-frame-9slice.png", 3.2f);
        Image selected = CreateStateFrame("Selected Frame", rootRect, "Controls/card-selected-frame-9slice.png", 3.2f);
        Image releaseCandidate = CreateStateFrame("Release Candidate Frame", rootRect, "Overlays/release-valid-frame.png", 3.2f);
        hover.gameObject.SetActive(false);
        selected.gameObject.SetActive(false);
        releaseCandidate.gameObject.SetActive(false);

        SerializedObject serializedView = new SerializedObject(view);
        SetReference(serializedView, "portraitImage", portrait);
        SetReference(serializedView, "nameText", nameText);
        SetReference(serializedView, "weightText", weightText);
        SetReference(serializedView, "valueText", valueText);
        SetReference(serializedView, "passiveEffectIcon", passiveIcon);
        SetReference(serializedView, "rigAttachmentPoint", attachment);
        SetReference(serializedView, "selectionButton", button);
        SetReference(serializedView, "hoverFrame", hover);
        SetReference(serializedView, "selectedFrame", selected);
        SetReference(serializedView, "disabledOverlay", disabled);
        SetReference(serializedView, "releaseCandidateFrame", releaseCandidate);
        serializedView.ApplyModifiedPropertiesWithoutUndo();
        view.SetPreview(string.Empty, null, 0, 0, null);

        PrefabUtility.SaveAsPrefabAsset(root, CompactCatchCardPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void CreateStatIcon(string name, Transform parent, string path, Vector2 anchorMin, Vector2 anchorMax)
    {
        Image icon = CreateImage(name, parent, LoadSprite(path), Image.Type.Simple);
        SetAnchoredRect(icon.rectTransform, anchorMin, anchorMax);
        icon.preserveAspect = true;
    }

    private static Image CreateStateFrame(
        string name,
        Transform parent,
        string path,
        float pixelsPerUnitMultiplier)
    {
        Image frame = CreateImage(name, parent, LoadSprite(path), Image.Type.Sliced);
        frame.fillCenter = false;
        frame.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
        return frame;
    }

    /// <summary>Checks compact content, attachment, selection, and non-color state layers.</summary>
    private static void ValidateCompactCatchCard()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CompactCatchCardPath);
        Require(prefab != null, "CompactCatchCard prefab was not created.");
        CompactCatchCardView view = prefab.GetComponent<CompactCatchCardView>();
        Require(view != null, "CompactCatchCardView component is missing.");
        Require(prefab.GetComponent<Button>() != null, "Compact catch card is not selectable.");
        Require(view.RigAttachmentPoint != null, "Compact catch card has no rig attachment point.");
        Require(prefab.transform.Find("Effect Text") == null, "Compact cards must not show full effect descriptions.");
        Require(prefab.transform.Find("Passive Effect Icon") == null, "Passive icon must remain inside its fixed socket.");
        Require(prefab.transform.Find("Passive Effect Socket/Passive Effect Icon") != null, "Passive effect icon slot is missing.");
        Require(prefab.transform.Find("Selected Frame") != null, "Selected state frame is missing.");
        Require(prefab.transform.Find("Disabled Hatch") != null, "Disabled hatch state is missing.");
        Require(prefab.transform.Find("Release Candidate Frame") != null, "Release candidate frame is missing.");
    }

    /// <summary>Renders four readable compact cards, including a three-card Catch Rig stack.</summary>
    private static void CaptureCompactCatchCards()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const int width = 1400;
        const int height = 900;
        Camera camera = CreateValidationCanvas(scene, width, height, out RectTransform canvasRoot, out RenderTexture target);
        CreateSolidBackground(canvasRoot, new Color32(7, 37, 39, 255));
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CompactCatchCardPath);
        Sprite portrait = LoadSprite("Icons/catch.png");
        Sprite passive = LoadSprite("Icons/info.png");
        CompactCatchCardState[] states =
        {
            CompactCatchCardState.Normal,
            CompactCatchCardState.Selected,
            CompactCatchCardState.Disabled
        };
        string[] names = { "COASTAL COD", "MOONLIT RAY", "CLINGING CRAB" };

        for (int i = 0; i < 3; i++)
        {
            CompactCatchCardView card = CreateCompactPreview(
                prefab,
                scene,
                canvasRoot,
                new Rect(85f, 65f + i * 265f, 360f, 240f));
            card.SetPreview(names[i], portrait, 3 + i * 2, 5 + i * 3, i == 1 ? passive : null);
            card.SetState(states[i]);
        }

        CompactCatchCardView release = CreateCompactPreview(
            prefab,
            scene,
            canvasRoot,
            new Rect(700f, 290f, 540f, 360f));
        release.SetPreview("GOLDEN MACKEREL", portrait, 7, 14, passive);
        release.SetState(CompactCatchCardState.ReleaseCandidate);
        SaveRender(camera, target, width, height, "task-3.3-compact-catch-card.png");
    }

    private static CompactCatchCardView CreateCompactPreview(
        GameObject prefab,
        Scene scene,
        RectTransform parent,
        Rect rect)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.SetParent(parent, false);
        Place(instance.GetComponent<RectTransform>(), rect);
        return instance.GetComponent<CompactCatchCardView>();
    }

    /// <summary>Builds the stable Technique slot and its independently animated visual root.</summary>
    private static void BuildTechniqueCard(TMP_FontAsset displayFont, TMP_FontAsset bodyFont)
    {
        GameObject root = CreateUiObject("TechniqueCardView", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(300f, 450f);
        TechniqueCardView view = root.AddComponent<TechniqueCardView>();

        RectTransform visualRoot = CreateRect("Visual Root", rootRect, Vector2.zero, Vector2.one);
        Image background = visualRoot.gameObject.AddComponent<Image>();
        background.sprite = LoadSprite("Cards/Technique/technique-card-base.png");
        background.raycastTarget = true;
        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;

        RectTransform artworkMaskRect = CreateRect(
            "Equipment Artwork Mask",
            visualRoot,
            new Vector2(0.12f, 0.49f),
            new Vector2(0.88f, 0.795f));
        Image artworkMaskImage = artworkMaskRect.gameObject.AddComponent<Image>();
        artworkMaskImage.sprite = LoadSprite("Cards/Technique/technique-art-mask.png");
        artworkMaskImage.preserveAspect = true;
        artworkMaskImage.raycastTarget = false;
        Mask mask = artworkMaskRect.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image artwork = CreateImage("Equipment Illustration", artworkMaskRect, null, Image.Type.Simple);
        artwork.preserveAspect = true;

        Image disabled = CreateImage(
            "Disabled Hatch",
            visualRoot,
            LoadSprite("Cards/card-disabled-overlay.png"),
            Image.Type.Simple);
        disabled.gameObject.SetActive(false);

        Image titlePlate = CreateImage(
            "Title Safe Region",
            visualRoot,
            LoadSprite("Cards/Technique/technique-title-plate-9slice.png"),
            Image.Type.Sliced);
        SetAnchoredRect(titlePlate.rectTransform, new Vector2(0.07f, 0.815f), new Vector2(0.93f, 0.96f));
        titlePlate.pixelsPerUnitMultiplier = 8f;
        TextMeshProUGUI nameText = CreateText("Name", titlePlate.rectTransform, displayFont, 27f, TextAlignmentOptions.Center);
        SetStretchOffsets(nameText.rectTransform, 35f, 16f, -35f, -16f);
        nameText.color = new Color32(239, 226, 194, 255);
        nameText.fontSizeMin = 13f;
        nameText.textWrappingMode = TextWrappingModes.NoWrap;

        Image rulesPanel = CreateImage(
            "Rules Safe Region",
            visualRoot,
            LoadSprite("Cards/Technique/technique-rules-panel-9slice.png"),
            Image.Type.Sliced);
        SetAnchoredRect(rulesPanel.rectTransform, new Vector2(0.10f, 0.17f), new Vector2(0.90f, 0.465f));
        rulesPanel.pixelsPerUnitMultiplier = 7f;
        TextMeshProUGUI rulesText = CreateText("Rules", rulesPanel.rectTransform, bodyFont, 17f, TextAlignmentOptions.Center);
        SetStretchOffsets(rulesText.rectTransform, 30f, 24f, -30f, -24f);
        rulesText.color = new Color32(25, 55, 54, 255);

        TextMeshProUGUI keywordText = CreateText("Keyword", visualRoot, bodyFont, 13f, TextAlignmentOptions.Center);
        SetAnchoredRect(keywordText.rectTransform, new Vector2(0.16f, 0.115f), new Vector2(0.84f, 0.17f));
        keywordText.color = new Color32(212, 185, 123, 255);
        keywordText.fontWeight = FontWeight.SemiBold;
        keywordText.textWrappingMode = TextWrappingModes.NoWrap;

        Image frame = CreateStateFrame("Card Frame", visualRoot, "Frames/technique-card-frame-9slice.png", 3.2f);
        Image hover = CreateStateFrame("Hover Frame", visualRoot, "Controls/card-hover-frame-9slice.png", 3.2f);
        Image selected = CreateStateFrame("Selected Frame", visualRoot, "Controls/card-selected-frame-9slice.png", 3.2f);
        hover.gameObject.SetActive(false);
        selected.gameObject.SetActive(false);

        GameObject playableBadge = CreateTechniqueStateBadge(
            "Playable Badge",
            visualRoot,
            "Controls/action-button-normal-9slice.png",
            "PLAYABLE",
            null,
            displayFont);
        GameObject lockedIndicator = CreateTechniqueStateBadge(
            "Locked Indicator",
            visualRoot,
            "Controls/action-button-disabled-9slice.png",
            "LOCKED",
            LoadSprite("Icons/locked.png"),
            displayFont);
        playableBadge.SetActive(false);
        lockedIndicator.SetActive(false);

        SerializedObject serializedView = new SerializedObject(view);
        SetReference(serializedView, "visualRoot", visualRoot);
        SetReference(serializedView, "artworkImage", artwork);
        SetReference(serializedView, "nameText", nameText);
        SetReference(serializedView, "rulesText", rulesText);
        SetReference(serializedView, "keywordText", keywordText);
        SetReference(serializedView, "useButton", button);
        SetReference(serializedView, "hoverFrame", hover);
        SetReference(serializedView, "selectedFrame", selected);
        SetReference(serializedView, "disabledOverlay", disabled);
        SetReference(serializedView, "playableBadge", playableBadge);
        SetReference(serializedView, "lockedIndicator", lockedIndicator);
        serializedView.ApplyModifiedPropertiesWithoutUndo();
        view.SetPreview(string.Empty, null, string.Empty, string.Empty);
        view.SetState(TechniqueCardVisualState.Normal);

        PrefabUtility.SaveAsPrefabAsset(root, TechniqueCardPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static GameObject CreateTechniqueStateBadge(
        string name,
        Transform parent,
        string backgroundPath,
        string label,
        Sprite icon,
        TMP_FontAsset font)
    {
        Image badge = CreateImage(name, parent, LoadSprite(backgroundPath), Image.Type.Sliced);
        SetAnchoredRect(badge.rectTransform, new Vector2(0.24f, 0.025f), new Vector2(0.76f, 0.115f));
        badge.pixelsPerUnitMultiplier = 10f;
        float textLeft = icon == null ? 18f : 40f;
        if (icon != null)
        {
            Image iconImage = CreateImage("Icon", badge.rectTransform, icon, Image.Type.Simple);
            SetAnchoredRect(iconImage.rectTransform, new Vector2(0.05f, 0.2f), new Vector2(0.25f, 0.8f));
            iconImage.preserveAspect = true;
        }

        TextMeshProUGUI text = CreateText("Label", badge.rectTransform, font, 13f, TextAlignmentOptions.Center);
        SetStretchOffsets(text.rectTransform, textLeft, 5f, -14f, -5f);
        text.color = new Color32(239, 226, 194, 255);
        text.text = label;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return badge.gameObject;
    }

    /// <summary>Checks the Technique card's content regions and all five authored states.</summary>
    private static void ValidateTechniqueCard()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TechniqueCardPath);
        Require(prefab != null, "TechniqueCardView prefab was not created.");
        Require(prefab.GetComponent<TechniqueCardView>() != null, "TechniqueCardView component is missing.");
        Require(prefab.GetComponent<Button>() != null, "Technique card use button is missing.");
        Transform visual = prefab.transform.Find("Visual Root");
        Require(visual != null, "Technique visual root is missing.");
        Require(visual.Find("Equipment Artwork Mask")?.GetComponent<Mask>() != null, "Technique artwork mask is missing.");
        Require(!visual.Find("Equipment Artwork Mask").GetComponent<Mask>().showMaskGraphic, "Technique mask RGB must remain hidden.");
        Require(visual.Find("Title Safe Region/Name") != null, "Technique name field is missing.");
        Require(visual.Find("Rules Safe Region/Rules") != null, "Technique rules field is missing.");
        Require(visual.Find("Keyword") != null, "Technique keyword area is missing.");
        Require(visual.Find("Hover Frame") != null, "Technique hover state is missing.");
        Require(visual.Find("Selected Frame") != null, "Technique selected state is missing.");
        Require(visual.Find("Playable Badge") != null, "Technique playable state needs a non-color badge.");
        Require(visual.Find("Locked Indicator") != null, "Technique disabled state needs a lock indicator.");
    }

    /// <summary>Renders five cards together to test readability and bounded hover elevation.</summary>
    private static void CaptureTechniqueCards()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const int width = 1400;
        const int height = 900;
        Camera camera = CreateValidationCanvas(scene, width, height, out RectTransform canvasRoot, out RenderTexture target);
        CreateSolidBackground(canvasRoot, new Color32(7, 37, 39, 255));
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TechniqueCardPath);
        Sprite artwork = LoadSprite("Icons/tension.png");
        TechniqueCardVisualState[] states =
        {
            TechniqueCardVisualState.Normal,
            TechniqueCardVisualState.Hovered,
            TechniqueCardVisualState.Selected,
            TechniqueCardVisualState.Playable,
            TechniqueCardVisualState.Disabled
        };
        string[] names = { "SLIP KNOT", "DEEP DROP", "CLEAR LENS", "SPARE LINE", "HEAVY GAMBLE" };

        for (int i = 0; i < states.Length; i++)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(canvasRoot, false);
            Place(instance.GetComponent<RectTransform>(), new Rect(45f + i * 270f, 220f, 230f, 345f));
            TechniqueCardView card = instance.GetComponent<TechniqueCardView>();
            card.SetPreview(
                names[i],
                artwork,
                i == 4 ? "Trade certainty for a stronger result on the next Descend." : "Adjust the current fishing attempt, then discard this card.",
                i % 2 == 0 ? "REACTION" : "NEXT DESCEND");
            card.SetState(states[i]);
        }

        SaveRender(camera, target, width, height, "task-3.4-technique-card.png");
    }

    /// <summary>Builds one fixed-geometry icon-and-label command surface.</summary>
    private static void BuildRunActionButton(TMP_FontAsset displayFont)
    {
        GameObject root = CreateUiObject("RunActionButton", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(360f, 104f);
        Image background = root.AddComponent<Image>();
        background.sprite = LoadSprite("Controls/action-button-normal-9slice.png");
        background.type = Image.Type.Sliced;
        background.pixelsPerUnitMultiplier = 6f;
        background.raycastTarget = true;
        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        RunActionButton actionButton = root.AddComponent<RunActionButton>();

        Image icon = CreateImage("Icon", rootRect, LoadSprite("Icons/descend.png"), Image.Type.Simple);
        SetAnchoredRect(icon.rectTransform, new Vector2(0.07f, 0.18f), new Vector2(0.24f, 0.82f));
        icon.preserveAspect = true;

        TextMeshProUGUI label = CreateText("Label", rootRect, displayFont, 31f, TextAlignmentOptions.Center);
        SetAnchoredRect(label.rectTransform, new Vector2(0.22f, 0.17f), new Vector2(0.92f, 0.83f));
        label.color = new Color32(239, 226, 194, 255);
        label.fontSizeMin = 16f;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        Image focus = CreateStateFrame(
            "Controller Focus",
            rootRect,
            "Controls/controller-focus-frame-9slice.png",
            4f);
        focus.gameObject.SetActive(false);

        SerializedObject serializedButton = new SerializedObject(actionButton);
        SetReference(serializedButton, "button", button);
        SetReference(serializedButton, "background", background);
        SetReference(serializedButton, "icon", icon);
        SetReference(serializedButton, "label", label);
        SetReference(serializedButton, "focusFrame", focus);
        SetReference(serializedButton, "normalSprite", LoadSprite("Controls/action-button-normal-9slice.png"));
        SetReference(serializedButton, "hoverSprite", LoadSprite("Controls/action-button-hover-9slice.png"));
        SetReference(serializedButton, "pressedSprite", LoadSprite("Controls/action-button-pressed-9slice.png"));
        SetReference(serializedButton, "disabledSprite", LoadSprite("Controls/action-button-disabled-9slice.png"));
        SetReference(serializedButton, "dangerousSprite", LoadSprite("Controls/action-button-destructive-9slice.png"));
        SetReference(serializedButton, "descendIcon", LoadSprite("Icons/descend.png"));
        SetReference(serializedButton, "releaseIcon", LoadSprite("Icons/release.png"));
        SetReference(serializedButton, "surfaceIcon", LoadSprite("Icons/surface.png"));
        serializedButton.ApplyModifiedPropertiesWithoutUndo();
        actionButton.SetTheme(RunActionTheme.Descend);
        actionButton.SetPreviewState(RunActionButtonVisualState.Normal);

        PrefabUtility.SaveAsPrefabAsset(root, RunActionButtonPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    /// <summary>Checks fixed geometry, runtime command structure, state sprites, and focus treatment.</summary>
    private static void ValidateRunActionButton()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RunActionButtonPath);
        Require(prefab != null, "RunActionButton prefab was not created.");
        Require(prefab.GetComponent<RunActionButton>() != null, "RunActionButton component is missing.");
        Require(prefab.GetComponent<Button>() != null, "RunActionButton requires a Unity Button input target.");
        Image background = prefab.GetComponent<Image>();
        Require(background.type == Image.Type.Sliced, "RunActionButton background must be sliced.");
        Require(prefab.transform.Find("Icon") != null, "RunActionButton icon is missing.");
        Require(prefab.transform.Find("Label")?.GetComponent<TextMeshProUGUI>() != null, "RunActionButton label is missing.");
        Require(prefab.transform.Find("Controller Focus") != null, "RunActionButton controller focus frame is missing.");
    }

    /// <summary>Renders all themes and every required state without changing button geometry.</summary>
    private static void CaptureRunActionButtons()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const int width = 1400;
        const int height = 900;
        Camera camera = CreateValidationCanvas(scene, width, height, out RectTransform canvasRoot, out RenderTexture target);
        CreateSolidBackground(canvasRoot, new Color32(7, 37, 39, 255));
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RunActionButtonPath);
        RunActionTheme[] themes = { RunActionTheme.Descend, RunActionTheme.Release, RunActionTheme.Surface };
        for (int i = 0; i < themes.Length; i++)
        {
            RunActionButton button = CreateActionPreview(
                prefab,
                scene,
                canvasRoot,
                new Rect(85f + i * 440f, 625f, 350f, 104f));
            button.SetTheme(themes[i]);
        }

        RunActionButtonVisualState[] states =
        {
            RunActionButtonVisualState.Normal,
            RunActionButtonVisualState.Hover,
            RunActionButtonVisualState.Pressed,
            RunActionButtonVisualState.Disabled,
            RunActionButtonVisualState.Dangerous
        };
        for (int i = 0; i < states.Length; i++)
        {
            RunActionButton button = CreateActionPreview(
                prefab,
                scene,
                canvasRoot,
                new Rect(45f + i * 270f, 315f, 240f, 82f));
            button.SetTheme(i == 4 ? RunActionTheme.Release : i == 3 ? RunActionTheme.Surface : RunActionTheme.Descend);
            button.SetPreviewState(states[i]);
        }

        SaveRender(camera, target, width, height, "task-3.5-run-action-button.png");
    }

    private static RunActionButton CreateActionPreview(
        GameObject prefab,
        Scene scene,
        RectTransform parent,
        Rect rect)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.SetParent(parent, false);
        Place(instance.GetComponent<RectTransform>(), rect);
        return instance.GetComponent<RunActionButton>();
    }

    private static TMP_FontAsset EnsureFontAsset(string sourcePath, string destinationPath)
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(destinationPath);
        if (existing != null)
        {
            return existing;
        }

        Font source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        Require(source != null, "Source font is missing: " + sourcePath);
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(source);
        Require(fontAsset != null, "Could not create TMP font asset: " + destinationPath);
        fontAsset.name = Path.GetFileNameWithoutExtension(destinationPath);
        fontAsset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
        AssetDatabase.CreateAsset(fontAsset, destinationPath);

        if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTexture != null && !AssetDatabase.Contains(fontAsset.atlasTexture))
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    private static Camera CreateValidationCanvas(
        Scene scene,
        int width,
        int height,
        out RectTransform canvasRoot,
        out RenderTexture target)
    {
        GameObject cameraObject = new GameObject("Validation Camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = height * 0.5f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.transform.position = new Vector3(width * 0.5f, height * 0.5f, -10f);
        target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        target.Create();
        camera.targetTexture = target;

        GameObject canvasObject = new GameObject("Validation Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasRoot = canvasObject.GetComponent<RectTransform>();
        canvasRoot.anchorMin = Vector2.zero;
        canvasRoot.anchorMax = Vector2.zero;
        canvasRoot.pivot = Vector2.zero;
        canvasRoot.sizeDelta = new Vector2(width, height);
        canvasRoot.position = Vector3.zero;
        return camera;
    }

    private static void SaveRender(Camera camera, RenderTexture target, int width, int height, string filename)
    {
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D capture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        capture.Apply();
        File.WriteAllBytes(Path.Combine(ValidationDirectory, filename), capture.EncodeToPNG());
        RenderTexture.active = previous;
        UnityEngine.Object.DestroyImmediate(capture);
        camera.targetTexture = null;
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
    }

    private static void CreateSolidBackground(RectTransform parent, Color color)
    {
        Image image = CreateImage("Background", parent, null, Image.Type.Simple);
        image.color = color;
        image.transform.SetAsFirstSibling();
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = LayerMask.NameToLayer("UI");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }
        return gameObject;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Image.Type type)
    {
        RectTransform rect = CreateRect(name, parent, Vector2.zero, Vector2.one);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.raycastTarget = false;
        return image;
    }

    private static RawImage CreateRawImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
        RawImage image = rect.gameObject.AddComponent<RawImage>();
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent, Vector2.zero, Vector2.one);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(10f, fontSize * 0.55f);
        text.fontSizeMax = fontSize;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static void Place(RectTransform rect, Rect position)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = position.position;
        rect.sizeDelta = position.size;
    }

    private static void SetStretchOffsets(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite LoadSprite(string relativePath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FishingUIAssets/" + relativePath);
        Require(sprite != null, "Missing sprite: " + relativePath);
        return sprite;
    }

    private static Texture LoadTexture(string relativePath)
    {
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/FishingUIAssets/" + relativePath);
        Require(texture != null, "Missing texture: " + relativePath);
        return texture;
    }

    private static void SetReference(SerializedObject target, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        Require(property != null, "Missing serialized property: " + propertyName);
        property.objectReferenceValue = value;
    }

    private static void SetReferenceArray<T>(SerializedObject target, string propertyName, T[] values)
        where T : UnityEngine.Object
    {
        SerializedProperty property = target.FindProperty(propertyName);
        Require(property != null, "Missing serialized array: " + propertyName);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(ComponentDirectory);
        Directory.CreateDirectory(ValidationDirectory);
        Directory.CreateDirectory("Assets/FishingUIAssets/Fonts/TMP");
    }

    private static void DeleteFailure(string filename)
    {
        string path = Path.Combine(ValidationDirectory, filename);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void WriteFailure(string filename, Exception exception)
    {
        Directory.CreateDirectory(ValidationDirectory);
        File.WriteAllText(Path.Combine(ValidationDirectory, filename), exception.ToString());
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
