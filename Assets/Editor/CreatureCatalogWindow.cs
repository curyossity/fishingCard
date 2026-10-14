using UnityEditor;
using UnityEngine;

/// <summary>Provides the creature and catch-card catalog using the shared card authoring workspace.</summary>
public sealed class CreatureCatalogWindow : CardCatalogWindow
{
    private static readonly string[] scopes = { "Creatures", "All Catch Cards" };
    protected override string[] ScopeLabels => scopes;
    protected override string DefinitionsLabel => "Creature / Catch-Card Definitions";
    protected override string SelectionPrompt => "Select a creature or catch card to view and edit it.";

    /// <summary>Opens the existing creature catalog from the data menu.</summary>
    [MenuItem("Fishing Cards/Data/Creature Catalog")]
    public static void Open()
    {
        CreatureCatalogWindow window = GetWindow<CreatureCatalogWindow>();
        window.titleContent = new GUIContent("Creature Catalog");
        window.minSize = new Vector2(1100f, 650f);
        window.Show();
    }

    /// <summary>Exposes the same creature workspace through the standard Window menu.</summary>
    [MenuItem("Window/Fishing Cards/Creature Catalog")]
    private static void OpenFromWindowMenu()
    {
        Open();
    }

    /// <summary>Includes creatures by default and Treasure/Apex when all catch cards are selected.</summary>
    protected override bool IsInScope(CardDefinition card, int selectedScope)
    {
        return card != null && (card.CardType == CardType.Creature || selectedScope == 1
            && (card.CardType == CardType.Treasure || card.CardType == CardType.ApexEncounter));
    }
}
