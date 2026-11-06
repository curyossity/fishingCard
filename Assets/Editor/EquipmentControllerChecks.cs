using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Checks authored loadouts through real controller actions using disposable owners and memory-only saves.</summary>
public static class EquipmentControllerChecks
{
    private static readonly List<UnityEngine.Object> fixtures = new List<UnityEngine.Object>();
    private static int assertions;

    /// <summary>Verifies combined dynamic passives, mandatory filtering and post-Technique draw/reset ordering.</summary>
    public static void Validate(string output)
    {
        assertions = 0;
        try
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>(); fixtures.Add(card);
            Set(card, "cardType", CardType.Creature); Set(card, "weight", 3); Set(card, "value", 8);
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>(); fixtures.Add(biome); Set(biome, "biomeId", "Coastal");
            var tier = new BiomeDepthTierDefinition(); Set(tier, "maximumDepth", 100); Set(tier, "encounterPool", new[] { card });
            Set(biome, "depthTiers", new[] { tier });
            var obj = new GameObject("Equipment controller verification"); obj.SetActive(false); fixtures.Add(obj);
            var c = obj.AddComponent<FishingRunController>(); Set(c, "startRunOnAwake", false);
            Set(c, "startingBiome", biome); Set(c, "startingHandSize", 0); Set(c, "startingLineCapacity", 100);
            var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath);
            var shop = new TackleShopRuntime(catalog, 10000, 0, () => null, _ => { }); Set(c, "tackleShop", shop);
            string reason;
            foreach (var item in catalog.Items) Check(shop.TryPurchase(item.Id, shop.Gold, 0, false, out reason), "Fixture purchase failed.");
            Equip(shop, "ironback_rod", "pearlwind_reel", "living_hook"); c.StartRun();
            var deck = Get<TechniqueDeckRuntime>(c, "techniqueDeckRuntime");
            var cut = AssetDatabase.FindAssets("t:CardDefinition").Select(id => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(id))).First(x => x.UniqueId == "cut_loose");
            deck.Initialize(new[] { cut, cut, cut }, 1, new System.Random(3), null);
            Check(c.TryChooseEncounterRole(EncounterEffectChoice.Bait) && c.TryDescend(), "First Bait commitment failed.");
            Check(c.HasPendingCreatureChoice && deck.Hand.Length == 2 && !c.CanDescend, "Pearlwind did not pause for mandatory discard.");
            Check(c.TryChooseRequiredTechniqueDiscard(0) && deck.Hand.Length == 1 && !c.HasPendingCreatureChoice, "Pearlwind discard did not resume Descend.");
            Check(c.TryChooseEncounterRole(EncounterEffectChoice.Bait) && c.TryDescend() && c.TryChooseRequiredTechniqueDiscard(0), "Second Bait filter failed.");
            var chain = Get<CatchChainRuntime>(c, "catchChainRuntime");
            Check(chain.Catches.Length == 2 && chain.Catches[0].CurrentWeight == 0 && chain.Catches[1].CurrentWeight == 2,
                "Ironback and Living did not combine with a zero floor.");
            Check(TechniqueRuleChecks.ReleaseFixture(c, 0) && chain.Catches[0].CurrentWeight == 1,
                "Removal left stale Living/Ironback conditions.");

            Set(c, "runActive", false); Equip(shop, "stormbent_rod", "clockwork_reel", "twinfang_hook");
            Set(c, "startingLineCapacity", 4); c.StartRun();
            Check(c.TryChooseEncounterRole(EncounterEffectChoice.Catch) && c.TryDescend(), "Catch commitment failed.");
            Check(c.TryChooseEncounterRole(EncounterEffectChoice.Bait) && c.TryDescend(), "Support Bait commitment failed.");
            Check(chain.Catches[0].CurrentValue == 10 && chain.Catches[1].HaulValueContribution == 0,
                "Overload and Bait-conditioned bonuses did not combine.");
            deck.Initialize(new[] { cut, cut, cut }, 1, new System.Random(3), null);
            Check(TechniqueRuleChecks.Play(c, 0) && deck.Hand.Length == 1, "Clockwork did not draw after the empty-hand play.");
            Check(TechniqueRuleChecks.Play(c, 0) && deck.Hand.Length == 0, "Clockwork repeated during one encounter.");
            Check(c.TryChooseEncounterRole(EncounterEffectChoice.Catch) && c.TryDescend(), "Encounter advancement failed.");
            deck.DrawTechniques(1, new System.Random(3));
            Check(TechniqueRuleChecks.Play(c, 0) && deck.Hand.Length == 1, "Advancement failed to reset Clockwork.");
            Directory.CreateDirectory(output);
            File.WriteAllText(output + "ControllerVerification.json", "{\"passed\":true,\"assertions\":" + assertions + ",\"playerSavesTouched\":false}");
        }
        finally { foreach (var fixture in fixtures) UnityEngine.Object.DestroyImmediate(fixture); fixtures.Clear(); }
    }

    /// <summary>Equips a three-category test loadout through normal category replacement.</summary>
    private static void Equip(TackleShopRuntime shop, params string[] ids)
    { foreach (string id in ids) Check(shop.TryEquip(id, shop.Gold, 0, false, out _), "Fixture equip failed: " + id); }
    /// <summary>Sets private fixture state without modifying serialized production assets.</summary>
    private static void Set(object owner, string name, object value) { owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value); }
    /// <summary>Reads the actual controller-owned runtime.</summary>
    private static T Get<T>(object owner, string name) { return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    /// <summary>Stops at the first broken integration invariant.</summary>
    private static void Check(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
