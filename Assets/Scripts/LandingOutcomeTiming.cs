using UnityEngine;

/// <summary>How a Landing challenge ended, as presented after its score feedback.</summary>
public enum LandingOutcomeKind { Landed, Escaped }

/// <summary>
/// Pure schedule for the beat that closes a Landing challenge. Landed: the creature card is yanked up out of the water and slams
/// back into its frame under a LANDED stamp. Escaped: the creature wriggles and sinks away under an ESCAPED stamp, then every
/// previously landed creature in the haul is cashed out one by one into the full haul payout. Presentation only: the outcome,
/// the payout and the rig cleanup have already resolved when this plays.
/// </summary>
public readonly struct LandingOutcomeTiming
{
    // Landed: dip, yank, hang, drop, slam.
    public const float DipEnd = .08f, YankEnd = .2f, HangEnd = .28f, LandAt = .38f, LandedSettleEnd = .58f, LandedDuration = .95f;
    public const float DipDepth = 10, YankHeight = 46;
    // Escaped: wriggle, sink, stamp, then the payout tally.
    public const float WriggleEnd = .34f, SinkEnd = .66f, PayoutAt = 1.5f, SinkDepth = 96;
    public const float TallyBudget = 1.6f, MinStep = .03f, MaxStep = .22f, PunchSeconds = .26f, CardFadeSeconds = .3f, EndHold = .2f;
    /// <summary>Upper bound for ordinary haul sizes; very long hauls add at most MinStep per extra creature.</summary>
    public const float TypicalMaximum = PayoutAt + TallyBudget + PunchSeconds + CardFadeSeconds + EndHold;
    public const string Caption = "SPACE TO SKIP";

    public readonly LandingOutcomeKind Kind;
    /// <summary>Creatures cashed out in the payout tally (Escaped only).</summary>
    public readonly int Cards;

    private LandingOutcomeTiming(LandingOutcomeKind kind, int cards) { Kind = kind; Cards = Mathf.Max(0, cards); }
    public static LandingOutcomeTiming For(LandingOutcomeKind kind, int cards = 0) => new LandingOutcomeTiming(kind, kind == LandingOutcomeKind.Escaped ? cards : 0);

    /// <summary>Seconds between consecutive cash-outs: generous for small hauls, compressed so long hauls stay brisk.</summary>
    public float Step => Cards == 0 ? 0 : Mathf.Clamp(TallyBudget / Cards, MinStep, MaxStep);
    public float CardAt(int index) => PayoutAt + index * Step;
    public float PayoutEnd => PayoutAt + Cards * Step;
    /// <summary>When the cashed-out cards leave the rig column.</summary>
    public float ClearAt => PayoutEnd + PunchSeconds;
    public float Duration => Kind == LandingOutcomeKind.Landed ? LandedDuration : ClearAt + CardFadeSeconds + EndHold;
    /// <summary>When the outcome stamp lands: on the slam (Landed) or once the creature has sunk out of sight (Escaped).</summary>
    public float StampAt => Kind == LandingOutcomeKind.Landed ? LandAt : SinkEnd;
    /// <summary>The ESCAPED stamp folds away just before the payout takes the empty frame; LANDED stays until the shutter covers it.</summary>
    public float StampHold => Kind == LandingOutcomeKind.Landed ? float.PositiveInfinity : PayoutAt - LandingRarityBanner.FoldSeconds - SinkEnd;
    /// <summary>The header names the escaped creature, then states that the expedition is over once the payout begins.</summary>
    public bool ShowsExpeditionOver(float age) => Kind == LandingOutcomeKind.Escaped && age >= PayoutAt - .1f;

    /// <summary>Vertical card offset in reference pixels (up is positive).</summary>
    public float Lift(float age)
    {
        if (Kind == LandingOutcomeKind.Escaped)
        {
            if (age < WriggleEnd) return 0;
            float u = Mathf.Clamp01((age - WriggleEnd) / (SinkEnd - WriggleEnd));
            return -SinkDepth * u * u;
        }
        if (age <= 0 || age >= LandedSettleEnd) return 0;
        if (age < DipEnd) return -DipDepth * Mathf.Sin(age / DipEnd * Mathf.PI * .5f);
        if (age < YankEnd) { float u = (age - DipEnd) / (YankEnd - DipEnd); return Mathf.Lerp(-DipDepth, YankHeight, 1 - (1 - u) * (1 - u) * (1 - u)); }
        if (age < HangEnd) return YankHeight + 4 * Mathf.Sin((age - YankEnd) / (HangEnd - YankEnd) * Mathf.PI * .5f);
        if (age < LandAt) { float u = (age - HangEnd) / (LandAt - HangEnd); return Mathf.Lerp(YankHeight + 4, 0, u * u); }
        float b = (age - LandAt) / (LandedSettleEnd - LandAt);
        return 7 * Mathf.Sin(b * Mathf.PI) * (1 - b);
    }

    /// <summary>Horizontal card offset: the escaping creature thrashes side to side before it sinks.</summary>
    public float Sway(float age)
    {
        if (Kind != LandingOutcomeKind.Escaped || age <= 0 || age >= SinkEnd) return 0;
        float envelope = age < WriggleEnd ? Mathf.Sqrt(age / WriggleEnd) : 1 - (age - WriggleEnd) / (SinkEnd - WriggleEnd);
        return 6 * envelope * Mathf.Sin(age * Mathf.PI * 2 * 7.5f + .6f);
    }

    /// <summary>Card rotation in degrees: thrash while wriggling, then a slow roll as it sinks.</summary>
    public float Rotation(float age)
    {
        if (Kind != LandingOutcomeKind.Escaped || age <= 0) return 0;
        if (age < WriggleEnd) return 7 * Mathf.Sqrt(age / WriggleEnd) * Mathf.Sin(age * Mathf.PI * 2 * 7.5f);
        return -8 * Mathf.Clamp01((age - WriggleEnd) / (SinkEnd - WriggleEnd));
    }

    /// <summary>Card scale multipliers (x, y): squash and stretch through the yank and slam, shrinking as an escaping creature sinks.</summary>
    public Vector2 Scale(float age)
    {
        if (Kind == LandingOutcomeKind.Escaped)
        {
            float u = Mathf.Clamp01((age - WriggleEnd) / (SinkEnd - WriggleEnd));
            return Vector2.one * (1 - .16f * u);
        }
        if (age <= 0 || age >= LandedSettleEnd) return Vector2.one;
        if (age < DipEnd) { float s = Mathf.Sin(age / DipEnd * Mathf.PI * .5f); return new Vector2(1 + .03f * s, 1 - .05f * s); }
        if (age < YankEnd) { float s = Mathf.Sin((age - DipEnd) / (YankEnd - DipEnd) * Mathf.PI); return new Vector2(1 - .05f * s, 1 + .09f * s); }
        if (age < HangEnd) return Vector2.one;
        if (age < LandAt) { float u = (age - HangEnd) / (LandAt - HangEnd); return new Vector2(1 - .03f * u, 1 + .05f * u); }
        float a = age - LandAt, k = Mathf.Exp(-a * 16) * Mathf.Cos(a * 28);
        return new Vector2(1 + .06f * k, 1 - .1f * k);
    }

    /// <summary>Opacity of the creature card: Landed stays visible; Escaped fades as it sinks (a stationary fade with reduced motion).</summary>
    public float FaceOpacity(float age, bool reduced)
    {
        if (Kind == LandingOutcomeKind.Landed) return 1;
        if (reduced) return 1 - Mathf.Clamp01((age - .1f) / .3f);
        return 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(((age - WriggleEnd) / (SinkEnd - WriggleEnd) - .2f) / .8f));
    }

    /// <summary>White contact flash as the landed card slams home.</summary>
    public float Flash(float age)
    {
        if (Kind != LandingOutcomeKind.Landed) return 0;
        float a = age - LandAt;
        if (a < 0 || a > .2f) return 0;
        return a < .012f ? a / .012f * .7f : .7f * Mathf.Exp(-(a - .012f) * 26);
    }

    /// <summary>Downward stage impulse from the slam, decaying within a quarter second.</summary>
    public Vector2 StageOffset(float age)
    {
        if (Kind != LandingOutcomeKind.Landed) return Vector2.zero;
        float a = age - LandAt;
        if (a < 0 || a > .25f) return Vector2.zero;
        return new Vector2(0, -6 * Mathf.Exp(-a * 22) * Mathf.Cos(a * 40));
    }

    /// <summary>Running payout shown while cards cash out: each card's value rolls in during its step; exactly the total at the end.</summary>
    public int Payout(float age, int[] values, int total)
    {
        if (Kind != LandingOutcomeKind.Escaped || age < PayoutAt || Cards == 0) return 0;
        if (age >= PayoutEnd || values == null) return total;
        long running = 0;
        for (int i = 0; i < Cards && i < values.Length; i++)
        {
            float u = Mathf.Clamp01((age - CardAt(i)) / Step);
            if (u <= 0) break;
            running += u >= 1 ? values[i] : Mathf.FloorToInt(values[i] * (1 - (1 - u) * (1 - u)));
        }
        return (int)Mathf.Clamp(running, 0, total);
    }

    /// <summary>Index of the card currently cashing out, or -1 outside the tally.</summary>
    public int ActiveCard(float age) => Kind != LandingOutcomeKind.Escaped || Cards == 0 || age < PayoutAt || age >= PayoutEnd ? -1 : Mathf.Min(Cards - 1, Mathf.FloorToInt((age - PayoutAt) / Step));

    /// <summary>Card pop as it is cashed out: a quick swell that settles on exactly 1.</summary>
    public float CardPop(int index, float age)
    {
        float a = age - CardAt(index);
        if (a < 0 || a > .3f) return 1;
        return 1 + .08f * Mathf.Exp(-a * 14) * Mathf.Sin(Mathf.Min(a * 22, Mathf.PI));
    }

    /// <summary>Opacity of a card in the rig column: full until cashed out, dimmed after, gone once the rig clears.</summary>
    public float CardOpacity(int index, float age)
    {
        if (Kind != LandingOutcomeKind.Escaped) return 1;
        float clear = 1 - Mathf.Clamp01((age - ClearAt) / CardFadeSeconds);
        return (age >= CardAt(index) + Step * .6f ? .4f : 1) * clear;
    }

    /// <summary>Scale punch of the payout number when the tally completes.</summary>
    public float PayoutPunch(float age)
    {
        float a = age - PayoutEnd;
        if (Kind != LandingOutcomeKind.Escaped || a < 0 || a > PunchSeconds) return 1;
        return 1 + .22f * Mathf.Exp(-a * 12) * Mathf.Cos(a * 18) * (1 - a / PunchSeconds);
    }

    /// <summary>Opacity of the payout readout that replaces the escaped creature in its frame.</summary>
    public float PayoutOpacity(float age) => Kind != LandingOutcomeKind.Escaped ? 0 : Mathf.Clamp01((age - (PayoutAt - .08f)) / .12f);
}

/// <summary>Shared count-up curve for haul readouts: an ease-out roll that lands exactly on the target.</summary>
public static class TackleHaulCount
{
    /// <summary>Value between from and to at normalized progress u (0..1).</summary>
    public static int Value(int from, int to, float u)
    {
        if (u <= 0) return from;
        if (u >= 1) return to;
        float eased = 1 - (1 - u) * (1 - u) * (1 - u);
        return from + Mathf.FloorToInt((to - from) * eased);
    }

    /// <summary>Label scale punch after a count lands: swells and settles on exactly 1 within the given seconds.</summary>
    public static float Punch(float age, float seconds = .24f, float strength = .16f)
    {
        if (age < 0 || age > seconds) return 1;
        return 1 + strength * Mathf.Exp(-age * 12) * Mathf.Cos(age * 18) * (1 - age / seconds);
    }
}
