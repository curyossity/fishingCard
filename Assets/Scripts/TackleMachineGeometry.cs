using UnityEngine;

/// <summary>Defines the fixed machine anchor and size shared by phase variants on the 1920 by 1080 stage.</summary>
public static class TackleMachineGeometry
{
    /// <summary>Preserves the outer shell's position and proportional scale when transitioning between Landing and Build.</summary>
    public static Rect StageBounds => new Rect(490,163,1310,681);
}
