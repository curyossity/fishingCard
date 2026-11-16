using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>Describes the authored current-level rules without substituting printed Level 1 numbers.</summary>
public static class RigAbilityText
{
    /// <summary>Formats supported effect components at their resolved level; unassigned species gain no invented ability.</summary>
    public static string Describe(RigAbilitySpec ability, int level, bool richText = true)
    {
        if (ability == null || ability.Effects.Count == 0)
            return string.IsNullOrWhiteSpace(ability?.Description) ? "No active ability." : ability.Description;
        var lines = new List<string>();
        foreach (var effect in ability.Effects)
        {
            string number = Number(effect.Amount(level)), amount = Highlight("+" + number, richText);
            string count = Count(effect), sentence;
            string strikes = effect.Accuracy == LandingAccuracyFilter.Successful ? "Successful strikes" : effect.Accuracy + " strikes";
            switch (effect.Kind)
            {
                case RigAbilityKind.LandingMultiplier:
                    sentence = strikes + " gain " + Highlight("+" + number + "x", richText) + count + "."; break;
                case RigAbilityKind.LandingBonus:
                    sentence = strikes + " gain " + amount + " Landing Points" + count + "."; break;
                case RigAbilityKind.ValueBonus:
                    sentence = Target(effect) + (effect.ValueTarget==RigValueTarget.Self ? " gains " : " gain ") + amount + " sale value" + count + "."; break;
                case RigAbilityKind.ValueMultiplier:
                    sentence = Target(effect) + (effect.ValueTarget==RigValueTarget.Self ? " gains " : " gain ") + Highlight("+"+number+"x",richText) + " sale multiplier" + count + "."; break;
                case RigAbilityKind.ExtraAttempts:
                    sentence = "Gain " + amount + " landing attempts" + count + "."; break;
                case RigAbilityKind.MissProtection:
                    sentence = "Protect " + Highlight(number, richText) + " Miss attempts each encounter" + count + "."; break;
                case RigAbilityKind.BlockInsertionBefore:
                    sentence = "Prevents insertion before this creature" + Condition(effect) + "."; break;
                case RigAbilityKind.BlockInsertionAfter:
                    sentence = "Prevents insertion after this creature" + Condition(effect) + "."; break;
                default: throw new ArgumentOutOfRangeException();
            }
            if (effect.Trigger != RigAbilityTrigger.Continuous) sentence = "On " + (effect.Trigger == RigAbilityTrigger.Acquired ? "acquisition: " : "upgrade: ") + sentence;
            lines.Add(sentence);
        }
        return string.Join("\n", lines);
    }

    /// <summary>Uses the same level precision as scoring rather than a fixed number of decimal places.</summary>
    private static string Number(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);
    /// <summary>Accents the live coefficient, leaving all conditions readable in the body font.</summary>
    private static string Highlight(string value, bool rich) => rich ? "<color=#E9BC63>" + value + "</color>" : value;
    /// <summary>Distinguishes a per-position amount from a binary positional activation condition.</summary>
    private static string Count(RigAbilityEffect effect)
    {
        if (effect.Count == RigPositionCount.Once) return "";
        if (!effect.PerPosition) return Condition(effect);
        string species = string.IsNullOrEmpty(effect.Tag) ? "creature" : effect.Tag + " creature";
        switch (effect.Count)
        {
            case RigPositionCount.Adjacent: return " per adjacent " + species;
            case RigPositionCount.ContiguousOther: return " per other contiguous " + species;
            case RigPositionCount.Before: return " per " + species + " before it";
            case RigPositionCount.After: return " per " + species + " after it";
            default: return " per other " + species;
        }
    }
    /// <summary>Describes the same counted condition used by binary assistance and restriction rules.</summary>
    private static string Condition(RigAbilityEffect effect)
    {
        if (effect.Count == RigPositionCount.Once) return "";
        string species = string.IsNullOrEmpty(effect.Tag) ? "creature" : effect.Tag + " creature";
        switch (effect.Count)
        {
            case RigPositionCount.Adjacent: return " while adjacent to another " + species;
            case RigPositionCount.ContiguousOther: return " while connected to another contiguous " + species;
            case RigPositionCount.Before: return " while a " + species + " is before it";
            case RigPositionCount.After: return " while a " + species + " is after it";
            default: return " while another " + species + " is attached";
        }
    }
    /// <summary>Preserves authored value recipients without implying that only the source gains value.</summary>
    private static string Target(RigAbilityEffect effect)
    {
        string tag = string.IsNullOrEmpty(effect.TargetTag) ? "creatures" : effect.TargetTag + " creatures";
        switch (effect.ValueTarget)
        {
            case RigValueTarget.Adjacent: return "Adjacent " + tag;
            case RigValueTarget.Before: return tag + " before it";
            case RigValueTarget.After: return tag + " after it";
            case RigValueTarget.All: return "All " + tag;
            default: return "This creature";
        }
    }
}
