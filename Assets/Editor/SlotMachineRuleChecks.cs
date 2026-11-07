using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>Checks approved Discover probability, matching, eligibility, fallback and landing-handoff rules.</summary>
public static class SlotMachineRuleChecks
{
    private static int assertions;

    /// <summary>Runs deterministic rules against real runtime classes, without modifying assets or player saves.</summary>
    public static int ValidateRules()
    {
        assertions = TagSlotMachineChecks.ValidateRules();
        CheckProbabilities();
        var cards = new List<CardDefinition>();
        var profile = ScriptableObject.CreateInstance<SlotMachineProfile>();
        try
        {
            SlotSelectionSettings settings = profile.CreateSettings();
            CardDefinition predator = Card(cards, "predator", CardRarity.Common, "Predator");
            CardDefinition both = Card(cards, "both", CardRarity.Common, "Predator", "Schooling");
            CardDefinition fish = Card(cards, "fish", CardRarity.Rare, "Fish", "Small");
            CardDefinition high = Card(cards, "high", CardRarity.Rare, "Predator");
            CardDefinition apex = Card(cards, "apex", CardRarity.Legendary, "Predator");
            Set(apex, "cardType", CardType.ApexEncounter);
            CardDefinition foreign = Card(cards, "foreign", CardRarity.Common, "Predator");
            Set(foreign, "biomeIds", new[] { "elsewhere" });
            CardDefinition locked = Card(cards, "locked", CardRarity.Legendary, "Predator");
            Set(locked, "requiredCreatureUnlockId", "unlock_locked");
            Set(predator, "minimumDepth", 999);
            CardDefinition[] pool = { predator, both, fish, high, apex, foreign, locked, predator };
            CardDefinition[] eligible = SlotCreatureSelector.EligiblePool(pool, "test", null);
            Assert(eligible.Length == 4 && Array.IndexOf(eligible, predator) >= 0,
                "Biome/unlock/Apex filtering or normal-creature depth independence failed.");
            Assert(SlotCreatureSelector.EligiblePool(pool, "test", c => c != locked).Length == 4, "Unlock callback ignored.");
            Assert(SlotCreatureSelector.EligiblePool(pool, "test", c => true).Length == 5, "Granted creature unlock rejected.");
            var pair = Match("Predator", "Schooling", "Predator");
            Assert(pair.Kind == SlotMatchKind.Pair && pair.GuaranteedTag == "Predator" && pair.PreferredTag == "Schooling", "Pair interpretation failed.");
            for (int ticket = 0; ticket < 3; ticket++)
            {
                Assert(SlotCreatureSelector.TrySelect(eligible, pair, settings, new Tickets(0, ticket), out var s, out _), "Pair selection failed.");
                Assert(s.Creature == (ticket == 0 ? predator : both), "Secondary preference weights are wrong.");
            }
            // Requested Uncommon is equally far from Common/Rare. The documented tie goes downward.
            Assert(SlotCreatureSelector.TrySelect(eligible, pair, settings, new Tickets(55, 0), out var tied, out _)
                && tied.RequestedRarity == CardRarity.Uncommon && tied.ResolvedRarity == CardRarity.Common,
                "Nearest-rarity tie did not preserve the tag and choose the lower tier.");
            Assert(SlotCreatureSelector.TrySelect(new[] { predator, fish }, pair, settings, new Tickets(83, 0), out var fallback, out _)
                && fallback.Creature == predator, "Rarity fallback abandoned the guaranteed tag.");
            var different = Match("Predator", "Schooling", "Small");
            Assert(different.Kind == SlotMatchKind.ThreeDifferent && different.GuaranteedTag == null, "Different symbols imposed a tag.");
            Assert(SlotCreatureSelector.TrySelect(new[] { fish }, different, settings, new Tickets(0, 0), out var open, out _)
                && open.Creature == fish, "Different symbols excluded an unshown tag.");
            var triple = Match("Predator", "Predator", "Predator");
            Assert(SlotCreatureSelector.TrySelect(eligible, triple, settings, new Tickets(0, 0), out var commonTriple, out _)
                && commonTriple.ResolvedRarity == CardRarity.Common, "Triple incorrectly guaranteed a rare creature.");
            Assert(!SlotCreatureSelector.TrySelect(new[] { fish }, triple, settings, new Tickets(), out _, out var error)
                && error != null, "Empty guaranteed-tag pool silently fell back.");
            Assert(!EncounterRuntime.TryValidateActiveTags(new[] { "Fish", "Fish", "Small", "Predator" }, out _), "Duplicate active tags accepted.");
            Assert(!EncounterRuntime.TryValidateActiveTags(new[] { "Fish", "Predator", "Small", "Unknown" }, out _), "Unknown active tag accepted.");
            string[] active = { "Fish", "Predator", "Schooling", "Small" };
            var runtime = new EncounterRuntime();
            int reveals = 0;
            runtime.CreatureRevealed += (card, revision) =>
            {
                reveals++;
                Assert(runtime.AwaitingLandingChallenge && runtime.TakeHookedEncounter() == null, "Reveal callback acquired an unlanded creature.");
            };
            Assert(runtime.BeginSlotEncounter(eligible, "test", active, settings, new Tickets(0, 0, 0), null), "Discover did not begin.");
            long old = runtime.SlotMachine.Revision;
            Assert(runtime.CurrentEncounter == null && reveals == 0, "Creature revealed before accepting.");
            Assert(runtime.SlotMachine.RespinsRemaining == 1, "Baseline did not allow exactly one respin.");
            Assert(runtime.CompleteSlotEncounter(old, new Tickets(0, 0)), "Accepted result did not reveal.");
            Assert(runtime.CurrentEncounter == fish && reveals == 1 && runtime.AwaitingLandingChallenge, "Landing handoff failed.");
            Assert(!runtime.CompleteSlotEncounter(old, new Tickets()) && reveals == 1, "Duplicate acceptance revealed again.");
            Assert(runtime.TakeHookedEncounter() == null, "Slot completion automatically caught a creature.");
            runtime.Reset(true);
            Assert(!runtime.SlotMachine.Active && !runtime.AwaitingLandingChallenge && runtime.CurrentEncounter == null, "Biome reset retained slot ownership.");
            Assert(!runtime.CompleteSlotEncounter(old, new Tickets()), "Stale slot completion accepted after reset.");
            Assert(!runtime.BeginSlotEncounter(new[] { predator }, "test", active, settings, new Tickets(), null)
                && runtime.SlotFailure != null && !runtime.SlotMachine.Active, "Missing guaranteed-tag coverage did not fail safely.");
            return assertions;
        }
        finally
        {
            foreach (CardDefinition card in cards) UnityEngine.Object.DestroyImmediate(card);
            UnityEngine.Object.DestroyImmediate(profile);
        }
    }

