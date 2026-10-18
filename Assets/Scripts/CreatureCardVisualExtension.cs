using UnityEngine;

/// <summary>Optional theme-owned artwork and metadata presentation without gameplay or source-asset mutations.</summary>
public abstract class CreatureCardVisualExtension : MonoBehaviour
{
    /// <summary>Returns this theme's exact illustration, including null when the card has no approved artwork.</summary>
    public abstract Sprite GetArtwork(CardDefinition card);
    /// <summary>Updates permitted metadata after the shared view applies information masks and card-type layout.</summary>
    public abstract void Refresh(CardDefinition card, bool informationHidden, bool identityHidden, bool catchLayout);
    /// <summary>Clears all identity-bearing decoration when the shared card is cleared or blanked.</summary>
    public abstract void Clear();
    /// <summary>Updates optional theme-owned role borders without resolving gameplay.</summary>
    public virtual void RefreshSelection(EncounterEffectChoice choice, bool active) { }
}
