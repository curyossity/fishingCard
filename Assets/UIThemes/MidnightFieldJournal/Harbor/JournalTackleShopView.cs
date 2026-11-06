using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] private Image[] slotArt;
    [SerializeField] private TMP_Text[] slotNames, slotPluses;
    [SerializeField] private GameObject[] catalogCards;
    [SerializeField] private Graphic[] selectionMarks, filterMarks;
    [SerializeField] private ScrollRect catalogScroll;
    [SerializeField] private TMP_Text catalogCount, equippedCount;
    private int selected;
    private int filter;
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
        else if (shop.IsEquipped(item.Id)) controller.TryUnequipEquipment(item.Slot, out notification);
        else controller.TryEquipEquipment(item.Id, out notification);
        flow.RefreshEconomy();
    }

    /// <summary>Filters catalog entries while the loadout and inspection stay fixed.</summary>
    public void Filter(int category)
    {
        filter = Mathf.Clamp(category, 0, 3); Refresh();
        if (catalogScroll != null) { Canvas.ForceUpdateCanvases(); catalogScroll.verticalNormalizedPosition = 1; }
    }

    /// <summary>Inspects the equipped category, or browses its available items when empty.</summary>
    public void InspectSlot(int category)
    {
        var shop = flow.Controller.TackleShop;
        var item = shop.GetEquipped((EquipmentSlot)category);
        if (item != null) Select(Array.FindIndex(shop.Catalog.Items, entry => entry.Id == item.Id));
        else Filter(category + 1);
    }

    /// <summary>Moves the catalog by one visible row without moving fixed controls.</summary>
    public void Scroll(int direction)
    {
        float overflow = catalogScroll.content.rect.height - catalogScroll.viewport.rect.height;
        if (overflow > 0) catalogScroll.verticalNormalizedPosition = Mathf.Clamp01(catalogScroll.verticalNormalizedPosition + direction * 665f / overflow);
    }

    /// <summary>Refreshes prices, effects, actions and all shared loadout captions from authoritative progression.</summary>
    public void Refresh()
    {
        if (flow == null || flow.Controller == null) return;
        var controller = flow.Controller; var shop = controller.TackleShop;
        if (shop == null) return;
        var items = shop.Catalog.Items;
        selected = Mathf.Clamp(selected, 0, items.Length - 1);
        for (int i = 0; i < cardStates.Length; i++)
        {
            var item = items[i]; bool owned = shop.Owns(item.Id);
            cardStates[i].text = shop.IsEquipped(item.Id) ? "EQUIPPED" : owned ? "OWNED" : item.Price + " GOLD";
            cardStates[i].color = owned ? new Color(.42f, .66f, .53f) : new Color(.81f, .66f, .40f);
            if (i < cardEffects.Length) cardEffects[i].text = item.Description;
            itemButtons[i].interactable = !controller.RunActive;
            if (catalogCards != null && i < catalogCards.Length) catalogCards[i].SetActive(filter == 0 || (int)item.Slot == filter - 1);
            if (selectionMarks != null && i < selectionMarks.Length) selectionMarks[i].enabled = i == selected;
        }
        var choice = items[selected]; bool purchased = shop.Owns(choice.Id), equipped = shop.IsEquipped(choice.Id);
        detailCategory.text = choice.Slot.ToString().ToUpperInvariant();
        detailName.text = choice.DisplayName.ToUpperInvariant(); detailEffect.text = choice.Description;
        detailState.text = equipped ? "EQUIPPED" : purchased ? "OWNED · READY TO EQUIP" : "PERMANENT UNLOCK";
        detailArt.sprite = portraits[selected]; detailArt.preserveAspect = true;
        actionLabel.text = !purchased ? "BUY · " + choice.Price + " GOLD" : equipped ? "UNEQUIP" : "EQUIP";
        action.interactable = !controller.RunActive && (purchased ? shop.CanEdit : shop.CanPurchase(choice.Id, controller.TotalGold, false, out _));
        message.text = !string.IsNullOrEmpty(notification) ? notification : !string.IsNullOrEmpty(shop.Error) ? shop.Error
            : !purchased && controller.TotalGold < choice.Price ? "Earn " + (choice.Price - controller.TotalGold) + " more Gold to purchase this item." : "Equip one Rod, one Reel and one Hook.";
        foreach (var label in loadoutLabels) label.text = shop.EquippedItems.Length == 0 ? "No equipment selected" : string.Join(" · ", shop.EquippedItems.Select(item => item.DisplayName));
        foreach (var label in loadoutHeadings) label.text = "CURRENT LOADOUT · " + shop.EquippedItems.Length + " / 3";
        if (equippedCount != null) equippedCount.text = shop.EquippedItems.Length + " / 3 EQUIPPED";
        for (int i = 0; slotArt != null && i < slotArt.Length; i++)
        {
            var item = shop.GetEquipped((EquipmentSlot)i); slotArt[i].enabled = item != null;
            slotPluses[i].gameObject.SetActive(item == null); slotNames[i].text = item == null ? "" : item.DisplayName;
            if (item != null) slotArt[i].sprite = portraits[Array.FindIndex(items, entry => entry.Id == item.Id)];
        }
        if (filterMarks != null) for (int i = 0; i < filterMarks.Length; i++) filterMarks[i].enabled = i == filter;
        if (catalogCount != null) catalogCount.text = catalogCards.Count(card => card.activeSelf) + " ITEMS";
        if (collectionHand != null) collectionHand.text = "Starting hand: " + controller.NextExpeditionHandSize + " Techniques";
    }
}
