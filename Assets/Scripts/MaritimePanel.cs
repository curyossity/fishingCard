using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MaritimePanelVariant
{
    DarkTeal,
    Ivory,
    Turquoise,
    Coral
}

public enum MaritimeBorderStyle
{
    AgedBrass,
    OxidizedTeal
}

public sealed class MaritimePanel : MonoBehaviour
{
    private static readonly Color DarkTeal = new Color32(13, 51, 52, 255);
    private static readonly Color WarmIvory = new Color32(232, 215, 176, 255);
    private static readonly Color Turquoise = new Color32(43, 143, 138, 255);
    private static readonly Color Coral = new Color32(169, 79, 69, 255);

    [Header("Appearance")]
    [SerializeField] private MaritimePanelVariant variant = MaritimePanelVariant.DarkTeal;
    [SerializeField] private MaritimeBorderStyle borderStyle = MaritimeBorderStyle.AgedBrass;
    [SerializeField] private Color accentColor = new Color32(181, 138, 74, 255);
    [SerializeField] private Vector4 contentPadding = new Vector4(28f, 28f, 28f, 28f);

    [Header("Structure")]
    [SerializeField] private RawImage borderMaterial;
    [SerializeField] private Image interior;
    [SerializeField] private RawImage interiorMaterial;
    [SerializeField] private Image outerFrame;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private GameObject headerRoot;
    [SerializeField] private Image headerFrame;
    [SerializeField] private Image headerIcon;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private Image[] cornerOrnaments = new Image[4];

    [Header("Approved Materials")]
    [SerializeField] private Texture tealPaperTexture;
    [SerializeField] private Texture ivoryPaperTexture;
    [SerializeField] private Texture printNoiseTexture;
    [SerializeField] private Texture agedBrassTexture;
    [SerializeField] private Texture oxidizedTealTexture;

    public MaritimePanelVariant Variant => variant;
    public MaritimeBorderStyle BorderStyle => borderStyle;
    public Color AccentColor => accentColor;
    public RectTransform ContentRoot => contentRoot;

    /// <summary>
    /// Applies serialized appearance settings when the prefab instance starts.
    /// </summary>
    private void Awake()
    {
        ApplyAppearance();
    }

    /// <summary>
    /// Keeps authored prefab variants current in the Inspector.
    /// </summary>
    private void OnValidate()
    {
        ApplyAppearance();
    }

    /// <summary>
    /// Changes the panel interior and nested border without tinting supplied frame artwork.
    /// </summary>
    public void SetAppearance(
        MaritimePanelVariant newVariant,
        MaritimeBorderStyle newBorderStyle,
        Color newAccentColor)
    {
        variant = newVariant;
        borderStyle = newBorderStyle;
        accentColor = newAccentColor;
        ApplyAppearance();
    }

    /// <summary>
    /// Sets optional header content while preserving the panel's content geometry.
    /// </summary>
    public void SetHeader(string title, Sprite icon = null)
    {
        bool hasTitle = !string.IsNullOrWhiteSpace(title);
        bool hasIcon = icon != null;

        if (headerRoot != null)
        {
            headerRoot.SetActive(hasTitle || hasIcon);
        }

        if (headerText != null)
        {
            headerText.text = hasTitle ? title : string.Empty;
        }

        if (headerIcon != null)
        {
            headerIcon.sprite = icon;
            headerIcon.gameObject.SetActive(hasIcon);
        }
    }

    /// <summary>
    /// Assigns one optional corner ornament without affecting the sliced frame.
    /// </summary>
    public void SetCornerOrnament(int cornerIndex, Sprite ornament)
    {
        if (cornerOrnaments == null || cornerIndex < 0 || cornerIndex >= cornerOrnaments.Length)
        {
            return;
        }

        Image corner = cornerOrnaments[cornerIndex];
        if (corner == null)
        {
            return;
        }

        corner.sprite = ornament;
        corner.gameObject.SetActive(ornament != null);
    }

    /// <summary>
    /// Updates material selection, safe content padding, and text contrast.
    /// </summary>
    public void ApplyAppearance()
    {
        if (interior != null)
        {
            interior.color = GetInteriorColor(variant);
        }

        if (interiorMaterial != null)
        {
            bool usesPaper = variant == MaritimePanelVariant.DarkTeal || variant == MaritimePanelVariant.Ivory;
            interiorMaterial.texture = variant == MaritimePanelVariant.Ivory
                ? ivoryPaperTexture
                : usesPaper ? tealPaperTexture : printNoiseTexture;
            interiorMaterial.color = usesPaper
                ? new Color(1f, 1f, 1f, 0.38f)
                : new Color(1f, 1f, 1f, 0.24f);
        }

        if (borderMaterial != null)
        {
            borderMaterial.texture = borderStyle == MaritimeBorderStyle.AgedBrass
                ? agedBrassTexture
                : oxidizedTealTexture;
            borderMaterial.color = Color.white;
        }

        if (outerFrame != null)
        {
            outerFrame.color = Color.white;
            outerFrame.type = Image.Type.Sliced;
        }

        if (headerFrame != null)
        {
            headerFrame.color = Color.white;
            headerFrame.type = Image.Type.Sliced;
        }

        if (headerText != null)
        {
            headerText.color = new Color32(239, 226, 194, 255);
        }

        ApplyContentPadding();
    }

    /// <summary>
    /// Converts the serialized pixel padding into stable RectTransform offsets.
    /// </summary>
    private void ApplyContentPadding()
    {
        if (contentRoot == null)
        {
            return;
        }

        contentRoot.anchorMin = Vector2.zero;
        contentRoot.anchorMax = Vector2.one;
        contentRoot.offsetMin = new Vector2(contentPadding.x, contentPadding.w);
        contentRoot.offsetMax = new Vector2(-contentPadding.y, -contentPadding.z);
    }

    private static Color GetInteriorColor(MaritimePanelVariant panelVariant)
    {
        return panelVariant switch
        {
            MaritimePanelVariant.Ivory => WarmIvory,
            MaritimePanelVariant.Turquoise => Turquoise,
            MaritimePanelVariant.Coral => Coral,
            _ => DarkTeal
        };
    }
}
