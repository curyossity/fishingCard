using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Checks reveal ordering, exact deferred completion, cancellation rewards, and guarded event replacement.</summary>
public static class EventRevealChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
#if UNITY_EDITOR
    /// <summary>Runs reveal/cancellation/replacement integration checks in Unity.</summary>
    [MenuItem("Fishing Cards/Validation/Event Reveal Cancellation and Replacement")]
    public static void Run()
    {
        Debug.Log("Event reveal rules passed: " + ValidateRules() + " assertions.");
    }
#endif
    /// <summary>Runs actual runtime paths and releases fixture objects afterward.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try { ValidateReactions(); ValidateCancellation(); ValidateReplacement(); ValidateChainsAndTechnique(); return assertions; }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }

    /// <summary>Checks multiple deferred reactions, exact tokens, nested cancellation, and stale completions.</summary>
    private static void ValidateReactions()
    {
        EncounterRuntime runtime = new EncounterRuntime(); CardDefinition environment = Card(CardType.Environment);
        long eventId = 0, first = 0, second = 0;
        runtime.EncounterRevealEffects += card =>
        {
            eventId = runtime.CurrentEventOccurrence.Id;
            Require(!runtime.HasEventReactionWindow && runtime.TryDeferEventRevealReaction(eventId, out first)
                && runtime.TryDeferEventRevealReaction(eventId, out second), "Reveal did not accept deferred reactions behind a closed window.");
            Require(first != second && !runtime.TryBeginEventResolution(eventId, out _), "Reveal reactions reused identity or allowed consequences early.");
        };
        runtime.SetCurrentEncounter(environment);
        Require(runtime.CurrentEventOccurrence.PendingRevealReactionCount == 2 && !runtime.HasEventReactionWindow,
            "Returning from callbacks opened input before deferred reactions completed.");
        Require(!runtime.TryCompleteEventRevealReaction(eventId + 1, first) && !runtime.TryCompleteEventRevealReaction(eventId, first + second),
            "Wrong event or reaction token released a hold.");
        Require(runtime.TryCompleteEventRevealReaction(eventId, second) && !runtime.HasEventReactionWindow
            && runtime.CurrentEventOccurrence.PendingRevealReactionCount == 1, "Completing one reaction released both holds.");
        Require(!runtime.TryCompleteEventRevealReaction(eventId, second), "Duplicate completion released another reaction.");
        Require(runtime.TryCompleteEventRevealReaction(eventId, first) && runtime.HasEventReactionWindow
            && !runtime.TryDeferEventRevealReaction(eventId, out _), "Final completion failed to open window or accepted a late hold.");
        long oldEvent = eventId, oldToken = first;
        runtime.SetCurrentEncounter(environment);
        Require(!runtime.TryCompleteEventRevealReaction(oldEvent, oldToken) && !runtime.TryCompleteEventRevealReaction(eventId, oldToken),
            "Old token completed a new occurrence.");
        runtime.Reset(); Require(!runtime.TryCompleteEventRevealReaction(eventId, first), "Reset allowed stale reveal completion.");

        runtime = new EncounterRuntime(); CardDefinition hazard = Card(CardType.Hazard); long deferred = 0; int cancellationCount = 0;
        runtime.HazardCancelled += card =>
        {
            cancellationCount++;
            Require(!runtime.HasEventReactionWindow && runtime.CurrentEncounter == null
                && runtime.TryDeferEventRevealReaction(runtime.CurrentEventOccurrence.Id, out deferred),
                "Cancellation rewards ran after window opening or before discard.");
        };
        int listeners = 0;
        runtime.EncounterRevealEffects += card =>
        {
            listeners++;
            Require(runtime.TryCancelRevealedHazard(runtime.CurrentEventOccurrence.Id) && !runtime.HasEventReactionWindow,
                "Nested reveal cancellation opened input while its outer callback was active.");
        };
        runtime.EncounterRevealEffects += card =>
        {
            listeners++; Require(!runtime.HasEventReactionWindow && !runtime.CurrentEventOccurrence.RevealReactionsComplete,
                "A later reveal observer saw cancellation as fully complete.");
        };
        runtime.SetCurrentEncounter(hazard);
        Require(listeners == 2 && cancellationCount == 1 && runtime.CurrentEventOccurrence.State == EventOccurrenceState.Cancelled
            && !runtime.HasEventReactionWindow, "Nested cancellation lost or repeated its outcome.");
        Require(runtime.TryCompleteEventRevealReaction(runtime.CurrentEventOccurrence.Id, deferred) && runtime.HasEventReactionWindow,
            "Deferred cancellation completion did not open the cancelled event window.");
        Require(!runtime.TryCancelRevealedHazard(runtime.CurrentEventOccurrence.Id), "Cancelled Hazard rewarded twice.");
    }

    /// <summary>Checks automatic and manual Hazard cancellation, rewards, no-target resolution, and category/state exclusions.</summary>
    private static void ValidateCancellation()
    {
        EffectResolver resolver = new EffectResolver(); CatchChainRuntime chain = new CatchChainRuntime();
        CardDefinition lobster = Card(CardType.Creature);
        Set(lobster, "value", 24); Set(lobster, "effects", new[] { Effect(CardEffectType.GainValueOnHazardCancelled,
            CardEffectTrigger.AfterHazardCancelled, CardEffectTarget.Self, 4) });
        CardInstance one = chain.Add(lobster, resolver); CardInstance two = chain.Add(lobster, resolver);
        CardInstance bait = chain.Add(lobster, resolver, CatchAttachmentRole.Bait);
        CardDefinition mussel = Card(CardType.Creature);
        Set(mussel, "baitEffects", new[] { Effect(CardEffectType.CancelNextHazard, CardEffectTrigger.WhenCaught, CardEffectTarget.Self, 1) });
        chain.Add(mussel, resolver, CatchAttachmentRole.Bait);
        EncounterRuntime runtime = new EncounterRuntime(); runtime.HazardCancellation = chain.TryCancelHazard;
        int cancellations = 0, revealEffects = 0;
        runtime.HazardCancelled += card => { cancellations++; chain.RecordHazardCancelled(card, resolver); };
        runtime.EncounterRevealEffects += card => revealEffects++;
        CardDefinition hazard = Card(CardType.Hazard);
        Require(runtime.Reveal(new[] { hazard }, "Coastal", 0, new System.Random(1), chain.ActiveEffectRecords, resolver)
            && runtime.CurrentEncounter == null && runtime.HasEventReactionWindow && revealEffects == 0,
            "Automatic cancellation activated Hazard reveal effects or lost the post-reaction window.");
        Require(cancellations == 1 && one.CurrentValue == 28 && two.CurrentValue == 28 && bait.CurrentValue == 24,
            "Cancellation failed per-Lobster Catch rewards or rewarded Bait.");
        Require(runtime.RecentEncounterHistory.Length == 1 && runtime.RecentEncounterHistory[0] == hazard,
            "Cancelled draw vanished from encounter history.");
        runtime.SetCurrentEncounter(hazard);
        Require(revealEffects == 1 && runtime.CurrentEventOccurrence.State == EventOccurrenceState.Unresolved
            && cancellations == 1, "Used Mussel cancelled another Hazard or plain reveal awarded rewards.");
        long id = runtime.CurrentEventOccurrence.Id;
        Require(!runtime.TryCancelRevealedHazard(id + 1) && runtime.TryBeginEventResolution(id, out _)
            && !runtime.TryCancelRevealedHazard(id) && runtime.TryCompleteEventResolution(id),
            "Stale ID/active consequences were cancelled or no-target completion failed.");
        Require(cancellations == 1 && one.CurrentValue == 28, "Resolution without targets was mistaken for cancellation.");
        runtime.SetCurrentEncounter(Card(CardType.Environment));
        Require(!runtime.TryCancelRevealedHazard(runtime.CurrentEventOccurrence.Id), "Non-Hazard emitted cancellation.");
        runtime.SetCurrentEncounter(hazard);
        Require(runtime.TryCancelRevealedHazard(runtime.CurrentEventOccurrence.Id) && runtime.LastCancelledHazard == hazard
            && runtime.HasEventReactionWindow && cancellations == 2 && one.CurrentValue == 32,
            "Explicit cancellation failed discard/rewards/window.");
        runtime.SetCurrentEncounter(Card(CardType.ApexEncounter));
        Require(!runtime.TryCancelRevealedHazard(id), "Apex was cancelled through a stale Hazard occurrence.");
    }

    /// <summary>Checks failed replacement atomicity, stale revisions, old consequence invalidation, and independent replacement cancellation.</summary>
    private static void ValidateReplacement()
    {
        EncounterRuntime runtime = new EncounterRuntime(); EffectResolver resolver = new EffectResolver();
        CardDefinition original = Card(CardType.Hazard), replacement = Card(CardType.Environment);
        EncounterChainDefinition chain = Chain(original, Card(CardType.Opportunity));
        runtime.Reveal(new[] { original }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver,
            null, null, new[] { chain });
        EventEncounterOccurrence old = runtime.CurrentEventOccurrence; int revision = runtime.EncounterRevision;
        int notifications = 0; runtime.HazardCancelled += card => notifications++;
        Require(!runtime.TryReplaceEncounter(revision, original, new[] { original }, "Coastal", 0, new System.Random(1),
            Array.Empty<ActiveCatchEffectRecord>(), resolver) && runtime.CurrentEventOccurrence == old
            && old.State == EventOccurrenceState.Unresolved && runtime.ActiveEncounterChain == chain
            && runtime.EncounterRevision == revision, "Failed replacement mutated occurrence or chain.");
        Require(!runtime.TryReplaceEncounter(revision - 1, original, new[] { replacement }, "Coastal", 0, new System.Random(1),
            Array.Empty<ActiveCatchEffectRecord>(), resolver), "Stale revision replaced a fresh encounter.");
        Require(runtime.TryReplaceEncounter(revision, original, new[] { replacement }, "Coastal", 0, new System.Random(1),
            Array.Empty<ActiveCatchEffectRecord>(), resolver) && old.State == EventOccurrenceState.Replaced
            && runtime.CurrentEncounter == replacement && notifications == 0 && runtime.ActiveEncounterChain == null,
            "Replacing Hazard cancelled it or retained its authored trail.");
        Require(!runtime.TryBeginEventResolution(old.Id, out _), "Replaced event retained executable consequences.");
        old = runtime.CurrentEventOccurrence;
        CardDefinition nextHazard = Card(CardType.Hazard); runtime.HazardCancellation = card => true;
        Require(runtime.TryReplaceEncounter(runtime.EncounterRevision, replacement, new[] { nextHazard }, "Coastal", 0,
            new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver) && old.State == EventOccurrenceState.Replaced
            && runtime.CurrentEventOccurrence.State == EventOccurrenceState.Cancelled && notifications == 1
            && runtime.CurrentEncounter == null && runtime.HasEventReactionWindow,
            "Replacement Hazard did not receive its own independent cancellation.");
        Require(!runtime.TryReplaceEncounter(runtime.EncounterRevision, nextHazard, new[] { replacement }, "Coastal", 0,
            new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver), "Discarded cancelled card remained replaceable.");
        runtime.SetCurrentEncounter(replacement); runtime.TryBeginEventResolution(runtime.CurrentEventOccurrence.Id, out _);
        CardDefinition[] history = runtime.RecentEncounterHistory; revision = runtime.EncounterRevision;
        bool rejected = false;
        try { runtime.Reveal(new[] { original }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver); }
        catch (InvalidOperationException) { rejected = true; }
        Require(rejected && runtime.EncounterRevision == revision && runtime.RecentEncounterHistory.Length == history.Length,
            "Blocked reveal changed selection history during resolution.");
    }

    /// <summary>Checks chain follow-ups, nested reveal bookkeeping, and the actual Technique replacement backend.</summary>
    private static void ValidateChainsAndTechnique()
    {
        EncounterRuntime runtime = new EncounterRuntime(); EffectResolver resolver = new EffectResolver();
        CardDefinition first = Card(CardType.Environment), hazard = Card(CardType.Hazard), finish = Card(CardType.Opportunity);
        EncounterChainDefinition chain = Chain(first, hazard, finish); int notifications = 0;
        runtime.HazardCancellation = card => true; runtime.HazardCancelled += card => notifications++;
        runtime.Reveal(new[] { first }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver,
            null, null, new[] { chain });
        Require(runtime.ActiveEncounterChain == chain && runtime.NextEncounterChainIndex == 1, "Chain starter was not queued.");
        Require(runtime.Reveal(new[] { first }, "Coastal", 1, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver)
            && runtime.CurrentEventOccurrence.State == EventOccurrenceState.Cancelled && runtime.HasEventReactionWindow
            && notifications == 1 && runtime.NextEncounterChainIndex == 2, "Chain Hazard did not use normal cancellation rules.");
        Require(runtime.RecentEncounterHistory.Length == 2 && runtime.RecentEncounterHistory[1] == hazard,
            "Chain cancellation lost selected history.");
        Require(runtime.Reveal(new[] { first }, "Coastal", 2, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver)
            && runtime.CurrentEncounter == finish && runtime.ActiveEncounterChain == null, "Cancelled chain Hazard caused an extra draw or repeated follow-up.");

        runtime = new EncounterRuntime(); CardDefinition replacement = Card(CardType.Opportunity);
        runtime.EncounterRevealEffects += revealed =>
        {
            if (revealed == first) { runtime.CancelActiveEncounterChain(); runtime.SetCurrentEncounter(replacement); }
        };
        runtime.Reveal(new[] { first }, "Coastal", 0, new System.Random(1), Array.Empty<ActiveCatchEffectRecord>(), resolver,
            null, null, new[] { chain });
        Require(runtime.CurrentEncounter == replacement && runtime.ActiveEncounterChain == null,
            "Returning from reveal restored the replaced starter's old chain.");
        runtime = new EncounterRuntime(); runtime.SetCurrentEncounter(first);
        EventEncounterOccurrence old = runtime.CurrentEventOccurrence; TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        CardDefinition technique = Card(CardType.Technique); Set(technique, "effects", new[] {
            Effect(CardEffectType.ReplaceEncounter, CardEffectTrigger.Manual, CardEffectTarget.CurrentEncounter, 1) });
        Require(techniques.CanUseCard(technique, runtime, new CatchChainRuntime(), false, new[] { replacement }, "Coastal", 0, out _),
            "Technique backend rejected a legal unresolved event replacement.");
        Require(techniques.ApplyCard(technique, runtime, new CatchChainRuntime(), resolver, new[] { replacement }, null, "Coastal", 0, new System.Random(1), false, out _, TechniqueRuleChecks.FirstTargets(technique, new CatchChainRuntime(), runtime)) && runtime.CurrentEncounter == replacement && old.State == EventOccurrenceState.Replaced,
            "Technique backend bypassed replacement invalidation.");
        runtime = new EncounterRuntime(); runtime.SetCurrentEncounter(Card(CardType.Creature));
        Require(techniques.ApplyCard(technique, runtime, new CatchChainRuntime(), resolver, new[] { replacement }, null, "Coastal", 0, new System.Random(1), false, out _, TechniqueRuleChecks.FirstTargets(technique, new CatchChainRuntime(), runtime)) && runtime.CurrentEventOccurrence.Definition == replacement,
            "Creature-to-event Technique replacement lost event lifecycle setup.");
    }

    /// <summary>Creates a fixture definition without changing authored card assets.</summary>
    private static CardDefinition Card(CardType type)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "cardType", type); Set(card, "displayName", type.ToString()); Set(card, "maximumDepth", 20);
        Set(card, "hasExplicitBaitData", true); return card;
    }

    /// <summary>Creates an authored chain fixture.</summary>
    private static EncounterChainDefinition Chain(params CardDefinition[] sequence)
    {
        EncounterChainDefinition chain = ScriptableObject.CreateInstance<EncounterChainDefinition>(); created.Add(chain);
        Set(chain, "sequence", sequence); return chain;
    }

    /// <summary>Creates existing effect data used by cancellation and replacement integration.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target, int amount)
    {
        CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", type); Set(effect, "trigger", trigger);
        Set(effect, "target", target); Set(effect, "amount", amount); return effect;
    }

    /// <summary>Configures fixtures through existing serialized fields.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Counts invariant assertions and reports the first integration failure.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++; if (!condition) throw new InvalidOperationException(message);
    }
}
