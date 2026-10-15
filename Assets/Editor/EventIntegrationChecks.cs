using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>Runs event foundation suites together and checks full multi-Technique action sequences.</summary>
public static class EventIntegrationChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;
    private sealed class ValueHandler : IEventEffectHandler
    {
        public CardEffectType EffectType => CardEffectType.ModifyCatchValue;
        public int Calls;
        public int RecipientId;
        /// <summary>Accepts only the explicit immediate fixture consequence.</summary>
        public bool CanResolve(CardEffectDefinition effect, out string reason) { reason = string.Empty; return effect.Amount == 3; }
        /// <summary>Adds a tracked fixture modifier through the real persistence owner.</summary>
        public EventEffectOutcome Resolve(EventResolutionContext context, CardEffectDefinition effect)
        {
            Calls++;
            return context.Persistence.TryAddModifier(context.OccurrenceId, context.Source, effect, RecipientId,
                0, 3, EventEffectLifetime.UntilRunEnds(), out _) ? EventEffectOutcome.Applied : EventEffectOutcome.NoEligibleTarget;
        }
    }
#if UNITY_EDITOR
    /// <summary>Executes all shared event suites, coastal regressions, and real Editor presentation/authoring checks.</summary>
    [MenuItem("Fishing Cards/Validation/All Event Integration Checks")]
    public static void Run()
    {
        string output = "Docs/Validation/EventIntegration";
        Directory.CreateDirectory(output);
        try
        {
            int runtime = ValidateRules() + TechniqueRuleChecks.ValidateRules() + CoastalEventChecks.ValidateRules() + EventPresentationChecks.ValidateRules() + EventActionChecks.ValidateRules()
                + EventPersistenceChecks.ValidateRules() + EventResolutionChecks.ValidateRules() + EventTechniqueChecks.ValidateRules()
                + EventRevealChecks.ValidateRules() + EventLifecycleChecks.ValidateRules()
                + LobsterSquidEelChecks.ValidateRules() + BassUrchinOctopusChecks.ValidateRules()
                + ShellFeedingExchangeChecks.ValidateRules() + PredatorSchoolRayChecks.ValidateRules()
                + CoastalSupportChecks.ValidateRules() + CreatureReleaseChecks.ValidateRules();
            int editor = EventPresentationChecks.ValidateEditorContract() + CoastalEventChecks.ValidateAuthoredAssets() + TechniqueRuleChecks.ValidateEditorPresentation();
            int captures = EventPresentationChecks.CaptureReadability(output);
            File.WriteAllText(Path.Combine(output, "unity-result.json"),
                "{\"status\":\"passed\",\"unity\":\"" + Application.unityVersion
                + "\",\"runtimeAssertions\":" + runtime + ",\"editorAssertions\":" + editor + ",\"readabilityCaptures\":" + captures + "}");
            Debug.Log("Event integration passed: " + runtime + " runtime and " + editor + " Editor assertions. Run Cleaner Shrimp separately for its asset/modal checks.");
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(output, "unity-failure.txt"), error.ToString());
            throw;
        }
    }
