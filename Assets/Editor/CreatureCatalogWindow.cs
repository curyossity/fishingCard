using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides one editor workspace for reviewing and authoring catch-card data and biome placement.
/// </summary>
public sealed class CreatureCatalogWindow : EditorWindow
{
    private enum CatalogScope
    {
        Creatures,
        AllCatchCards
    }

    private const string CardFolder = "Assets/Cards";
    private const string BiomeFolder = "Assets/Biomes";
    private const float CatalogWidth = 620f;
    private const float PreviewSize = 140f;

    private readonly List<CardDefinition> catalogCards = new List<CardDefinition>();
    private readonly List<BiomeDefinition> biomes = new List<BiomeDefinition>();
    private Vector2 catalogScroll;
    private Vector2 detailsScroll;
    private string searchText = string.Empty;
    private CatalogScope scope = CatalogScope.Creatures;
    private CardDefinition selectedCard;

    [MenuItem("Fishing Cards/Data/Creature Catalog")]
    public static void Open()
    {
        CreatureCatalogWindow window = GetWindow<CreatureCatalogWindow>();
        window.titleContent = new GUIContent("Creature Catalog");
        window.minSize = new Vector2(1100f, 650f);
        window.Show();
    }

    /// <summary>Provides the same catalog in Unity's standard Window menu for easier discovery.</summary>
    [MenuItem("Window/Fishing Cards/Creature Catalog")]
    private static void OpenFromWindowMenu()
    {
        Open();
    }

    /// <summary>Loads authored cards and biome definitions whenever the window becomes active.</summary>
    private void OnEnable()
    {
        RefreshCatalog();
        Undo.undoRedoPerformed += HandleUndoRedo;
    }

    /// <summary>Releases editor callbacks when the catalog closes or reloads.</summary>
    private void OnDisable()
    {
        Undo.undoRedoPerformed -= HandleUndoRedo;
    }

    /// <summary>Keeps the catalog synchronized when assets are created, deleted, or moved.</summary>
    private void OnProjectChange()
    {
        RefreshCatalog();
        Repaint();
    }

    /// <summary>Refreshes visible values after an Undo or Redo operation.</summary>
    private void HandleUndoRedo()
    {
        RefreshCatalog();
        Repaint();
    }

    /// <summary>Draws the catalog list and selected-card authoring surface.</summary>
    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.BeginHorizontal();
        DrawCatalogPane();
        DrawDetailsPane();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Draws search, scope, refresh, and persistence controls.</summary>
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Search", GUILayout.Width(44f));
        string updatedSearch = GUILayout.TextField(
            searchText,
            GUI.skin.FindStyle("ToolbarSearchTextField"),
            GUILayout.MinWidth(180f));
        if (!string.Equals(updatedSearch, searchText, StringComparison.Ordinal))
        {
            searchText = updatedSearch;
            catalogScroll = Vector2.zero;
        }

