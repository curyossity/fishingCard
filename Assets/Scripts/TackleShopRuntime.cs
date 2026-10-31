using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns durable equipment transactions and their shared gold snapshot, with no UI or active-run effects.</summary>
public sealed class TackleShopRuntime
{
    [Serializable] private sealed class SaveData
    {
        public int version = 1;
        public int gold;
        public int lineCapacityUpgrades;
        public List<string> owned = new List<string>();
        public string equipped = "";
    }
    private SaveData state;
    private readonly TackleShopCatalog catalog;
    private readonly Action<string> persist;
    private bool readOnly;
    public string Error { get; private set; }
    public bool CanEdit => !readOnly;
    public int Gold => state.gold;
    public int LineCapacityUpgrades => state.lineCapacityUpgrades;
    public string EquippedId => state.equipped;
    public TackleShopCatalog Catalog => catalog;
    public TackleShopCatalog.Item Equipped => catalog.Find(state.equipped);

    /// <summary>Loads one versioned transaction document; an unreadable save is never overwritten.</summary>
    public TackleShopRuntime(TackleShopCatalog catalog, int initialGold, int initialUpgrades, Func<string> load, Action<string> save)
    {
        catalog.Validate(); this.catalog = catalog; persist = save;
        state = new SaveData { gold = Math.Max(0, initialGold), lineCapacityUpgrades = Math.Max(0, initialUpgrades) };
        try
        {
            string json = load();
            if (!string.IsNullOrEmpty(json))
            {
                var loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded == null || loaded.version != 1 || loaded.gold < 0 || loaded.lineCapacityUpgrades < 0 || loaded.owned == null
                    || new HashSet<string>(loaded.owned).Count != loaded.owned.Count
                    || loaded.owned.Exists(string.IsNullOrWhiteSpace)
                    || !string.IsNullOrEmpty(loaded.equipped) && !loaded.owned.Contains(loaded.equipped))
                    throw new InvalidOperationException("Invalid player progression save.");
                state = loaded;
            }
        }
        catch (Exception exception) { readOnly = true; Error = "Progress could not be loaded. Existing saved data is preserved. " + exception.Message; }
    }

    /// <summary>Reports permanent ownership independently of the selected loadout.</summary>
    public bool Owns(string id) { return state.owned.Contains(id); }

    /// <summary>Validates price, ownership, wallet and expedition state without spending anything.</summary>
    public bool CanPurchase(string id, int gold, bool runActive, out string reason)
    {
        var item = catalog.Find(id);
        reason = readOnly ? Error : runActive ? "Available between expeditions" : item == null ? "Unknown equipment"
            : Owns(id) ? "Already owned" : gold < item.Price ? "Requires " + item.Price + " Gold" : "";
        return reason.Length == 0;
    }

    /// <summary>Commits gold and ownership in the same saved document before publishing either change.</summary>
    public bool TryPurchase(string id, int currentGold, int upgrades, bool runActive, out string reason)
    {
        if (!CanPurchase(id, currentGold, runActive, out reason)) return false;
        var next = Copy(); next.gold = currentGold - catalog.Find(id).Price; next.lineCapacityUpgrades = upgrades; next.owned.Add(id);
        if (!Commit(next)) { reason = Error; return false; }
        reason = catalog.Find(id).DisplayName + " purchased. Equip it for your next expedition."; return true;
    }

    /// <summary>Changes the one-item loadout only between expeditions; an empty identity unequips it.</summary>
    public bool TryEquip(string id, int currentGold, int upgrades, bool runActive, out string reason)
    {
        if (readOnly || runActive || !string.IsNullOrEmpty(id) && (catalog.Find(id) == null || !Owns(id)))
        { reason = readOnly ? Error : runActive ? "Available between expeditions" : "Purchase this item first"; return false; }
        var next = Copy(); next.equipped = id ?? ""; next.gold = currentGold; next.lineCapacityUpgrades = upgrades;
        if (!Commit(next)) { reason = Error; return false; }
        reason = Equipped == null ? "No equipment selected" : Equipped.DisplayName + " equipped"; return true;
    }

    /// <summary>Records ordinary haul earnings and existing capacity purchases in the same progression document.</summary>
    public bool SaveWallet(int gold, int upgrades)
    {
        if (readOnly) return false;
        var next = Copy(); next.gold = Math.Max(0, gold); next.lineCapacityUpgrades = Math.Max(0, upgrades); return Commit(next);
    }

    /// <summary>Copies transaction state so a failed write cannot partially spend or unlock an item.</summary>
    private SaveData Copy() { return JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(state)); }

    /// <summary>Publishes a transaction only after its complete serialized state is persisted.</summary>
    private bool Commit(SaveData next)
    {
        try { persist(JsonUtility.ToJson(next)); state = next; Error = null; return true; }
        catch (Exception exception) { Error = "Progress could not be saved. " + exception.Message; return false; }
    }
}
