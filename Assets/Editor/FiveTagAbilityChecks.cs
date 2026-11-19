using System;
using System.Linq;
using UnityEditor;

/// <summary>Validates installed content and its positional, scoring, recovery and movement interactions without altering saves.</summary>
public static class FiveTagAbilityChecks
{
    private static int checks;

    /// <summary>Checks every installed creature and exercises actual authored abilities through the real rig and Landing owners.</summary>
    public static int Validate()
    {
        checks = 0;
        var cards = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
            .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => CreatureTagVocabulary.AppliesTo(c.CardType)).ToArray();
        Check(cards.Length == 25 && cards.Count(c => c.CardType == CardType.Creature) == 23, "Expected 23 normal and two Apex species.");
        Check(cards.Select(c => c.UniqueId).Distinct().Count() == cards.Length, "Duplicate species identity.");
        foreach (var card in cards)
        {
            Check(CreatureTagVocabulary.IsSingleTag(card.Tags), "Invalid tag: " + card.DisplayName);
            var ability = card.PrimaryAbility.Snapshot();
            Check(ability.Effects.Count == 1 && !string.IsNullOrWhiteSpace(ability.Description), "Missing primary ability: " + card.DisplayName);
            string initial = RigAbilityText.Describe(ability, 1, false), upgraded = RigAbilityText.Describe(ability, 4, false);
            Check(!initial.Contains("{") && !upgraded.Contains("{") && initial != upgraded, "Unresolved description or missing progression: " + card.DisplayName);
            var rig = Rig(card); Acquire(rig, card);
            Check(rig.Members.Length == 1 && rig.Members[0].Level == 2 && card.PrimaryAbility.Snapshot().Effects[0].Amount(1) == ability.Effects[0].Amount(1), "Upgrade changed the shared definition.");
        }
        var biome = AssetDatabase.LoadAssetAtPath<BiomeDefinition>("Assets/Biomes/Coastal Waters.asset");
        var pool = SlotCreatureSelector.EligiblePool(biome.GetStandardCreaturePool(), biome.BiomeId, c => true);
        Check(pool.Length == 23, "New creatures missing from the standard coastal pool, or Apex leaked into Discover.");
        foreach (string tag in CreatureTagVocabulary.All) Check(pool.Any(c => SlotCreatureSelector.HasTag(c, tag)), "Missing tag coverage: " + tag);
        Check(!EncounterRuntime.TryValidateActiveTags(new[] { "Fish", "Predator", "Armored", "Anchored" }, out _), "Retired tag accepted.");
        Check(EncounterRuntime.TryValidateActiveTags(new[] { "Schooling", "Predator", "Armored", "Anchored" }, out _), "Four-of-five rejected.");

        var sardine = Card("Sardine"); var minnow = Card("Glass Minnow"); var mackerel = Card("Golden Mackerel");
        var rigGroup = Rig(sardine, mackerel, minnow);
        Check(Score(rigGroup, sardine, LandingAccuracy.Good) == 16, "Schooling counted the wrong positions.");
        Check(rigGroup.FindSpecies(mackerel.UniqueId).CurrentValue == mackerel.Value + 10, "Contiguous value bonus is incorrect.");
        Acquire(rigGroup, sardine);
        Check(Score(rigGroup, sardine, LandingAccuracy.Good) == 18, "Upgraded copies counted as multiple neighbors or did not strengthen the source.");
        var mixed = Rig(Card("Cod", "Coastal Cod"), minnow, Card("Mussel"));
        Check(Score(mixed, minnow, LandingAccuracy.Good) == 12.5m, "Mixed-neighbor cooperation failed.");

