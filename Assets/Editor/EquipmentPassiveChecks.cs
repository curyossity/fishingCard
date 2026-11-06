using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks all nine passive behaviors using isolated definitions, without changing live equipment or saves.</summary>
public static class EquipmentPassiveChecks
{
    private static readonly List<UnityEngine.Object> fixtures = new List<UnityEngine.Object>();
    private static int assertions;

    /// <summary>Validates attachment counters, dynamic stats, draw boundaries and ordered Bait filtering.</summary>
    public static void Validate(string output = "Docs/Validation/EquipmentPassives/")
    {
        assertions = 0;
        try
        {
            var cod = Card("creature_coastal_cod"); var lobster = NeutralCreature("creature_spiny_lobster");
            var cut = Card("cut_loose"); var random = new System.Random(17);
            var engine = new EquipmentPassiveRuntime(); var resolver = new EffectResolver { ResolveExternalPassiveStats = engine.ResolvePassiveStats };
            var chain = new CatchChainRuntime(); chain.BeforeAttachmentEffects += engine.BeforeAttachment;
            var rod = Item("Driftwood Rod", EquipmentPassiveKind.ReduceFirstAttachmentWeights, 1, 2);
            engine.BeginRun(rod);
            var preview = chain.CreateResolvedCatchPreview(cod, resolver, CatchAttachmentRole.Bait, engine.PreviewCatchBonus);
            Check(preview.CurrentWeight == cod.Weight - 1 && engine.CreatureAttachmentCount == 0, "Weight preview consumed a grant.");
            var first = chain.Add(cod, resolver, CatchAttachmentRole.Bait); var second = chain.Add(cod, resolver);
            Check(first.CurrentWeight == cod.Weight - 1 && second.CurrentWeight == cod.Weight - 1, "First two roles did not receive reductions.");
            chain.TryReleaseTogether(new[] { first.InstanceId }, resolver); var third = chain.Add(cod, resolver);
            Check(third.CurrentWeight == cod.Weight && engine.CreatureAttachmentCount == 3, "Release transferred a reduction.");
            chain.TryChangeCreatureMode(second.InstanceId, resolver); Check(engine.CreatureAttachmentCount == 3, "Mode change consumed an attachment.");

            engine.BeginRun(Item("Ironback Rod", EquipmentPassiveKind.ReduceHeaviestWeight, 2, 1)); chain.Reset();
            first = chain.Add(cod, resolver); var heavy = chain.Add(lobster, resolver, CatchAttachmentRole.Bait); var later = chain.Add(lobster, resolver);
            Check(first.CurrentWeight == cod.Weight && heavy.CurrentWeight == Math.Max(0, lobster.Weight - 2) && later.CurrentWeight == lobster.Weight, "Original-weight or oldest tie target incorrect.");
            chain.TryReleaseTogether(new[] { heavy.InstanceId }, resolver);
            Check(later.CurrentWeight == Math.Max(0, lobster.Weight - 2), "Heaviest reduction failed to move after removal.");
            Check(lobster.Weight == 3, "Equipment mutated the creature definition.");

            engine.BeginRun(Item("Living Hook", EquipmentPassiveKind.ReduceBaitWeightsWithMinimumCount, 1, 2)); chain.Reset();
            first = chain.Add(cod, resolver, CatchAttachmentRole.Bait); Check(first.CurrentWeight == cod.Weight, "One Bait activated Living Hook.");
            second = chain.Add(cod, resolver, CatchAttachmentRole.Bait); third = chain.Add(cod, resolver);
            Check(first.CurrentWeight == cod.Weight - 1 && second.CurrentWeight == cod.Weight - 1 && third.CurrentWeight == cod.Weight, "Living Hook targets incorrect.");
            chain.TryReleaseTogether(new[] { first.InstanceId }, resolver); Check(second.CurrentWeight == cod.Weight, "Living Hook failed to deactivate.");
            chain.TryChangeCreatureMode(third.InstanceId, resolver); Check(second.CurrentWeight == cod.Weight - 1, "Mode changes did not recalculate Bait count.");
            chain.LoseRandomCatchesAfterLineBreak(resolver, new System.Random(5));
            int remainingBaits = chain.Catches.Count(c => c.IsBait);
            Check(chain.Catches.All(c => c.CurrentWeight == Math.Max(0, cod.Weight - (c.IsBait && remainingBaits >= 2 ? 1 : 0))),
                "Line loss left stale Living Hook reductions.");

            engine.BeginRun(Item("Twinfang Hook", EquipmentPassiveKind.CatchValueWithMinimumBaitCount, 1, 1)); chain.Reset();
            first = chain.Add(cod, resolver); second = chain.Add(cod, resolver, CatchAttachmentRole.Bait);
            Check(first.CurrentValue == cod.Value + 1 && second.HaulValueContribution == 0, "Twinfang applied sale value to Bait or missed Catch.");
            Check(new CardStatBreakdown(first, CreatureStat.Value, 1).Reasons.Any(r => r.Name == "Twinfang Hook" && r.Amount == 1), "Equipment provenance missing.");
            chain.TryReleaseTogether(new[] { second.InstanceId }, resolver); Check(first.CurrentValue == cod.Value, "Twinfang left a stale bonus.");

            int capacity = cod.Weight * 2 - 1; engine.BindCapacity(() => capacity);
            engine.BeginRun(Item("Stormbent Rod", EquipmentPassiveKind.CatchValueWhileOverloaded, 1, 1)); chain.Reset();
            first = chain.Add(cod, resolver); second = chain.Add(cod, resolver);
            Check(first.CurrentValue == cod.Value + 1 && second.CurrentValue == cod.Value + 1, "Overload value missing.");
            capacity = cod.Weight * 2; chain.Recalculate(resolver); Check(first.CurrentValue == cod.Value, "Equality with capacity counted as overload.");
            var bait = chain.Add(cod, resolver, CatchAttachmentRole.Bait); Check(first.CurrentValue == cod.Value + 1 && bait.HaulValueContribution == 0, "Bait overload weight or zero sale value incorrect.");

            engine.BeginRun(Item("Barbed Hook", EquipmentPassiveKind.ValueEveryNthCatch, 2, 3)); chain.Reset();
            bait = chain.Add(cod, resolver, CatchAttachmentRole.Bait);
            chain.TryChangeCreatureMode(bait.InstanceId, resolver); Check(engine.CatchAttachmentCount == 0, "Bait or mode change incremented Catch count.");
            first = chain.Add(cod, resolver); second = chain.Add(cod, resolver);
            preview = chain.CreateResolvedCatchPreview(cod, resolver, CatchAttachmentRole.Catch, engine.PreviewCatchBonus);
            Check(preview.CurrentValue == cod.Value + 2 && engine.CatchAttachmentCount == 2, "Nth-Catch preview consumed counter or missed bonus.");
            third = chain.Add(cod, resolver); Check(third.CurrentValue == cod.Value + 2 && first.CurrentValue == cod.Value, "Third Catch bonus incorrect.");
            chain.TryReleaseTogether(new[] { third.InstanceId }, resolver); chain.Add(cod, resolver); chain.Add(cod, resolver); var sixth = chain.Add(cod, resolver);
            Check(sixth.CurrentValue == cod.Value + 2 && engine.CatchAttachmentCount == 6, "Sixth Catch bonus or nonrefundable count incorrect.");
            engine.BeginBiome(new TechniqueDeckRuntime(), random); Check(engine.CatchAttachmentCount == 6, "Biome reset Catch count.");

            var deck = new TechniqueDeckRuntime(); var cards = Enumerable.Repeat(cut, 8).ToArray();
            engine.BeginRun(Item("Tideglass Reel", EquipmentPassiveKind.DrawAtBiomeStart, 1, 1)); deck.Initialize(cards, 4, random, null);
            Check(engine.BeginBiome(deck, random) == 1 && deck.Hand.Length == 5, "First biome extra draw incorrect.");
            Check(engine.BeginBiome(deck, random) == 1 && deck.Hand.Length == 6, "Later biome draw missing.");
            deck.DrawTechniques(20, random); deck.TryDiscardTechnique(0); int hand = deck.Hand.Length;
            Check(engine.BeginBiome(deck, random) == 1 && deck.Hand.Length == hand + 1 && deck.DiscardPile.Length == 0, "Reel failed to use normal reshuffling.");
            Check(engine.BeginBiome(deck, random) == 0, "Reel invented a card when both piles were empty.");

            engine.BeginRun(Item("Clockwork Reel", EquipmentPassiveKind.DrawAfterEmptyHandTechnique, 1, 1)); deck.Initialize(cards, 1, random, null);
            Check(engine.CompleteTechnique(deck, random) == 0, "Nonempty hand triggered Clockwork.");
            PlayOne(deck); Check(engine.CompleteTechnique(deck, random) == 1 && deck.Hand.Length == 1, "Empty final hand did not trigger Clockwork.");
            PlayOne(deck); Check(engine.CompleteTechnique(deck, random) == 0 && deck.Hand.Length == 0, "Clockwork repeated in one encounter.");
            engine.BeginEncounter(); Check(engine.CompleteTechnique(deck, random) == 1, "New encounter did not reset Clockwork opportunity.");

            var pearl = Item("Pearlwind Reel", EquipmentPassiveKind.FilterAfterBaitAttachment, 1, 1);
            engine.BeginRun(pearl); chain.Reset(); deck.Initialize(cards, 4, random, null);
            var reactions = new CreatureEffectRuntime { NewAttachmentReactions = engine.RecordBaitAttachment };
            first = chain.Add(cod, resolver); bait = chain.Add(Card("creature_clinging_crab"), resolver, CatchAttachmentRole.Bait);
            reactions.RecordAttachment(bait, false, chain.Catches);
            Prepare(reactions, chain, resolver, deck, random);
            Check(reactions.ChoiceKind == CreatureEffectChoiceKind.ReleaseAttachedCard && deck.Hand.Length == 4, "Pearlwind drew before the Bait's own effect finished.");
            Check(reactions.TryRelease(first.InstanceId, chain, resolver), "Crab own release could not finish.");
            Prepare(reactions, chain, resolver, deck, random);
            Check(reactions.ChoiceKind == CreatureEffectChoiceKind.DiscardTechnique && deck.Hand.Length == 5, "Pearlwind did not draw before discarding.");
            Check(reactions.TryChooseRequiredDiscard(4, deck) && deck.Hand.Length == 4, "Pearlwind failed to discard the selected card or changed final hand size.");
            Prepare(reactions, chain, resolver, deck, random); Check(reactions.ChoiceKind == CreatureEffectChoiceKind.None, "Equipment choice remained queued.");
            deck.Initialize(new[] { cut }, 1, random, null); reactions.QueueEquipmentFilter(pearl.SourceCard, 1);
            Prepare(reactions, chain, resolver, deck, random); Check(reactions.ChoiceKind == CreatureEffectChoiceKind.None && deck.Hand.Length == 1, "Empty-pile Pearlwind discarded without drawing.");
            deck.TryDiscardTechnique(0); reactions.QueueEquipmentFilter(pearl.SourceCard, 1);
            Prepare(reactions, chain, resolver, deck, random);
            Check(deck.Hand.Length == 1 && reactions.ChoiceKind == CreatureEffectChoiceKind.DiscardTechnique
                && reactions.TryChooseRequiredDiscard(0, deck) && deck.Hand.Length == 0,
                "Pearlwind failed to reshuffle before its mandatory discard.");
            Directory.CreateDirectory(output); File.WriteAllText(output + "PassiveEngineVerification.json", "{\"passed\":true,\"assertions\":" + assertions + ",\"liveCatalogChanged\":false}");
        }
        finally { foreach (var fixture in fixtures) UnityEngine.Object.DestroyImmediate(fixture); fixtures.Clear(); }
    }

