using System;
using TMPro;
using UnityEngine;

/// <summary>Displays live shop data and routes purchases/equipment selection to the shared controller.</summary>
public sealed class JournalTackleShopView : MonoBehaviour
{
    [SerializeField] private JournalHarborFlow flow;
    [SerializeField] private TMP_Text[] cardStates, cardEffects, loadoutLabels, loadoutHeadings;
    [SerializeField] private TMP_Text detailCategory, detailName, detailEffect, detailState, actionLabel, message, collectionHand;
    [SerializeField] private UnityEngine.UI.Image detailArt;
    [SerializeField] private UnityEngine.UI.Button action;
    [SerializeField] private Sprite[] portraits;
    [SerializeField] private UnityEngine.UI.Button[] itemButtons;
    private int selected;
    private string notification;

    /// <summary>Loads actual ownership and prices each time the shop becomes visible.</summary>
    private void OnEnable() { Refresh(); }

    /// <summary>Selects an equipment design for inspection without purchasing or equipping it.</summary>
    public void Select(int index)
    {
        var shop = flow.Controller.TackleShop;
        if (shop == null || index < 0 || index >= shop.Catalog.Items.Length) return;
        selected = index; notification = null; Refresh();
    }

    /// <summary>Buys an unowned item or toggles the selected owned item's future loadout state.</summary>
    public void Activate()
    {
        var controller = flow.Controller; var shop = controller.TackleShop;
        if (shop == null) return;
        var item = shop.Catalog.Items[selected];
        if (!shop.Owns(item.Id)) controller.TryPurchaseEquipment(item.Id, out notification);
        else controller.TryEquipEquipment(shop.EquippedId == item.Id ? "" : item.Id, out notification);
        flow.RefreshEconomy();
    }

    /// <summary>Refreshes prices, effects, actions and all shared loadout captions from authoritative progression.</summary>
    public void Refresh()
    {
        if (flow == null || flow.Controller == null) return;
        var controller = flow.Controller; var shop = controller.TackleShop;
        if (shop == null) return;
        var items = shop.Catalog.Items;
        selected = Mathf.Clamp(selected, 0, items.Length - 1);
        for (int i = 0; i < items.Length; i++)
        {
            bool owned = shop.Owns(items[i].Id);
            cardStates[i].text = shop.EquippedId == items[i].Id ? "EQUIPPED" : owned ? "OWNED" : items[i].Price + " GOLD";
            cardStates[i].color = owned ? new Color(.42f, .66f, .53f) : new Color(.81f, .66f, .40f);
            cardEffects[i].text = items[i].Description;
            itemButtons[i].interactable = !controller.RunActive;
        }
        var choice = items[selected]; bool purchased = shop.Owns(choice.Id), equipped = shop.EquippedId == choice.Id;
        detailCategory.text = choice.Category.ToUpperInvariant() + " · " + choice.Price + " GOLD";
        detailName.text = choice.DisplayName.ToUpperInvariant(); detailEffect.text = choice.Description;
        detailState.text = equipped ? "EQUIPPED" : purchased ? "OWNED · READY TO EQUIP" : "PERMANENT UNLOCK";
        detailArt.sprite = portraits[selected]; detailArt.preserveAspect = true;
        actionLabel.text = !purchased ? "BUY · " + choice.Price + " GOLD" : equipped ? "UNEQUIP" : "EQUIP";
        action.interactable = !controller.RunActive && (purchased ? shop.CanEdit : shop.CanPurchase(choice.Id, controller.TotalGold, false, out _));
        message.text = !string.IsNullOrEmpty(notification) ? notification : !string.IsNullOrEmpty(shop.Error) ? shop.Error
            : !purchased && controller.TotalGold < choice.Price ? "Earn " + (choice.Price - controller.TotalGold) + " more Gold to purchase this item." : "Buy once. Equip one item for your next expedition.";
        foreach (var label in loadoutLabels) label.text = shop.Equipped == null ? "No equipment selected" : shop.Equipped.DisplayName + " equipped";
        foreach (var label in loadoutHeadings) label.text = "CURRENT LOADOUT · " + (shop.Equipped == null ? "0" : "1") + " / 1";
        if (collectionHand != null) collectionHand.text = "Starting hand: " + controller.NextExpeditionHandSize + " Techniques";
    }
}
