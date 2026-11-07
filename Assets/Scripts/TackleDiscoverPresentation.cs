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
    [SerializeField] private TackleReelAnimator[] reels = Array.Empty<TackleReelAnimator>();
    [SerializeField, Min(.2f)] private float spinDuration = 1.05f;
    [SerializeField, Min(0)] private float stopStagger = .15f;
    private long shownSpin = -1;
    private bool wasSpinning;
    private Action onSettled;
    public bool IsSpinning => Array.Exists(reels, reel => reel != null && reel.IsSpinning);
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
    public void Present(FishingRunController controller, SlotMatch match, Action settled = null)
    {
        onSettled = settled;
        var machine = controller.SlotMachine;
        if (reels.Length == 3 && shownSpin != machine.SpinRevision)
        {
            shownSpin = machine.SpinRevision;
            int moving = 0;
            for (int i = 0; i < reels.Length; i++)
                if (!machine.IsLocked(i)) reels[i].Spin(controller.ExpeditionSlotTags, machine.GetTag(i), FindSymbol, spinDuration + stopStagger * moving++);
            wasSpinning = IsSpinning;
        }
        string biome = controller.CurrentBiome?.GetDepthTier(controller.CurrentDepth)?.DisplayName
            ?? controller.CurrentBiome?.DisplayName ?? "Uncharted waters";
        biomeLabel.text = biome.ToUpperInvariant() + " / DISCOVER";
        for (int i = 0; i < 3; i++)
        {
            string tag = controller.SlotMachine.GetTag(i);
            Sprite symbol = FindSymbol(tag);
            reelSymbols[i].sprite = symbol;
            reelSymbols[i].enabled = symbol != null;
            fallbackSymbols[i].text = tag.ToUpperInvariant();
            fallbackSymbols[i].gameObject.SetActive(reels.Length != 3 && symbol == null);
            lockLevers[i].sprite = controller.SlotMachine.IsLocked(i) ? lockedLever : openLever;
        }
        matchLamps.sprite = IsSpinning || match.Kind == SlotMatchKind.ThreeDifferent ? matchOff : matchOn;
        RefreshRig(controller.CatchChain);
    }

    /// <summary>Advances only presentation; completion refreshes input without selecting or accepting a creature.</summary>
    private void Update()
    {
        if (!wasSpinning) return;
        foreach (var reel in reels) reel.Advance(Time.unscaledDeltaTime);
        if (IsSpinning) return;
        wasSpinning = false;
        onSettled?.Invoke();
        if (initialFocus != null && initialFocus.IsInteractable()) initialFocus.Select();
    }

    /// <summary>Looks up approved illustrations, retaining native text for unprovided tag artwork.</summary>
    private Sprite FindSymbol(string tag)
    {
        foreach (TagArt art in tagArtwork)
            if (string.Equals(art.tag, tag, StringComparison.OrdinalIgnoreCase)) return art.sprite;
        return null;
    }

    /// <summary>Invalidates visual work at phase exit so later encounters never inherit an animation callback.</summary>
    private void OnDisable() { shownSpin = -1; wasSpinning = false; onSettled = null; }

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