        CatalogScope updatedScope = (CatalogScope)EditorGUILayout.EnumPopup(
            scope,
            EditorStyles.toolbarPopup,
            GUILayout.Width(130f));
        if (updatedScope != scope)
        {
            scope = updatedScope;
            catalogScroll = Vector2.zero;
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
        {
            RefreshCatalog();
        }

        using (new EditorGUI.DisabledScope(selectedCard == null))
        {
            if (GUILayout.Button("Locate Asset", EditorStyles.toolbarButton, GUILayout.Width(90f)))
            {
                Selection.activeObject = selectedCard;
                EditorGUIUtility.PingObject(selectedCard);
            }
        }

        if (GUILayout.Button("Save Changes", EditorStyles.toolbarButton, GUILayout.Width(96f)))
        {
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Draws a searchable overview of creature and catch-card definitions.</summary>
    private void DrawCatalogPane()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(CatalogWidth));
        EditorGUILayout.LabelField(
            scope == CatalogScope.Creatures ? "Creature Definitions" : "All Catch-Card Definitions",
            EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a row to edit every authored field. Biome IDs and depth values are eligibility constraints; actual tier membership is edited in the details pane.",
            MessageType.Info);

        DrawCatalogHeader();
        catalogScroll = EditorGUILayout.BeginScrollView(catalogScroll);

        int visibleCount = 0;
        for (int i = 0; i < catalogCards.Count; i++)
        {
            CardDefinition card = catalogCards[i];
            if (!IsInScope(card) || !MatchesSearch(card))
            {
                continue;
            }

            visibleCount++;
            DrawCatalogRow(card, visibleCount);
        }

        if (visibleCount == 0)
        {
            EditorGUILayout.HelpBox("No cards match the current scope and search.", MessageType.None);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField($"Showing {visibleCount} of {catalogCards.Count} card assets", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws stable column labels for the data overview.</summary>
    private static void DrawCatalogHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Name", EditorStyles.miniBoldLabel, GUILayout.Width(170f));
        GUILayout.Label("Type", EditorStyles.miniBoldLabel, GUILayout.Width(88f));
        GUILayout.Label("Rarity", EditorStyles.miniBoldLabel, GUILayout.Width(76f));
        GUILayout.Label("W", EditorStyles.miniBoldLabel, GUILayout.Width(28f));
        GUILayout.Label("V", EditorStyles.miniBoldLabel, GUILayout.Width(28f));
        GUILayout.Label("Biome constraints", EditorStyles.miniBoldLabel, GUILayout.Width(125f));
        GUILayout.Label("Depth", EditorStyles.miniBoldLabel, GUILayout.Width(72f));
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Draws one selectable summary row.</summary>
    private void DrawCatalogRow(CardDefinition card, int visibleIndex)
    {
        Color previousColor = GUI.backgroundColor;
        if (card == selectedCard)
        {
            GUI.backgroundColor = new Color(0.45f, 0.78f, 0.74f, 1f);
        }
        else if ((visibleIndex & 1) == 0)
        {
            GUI.backgroundColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUI.backgroundColor = previousColor;
        if (GUILayout.Button(card.DisplayName, EditorStyles.label, GUILayout.Width(170f)))
        {
            SelectCard(card);
        }

        GUILayout.Label(card.CardType.ToString(), GUILayout.Width(88f));
        GUILayout.Label(card.Rarity.ToString(), GUILayout.Width(76f));
        GUILayout.Label(card.Weight.ToString(), GUILayout.Width(28f));
        GUILayout.Label(card.Value.ToString(), GUILayout.Width(28f));
        GUILayout.Label(BuildBiomeSummary(card), GUILayout.Width(125f));
        GUILayout.Label(BuildDepthSummary(card), GUILayout.Width(72f));
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Draws all serialized fields, art previews, and actual biome-tier placement.</summary>
    private void DrawDetailsPane()
    {
        EditorGUILayout.BeginVertical();
        if (selectedCard == null)
        {
            EditorGUILayout.HelpBox("Select a creature from the catalog to view and edit it.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);
        SerializedObject cardObject = new SerializedObject(selectedCard);
        cardObject.UpdateIfRequiredOrScript();

        EditorGUILayout.LabelField(selectedCard.DisplayName, EditorStyles.largeLabel);
        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(selectedCard), EditorStyles.miniLabel);
        EditorGUILayout.Space(6f);

        EditorGUI.BeginChangeCheck();
        DrawSection("Identity", cardObject, "uniqueId", "displayName", "cardType", "rarity");
        DrawTagsSection(cardObject);
        DrawArtworkSection(cardObject);
        DrawRulesTextSection(cardObject);
        DrawSection(
            "Catch Stats",
            cardObject,
            "weight",
            "value",
            "randomizeValueWhenCaught",
            "minimumCaughtValue",
            "maximumCaughtValue");
        DrawSection("Availability Constraints", cardObject, "biomeIds", "minimumDepth", "maximumDepth");
        DrawSection("Catch Effects", cardObject, "effects");
        EditorGUILayout.HelpBox(
            "When Bait Effects is empty, the card temporarily uses its Catch Effects as the Bait fallback. Add any Bait Effect entry to replace that fallback.",
            MessageType.None);
        DrawSection("Bait Effects", cardObject, "baitEffects");
        bool cardFieldsChanged = EditorGUI.EndChangeCheck();

        if (cardObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(selectedCard);
            if (cardFieldsChanged)
            {
                SortCards();
                Repaint();
            }
        }

        DrawBiomeMembershipSection();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>Uses the closed creature vocabulary while preserving free-form tags for non-creature card groups.</summary>
    private static void DrawTagsSection(SerializedObject cardObject)
    {
        SerializedProperty cardTypeProperty = cardObject.FindProperty("cardType");
        CardType cardType = (CardType)cardTypeProperty.enumValueIndex;
        if (!CreatureTagVocabulary.AppliesTo(cardType))
        {
            DrawSection("Tags", cardObject, "tags");
            return;
        }

        SerializedProperty tagsProperty = cardObject.FindProperty("tags");
        int currentMask = 0;
        for (int tagIndex = 0; tagIndex < CreatureTagVocabulary.All.Length; tagIndex++)
        {
            for (int authoredIndex = 0; authoredIndex < tagsProperty.arraySize; authoredIndex++)
            {
                if (string.Equals(
                    tagsProperty.GetArrayElementAtIndex(authoredIndex).stringValue,
                    CreatureTagVocabulary.All[tagIndex],
                    StringComparison.OrdinalIgnoreCase))
                {
                    currentMask |= 1 << tagIndex;
                    break;
                }
            }
        }

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Creature Tags", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.HelpBox(
            "Creature and Apex creature cards use the controlled gameplay tag vocabulary. Choose every applicable trait.",
            MessageType.None);
        int updatedMask = EditorGUILayout.MaskField("Tags", currentMask, CreatureTagVocabulary.All);
        if (updatedMask != currentMask)
        {
            int selectedCount = 0;
            for (int i = 0; i < CreatureTagVocabulary.All.Length; i++)
            {
                if ((updatedMask & (1 << i)) != 0)
                {
                    selectedCount++;
                }
            }

            tagsProperty.arraySize = selectedCount;
            int destinationIndex = 0;
            for (int i = 0; i < CreatureTagVocabulary.All.Length; i++)
            {
                if ((updatedMask & (1 << i)) == 0)
                {
                    continue;
                }

                tagsProperty.GetArrayElementAtIndex(destinationIndex).stringValue = CreatureTagVocabulary.All[i];
                destinationIndex++;
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws rules copy and its central-card word/phrase emphasis metadata.</summary>
    private static void DrawRulesTextSection(SerializedObject cardObject)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Catch / Bait Rules Text", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Catch", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(cardObject.FindProperty("rulesText"), true);
        EditorGUILayout.Space(4f);
        EditorGUILayout.HelpBox(
            "Add a highlighted word or phrase, then choose its color and relative size. Every matching occurrence is styled on the central encounter card. Whole Word prevents partial matches inside longer words.",
            MessageType.None);
        EditorGUILayout.PropertyField(cardObject.FindProperty("rulesTextHighlights"), true);
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Bait", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox(
            "Leave Bait Rules Text and highlights empty to mirror the Catch presentation until a distinct Bait effect is authored.",
            MessageType.None);
        EditorGUILayout.PropertyField(cardObject.FindProperty("baitRulesText"), true);
        EditorGUILayout.PropertyField(cardObject.FindProperty("baitRulesTextHighlights"), true);
        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws a titled group of serialized card fields.</summary>
    private static void DrawSection(string heading, SerializedObject serializedObject, params string[] propertyNames)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        for (int i = 0; i < propertyNames.Length; i++)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyNames[i]);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws every authored art reference alongside visual previews.</summary>
    private void DrawArtworkSection(SerializedObject cardObject)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Artwork", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        SerializedProperty artwork = cardObject.FindProperty("artwork");
        SerializedProperty encounterArtwork = cardObject.FindProperty("encounterArtwork");
        SerializedProperty cardFaceArtwork = cardObject.FindProperty("cardFaceArtwork");
        EditorGUILayout.PropertyField(artwork);
        EditorGUILayout.PropertyField(encounterArtwork);
        EditorGUILayout.PropertyField(cardObject.FindProperty("artworkLayout"));
        EditorGUILayout.PropertyField(cardObject.FindProperty("encounterArtworkScale"));
        EditorGUILayout.PropertyField(cardObject.FindProperty("encounterArtworkRotation"));
        EditorGUILayout.PropertyField(cardObject.FindProperty("encounterArtworkOffset"));
        EditorGUILayout.PropertyField(cardFaceArtwork);
        EditorGUILayout.PropertyField(cardObject.FindProperty("cardFaceIncludesName"));

        EditorGUILayout.BeginHorizontal();
        DrawSpritePreview("Compact / portrait", artwork.objectReferenceValue as Sprite);
        DrawSpritePreview("Encounter overlay", encounterArtwork.objectReferenceValue as Sprite);
        DrawSpritePreview("Legacy complete face", cardFaceArtwork.objectReferenceValue as Sprite);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws one aspect-preserving sprite preview.</summary>
    private void DrawSpritePreview(string label, Sprite sprite)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(PreviewSize));
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel, GUILayout.Width(PreviewSize));
        Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.Width(PreviewSize));
        EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));

        if (sprite != null)
        {
            Texture preview = AssetPreview.GetAssetPreview(sprite);
            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(sprite);
                if (AssetPreview.IsLoadingAssetPreview(sprite.GetEntityId()))
                {
                    Repaint();
                }
            }

            if (preview != null)
            {
                EditorGUI.DrawPreviewTexture(previewRect, preview, null, ScaleMode.ScaleToFit);
            }
        }
        else
        {
            GUI.Label(previewRect, "No sprite assigned", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws editable membership in each biome's depth-tier pool and Apex list.</summary>
    private void DrawBiomeMembershipSection()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Actual Encounter Pool Membership", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "A checked tier means this card is present in that biome tier's encounter pool. The card's availability constraints above are applied afterward.",
            MessageType.Info);

        if (biomes.Count == 0)
        {
            EditorGUILayout.HelpBox("No BiomeDefinition assets were found in Assets/Biomes.", MessageType.Warning);
            return;
        }

        for (int biomeIndex = 0; biomeIndex < biomes.Count; biomeIndex++)
        {
            DrawBiomeMembership(biomes[biomeIndex]);
        }
    }

    /// <summary>Draws membership toggles for one biome definition.</summary>
    private void DrawBiomeMembership(BiomeDefinition biome)
    {
        SerializedObject biomeObject = new SerializedObject(biome);
        biomeObject.UpdateIfRequiredOrScript();
        SerializedProperty displayName = biomeObject.FindProperty("displayName");
        SerializedProperty depthTiers = biomeObject.FindProperty("depthTiers");
        SerializedProperty apexEncounters = biomeObject.FindProperty("apexEncounters");

        string biomeLabel = displayName == null || string.IsNullOrWhiteSpace(displayName.stringValue)
            ? biome.name
            : displayName.stringValue;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(biomeLabel, EditorStyles.boldLabel);
        if (GUILayout.Button("Locate", GUILayout.Width(58f)))
        {
            Selection.activeObject = biome;
            EditorGUIUtility.PingObject(biome);
        }

        EditorGUILayout.EndHorizontal();

        bool changed = false;
        if (depthTiers != null)
        {
            for (int tierIndex = 0; tierIndex < depthTiers.arraySize; tierIndex++)
            {
                SerializedProperty tier = depthTiers.GetArrayElementAtIndex(tierIndex);
                SerializedProperty tierName = tier.FindPropertyRelative("displayName");
                SerializedProperty minimumDepth = tier.FindPropertyRelative("minimumDepth");
                SerializedProperty maximumDepth = tier.FindPropertyRelative("maximumDepth");
                SerializedProperty encounterPool = tier.FindPropertyRelative("encounterPool");
                string label = BuildTierLabel(tierName, minimumDepth, maximumDepth, tierIndex);
                bool containsCard = FindReferenceIndex(encounterPool, selectedCard) >= 0;
                bool updatedContainsCard = EditorGUILayout.ToggleLeft(label, containsCard);
                if (updatedContainsCard != containsCard)
                {
                    SetReferenceMembership(encounterPool, selectedCard, updatedContainsCard);
                    changed = true;
                }
            }
        }

        if (selectedCard.CardType == CardType.ApexEncounter && apexEncounters != null)
        {
            bool containsApex = FindReferenceIndex(apexEncounters, selectedCard) >= 0;
            bool updatedContainsApex = EditorGUILayout.ToggleLeft("Apex possibilities", containsApex);
            if (updatedContainsApex != containsApex)
            {
                SetReferenceMembership(apexEncounters, selectedCard, updatedContainsApex);
                changed = true;
            }
        }

        if (changed && biomeObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(biome);
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>Builds a readable depth-tier membership label.</summary>
    private static string BuildTierLabel(
        SerializedProperty tierName,
        SerializedProperty minimumDepth,
        SerializedProperty maximumDepth,
        int tierIndex)
    {
        string name = tierName == null || string.IsNullOrWhiteSpace(tierName.stringValue)
            ? $"Tier {tierIndex + 1}"
            : tierName.stringValue;
        int minimum = minimumDepth?.intValue ?? 0;
        int maximum = maximumDepth?.intValue ?? -1;
        string maximumLabel = maximum < 0 ? "∞" : maximum.ToString();
        return $"{name} (Depth {minimum}–{maximumLabel})";
    }

    /// <summary>Adds or removes an object reference from a serialized array.</summary>
    private static void SetReferenceMembership(
        SerializedProperty array,
        UnityEngine.Object referencedObject,
        bool shouldContain)
    {
        if (array == null || !array.isArray)
        {
            return;
        }

        int existingIndex = FindReferenceIndex(array, referencedObject);
        if (shouldContain && existingIndex < 0)
        {
            int newIndex = array.arraySize;
            array.InsertArrayElementAtIndex(newIndex);
            array.GetArrayElementAtIndex(newIndex).objectReferenceValue = referencedObject;
            return;
        }

        if (!shouldContain && existingIndex >= 0)
        {
            int originalSize = array.arraySize;
            array.DeleteArrayElementAtIndex(existingIndex);
            if (array.arraySize == originalSize)
            {
                array.DeleteArrayElementAtIndex(existingIndex);
            }
        }
    }

    /// <summary>Returns the matching object-reference index, or -1 when absent.</summary>
    private static int FindReferenceIndex(SerializedProperty array, UnityEngine.Object referencedObject)
    {
        if (array == null || !array.isArray)
        {
            return -1;
        }

        for (int index = 0; index < array.arraySize; index++)
        {
            if (array.GetArrayElementAtIndex(index).objectReferenceValue == referencedObject)
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Selects one card without losing the catalog window's current filtering.</summary>
    private void SelectCard(CardDefinition card)
    {
        selectedCard = card;
        detailsScroll = Vector2.zero;
        Selection.activeObject = card;
        Repaint();
    }

    /// <summary>Loads and sorts all authored card and biome assets.</summary>
    private void RefreshCatalog()
    {
        catalogCards.Clear();
        string[] cardGuids = AssetDatabase.FindAssets("t:CardDefinition", new[] { CardFolder });
        for (int i = 0; i < cardGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(cardGuids[i]);
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (card != null)
            {
                catalogCards.Add(card);
            }
        }

        SortCards();

        biomes.Clear();
        string[] biomeGuids = AssetDatabase.FindAssets("t:BiomeDefinition", new[] { BiomeFolder });
        for (int i = 0; i < biomeGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(biomeGuids[i]);
            BiomeDefinition biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>(path);
            if (biome != null)
            {
                biomes.Add(biome);
            }
        }

        biomes.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
        if (selectedCard == null && catalogCards.Count > 0)
        {
            selectedCard = FindFirstVisibleCard();
        }
    }

    /// <summary>Sorts card definitions by display name while keeping blank names deterministic.</summary>
    private void SortCards()
    {
        catalogCards.Sort((left, right) => string.Compare(
            left == null ? string.Empty : left.DisplayName,
            right == null ? string.Empty : right.DisplayName,
            StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Finds the first card included by the current catalog scope.</summary>
    private CardDefinition FindFirstVisibleCard()
    {
        for (int i = 0; i < catalogCards.Count; i++)
        {
            if (IsInScope(catalogCards[i]))
            {
                return catalogCards[i];
            }
        }

        return null;
    }

    /// <summary>Checks whether a card belongs to the selected catalog scope.</summary>
    private bool IsInScope(CardDefinition card)
    {
        if (card == null)
        {
            return false;
        }

        if (scope == CatalogScope.Creatures)
        {
            return card.CardType == CardType.Creature;
        }

        return card.CardType == CardType.Creature
            || card.CardType == CardType.Treasure
            || card.CardType == CardType.ApexEncounter;
    }

    /// <summary>Matches search text against names, IDs, tags, and biome constraints.</summary>
    private bool MatchesSearch(CardDefinition card)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return ContainsSearch(card.DisplayName)
            || ContainsSearch(card.UniqueId)
            || ContainsSearch(card.CardType.ToString())
            || ContainsSearch(card.Rarity.ToString())
            || ArrayContainsSearch(card.Tags)
            || ArrayContainsSearch(card.BiomeIds);
    }

    /// <summary>Checks a single value against the case-insensitive catalog query.</summary>
    private bool ContainsSearch(string value)
    {
        return !string.IsNullOrEmpty(value)
            && value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>Checks every string in an authored array against the catalog query.</summary>
    private bool ArrayContainsSearch(string[] values)
    {
        if (values == null)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (ContainsSearch(values[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Builds the compact biome-constraint value shown in the overview.</summary>
    private static string BuildBiomeSummary(CardDefinition card)
    {
        string[] biomeIds = card.BiomeIds;
        return biomeIds == null || biomeIds.Length == 0 ? "All" : string.Join(", ", biomeIds);
    }

    /// <summary>Builds the compact inclusive depth range shown in the overview.</summary>
    private static string BuildDepthSummary(CardDefinition card)
    {
        string maximum = card.MaximumDepth < 0 ? "∞" : card.MaximumDepth.ToString();
        return $"{card.MinimumDepth}–{maximum}";
    }
}