        var barnacle = Card("Acorn Barnacle"); var center = Rig(sardine, barnacle, minnow);
        center.SnapshotForLanding(out int extra, out _); Check(extra == 1, "Balanced center did not grant an attempt.");
        Acquire(center, Card("Mussel")); center.SnapshotForLanding(out extra, out _); Check(extra == 0, "Off-center source kept its attempt.");
        Rig(barnacle).SnapshotForLanding(out extra, out _); Check(extra == 0, "A lone creature qualified as the center.");
        var sponge = Card("Sea Sponge"); var anemone = Card("Sea Anemone");
        var haven = Rig(sponge, sardine, anemone, minnow);
        Check(haven.FindSpecies(sardine.UniqueId).CurrentValue == sardine.Value + 5, "Downstream value failed.");
        Check(haven.FindSpecies(anemone.UniqueId).CurrentValue == anemone.Value && haven.FindSpecies(minnow.UniqueId).CurrentValue == minnow.Value,
            "Downstream value crossed its Anchored boundary.");
        long rev = haven.Revision; int value = haven.HaulValue;
        Check(haven.TryPreview(Card("Mussel"), 1, out _, out _, out _) && haven.Revision == rev && haven.HaulValue == value, "Preview mutated value.");
        Check(Score(Rig(sardine, anemone), anemone, LandingAccuracy.Good) == 12, "Distinct-before structure failed.");
        var worm = Card("Peacock Worm"); Check(Score(Rig(sardine, worm), worm, LandingAccuracy.Good) == 18, "Line Tip bonus missing.");
        Check(Score(Rig(worm, sardine), worm, LandingAccuracy.Good) == 10, "Line Tip bonus applied away from the tip.");

        Check(Score(Rig(Card("Mussel")), Card("Mussel"), LandingAccuracy.Good) == 16, "Good reliability failed.");
        Check(Single("Coastal Cod", LandingAccuracy.Good, new LandingStrikeContext { EncounterRarity = CardRarity.Legendary }) == 18, "Higher-rarity condition failed.");
        Check(Single("Garfish", LandingAccuracy.Perfect, new LandingStrikeContext()) == 35, "First-success ambush failed.");
        Check(Single("Garfish", LandingAccuracy.Perfect, new LandingStrikeContext { Successes = 1 }) == 20, "Good did not use up ambush.");
        Check(Single("Sea Bass", LandingAccuracy.Good, new LandingStrikeContext { Points = 50, Resistance = 100 }) == 14, "Half-Resistance condition failed.");
        Check(Single("Sea Bass", LandingAccuracy.Good, new LandingStrikeContext { Points = 49, Resistance = 100 }) == 10, "Half-Resistance included the current strike.");
        Check(Single("Jellyfish", LandingAccuracy.Good, new LandingStrikeContext { EncounterTag = "Schooling" }) == 18, "Encounter tag condition failed.");
        Check(Single("Lionfish", LandingAccuracy.Good, new LandingStrikeContext { Attempts = 1 }) == 15, "Final-attempt multiplier failed.");
        Check(Single("Clinging Crab", LandingAccuracy.Good, new LandingStrikeContext()) == 20, "Good base substitution failed.");
        Check(Single("Clinging Crab", LandingAccuracy.Good, new LandingStrikeContext { Goods = 1 }) == 10, "First Good repeated.");
        Check(Single("Sea Urchin", LandingAccuracy.Good, new LandingStrikeContext { Previous = LandingAccuracy.Miss }) == 22, "Recovery sequence failed.");
        Check(Single("Small Octopus", LandingAccuracy.Perfect, new LandingStrikeContext { Previous = LandingAccuracy.Perfect }) == 35, "Perfect streak failed.");
        Check(Single("Small Octopus", LandingAccuracy.Perfect, new LandingStrikeContext { Previous = LandingAccuracy.Good }) == 20, "Good incorrectly started a Perfect streak.");
        Check(Single("Cuttlefish", LandingAccuracy.Perfect, new LandingStrikeContext()) == 26, "Precision multiplier failed.");
        Check(Single("Tidepool Eel", LandingAccuracy.Perfect, new LandingStrikeContext { Attempts = 1 }) == 40, "Final Perfect bonus failed.");
        Check(Single("Stormjaw Tuna", LandingAccuracy.Perfect, new LandingStrikeContext()) == 30, "First Perfect multiplier failed.");
        var apex = Card("Harbor Leviathan");
        Check(Score(Rig(apex, sardine, minnow, Card("Mussel")), apex, LandingAccuracy.Good) == 16, "Distinct other tags counted species instead of identities.");
        foreach (var card in cards) Check(Score(Rig(card), card, LandingAccuracy.Miss, new LandingStrikeContext { Attempts = 1, Resistance = 100 }) == 0, "An ordinary Miss scored: " + card.DisplayName);