    /// <summary>Creates an isolated serialized definition with no purchase, asset, or save side effects.</summary>
    private static TackleShopCatalog.Item Item(string name, EquipmentPassiveKind kind, int amount, int threshold)
    {
        var source = ScriptableObject.CreateInstance<CardDefinition>(); fixtures.Add(source);
        JsonUtility.FromJsonOverwrite("{\"displayName\":\"" + name + "\",\"effects\":[{\"effectId\":\"passive_fixture\",\"displayName\":\"" + name + "\"}]}", source);
        var s = new SerializedObject(source); s.FindProperty("cardType").enumValueIndex = (int)CardType.Equipment; s.ApplyModifiedPropertiesWithoutUndo();
        var catalog = ScriptableObject.CreateInstance<TackleShopCatalog>(); fixtures.Add(catalog);
        JsonUtility.FromJsonOverwrite("{\"items\":[{\"id\":\"fixture\",\"displayName\":\"" + name + "\",\"passiveEffects\":[{\"kind\":" + (int)kind + ",\"amount\":" + amount + ",\"threshold\":" + threshold + "}]}]}", catalog);
        var c = new SerializedObject(catalog); c.FindProperty("items").GetArrayElementAtIndex(0).FindPropertyRelative("sourceCard").objectReferenceValue = source; c.ApplyModifiedPropertiesWithoutUndo(); return catalog.Items[0];
    }

