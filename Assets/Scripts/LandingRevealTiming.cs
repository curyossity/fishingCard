using UnityEngine;

/// <summary>
/// Rarity-scaled pacing for the encounter creature's entrance into the Landing machine. Presentation only: rarity is already
/// resolved by Discover, and the reveal never alters the creature, its Resistance or the challenge's motion.
/// </summary>
public readonly struct LandingRevealTiming
{
    public readonly CardRarity Rarity;
    /// <summary>Machine rumble before the face appears; zero for ordinary creatures.</summary>
    public readonly float Anticipation;
    /// <summary>Vertical turns that open the face from edge-on, ending face-up.</summary>
    public readonly float Turning;
    /// <summary>Scale pop, contact flash and stage impulse after the final turn.</summary>
    public readonly float Settle;
    /// <summary>Number of times the face swings into view, counting the final face-up stop.</summary>
    public readonly int Turns;
    public readonly float Pop, Flash, Shake, Pitch, GainDb;
    public float Duration => Anticipation + Turning + Settle;
    public float SettleAt => Anticipation + Turning;
    /// <summary>Rare creatures and above accelerate into their final turn; ordinary ones spin fast and settle.</summary>
    public bool Accelerates => Rarity >= CardRarity.Rare;
    /// <summary>Upper bound across every rarity, used by validation and any caller that needs a fixed ceiling.</summary>
    public const float MaximumDuration = 1.3f;

    private LandingRevealTiming(CardRarity rarity, float anticipation, float turning, float settle, int turns, float pop, float flash, float shake, float pitch, float gainDb)
    {
        Rarity = rarity; Anticipation = anticipation; Turning = turning; Settle = settle; Turns = turns;
        Pop = pop; Flash = flash; Shake = shake; Pitch = pitch; GainDb = gainDb;
    }

    /// <summary>Common is a quick acknowledgment; each tier adds turns, then anticipation, flash and impulse, keeping routine reveals light.</summary>
    public static LandingRevealTiming For(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Uncommon: return new LandingRevealTiming(rarity, 0, .38f, .16f, 2, .04f, 0, 0, 1.06f, 1);
            case CardRarity.Rare: return new LandingRevealTiming(rarity, .22f, .46f, .22f, 3, .06f, .55f, 6, 1.12f, 2);
            case CardRarity.Legendary: return new LandingRevealTiming(rarity, .42f, .58f, .28f, 3, .08f, .85f, 10, 1.18f, 3);
            default: return new LandingRevealTiming(CardRarity.Common, 0, .24f, .14f, 1, .03f, 0, 0, 1, 0);
        }
    }

    /// <summary>Face rotation in radians: starts edge-on (π/2) and ends face-up after the configured turns.</summary>
    public float Angle(float elapsed)
    {
        float u = Mathf.Clamp01((elapsed - Anticipation) / Mathf.Max(.001f, Turning));
        float eased = Accelerates ? u * u * u * (u * (u * 6 - 15) + 10) : 1 - (1 - u) * (1 - u) * (1 - u);
        return Mathf.PI * (.5f + eased * (Turns - .5f));
    }

    /// <summary>Vertical projection of the turning face; never fully zero so the panel keeps valid geometry.</summary>
    public float FaceScale(float elapsed) => elapsed < Anticipation ? .015f : Mathf.Max(.015f, Mathf.Abs(Mathf.Cos(Angle(elapsed))));

    /// <summary>Hides the face while edge-on so a turning panel never shows a sliver of the wrong side.</summary>
    public float FaceOpacity(float elapsed, bool reduced)
    {
        if (reduced) return Mathf.SmoothStep(0, 1, Mathf.InverseLerp(Anticipation, SettleAt, elapsed));
        if (elapsed < Anticipation) return 0;
        if (elapsed >= SettleAt) return 1;
        float half = Angle(elapsed) / Mathf.PI;
        return LandingCardHandoff.FaceOpacity(half - Mathf.Floor(half), false);
    }

    /// <summary>The name band stays blank until the face has settled, so the title lands with the card.</summary>
    public float NameOpacity(float elapsed) => Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - SettleAt) / Mathf.Max(.001f, Settle * .6f)));

    /// <summary>Uniform scale pop on settling: a damped overshoot that returns to exactly 1 at the end.</summary>
    public float SettleScale(float elapsed)
    {
        if (elapsed < SettleAt || elapsed >= Duration) return 1;
        float s = (elapsed - SettleAt) / Mathf.Max(.001f, Settle);
        return 1 + Pop * Mathf.Exp(-s * 4) * Mathf.Sin(s * Mathf.PI * 2) * (1 - s);
    }

    /// <summary>55 ms white contact pulse at settle for Rare and above, matching the scoring flash profile.</summary>
    public float FlashOpacity(float elapsed)
    {
        float age = elapsed - SettleAt;
        if (Flash <= 0 || age < 0 || age >= .055f) return 0;
        float fade = Mathf.Clamp01((age - .012f) / .043f);
        return Flash * (1 - fade) * (1 - fade);
    }

    /// <summary>Stage offset: a rising rumble during anticipation, then one decaying impulse at settle. Summed sines, never per-frame noise.</summary>
    public Vector2 StageOffset(float elapsed)
    {
        if (Shake <= 0) return Vector2.zero;
        if (elapsed < Anticipation)
        {
            float rise = elapsed / Anticipation; float amount = Shake * .3f * rise * rise;
            return new Vector2(Mathf.Sin(elapsed * 90), Mathf.Sin(elapsed * 123) * .5f) * amount;
        }
        float age = elapsed - SettleAt;
        if (age < 0 || age > .32f) return Vector2.zero;
        float envelope = (1 - Mathf.Exp(-age * 170)) * Mathf.Exp(-age * 18) * Shake;
        return new Vector2(Mathf.Sin(age * 100), Mathf.Sin(age * 137) * .43f) * envelope;
    }

    /// <summary>Message-band text while the entrance plays; rarity itself is announced only by the Rare/Legendary banner.</summary>
    public const string Caption = "SPACE TO SKIP";
}
