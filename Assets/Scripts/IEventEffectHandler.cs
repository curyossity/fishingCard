using System;

/// <summary>Reports whether one synchronous event consequence changed runtime state or had no eligible target.</summary>
public enum EventEffectOutcome
{
    Applied,
    NoEligibleTarget
}

/// <summary>Reports the event dispatch boundary without owning the confirmed fishing action.</summary>
public enum EventResolutionProgress
{
    Inactive,
    Ready,
    WaitingForCreatureReactions,
    Completed,
    Failed
}

/// <summary>Provides runtime owners to event handlers without allowing mutations of authored definitions.</summary>
public sealed class EventResolutionContext
{
    public EncounterRuntime Encounters { get; }
    public CatchChainRuntime Chain { get; }
    public CreatureEffectRuntime Creatures { get; }
    public TechniqueDeckRuntime Deck { get; }
    public EffectResolver Resolver { get; }
    public EventPersistenceRuntime Persistence { get; }
    public System.Random Random { get; }
    public long OccurrenceId { get; }
    public CardDefinition Source { get; }
    public BiomeDefinition Biome { get; }
    public int Depth { get; }

    /// <summary>Binds a context to the current event identity and the run's mutation owners.</summary>
    public EventResolutionContext(EncounterRuntime encounters, CatchChainRuntime chain, CreatureEffectRuntime creatures,
        TechniqueDeckRuntime deck, EffectResolver resolver, System.Random random, EventPersistenceRuntime persistence,
        BiomeDefinition biome = null, int depth = 0)
    {
        Encounters = encounters ?? throw new ArgumentNullException(nameof(encounters));
        Chain = chain ?? throw new ArgumentNullException(nameof(chain));
        Creatures = creatures ?? throw new ArgumentNullException(nameof(creatures));
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        Random = random ?? throw new ArgumentNullException(nameof(random));
        Persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        Persistence.Bind(Chain, Resolver);
        Creatures.EventPersistence = Persistence;
        Biome = biome; Depth = depth;
        OccurrenceId = encounters.CurrentEventOccurrence?.Id ?? 0;
        Source = encounters.CurrentEventOccurrence?.Definition;
    }
}

/// <summary>Defines a registered, approved event effect; choices/reactions belong to existing runtime owners.</summary>
public interface IEventEffectHandler
{
    CardEffectType EffectType { get; }

    /// <summary>Validates supported targeting/timing/duration data before any consequence is claimed or executed.</summary>
    bool CanResolve(CardEffectDefinition effect, out string reason);

    /// <summary>Applies one consequence once; any resulting creature choices are queued for completion before further dispatch.</summary>
    EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect);
}