    /// <summary>Finds an existing exact creature/Technique identity without editing its authored data.</summary>
    private static CardDefinition Card(string id) { return AssetDatabase.FindAssets("t:CardDefinition").Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.UniqueId == id); }
    /// <summary>Removes creature-specific passives only on an isolated copy to test equipment targeting independently.</summary>
    private static CardDefinition NeutralCreature(string id)
    {
        var card = UnityEngine.Object.Instantiate(Card(id)); fixtures.Add(card); var data = new SerializedObject(card);
        data.FindProperty("effects").arraySize = 0; data.FindProperty("baitEffects").arraySize = 0;
        data.FindProperty("hasExplicitBaitData").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo(); return card;
    }
    /// <summary>Finishes one physical play so its discard is visible to post-Technique reactions.</summary>
    private static void PlayOne(TechniqueDeckRuntime deck) { Check(deck.TryBeginPlay(0, out _, out _), "Fixture play failed."); Check(deck.TryCompletePlay(deck.ResolvingOccurrenceId), "Fixture discard failed."); }
    /// <summary>Runs the actual ordered choice queue with no encounter or player-save mutations.</summary>
    private static void Prepare(CreatureEffectRuntime reactions, CatchChainRuntime chain, EffectResolver resolver, TechniqueDeckRuntime deck, System.Random random) { reactions.PrepareChoice(chain, new EncounterRuntime(), null, new TechniqueEffectRuntime(), resolver, random, () => 0, deck); }
    /// <summary>Stops immediately on a violated passive contract.</summary>
    private static void Check(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
