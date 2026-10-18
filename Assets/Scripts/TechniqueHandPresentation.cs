using System;
using UnityEngine;

/// <summary>Optional hand rendering boundary that preserves the controller's ordering, legality and use command.</summary>
public abstract class TechniqueHandPresentation : MonoBehaviour
{
    /// <summary>Displays real hand slots without drawing, discarding or resolving Techniques.</summary>
    public abstract void Refresh(CardDefinition[] hand, bool[] playable, string[] restrictions,
        int drawCount, int discardCount, Func<int, bool> useCard, long handVersion = 0);
}
