using TMPro;
using UnityEngine;

/// <summary>Reusable blank rig face, portrait and live role/value labels in the approved pixel presentation.</summary>
public sealed class TackleRigCardView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image face, portrait;
    [SerializeField] private Sprite catchFace, baitFace;
    [SerializeField] private TMP_Text nameLabel, roleLabel, valueLabel;
    public CardInstance BoundInstance { get; private set; }

    /// <summary>Briefly accents a scoring source without changing its role, value or supplied artwork.</summary>
    public void SetContributing(bool active) { face.color = active ? new Color32(255, 222, 151, 255) : Color.white; }

    /// <summary>Displays role-aware resolved value without exposing concealed numbers or running attachment effects.</summary>
    public void Bind(CardInstance instance)
    {
        BoundInstance = instance;
        nameLabel.text = instance.Definition.DisplayName.ToUpperInvariant();
        roleLabel.text = instance.IsBait ? "BAIT" : "CATCH";
        valueLabel.text = !instance.IsBait && instance.HidesOwnValueDuringRun ? "?" : instance.HaulValueContribution.ToString();
        face.sprite = instance.IsBait ? baitFace : catchFace;
        Sprite image = instance.Definition.Artwork;
        portrait.sprite = image;
        portrait.enabled = image != null;
    }
}
