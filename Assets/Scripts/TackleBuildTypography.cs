using TMPro;
using UnityEngine;

/// <summary>Aligns live glyph ink with the approved concept's ink-top coordinates, independent of font ascenders.</summary>
public static class TackleBuildTypography
{
    /// <summary>Updates only changed content and aligns its ink inside the fixed parent layout rectangle.</summary>
    public static void SetText(TMP_Text text, string value)
    {
        text.text = value ?? "";
        text.rectTransform.anchoredPosition = Vector2.zero;
        if (string.IsNullOrEmpty(value)) return;
        text.ForceMeshUpdate(true, true);
        float top=float.NegativeInfinity,left=float.PositiveInfinity,right=float.NegativeInfinity;
        for(int index=0;index<text.textInfo.characterCount;index++)
        {
            var character=text.textInfo.characterInfo[index];
            if(!character.isVisible||character.textElement?.glyph==null)continue;
            var metrics=character.textElement.glyph.metrics;
            top=Mathf.Max(top,character.baseLine+metrics.horizontalBearingY*character.scale);
            left=Mathf.Min(left,character.origin+metrics.horizontalBearingX*character.scale);
            right=Mathf.Max(right,character.origin+(metrics.horizontalBearingX+metrics.width)*character.scale);
        }
        if(float.IsNegativeInfinity(top))return;
        bool alignLeft=text.alignment==TextAlignmentOptions.TopLeft;
        float x=alignLeft?-left:(text.rectTransform.rect.width-(right-left))*.5f-left;
        text.rectTransform.anchoredPosition = new Vector2(x,-top);
    }
}
