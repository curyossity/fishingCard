using UnityEngine;

/// <summary>Authors template geometry and ink colors independently of creature gameplay data.</summary>
[CreateAssetMenu(menuName = "Fishing Cards/UI/Creature Card Visual Profile")]
public sealed class CreatureCardVisualProfile : ScriptableObject
{
    public Rect catchRules = new Rect(0.085f, 0.09f, 0.39f, 0.22f);
    public Rect baitRules = new Rect(0.525f, 0.09f, 0.39f, 0.22f);
    public Rect eventRules = new Rect(0.09f, 0.07f, 0.82f, 0.30f);
    public Color nameInk = new Color32(28, 34, 34, 255);
    public Color catchRulesInk = new Color32(240, 228, 202, 255);
    public Color eventRulesInk = new Color32(28, 34, 34, 255);
    public float minimumRulesSize = 22f;
    public float maximumRulesSize = 36f;
    public bool separateAbilityHeadings;
    public bool plainStatNumbers;
    public bool showEventCategory;
    public bool respectParentInputLocks;
    public float catchAspectRatio = 1040f / 1300f;
    public float eventAspectRatio = 1040f / 1300f;
    public float minimumStatSize = 20;
    public float maximumStatSize = 56;
    public Vector2 rulesPadding = new Vector2(12, 8);
    public Rect catchDescription = new Rect(.025f, .05f, .95f, .52f);
    public Rect baitDescription = new Rect(.025f, .05f, .95f, .52f);
    public Color selectionWash = new Color32(142, 214, 219, 28);
    public Color selectionBorder = new Color32(184, 135, 85, 235);
}