    /// <summary>Enumerates exact initial and accept-matches/respin-different probabilities rather than sampling them.</summary>
    private static void CheckProbabilities()
    {
        int[] initial = new int[3], final = new int[3];
        string[] names = { "Fish", "Predator", "Schooling", "Small" };
        for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++) for (int c = 0; c < 4; c++)
        {
            var match = Match(names[a], names[b], names[c]);
            initial[(int)match.Kind]++;
            if (match.Kind != SlotMatchKind.ThreeDifferent) final[(int)match.Kind] += 16;
            else for (int x = 0; x < 4; x++) for (int y = 0; y < 4; y++)
                final[(int)Match(names[a], names[x], names[y]).Kind]++;
        }
        Assert(initial[0] == 24 && initial[1] == 36 && initial[2] == 4, "Initial 37.5/56.25/6.25 probabilities differ.");
        Assert(final[0] == 144 && final[1] == 792 && final[2] == 88, "Optional-respin strategy probabilities differ.");
    }

    /// <summary>Creates exact settled faces through the public reel API.</summary>
    private static SlotMatch Match(string a, string b, string c)
    {
        var machine = new TagSlotMachineRuntime();
        machine.Begin(new IReadOnlyList<TagSlotMachineRuntime.Symbol>[]
        {
            new[] { new TagSlotMachineRuntime.Symbol(a, 1) }, new[] { new TagSlotMachineRuntime.Symbol(b, 1) },
            new[] { new TagSlotMachineRuntime.Symbol(c, 1) }
        }, 1);
        machine.TrySpin(machine.Revision, new Tickets(0, 0, 0));
        machine.TryAccept(machine.Revision, out var result);
        return new SlotMatch(result);
    }

    /// <summary>Creates isolated real card definitions for availability and selection checks.</summary>
    private static CardDefinition Card(List<CardDefinition> owned, string id, CardRarity rarity, params string[] tags)
    {
        var card = ScriptableObject.CreateInstance<CardDefinition>(); owned.Add(card);
        Set(card, "uniqueId", id); Set(card, "displayName", id); Set(card, "cardType", CardType.Creature);
        Set(card, "rarity", rarity); Set(card, "tags", tags);
        return card;
    }

    /// <summary>Configures private authored fixture fields without adding test APIs to runtime definitions.</summary>
    private static void Set(object target, string field, object value)
    { target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value); }

    /// <summary>Fails immediately on an invariant violation and records successful checks.</summary>
    private static void Assert(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); assertions++; }

    private sealed class Tickets : System.Random
    {
        private readonly Queue<int> tickets;
        /// <summary>Stores an exact deterministic roll sequence.</summary>
        public Tickets(params int[] values) { tickets = new Queue<int>(values); }
        /// <summary>Detects unexpected random calls and incorrect weighted ranges.</summary>
        public override int Next(int maxValue)
        {
            int ticket = tickets.Dequeue();
            if (ticket < 0 || ticket >= maxValue) throw new InvalidOperationException("Invalid weighted ticket.");
            return ticket;
        }
    }
}
