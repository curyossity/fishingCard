using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Fishing Cards/Biome Definition")]
public sealed class BiomeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string biomeId;
    [SerializeField] private string displayName;
    [TextArea(2, 4)]
    [SerializeField] private string coreIdentity;
    [TextArea(2, 5)]
    [SerializeField] private string strategicTension;
    [SerializeField] private string[] signatureTags = Array.Empty<string>();

    public string BiomeId => biomeId;
    public string DisplayName => displayName;
    public string CoreIdentity => coreIdentity;
    public string StrategicTension => strategicTension;
    public string[] SignatureTags => signatureTags;
}
