using UnityEditor;
using UnityEngine;

/// <summary>Provides searchable editing of non-catchable event definitions and their biome pool placement.</summary>
public sealed class EventCatalogWindow : CardCatalogWindow
{
    private static readonly string[] scopes = { "All Events", "Hazards", "Environments", "Opportunities", "Other Events" };
    protected override string[] ScopeLabels => scopes;
    protected override string DefinitionsLabel => "Event Definitions";
    protected override string SelectionPrompt => "Select an event to view and edit it.";
    protected override bool IsEventCatalog => true;

    /// <summary>Opens the events collector from the data menu.</summary>
    [MenuItem("Fishing Cards/Data/Events Collector")]
    public static void Open()
    {
        EventCatalogWindow window = GetWindow<EventCatalogWindow>();
        window.titleContent = new GUIContent("Events Collector");
        window.minSize = new Vector2(1100f, 650f);
        window.Show();
    }

    /// <summary>Exposes the same events workspace through the standard Window menu.</summary>
    [MenuItem("Window/Fishing Cards/Events Collector")]
    private static void OpenFromWindowMenu()
    {
        Open();
    }

    /// <summary>Filters only Hazard, Environment, Opportunity, and generic Encounter definitions.</summary>
    protected override bool IsInScope(CardDefinition card, int selectedScope)
    {
        if (card == null) return false;
        switch (card.CardType)
        {
            case CardType.Hazard: return selectedScope == 0 || selectedScope == 1;
            case CardType.Environment: return selectedScope == 0 || selectedScope == 2;
            case CardType.Opportunity: return selectedScope == 0 || selectedScope == 3;
            case CardType.Encounter: return selectedScope == 0 || selectedScope == 4;
            default: return false;
        }
    }
}
