using System;

/// <summary>Registers approved coastal event mechanics without enabling legacy effect choices implicitly.</summary>
public static class CoastalEventHandlers
{
    /// <summary>Rebinds production handlers while preserving explicit existing registrations.</summary>
    public static void Register(EventEffectRuntime runtime)
    {
        foreach (CardEffectType type in new[] { CardEffectType.SnagHeaviestUnarmoredCreature,
            CardEffectType.QueueCreatureEncounterConcealment, CardEffectType.QueueBaitWeightReduction,
            CardEffectType.OptionalEventCreatureRelease, CardEffectType.ScheduleCreatureTrail })
            if (!runtime.HasHandler(type)) runtime.RegisterHandler(new Handler(type));
    }

    private sealed class Handler : IEventEffectHandler
    {
        public CardEffectType EffectType { get; }
        /// <summary>Binds one supported mechanic.</summary>
        public Handler(CardEffectType type) { EffectType = type; }
        /// <summary>Rejects unsupported timing, amount, targeting, and lifetime before consequences execute.</summary>
        public bool CanResolve(CardEffectDefinition effect, out string reason)
        {
            reason = "Invalid coastal event timing, target, amount, or duration";
            if (effect == null || effect.Trigger != CardEffectTrigger.OnEventResolution || effect.EffectType != EffectType) return false;
            bool valid;
            switch (EffectType)
            {
                case CardEffectType.SnagHeaviestUnarmoredCreature:
                    valid = effect.Amount == 1 && effect.Target == CardEffectTarget.SpecificCaughtCard
                        && effect.EffectTone == CardEffectTone.Negative && effect.EventLifetime?.BoundaryId == EventEffectLifetime.BiomeEnd
                        && effect.EventLifetime.BoundaryCount == 1; break;
                case CardEffectType.QueueCreatureEncounterConcealment:
                    valid = effect.Amount == 1 && effect.Target == CardEffectTarget.FutureEncounters
                        && effect.EffectTone == CardEffectTone.Negative && effect.EventLifetime == null; break;
                case CardEffectType.QueueBaitWeightReduction:
                    valid = effect.Amount == 1 && effect.Target == CardEffectTarget.CatchChain
                        && effect.EventLifetime != null && !effect.EventLifetime.HasBoundary; break;
                case CardEffectType.OptionalEventCreatureRelease:
                    valid = effect.Amount == 1 && effect.Target == CardEffectTarget.CatchChain && effect.EventLifetime == null; break;
                case CardEffectType.ScheduleCreatureTrail:
                    valid = effect.Amount == 3 && effect.Target == CardEffectTarget.FutureEncounters
                        && effect.ReplacementCard?.CardType == CardType.Creature && effect.EventLifetime == null; break;
                default: valid = false; break;
            }
            if (valid) reason = string.Empty;
            return valid;
        }
        /// <summary>Delegates mutations and choices to runtime owners and the shared reaction queue.</summary>
        public EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect)
        {
            switch (EffectType)
            {
                case CardEffectType.SnagHeaviestUnarmoredCreature:
                    if (!Array.Exists(context.Chain.Catches, c => c?.Definition?.CardType == CardType.Creature
                        && !c.HasTag(CreatureTagVocabulary.Armored))) return EventEffectOutcome.NoEligibleTarget;
                    context.Creatures.QueueEventSnag(context.OccurrenceId, context.Source, effect);
                    return EventEffectOutcome.Applied;
                case CardEffectType.QueueCreatureEncounterConcealment:
                    context.Encounters.GrantCreatureConcealment(effect, context.Source); return EventEffectOutcome.Applied;
                case CardEffectType.QueueBaitWeightReduction:
                    context.Persistence.QueuePending(context.OccurrenceId, context.Source, effect, effect.EventLifetime);
                    return EventEffectOutcome.Applied;
                case CardEffectType.OptionalEventCreatureRelease:
                    if (!Array.Exists(context.Chain.Catches, c => c?.Definition?.CardType == CardType.Creature
                        && !c.PreventsReleaseWhileAttached)) return EventEffectOutcome.NoEligibleTarget;
                    context.Creatures.QueueEventRelease(context.Source.DisplayName); return EventEffectOutcome.Applied;
                case CardEffectType.ScheduleCreatureTrail:
                    return context.Encounters.ActiveEncounterChain == null && context.Encounters.Trail.TrySchedule(
                        context.Biome, context.Depth, effect.ReplacementCard, effect.Amount)
                        ? EventEffectOutcome.Applied : EventEffectOutcome.NoEligibleTarget;
                default: throw new InvalidOperationException("Unsupported coastal event handler");
            }
        }
    }
}
