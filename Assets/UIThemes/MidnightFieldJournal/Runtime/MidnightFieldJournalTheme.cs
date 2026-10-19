using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Holds isolated visual resources without modifying shared card definitions or run state.</summary>
[CreateAssetMenu(menuName = "Fishing Cards/UI/Midnight Field Journal Theme")]
public sealed class MidnightFieldJournalTheme : ScriptableObject
{
    [Serializable]
    public sealed class CentralEventEntry
    {
        public string cardUniqueId;
        public Sprite sprite;
        public Rect portraitBounds;
        public float titleSize = 66;
        public int[] descriptionLineBreaks = Array.Empty<int>();
    }
    [Serializable]
    public sealed class CentralCardEntry
    {
        public string cardUniqueId;
        public Sprite sprite;
        public Rect portraitBounds;
        public Rect catchNameBounds, catchTextBounds, baitNameBounds, baitTextBounds;
        public float catchNameSize, catchTextSize, baitNameSize, baitTextSize;
    }
    [Serializable]
    public sealed class SpriteEntry
    {
        [SerializeField] private string sourceId;
        [SerializeField] private Sprite sprite;
        [SerializeField] private string cardUniqueId;
        [SerializeField] private Rect visibleBoundsTopLeft;
        [SerializeField] private Vector2 designCanvas;
        [SerializeField] private string sourceHash;
        [SerializeField] private Rect compactArtworkBounds;
        [SerializeField] private int techniqueRulesBreakAfterWords;
        public string SourceId => sourceId;
        public Sprite Sprite => sprite;
        public string CardUniqueId => cardUniqueId;
        public Rect VisibleBoundsTopLeft => visibleBoundsTopLeft;
        public Vector2 DesignCanvas => designCanvas;
        public string SourceHash => sourceHash;
        public Rect CompactArtworkBounds => compactArtworkBounds;
        public int TechniqueRulesBreakAfterWords => techniqueRulesBreakAfterWords;
    }

    [SerializeField] private Vector2 referenceResolution = new Vector2(3840, 2160);
    [SerializeField] private TMP_FontAsset displayFont;
    [SerializeField] private TMP_FontAsset bodyFont;
    [SerializeField] private TMP_FontAsset bodySemibold;
    [SerializeField] private TMP_FontAsset bodyBold;
    [SerializeField] private Color parchmentInk = new Color32(35, 42, 41, 255);
    [SerializeField] private Color lightInk = new Color32(240, 224, 189, 255);
    [SerializeField] private Color copper = new Color32(184, 135, 85, 255);
    [SerializeField] private Color baitAccent = new Color32(142, 214, 219, 255);
    [SerializeField] private Color mutedInk = new Color32(184, 185, 170, 255);
    [SerializeField] private SpriteEntry[] sprites = Array.Empty<SpriteEntry>();
    [SerializeField] private CentralCardEntry[] centralCards = Array.Empty<CentralCardEntry>();
    [SerializeField] private CentralEventEntry[] centralEvents = Array.Empty<CentralEventEntry>();
    [SerializeField] private string[] unillustratedCardIds = Array.Empty<string>();

    public Vector2 ReferenceResolution => referenceResolution;
    public TMP_FontAsset DisplayFont => displayFont;
    public TMP_FontAsset BodyFont => bodyFont;
    public TMP_FontAsset BodySemibold => bodySemibold;
    public TMP_FontAsset BodyBold => bodyBold;
    public Color ParchmentInk => parchmentInk;
    public Color LightInk => lightInk;
    public Color Copper => copper;
    public Color BaitAccent => baitAccent;
    public Color MutedInk => mutedInk;
    public IReadOnlyList<SpriteEntry> Sprites => sprites;
    public IReadOnlyList<CentralCardEntry> CentralCards => centralCards;
    public IReadOnlyList<CentralEventEntry> CentralEvents => centralEvents;

    /// <summary>Finds an exact central event illustration without supplying it to compact catch cards.</summary>
    public CentralEventEntry GetCentralEvent(string cardUniqueId)
    {
        foreach (var entry in centralEvents)
            if (entry.cardUniqueId == cardUniqueId) return entry;
        return null;
    }
    public IReadOnlyList<string> UnillustratedCardIds => unillustratedCardIds;

    /// <summary>Applies only authored presentation line breaks, retaining every word of the live definition's rules.</summary>
    public string FormatTechniqueRules(CardDefinition card)
    {
        if (card == null) return string.Empty;
        foreach (SpriteEntry entry in sprites)
        {
            if (string.IsNullOrEmpty(entry.CardUniqueId) || entry.CardUniqueId != card.UniqueId || entry.TechniqueRulesBreakAfterWords <= 0) continue;
            string[] words = card.RulesText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int at = entry.TechniqueRulesBreakAfterWords;
            if (at >= words.Length) return card.RulesText;
            return string.Join(" ", words, 0, at) + "\n" + string.Join(" ", words, at, words.Length - at);
        }
        return card.RulesText;
    }

    /// <summary>Returns authored portrait framing in bottom-left normalized compact-card coordinates.</summary>
    public Rect GetCompactArtworkBounds(string cardUniqueId)
    {
        foreach (SpriteEntry entry in sprites)
            if (!string.IsNullOrEmpty(entry.CardUniqueId) && entry.CardUniqueId == cardUniqueId
                && entry.CompactArtworkBounds.width > 0 && entry.CompactArtworkBounds.height > 0)
                return entry.CompactArtworkBounds;
        return CompactBounds(64, 100, 552, 221);
    }

    /// <summary>Converts reference portrait geometry to anchors without cropping its transparent source.</summary>
    public static Rect CompactBounds(float x, float y, float width, float height)
    {
        const float h = 680f / 1.78f;
        return new Rect(x / 680, 1 - (y + height) / h, width / 680, height / h);
    }

    /// <summary>Looks up one explicitly selected theme resource; absent art never resolves to another card.</summary>
    public bool TryGetSprite(string sourceId, out Sprite sprite)
    {
        foreach (SpriteEntry entry in sprites)
        {
            if (!string.Equals(entry.SourceId, sourceId, StringComparison.Ordinal)) continue;
            sprite = entry.Sprite;
            return sprite != null;
        }
        sprite = null;
        return false;
    }

    /// <summary>Returns the theme's optional illustration for an existing stable gameplay identity.</summary>
    public bool TryGetArtwork(string cardUniqueId, out Sprite sprite)
    {
        foreach (SpriteEntry entry in sprites)
        {
            if (string.IsNullOrEmpty(entry.CardUniqueId) ||
                !string.Equals(entry.CardUniqueId, cardUniqueId, StringComparison.Ordinal)) continue;
            sprite = entry.Sprite;
            return sprite != null;
        }
        sprite = null;
        return false;
    }

    /// <summary>Resolves the central-only portrait and Penpot geometry without replacing compact-chain sources.</summary>
    public CentralCardEntry GetCentralCard(string cardUniqueId)
    {
        foreach (CentralCardEntry entry in centralCards)
            if (entry.cardUniqueId == cardUniqueId) return entry;
        return null;
    }
}
