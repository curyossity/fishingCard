using System;
using UnityEngine;

/// <summary>Optional presentation boundary for the shared controller-facing Catch Chain view.</summary>
public abstract class CatchChainPresentation : MonoBehaviour
{
    /// <summary>Displays resolved attachments and forwards selection without deciding gameplay legality.</summary>
    public abstract void Refresh(CardInstance[] catches, ActiveCatchEffectRecord[] activeEffects,
        int currentLineLoad, int lineCapacity, int selectedCatchIndex, Action<int> selectCatchAction);

    /// <summary>Updates the independently localizable chain heading.</summary>
    public abstract void SetHeading(string heading);
}
