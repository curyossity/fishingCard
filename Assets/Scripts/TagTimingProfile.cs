using UnityEngine;

/// <summary>Authored tag timing motion, fatigue, precision and rarity boost; never per-run state.</summary>
[CreateAssetMenu(menuName = "Fishing Cards/Encounter Tag Timing")]
public sealed class TagTimingProfile : ScriptableObject
{
    [Min(.05f)] public float traversalsPerSecond = .6f;
    [Min(0)] public float resultHoldSeconds = .65f;
    [Range(.01f, 1)] public float precisionFraction = .18f;
    [Range(0, .9f)] public float selectedShrink = .15f;
    [Range(0, .125f)] public float recoveryPerSelection = .01f;
    [Range(.001f, .125f)] public float minimumShare = .05f;
    [Min(0)] public float resizeSeconds = .3f;
    [Min(1)] public int precisionUncommonWeight = 2;
    [Min(1)] public int precisionRareWeight = 4;
    [Min(1)] public int precisionLegendaryWeight = 8;

    /// <summary>Returns the approved relative per-species weight on a precision hit.</summary>
    public int PrecisionWeight(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Uncommon: return Mathf.Clamp(precisionUncommonWeight, 1, 1024);
            case CardRarity.Rare: return Mathf.Clamp(precisionRareWeight, 1, 1024);
            case CardRarity.Legendary: return Mathf.Clamp(precisionLegendaryWeight, 1, 1024);
            default: return 1;
        }
    }
}
