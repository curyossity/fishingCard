using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Temporarily stages scoring contributors in the encounter portrait, restoring its exact authored state afterward.</summary>
public sealed class LandingScoringPortrait : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image portrait;
    [SerializeField] private TMP_Text creatureName, tags;
    private readonly List<UnityEngine.UI.Image> groupedPortraits = new List<UnityEngine.UI.Image>();
    private Sprite encounterSprite;
    private Color encounterColor;
    private Vector3 encounterScale;
    private string encounterName, encounterTags;
    private bool captured, encounterEnabled, contributing;
    private float encounterNameAlpha;
    public bool IsShowingContributors => contributing;
    public Vector3 ProjectileOrigin => portrait.rectTransform.TransformPoint(portrait.rectTransform.rect.center);

    /// <summary>Captures the view after the encounter owner has bound the target, including failure or concealed presentation.</summary>
    public void CaptureEncounter()
    {
        if (captured) EndEncounter();
        encounterSprite = portrait.sprite; encounterEnabled = portrait.enabled; encounterColor = portrait.color;
        encounterScale = portrait.rectTransform.localScale; encounterName = creatureName.text; encounterTags = tags.text;
        encounterNameAlpha = creatureName.alpha; captured = true;
    }
    /// <summary>Shows all identities in a grouped beat without implying that one creature owns the combined operation.</summary>
    public void ShowContributors(IReadOnlyList<CardInstance> members)
    {
        if (!captured || members.Count == 0) { RestoreEncounter(); return; }
        HideGroup(); contributing = true; portrait.color = Color.white;
        if (members.Count == 1)
        {
            var member = members[0]; portrait.sprite = member.Definition.EncounterArtwork ?? member.Definition.Artwork;
            portrait.enabled = portrait.sprite != null; creatureName.text = member.Definition.DisplayName.ToUpperInvariant();
            tags.text = "RIG EFFECT · " + string.Join(" / ", member.Tags).ToUpperInvariant();
        }
        else
        {
            portrait.enabled = false;
            creatureName.text = members.Count + " RIG CREATURES"; tags.text = "COMBINED RIG EFFECT";
            var bounds = portrait.rectTransform.rect;
            int columns = Mathf.Min(members.Count, Mathf.CeilToInt(Mathf.Sqrt(members.Count * bounds.width / bounds.height)));
            int rows = Mathf.CeilToInt(members.Count / (float)columns);
            for (int i = 0; i < members.Count; i++)
            {
                if (i == groupedPortraits.Count)
                {
                    var image = new GameObject("Grouped scoring contributor", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
                    image.transform.SetParent(portrait.transform, false); image.preserveAspect = true; image.raycastTarget = false;
                    groupedPortraits.Add(image);
                }
                var imageView = groupedPortraits[i]; var rect = imageView.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(bounds.width / columns - 6, bounds.height / rows - 4);
                rect.anchoredPosition = new Vector2(bounds.xMin + (i % columns + .5f) * bounds.width / columns, bounds.yMax - (i / columns + .5f) * bounds.height / rows);
                imageView.sprite = members[i].Definition.EncounterArtwork ?? members[i].Definition.Artwork;
                imageView.gameObject.SetActive(imageView.sprite != null);
            }
        }
    }
    /// <summary>Restrains source emphasis to the active illustration without moving the portrait frame.</summary>
    public void SetPulse(float amount) { if (captured) portrait.rectTransform.localScale = encounterScale * (1 + (contributing ? amount * .06f : 0)); }
    /// <summary>Fades the external name in step with the card face so a hidden swap never displays conflicting identities.</summary>
    public void SetHandoffOpacity(float alpha) { if (captured) creatureName.alpha = encounterNameAlpha * Mathf.Clamp01(alpha); }
    /// <summary>Returns the original target immediately at payout or when an ordinary strike has no contributors.</summary>
    public void RestoreEncounter()
    {
        HideGroup(); contributing = false;
        if (!captured) return;
        portrait.sprite = encounterSprite; portrait.enabled = encounterEnabled; portrait.color = encounterColor;
        portrait.rectTransform.localScale = encounterScale; creatureName.text = encounterName; tags.text = encounterTags;
        creatureName.alpha = encounterNameAlpha;
    }
    /// <summary>Releases the snapshot after skip, completion or phase exit so a later encounter cannot restore stale art.</summary>
    public void EndEncounter() { RestoreEncounter(); captured = false; }
    /// <summary>Clears every pooled contributor without destroying reusable native graphics.</summary>
    private void HideGroup() { foreach (var image in groupedPortraits) image.gameObject.SetActive(false); }
    /// <summary>Restores the target if this presentation is independently disabled.</summary>
    private void OnDisable() => EndEncounter();
}
