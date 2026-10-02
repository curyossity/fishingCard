using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Forwards pointer hover state from one card-tag icon to its owning card view.</summary>
public sealed class CardTagTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CreatureCardView owner;
    private string tagName;

    /// <summary>Assigns the card view and player-facing tag name represented by this icon.</summary>
    public void Configure(CreatureCardView newOwner, string newTagName)
    {
        owner = newOwner;
        tagName = newTagName;
    }

    /// <summary>Shows the configured tag name beside the hovered icon.</summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null && !string.IsNullOrWhiteSpace(tagName))
        {
            owner.ShowTagTooltip(tagName, transform as RectTransform, this);
        }
    }

    /// <summary>Hides the tooltip when the pointer leaves this tag icon.</summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        owner?.HideTagTooltip(this);
    }

    /// <summary>Prevents a tooltip from remaining visible if its icon is hidden while hovered.</summary>
    private void OnDisable()
    {
        owner?.HideTagTooltip(this);
    }
}