        TestRecovery(); TestAssistance();
        return checks;
    }

    /// <summary>Checks grants before terminal exhaustion, repeat protection, stale commands and encounter reset.</summary>
    private static void TestRecovery()
    {
        var shrimp = Rig(Card("Cleaner Shrimp")); var model = Begin(shrimp, 1); double clock = 0;
        Strike(model, LandingAccuracy.Miss, ref clock);
        Check(model.AttemptsRemaining == 1 && model.State == LandingChallengeState.Active && model.Points == 0, "First-Miss grant failed on the last attempt.");
        Check(model.LastScore.Contributions.Count == 1 && model.LastScore.Contributions[0].Attempts == 1
            && model.LastScore.Contributions[0].SourceId == shrimp.Members[0].InstanceId, "Attempt grant lost source attribution.");
        Check(!model.TryStrike(model.Occurrence, model.Revision - 1, clock), "Stale strike reused a grant.");
        Strike(model, LandingAccuracy.Miss, ref clock);
        Check(model.State == LandingChallengeState.Failed, "First-Miss grant repeated.");
        model.Begin(2, 999999, Settings(1), shrimp.SnapshotForLanding(out _, out _), new Random(17), clock);
        Strike(model, LandingAccuracy.Miss, ref clock); Check(model.State == LandingChallengeState.Active, "Miss grant did not reset.");
        var lobster = Begin(Rig(Card("Spiny Lobster")), 2); clock = 0;
        Strike(lobster, LandingAccuracy.Perfect, ref clock); Strike(lobster, LandingAccuracy.Good, ref clock);
        Check(lobster.AttemptsRemaining == 1 && lobster.State == LandingChallengeState.Active, "Consecutive-success grant failed at exhaustion.");
        Strike(lobster, LandingAccuracy.Good, ref clock); Check(lobster.State == LandingChallengeState.Failed, "Once-per-encounter endurance repeated.");
        var successful = new LandingChallengeRuntime();
        successful.Begin(1, 40, Settings(2), Rig(Card("Spiny Lobster")).SnapshotForLanding(out _, out _), new Random(17), 0); clock = 0;
        Strike(successful, LandingAccuracy.Perfect, ref clock); Strike(successful, LandingAccuracy.Perfect, ref clock);
        Check(successful.State == LandingChallengeState.Succeeded && !successful.TryStrike(successful.Occurrence, successful.Revision, clock),
            "An attempt grant extended a successful encounter.");
        var crab = Begin(Rig(Card("Clinging Crab"), Card("Cuttlefish")), 4); clock = 0;
        Strike(crab, LandingAccuracy.Good, ref clock);
        Check(crab.LastScore.Accuracy == LandingAccuracy.Good && crab.LastScore.Points == 20, "Base substitution reclassified Good or triggered Perfect multipliers.");
    }

    /// <summary>Checks assistance bounds, immutable snapshots and deterministic slowing across frame subdivision.</summary>
    private static void TestAssistance()
    {
        var baseSettings = Settings(4);
        var wide = new LandingChallengeRuntime();
        wide.Begin(1, 40, new LandingSettings(4, 40, 0, 10, 20, .95, .08, .05, 3.4, .26, 0), Array.Empty<LandingRigMember>(), new Random(17), 0);
        Check(wide.GoodWidth == .95, "Assistance limits shrank the authored baseline.");
        var flounder = Begin(Rig(Card("Flounder")), 4);
        Check(Math.Abs(flounder.PerfectWidth - baseSettings.PerfectWidth * 1.2) < .000001 && flounder.GoodWidth == baseSettings.GoodWidth, "Perfect assistance changed Good.");
        var ray = Begin(Rig(Card("Moonlit Ray")), 4);
        Check(Math.Abs(ray.GoodWidth - baseSettings.GoodWidth * 1.15) < .000001 && ray.PerfectWidth == baseSettings.PerfectWidth, "Good assistance changed Perfect.");
        var squid = Rig(Card("squid")); var a = Begin(squid, 4); var b = Begin(squid, 4); var plain = Begin(new RigRuntime(), 4);
        a.AdvanceTo(5); for (int i = 1; i <= 500; i++) b.AdvanceTo(i / 100.0); plain.AdvanceTo(5);
        Check(Math.Abs(a.FishPosition - b.FishPosition) < .000001 && a.ZonePosition == b.ZonePosition, "Slow assistance depends on render cadence.");
        Check(Math.Abs(a.FishPosition - plain.FishPosition) > .000001 && a.ZonePosition == plain.ZonePosition, "Slowing did not affect only the fish.");
        for (int i = 0; i < 60; i++) Acquire(squid, Card("squid"));
        var upgraded = Begin(squid, 4);
        Check(squid.Members[0].PrimaryAbility.Effects[0].Amount(61) == .6m && a.PerfectWidth == baseSettings.PerfectWidth, "Assistance cap or snapshot boundary failed.");
    }

    /// <summary>Finds an installed definition without substituting a fixture or a species-specific runtime handler.</summary>
    private static CardDefinition Card(string name, string filename = null) => AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/" + (filename ?? name) + ".asset") ?? throw new InvalidOperationException("Missing " + name);
    /// <summary>Builds a committed rig with all original relative positions preserved.</summary>
    private static RigRuntime Rig(params CardDefinition[] cards) { var rig = new RigRuntime(); foreach (var card in cards) Acquire(rig, card); return rig; }
    /// <summary>Acquires through the transactional production entry point.</summary>
    private static void Acquire(RigRuntime rig, CardDefinition card) { Check(rig.TryAcquire(rig.Revision, card, rig.Members.Length, out _, out string error), error ?? "Acquisition failed."); }
    /// <summary>Scores only the selected source while preserving its actual positional conditions.</summary>
    private static decimal Score(RigRuntime rig, CardDefinition card, LandingAccuracy accuracy, LandingStrikeContext context = null)
    {
        int id = rig.FindSpecies(card.UniqueId).InstanceId;
        var members = rig.SnapshotForLanding(out _, out _).Select(m => new LandingRigMember(m.Id, m.Name, m.Role, m.Tags.ToArray(), m.Id == id ? m.Rules.ToArray() : Array.Empty<LandingRigRule>(), m.Level, m.Rarity)).ToArray();
        return LandingScoring.Resolve(accuracy, Settings(4), members, context).Points;
    }
    /// <summary>Resolves a conditional rule from its actual installed definition.</summary>
    private static decimal Single(string name, LandingAccuracy accuracy, LandingStrikeContext context) => Score(Rig(Card(name)), Card(name), accuracy, context);
    /// <summary>Supplies deterministic tuning with no presentation freeze for fast real-strike checks.</summary>
    private static LandingSettings Settings(int attempts) => new LandingSettings(attempts, 40, 0, 10, 20, .24, .08, .05, 3.4, .26, 0);
    /// <summary>Starts the real challenge with all captured assistance.</summary>
    private static LandingChallengeRuntime Begin(RigRuntime rig, int attempts)
    {
        var members = rig.SnapshotForLanding(out int extra, out int protection); var model = new LandingChallengeRuntime();
        model.Begin(1, 999999, Settings(attempts), members, new Random(17), 0, extra, protection); return model;
    }
    /// <summary>Finds a real overlap grade on the deterministic track, avoiding a test-only scoring bypass.</summary>
    private static void Strike(LandingChallengeRuntime model, LandingAccuracy accuracy, ref double clock)
    {
        for (int i = 0; i < 30000; i++)
        {
            clock += .005; model.AdvanceTo(clock);
            if (LandingChallengeRuntime.Grade(model.FishPosition, model.FishWidth, model.ZonePosition, model.GoodWidth, model.PerfectWidth) != accuracy) continue;
            Check(model.TryStrike(model.Occurrence, model.Revision, clock), "Real strike rejected."); return;
        }
        throw new InvalidOperationException("Could not find " + accuracy + " overlap.");
    }
    /// <summary>Reports the first broken invariant while counting completed checks.</summary>
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks++; }
}
