/// <summary>Read-only positional amount before strike conditions; projections grant no effects.</summary>
public readonly struct RigEffectProjection
{
    public readonly int SourceId, EffectIndex, Positions;
    public readonly RigAbilityEffect Effect;
    public readonly decimal Amount;
    /// <summary>Freezes one authored component's positional contribution without predicting a strike.</summary>
    public RigEffectProjection(int source, int index, int positions, RigAbilityEffect effect, decimal amount)
    { SourceId=source; EffectIndex=index; Positions=positions; Effect=effect; Amount=amount; }
}
