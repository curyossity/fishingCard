using UnityEngine;

/// <summary>Authored proportions and motion for creature encounter timing.</summary>
[CreateAssetMenu(menuName = "Fishing Cards/Encounter Rarity Timing")]
public sealed class RarityTimingProfile : ScriptableObject
{
    [Min(0)] public float common = 87, uncommon = 5, rare = 8, legendary;
    [Min(0)] public float miss;
    [Min(.05f)] public float traversalsPerSecond = .6f;
    [Min(0)] public float resultHoldSeconds = .65f;

    /// <summary>Returns an authored relative rarity width, before eligibility filtering.</summary>
    public float Width(CardRarity rarity)
    {
        switch (rarity) { case CardRarity.Common: return common; case CardRarity.Uncommon: return uncommon;
            case CardRarity.Rare: return rare; default: return legendary; }
    }
}
