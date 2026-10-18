using System;
using UnityEngine;

/// <summary>Optional authored choice UI; option eligibility and resolution remain in the controller.</summary>
public abstract class CreatureChoicePresentation : MonoBehaviour
{
    /// <summary>Displays the exact runtime-provided labels/details and forwards the current selection.</summary>
    public abstract void Show(string title, string prompt, string[] labels, string[] details, Sprite[] portraits, Action<int> select);
    /// <summary>Dismisses the choice without selecting a runtime option.</summary>
    public abstract void Hide();
}
