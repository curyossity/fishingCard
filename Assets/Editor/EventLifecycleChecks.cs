using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks event occurrence identity, guarded transitions, and independence from visible/catchable encounters.</summary>
public static class EventLifecycleChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs the lifecycle checks against Unity runtime sources.</summary>
    [MenuItem("Fishing Cards/Validation/Event Lifecycle")]
    public static void Run()
    {
        Debug.Log("Event lifecycle passed: " + ValidateRules() + " assertions.");
    }
#endif
    /// <summary>Executes transition and reveal integration checks without duplicating gameplay implementations.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateIdentity(); ValidateCancellation(); ValidateTransitions(); ValidateRevealCallbacks(); ValidateCatchableEncounters(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks event eligibility, repeated definitions, reset identities, and independent runtime copies.</summary>
    private static void ValidateIdentity()
    {
        EncounterRuntime runtime = new EncounterRuntime();
        foreach (CardType type in (CardType[])Enum.GetValues(typeof(CardType)))
        {
            CardDefinition card = Card(type); runtime.SetCurrentEncounter(card);
            bool isEvent = type == CardType.Hazard || type == CardType.Environment || type == CardType.Opportunity || type == CardType.Encounter;
            Require(EncounterRuntime.IsEventCard(card) == isEvent && (runtime.CurrentEventOccurrence != null) == isEvent,
                "Event eligibility included catchable, player-deck, or Location cards.");
            if (isEvent) Require(runtime.HookedEncounter == null && runtime.TakeHookedEncounter() == null
                && runtime.CurrentState == EncounterState.Encountered && runtime.HasEventReactionWindow,
                "Event entered Hooked commitment or failed its independent window.");
        }
        CardDefinition shared = Card(CardType.Environment); runtime.SetCurrentEncounter(shared);
        EventEncounterOccurrence old = runtime.CurrentEventOccurrence;
        runtime.SetCurrentEncounter(shared); EventEncounterOccurrence next = runtime.CurrentEventOccurrence;
        Require(next.Id > old.Id && next != old && old.State == EventOccurrenceState.Replaced && !old.IsReactionWindowOpen,
            "Same definition reveal reused identity or left old consequences active.");
        Require(!runtime.TryBeginEventResolution(old.Id, out _) && !runtime.TryCompleteEventResolution(old.Id), "Stale occurrence executed/completed a newer reveal.");
        EncounterRuntime independent = new EncounterRuntime(); independent.SetCurrentEncounter(shared);
        Require(independent.TryBeginEventResolution(independent.CurrentEventOccurrence.Id, out _)
            && next.State == EventOccurrenceState.Unresolved && shared.CardType == CardType.Environment,
            "A runtime occurrence mutated shared definitions or another runtime.");
        long beforeReset = next.Id; runtime.Reset();
        Require(runtime.CurrentEventOccurrence == null && !runtime.HasEventReactionWindow && runtime.CurrentEncounter == null,
            "Full reset retained an event window.");
        runtime.SetCurrentEncounter(shared);
        Require(runtime.CurrentEventOccurrence.Id > beforeReset && !runtime.TryBeginEventResolution(beforeReset, out _),
            "Full reset reused an ID accepted by a stale callback.");
        beforeReset = runtime.CurrentEventOccurrence.Id; runtime.Reset(true); runtime.SetCurrentEncounter(shared);
        Require(runtime.CurrentEventOccurrence.Id > beforeReset, "Biome reset reused an event ID.");
    }

    /// <summary>Checks cancelled-Hazard identity is established before rewards and remains actionable after discard.</summary>
    private static void ValidateCancellation()
    {
        EncounterRuntime runtime = new EncounterRuntime(); CardDefinition hazard = Card(CardType.Hazard);
        runtime.HazardCancellation = card => true; int notifications = 0;
        runtime.HazardCancelled += card =>
        {
            notifications++;
            Require(card == hazard && runtime.CurrentEventOccurrence.Definition == hazard
                && runtime.CurrentEventOccurrence.State == EventOccurrenceState.Cancelled && runtime.CurrentEncounter == null,
                "Cancellation observers missed the authoritative occurrence or discarded presentation.");
            Require(!runtime.HasEventReactionWindow && !runtime.TryBeginEventResolution(runtime.CurrentEventOccurrence.Id, out _),
                "Cancelled consequences or Technique window opened during reveal reactions.");
        };
        runtime.SetCurrentEncounter(hazard); EventEncounterOccurrence cancelled = runtime.CurrentEventOccurrence;
        Require(notifications == 1 && runtime.HasEventReactionWindow && runtime.LastCancelledHazard == hazard,
            "Cancellation failed to preserve the post-reaction window.");
        runtime.SetCurrentEncounter(null);
        Require(runtime.CurrentEventOccurrence == cancelled && cancelled.State == EventOccurrenceState.Cancelled
            && runtime.HasEventReactionWindow && notifications == 1, "Clearing presentation erased or repeated cancellation.");
        Require(!runtime.TryBeginEventResolution(cancelled.Id, out _) && !runtime.TryCompleteEventResolution(cancelled.Id),
            "Cancelled consequences were claimable.");
        Require(!runtime.TryCloseCancelledEventWindow(cancelled.Id + 1) && runtime.HasEventReactionWindow,
            "Wrong ID closed a cancelled window.");
        Require(runtime.TryCloseCancelledEventWindow(cancelled.Id) && !runtime.HasEventReactionWindow
            && cancelled.State == EventOccurrenceState.Cancelled, "Closing cancellation window lost its outcome.");
        Require(!runtime.TryCloseCancelledEventWindow(cancelled.Id), "Cancelled window closed twice.");
        runtime.SetCurrentEncounter(hazard);
        Require(notifications == 2 && runtime.CurrentEventOccurrence.Id > cancelled.Id, "Another Hazard reused cancellation identity.");
        runtime.SetCurrentEncounter(Card(CardType.Creature));
        Require(!runtime.HasEventReactionWindow && runtime.CurrentEventOccurrence == null, "Cancelled window leaked into creature encounter.");
    }

    /// <summary>Checks resolution ownership, repeated claims, hidden presentation, replacement invalidation, and no-target completion.</summary>
    private static void ValidateTransitions()
    {
        EncounterRuntime runtime = new EncounterRuntime(); CardDefinition environment = Card(CardType.Environment);
        runtime.SetCurrentEncounter(environment); EventEncounterOccurrence current = runtime.CurrentEventOccurrence;
        runtime.SetCurrentEncounter(null);
        Require(runtime.CurrentEventOccurrence == current && runtime.HasEventReactionWindow && current.Definition == environment,
            "Discarding visible event destroyed unresolved lifecycle state.");
        Require(!runtime.TryBeginEventResolution(current.Id + 1, out CardDefinition wrong) && wrong == null
            && !runtime.TryCompleteEventResolution(current.Id), "Invalid ID or unclaimed completion changed lifecycle.");
        Require(runtime.TryBeginEventResolution(current.Id, out CardDefinition claimed) && claimed == environment
            && current.State == EventOccurrenceState.Resolving && !runtime.HasEventReactionWindow,
            "Valid claim failed to return the event and close its window.");
        Require(!runtime.TryBeginEventResolution(current.Id, out _) && !runtime.TryCloseCancelledEventWindow(current.Id),
            "Resolving event was claimed twice or treated as cancellation.");
        bool rejected = false;
        try { runtime.SetCurrentEncounter(Card(CardType.Opportunity)); } catch (InvalidOperationException) { rejected = true; }
        Require(rejected && runtime.CurrentEventOccurrence == current && current.State == EventOccurrenceState.Resolving,
            "A new reveal interrupted active event consequences.");
        runtime.SetCurrentEncounter(null);
        Require(current.State == EventOccurrenceState.Resolving, "Clearing presentation ended resolution early.");
        Require(runtime.TryCompleteEventResolution(current.Id) && current.State == EventOccurrenceState.Resolved
            && !runtime.HasEventReactionWindow && runtime.CurrentEncounter == null, "Completed event retained its window or visibility.");
        Require(!runtime.TryCompleteEventResolution(current.Id) && !runtime.TryBeginEventResolution(current.Id, out _),
            "Resolved event executed a second time.");
        runtime.SetCurrentEncounter(environment); current = runtime.CurrentEventOccurrence;
        Require(runtime.TryBeginEventResolution(current.Id, out _) && runtime.TryCompleteEventResolution(current.Id)
            && runtime.CurrentEncounter == null && runtime.CurrentState == EncounterState.None,
            "Resolution without targets did not discard its card.");
        runtime.SetCurrentEncounter(environment); current = runtime.CurrentEventOccurrence;
        runtime.SetCurrentEncounter(Card(CardType.Hazard));
        Require(current.State == EventOccurrenceState.Replaced && !current.IsReactionWindowOpen
            && !runtime.TryBeginEventResolution(current.Id, out _), "Replacing unresolved event left old consequences active.");
        current = runtime.CurrentEventOccurrence;
        runtime.SetCurrentEncounter(Card(CardType.Treasure));
        Require(current.State == EventOccurrenceState.Replaced && runtime.CurrentEventOccurrence == null,
            "Replacement by catchable card retained event ownership.");
        runtime.SetCurrentEncounter(environment); current = runtime.CurrentEventOccurrence;
        runtime.TryBeginEventResolution(current.Id, out _); runtime.Reset();
        Require(!runtime.TryCompleteEventResolution(current.Id), "Run reset accepted completion of abandoned consequences.");
    }

    /// <summary>Checks normal weighted reveals, closed reveal-reaction windows, nested replacement, and removal during reveal.</summary>
    private static void ValidateRevealCallbacks()
    {
        EncounterRuntime runtime = new EncounterRuntime(); CardDefinition card = Card(CardType.Opportunity);
        int observations = 0;
        runtime.EncounterRevealEffects += revealed =>
        {
            observations++;
            Require(runtime.CurrentEventOccurrence.Definition == revealed && !runtime.HasEventReactionWindow
                && !runtime.TryBeginEventResolution(runtime.CurrentEventOccurrence.Id, out _),
                "Event was unavailable to reveal reactions or resolvable before those reactions completed.");
        };
        Require(runtime.Reveal(new[] { card }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), new EffectResolver())
            && observations == 1 && runtime.HasEventReactionWindow, "Weighted reveal failed event lifecycle integration.");
        long firstId = runtime.CurrentEventOccurrence.Id;
        Require(runtime.Reveal(new[] { card }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), new EffectResolver())
            && runtime.CurrentEventOccurrence.Id > firstId && observations == 2, "Repeated weighted draw reused event identity.");
        runtime = new EncounterRuntime(); CardDefinition replacement = Card(CardType.Environment);
        EventEncounterOccurrence replaced = null;
        runtime.EncounterRevealEffects += revealed =>
        {
            if (revealed == card) { replaced = runtime.CurrentEventOccurrence; runtime.SetCurrentEncounter(replacement); }
        };
        runtime.SetCurrentEncounter(card);
        Require(replaced.State == EventOccurrenceState.Replaced && !replaced.IsReactionWindowOpen
            && runtime.CurrentEventOccurrence.Definition == replacement && runtime.HasEventReactionWindow,
            "Returning from nested reveal revived the superseded event.");
        runtime = new EncounterRuntime();
        runtime.EncounterRevealEffects += revealed => runtime.SetCurrentEncounter(null);
        runtime.SetCurrentEncounter(card);
        Require(runtime.CurrentEncounter == null && runtime.CurrentEventOccurrence.Definition == card && runtime.HasEventReactionWindow,
            "Reveal-side presentation removal destroyed event ownership.");
    }

    /// <summary>Checks Creature/Treasure/Apex commitment and independent Squid concealment survive event lifecycles.</summary>
    private static void ValidateCatchableEncounters()
    {
        EncounterRuntime runtime = new EncounterRuntime();
        foreach (CardType type in new[] { CardType.Creature, CardType.Treasure, CardType.ApexEncounter })
        {
            CardDefinition card = Card(type); runtime.SetCurrentEncounter(card);
            Require(runtime.CurrentEventOccurrence == null && runtime.HookedEncounter == card && runtime.TakeHookedEncounter() == card,
                "Catchable encounter commitment changed.");
        }
        CardEffectDefinition ink = new CardEffectDefinition(); Set(ink, "amount", 2); Set(ink, "effectTone", CardEffectTone.Negative);
        CardDefinition squid = Card(CardType.Creature); runtime.GrantCreatureConcealment(ink, squid);
        runtime.SetCurrentEncounter(Card(CardType.Environment));
        Require(!runtime.IsCreatureConcealed && runtime.PendingConcealments[0].Remaining == 2, "Event consumed Squid concealment.");
        runtime.SetCurrentEncounter(squid); Require(runtime.IsCreatureConcealed && runtime.CurrentEventOccurrence == null,
            "Creature reveal lost concealment or retained event state.");
        Require(runtime.TakeHookedEncounter() == squid && !runtime.IsCreatureConcealed && runtime.PendingConcealments[0].Remaining == 1,
            "Creature commitment lost correct Ink charge consumption.");
    }

    /// <summary>Creates a serialized authored-card fixture without editing repository assets.</summary>
    private static CardDefinition Card(CardType type)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "cardType", type); Set(card, "displayName", type.ToString()); Set(card, "maximumDepth", 20); return card;
    }

    /// <summary>Sets fixture fields through the existing Unity serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Counts invariant checks and reports the first lifecycle failure.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++; if (!condition) throw new InvalidOperationException(message);
    }
}
