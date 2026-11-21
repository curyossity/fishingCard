using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Names presentation events; none of these cues evaluates a gameplay rule or awards a reward.</summary>
public enum DeepwaterSoundCue
{
    TagSelect, TagDeselect, Preview, SlotStart, SlotRoll, SlotTick, SlotStop1, SlotStop2, SlotStop3,
    SlotLock, SlotUnlock, Pair, Triple, Reveal, Strike, Good, Perfect, Miss, Addition, Multiplier,
    Travel, PayoutSmall, PayoutMedium, PayoutBig, Failure, BuildTravel, Attach, Upgrade
}

/// <summary>Separates quiet decisions, moving apparatus and foreground score resolution in the mix.</summary>
public enum DeepwaterSoundBus { Interface, Motion, Score }

/// <summary>Editable cue identity and mix; take order belongs to each player, never to this shared asset.</summary>
[CreateAssetMenu(menuName = "Deepwater/Audio/Sound Library")]
public sealed class DeepwaterSoundLibrary : ScriptableObject
{
    [Serializable]
    public sealed class Definition
    {
        public DeepwaterSoundCue Cue;
        public AudioClip[] Takes = Array.Empty<AudioClip>();
        [Range(-60, 0)] public float GainDb = -12;
        public DeepwaterSoundBus Bus;
        [Range(0, 256)] public int Priority = 100;
        [Min(0)] public float Cooldown;
        [Range(0, 1)] public float PitchGrowth = 1;
        [Range(0, 3)] public float IntensityGainDb = 1.5f;
    }

    [SerializeField] private Definition[] cues = Array.Empty<Definition>();
    public IReadOnlyList<Definition> Definitions => Array.AsReadOnly(cues);

    /// <summary>Looks up the small authored vocabulary; a missing cue stays silent instead of inventing a fallback.</summary>
    public Definition Find(DeepwaterSoundCue cue)
    {
        foreach (var definition in cues) if (definition.Cue == cue) return definition;
        return null;
    }
}
