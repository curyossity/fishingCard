using System;
using TMPro;
using UnityEngine;

/// <summary>Reusable blank rig face, portrait and live role/value labels in the approved pixel presentation.</summary>
public sealed class TackleRigCardView : MonoBehaviour
{
    [Serializable]
    public struct Portrait
    {
        public string creatureId;
        public Sprite sprite;
    }
    [SerializeField] private UnityEngine.UI.Image face, portrait;
    [SerializeField] private Sprite catchFace, baitFace;
    [SerializeField] private TMP_Text nameLabel, roleLabel, valueLabel;
    [SerializeField] private Portrait[] portraits = Array.Empty<Portrait>();
    public CardInstance BoundInstance { get; private set; }

    /// <summary>Displays role-aware resolved value without exposing concealed numbers or running attachment effects.</summary>
    public void Bind(CardInstance instance)
    {
        BoundInstance = instance;
        nameLabel.text = instance.Definition.DisplayName.ToUpperInvariant();
        roleLabel.text = instance.IsBait ? "BAIT" : "CATCH";
        valueLabel.text = !instance.IsBait && instance.HidesOwnValueDuringRun ? "?" : instance.HaulValueContribution.ToString();
        face.sprite = instance.IsBait ? baitFace : catchFace;
        Sprite image = null;
        foreach (Portrait entry in portraits)
            if (string.Equals(entry.creatureId, instance.Definition.UniqueId, StringComparison.OrdinalIgnoreCase)) { image = entry.sprite; break; }
        portrait.sprite = image;
        portrait.enabled = image != null;
    }
}
