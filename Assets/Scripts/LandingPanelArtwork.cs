using UnityEngine;

/// <summary>Pairs one independently authored machine-panel sprite with its visible silhouette, sharing the preserved master texture.</summary>
public sealed class LandingPanelArtwork : ScriptableObject
{
    [SerializeField] private Sprite face;
    [SerializeField] private Vector2[] silhouette;
    public Sprite Face => face;
    public int VertexCount => silhouette == null ? 0 : silhouette.Length;

    /// <summary>Returns an outline point normalized within the original reference-pixel panel bounds.</summary>
    public Vector2 Point(int index) => silhouette[index];
}
