using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Renders the approved Tackle Apparatus shell and live reel/rig state without selecting encounters.</summary>
public sealed class TackleDiscoverPresentation : MonoBehaviour
{
    [Serializable]
    public struct TagArt
    {
        public string tag;
        public Sprite sprite;
    }
    [SerializeField] private TMP_Text biomeLabel, haulLabel, emptyRigLabel;
    [SerializeField] private UnityEngine.UI.Image[] reelSymbols = Array.Empty<UnityEngine.UI.Image>();
    [SerializeField] private TMP_Text[] fallbackSymbols = Array.Empty<TMP_Text>();
    [SerializeField] private UnityEngine.UI.Image[] lockLevers = Array.Empty<UnityEngine.UI.Image>();
    [SerializeField] private Sprite openLever, lockedLever, matchOn, matchOff;
    [SerializeField] private UnityEngine.UI.Image matchLamps;
    [SerializeField] private TagArt[] tagArtwork = Array.Empty<TagArt>();
    [SerializeField] private RectTransform rigContent;
    [SerializeField] private TackleRigCardView rigCardPrefab;
    [SerializeField] private UnityEngine.UI.ScrollRect rigScroll;
    [SerializeField] private UnityEngine.UI.Button initialFocus;
    private readonly List<TackleRigCardView> cards = new List<TackleRigCardView>();
    private readonly List<CardInstance> previousOrder = new List<CardInstance>();

    /// <summary>Restores controller focus after the Harbor's closing input frame.</summary>
    private void OnEnable() { StartCoroutine(FocusAfterHarbor()); }

    /// <summary>Defers focus until the Harbor has released its own selected object.</summary>
    private System.Collections.IEnumerator FocusAfterHarbor()
    {
        yield return null;
        if (UnityEngine.EventSystems.EventSystem.current != null && initialFocus != null && initialFocus.IsInteractable()) initialFocus.Select();
    }

    /// <summary>Displays resolved model values, one authoritative lock and optional existing symbol illustrations.</summary>
    public void Present(FishingRunController controller, SlotMatch match)
    {
        string biome = controller.CurrentBiome?.GetDepthTier(controller.CurrentDepth)?.DisplayName
            ?? controller.CurrentBiome?.DisplayName ?? "Uncharted waters";
        biomeLabel.text = biome.ToUpperInvariant() + " / DISCOVER";
        for (int i = 0; i < 3; i++)
        {
            string tag = controller.SlotMachine.GetTag(i);
            Sprite symbol = null;
            foreach (TagArt art in tagArtwork)
                if (string.Equals(art.tag, tag, StringComparison.OrdinalIgnoreCase)) { symbol = art.sprite; break; }
            reelSymbols[i].sprite = symbol;
            reelSymbols[i].enabled = symbol != null;
            fallbackSymbols[i].text = tag.ToUpperInvariant();
            fallbackSymbols[i].gameObject.SetActive(symbol == null);
            lockLevers[i].sprite = controller.SlotMachine.IsLocked(i) ? lockedLever : openLever;
        }
        matchLamps.sprite = match.Kind == SlotMatchKind.ThreeDifferent ? matchOff : matchOn;
        RefreshRig(controller.CatchChain);
    }

    /// <summary>Reuses physical rig-card views by instance identity and keeps longer rigs scrollable without a card cap.</summary>
    private void RefreshRig(CardInstance[] rig)
    {
        rig = rig ?? Array.Empty<CardInstance>();
        bool changed = previousOrder.Count != rig.Length;
        for (int i = 0; i < rig.Length && !changed; i++) changed |= !ReferenceEquals(rig[i], previousOrder[i]);
        var ordered = new List<TackleRigCardView>();
        int haul = 0;
        bool concealed = false;
        for (int i = 0; i < rig.Length; i++)
        {
            CardInstance instance = rig[i];
            var card = cards.Find(view => ReferenceEquals(view.BoundInstance, instance));
            if (card == null) card = cards.Find(view => !ordered.Contains(view) && Array.IndexOf(rig, view.BoundInstance) < 0);
            if (card == null) card = Instantiate(rigCardPrefab, rigContent);
            ordered.Add(card);
            card.gameObject.SetActive(true);
            card.Bind(instance);
            var rect = (RectTransform)card.transform;
            rect.anchoredPosition = new Vector2(0, -20 - i * 275);
            card.transform.SetSiblingIndex(i);
            haul += instance.HaulValueContribution;
            concealed |= !instance.IsBait && instance.HidesOwnValueDuringRun;
        }
        foreach (var card in cards) if (!ordered.Contains(card)) { card.gameObject.SetActive(false); ordered.Add(card); }
        cards.Clear(); cards.AddRange(ordered);
        rigContent.sizeDelta = new Vector2(315, Mathf.Max(825, 20 + rig.Length * 275));
        if (changed) rigScroll.verticalNormalizedPosition = 1;
        previousOrder.Clear(); previousOrder.AddRange(rig);
        emptyRigLabel.gameObject.SetActive(rig.Length == 0);
        haulLabel.text = "HAUL " + (concealed ? "?" : haul.ToString());
    }
}
