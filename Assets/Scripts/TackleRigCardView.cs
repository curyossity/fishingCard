using TMPro;
using UnityEngine;

/// <summary>Reusable blank rig face, portrait and live role/value labels in the approved pixel presentation.</summary>
public sealed class TackleRigCardView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image face, portrait;
    [SerializeField] private Sprite catchFace, baitFace;
    [SerializeField] private TMP_Text nameLabel, roleLabel, valueLabel;
    [SerializeField] private bool alignInkTop;
    [SerializeField] private GameObject acquiredHighlight;
    [SerializeField] private GameObject railClip;
    [SerializeField] private TackleCardTooltipSource tooltipSource;
    public CardInstance BoundInstance { get; private set; }
    public UnityEngine.UI.Selectable InspectionTarget => tooltipSource;
    public RectTransform ClipTransform => railClip != null ? railClip.transform as RectTransform : null;

    /// <summary>Briefly accents a scoring source without changing its role, value or supplied artwork.</summary>
    public void SetContributing(bool active) { face.color = active ? new Color32(255, 222, 151, 255) : Color.white; }

    /// <summary>Accents the existing upgraded position without enabling rearrangement or another acquisition.</summary>
    public void SetAcquiredHighlight(bool active) { if (acquiredHighlight != null) acquiredHighlight.SetActive(active); }

    /// <summary>Uses the original three-card tray's clips or independent clips when the scrolling rig grows.</summary>
    public void SetClipVisible(bool active) { if (railClip != null) railClip.SetActive(active); }

    /// <summary>Displays role-aware resolved value without exposing concealed numbers or running attachment effects.</summary>
    public void Bind(CardInstance instance, bool afterPlacement = false)
    {
        BoundInstance = instance;
        nameLabel.text = instance.Definition.DisplayName.ToUpperInvariant();
        roleLabel.text = instance.IsRigCreature ? "LV " + instance.Level : instance.IsBait ? "BAIT" : "CATCH";
        valueLabel.text = !instance.IsBait && instance.HidesOwnValueDuringRun ? "?" : instance.HaulValueContribution.ToString();
        if (alignInkTop)
        {
            TackleBuildTypography.SetText(nameLabel, nameLabel.text);
            TackleBuildTypography.SetText(roleLabel, roleLabel.text);
            TackleBuildTypography.SetText(valueLabel, valueLabel.text);
        }
        face.sprite = instance.IsBait ? baitFace : catchFace;
        Sprite image = instance.Definition.Artwork;
        portrait.sprite = image;
        portrait.enabled = image != null;
        if (tooltipSource != null) tooltipSource.Rebind(instance, afterPlacement);
    }
}