#endif
    /// <summary>Runs end-to-end sequences using real controllers, effect owners, and authored encounter chains.</summary>
    public static int ValidateRules()
    {
        assertions = 0;
        try
        {
            foreach (CardType type in new[] { CardType.Hazard, CardType.Environment, CardType.Opportunity, CardType.Encounter })
            { ValidateSequence(type, false); ValidateSequence(type, true); }
            ValidateChain(); return assertions;
        }
        finally { foreach (UnityEngine.Object obj in created) UnityEngine.Object.DestroyImmediate(obj); created.Clear(); }
    }
    /// <summary>Checks two legal Techniques remain in one window, then consequences run before either core action.</summary>
    private static void ValidateSequence(CardType type, bool surface)
    {
        CardDefinition source = Card(type); Set(source, "effects", new[] { Effect(3) });
        FishingRunController controller = Controller(source);
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        CardInstance target = chain.Add(Card(CardType.Creature), resolver);
        ValueHandler handler = new ValueHandler { RecipientId = target.InstanceId }; controller.RegisterEventEffectHandler(handler);
        CardDefinition technique = Card(CardType.Technique); Set(technique, "effects", new[] { Effect(1) });
        Set(controller, "startingHandSize", 0);
        Set(Get<TechniqueDeckRuntime>(controller, "techniqueDeckRuntime"), "hand", new[] { technique, technique });
        EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        EventEncounterOccurrence occurrence = encounters.CurrentEventOccurrence;
        Require(TechniqueRuleChecks.Play(controller, 0) && controller.CanUseTechniqueCard(0, out _) && handler.Calls == 0
            && occurrence.State == EventOccurrenceState.Unresolved && controller.CurrentDepth == 0,
            "First Technique resolved the event or closed its window.");
        Require(TechniqueRuleChecks.Play(controller, 0) && target.CurrentValue == 10 && handler.Calls == 0
            && controller.CanDescend && controller.CanSurface, "Second Technique did not remain in the event window.");
        Require((surface ? controller.TrySurface() : controller.TryDescend()) && handler.Calls == 1
            && occurrence.State == EventOccurrenceState.Resolved && !controller.HasConfirmedEventAction,
            "Confirmed action did not resolve the occurrence exactly once.");
        if (surface)
        {
            Require(Get<FishingRunResult>(controller, "lastSurfaceResult").HaulValue == 13 && target.CurrentValue == 10
                && !controller.RunActive, "Surface omitted Technique/event modifiers or cleanup changed recorded value.");
            Require(encounters.CurrentEventOccurrence == null && encounters.RecentEncounterHistory.Length == 0
                && Get<EventPersistenceRuntime>(controller, "eventPersistenceRuntime").Effects.Length == 0,
                "Completed Surface retained event owners or trails.");
        }
        else
        {
            Require(controller.CurrentDepth == 1 && target.CurrentValue == 13 && chain.Catches.Length == 1
                && controller.CurrentEncounter == source, "Event Descend attached the event or advanced incorrectly.");
            Require(encounters.CurrentEventOccurrence.Id != occurrence.Id && encounters.HasTechniqueReactionWindow
                && handler.Calls == 1, "Next event reused completed consequences or lacked its own input window.");
        }
    }
    /// <summary>Checks ordinary chain follow-ups resolve at their own confirmation without extra draws.</summary>
    private static void ValidateChain()
    {
        CardDefinition first = Card(CardType.Environment), second = Card(CardType.Opportunity);
        FishingRunController controller = Controller(first);
        EncounterChainDefinition trail = ScriptableObject.CreateInstance<EncounterChainDefinition>(); created.Add(trail);
        Set(trail, "sequence", new[] { first, second }); Set(controller.CurrentBiome, "encounterChains", new[] { trail });
        controller.StartRun(); EncounterRuntime encounters = Get<EncounterRuntime>(controller, "encounterRuntime");
        Require(encounters.ActiveEncounterChain == trail, "Authored event chain did not start.");
        EventEncounterOccurrence occurrence = encounters.CurrentEventOccurrence;
        Require(controller.TryDescend() && controller.CurrentDepth == 1 && controller.CurrentEncounter == second
            && occurrence.State == EventOccurrenceState.Resolved, "Confirmed chain event skipped its follow-up.");
        occurrence = encounters.CurrentEventOccurrence;
        Require(controller.TrySurface() && Get<FishingRunResult>(controller, "lastSurfaceResult").SurfaceDepth == 1
            && occurrence.State == EventOccurrenceState.Resolved, "Chain follow-up did not resolve before Surface.");
        Require(encounters.ActiveEncounterChain == null && encounters.RecentEncounterHistory.Length == 0,
            "Chain ending retained unfinished encounter trail.");
    }
    /// <summary>Creates a fixture with a real event-only biome pool.</summary>
    private static FishingRunController Controller(CardDefinition source)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", "Coastal"); BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", 0); Set(tier, "maximumDepth", 20); Set(tier, "encounterPool", new[] { source });
        Set(biome, "depthTiers", new[] { tier }); GameObject obj = new GameObject("Event integration checks"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>(); Set(controller, "startRunOnAwake", false);
        Set(controller, "startingBiome", biome); Set(controller, "startingLineCapacity", 100); controller.StartRun(); return controller;
    }
    /// <summary>Creates explicit shared-schema fixtures without changing production assets.</summary>
    private static CardDefinition Card(CardType type)
    { CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card); Set(card, "cardType", type); Set(card, "weight", 1); Set(card, "value", 8); return card; }
    /// <summary>Creates an attached-recipient value effect with an explicit amount.</summary>
    private static CardEffectDefinition Effect(int amount)
    { CardEffectDefinition effect = new CardEffectDefinition(); Set(effect, "effectType", CardEffectType.ModifyCatchValue); Set(effect, "target", CardEffectTarget.SpecificCaughtCard); Set(effect, "amount", amount); return effect; }
    /// <summary>Sets serialized fixture fields.</summary>
    private static void Set(object owner, string field, object value) { owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads the actual runtime owner for assertions.</summary>
    private static T Get<T>(object owner, string field) { return (T)owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Counts an invariant and reports its first failure.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
