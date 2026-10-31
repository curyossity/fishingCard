using System;
using UnityEngine;

/// <summary>Isolated visual resources for the approved modifier tooltip; never owns gameplay attribution.</summary>
public sealed class JournalStatTooltipStyle : ScriptableObject
{
    [Serializable] public sealed class Icon { public string creatureId; public Sprite sprite; }
    public MidnightFieldJournalTheme theme;
    public Sprite frame, techniqueIcon, eventIcon;
    public Material silhouetteMaterial;
    public Icon[] creatureIcons = Array.Empty<Icon>();

    /// <summary>Uses exact approved glyphs where supplied, otherwise a flat silhouette of that creature's own approved illustration.</summary>
    public Sprite GetIcon(CardDefinition source, out bool silhouette)
    {
        silhouette = false;
        if (source == null || source.CardType == CardType.Technique) return techniqueIcon;
        if (EncounterRuntime.IsEventCard(source)) return eventIcon;
        if (source.CardType == CardType.Equipment) { silhouette = true; return source.Artwork; }
        foreach (Icon icon in creatureIcons) if (icon.creatureId == source.UniqueId) return icon.sprite;
        silhouette = true;
        return theme.GetCentralCard(source.UniqueId)?.sprite;
    }
}
