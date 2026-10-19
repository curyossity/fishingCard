using TMPro;
using UnityEngine;

/// <summary>Optional authored HUD surfaces; the shared run view retains commands and encounter transitions.</summary>
public abstract class FishingRunHudPresentation : MonoBehaviour
{
    public abstract TMP_Text BiomeText { get; }
    public abstract TMP_Text DepthText { get; }
    public abstract TMP_Text DeckText { get; }
    public abstract UnityEngine.UI.Button DescendButton { get; }
    public abstract UnityEngine.UI.Button SurfaceButton { get; }
    public abstract UnityEngine.UI.Button SettingsButton { get; }
    /// <summary>Displays controller-owned navigation, load and haul without calculating gameplay consequences.</summary>
    public abstract void Refresh(BiomeDefinition biome, int depth, int load, int capacity, int deckCount, int? haulValue);
    /// <summary>Displays learned trail information outside the encounter face.</summary>
    public abstract void SetTrailPreview(string preview);
    /// <summary>Shows optional presentation-only boat artwork before the first encounter.</summary>
    public virtual void SetOpeningVisible(bool visible) { }
}
